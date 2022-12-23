using HttpServer.Interfaces;

namespace HttpServer.MyORM;

public class RegisteredDAO:IDAO<Registered>
{
    private static string _connectionStr;
    private List<Registered> registrations;

    public RegisteredDAO(string connectionString)
    {
        _connectionStr = connectionString;
        registrations = GetAll().ToList();
    }

    public Registered GetById(int id)
    {
        return new Database(_connectionStr).Select<Registered>(id);
    }

    public IEnumerable<Registered> GetAll()
    {
        return new Database(_connectionStr).Select<Registered>();
    }
    
    
    public int Insert(Registered entity)
    {
        return new Database(_connectionStr).Insert(entity);
    }

    public int Delete(int id)
    {
        return new Database(_connectionStr).Delete<Registered>(id);
    }

    public int Delete(Registered entity)
    {
        return new Database(_connectionStr).Delete<Registered>(entity);
    }

    public int Update(Registered entity)
    {
        return new Database(_connectionStr).Update<Registered>(entity);
    }
}