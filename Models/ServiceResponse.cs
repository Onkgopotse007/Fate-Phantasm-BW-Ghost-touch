using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet.Models
{
    public class ServiceResponse<T>
    {
        public bool success { get; set; } = true;
        public required string message { get; set; }
        public T? data { get; set; }
        
    }
}