namespace RPG_dotnet.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex, _logger);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception ex, ILogger<ExceptionHandlingMiddleware> logger)
        {
            var statusCode = (int)HttpStatusCode.InternalServerError;
            string responseMessage = ErrorMessages.INTERNAL_SERVER_ERROR;
            object? responseData = null;

            if (ex is AuthException authException)
            {
                statusCode = authException.StatusCode;
                responseMessage = authException.Message;
                logger.LogWarning(authException, "{ResponseMessage} (Status: {StatusCode})", responseMessage, statusCode);
            }
            else if (ex is ConflictException conflictEx)
            {
                statusCode = conflictEx.StatusCode;
                responseMessage = conflictEx.Message;
                logger.LogWarning(conflictEx, "{ResponseMessage} (Status: {StatusCode})", responseMessage, statusCode);
            }
            else if (ex is NotFoundException notFoundEx)
            {
                statusCode = notFoundEx.StatusCode;
                responseMessage = notFoundEx.Message;
                logger.LogWarning(notFoundEx, "{ResponseMessage} (Status: {StatusCode})", responseMessage, statusCode);
            }
            else if (ex is FluentValidation.ValidationException validationException)
            {
                statusCode = (int)HttpStatusCode.BadRequest;
                responseMessage = ErrorMessages.VALIDATION_ERROR;
                responseData = validationException.Errors.Select(e => new
                {
                    propertyName = e.PropertyName,
                    errorMessage = e.ErrorMessage
                }).ToList<object>();
                logger.LogWarning(validationException, "{ResponseMessage} (Status: {StatusCode}, Details: {ValidationErrors})", responseMessage, statusCode, responseData);
            }
            else if (ex is DbUpdateException updateException)
            {
                if (updateException.InnerException is SqlException sqlEx)
                {
                    if (sqlEx.Number == 2627 || sqlEx.Number == 2601)
                    {
                        statusCode = (int)HttpStatusCode.Conflict;
                        responseMessage = ErrorMessages.CONFLICT_ERROR;
                        logger.LogWarning(updateException, "{ResponseMessage} (SQL Error: {SqlErrorCode}, Status: {StatusCode})", responseMessage, sqlEx.Number, statusCode);
                    }
                    else
                    {
                        statusCode = (int)HttpStatusCode.InternalServerError;
                        responseMessage = ErrorMessages.DATABASE_ERROR;
                        logger.LogError(updateException, "{ResponseMessage} (SQL Error: {SqlErrorCode}, Status: {StatusCode})", responseMessage, sqlEx.Number, statusCode);
                    }
                }
                else
                {
                    statusCode = (int)HttpStatusCode.InternalServerError;
                    responseMessage = ErrorMessages.ORM_ERROR;
                    logger.LogError(updateException, "{ResponseMessage} (Status: {StatusCode})", responseMessage, statusCode);
                }
            }
            else if (ex is SqlException sqlException)
            {
                if (sqlException.Number == 2627 || sqlException.Number == 2601)
                {
                    statusCode = (int)HttpStatusCode.Conflict;
                    responseMessage = ErrorMessages.CONFLICT_ERROR;
                    logger.LogWarning(sqlException, "{ResponseMessage} (SQL Error: {SqlErrorCode}, Status: {StatusCode})", responseMessage, sqlException.Number, statusCode);
                }
                else
                {
                    statusCode = (int)HttpStatusCode.InternalServerError;
                    responseMessage = ErrorMessages.DATABASE_ERROR;
                    logger.LogError(sqlException, "{ResponseMessage} (SQL Error: {SqlErrorCode}, Status: {StatusCode})", responseMessage, sqlException.Number, statusCode);
                }
            }

            else if (ex is BaseException baseException)
            {
                statusCode = baseException.StatusCode;
                responseMessage = baseException.Message;
                logger.LogWarning(baseException, "{ResponseMessage} (Status: {StatusCode})", responseMessage, statusCode);
            }
            else if (ex is NullReferenceException)
            {
                statusCode = (int)HttpStatusCode.InternalServerError;
                responseMessage = ErrorMessages.INTERNAL_SERVER_ERROR;
                logger.LogError(ex, "{ResponseMessage} (Status: {StatusCode})", responseMessage, statusCode);
            }
            else
            {
                statusCode = (int)HttpStatusCode.InternalServerError;
                responseMessage = ErrorMessages.INTERNAL_SERVER_ERROR;
                logger.LogError(ex, "{ResponseMessage} (Status: {StatusCode})", responseMessage, statusCode);
            }


            context.Response.StatusCode = statusCode;


            var serviceResponse = new ServiceResponse<object>
            {
                success = false,
                message = responseMessage,
                data = responseData
            };

            var jsonExceptionResponse = JsonConvert.SerializeObject(serviceResponse);

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(jsonExceptionResponse);
        }
    }
}