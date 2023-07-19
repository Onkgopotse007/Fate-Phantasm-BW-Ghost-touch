using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet.Dtos.User
{
    public class UserLoginDto
    {
        public required string userName { get; set; }
        public required string password {get; set;}
    }
}