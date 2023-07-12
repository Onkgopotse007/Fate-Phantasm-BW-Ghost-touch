using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet.Helpers
{
public class BaseException : Exception
{
    public int StatusCode { get; }

    public BaseException(string message = "An error occurred", int statusCode = (int)HttpStatusCode.InternalServerError) : base(message)
    {
        StatusCode = statusCode;
    }
}

public class GenericException : BaseException
{
    public GenericException(string message, int statusCode) : base(message, statusCode)
    {
    }
}

public class NotFoundException : BaseException
{
    public NotFoundException(string message = "Not Found") : base(message, (int)HttpStatusCode.NotFound)
    {
    }
}

public class ConflictException : BaseException
{
    public ConflictException(string message = "Conflict", int statusCode = (int)HttpStatusCode.Conflict) : base(message, statusCode)
    {
    }
}

public class DatabaseException : BaseException
{
    public DatabaseException(string message = "Database exception") : base(message, (int)HttpStatusCode.InternalServerError)
    {
    }
}

}