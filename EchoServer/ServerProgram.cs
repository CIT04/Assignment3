using EchoServer;
using System;
using System.Net;
using System.Net.Sockets;
using Newtonsoft.Json;
using System.IO;

var port = 5000;

var server = new TcpListener(IPAddress.Loopback, port);
server.Start();

Console.WriteLine("Server started");

while (true)
{
    var client = server.AcceptTcpClient();
    Console.WriteLine("Client connected");

    string request = "";
    CJTPResponse response = new CJTPResponse();

    try
    {
        request = client.MyRead();
        Console.WriteLine($"Request: {request}");

        // Process the request here.
        CJTPRequest cjtpRequest = JsonConvert.DeserializeObject<CJTPRequest>(request);
        response = ProcessRequest(cjtpRequest);
    }
    catch (IOException)
    {
        // Handle the exception gracefully, e.g., log the error or continue listening for new connections.
        Console.WriteLine("Connection closed by the client.");
    }

    // Respond to the client
    client.MyWrite(JsonConvert.SerializeObject(response));
}

// RequestHandler to process CJTPRequest and generate CJTPResponse
CJTPResponse ProcessRequest(CJTPRequest request)
{
    // Implement request processing logic here
    // Return an appropriate CJTPResponse based on the request.
    if (string.IsNullOrWhiteSpace(request.Method) || request.Method == "{}")
    {
        return new CJTPResponse
        {
            Status = "Missing Method",
            Body = "Missing Method"
        };
    }
    // Add more request processing logic as needed
    else
    {
        return new CJTPResponse
        {
            Status = "Success",
            Body = request.Method.ToUpper()
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