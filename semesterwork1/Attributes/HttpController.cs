namespace HttpServer.Attributes;

internal class HttpController: Attribute
{
    public string ControllerName;

    public HttpController()
    { }
    
    public HttpController(string controllerName)
    {
        ControllerName = controllerName;
    }
}