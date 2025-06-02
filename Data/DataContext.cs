using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RPG_dotnet.Models;

namespace RPG_dotnet.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<Characters> Characters => Set<Characters>();
        public DbSet<User> Users => Set<User>();
        public DbSet<UserCharacter> UserCharacters => Set<UserCharacter>();
        public DbSet<Ability> Abilities => Set<Ability>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Unique indexes
            modelBuilder.Entity<Characters>()
                .HasIndex(c => c.name)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.userName)
                .IsUnique();

            // Seed abilities
            modelBuilder.Entity<Ability>().HasData(
                new Ability
                {
                    id = 1,
                    name = "Excalibur",
                    description = "Unleashes a powerful beam of light.",
                    manaCost = 30,
                    damage = 40
                },
                new Ability
                {
                    id = 2,
                    name = "Unlimited blade works",
                    description = "Unleashes an arsenal of copied sacred treasures toward the target. - \"I am the bone of my sword \"",
                    manaCost = 60,
                    damage = 70
                }
            );
        }
    }
}
