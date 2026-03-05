using System;
using System.Collections.Concurrent;
using UnityEngine;
using UnityEngine.Events;
public class WebSocketMessageReceiverAsync : MonoBehaviour, IWebSocketTransport
{
    [Header("WebSocket 客戶端")]
    [SerializeField] private WebSocketClient webSocketClient;

    [Header("是否允許接收訊息")]
    [SerializeField] private bool allowReceiveMessages = true;
    public bool AllowReceiveMessages
    {
        get => allowReceiveMessages;
        set => allowReceiveMessages = value;
    }

    // 對外事件：給解析模組 / UI / 狀態機訂閱
    public event Action<string> OnRawJsonReceived;
    public event Action OnConnected;
    public event Action<string> OnConnectionFailed;
    public event Action OnDisconnected;

    // 多執行緒訊息佇列（WebSocket thread → Unity main thread）
    private readonly ConcurrentQueue<string> rawJsonQueue = new();

    private void Awake()
    {
        if (webSocketClient != null)
        {
            BindClient(webSocketClient);
        }
        else
        {
            Debug.LogWarning("[WebSocketMessageReceiverAsync] WebSocketClient 未指定");
        }
    }

    private void OnDestroy()
    {
        if (webSocketClient != null)
        {
            UnbindClient(webSocketClient);
        }
    }

    private void Update()
    {
        // 將 background queue 的訊息送回 Unity main thread
        while (rawJsonQueue.TryDequeue(out var json))
        {
            OnRawJsonReceived?.Invoke(json);
        }
    }

    /// <summary>
    /// 綁定 WebSocketClient 的事件
    /// </summary>
    private void BindClient(WebSocketClient client)
    {
        client.OnMessageReceive.AddListener(HandleIncomingMessage);

        client.OnConnected.AddListener(HandleClientConnected);
        client.OnConnectionError.AddListener(HandleClientConnectionError);
        client.OnDisconnected.AddListener(HandleClientDisconnected);
    }

    /// <summary>
    /// 解綁 WebSocketClient 的事件
    /// </summary>
    private void UnbindClient(WebSocketClient client)
    {
        client.OnMessageReceive.RemoveListener(HandleIncomingMessage);

        client.OnConnected.RemoveListener(HandleClientConnected);
        client.OnConnectionError.RemoveListener(HandleClientConnectionError);
        client.OnDisconnected.RemoveListener(HandleClientDisconnected);
    }

    /// <summary>
    /// 背景 thread 收到 JSON 時呼叫
    /// </summary>
    private void HandleIncomingMessage(string json)
    {
        if (!allowReceiveMessages)
            return;

        rawJsonQueue.Enqueue(json);
    }

    private void HandleClientConnected()
    {
        Debug.Log("[WS] Connected");
        OnConnected?.Invoke();
    }

    private void HandleClientConnectionError()
    {
        Debug.LogError("[WS] Connection Error");
        OnConnectionFailed?.Invoke("連線失敗");
    }

    private void HandleClientDisconnected()
    {
        Debug.LogWarning("[WS] Disconnected");
        OnDisconnected?.Invoke();
    }

    // ------------------------------
    // 公用方法：Connect / Send / Close
    // ------------------------------

    public void Connect(string ip, string port)
    {
        if (webSocketClient == null)
        {
            Debug.LogWarning("[WebSocketMessageReceiverAsync] 無法連線，webSocketClient 未設定");
            return;
        }

        string address = $"ws://{ip}:{port}";
        Debug.Log($"[WS] Connecting to {address}");

        webSocketClient.CloseConnection();
        webSocketClient.StartConnection(address);
    }

    public void Send(string message)
    {
        if (webSocketClient == null)
        {
            Debug.LogWarning("[WebSocketMessageReceiverAsync] webSocketClient 未設定");
            return;
        }

        webSocketClient.SendSocketMessage(message);
    }

    public void Close()
    {
        if (webSocketClient == null)
            return;

        webSocketClient.CloseConnection();
    }
}
