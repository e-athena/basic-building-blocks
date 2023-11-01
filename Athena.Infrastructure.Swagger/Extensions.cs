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
        var serviceName = configuration.GetEnvValue<string>("ServiceName");
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
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(config =>
        {
            // config.SwaggerDoc("v1", openApiInfo);
            typeof(ApiVersions).GetEnumNames().ToList().ForEach(version =>
            {
                config.SwaggerDoc(version.ToLower(), new OpenApiInfo
                {
                    Title = $"{openApiInfo.Title} {version.ToLower()}",
                    Version = version.ToLower(),
                    Contact = openApiInfo.Contact
                });
            });

            // 添加HTTP Header参数
            config.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "请输入JWT Token，不需要输入Bearer",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Scheme = "Bearer",
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
        this WebApplication app,
        // 开发环境下启用Swagger
        bool isDevelopment = true,
        Action<SwaggerOptions>? setupAction = null,
        Action<SwaggerUIOptions>? setupAction1 = null,
        Action<ReDocOptions>? setupAction2 = null)
    {
        // 开发环境下启用Swagger
        if (isDevelopment && !app.Environment.IsDevelopment())
        {
            return app;
        }

        app.UseSwagger(opts => { setupAction?.Invoke(opts); });
        app.UseSwaggerUI(options =>
        {
            // options.SwaggerEndpoint("/swagger/v1/swagger.json", "Swagger APIs");

            typeof(ApiVersions).GetEnumNames().ToList().ForEach(version =>
            {
                options.SwaggerEndpoint($"/swagger/{version.ToLower()}/swagger.json", $"{version} APIs");
            });


            setupAction1?.Invoke(options);
        });
        // app.UseReDoc(options =>
        // {
        //     options.RoutePrefix = "api-docs";
        //     options.SpecUrl = "/swagger/v1/swagger.json";
        //     options.ConfigObject = new Swashbuckle.AspNetCore.ReDoc.ConfigObject
        //     {
        //         HideDownloadButton = true,
        //         HideLoading = true
        //     };
        //     setupAction2?.Invoke(options);
        // });
        return app;
    }
}