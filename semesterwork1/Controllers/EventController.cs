using System.Text;
using HttpServer.Attributes;
using HttpServer.MyORM;

namespace HttpServer.Controllers;

[HttpController("events")]
public class EventController : Controller
{
    private static string _connectionStr = "Server=localhost;Database=museum;Port=5432;SSLMode=Prefer";

    // static byte[] dbSettingsFile = File.ReadAllBytes($"{DBSettings.SettingsPath}");
    // private DBSettings _dbSettings = JsonSerializer.Deserialize<DBSettings>(dbSettingsFile);

    // private static string Host = "localhost";
    // private static string User = "root";
    // private static string DBname = "postgres";
    // private static string Password = "1234";
    // private static string Port = "5432";

    private static EventDAO _eventDao = new EventDAO(_connectionStr);

    [HttpGET("events")]
    public string events(string path)
    {
        var events = _eventDao.GetAll();
        if (events is null) return "Events not found";
        return CreateHtmlCode(path,
            new { Events = events });
    }


    // [HttpGET("accounts/getAccounts")]
    // public string getAccounts()
    // {
    //     var accounts = _accountDao.GetAll();
    //     if (accounts is null) return "Accounts not found";
    //     var accountList = new StringBuilder();
    //     foreach (var user in accounts)
    //     {
    //         accountList.Append($"User with id = {user.Id}, login =  {user.Email}, password = {user.Password}   ");
    //     }
    //
    //     return accountList.ToString();
    // }
    //
    //
    // [HttpGET("accounts/getAccountById")]
    // public string getAccountById(int userId)
    // {
    //     var account = _accountDao.GetById(userId);
    //
    //     if (account is null) return "User not found";
    //     return string.Format("User with id = {0}, login =  {1}, password = {2}",
    //         account.Id.ToString(),
    //         account.Email,
    //         account.Password);
    // }
    //
    // [HttpGET("accounts/getAccountInfo")]
    // public string getAccountInfo(int userId)
    // {
    //     var account = _accountDao.GetById(userId);
    //
    //     if (account is null) return "User not found";
    //     return string.Format("User with id = {0}, login =  {1}, password = {2}",
    //         account.Id.ToString(),
    //         account.Email,
    //         account.Password);
    // }
    //
    // [HttpPOST("accounts/saveAccount")]
    // public string saveAccount(string email, string password)
    // {
    //     var res = _accountDao.Insert(new Account() { Email = email, Password = password });
    //     if (res == 0)
    //     {
    //         Console.WriteLine("Error while saving data");
    //         return "Error while saving data";
    //     }
    //
    //     Console.WriteLine("Data saved successfully");
    //     return "Data saved successfully";
    // }
    //
    // [HttpPOST("accounts/login")]
    // public (bool, int?) login(string login, string password, string remember)
    // {
    //     return _accountDao.VerifyLoginAndPassword(login, password);
    // }
}