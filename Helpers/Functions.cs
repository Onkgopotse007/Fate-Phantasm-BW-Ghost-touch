using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet.Helpers
{
    public class Functions
    {
        public async Task validateDtoAsync<TDto>(TDto dto, IValidator<TDto> validator)
        {
            FluentValidation.Results.ValidationResult validationResult = await validator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                var validationErrors = validationResult.Errors;
                throw new FluentValidation.ValidationException("Input validation failed", validationErrors);
            }
        }
        public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
        public bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using( var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt)){
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
    }
}