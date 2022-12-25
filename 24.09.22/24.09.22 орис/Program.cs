using System;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using static _24._09._22_орис.HttpServer;

namespace _24._09._22_орис
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var server = new MyHttpServer(new string[] { "http://localhost:8888/google/" });
            //await server.Start();
        }
    }
    public class MyHttpServer
    {
        private HttpListener listener;

        public MyHttpServer(string[] prefixes)
        { }
        public class HttpServer
    {
        public void Start()
        {
            Console.WriteLine("Запуск сервера");
            listener.Start();
            Console.WriteLine("Сервер запущен");
            Receive();
        }
        public void Stop()
        {
            Console.WriteLine("Остановка сервера");
            listener.Stop();
            Console.WriteLine("Сервер остановлен");
        }
        public void Receive()
        {
            listener.BeginGetContext(new AsyncCallback(ListenerCall), listener);
        }
        private readonly HttpListener listener;
        public HttpServer()
        {
            listener = new HttpListener();
            listener.Prefixes.Add($"http://localhost:8888/google/");
        }
        private void ListenerCall(IAsyncResult res)
        {
            if (listener.IsListening)
            {
                var cont= listener.EndGetContext(res);                
                // получаем объект ответа
                var resp = cont.Response;
                // создаем ответ в виде кода html
                string respStr = File.ReadAllText("Users/User/Desktop/24.09.22 орис/24.09.22 орис/google.html");
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(respStr);
                // получаем поток ответа и пишем в него ответ
                resp.ContentLength64 = buffer.Length;
                Stream output = resp.OutputStream;
                output.Write(buffer, 0, buffer.Length);
                // закрываем поток
                output.Close();
                Receive();
            }
        }
    }
}