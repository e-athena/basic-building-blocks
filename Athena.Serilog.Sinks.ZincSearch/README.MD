# Athena.Serilog.Sinks.ZincSearch

## ZincSearch Sink for Serilog
> https://zincsearch-docs.zinc.dev/

## Quickstart
The `Athena.Serilog.Sinks.ZincSearch` NuGet [package could be found here](https://www.nuget.org/packages/Athena.Serilog.Sinks.ZincSearch). Alternatively you can install it via one of the following commands below:

NuGet command:
```bash
Install-Package Athena.Serilog.Sinks.ZincSearch
```
.NET Core CLI:
```bash
dotnet add package Athena.Serilog.Sinks.ZincSearch
```

In the following example, the sink will send log events to ZincSearch available on `http://localhost:4080`
```csharp
ILogger logger = new LoggerConfiguration()
    .WriteTo.ZincSearch(
        "http://localhost:4080"
         index: "zincsearch_test",
         credentials: new ZincSearchCredentials{
            Username = "admin",
            Password = "123456"
         }
    )
    .CreateLogger();

logger.Information("The god of the day is {@God}", odin)
```

Used in conjunction with [Serilog.Settings.Configuration](https://github.com/serilog/serilog-settings-configuration) the same sink can be configured in the following way:

```json
{
  "Serilog": {
    "Using": [
      "Athena.Serilog.Sinks.ZincSearch"
    ],
    "MinimumLevel": {
      "Default": "Debug"
    },
    "WriteTo": [
      {
        "Name": "ZincSearch",
        "Args": {
          "uri": "http://localhost:4080",
          "index": "zincsearch_test",
          "credentials": {
            "username": "admin",
            "password": "123456"
          }
        }
      }
    ]
  }
}
```

Description of parameters and configuration details could be found [here](https://github.com/mishamyte/serilog-sinks-grafana-ZincSearch/wiki/Application-settings).

### Custom HTTP Client
Athena.Serilog.Sinks.ZincSearch exposes `IZincSearchHttpClient` interface with the main operations, required for sending logs.
In order to use a custom HttpClient you can extend of default implementations:
- `Athena.Serilog.Sinks.ZincSearch.HttpClients.BaseZincSearchHttpClient` (implements creation of internal `HttpClient` and setting credentials)
- `Athena.Serilog.Sinks.ZincSearch.HttpClients.ZincSearchHttpClient` (default client which sends logs via HTTP)
- `Athena.Serilog.Sinks.ZincSearch.HttpClients.ZincSearchGzipHttpClient` (default client which sends logs via HTTP with gzip compression)

or create one implementing `Athena.Serilog.Sinks.ZincSearch.IZincSearchHttpClient`.

```csharp
// CustomHttpClient.cs

public class CustomHttpClient : BaseZincSearchHttpClient
{
    public override Task<HttpResponseMessage> PostAsync(string requestUri, Stream contentStream)
    {
        return base.PostAsync(requestUri, contentStream);
    }
}
```
```csharp
// Usage

Log.Logger = new LoggerConfiguration()
    .WriteTo.ZincSearch(
         "http://localhost:4080",
         index: "zincsearch_test",
         credentials: new ZincSearchCredentials{
            Username = "admin",
            Password = "123456"
         },
         httpClient: new CustomHttpClient()
    )
    .CreateLogger();
```

### Sending json content to ZincSearch
From v8 Athena.Serilog.Sinks.ZincSearch uses `ZincSearchJsonTextFormatter` by default, which allows to send logs to ZincSearch as a JSON-payloads. This allows easier filtering in ZincSearch v2, more information about how to filter can be found [here](https://grafana.com/blog/2020/10/28/ZincSearch-2.0-released-transform-logs-as-youre-querying-them-and-set-up-alerts-within-ZincSearch/)

Also, you could implement your own formatter, implementing `Serilog.Formatting.ITextFormatter` interface and pass it to the sink configuration.

Example configuration:
```json
{
  "Serilog": {
    "Using": [
      "Athena.Serilog.Sinks.ZincSearch"
    ],
    "MinimumLevel": {
      "Default": "Debug"
    },
    "WriteTo": [
      {
        "Name": "ZincSearch",
        "Args": {
          "uri": "http://localhost:4080",
          "index": "zincsearch_test",
          "credentials": {
            "username": "admin",
            "password": "123456"
          },
          "textFormatter": "My.Awesome.Namespace.MyTextFormatter, MyCoolAssembly"
        }
      }
    ]
  }
}
```

### Inspiration and Credits
- [Athena.Serilog.Sinks.ZincSearch](https://github.com/serilog-contrib/serilog-sinks-grafana-ZincSearch)