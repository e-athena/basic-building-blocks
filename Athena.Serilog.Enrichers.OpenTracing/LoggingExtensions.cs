using Serilog;
using Serilog.Configuration;

namespace Athena.Serilog.Enrichers.OpenTracing;

public static class LoggingExtensions
{
    public static LoggerConfiguration WithOpenTracingContext(this LoggerEnrichmentConfiguration enrich)
    {
        if (enrich == null)
            throw new ArgumentNullException(nameof(enrich));

        return enrich.With<OpenTracingContextLogEventEnricher>();
    }
}