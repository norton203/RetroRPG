using LiteDB;

namespace RetroRPG.Core.Data;

/// <summary>
/// Shared LiteDB instance to prevent file locking
/// </summary>
public class LiteDatabaseService : IDisposable
{
    private readonly LiteDatabase _db;
    private bool _disposed = false;

    public LiteDatabaseService(string databasePath)
    {
        _db = new LiteDatabase(databasePath);
    }

    public LiteDatabase Database => _db;

    public void Dispose()
    {
        if (!_disposed)
        {
            _db?.Dispose();
            _disposed = true;
        }
    }
}