using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using HttpServer.Attributes;
using HttpServer.Controllers;
using HttpServer.MyORM;

namespace HttpServer;

public class ResponseProvider
{
    private HttpListenerContext _listenerContext;
    private static string _connectionStr = "Server=localhost;Database=museum;Port=5432;SSLMode=Prefer";
    private SessionManager _sessionManager;
    //private AccountDAO _accountDao;
    private ServerSettings _serverSettings;

    public ResponseProvider(ServerSettings serverSettings, HttpListenerContext listenerContext)
    {
        _listenerContext = listenerContext;
        _sessionManager = SessionManager.Instance;
       
        _serverSettings = serverSettings;
    }

    public bool FilesHandler( out byte[] buffer)
    {
        buffer = null;
        if (Directory.Exists(_serverSettings.Path))
        {
            buffer = LoadFile(_serverSettings.Path);
        }

        if (buffer is null) return false;
        return true;
    }
    

    private byte[] LoadFile(string path)
    {
        var rawUrl = _listenerContext.Request.RawUrl.Replace("%20", " ");
        byte[] localBuffer = null;
        var filePath = path + rawUrl;

        if (Directory.Exists(filePath))
        {
            filePath = filePath + "/index.html";
            if (File.Exists(filePath))
            {
                _listenerContext.Response.Headers.Set("Content-Type", "text/html");
                localBuffer = File.ReadAllBytes(filePath);
            }
        }
        else if (File.Exists(filePath))
        {
            var contentType = Mime.GetMimeType(filePath);
            _listenerContext.Response.Headers.Set("Content-Type", contentType);
            localBuffer = File.ReadAllBytes(filePath);
        }

        return localBuffer;
    }

    public byte[] NotFound()
    {
        _listenerContext.Response.Headers.Set("Content-Type", "text/plain");
        _listenerContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
        string err = "404 - not found";
        return Encoding.UTF8.GetBytes(err);
    }


    public bool MethodHandler(out byte[] buffer)
    {
        bool isSaveAccountMethod = false;
        buffer = null;
        // объект запроса
        HttpListenerRequest request = _listenerContext.Request;

        // объект ответа
        HttpListenerResponse response = _listenerContext.Response;
        
        // if (request.Url.Segments.Length < 1) FilesHandler(out buffer);
        //
        // if (request.Url.Segments.Length < 2) return false;

        if (request.Url.Segments.Length < 2)
        {
            FilesHandler(out buffer);
            return true;
        }
        
        string controllerName = request.Url.Segments[1].Replace("/", "");
        
        string methodName;
        if (request.Url.Segments.Length < 3)
        {
             methodName = controllerName;
        }
        else
        {
             methodName = request.Url.Segments[2].Replace("/", "");
        }
      

        // if (String.IsNullOrEmpty(methodName))
        // {
        //   
        // }
        
        string[] strParams = { };
        object[] queryParams = { };

        Type controller;
        if (!TryGetController(out controller, controllerName)) return false;

        MethodInfo method;
        if (!TryGetMethod(out method, controller, methodName, out queryParams, controllerName)) return false;
        

        if (request.Url.Segments.Length > 3 && _listenerContext.Request.HttpMethod == HttpMethod.Get.ToString() && queryParams.Length == 1)
        {
            strParams = request.Url
                .Segments
                .Skip(3)
                .Select(s => s.Replace("/", ""))
                .ToArray();
            strParams = queryParams.Concat(strParams).Select(x=>x.ToString()).ToArray();

            queryParams = method.GetParameters()
                .Select((p, i) => Convert.ChangeType(strParams[i], p.ParameterType))
                .ToArray();
        }

        switch (method.Name)
        {
            case "getAccounts":
            {
                var cookie = request.Cookies["SessionId"];
                var isCookieAndSessionExist =
                    cookie is not null && _sessionManager.IsSessionExist(Guid.Parse(cookie.Value));
                if (!isCookieAndSessionExist)
                {
                    response.StatusCode = 401;
                    response.ContentType = "text/plain";
                    buffer = Encoding.ASCII.GetBytes("User is not authorized");
                    response.ContentLength64 = buffer.Length;
                    return true;
                }

                break;
            }
            case "getAccountInfo":
            {
                if (queryParams.Length != 1) return false;
                var cookie = request.Cookies["SessionId"];
                var isCookieAndSessionExist =
                    cookie is not null && _sessionManager.IsSessionExist(Guid.Parse(cookie.Value));
                if (!isCookieAndSessionExist)
                {
                    response.StatusCode = 401;
                    response.ContentType = "text/plain";
                    buffer = Encoding.ASCII.GetBytes("User is not authorized");
                    response.ContentLength64 = buffer.Length;
                    return true;
                }

                break;
            }

            case "getAccountById":
            {
                if (queryParams.Length != 1) return false;
                break;
            }
            
        }

        var ret = method.Invoke(Activator.CreateInstance(controller), queryParams);

       // response.ContentType = "Application/json";
       response.ContentType = "text/html";
       buffer = Encoding.UTF8.GetBytes(ret.ToString());
        //buffer = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(ret));

        switch (method.Name)
        {
            // case "saveAccount":
            // {
            //     response.Redirect("https://store.steampowered.com/login/");
            //
            //     break;
            // }

            case "login":
            {
                var res = ((bool, int?))ret;
                if (res.Item1)
                {
                    AccountDAO accountDao = new(_connectionStr);
                    var account = accountDao.GetById(res.Item2.Value);
                    var guid = _sessionManager.CreateSession(res.Item2!.Value,
                        account.Email, DateTime.Now);
                    response.SetCookie(new Cookie("SessionId", guid.ToString()));
                    var path = "./site/profil.html";
                    buffer = Encoding.UTF8.GetBytes(Controller.CreateHtmlCode(path, new{Account = account, Show = true}));
                    // buffer = Encoding.UTF8.GetBytes(
                    //     JsonSerializer.Serialize(new AccountDAO(_connectionStr).GetById(res.Item2.Value)));
                    // response.ContentType = "Application/json";
                }
                else
                {
                    response.ContentType = "text/plain";
                    buffer = Encoding.UTF8.GetBytes("User not found");
                   // return false;
                }

                break;
            }
        }

        response.ContentLength64 = buffer.Length;
        return true;
    }

