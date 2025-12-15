namespace RetroRPG.Shared.DTOs;

/// <summary>
/// DTO for character cloud backup
/// </summary>
public class CharacterBackupDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string CharacterName { get; set; } = string.Empty;
    public int Level { get; set; }
    public string CharacterData { get; set; } = string.Empty; // JSON serialized character
    public DateTime LastBackupDate { get; set; }
}