using System.Text.Json;
using Scriban;

namespace HttpServer.Controllers;

public class Controller
{
    public static string CreateHtmlCode(string path, object model)
    {
        if (!File.Exists(path)) return "404 - not found";
        var template = File.ReadAllText(path);
        var parsed = Template.Parse(template);
        return parsed.Render(model);
    }
}