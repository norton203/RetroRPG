using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetroRPG.Api.Data;
using RetroRPG.Shared.DTOs;

namespace RetroRPG.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LeaderboardController : ControllerBase
{
    private readonly GameDbContext _context;
    private readonly ILogger<LeaderboardController> _logger;

    public LeaderboardController(GameDbContext context, ILogger<LeaderboardController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get top players by level
    /// </summary>
    [HttpGet("top/{count:int}")]
    public async Task<IActionResult> GetTopPlayers(int count = 10)
    {
        try
        {
            var topPlayers = await _context.Leaderboard
                .OrderByDescending(l => l.Level)
                .ThenByDescending(l => l.Experience)
                .Take(Math.Min(count, 100))
                .ToListAsync();

            return Ok(topPlayers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching leaderboard");
            return StatusCode(500, "Error fetching leaderboard");
        }
    }

    /// <summary>
    /// Update leaderboard entry for a character
    /// </summary>
    [HttpPost("update")]
    public async Task<IActionResult> UpdateLeaderboard([FromBody] LeaderboardEntryDto entry)
    {
        try
        {
            var existing = await _context.Leaderboard
                .FirstOrDefaultAsync(l => l.CharacterId == entry.CharacterId);

            if (existing != null)
            {
                existing.CharacterName = entry.CharacterName;
                existing.Level = entry.Level;
                existing.Experience = entry.Experience;
                existing.CharacterClass = entry.CharacterClass;
                existing.LastUpdated = DateTime.UtcNow;
                _context.Leaderboard.Update(existing);
            }
            else
            {
                entry.Id = Guid.NewGuid().ToString();
                entry.LastUpdated = DateTime.UtcNow;
                _context.Leaderboard.Add(entry);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Leaderboard updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating leaderboard");
            return StatusCode(500, "Error updating leaderboard");
        }
    }
}