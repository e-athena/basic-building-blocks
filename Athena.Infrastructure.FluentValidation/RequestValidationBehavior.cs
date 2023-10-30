namespace Athena.Infrastructure.FluentValidation;

/// <summary>
/// 请求验证行为
/// </summary>
/// <typeparam name="TRequest"></typeparam>
/// <typeparam name="TResponse"></typeparam>
public class RequestValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// 处理验证
    /// </summary>
    /// <param name="request"></param>
    /// <param name="next"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var validator = AthenaProvider.GetService<IValidator<TRequest>>();
        if (validator == null)
        {
            return await next();
        }
        // 处理验证
        await validator.HandleValidation(request);
        return await next();
    }
}