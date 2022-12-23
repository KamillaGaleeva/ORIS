using HttpServer.Interfaces;

namespace HttpServer.MyORM;

public class AccountDAO: IDAO<Account>
{
    private static string _connectionStr;
   // private List<Account> accounts;
    
    private List<Account> Accounts => GetAll().ToList();

    public AccountDAO(string connectionString)
    {
        _connectionStr = connectionString;
       // accounts = GetAll().ToList();
    }

    public Account GetById(int id)
    {
        return new Database(_connectionStr).Select<Account>(id);
    }

    public Account GetAccount(string login, string password)
    {
        var account = Accounts.Find(a => a.Email == login && a.Password == password);
        return account;
    }
    
    public IEnumerable<Account> GetAll()
    {
        return new Database(_connectionStr).Select<Account>();
    }
    
    
    public int Insert(Account entity)
    {
        return new Database(_connectionStr).Insert(entity);
    }

    public int Delete(int id)
    {
        return new Database(_connectionStr).Delete<Account>(id);
    }

    public int Delete(Account entity)
    {
        return new Database(_connectionStr).Delete<Account>(entity);
    }

    public int Update(Account entity)
    {
        return new Database(_connectionStr).Update<Account>(entity);
    }

    public (bool, int?) VerifyLoginAndPassword(string login, string password)
    {
        var account = Accounts.Find(a => a.Email == login && a.Password == password);
        if (account is not null) return (true, account.Id);
        return (false, null);
    }
}