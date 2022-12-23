using HttpServer.Interfaces;

namespace HttpServer.MyORM;

public class EventDAO: IDAO<Event>
{
    private static string _connectionStr;
    private List<Event> events;

    public EventDAO(string connectionString)
    {
        _connectionStr = connectionString;
        events = GetAll().ToList();
    }

    public Event GetById(int id)
    {
        return new Database(_connectionStr).Select<Event>(id);
    }

    public IEnumerable<Event> GetAll()
    {
        return new Database(_connectionStr).Select<Event>();
    }
    
    
    public int Insert(Event entity)
    {
        return new Database(_connectionStr).Insert(entity);
    }

    public int Delete(int id)
    {
        return new Database(_connectionStr).Delete<Event>(id);
    }

    public int Delete(Event entity)
    {
        return new Database(_connectionStr).Delete<Event>(entity);
    }

    public int Update(Event entity)
    {
        return new Database(_connectionStr).Update<Event>(entity);
    }
}