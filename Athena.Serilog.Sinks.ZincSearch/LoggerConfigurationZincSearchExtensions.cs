// Copyright 2020-2022 Mykhailo Shevchuk & Contributors
//
// Licensed under the MIT license;
// you may not use this file except in compliance with the License.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See LICENSE file in the project root for full license information.

using Athena.Serilog.Sinks.ZincSearch.HttpClients;
using Athena.Serilog.Sinks.ZincSearch.Utils;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;

namespace Athena.Serilog.Sinks.ZincSearch;

/// <summary>
/// Class containing extension methods to <see cref="LoggerConfiguration"/>, configuring sinks
/// sending log events to Grafana Loki using HTTP.
/// </summary>
public static class LoggerConfigurationZincSearchExtensions
{
    /// <summary>
    /// Adds a non-durable sink that will send log events to Grafana Loki.
    /// A non-durable sink will lose data after a system or process restart.
    /// </summary>
    /// <param name="sinkConfiguration">
    /// The logger configuration.
    /// </param>
    /// <param name="uri">
    /// The root URI of Loki.
    /// </param>
    /// <param name="index">
    /// The index of the log entries.
    /// </param>
    /// <param name="credentials">
    /// Auth <see cref="ZincSearchCredentials"/>.
    /// </param>
    /// <param name="rollingInterval">
    /// The interval at which the log file will be rolled. Default value is null. <see cref="ZincSearchRollingInterval"/>
    /// </param>
    /// <param name="tenant">
    /// Tenant ID See <a href="https://grafana.com/docs/loki/latest/operations/multi-tenancy/">docs</a>.
    /// </param>
    /// <param name="restrictedToMinimumLevel">
    /// The minimum level for events passed through the sink.
    /// Default value is <see cref="LevelAlias.Minimum"/>.
    /// </param>
    /// <param name="batchPostingLimit">
    /// The maximum number of events to post in a single batch. Default value is 1000.
    /// </param>
    /// <param name="queueLimit">
    /// The maximum number of events stored in the queue in memory, waiting to be posted over
    /// the network. Default value is infinitely.
    /// </param>
    /// <param name="period">
    /// The time to wait between checking for event batches. Default value is 2 seconds.
    /// </param>
    /// <param name="textFormatter">
    /// The formatter rendering individual log events into text, for example JSON. Default
    /// value is <see cref="MessageTemplateTextFormatter"/>.
    /// </param>
    /// <param name="httpClient">
    /// A custom <see cref="IZincSearchHttpClient"/> implementation. Default value is
    /// <see cref="ZincSearchHttpClient"/>.
    /// </param>
    /// <param name="reservedPropertyRenamingStrategy">
    /// Renaming strategy for properties' names equal to reserved keywords.
    /// </param>
    /// <returns>Logger configuration, allowing configuration to continue.</returns>
    public static LoggerConfiguration ZincSearch(
        this LoggerSinkConfiguration sinkConfiguration,
        string uri,
        string index,
        ZincSearchCredentials credentials,
        ZincSearchRollingInterval? rollingInterval= null,
        string? tenant = null,
        LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
        int batchPostingLimit = 1000,
        int? queueLimit = null,
        TimeSpan? period = null,
        ITextFormatter? textFormatter = null,
        IZincSearchHttpClient? httpClient = null,
        IReservedPropertyRenamingStrategy? reservedPropertyRenamingStrategy = null)
    {
        if (sinkConfiguration == null)
        {
            throw new ArgumentNullException(nameof(sinkConfiguration));
        }

        reservedPropertyRenamingStrategy ??= new DefaultReservedPropertyRenamingStrategy();
        period ??= TimeSpan.FromSeconds(1);
        textFormatter ??= new ZincSearchJsonTextFormatter(reservedPropertyRenamingStrategy);
        httpClient ??= new ZincSearchHttpClient();

        httpClient.SetCredentials(credentials);
        httpClient.SetTenant(tenant);

        if (rollingInterval.HasValue)
        {
            index += rollingInterval.Value switch
            {
                ZincSearchRollingInterval.Year => $"-{DateTime.Now.Year}",
                ZincSearchRollingInterval.Month => $"-{DateTime.Now:yyyyMM}",
                ZincSearchRollingInterval.Day => $"-{DateTime.Now:yyyyMMdd}",
                ZincSearchRollingInterval.Hour => $"-{DateTime.Now:yyyyMMddHH}",
                ZincSearchRollingInterval.Minute => $"-{DateTime.Now:yyyyMMddHHmm}",
                _ => throw new ArgumentOutOfRangeException(nameof(rollingInterval), rollingInterval, null)
            };
        }

        var batchFormatter = new ZincSearchBatchFormatter(index);

        var sink = new ZincSearchSink(
            ZincSearchRoutesBuilder.BuildLogsEntriesRoute(uri),
            batchPostingLimit,
            queueLimit,
            period.Value,
            textFormatter,
            batchFormatter,
            httpClient);

        return sinkConfiguration.Sink(sink, restrictedToMinimumLevel);
    }

    /// <summary>
    /// Adds a non-durable sink that will send log events to ZincSearch.
    /// </summary>
    public enum ZincSearchRollingInterval
    {
        /// <summary>
        /// The log file will be rolled every year.
        /// </summary>
        Year = 0,

        /// <summary>
        /// The log file will be rolled every month.
        /// </summary>
        Month,

        /// <summary>
        /// The log file will be rolled every day.
        /// </summary>
        Day,

        /// <summary>
        /// The log file will be rolled every hour.
        /// </summary>
        Hour,
        /// <summary>
        /// The log file will be rolled every minute.
        /// </summary>
        Minute
    }
}