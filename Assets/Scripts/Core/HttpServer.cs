using System;
using System.Net;
using System.Text;
using System.Collections.Generic;
// using System.Text.Json;

public class SimpleHttpServer
{
    private static Dictionary<string, string> boardState = new Dictionary<string, string>();

    public bool IsPlayerConnected(string player)
    {
        return boardState.ContainsKey(player);
    }
    public static void Main()
    {
        // HttpListener listener = new HttpListener();
        // listener.Prefixes.Add("http://localhost:8080/");
        // listener.Start();
        // Console.WriteLine("Listening...");

        // while (true)
        // {
        //     HttpListenerContext context = listener.GetContext();
        //     HttpListenerRequest request = context.Request;
        //     HttpListenerResponse response = context.Response;

        //     if (request.HttpMethod == "POST")
        //     {
        //         System.IO.Stream body = request.InputStream;
        //         System.Text.Encoding encoding = request.ContentEncoding;
        //         System.IO.StreamReader reader = new System.IO.StreamReader(body, encoding);
        //         string s = reader.ReadToEnd();

        //         boardState = JsonSerializer.Deserialize<Dictionary<string, string>>(s);
        //     }

        //     string responseString = JsonSerializer.Serialize(boardState);
        //     byte[] buffer = Encoding.UTF8.GetBytes(responseString);

        //     response.ContentLength64 = buffer.Length;
        //     System.IO.Stream output = response.OutputStream;
        //     output.Write(buffer, 0, buffer.Length);
        // output.Close();
    }
    // }
}