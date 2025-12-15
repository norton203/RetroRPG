using LiteDB;
using RetroRPG.Core.Models;

namespace RetroRPG.Core.Data;

/// <summary>
/// Repository for managing character data in LiteDB
/// </summary>
public class CharacterRepository
{
    private readonly ILiteCollection<Character> _characters;

    public CharacterRepository(LiteDatabaseService dbService)
    {
        _characters = dbService.Database.GetCollection<Character>("characters");
        _characters.EnsureIndex(x => x.Id);
        _characters.EnsureIndex(x => x.Name);
    }

    public Character? GetById(string id)
    {
        return _characters.FindById(id);
    }

    public Character? GetByName(string name)
    {
        return _characters.FindOne(x => x.Name == name);
    }

    public List<Character> GetAll()
    {
        return _characters.FindAll().ToList();
    }

    public void Save(Character character)
    {
        character.LastPlayedAt = DateTime.UtcNow;
        _characters.Upsert(character);
    }

    public bool Delete(string id)
    {
        return _characters.Delete(id);
    }
}