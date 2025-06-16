using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet.Models
{
    public class ServiceResponse<T> : IServiceResponse
    {
        public bool success { get; set; } = true;
        public required string message { get; set; }
        public T? data { get; set; }

        public static ServiceResponse<T> Success(T data, string message)
        {
            return new ServiceResponse<T>
            {
                success = true,
                message = message,
                data = data
            };
        }


    }
    public interface IServiceResponse
    {
        bool success { get; }
        string message { get; }
    }

}