using ValidationException = Athena.Infrastructure.FluentValidation.ValidationException;

// ReSharper disable once CheckNamespace

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 
/// </summary>
public static class Extensions
{
    private static ValidationResultModel ToValidationResultModel(this ValidationResult validationResult)
    {
        return new ValidationResultModel(validationResult);
    }

    /// <summary>
    /// Ref https://www.jerriepelser.com/blog/validation-response-aspnet-core-webapi
    /// </summary>
    public static async Task HandleValidation<TRequest>(this IValidator<TRequest> validator, TRequest request)
    {
        // 验证
        var validationResult = await validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.ToValidationResultModel());
        }
    }

    /// <summary>
    /// 添加自定义验证器
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assemblyKeyword">程序集关键字</param>
    /// <returns></returns>
    public static IServiceCollection AddCustomValidators(this IServiceCollection services,
        string? assemblyKeyword = null)
    {
        var assemblies = AssemblyHelper.GetCurrentDomainBusinessAssemblies(assemblyKeyword);
        return services.AddCustomValidators(assemblies);
    }

    /// <summary>
    /// 添加自定义验证器
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assemblyKeywords"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomValidators(this IServiceCollection services,
        params string[] assemblyKeywords)
    {
        var assemblies = AssemblyHelper.GetCurrentDomainBusinessAssemblies(assemblyKeywords);
        return services.AddCustomValidators(assemblies);
    }

    /// <summary>
    /// 添加自定义验证器
    /// </summary>
    /// <param name="services"></param>
    /// <param name="assemblies"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomValidators(this IServiceCollection services,
        params Assembly[] assemblies)
    {
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));
        services.Scan(scan => scan
            .FromAssemblies(assemblies)
            .AddClasses(c => c.AssignableTo(typeof(IValidator<>)))
            .AsImplementedInterfaces()
            .WithTransientLifetime());
        return services;
    }

    /// <summary>
    /// 添加自定义验证器
    /// </summary>
    /// <param name="services"></param>
    /// <param name="types"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomValidators(this IServiceCollection services,
        params Type[] types)
    {
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));
        services.Scan(scan => scan
            .FromTypes(types)
            .AddClasses(c => c.AssignableTo(typeof(IValidator<>)))
            .AsImplementedInterfaces()
            .WithTransientLifetime());
        return services;
    }

    /// <summary>
    /// 添加自定义验证器
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomValidators<TType>(this IServiceCollection services)
    {
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));
        services.Scan(scan => scan
            .FromAssemblyOf<TType>()
            .AddClasses(c => c.AssignableTo(typeof(IValidator<>)))
            .AsImplementedInterfaces()
            .WithTransientLifetime());
        return services;
    }
}