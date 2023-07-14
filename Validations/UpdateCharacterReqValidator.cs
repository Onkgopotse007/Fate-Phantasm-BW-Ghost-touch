using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet.Validations
{
    public class UpdateCharacterReqValidator:AbstractValidator<UpdateCharacterDto>
    {
        public UpdateCharacterReqValidator()
        {  
            RuleFor(x => x.id).GreaterThanOrEqualTo(1);
            RuleFor(x => x.hitpoints).LessThanOrEqualTo(1000);
            RuleFor(x => x.strength).LessThanOrEqualTo(100);
            RuleFor(x => x.defense).LessThanOrEqualTo(100);
            RuleFor(x=> x.intelligence).LessThanOrEqualTo(100);
        }
    }
}