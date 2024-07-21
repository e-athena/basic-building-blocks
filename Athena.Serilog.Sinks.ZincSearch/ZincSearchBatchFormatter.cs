using System.Text;
using System.Text.Json;
using Athena.Serilog.Sinks.ZincSearch.Models;
using Serilog.Formatting;

namespace Athena.Serilog.Sinks.ZincSearch;

/// <summary>
/// Formatter serializing batches of log events into a JSON object in the format, recognized by ZincSearch.
/// <para/>
/// Example:
/// <code>
/// {
///     "index": "value",
///     "records": [
///     {
///         "additionalProp1": {},
///         "additionalProp2": {}
///     }
///     ]
/// }
/// </code>
/// </summary>
public class ZincSearchBatchFormatter : IZincSearchBatchFormatter
{
    private const int DefaultWriteBufferCapacity = 256;
    private readonly string _index;

    /// <summary>
    ///
    /// </summary>
    /// <param name="index"></param>
    public ZincSearchBatchFormatter(string index)
    {
        _index = index;
    }


    /// <summary>
    ///
    /// </summary>
    /// <param name="zincSearchLogEvents"></param>
    /// <param name="formatter"></param>
    /// <param name="output"></param>
    /// <exception cref="NotImplementedException"></exception>
    public void Format(IReadOnlyCollection<ZincSearchLogEvent> zincSearchLogEvents, ITextFormatter formatter, TextWriter output)
    {
        if (zincSearchLogEvents == null)
        {
            throw new ArgumentNullException(nameof(zincSearchLogEvents));
        }

        if (formatter == null)
        {
            throw new ArgumentNullException(nameof(formatter));
        }

        if (output == null)
        {
            throw new ArgumentNullException(nameof(output));
        }

        if (zincSearchLogEvents.Count == 0)
        {
            return;
        }

        var batch = new ZincSearchBatch(_index);

        foreach (var item in zincSearchLogEvents)
        {
            var buffer = new StringWriter(new StringBuilder(DefaultWriteBufferCapacity));
            var logEvent = item.LogEvent;
            formatter.Format(logEvent, buffer);
            var jsonStr = buffer.ToString().TrimEnd('\r', '\n');
            var record = JsonSerializer.Deserialize<object>(jsonStr);
            if (record == null)
            {
                continue;
            }

            batch.Records.Add(record);
        }

        if (batch.IsNotEmpty)
        {
            output.Write(batch.Serialize());
        }
    }
}