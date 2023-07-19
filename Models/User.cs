using System;
using System.Collections.Generic;

using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet.Models
{
    public class User
    {
        
        public int id { get; set; }
        [Required]
        [MaxLength(255)]
        
        public string userName { get; set; } = string.Empty;
        public byte[] passwordHash { get; set; } = new byte[0];
        public byte[] passwordSalt { get; set; } = new byte[0];
        public int userRole {get; set;} = 2;
        public ICollection<UserCharacter> userCharacters { get; set; }

        [NotMapped]
        public ICollection<Characters> characters
        {
            get => userCharacters?.Select(uc => uc.character).ToList();
            set
            {
                if (value.Count > 7)
                {
                    throw new InvalidOperationException("A user cannot have more than 7 characters.");
                }

                if (value.Select(c => c.fighterClass).Distinct().Count() != value.Count)
                {
                    throw new InvalidOperationException("A user cannot have repeating character classes.");
                }

                userCharacters = value.Select(c => new UserCharacter { user = this, character = c }).ToList();
            }
        }
    }
}