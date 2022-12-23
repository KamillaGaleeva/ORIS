using HttpServer.Attributes;
using HttpServer.MyORM;

namespace HttpServer.Controllers;

[HttpController("exhibitions")]
public class ExhibitionController: Controller
{
    private static string _connectionStr = "Server=localhost;Database=museum;Port=5432;SSLMode=Prefer";

    private static ExhibitionDAO _exhibitionDao = new(_connectionStr);
    
    [HttpGET("exhibitions")]
    public string exhibitions(string path)
    {
        var exhibitions = _exhibitionDao.GetAll();
        if (exhibitions is null) return "exhibitions not found";
        return CreateHtmlCode(path,
            new { Exhibitions = exhibitions });
    }
}