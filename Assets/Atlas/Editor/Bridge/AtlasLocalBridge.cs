using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class AtlasLocalBridge
{
    private const int Port = 52741;

    private static readonly ConcurrentQueue<Action>
        MainThreadActions = new();

    private static TcpListener listener;
    private static Thread serverThread;
    private static bool isRunning;

    static AtlasLocalBridge()
    {
        EditorApplication.update += ProcessMainThreadActions;
    }

    [MenuItem("Atlas/Bridge/Start")]
    public static void Start()
    {
        if (isRunning)
        {
            Debug.Log("Atlas Bridge is already running.");
            return;
        }

        listener = new TcpListener(
            IPAddress.Loopback,
            Port
        );

        listener.Start();

        isRunning = true;

        serverThread = new Thread(ServerLoop)
        {
            IsBackground = true,
            Name = "Atlas Local Bridge"
        };

        serverThread.Start();

        Debug.Log(
            $"Atlas Bridge started on http://127.0.0.1:{Port}"
        );
    }

    [MenuItem("Atlas/Bridge/Stop")]
    public static void Stop()
    {
        if (!isRunning)
        {
            return;
        }

        isRunning = false;

        listener?.Stop();
        listener = null;

        Debug.Log("Atlas Bridge stopped.");
    }

    private static void ServerLoop()
    {
        while (isRunning)
        {
            try
            {
                TcpClient client = listener.AcceptTcpClient();

                HandleClient(client);
            }
            catch (SocketException)
            {
                if (isRunning)
                {
                    Debug.LogWarning(
                        "Atlas Bridge socket stopped unexpectedly."
                    );
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }

    private static void HandleClient(TcpClient client)
    {
        using (client)
        using (NetworkStream stream = client.GetStream())
        using (StreamReader reader = new(
                   stream,
                   Encoding.UTF8,
                   false,
                   1024,
                   leaveOpen: true))
        {
            string requestLine = reader.ReadLine();

            if (string.IsNullOrWhiteSpace(requestLine))
            {
                return;
            }

            string[] parts = requestLine.Split(' ');

            if (parts.Length < 2)
            {
                WriteResponse(
                    stream,
                    400,
                    "{\"error\":\"Invalid request\"}"
                );

                return;
            }

            string method = parts[0];
            string path = parts[1];

            // Consume the remaining request headers.
            string line;

            while (!string.IsNullOrEmpty(
                       line = reader.ReadLine()))
            {
            }

            if (method == "GET" &&
                path == "/atlas/scene")
            {
                HandleGetActiveScene(stream);
                return;
            }

            WriteResponse(
                stream,
                404,
                "{\"error\":\"Not found\"}"
            );
        }
    }

    private static void HandleGetActiveScene(
        NetworkStream stream)
    {
        ManualResetEventSlim completed = new(false);

        string json = null;
        Exception error = null;

        MainThreadActions.Enqueue(() =>
        {
            try
            {
                AtlasSceneSummary scene =
                    AtlasSceneTools.GetActiveScene();

                json = JsonUtility.ToJson(scene);
            }
            catch (Exception exception)
            {
                error = exception;
            }
            finally
            {
                completed.Set();
            }
        });

        if (!completed.Wait(TimeSpan.FromSeconds(5)))
        {
            WriteResponse(
                stream,
                503,
                "{\"error\":\"Unity Editor did not respond in time\"}"
            );

            return;
        }

        if (error != null)
        {
            WriteResponse(
                stream,
                500,
                "{\"error\":\"Unity inspection failed\"}"
            );

            return;
        }

        WriteResponse(
            stream,
            200,
            json
        );
    }

    private static void WriteResponse(
        NetworkStream stream,
        int statusCode,
        string body)
    {
        string statusText = statusCode switch
        {
            200 => "OK",
            400 => "Bad Request",
            404 => "Not Found",
            500 => "Internal Server Error",
            503 => "Service Unavailable",
            _ => "Unknown"
        };

        byte[] bodyBytes =
            Encoding.UTF8.GetBytes(body);

        string headers =
            $"HTTP/1.1 {statusCode} {statusText}\r\n" +
            "Content-Type: application/json; charset=utf-8\r\n" +
            $"Content-Length: {bodyBytes.Length}\r\n" +
            "Connection: close\r\n" +
            "\r\n";

        byte[] headerBytes =
            Encoding.UTF8.GetBytes(headers);

        stream.Write(
            headerBytes,
            0,
            headerBytes.Length
        );

        stream.Write(
            bodyBytes,
            0,
            bodyBytes.Length
        );

        stream.Flush();
    }

    private static void ProcessMainThreadActions()
    {
        while (MainThreadActions.TryDequeue(
                   out Action action))
        {
            action?.Invoke();
        }
    }
}