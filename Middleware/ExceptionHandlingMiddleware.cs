using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;


namespace RPG_dotnet.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionHandlingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            var statusCode = (int)HttpStatusCode.InternalServerError;
            var message = "An unexpected error occurred";
            var data = new List<object>();
            if (ex is NullReferenceException)
            {
                var notFoundException = new NotFoundException();
                statusCode = notFoundException.StatusCode;
                message = notFoundException.Message;
            }
            else if (ex is SqlException sqlException)
            {
                if (sqlException.Number == 2627 || sqlException.Number == 2601)
                {
                    var conflictException = new ConflictException(sqlException.Message);
                    statusCode = conflictException.StatusCode;
                    message = conflictException.Message;
                }
                else
                {
                    var databaseException = new DatabaseException();
                    statusCode = databaseException.StatusCode;
                    message = databaseException.Message;
                }
            }

            else if (ex is BaseException baseException)
            {
                statusCode = baseException.StatusCode;
                message = ex.Message;
            }
            else if (ex is FluentValidation.ValidationException validationException)
            {
                statusCode = (int)HttpStatusCode.BadRequest;
                message = validationException.Message;
                data = validationException.Errors.Select(e => new
                {
                    PropertyName = e.PropertyName,
                    ErrorMessage = e.ErrorMessage
                }).ToList<object>();
            }
            else if (ex is DbUpdateException updateException)
            {
                if (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx && (sqlEx.Number == 2627 || sqlEx.Number == 2601))
                {
                    var conflictException = new ConflictException("One of the inputs is violating a unique key constraint");
                    statusCode = conflictException.StatusCode;
                    message = conflictException.Message;
                }
                else
                {
                    var databaseException = new DatabaseException();
                    statusCode = databaseException.StatusCode;
                    message = databaseException.Message;
                }
            }
            else if(ex is AuthException authException){
                statusCode = authException.StatusCode;
                message = authException.Message;
            }
            else
            {
                GenericException genericException = new GenericException();
                statusCode = genericException.StatusCode;
                message = genericException.Message;
            }
            

            context.Response.StatusCode = statusCode;

            var errorResponse = new
            {
                success = false,
                message = message,
                exception = ex.GetType().Name,
                data = data
            };
            Log.Error(ex, $"An exception occurred {ex.Message}", data);

            var jsonExceptionResponse = JsonConvert.SerializeObject(errorResponse);

            context.Response.ContentType = "application/json";

            return context.Response.WriteAsync(jsonExceptionResponse);
        }
    }
}
