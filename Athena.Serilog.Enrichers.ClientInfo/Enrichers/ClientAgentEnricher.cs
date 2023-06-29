using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Enrichers;

/// <summary>
/// 
/// </summary>
public class ClientAgentEnricher : ILogEventEnricher
{
    private const string ClientAgentPropertyName = "ClientAgent";
    private const string ClientAgentItemKey = "Serilog_ClientAgent";

    private readonly IHttpContextAccessor _contextAccessor;

    /// <summary>
    /// 
    /// </summary>
    public ClientAgentEnricher() : this(new HttpContextAccessor())
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="contextAccessor"></param>
    internal ClientAgentEnricher(IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="logEvent"></param>
    /// <param name="propertyFactory"></param>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var httpContext = _contextAccessor.HttpContext;
        if (httpContext == null)
            return;

        if (httpContext.Items[ClientAgentItemKey] is LogEventProperty logEventProperty)
        {
            logEvent.AddPropertyIfAbsent(logEventProperty);
            return;
        }

        var agentName = httpContext.Request.Headers["User-Agent"];

        var clientAgentProperty = new LogEventProperty(ClientAgentPropertyName, new ScalarValue(agentName));
        httpContext.Items.Add(ClientAgentItemKey, clientAgentProperty);

        logEvent.AddPropertyIfAbsent(clientAgentProperty);
    }
}