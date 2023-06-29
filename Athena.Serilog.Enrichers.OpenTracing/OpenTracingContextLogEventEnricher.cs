using System.Diagnostics;
using Serilog.Core;
using Serilog.Events;

namespace Athena.Serilog.Enrichers.OpenTracing;

public class OpenTracingContextLogEventEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var tracer = Activity.Current;
        if (tracer == null)
        {
            return;
        }

        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TraceId", tracer.TraceId));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("SpanId", tracer.SpanId));
    }
}