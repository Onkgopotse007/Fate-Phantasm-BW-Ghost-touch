using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RPG_dotnet.Validations
{
    public class CreateUserReqValidator : AbstractValidator<UserRegisterDto>
    {
        public CreateUserReqValidator()
        {
            RuleFor(x => x.password).NotEmpty().WithMessage("Password is required.")
            .Length(8, 16).WithMessage("Password must be between 8 and 16 characters.")
            .Must(ContainNumber).WithMessage("Password must contain at least one number.")
            .Must(ContainUpperCaseLetter).WithMessage("Password must contain at least one uppercase letter.")
            .Must(ContainLowerCaseLetter).WithMessage("Password must contain at least one lowercase letter.")
            .Must(ContainSpecialCharacter).WithMessage("Password must contain at least one special character.");
            RuleFor(x => x.userName).NotEmpty().WithMessage("UserName is required.")
            .Length(1, 16).WithMessage("userName must be between 1 and 16 characters.")
            .Matches(@"^[A-Za-z0-9_-]+$").WithMessage("userName can only contain letters, numbers, underscores, and hyphens.")
            .Must(NotStartOrEndWithSpecialCharacters).WithMessage("userName cannot start or end with special characters.")
            .Must(NotContainSpaces).WithMessage("userName cannot contain spaces.");
        }
        private bool ContainNumber(string password)
        {
            return Regex.IsMatch(password, @"\d");
        }

        private bool ContainUpperCaseLetter(string password)
        {
            return Regex.IsMatch(password, @"[A-Z]");
        }

        private bool ContainLowerCaseLetter(string password)
        {
            return Regex.IsMatch(password, @"[a-z]");
        }

        private bool ContainSpecialCharacter(string password)
        {
            return Regex.IsMatch(password, @"[!@#$%^&*(),.?""':{}|<>_-]");
        }
        private bool NotStartOrEndWithSpecialCharacters(string userName)
        {
            return !(userName.StartsWith("_") || userName.StartsWith("-") || userName.EndsWith("_") || userName.EndsWith("-"));
        }
        private bool NotContainSpaces(string userName)
        {
            return !userName.Contains(" ");
        }
    }
}