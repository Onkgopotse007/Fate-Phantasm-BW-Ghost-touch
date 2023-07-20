using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet.Data
{
    public class AuthService : IAuthService
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;
        public AuthService(DataContext context, IConfiguration configuration)
        {
            _configuration = configuration;
            _context = context;
            
        }
        public async Task<ServiceResponse<string>> Login(string userName, string password)
        {
            Functions functions = new Functions();
            
            var response = new ServiceResponse<string>();
            var user = await _context.Users
                .FirstOrDefaultAsync(u=> u.userName.Equals(userName));
            if(user is null || !functions.VerifyPasswordHash(password, user.passwordHash,user.passwordSalt))
            {
                throw new NotFoundException("User details incorrect");
            }
            else{
                response.data= CreateToken(user);
                return response;
            }
        
        }

        public async Task<ServiceResponse<int>> Register(User user, string password)
        {
            var response = new ServiceResponse<int>();
            Functions functions = new Functions();
            if(await UserExists(user.userName)){
                throw new ConflictException("User already exists");
            }
            functions.CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);
            user.passwordHash = passwordHash;
            user.passwordSalt = passwordSalt;

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            response.data = user.id;
            response.message ="New user created";
            return response;
        }

        public async Task<bool> UserExists(string userName)
        {
            if (await _context.Users.AnyAsync(u => u.userName.Equals(userName)))
            {
                return true;
            }
            return false;
        }
        private string CreateToken(User user)
        {
            var claims = new List<Claim>{
                new Claim(ClaimTypes.NameIdentifier, user.id.ToString()),
                new Claim(ClaimTypes.Name, user.userName),
                new Claim(ClaimTypes.Role, user.userRole.ToString())
            };
            var appSettingsToken = Environment.GetEnvironmentVariable("TOKEN").ToString();
            if(appSettingsToken is null)
                throw new BaseException("Token issue");

            SymmetricSecurityKey key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(appSettingsToken));

            SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddHours(12),
                SigningCredentials = credentials
            };
            JwtSecurityTokenHandler securityTokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = securityTokenHandler.CreateToken(tokenDescriptor);
            return $"bearer {securityTokenHandler.WriteToken(token)}";
        }

    }
}