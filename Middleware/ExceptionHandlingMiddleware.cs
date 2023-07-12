using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RPG_dotnet.Helpers;

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
            Log.Error(ex, "An exception occurred", ex.Message);
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

            context.Response.StatusCode = statusCode;

            var errorResponse = new
            {
                success = false,
                message = message,
                exception = ex.GetType().Name,
                data = new string[] { }
            };

            var jsonExceptionResponse = JsonConvert.SerializeObject(errorResponse);

            context.Response.ContentType = "application/json";

            return context.Response.WriteAsync(jsonExceptionResponse);
        }
    }
}
