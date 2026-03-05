using System;

public interface IWebSocketTransport
{
    event Action<string> OnRawJsonReceived;
    event Action OnConnected;
    event Action<string> OnConnectionFailed;
    event Action OnDisconnected;

    bool AllowReceiveMessages { get; set; }

    void Connect(string ip, string port);
    void Send(string message);
    void Close();
}