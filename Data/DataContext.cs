using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


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
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserCharacter>()
                .HasKey(uc => new { uc.userId, uc.characterId });

            modelBuilder.Entity<UserCharacter>()
                .HasOne(uc => uc.user)
                .WithMany(u => u.userCharacters)
                .HasForeignKey(uc => uc.userId);

            modelBuilder.Entity<UserCharacter>()
                .HasOne(uc => uc.character)
                .WithMany(c => c.userCharacters)
                .HasForeignKey(uc => uc.characterId);
            modelBuilder.Entity<Characters>()
                .HasIndex(c => c.name)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.userName)
                .IsUnique();
            base.OnModelCreating(modelBuilder);
        }
    }
}