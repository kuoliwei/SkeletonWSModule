using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;

public class WebSocketDebugPanel : MonoBehaviour
{
    [Header("Receiver")]
    [SerializeField] private WebSocketMessageReceiverAsync receiver;

    [Header("UI")]
    [SerializeField] private InputField ipInput;
    [SerializeField] private InputField portInput;

    [SerializeField] private Button connectButton;
    [SerializeField] private Button disconnectButton;

    [SerializeField] private Text statusText;
    [SerializeField] private Text rawJsonText;

    [Header("Default")]
    [SerializeField] private string ip;
    [SerializeField] private string port;

    private void Awake()
    {
        ipInput.text = ip;
        portInput.text = port;
        connectButton.onClick.AddListener(OnClickConnect);
        disconnectButton.onClick.AddListener(OnClickDisconnect);
    }

    private void OnEnable()
    {
        receiver.OnConnected += HandleConnected;
        receiver.OnDisconnected += HandleDisconnected;
        receiver.OnConnectionFailed += HandleConnectionFailed;
        receiver.OnRawJsonReceived += HandleRawJson;
    }

    private void OnDisable()
    {
        receiver.OnConnected -= HandleConnected;
        receiver.OnDisconnected -= HandleDisconnected;
        receiver.OnConnectionFailed -= HandleConnectionFailed;
        receiver.OnRawJsonReceived -= HandleRawJson;
    }

    private void OnClickConnect()
    {
        statusText.text = "Connecting...";
        receiver.Connect(ipInput.text, portInput.text);
    }

    private void OnClickDisconnect()
    {
        receiver.Close();
    }

    // --------------------
    // Event Handlers
    // --------------------

    private void HandleConnected()
    {
        statusText.text = "Connected";
    }

    private void HandleDisconnected()
    {
        statusText.text = "Disconnected";
    }

    private void HandleConnectionFailed(string reason)
    {
        statusText.text = $"Connection Failed: {reason}";
    }

    private void HandleRawJson(string json)
    {
        rawJsonText.text = json;
    }
}
