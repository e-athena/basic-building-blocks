// ReSharper disable once CheckNamespace

namespace Microsoft.Extensions.DependencyInjection;

public static class Extensions
{
    /// <summary>
    /// 添加Swagger服务
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="configAction"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomSwaggerGen(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<SwaggerGenOptions>? configAction = null)
    {
        return services.AddCustomSwaggerGen(configuration, new OpenApiContact
        {
            Email = "zheng_jinfan@126.com",
            Name = "Mango",
        }, configAction);
    }

    /// <summary>
    /// 添加Swagger服务
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="apiContact"></param>
    /// <param name="configAction"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomSwaggerGen(
        this IServiceCollection services,
        IConfiguration configuration,
        OpenApiContact apiContact,
        Action<SwaggerGenOptions>? configAction = null)
    {
        var serviceName = configuration.GetValue<string>("ServiceName");
        return services.AddCustomSwaggerGen(new OpenApiInfo
        {
            Title = serviceName ?? "SwaggerAPI",
            Version = "v1.0",
            Contact = apiContact
        }, configAction);
    }

    /// <summary>
    /// 添加Swagger服务
    /// </summary>
    /// <param name="services"></param>
    /// <param name="openApiInfo"></param>
    /// <param name="configAction"></param>
    /// <returns></returns>
    public static IServiceCollection AddCustomSwaggerGen(
        this IServiceCollection services,
        OpenApiInfo openApiInfo,
        Action<SwaggerGenOptions>? configAction = null)
    {
        services.AddSwaggerGen(config =>
        {
            config.SwaggerDoc("v1", openApiInfo);
            // 添加HTTP Header参数
            config.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Scheme = "bearer",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT"
            });

            config.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new List<string>()
                }
            });

            // 设置Swagger Json和UI的注释路径。
            var applicationBasePath = PlatformServices.Default.Application.ApplicationBasePath;
            var dir = new DirectoryInfo(applicationBasePath);
            var files = dir.GetFiles().Where(p => p.Name.Contains(".xml")).ToList();
            foreach (var file in files)
            {
                config.IncludeXmlComments(Path.Combine(applicationBasePath, file.Name), true);
            }

            configAction?.Invoke(config);
        });
        return services;
    }

    /// <summary>
    /// Register the Swagger middleware with optional setup action for DI-injected options
    /// </summary>
    public static IApplicationBuilder UseCustomSwagger(
        this IApplicationBuilder app,
        Action<SwaggerOptions>? setupAction = null,
        Action<SwaggerUIOptions>? setupAction1 = null,
        Action<ReDocOptions>? setupAction2 = null)
    {
        app.UseSwagger(opts => { setupAction?.Invoke(opts); });
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "Swagger APIs");
            setupAction1?.Invoke(options);
        });
        app.UseReDoc(options =>
        {
            options.RoutePrefix = "api-docs";
            options.SpecUrl = "/swagger/v1/swagger.json";
            options.ConfigObject = new Swashbuckle.AspNetCore.ReDoc.ConfigObject
            {
                HideDownloadButton = true,
                HideLoading = true
            };
            setupAction2?.Invoke(options);
        });
        return app;
    }
}