using RetroRPG.Core.Models;

namespace RetroRPG.Core.Services;

/// <summary>
/// Interface for AI Game Master service
/// </summary>
public interface IAIGameMasterService
{
    /// <summary>
    /// Generate narrative response based on player action
    /// </summary>
    Task<string> GenerateNarrative(Character character, GameState gameState, string playerAction);

    /// <summary>
    /// Generate combat description
    /// </summary>
    Task<string> GenerateCombatNarrative(Character character, Combat combat, string action);

    /// <summary>
    /// Generate character introduction
    /// </summary>
    Task<string> GenerateCharacterIntroduction(Character character);

    /// <summary>
    /// Check if AI service is available
    /// </summary>
    Task<bool> IsAvailable();
}