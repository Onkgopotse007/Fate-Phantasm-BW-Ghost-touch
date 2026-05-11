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
        private readonly bool _seedAbilities;

        public DataContext(DbContextOptions<DataContext> options, bool seedAbilities = false)
            : base(options)
        {
            _seedAbilities = seedAbilities;
        }

        public DbSet<Characters> Characters => Set<Characters>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Ability> Abilities => Set<Ability>();
        public DbSet<Loadout> Loadouts { get; set; }

        public DbSet<GameSession> GameSessions { get; set; }
        public DbSet<SessionCharacterState> SessionCharacterStates { get; set; }
        public DbSet<GameActionLog> GameActionLogs { get; set; }

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

            if (_seedAbilities)
            {
                modelBuilder.Entity<Ability>().HasData(
                    new Ability
                    {
                        id = 1,
                        name = "Excalibur",
                        description = "Unleashes a powerful beam of light.",
                        manaCost = 30,
                        damage = 40,
                        characterId = 0
                    },
                    new Ability
                    {
                        id = 2,
                        name = "Unlimited blade works",
                        description = "Unleashes an arsenal of copied sacred treasures toward the target. - \"I am the bone of my sword \"",
                        manaCost = 60,
                        damage = 70,
                        characterId = 0
                    }
                );
            }

            //loadout
            modelBuilder.Entity<LoadoutCharacter>()
                .HasKey(lc => new { lc.loadoutId, lc.characterId });

            modelBuilder.Entity<LoadoutCharacter>()
                .HasOne(lc => lc.loadout)
                .WithMany(l => l.characters)
                .HasForeignKey(lc => lc.loadoutId);

            modelBuilder.Entity<LoadoutCharacter>()
                .HasOne(lc => lc.character)
                .WithMany()
                .HasForeignKey(lc => lc.characterId);

            modelBuilder.Entity<GameSession>()
                .HasMany(gs => gs.participants)
                .WithOne(scs => scs.session)
                .HasForeignKey(scs => scs.sessionId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SessionCharacterState>()
                .HasOne(scs => scs.character)
                .WithMany()
                .HasForeignKey(scs => scs.characterId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SessionCharacterState>()
                .HasOne(scs => scs.user)
                .WithMany()
                .HasForeignKey(scs => scs.userId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<GameSession>()
                .HasOne(gs => gs.creatorUser)
                .WithMany()
                .HasForeignKey(gs => gs.creatorUserId)
                .OnDelete(DeleteBehavior.NoAction);


            modelBuilder.Entity<GameSession>()
                .HasOne(gs => gs.opponentUser)
                .WithMany()
                .HasForeignKey(gs => gs.opponentUserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<GameActionLog>()
                .HasOne(log => log.session)
                .WithMany(gs => gs.actionLog)
                .HasForeignKey(log => log.sessionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
