using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet.Helpers
{
public class BaseException : Exception
{
    public int StatusCode { get; }
    public new List<object>? Data {get;}

    public BaseException(string message = "An error occurred", int statusCode = (int)HttpStatusCode.InternalServerError, List<object>? data = null) : base(message)
    {
        StatusCode = statusCode;
        Data = data;
    }
}

public class GenericException : BaseException
{
    public GenericException(string message ="An API exception occured", int statusCode= (int)HttpStatusCode.InternalServerError, List<object>? data = null) : base(message, statusCode, data)
    {
    }
}

public class NotFoundException : BaseException
{
    public NotFoundException(string message = "Not Found", List<object>? data = null) : base(message, (int)HttpStatusCode.NotFound, data)
    {
    }
}

public class ConflictException : BaseException
{
    public ConflictException(string message = "Conflict", int statusCode = (int)HttpStatusCode.Conflict, List<object>? data = null) : base(message, statusCode, data)
    {
    }
}

public class DatabaseException : BaseException
{
    public DatabaseException(string message = "Database exception", List<object>? data = null) : base(message, (int)HttpStatusCode.InternalServerError, data)
    {
    }
}
public class AuthException: BaseException
{
    public AuthException(string message = "Not Authorized", List<object>? data = null, int statusCode =(int)HttpStatusCode.Unauthorized ) : base(message, (int)HttpStatusCode.Unauthorized, data)
    {
        
    }
}
}