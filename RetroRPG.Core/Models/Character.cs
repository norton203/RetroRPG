namespace RetroRPG.Core.Models;

/// <summary>
/// Player character with D&D-style attributes
/// </summary>
public class Character
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public CharacterClass Class { get; set; }
    public int Level { get; set; } = 1;
    public int Experience { get; set; } = 0;

    // Core Stats (D&D style)
    public int Strength { get; set; } = 10;
    public int Dexterity { get; set; } = 10;
    public int Constitution { get; set; } = 10;
    public int Intelligence { get; set; } = 10;
    public int Wisdom { get; set; } = 10;
    public int Charisma { get; set; } = 10;

    // Combat Stats
    public int HitPoints { get; set; } = 20;
    public int MaxHitPoints { get; set; } = 20;
    public int ArmorClass { get; set; } = 10;

    // Currency
    public int Gold { get; set; } = 100;

    // Equipment
    public List<Item> Inventory { get; set; } = new();
    public Equipment EquippedItems { get; set; } = new();

    // Progress
    public string CurrentLocation { get; set; } = "Town Square";
    public List<string> CompletedQuests { get; set; } = new();
    public List<string> ActiveQuests { get; set; } = new();

    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastPlayedAt { get; set; } = DateTime.UtcNow;
    public int TotalPlayTimeMinutes { get; set; } = 0;

    // Cloud sync
    public DateTime? LastSyncedAt { get; set; }
    public bool IsSynced { get; set; } = false;
}

public enum CharacterClass
{
    Warrior,
    Mage,
    Rogue,
    Cleric,
    Ranger,
    Paladin
}

public class Equipment
{
    public Item? Weapon { get; set; }
    public Item? Armor { get; set; }
    public Item? Shield { get; set; }
    public Item? Helmet { get; set; }
    public Item? Boots { get; set; }
    public Item? Ring { get; set; }
    public Item? Amulet { get; set; }
}