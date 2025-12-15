using LiteDB;
using RetroRPG.Core.Models;

namespace RetroRPG.Core.Data;

/// <summary>
/// Repository for managing game state in LiteDB
/// </summary>
public class GameStateRepository
{
    private readonly ILiteCollection<GameState> _gameStates;

    public GameStateRepository(LiteDatabaseService dbService)
    {
        _gameStates = dbService.Database.GetCollection<GameState>("gameStates");
        _gameStates.EnsureIndex(x => x.CharacterId);
    }

    public GameState? GetByCharacterId(string characterId)
    {
        return _gameStates.FindOne(x => x.CharacterId == characterId);
    }

    public GameState GetOrCreate(string characterId)
    {
        var state = GetByCharacterId(characterId);
        if (state == null)
        {
            state = new GameState
            {
                CharacterId = characterId,
                CurrentScene = "Town Square",
                CurrentNarrative = "You find yourself standing in the bustling town square..."
            };
            Save(state);
        }
        return state;
    }

    public void Save(GameState gameState)
    {
        gameState.LastUpdated = DateTime.UtcNow;
        _gameStates.Upsert(gameState);
    }

    public bool Delete(string characterId)
    {
        var state = GetByCharacterId(characterId);
        if (state != null)
        {
            return _gameStates.Delete(state.Id);
        }
        return false;
    }

    public void AddConversationMessage(string characterId, string role, string content)
    {
        var state = GetOrCreate(characterId);
        state.ConversationHistory.Add(new ConversationMessage
        {
            Role = role,
            Content = content,
            Timestamp = DateTime.UtcNow
        });

        // Keep only last 50 messages to prevent memory bloat
        if (state.ConversationHistory.Count > 50)
        {
            state.ConversationHistory = state.ConversationHistory
                .OrderByDescending(m => m.Timestamp)
                .Take(50)
                .OrderBy(m => m.Timestamp)
                .ToList();
        }

        Save(state);
    }
}