using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RetroRPG.Api.Data;
using RetroRPG.Shared.DTOs;

namespace RetroRPG.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CharacterSyncController : ControllerBase
{
    private readonly GameDbContext _context;
    private readonly ILogger<CharacterSyncController> _logger;

    public CharacterSyncController(GameDbContext context, ILogger<CharacterSyncController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Upload character data to cloud backup
    /// </summary>
    [HttpPost("backup")]
    public async Task<IActionResult> BackupCharacter([FromBody] CharacterBackupDto backup)
    {
        try
        {
            var existing = await _context.CharacterBackups
                .FirstOrDefaultAsync(c => c.UserId == backup.UserId && c.CharacterName == backup.CharacterName);

            if (existing != null)
            {
                existing.CharacterData = backup.CharacterData;
                existing.Level = backup.Level;
                existing.LastBackupDate = DateTime.UtcNow;
                _context.CharacterBackups.Update(existing);
            }
            else
            {
                backup.Id = Guid.NewGuid().ToString();
                backup.LastBackupDate = DateTime.UtcNow;
                _context.CharacterBackups.Add(backup);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Character backed up successfully", backupId = existing?.Id ?? backup.Id });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error backing up character");
            return StatusCode(500, "Error backing up character");
        }
    }

    /// <summary>
    /// Retrieve character backup from cloud
    /// </summary>
    [HttpGet("restore/{userId}/{characterName}")]
    public async Task<IActionResult> RestoreCharacter(string userId, string characterName)
    {
        try
        {
            var backup = await _context.CharacterBackups
                .FirstOrDefaultAsync(c => c.UserId == userId && c.CharacterName == characterName);

            if (backup == null)
                return NotFound("Character backup not found");

            return Ok(backup);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring character");
            return StatusCode(500, "Error restoring character");
        }
    }

    /// <summary>
    /// Get all character backups for a user
    /// </summary>
    [HttpGet("list/{userId}")]
    public async Task<IActionResult> ListCharacters(string userId)
    {
        try
        {
            var backups = await _context.CharacterBackups
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.LastBackupDate)
                .Select(c => new
                {
                    c.Id,
                    c.CharacterName,
                    c.Level,
                    c.LastBackupDate
                })
                .ToListAsync();

            return Ok(backups);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing characters");
            return StatusCode(500, "Error listing characters");
        }
    }
}