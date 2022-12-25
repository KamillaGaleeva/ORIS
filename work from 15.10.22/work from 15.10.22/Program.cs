using System;
using System.Net;
using System.IO;
using System.Text.Json;
using System.Text;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using work_from_1._10._22.Attributes;
using System.Data.SqlClient;
using HttpServer.Controllers;

namespace HttpServer.ORM
{
    public interface IAccountDao
    {
        public IEnumerable<Account> GetAll();
        public Account GetById(int id);
        public void Insert(string login, string password);
        public void Remove(int? id);
        public void Update(string field, string value, int? id);
    }
    public class AccountDao : IAccountDao
    {
        private const string ConnectionString =@"Data Source=DESKTOP-MFCEQVI\SQLEXPRESS;Initial Catalog={DbName};Integrated Security=True";
        private const string TableName = "[dbo].[Accounts]";
        private const string DbName = "SteamDB";
        public IEnumerable<Account> GetAll()
        {
            var query = $"select * from {TableName}";
            using var connection = new SqlConnection(ConnectionString);
            connection.Open();
            var cmd = new SqlCommand(query, connection);
            using var reader = cmd.ExecuteReader();
            if (!reader.HasRows) yield break;
            while (reader.Read())
                yield return new Account(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2));
        }
        public Account GetById(int id)
        {
            var query = $"select * from {TableName} where Id={id}";
            using var connection = new SqlConnection(ConnectionString);
            connection.Open();
            var cmd = new SqlCommand(query, connection);
            using var reader = cmd.ExecuteReader();
            if (!reader.HasRows || !reader.Read()) return null;
            return new Account(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2));
        }
        public void Insert(string login, string password)
        {
            var query = $"вставить в {TableName} значение ('{login}', '{password}')";
            using var connection = new SqlConnection(ConnectionString);
            connection.Open();
            var cmd = new SqlCommand(query, connection);
            cmd.ExecuteNonQuery();
        }
        public void Remove(int? id = null)
        {
            var query = $"удалить из {TableName}";
            query += id is not null ? $" Id={id}" : "";
            using var connection = new SqlConnection(ConnectionString);
            connection.Open();
            var cmd = new SqlCommand(query);
            cmd.ExecuteNonQuery();
        }
        public void Update(string field, string value, int? id = null)
        {
            var query = $"Обновить {TableName}  {field}={value}";
            query += id is not null ? $"where Id={id}" : "";
            using var connection = new SqlConnection(ConnectionString);
            connection.Open();
            var cmd = new SqlCommand(query);
            cmd.ExecuteNonQuery();
        }
    }
    public interface IAccountRepository
    {
        public IEnumerable<Account> GetAll();
        public Account GetById(int id);
        public void Insert(Account account);
        public void Remove(Account account);
        public void Update(Account old, Account @new);
    }
}
namespace HttpServer.Controllers
{
    [HttpController("accounts")]
    internal class Accounts
    {
        [HttpController("accounts")]
        public Account GetAccount(int id)
        {
            List<Account> accounts = new List<Account>();
            accounts.Add(new Account() { Id = 1, Login = "Ivan", Password = "123" });
            return accounts.FirstOrDefault(t => t.Id == id);
        }
        [HttpGET]
        public List<Account> GetAccounts()
        {
            List<Account> accounts = new List<Account>();
            string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SteamDB;Integrated Security=True";
            string sqlExpression = "SELECT * FROM  [dbo].[Table]";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                SqlDataReader reader = command.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        accounts.Add(new Account
                        {
                            Id = reader.GetInt32(0),
                            Login = reader.GetString(1),
                            Password = reader.GetString(2)
                        });
                    }
                }
                reader.Close();
            }
            return accounts;
        }       
        [HttpPOST]
        public void SaveAccount(string login, string password)
        {
            string connectionString = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SteamDB;Integrated Security=True";
            string sqlExpression = $"INSERT INTO Accounts (Login, Password) VALUES ('{login}', '{password}')";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand command = new SqlCommand(sqlExpression, connection);
                command.ExecuteNonQuery();
            }
        }
    }
    public class Account
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public Account()
        {
        }
        public Account(int id, string login, string password)
        {
            Id = id;
            Login = login;
            Password = password;
        }
    }}
