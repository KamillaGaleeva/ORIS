using HttpServer.Attributes;
using HttpServer.MyORM;

namespace HttpServer.Controllers;

[HttpController("main")]
public class MainController: Controller
{
    private static string _connectionStr = "Server=localhost;Database=museum;Port=5432;SSLMode=Prefer";

    private static TicketDAO _ticketDao = new(_connectionStr);

    [HttpGET("main")]
    public string main(string path)
    {
        var tickets = _ticketDao.GetAll();
        if (tickets is null) return "Events not found";
        return CreateHtmlCode(path,
            new { Tickets = tickets });
    }

}