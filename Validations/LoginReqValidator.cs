using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet.Validations
{
    public class LoginReqValidator: AbstractValidator<UserLoginDto>
    {
        public LoginReqValidator()
        {
            RuleFor(x => x.password).NotEmpty().WithMessage("Password is required.");
            RuleFor(x => x.userName).NotEmpty().WithMessage("UserName is required.");
        }
    }
}