using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet.Validations
{
    public class CreateLoadoutDtoValidator : AbstractValidator<CreateLoadoutDto>
    {
        public CreateLoadoutDtoValidator()
        {
            RuleFor(x => x.name).NotEmpty().MinimumLength(3);
            RuleFor(x => x.characterIds).NotNull().Must(ids => ids.Count == 2)
                .WithMessage("A loadout must have exactly 2 characters: one Vanguard and one Support.");
        }
    }
}