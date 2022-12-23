using HttpServer.Interfaces;

namespace HttpServer.MyORM;

public class VisitedDAO: IDAO<Visited>
{
    private static string _connectionStr;
    private List<Visited> visits;

    public VisitedDAO(string connectionString)
    {
        _connectionStr = connectionString;
        visits = GetAll().ToList();
    }

    public Visited GetById(int id)
    {
        return new Database(_connectionStr).Select<Visited>(id);
    }

    public IEnumerable<Visited> GetAll()
    {
        return new Database(_connectionStr).Select<Visited>();
    }
    
    
    public int Insert(Visited entity)
    {
        return new Database(_connectionStr).Insert(entity);
    }

    public int Delete(int id)
    {
        return new Database(_connectionStr).Delete<Visited>(id);
    }

    public int Delete(Visited entity)
    {
        return new Database(_connectionStr).Delete<Visited>(entity);
    }

    public int Update(Visited entity)
    {
        return new Database(_connectionStr).Update<Visited>(entity);
    }
}