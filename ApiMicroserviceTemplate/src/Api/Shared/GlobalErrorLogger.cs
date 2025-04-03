using FastEndpoints;
using FluentValidation;

namespace ApiServiceTemplate.Shared
{
    public class GlobalErrorLogger : IGlobalPostProcessor
    {
        public Task PostProcessAsync(IPostProcessorContext context, CancellationToken ct)
        {
            var logger = context.HttpContext.Resolve<ILogger<GlobalErrorLogger>>();

            if (context.ValidationFailures.Count <= 0) return Task.CompletedTask;
            logger.LogWarning("Validation error count: {@count}", context.ValidationFailures.Count);
            if (!context.HttpContext.Response.HasStarted)
            {
                throw new ValidationException(context.ValidationFailures);
            }

            return Task.CompletedTask;
        }
    }
}