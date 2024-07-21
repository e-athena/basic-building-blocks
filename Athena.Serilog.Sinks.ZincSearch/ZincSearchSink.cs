// Copyright 2020-2022 Mykhailo Shevchuk & Contributors
//
// Licensed under the MIT license;
// you may not use this file except in compliance with the License.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See LICENSE file in the project root for full license information.

using Athena.Serilog.Sinks.ZincSearch.Infrastructure;
using Athena.Serilog.Sinks.ZincSearch.Models;
using Athena.Serilog.Sinks.ZincSearch.Utils;
using Serilog.Core;
using Serilog.Debugging;
using Serilog.Events;
using Serilog.Formatting;

namespace Athena.Serilog.Sinks.ZincSearch;

internal class ZincSearchSink : ILogEventSink, IDisposable
{
    private readonly string _requestUri;
    private readonly int _batchPostingLimit;
    private readonly ITextFormatter _textFormatter;
    private readonly IZincSearchBatchFormatter _batchFormatter;
    private readonly IZincSearchHttpClient _httpClient;
    private readonly ExponentialBackoffConnectionSchedule _connectionSchedule;
    private readonly object _syncRoot = new();
    private readonly PortableTimer _timer;
    private readonly BoundedQueue<ZincSearchLogEvent> _queue;
    private readonly Queue<ZincSearchLogEvent> _waitingBatch = new();

    private bool _isDisposed;

    public ZincSearchSink(
        string requestUri,
        int batchPostingLimit,
        int? queueLimit,
        TimeSpan period,
        ITextFormatter textFormatter,
        IZincSearchBatchFormatter batchFormatter,
        IZincSearchHttpClient httpClient)
    {
        _requestUri = requestUri ?? throw new ArgumentNullException(nameof(requestUri));
        _batchPostingLimit = batchPostingLimit;
        _textFormatter = textFormatter ?? throw new ArgumentNullException(nameof(textFormatter));
        _batchFormatter = batchFormatter ?? throw new ArgumentNullException(nameof(batchFormatter));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        _connectionSchedule = new ExponentialBackoffConnectionSchedule(period);
        _timer = new PortableTimer(OnTick);
        _queue = new BoundedQueue<ZincSearchLogEvent>(queueLimit);

        SetTimer();
    }

    public void Emit(LogEvent logEvent)
    {
        if (logEvent == null)
        {
            throw new ArgumentNullException(nameof(logEvent));
        }

        if (!_queue.TryEnqueue(new ZincSearchLogEvent(logEvent)))
        {
            SelfLog.WriteLine("Queue has reached it's limit and the log event will be dropped");
        }
    }

    public void Dispose()
    {
        lock (_syncRoot)
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;
        }

        _timer.Dispose();
        OnTick().GetAwaiter().GetResult();
        _httpClient.Dispose();
    }

    private async Task OnTick()
    {
        try
        {
            bool batchWasFull;

            do
            {
                while (_waitingBatch.Count < _batchPostingLimit && _queue.TryDequeue(out var next))
                {
                    _waitingBatch.Enqueue(next!);
                }

                batchWasFull = _waitingBatch.Count >= _batchPostingLimit;

                if (_waitingBatch.Count > 0)
                {
                    HttpResponseMessage response;

                    using (var contentStream = new MemoryStream())
                    {
                        using (var contentWriter = new StreamWriter(contentStream, Encoding.UTF8WithoutBom))
                        {
                            _batchFormatter.Format(_waitingBatch, _textFormatter, contentWriter);
                            await contentWriter.FlushAsync();
                            contentStream.Position = 0;

                            if (contentStream.Length == 0)
                            {
                                continue;
                            }

                            response = await _httpClient
                                .PostAsync(_requestUri, contentStream)
                                .ConfigureAwait(false);
                        }
                    }

                    if (response.IsSuccessStatusCode)
                    {
                        _connectionSchedule.MarkSuccess();
                        _waitingBatch.Clear();
                    }
                    else
                    {
                        SelfLog.WriteLine(
                            "Received failure on HTTP shipping ({0}): {1}. {2} log events will be dropped",
                            (int)response.StatusCode,
                            await response.Content.ReadAsStringAsync().ConfigureAwait(false),
                            _waitingBatch.Count);

                        _connectionSchedule.MarkFailure();
                        _waitingBatch.Clear();

                        break;
                    }
                }
                else
                {
                    _connectionSchedule.MarkSuccess();
                }
            }
            while (batchWasFull);
        }
        catch (Exception ex)
        {
            SelfLog.WriteLine("Exception while emitting periodic batch from {0}: {1}", this, ex);
            _connectionSchedule.MarkFailure();
        }
        finally
        {
            lock (_syncRoot)
            {
                if (!_isDisposed)
                {
                    SetTimer();
                }
            }
        }
    }

    private void SetTimer()
    {
        _timer.Start(_connectionSchedule.NextInterval);
    }
}