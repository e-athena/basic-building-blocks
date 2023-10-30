namespace Athena.Infrastructure.Wechat;

/// <summary>
/// DelegatingFlurlClientFactory
/// </summary>
internal class DelegatingFlurlClientFactory : IFlurlClientFactory
{
    private readonly IHttpClientFactory _httpClientFactory;

    public DelegatingFlurlClientFactory(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    public IFlurlClient Get(Flurl.Url url)
    {
        return new FlurlClient(_httpClientFactory.CreateClient(url.ToUri().Host));
    }

    public void Dispose()
    {
        // Do Nothing
    }
}