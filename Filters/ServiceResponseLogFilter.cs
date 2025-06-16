using Serilog;
namespace RPG_dotnet.Filters
{

    public class ServiceResponseLogFilter : IAsyncResultFilter
    {
        private readonly Serilog.ILogger _logger;

        public ServiceResponseLogFilter(Serilog.ILogger logger)
        {
            // Inject Serilog logger for logging
            _logger = logger.ForContext<ServiceResponseLogFilter>();
        }

        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            await next();
            if (context.Result is ObjectResult objectResult)
            {
                if (objectResult.Value is IServiceResponse serviceResponse)
                {
                    if (serviceResponse.success)
                    {
                        _logger.Information("{message}", serviceResponse.message);
                    }
                    else
                    {
                        _logger.Warning("{message}", serviceResponse.message);
                    }
                }
            }
        }
    }
}