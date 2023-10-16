using EchoServer;
using System;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Linq;

var port = 5000;

var server = new TcpListener(IPAddress.Loopback, port);
server.Start();

Console.WriteLine("Server started");

while (true)
{
    var client = server.AcceptTcpClient();
    Console.WriteLine("Client connected");

    // Use Task.Run to handle the client on a separate thread
    Task.Run(() => HandleClient(client));
}

// Method to handle a client's request on a separate thread
void HandleClient(object clientObj)
{
    var client = (TcpClient)clientObj;

    try
    {
        string request = client.MyRead();
        Console.WriteLine($"Request: {request}");

        CJTPRequest cjtpRequest = JsonConvert.DeserializeObject<CJTPRequest>(request);

        CJTPResponse response = ProcessRequest(cjtpRequest);

        client.MyWrite(JsonConvert.SerializeObject(response));
        client.Close();
    }
    catch (IOException)
    {
        // Handle the exception gracefully, e.g., log the error and continue listening for new connections.
        Console.WriteLine("Connection closed by the client.");
        client.Close();
    }
}

// RequestHandler to process CJTPRequest and generate CJTPResponse
CJTPResponse ProcessRequest(CJTPRequest request)
{
    string status = "";
    // Tests for method

    if (string.IsNullOrWhiteSpace(request.Method))
    {
        status += "Missing Method ";
    }

    if (request.Method != "create" && request.Method != "read" && request.Method != "update" && request.Method != "delete" && request.Method != "echo")
    {
        status += "illegal method ";
    }

    if (string.IsNullOrWhiteSpace(request.Path) || string.IsNullOrWhiteSpace(request.Date))
    {
        status += "missing resource ";
    }
    // Tests for date
    if (string.IsNullOrWhiteSpace(request.Date))
    {
        status += "Missing Date ";
    }

    if (!long.TryParse(request.Date, out _))
    {
        status += "Illegal Date ";
    }

    // Test to check if the method is missing necessary body
    if (request.Method is "create" or "update" or "echo" && string.IsNullOrEmpty(request.Body))
    {
        status += "missing body";
    }

    if (request.Method is "create" or "update" && !IsValidJson(request.Body))
    {
        status += "illegal body ";
    }

    // Send echo
    if (request.Method == "echo" && !string.IsNullOrEmpty(request.Body))
    {
        return new CJTPResponse
        {
            Status = "Success",
            Body = request.Body
        };
    }

    // API tests
    // Wrong categories -path

    if (!IsValidCategoryPath(request.Path))
    {
        status += "4 Bad Request ";
    }
    
    //This was our prefered solution instead of hard coding however it doesnt work. we are unable to determaine why
    //if (request.Method == "create" && request.Path.Split('/').Length > 2)
    //{
    //    status += "4 Bad Request ";
    //}

    if (request.Method == "create" && request.Path != "/api/categories")
    {
        status += "4 Bad Request ";
    }


    //Not needed
    //if (request.Method == "update" && !IsValidCategoryPath(request.Path))
    //{
    //    status += "4 Bad Request ";
    //}
    // If no errors found so far, set the response status to "Success"

    
    if (string.IsNullOrEmpty(status))
    {
        return new CJTPResponse
        {
            Status = "Success",
            Body = request.Method.ToUpper()
        };
    }
    else
    {
        return new CJTPResponse
        {
            Status = status.Trim()
        };
    }
}
bool IsValidCategoryPath(string path)
{
    var categories = Data.GetCategoriesList();
    foreach (var category in categories)
    {
        if (path == $"/api/categories/{category.cid}")
        {
            return true;
        }
    }
    return false;
}
static bool IsValidJson(string json)
{
    if (string.IsNullOrWhiteSpace(json))
    {
        return false;
    }

    json = json.Trim();

    if ((json.StartsWith("{") && json.EndsWith("}")) || (json.StartsWith("[") && json.EndsWith("]")))
    {
        try
        {
            JToken.Parse(json);
            return true;
        }
        catch (JsonReaderException)
        {
            return false;
        }
    }

    return false;
}

public class CJTPRequest
{
    public string Method { get; set; }
    public string Path { get; set; }
    public string Date { get; set; }
    public string Body { get; set; }
}

public class CJTPResponse
{
    public string Status { get; set; }
    public string Body { get; set; }
}

public class categories
{
    public int cid { get; set; }
    public string name { get; set; }

    public override string ToString()
    {
        return $"Id = {cid}, Name = {name}";
    }
}


public class Data
{
    public static List<categories> GetCategoriesList()
    {
        return new List<categories>
        {
            new categories { cid = 1, name = "Beverages" },
            new categories { cid = 2, name = "Condiments" },
            new categories { cid = 3, name = "Confections" },


        };
    }

}