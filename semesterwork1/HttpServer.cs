using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using HttpServer.Attributes;

namespace HttpServer
{
    public class HttpServer
    {
        private readonly HttpListener _httpListener;
        private ServerSettings _serverSettings;
        private DBSettings _dbSettings;

        public HttpServer()
        {
            _httpListener = new HttpListener();
            _dbSettings = new DBSettings();
        }

        public void Start()
        {
            if (!File.Exists(ServerSettings.SettingsPath))
            {
                Console.WriteLine("Не удалось найти файл настроек.");
                Stop();
            }
            else
            {
                if (_httpListener.IsListening) Console.WriteLine("Сервер уже запущен.");
                else
                {
                    var settingsFile = File.ReadAllBytes($"{ServerSettings.SettingsPath}");
                    _serverSettings = JsonSerializer.Deserialize<ServerSettings>(settingsFile);

                    var dbSettingsFile = File.ReadAllBytes($"{DBSettings.SettingsPath}");
                    _dbSettings = JsonSerializer.Deserialize<DBSettings>(dbSettingsFile);

                    _httpListener.Prefixes.Clear();

                    _httpListener.Prefixes.Add($"http://localhost:{_serverSettings.Port}/");

                    Console.WriteLine("Ожидание подключений...");
                    _httpListener.Start();

                    Console.WriteLine("Сервер запущен.");
                    Run();
                }
            }
        }

        public void Run()
        {
            while (_httpListener.IsListening)
            {
                Listen();
                Program.ExecuteCommand(this);
            }
        }

        private async Task Listen()
        {
            while (true)
            {
                var _httpContext = await _httpListener.GetContextAsync();
                HttpListenerRequest request = _httpContext.Request;

                // получаем объект ответа
                HttpListenerResponse response = _httpContext.Response;

                byte[] buffer;
                var responseProvider = new ResponseProvider( _serverSettings, _httpContext);

                try
                {
                    if (!responseProvider.FilesHandler(out buffer) &&
                        !responseProvider.MethodHandler(out buffer))
                    // if (!responseProvider.MethodHandler(out buffer))
                    {
                        buffer = responseProvider.NotFound();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    buffer = Encoding.UTF8.GetBytes("Unexpected error");
                }

                response.ContentLength64 = buffer.Length;

                Stream output = response.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                // закрываем поток
                output.Close();
            }
        }


        public void Stop()
        {
            // останавливаем прослушивание подключений
            _httpListener.Stop();
            Console.WriteLine("Сервер завершил работу.");
        }
    }
}