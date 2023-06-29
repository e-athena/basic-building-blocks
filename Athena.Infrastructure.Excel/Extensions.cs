namespace Microsoft.Extensions.DependencyInjection;

public static class Extensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomExcelExporter(this IServiceCollection services)
    {
        services.AddSingleton<ICommonExcelExporter, CommonExcelExporter>();
        return services;
    }
}