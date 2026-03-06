using UnityEngine;
using UnityEngine.Events;

public class WebSocketServerAPI : MonoBehaviour
{
    [Header("WebSocket Server")]
    [SerializeField] private WebSocketServer.WebSocketServer server;

    // ===== Receive Events =====
    public UnityEvent<string> OnMessageReceived = new();

    // ===== Client Events =====
    public UnityEvent OnClientConnected = new();
    public UnityEvent OnClientDisconnected = new();

    // ==============================
    // Send Message
    // ==============================

    public void Send(string message)
    {
        if (server == null)
        {
            Debug.LogError("[WebSocketServerAPI] Server reference missing.");
            return;
        }

        server.SendMessageToClient(message);
    }

    // ==============================
    // Receive Message
    // ==============================

    /// <summary>
    /// ¥Ñ WebSocketServer ©I¥s
    /// </summary>
    public void Receive(string message)
    {
        Debug.Log($"[WebSocketServerAPI] Receive: {message}");

        OnMessageReceived?.Invoke(message);
    }

    // ==============================
    // Client Events
    // ==============================

    public void ClientConnected()
    {
        Debug.Log("[WebSocketServerAPI] Client Connected");
        OnClientConnected?.Invoke();
    }

    public void ClientDisconnected()
    {
        Debug.Log("[WebSocketServerAPI] Client Disconnected");
        OnClientDisconnected?.Invoke();
    }
}