namespace HttpServer.Attributes;

internal class HttpGET: Attribute
{
    public string MethodURI;

    public HttpGET()
    { }
    public HttpGET(string methodURI)
    {
        MethodURI = methodURI;
    }
}