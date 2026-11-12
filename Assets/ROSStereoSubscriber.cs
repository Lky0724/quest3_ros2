using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using System.Collections;
#if UNITY_ANDROID
using Unity.Collections;
using UnityEngine.XR;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR.Features.Meta;
#endif

public class ROSStereoSubscriber : MonoBehaviour
{
    // 订阅左右眼画面话题
    [Header("ROS Topics")]
    public string leftTopic = "/zed/zed_node/rgb/color/rect/image/compressed";
    public string rightTopic = "/zed/zed_node/rgb/color/rect/image/compressed";

    [Header("ROS Connection")]
    [Tooltip("Drag the ROSConnection GameObject for the image endpoint (e.g. port 10000) here. Falls back to the default singleton if left empty.")]
    public ROSConnection ros;

    [Header("Display")]
    [Tooltip("Quest 3: 72, 80, 90 or 120. 120 is an exact multiple of the 60 fps video. 60 is not supported.")]
    public float displayRefreshRate = 120f;

    // Frames decoded into the left texture since start; read by VideoFpsOverlay
    public int DecodedFrameCount { get; private set; }

    private Texture2D texLeft;
    private Texture2D texRight;
    private Material stereoMaterial;

    // Newest undecoded frames. Callbacks only store them, so a backlog can never block a frame:
    // ROSConnection delivers every queued message within a single Update on the main thread,
    // and this main thread also renders the scene and publishes the head commands.
    private byte[] pendingLeft;
    private byte[] pendingRight;

    private int receivedFrames;
    private int decodedFrames;
    private int renderedFrames;
    private float decodeMsSum;
    private float decodeMsMax;
    private float renderMsMax;
    private float statsStart;

    void Start()
    {
        if (ros == null)
            ros = ROSConnection.GetOrCreateInstance();

        // 获取当前物体上的材质球
        stereoMaterial = GetComponent<Renderer>().material;

        texLeft = CreateVideoTexture();
        stereoMaterial.SetTexture("_LeftTex", texLeft);
        ros.Subscribe<CompressedImageMsg>(leftTopic, ReceiveLeftImage);

        if (rightTopic == leftTopic)
        {
            // Mono camera: both eyes share one texture, so each frame is decoded only once
            texRight = texLeft;
        }
        else
        {
            texRight = CreateVideoTexture();
            ros.Subscribe<CompressedImageMsg>(rightTopic, ReceiveRightImage);
        }
        stereoMaterial.SetTexture("_RightTex", texRight);

        statsStart = Time.realtimeSinceStartup;
        StartCoroutine(RequestDisplayRefreshRate());
    }

    private IEnumerator RequestDisplayRefreshRate()
    {
#if UNITY_ANDROID
        // XR may still be starting up when the scene loads
        XRDisplaySubsystem display = null;
        float start = Time.realtimeSinceStartup;
        while (display == null || !display.running)
        {
            if (Time.realtimeSinceStartup - start > 10f)
            {
                Debug.LogWarning("[RefreshRate] XR display subsystem not available");
                yield break;
            }
            yield return null;
            var settings = XRGeneralSettings.Instance;
            var loader = settings != null && settings.Manager != null ? settings.Manager.activeLoader : null;
            display = loader != null ? loader.GetLoadedSubsystem<XRDisplaySubsystem>() : null;
        }

        if (display.TryGetSupportedDisplayRefreshRates(Allocator.Temp, out var rates))
            Debug.Log($"[RefreshRate] Supported: {string.Join(", ", rates)} Hz");

        bool accepted = display.TryRequestDisplayRefreshRate(displayRefreshRate);
        Debug.Log($"[RefreshRate] Request {displayRefreshRate} Hz: {(accepted ? "accepted" : "rejected")}");
#else
        yield break;
#endif
    }

    private static Texture2D CreateVideoTexture()
    {
        // No mipmaps: LoadImage would otherwise have to generate them for every frame
        var texture = new Texture2D(2, 2, TextureFormat.RGB24, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        return texture;
    }

    void ReceiveLeftImage(CompressedImageMsg message)
    {
        pendingLeft = message.data;
        receivedFrames++;
    }

    void ReceiveRightImage(CompressedImageMsg message)
    {
        pendingRight = message.data;
    }

    void Update()
    {
        // At most one decode per texture per rendered frame, always the newest image
        DecodePending(ref pendingLeft, texLeft);
        DecodePending(ref pendingRight, texRight);
        LogStats();
    }

    private void DecodePending(ref byte[] pending, Texture2D texture)
    {
        if (pending == null)
            return;

        if (texture == texLeft)
            DecodedFrameCount++;

        byte[] jpeg = pending;
        pending = null;

        var watch = System.Diagnostics.Stopwatch.StartNew();
        texture.LoadImage(jpeg);
        float decodeMs = (float)watch.Elapsed.TotalMilliseconds;

        decodedFrames++;
        decodeMsSum += decodeMs;
        decodeMsMax = Mathf.Max(decodeMsMax, decodeMs);
    }

    private void LogStats()
    {
        renderedFrames++;
        renderMsMax = Mathf.Max(renderMsMax, Time.unscaledDeltaTime * 1000f);

        float elapsed = Time.realtimeSinceStartup - statsStart;
        if (elapsed < 5f)
            return;

        float decodeMsAvg = decodedFrames > 0 ? decodeMsSum / decodedFrames : 0f;
        Debug.Log(
            $"[VideoStats] received {receivedFrames / elapsed:F1} fps, " +
            $"decoded {decodedFrames / elapsed:F1} fps, " +
            $"decode avg {decodeMsAvg:F1} ms, max {decodeMsMax:F1} ms, " +
            $"render {renderedFrames / elapsed:F1} fps, longest frame {renderMsMax:F0} ms");

        receivedFrames = 0;
        decodedFrames = 0;
        renderedFrames = 0;
        decodeMsSum = 0f;
        decodeMsMax = 0f;
        renderMsMax = 0f;
        statsStart = Time.realtimeSinceStartup;
    }
}
