namespace HttpServer;
using System.Net;

public class HttpServer
{
    private readonly HttpListener _listener;
    private byte[]? _buffer;

    private readonly string _path = @"google.html";
    
    public HttpServer(string url = "http://localhost:8888/google/")
    {
        _listener = new HttpListener();
        _listener.Prefixes.Add(url);
        if (InitializeResponse())
            Start();
        else
            Console.WriteLine("Error. Try again.");
    }

    private bool InitializeResponse()
    {
        try
        {
            using var fs = File.OpenRead(_path);
            _buffer = new byte[fs.Length];
            fs.Read(_buffer);
        }
        catch
        {
            Console.WriteLine("Have troubles with content.\nDocument file is missing or damaged.");
            return false;
        }

        return true;
    }

    public void Start()
    {
        _listener.Start();
        Console.WriteLine("Server is ready");
        var listen = new Thread(Listen);
        listen.Start();
        Console.Write("Stop server? Enter 0: ");
        var input = Console.ReadLine();
        while (input != "0")
        {
            Console.Write("Wrong operation.\nStop server? Enter 0: ");
            input = Console.ReadLine();
        }
        Stop();
    }

    public void Stop()
    {
        if (!_listener.IsListening)
            return;
        _listener.Stop();
        Console.Write("Server has stopped.\nRestart it or exit? Enter 1/0: ");
        var input = Console.ReadLine();
        while (input is not ("1" or "0"))
        {
            Console.Write("Wrong command.\nRestart server or exit? Enter 1/0: ");
            input = Console.ReadLine();
        }
        if (input == "1")
            Start();
    }

    private void Listen()
    {
        try
        {
            var context = _listener.GetContext();
            var response = context.Response;
            
            response.StatusCode = (int)HttpStatusCode.OK;
            response.ContentLength64 = _buffer.Length;
            
            var output = response.OutputStream;
            output.Write(_buffer);
            output.Close();
            Listen();
        }
        catch
        {
            if (!_listener.IsListening)
                return;
            Console.WriteLine("An error occured.");
            Stop();
        }
    }
}