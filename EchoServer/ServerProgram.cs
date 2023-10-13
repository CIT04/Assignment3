using EchoServer;
using System;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.IO;
using System.Threading.Tasks;

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
    }
    catch (IOException)
    {
        // Handle the exception gracefully, e.g., log the error and continue listening for new connections.
        Console.WriteLine("Connection closed by the client.");
    }
}

// RequestHandler to process CJTPRequest and generate CJTPResponse
CJTPResponse ProcessRequest(CJTPRequest request)
{
    string status = "";
    //Tests for method


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
    //Tests for date
    if (string.IsNullOrWhiteSpace(request.Date))
    {
        status += "Missing Date ";
    }

    if (!long.TryParse(request.Date, out _))
    {
        status += "Illegal Date ";
    }


    //Test to check if method is missing nessesary body
    if (request.Method == "create" || request.Method == "update" ||
        request.Method == "echo" && string.IsNullOrEmpty(request.Body))
    {
        status += "missing body";
    }

    //Send back succes if no errors have been added to status
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
            Status = status.Trim(), 
            Body = status.Trim() 
        };
    }
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