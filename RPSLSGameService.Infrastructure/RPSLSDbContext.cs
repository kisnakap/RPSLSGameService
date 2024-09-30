using Microsoft.EntityFrameworkCore;
using RPSLSGameService.Domain.Models;

namespace RPSLSGameService.Infrastructure
{
    public class RPSLSDbContext : DbContext
    {
        public RPSLSDbContext(DbContextOptions<RPSLSDbContext> options) : base(options) { }
        public DbSet<GameSession> GameSessions { get; set; }
        public DbSet<MatchResult> MatchResults { get; set; }
        public DbSet<Player> Players { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure GameSession entity
            modelBuilder.Entity<GameSession>()
                .HasKey(gs => gs.SessionId); // Set primary key for GameSession

            modelBuilder.Entity<GameSession>()
                .Property(gs => gs.CreatedAt)
                .IsRequired(); // Ensure CreatedAt is always set

            // Configure Players entity with relationship to GameSession
            modelBuilder.Entity<Player>()
                .HasKey(p => new { p.Name, p.GameSessionId }); // Define composite key using Name and GameSessionId

            modelBuilder.Entity<Player>()
                .Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Player>()
                .Property(p => p.Choice)
                .IsRequired(false); // Choice can be null until made

            // Relationships
            modelBuilder.Entity<Player>()
                .HasOne<GameSession>()
                .WithMany(gs => gs.Players)
                .HasForeignKey(p => p.GameSessionId)
                .OnDelete(DeleteBehavior.Cascade) // Cascade delete if the game session is deleted
                .IsRequired(); 

            // Configure MatchResult entity
            modelBuilder.Entity<MatchResult>()
                .HasKey(mr => mr.Id); // Set primary key for MatchResult

            modelBuilder.Entity<MatchResult>()
                .Property(mr => mr.WinnerName)
                .IsRequired()
                .HasMaxLength(100); // Winner's name length restriction

            modelBuilder.Entity<MatchResult>()
                .Property(mr => mr.ResultDate);

            modelBuilder.Entity<MatchResult>()
                .HasOne<GameSession>()
                .WithMany(gs => gs.MatchResults)
                .HasForeignKey(mr => mr.SessionId)
                .OnDelete(DeleteBehavior.Cascade); // Cascade delete if the game session is deleted
        }
    }
}
