using UnityEngine;

public class PoseSimulator : MonoBehaviour
{
    [Header("Server API")]
    public WebSocketServerAPI server;

    // ===== Pose JSON =====

    [Header("Pose JSON (Raw)")]
    [TextArea(6, 18)]
    public string poseJsonRaw;

    [Header("Pose JSON Source")]
    public string poseResourceName = "3DPOSE_Output";
    public bool autoLoadPoseFromResources = true;

    // ===== Streaming =====

    [Header("Streaming")]
    public float poseFps = 15f;

    [SerializeField] private bool allowPoseLoop = false;

    private float sendTimer = 0f;

    void Awake()
    {
        if (autoLoadPoseFromResources)
            LoadPoseFromResources();
    }

    void Update()
    {
        if (!allowPoseLoop)
            return;

        sendTimer += Time.deltaTime;

        float interval = 1f / Mathf.Max(1f, poseFps);

        if (sendTimer >= interval)
        {
            SendPoseOnce();
            sendTimer = 0f;
        }
    }

    // =============================
    // Load Pose JSON
    // =============================

    public void LoadPoseFromResources()
    {
        var ta = Resources.Load<TextAsset>(poseResourceName);

        if (ta == null)
        {
            Debug.LogError($"[PoseSimulator] 找不到 Resources/{poseResourceName}");
            return;
        }

        poseJsonRaw = ta.text;

        Debug.Log($"[PoseSimulator] 已載入 Pose JSON ({poseJsonRaw.Length} chars)");
    }

    // =============================
    // Send
    // =============================

    public void SendPoseOnce()
    {
        if (string.IsNullOrWhiteSpace(poseJsonRaw))
        {
            Debug.LogWarning("[PoseSimulator] poseJsonRaw is empty");
            return;
        }

        server.Send(poseJsonRaw);
    }
}