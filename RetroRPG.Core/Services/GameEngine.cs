using RetroRPG.Core.Data;
using RetroRPG.Core.Models;
using System.Text;

namespace RetroRPG.Core.Services;

/// <summary>
/// Core game engine managing game logic and state
/// </summary>
public class GameEngine
{
    private readonly CharacterRepository _characterRepo;
    private readonly GameStateRepository _gameStateRepo;
    private readonly IAIGameMasterService _aiService;

    public GameEngine(
        CharacterRepository characterRepo,
        GameStateRepository gameStateRepo,
        IAIGameMasterService aiService)
    {
        _characterRepo = characterRepo;
        _gameStateRepo = gameStateRepo;
        _aiService = aiService;
    }

    public Character CreateCharacter(string name, CharacterClass characterClass,
         int? strength = null, int? dexterity = null, int? constitution = null,
         int? intelligence = null, int? wisdom = null, int? charisma = null)
    {
        var character = new Character
        {
            Name = name,
            Class = characterClass,
            Level = 1,
            Experience = 0
        };

        // Apply custom stats if provided, otherwise use defaults
        if (strength.HasValue) character.Strength = strength.Value;
        if (dexterity.HasValue) character.Dexterity = dexterity.Value;
        if (constitution.HasValue) character.Constitution = constitution.Value;
        if (intelligence.HasValue) character.Intelligence = intelligence.Value;
        if (wisdom.HasValue) character.Wisdom = wisdom.Value;
        if (charisma.HasValue) character.Charisma = charisma.Value;

        // Set class-specific starting stats (adds bonuses to rolled stats)
        ApplyClassModifiers(character);
        CalculateDerivedStats(character);

        _characterRepo.Save(character);

        // Initialize game state
        _gameStateRepo.GetOrCreate(character.Id);

        return character;
    }

    public async Task<string> ProcessPlayerAction(string characterId, string action)
    {
        var character = _characterRepo.GetById(characterId);
        if (character == null)
            return "Character not found!";

        var gameState = _gameStateRepo.GetOrCreate(characterId);

        // Save player's action to conversation history
        _gameStateRepo.AddConversationMessage(characterId, "user", action);

        // Check if in combat
        if (gameState.ActiveCombat != null)
        {
            var combatResult = await ProcessCombatAction(character, gameState, action);
            return combatResult;
        }

        // Regular narrative action
        var narrative = await _aiService.GenerateNarrative(character, gameState, action);

        // Save AI response to conversation history
        _gameStateRepo.AddConversationMessage(characterId, "assistant", narrative);

        gameState.CurrentNarrative = narrative;
        _gameStateRepo.Save(gameState);

        // Update character play time
        _characterRepo.Save(character);

        return narrative;
    }

    private async Task<string> ProcessCombatAction(Character character, GameState gameState, string action)
    {
        if (gameState.ActiveCombat == null)
            return "No active combat!";

        var combat = gameState.ActiveCombat;
        var result = new StringBuilder();

        // Player attacks
        if (action.ToLower().Contains("attack") || action.ToLower().Contains("fight"))
        {
            var attackRoll = Random.Shared.Next(1, 21);
            var damage = CalculateDamage(character);

            if (attackRoll + GetModifier(character.Strength) >= combat.EnemyArmorClass)
            {
                combat.EnemyHitPoints -= damage;
                result.AppendLine($"You hit for {damage} damage!");
            }
            else
            {
                result.AppendLine("Your attack misses!");
            }
        }
        else if (action.ToLower().Contains("flee") || action.ToLower().Contains("run"))
        {
            result.AppendLine("You flee from combat!");
            gameState.ActiveCombat = null;
            _gameStateRepo.Save(gameState);
            return result.ToString();
        }

        // Check if enemy defeated
        if (combat.EnemyHitPoints <= 0)
        {
            var xpGain = combat.EnemyMaxHitPoints * 10;
            var goldGain = Random.Shared.Next(10, 50);

            character.Experience += xpGain;
            character.Gold += goldGain;

            result.AppendLine($"\nVictory! You gained {xpGain} XP and {goldGain} gold!");

            CheckLevelUp(character);

            gameState.ActiveCombat = null;
            _characterRepo.Save(character);
            _gameStateRepo.Save(gameState);

            return result.ToString();
        }

        // Enemy attacks
        var enemyAttackRoll = Random.Shared.Next(1, 21);
        if (enemyAttackRoll >= character.ArmorClass)
        {
            character.HitPoints -= combat.EnemyDamage;
            result.AppendLine($"\nThe {combat.EnemyName} hits you for {combat.EnemyDamage} damage!");

            if (character.HitPoints <= 0)
            {
                result.AppendLine("\nYou have been defeated!");
                character.HitPoints = character.MaxHitPoints / 2;
                character.Gold = Math.Max(0, character.Gold - 50);
                gameState.ActiveCombat = null;
            }
        }
        else
        {
            result.AppendLine($"\nThe {combat.EnemyName} misses!");
        }

        combat.RoundNumber++;
        _characterRepo.Save(character);
        _gameStateRepo.Save(gameState);

        return result.ToString();
    }

    public void StartCombat(string characterId, string enemyName, int enemyLevel)
    {
        var character = _characterRepo.GetById(characterId);
        if (character == null) return;

        var gameState = _gameStateRepo.GetOrCreate(characterId);

        gameState.ActiveCombat = new Combat
        {
            EnemyName = enemyName,
            EnemyHitPoints = 20 + (enemyLevel * 10),
            EnemyMaxHitPoints = 20 + (enemyLevel * 10),
            EnemyArmorClass = 10 + enemyLevel,
            EnemyDamage = 5 + (enemyLevel * 2),
            PlayerInitialHp = character.HitPoints,
            RoundNumber = 1
        };

        _gameStateRepo.Save(gameState);
    }

    private void ApplyClassModifiers(Character character)
    {
        switch (character.Class)
        {
            case CharacterClass.Warrior:
                character.Strength += 3;
                character.Constitution += 2;
                break;
            case CharacterClass.Mage:
                character.Intelligence += 3;
                character.Wisdom += 2;
                break;
            case CharacterClass.Rogue:
                character.Dexterity += 3;
                character.Charisma += 2;
                break;
            case CharacterClass.Cleric:
                character.Wisdom += 3;
                character.Constitution += 2;
                break;
            case CharacterClass.Ranger:
                character.Dexterity += 2;
                character.Wisdom += 2;
                break;
            case CharacterClass.Paladin:
                character.Strength += 2;
                character.Charisma += 2;
                break;
        }
    }

    private void CalculateDerivedStats(Character character)
    {
        character.MaxHitPoints = 20 + (character.Constitution - 10);
        character.HitPoints = character.MaxHitPoints;
        character.ArmorClass = 10 + GetModifier(character.Dexterity);
    }

    private void CheckLevelUp(Character character)
    {
        var xpNeeded = character.Level * 1000;
        if (character.Experience >= xpNeeded)
        {
            character.Level++;
            character.Experience -= xpNeeded;
            character.MaxHitPoints += 5 + GetModifier(character.Constitution);
            character.HitPoints = character.MaxHitPoints;
        }
    }

    private int GetModifier(int stat)
    {
        return (stat - 10) / 2;
    }

    private int CalculateDamage(Character character)
    {
        var baseDamage = 1 + Random.Shared.Next(1, 7); // 1d6
        var modifier = GetModifier(character.Strength);
        return Math.Max(1, baseDamage + modifier);
    }
}