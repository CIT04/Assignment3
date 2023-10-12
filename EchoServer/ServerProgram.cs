﻿using EchoServer;
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

    // Use a thread to handle the client
    ThreadPool.QueueUserWorkItem(HandleClient, client);
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

    if (request.Method != "create" || request.Method != "read" || request.Method != "update" || request.Method != "delete" || request.Method != "echo" )
    {
        return new CJTPResponse
        {
            Status = "illegal method",
            Body = "illegal method"
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