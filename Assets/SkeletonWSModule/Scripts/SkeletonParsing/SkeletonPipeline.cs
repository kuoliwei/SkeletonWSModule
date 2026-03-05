using System;
using UnityEngine;
using UnityEngine.Events;
using PoseTypes;

[Serializable]
public class FrameSampleEvent : UnityEvent<FrameSample> { }

public class SkeletonPipeline : MonoBehaviour
{
    [SerializeField] private WebSocketMessageReceiverAsync receiver;

    // Inspector 可訂閱
    [Header("Frame Output Event")]
    public FrameSampleEvent OnFrameReceived = new();

    // 純 C# 訂閱
    public event Action<FrameSample> OnFrameReceivedCode;

    private SkeletonJsonParser parser;

    private void Awake()
    {
        parser = new SkeletonJsonParser();

        receiver.OnRawJsonReceived += parser.Parse;
        parser.OnFrameParsed += HandleFrame;
    }

    private void OnDestroy()
    {
        if (receiver != null)
            receiver.OnRawJsonReceived -= parser.Parse;

        if (parser != null)
            parser.OnFrameParsed -= HandleFrame;
    }

    private void HandleFrame(FrameSample frame)
    {
        // 對外廣播（取代 Debug.Log）
        OnFrameReceived?.Invoke(frame);
        OnFrameReceivedCode?.Invoke(frame);
    }
}