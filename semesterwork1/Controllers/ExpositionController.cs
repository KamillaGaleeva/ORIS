using HttpServer.Attributes;
using HttpServer.MyORM;

namespace HttpServer.Controllers;

[HttpController("expositions")]
public class ExpositionController : Controller
{
    private static string _connectionStr = "Server=localhost;Database=museum;Port=5432;SSLMode=Prefer";

    private static ExpositionDAO _expositionDao = new(_connectionStr);

    [HttpGET("expositions")]
    public string expositions(string path)
    {
        var expositions = _expositionDao.GetAll();
        if (expositions is null) return "exhibitions not found";
        return CreateHtmlCode(path,
            new { Expositions = expositions });
    }
}