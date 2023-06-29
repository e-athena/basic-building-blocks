using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Enrichers;

/// <summary>
/// 
/// </summary>
public class ClientIpEnricher : ILogEventEnricher
{
    private const string IpAddressPropertyName = "ClientIp";
    private const string IpAddressItemKey = "Serilog_ClientIp";

    private readonly IHttpContextAccessor _contextAccessor;

    /// <summary>
    /// 
    /// </summary>
    public ClientIpEnricher()
    {
        _contextAccessor = new HttpContextAccessor();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="contextAccessor"></param>
    internal ClientIpEnricher(IHttpContextAccessor contextAccessor)
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

        if (httpContext.Items[IpAddressItemKey] is LogEventProperty logEventProperty)
        {
            logEvent.AddPropertyIfAbsent(logEventProperty);
            return;
        }

        var ipAddress = GetIpAddress();

        if (string.IsNullOrWhiteSpace(ipAddress))
            ipAddress = "unknown";

        var ipAddressProperty = new LogEventProperty(IpAddressPropertyName, new ScalarValue(ipAddress));
        httpContext.Items.Add(IpAddressItemKey, ipAddressProperty);

        logEvent.AddPropertyIfAbsent(ipAddressProperty);
    }


    private string GetIpAddress()
    {
        var ipAddress = _contextAccessor.HttpContext?.Request?.Headers[ClinetIpConfiguration.XForwardHeaderName]
            .FirstOrDefault();

        if (!string.IsNullOrEmpty(ipAddress))
            return GetIpAddressFromProxy(ipAddress);

        return _contextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
    }

    private string GetIpAddressFromProxy(string proxifiedIpList)
    {
        var addresses = proxifiedIpList.Split(',');

        if (addresses.Length != 0)
        {
            // If IP contains port, it will be after the last : (IPv6 uses : as delimiter and could have more of them)
            return addresses[0].Contains(":")
                ? addresses[0].Substring(0, addresses[0].LastIndexOf(":", StringComparison.Ordinal))
                : addresses[0];
        }

        return string.Empty;
    }
}