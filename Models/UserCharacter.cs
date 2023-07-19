using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RPG_dotnet.Models
{
    public class UserCharacter
    {
        [Key]
        public int id { get; set; }

        public int userId { get; set; }
        public User user { get; set; }
        public int characterId { get; set; }
        public Characters character { get; set; }
    }
}