namespace work_from_1._10._22.Attributes
{
    internal class HttpPOST : Attribute
    {
        public string MethodUri;
        public HttpPOST(string methodUri){MethodUri = methodUri;}
        public HttpPOST() { MethodUri = null; }
    }
    internal class HttpGET : Attribute
    {
        public string MethodUri;
        public HttpGET(string methodUri){MethodUri = methodUri;}
        public HttpGET() { MethodUri = null; }
    }
    internal class HttpController : Attribute
    {
        public string ControllerName;
        public HttpController(string controllerName) {ControllerName = controllerName;}
    }
}
namespace work_from_15._10._22
{
    class HttpServer
    {
        private readonly HttpListener httpListener;
        public string SettPath;
        private ServerSettings serverSett;
        public ServerStatus Status { get; private set; } = ServerStatus.Stopped;
        public HttpServer(string settPath)
        {
            SettPath = settPath;
            httpListener = new HttpListener();
        }
        public void Start()
        {
            if (Status == ServerStatus.Started)
            {
                Console.WriteLine("Сервер уже запущен");
                return;
            }
            if (!File.Exists(SettPath))
            {
                Console.WriteLine("файл не найден");
                return;
            }
            serverSett = JsonSerializer.Deserialize<ServerSettings>(File.ReadAllBytes(SettPath));
            httpListener.Prefixes.Clear();
            httpListener.Prefixes.Add("http://localhost:" + serverSett.Port + "/");
            Console.WriteLine("запуск сервера");
            httpListener.Start();
            Console.WriteLine("запущен");
            Status = ServerStatus.Started;
            Listening();
        }
        public void Stop()
        {
            if (Status == ServerStatus.Stopped)
                return;
            Console.WriteLine("остановка сервера");
            httpListener.Stop();
            Status = ServerStatus.Stopped;
            Console.WriteLine("окончен");
        }
        private async void Listening()
        {
            while (httpListener.IsListening)
            {
                var httpContext = await httpListener.GetContextAsync();
                if (Handler(httpContext)) return;
                StaticFiles(httpContext.Request, httpContext.Response);
            }
        }
        private void StaticFiles(HttpListenerRequest request, HttpListenerResponse response)
        {
            byte[] buffer;

            if (Directory.Exists(serverSett.Path))
            {
                buffer = getFile(request.RawUrl.Replace("%20", " "));
                if (buffer == null)
                {
                    response.Headers.Set("Content-Type", "text/plain");
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    string err = "404- not found";
                    buffer = Encoding.UTF8.GetBytes(err);
                }
            }
            else
            {
                var err = $"каталог '{serverSett.Path}'  не найден";
                buffer = Encoding.UTF8.GetBytes(err);
            }
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }
        private byte[] getFile(string rawUrl)
        {
            byte[] buffer = null;
            var filePath = serverSett.Path + rawUrl;
            if (Directory.Exists(filePath))
            {//каталог
                filePath = filePath + "/google.html";
                if (File.Exists(filePath))
                buffer = File.ReadAllBytes(filePath);
            }
            else if (File.Exists(filePath))
             //файл
             buffer = File.ReadAllBytes(filePath);
            return buffer;
        }

        private bool Handler(HttpListenerContext httpContext)
        {
            HttpListenerRequest request = httpContext.Request;
            HttpListenerResponse response = httpContext.Response;
            if (httpContext.Request.Url.Segments.Length > 1) return false;
            string controllerName = httpContext.Request.Url.Segments[1].Replace("/", "");
            string[] strParams = httpContext.Request.Url
                .Segments
                .Skip(2)
                .Select(s => s.Replace("/", ""))
                .ToArray();
            var assembly = Assembly.GetExecutingAssembly();
            var controller = assembly.GetTypes().Where(t => Attribute.IsDefined(t, typeof(HttpController)))
                .FirstOrDefault(c => c.Name.ToLower() == controllerName.ToLower());
            if (controller == null) return false;
            var test = typeof(HttpController).Name;
            var method = controller.GetMethods().Where(t => t.GetCustomAttributes(true).Any(attr => attr.GetType().Name == $"http{httpContext.Request.HttpMethod}"))
                .FirstOrDefault();
            if (method == null) return false;
            Object[] queryParms = method.GetParameters()
                .Select((p, i) => Convert.ChangeType(strParams[i], p.ParameterType))
                .ToArray();
            var ret = method.Invoke(Activator.CreateInstance(controller), queryParms);
            response.ContentType = "json";
            byte[] buffer = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(ret));
            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
            return true;
        }
    }
    internal class HttpController
    {
    }
    public class ServerSettings
    {
        public string Path { get; set; }
        public int Port { get; set; }
    }
    public enum ServerStatus
    {
        Started,
        Stopped
    }
    internal class Program
    {
        static void Main(string[] args)
        {
            var settingsPath = @"settings.json";
            var server = new HttpServer(settingsPath);
            while (true)
            Input(Console.ReadLine(), server);
        }
        static void Input(string input, HttpServer server)
        {
            switch (input)
            {
                case "start":
                    server.Start();
                    break;
                case "stop":
                    server.Stop();
                    break;
                case "restart":
                    server.Stop();
                    server.Start();
                    break;
                default:
                    Console.WriteLine("Нет такой команды");
                    break;
            }
        }
    }
}