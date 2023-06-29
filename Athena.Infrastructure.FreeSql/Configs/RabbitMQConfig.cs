namespace Athena.Infrastructure.FreeSql.Configs;

/// <summary>
/// RabbitMQConfig
/// </summary>
public class RabbitMqConfig
{
    /// <summary>
    /// Default password (value: "guest").
    /// </summary>
    /// <remarks>PLEASE KEEP THIS MATCHING THE DOC ABOVE.</remarks>
    public const string DefaultPass = "guest";

    /// <summary>
    /// Default user name (value: "guest").
    /// </summary>
    /// <remarks>PLEASE KEEP THIS MATCHING THE DOC ABOVE.</remarks>
    public const string DefaultUser = "guest";

    /// <summary>
    /// Default virtual host (value: "/").
    /// </summary>
    /// <remarks> PLEASE KEEP THIS MATCHING THE DOC ABOVE.</remarks>
    public const string DefaultVHost = "/";

    /// <summary>
    /// Default exchange name (value: "cap.default.router").
    /// </summary>
    public const string DefaultExchangeName = "cap.default.router";

    /// <summary> The topic exchange type. </summary>
    public const string ExchangeType = "topic";

    /// <summary>
    /// The host to connect to.
    /// If you want connect to the cluster, you can assign like “192.168.1.111,192.168.1.112”
    /// </summary>
    public string HostName { get; set; } = "localhost";

    /// <summary>
    /// Password to use when authenticating to the server.
    /// </summary>
    public string Password { get; set; } = DefaultPass;

    /// <summary>
    /// Username to use when authenticating to the server.
    /// </summary>
    public string UserName { get; set; } = DefaultUser;

    /// <summary>
    /// Virtual host to access during this connection.
    /// </summary>
    public string VirtualHost { get; set; } = DefaultVHost;

    /// <summary>
    /// Topic exchange name when declare a topic exchange.
    /// </summary>
    public string ExchangeName { get; set; } = DefaultExchangeName;

    /// <summary>
    /// The port to connect on.
    /// </summary>
    public int Port { get; set; } = -1;
}