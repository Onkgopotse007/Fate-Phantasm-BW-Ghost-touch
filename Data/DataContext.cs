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
