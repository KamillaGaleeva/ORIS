using System;
using System.Net;
using System.IO;
using System.Text.Json;
using System.Text;
using System.Reflection;
using System.Linq;
using work_from_1._10._22.Attributes;

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
        public string ClassUri;
        public HttpController(string classUri){ClassUri = classUri;}
        public HttpController() { ClassUri = null; }
    }
}

namespace work_from_1._10._22
{
    public class ServerSettings
    {
        public string Path { get; set; }
        public int Port { get; set; }
    }
    public enum ServerStatus{Started,Stopped}
    internal class Program
    {
        static void Main(string[] args)
        {
            var settingsPath = @"./settings.json";
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
    class HttpServer
    {
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
        private readonly HttpListener httpListener;
        public string SettPath;
        private ServerSettings serverSett;
        public ServerStatus Status { get; private set; } = ServerStatus.Stopped;
        public HttpServer(string settingsPath)
        {
            SettPath = settingsPath;
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
                Console.WriteLine("Файл настроек не найден");
                return;
            }
            serverSett = JsonSerializer.Deserialize<ServerSettings>(File.ReadAllBytes(SettPath));
            httpListener.Prefixes.Clear();
            httpListener.Prefixes.Add("http://localhost:" + serverSett.Port + "/");
            Console.WriteLine("Запуск сервера");
            httpListener.Start();
            Console.WriteLine("Сервер запущен");
            Status = ServerStatus.Started;
            Listening();
        }
        private async void Listening()
        {
            while (httpListener.IsListening)
            {
                var _httpContext = await httpListener.GetContextAsync();
                if (Handler(_httpContext)) return;
                StaticFiles(_httpContext.Request, _httpContext.Response);
            }
        }
        public void Stop()
        {
            if (Status == ServerStatus.Stopped)
                return;
            Console.WriteLine("Остановка сервера");
            httpListener.Stop();
            Status = ServerStatus.Stopped;
            Console.WriteLine("Сервер остановлен");
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
                var err = $"Каталог '{serverSett.Path}'  не найден";
                buffer = Encoding.UTF8.GetBytes(err);
            }
            Stream output = response.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
        }        
        private bool Handler(HttpListenerContext httpContext)
        {
            HttpListenerRequest req = httpContext.Request;
            HttpListenerResponse res = httpContext.Response;
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
            res.ContentType = "json";
            byte[] buffer = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(ret));
            res.ContentLength64 = buffer.Length;
            Stream output = res.OutputStream;
            output.Write(buffer, 0, buffer.Length);
            output.Close();
            return true;
        }
    }    
}