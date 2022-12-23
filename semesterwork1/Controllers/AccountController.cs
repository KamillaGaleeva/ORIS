using System.Text;
using HttpServer.Attributes;
using HttpServer.MyORM;

namespace HttpServer.Controllers;

[HttpController("accounts")]
internal class AccountsController: Controller
{
    private static string _connectionStr = "Server=localhost;Database=museum;Port=5432;SSLMode=Prefer";

    // static byte[] dbSettingsFile = File.ReadAllBytes($"{DBSettings.SettingsPath}");
    // private DBSettings _dbSettings = JsonSerializer.Deserialize<DBSettings>(dbSettingsFile);

    // private static string Host = "localhost";
    // private static string User = "root";
    // private static string DBname = "postgres";
    // private static string Password = "1234";
    // private static string Port = "5432";

    private static AccountDAO _accountDao = new AccountDAO(_connectionStr);

    [HttpGET("accounts")]
    public string accounts(string path)
    {
        path = "./site/login.html";
        var accounts = _accountDao.GetAll();
        if (accounts is null) return "Accounts not found";
        return CreateHtmlCode(path, null);
    }
    
    [HttpGET("accounts/getAccounts")]
    public string getAccounts()
    {
        var accounts = _accountDao.GetAll();
        if (accounts is null) return "Accounts not found";
        var accountList = new StringBuilder();
        foreach (var user in accounts)
        {
            accountList.Append($"User with id = {user.Id}, login =  {user.Email}, password = {user.Password}   ");
        }

        return accountList.ToString();
    }


    [HttpGET("accounts/getAccountById")]
    public string getAccountById(int userId)
    {
        var account = _accountDao.GetById(userId);

        if (account is null) return "User not found";
        return string.Format("User with id = {0}, login =  {1}, password = {2}",
            account.Id.ToString(),
            account.Email,
            account.Password);
    }

    [HttpGET("accounts/getAccountInfo")]
    public string getAccountInfo(int userId)
    {
        var account = _accountDao.GetById(userId);

        if (account is null) return "User not found";
        return string.Format("User with id = {0}, login =  {1}, password = {2}",
            account.Id.ToString(),
            account.Email,
            account.Password);
    }

    [HttpPOST("accounts/saveAccount")]
    public string saveAccount(string email, string password, string name, string surname, string gender)
    {
        switch (gender)
        {
            case "female":
                gender = "Женищна";
                break;
            case "male":
                gender = "Мужчина";
                break;
            default:
                gender = "Небинарная личность";
                break;
        }
        
        var res = _accountDao.Insert(new Account() { Email = email, Password = password, Name = name, Surname = surname, Gender = gender});
        if (res == 0)
        {
            Console.WriteLine("Error while saving data");
            return "Error while saving data";
        }

        Console.WriteLine("Data saved successfully");
        var path = "./site/profil.html";
        var account = _accountDao.GetAccount(email, password);
        return CreateHtmlCode(path, new{Account = account, Show=true});
    }
    
    [HttpPOST("accounts/updateAccount")]
    public string updateAccount(string id, string email, string password, string name, string surname, string gender)
    {
        var intId = int.Parse(id);
        switch (gender)
        {
            case "female":
                gender = "Женщина";
                break;
            case "male":
                gender = "Мужчина";
                break;
            default:
                gender = "Небинарная личность";
                break;
        }

        var account = new Account()
            { Id = intId, Email = email, Password = password, Name = name, Surname = surname, Gender = gender };
        var res = _accountDao.Update(account);
        if (res == 0)
        {
            Console.WriteLine("Error while saving data");
            return "Error while saving data";
        }
        var path = "./site/profil.html";
        return CreateHtmlCode(path, new{Account = account, Show = true});
    }
    
    [HttpGET("accounts/editAccount")]
    public string editAccount(string path, int id)
    {
        path = "./site/profil.html";
        var account = _accountDao.GetById(id);
        return CreateHtmlCode(path, new{Account = account, Show = false});
    }
   
    // [HttpGET("accounts/getCollection")]
    // public string  getCollection(string path, int id)
    // {
    //     path = "./site/profil.html";
    //     var account = _accountDao.GetById(id);
    //     return CreateHtmlCode(path, new{Account = account, Show = false});
    // }  
        
    [HttpPOST("accounts/login")]
    public (bool, int?) login(string login, string password, string remember)
    {
        return _accountDao.VerifyLoginAndPassword(login, password);
    }
}