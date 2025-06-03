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
    }
}