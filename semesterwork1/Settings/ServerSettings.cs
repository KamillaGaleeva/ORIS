namespace HttpServer;

public class ServerSettings
{
    public int Port { get; set; } = 8080;
    public string Path { get; set; } = @"./site/";
    public static string SettingsPath = @"./settings.json";
}