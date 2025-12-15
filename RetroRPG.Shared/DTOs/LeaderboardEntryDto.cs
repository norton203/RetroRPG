namespace RetroRPG.Shared.DTOs;

/// <summary>
/// DTO for leaderboard entries
/// </summary>
public class LeaderboardEntryDto
{
    public string Id { get; set; } = string.Empty;
    public string CharacterId { get; set; } = string.Empty;
    public string CharacterName { get; set; } = string.Empty;
    public string CharacterClass { get; set; } = string.Empty;
    public int Level { get; set; }
    public int Experience { get; set; }
    public DateTime LastUpdated { get; set; }
}