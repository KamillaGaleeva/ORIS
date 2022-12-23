namespace HttpServer.Attributes;

internal class HttpPOST : Attribute
{
    public string MethodURI;

    public HttpPOST(string methodIRI)
    {
        MethodURI = methodIRI;
    }
}