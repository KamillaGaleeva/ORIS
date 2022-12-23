using HttpServer.MyORM;

namespace HttpServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new HttpServer();
            Console.WriteLine("Введите start для запуска сервера, stop для остановки и restart для перезапуска.");
            while (true)
            {
                ExecuteCommand(server);
            }
        }

        public static void ExecuteCommand(HttpServer server)
        {
            var command = Console.ReadLine();

            switch (command)
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
                    Console.WriteLine("Введенная команда некорректна!");
                    break;
            }
        }
    }
}