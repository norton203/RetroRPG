namespace RetroRPG.Core.Models;

public class Item
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ItemType Type { get; set; }
    public int Value { get; set; } = 0;

    // Equipment stats
    public int DamageBonus { get; set; } = 0;
    public int ArmorBonus { get; set; } = 0;
    public int StrengthBonus { get; set; } = 0;
    public int DexterityBonus { get; set; } = 0;
    public int ConstitutionBonus { get; set; } = 0;
    public int IntelligenceBonus { get; set; } = 0;
    public int WisdomBonus { get; set; } = 0;
    public int CharismaBonus { get; set; } = 0;

    // Consumable properties
    public bool IsConsumable { get; set; } = false;
    public int HealthRestore { get; set; } = 0;

    // Requirements
    public int RequiredLevel { get; set; } = 1;
    public List<CharacterClass> AllowedClasses { get; set; } = new();
}

public enum ItemType
{
    Weapon,
    Armor,
    Shield,
    Helmet,
    Boots,
    Ring,
    Amulet,
    Potion,
    Scroll,
    QuestItem,
    Misc
}