using Microsoft.Extensions.Caching.Memory;

namespace HttpServer;

public class SessionManager
{
    private SessionManager()
    {
    }

    private static readonly Lazy<SessionManager> Lazy =
        new(() => new SessionManager());

    public static SessionManager Instance => Lazy.Value;

    private MemoryCache _cache = new(new MemoryCacheOptions());

    public Guid CreateSession(int accountId, string login, DateTime createDateTime)
    {
        var guid = Guid.NewGuid();
        var session = new Session(guid, accountId, login, createDateTime);
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            // Храним в кэше в течении этого времени, сбрасываем время при обращении.
            .SetSlidingExpiration(TimeSpan.FromSeconds(120));

        // Сохраняем данные в кэше.
        _cache.Set(guid, session, cacheEntryOptions);
        return guid;
    }


    public bool IsSessionExist(object key)
    {
        return _cache.TryGetValue(key, out var session);
    }

    public Session? GetSessionInfo(object key)
    {
        _cache.TryGetValue(key, out Session? session);
        return session;
    }
}