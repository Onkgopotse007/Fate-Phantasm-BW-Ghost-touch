using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet.Data
{
    public class UserRegistrationResponse
    {
        public int user_id { get; set; }
    }

    public class UserLoginResponse
    {
        public string token { get; set; }
    }

    public interface IAuthService
    {
        Task<ServiceResponse<UserRegistrationResponse>> Register(User user, string password);
        Task<ServiceResponse<UserLoginResponse>> Login(string userName, string password);
        Task<bool> UserExists(string userName);
    }

}