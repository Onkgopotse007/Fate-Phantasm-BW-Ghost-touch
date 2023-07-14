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
            ValidationResult validationResult = await validator.ValidateAsync(dto);

            if (!validationResult.IsValid)
            {
                var validationErrors = validationResult.Errors;
                throw new FluentValidation.ValidationException("Input validation failed", validationErrors);
            }
        }
    }
}