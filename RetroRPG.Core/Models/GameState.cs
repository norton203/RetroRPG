namespace RetroRPG.Core.Models;

/// <summary>
/// Represents the current game session state
/// </summary>
public class GameState
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string CharacterId { get; set; } = string.Empty;

    // Current narrative state
    public List<ConversationMessage> ConversationHistory { get; set; } = new();
    public string CurrentScene { get; set; } = string.Empty;
    public string CurrentNarrative { get; set; } = string.Empty;

    // Active combat
    public Combat? ActiveCombat { get; set; }

    // Temporary state
    public Dictionary<string, string> SessionFlags { get; set; } = new();

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}

public class ConversationMessage
{
    public string Role { get; set; } = string.Empty; // "user" or "assistant"
    public string Content { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class Combat
{
    public string EnemyName { get; set; } = string.Empty;
    public int EnemyHitPoints { get; set; }
    public int EnemyMaxHitPoints { get; set; }
    public int EnemyArmorClass { get; set; }
    public int EnemyDamage { get; set; }
    public int PlayerInitialHp { get; set; }
    public int RoundNumber { get; set; } = 1;
    public bool IsPlayerTurn { get; set; } = true;
}