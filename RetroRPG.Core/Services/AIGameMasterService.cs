using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using RetroRPG.Core.Models;

namespace RetroRPG.Core.Services;

/// <summary>
/// AI Game Master using local LLM (Ollama/LM Studio/llamafile)
/// Compatible with OpenAI-style API endpoints
/// </summary>
public class AIGameMasterService : IAIGameMasterService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiEndpoint;
    private readonly string _modelName;

    public AIGameMasterService(HttpClient httpClient, string apiEndpoint, string modelName = "llama3")
    {
        _httpClient = httpClient;
        _apiEndpoint = apiEndpoint;
        _modelName = modelName;
    }

    public async Task<string> GenerateNarrative(Character character, GameState gameState, string playerAction)
    {
        var systemPrompt = BuildGameMasterSystemPrompt(character);
        var userPrompt = BuildNarrativePrompt(character, gameState, playerAction);

        return await CallLLM(systemPrompt, userPrompt, gameState.ConversationHistory);
    }

    public async Task<string> GenerateCombatNarrative(Character character, Combat combat, string action)
    {
        var systemPrompt = BuildCombatSystemPrompt();
        var userPrompt = $@"
CHARACTER: {character.Name} (Level {character.Level} {character.Class})
HP: {character.HitPoints}/{character.MaxHitPoints}
ENEMY: {combat.EnemyName}
ENEMY HP: {combat.EnemyHitPoints}/{combat.EnemyMaxHitPoints}
Round: {combat.RoundNumber}

Player action: {action}

Describe the combat round in an exciting, retro RPG style (2-3 sentences).";

        return await CallLLM(systemPrompt, userPrompt);
    }

    public async Task<string> GenerateCharacterIntroduction(Character character)
    {
        var systemPrompt = "You are a classic D&D game master with a retro BBS/LORD game style.";
        var userPrompt = $@"Generate a brief character introduction for:
Name: {character.Name}
Class: {character.Class}
Level: {character.Level}

Make it dramatic and retro-styled (2-3 sentences).";

        return await CallLLM(systemPrompt, userPrompt);
    }

    public async Task<bool> IsAvailable()
    {
        try
        {
            var response = await _httpClient.GetAsync(_apiEndpoint.Replace("/chat/completions", "/models"));
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private async Task<string> CallLLM(string systemPrompt, string userPrompt, List<ConversationMessage>? history = null)
    {
        try
        {
            var messages = new List<object>
            {
                new { role = "system", content = systemPrompt }
            };

            // Add conversation history if provided
            if (history != null && history.Any())
            {
                foreach (var msg in history.TakeLast(10)) // Last 10 messages for context
                {
                    messages.Add(new { role = msg.Role, content = msg.Content });
                }
            }

            messages.Add(new { role = "user", content = userPrompt });

            var payload = new
            {
                model = _modelName,
                messages = messages,
                temperature = 0.8,
                max_tokens = 300,
                stream = false
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_apiEndpoint, content);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<LLMResponse>();
            return result?.Choices?.FirstOrDefault()?.Message?.Content ?? "The game master is silent...";
        }
        catch (Exception ex)
        {
            return $"[AI Error: {ex.Message}] The mists cloud your vision...";
        }
    }

    private string BuildGameMasterSystemPrompt(Character character)
    {
        return $@"You are the Game Master for a retro-style D&D adventure game inspired by Legend of the Red Dragon (LORD).

STYLE GUIDELINES:
- Keep responses concise (2-4 sentences)
- Use vivid, dramatic language
- Match the tone of classic BBS door games
- Don't use markdown or special formatting
- Present choices naturally in the narrative

CHARACTER CONTEXT:
Name: {character.Name}
Class: {character.Class}
Level: {character.Level}
Location: {character.CurrentLocation}

Generate immersive narrative responses to the player's actions. Include atmospheric descriptions, NPC dialogue when relevant, and natural consequences of actions.";
    }

    private string BuildCombatSystemPrompt()
    {
        return @"You are narrating combat in a retro D&D-style RPG. 
Keep descriptions exciting but concise (2-3 sentences).
Use classic fantasy combat language.
Don't use markdown formatting.";
    }

    private string BuildNarrativePrompt(Character character, GameState gameState, string playerAction)
    {
        return $@"Current scene: {gameState.CurrentScene}
Last narrative: {gameState.CurrentNarrative}

The player ({character.Name}) says: ""{playerAction}""

Generate the next part of the story based on their action.";
    }

    // Response model for parsing LLM API responses
    private class LLMResponse
    {
        public List<Choice>? Choices { get; set; }
    }

    private class Choice
    {
        public Message? Message { get; set; }
    }

    private class Message
    {
        public string? Content { get; set; }
    }
}