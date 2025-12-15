using Microsoft.EntityFrameworkCore;
using RetroRPG.Shared.DTOs;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace RetroRPG.Api.Data;

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
    {
    }

    public DbSet<CharacterBackupDto> CharacterBackups { get; set; }
    public DbSet<LeaderboardEntryDto> Leaderboard { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CharacterBackupDto>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CharacterData).IsRequired();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.CharacterName);
        });

        modelBuilder.Entity<LeaderboardEntryDto>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Level).IsDescending();
            entity.HasIndex(e => e.Experience).IsDescending();
        });
    }
}