    public bool TryGetController(out Type controller, string controllerName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        controller = assembly.GetTypes()
            .FirstOrDefault(c => Attribute.IsDefined(c, typeof(HttpController)) &&
                                 (((HttpController)c.GetCustomAttribute(typeof(HttpController))).ControllerName ==
                                  controllerName.ToLower() ||
                                  c.Name.ToLower() == controllerName.ToLower()));
        if (controller is null)
            return false;
        return true;
    }

    public bool TryGetMethod(out MethodInfo method, Type controller, string methodName, out object[] queryParams, string controllerName)
    {
        var methodType = $"Http{_listenerContext.Request.HttpMethod}";
        queryParams = new object[] { };
        switch (methodType)
        {
            case "HttpGET":
                method = controller.GetMethods()
                    .FirstOrDefault(c => Attribute.IsDefined(c, typeof(HttpGET)) &&
                                         (((HttpGET)c.GetCustomAttribute(typeof(HttpGET))).MethodURI ==
                                          methodName.ToLower() ||
                                          c.Name.ToLower() == methodName.ToLower()));
                string path ="";
                if(methodName == "main") path = _serverSettings.Path +"/index.html";
                else path = _serverSettings.Path +"/"+ controllerName.ToLower() + ".html";
                queryParams = new[] {path};
                break;
            case "HttpPOST":
                method = controller.GetMethods()
                    .FirstOrDefault(c => Attribute.IsDefined(c, typeof(HttpPOST)) &&
                                         (((HttpPOST)c.GetCustomAttribute(typeof(HttpPOST))).MethodURI ==
                                          methodName.ToLower() ||
                                          c.Name.ToLower() == methodName.ToLower()));
                //queryParams = new[] { GetPostData(_listenerContext.Request) };
                queryParams = GetParamsPostData(_listenerContext.Request);
                break;
            default:
                method = null;
                break;
        }

        if (method is null)
            return false;
        return true;
    }

    private static string[] GetParamsPostData(HttpListenerRequest request)
    {
        if (!request.HasEntityBody)
        {
            return null;
        }

        using (Stream body = request.InputStream)
        {
            using (var reader = new StreamReader(body, request.ContentEncoding))
            {
                var query = reader.ReadToEnd();
                var queryParams = query.Split('&')
                    .SelectMany(pair => pair.Split('='))
                    .Where(((s, i) => i % 2 == 1))
                    .ToArray();
                return queryParams;
            }
        }
    }

    // private static string GetPostData(HttpListenerRequest request)
    // {
    //     if (!request.HasEntityBody)
    //     {
    //         return null;
    //     }
    //
    //     using (Stream body = request.InputStream)
    //     {
    //         using (var reader = new StreamReader(body, request.ContentEncoding))
    //         {
    //             return reader.ReadToEnd();
    //         }
    //     }
    // }
}