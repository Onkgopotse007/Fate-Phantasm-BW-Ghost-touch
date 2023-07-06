using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet.Models
{
    public class ServiceResponse<T>
    {
        public bool success { get; set; } = true;
        public string message { get; set; } = string.Empty;
        public T? data { get; set; }
        
    }
}