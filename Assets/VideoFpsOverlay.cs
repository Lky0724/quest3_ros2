using UnityEngine;
using TMPro;

// Shows the video and render frame rates at the bottom-right corner of the video quad.
// World-space text, because screen-space UI is not visible in VR.
[RequireComponent(typeof(ROSStereoSubscriber))]
public class VideoFpsOverlay : MonoBehaviour
{
    [Tooltip("TextMeshPro font size; about 10 = 1 world unit tall")]
    public float fontSize = 1.5f;
    [Tooltip("Distance from the quad's bottom-right corner, in world units")]
    public Vector2 margin = new Vector2(0.08f, 0.06f);
    public float updateInterval = 0.5f;
    public Color textColor = Color.green;

    private ROSStereoSubscriber subscriber;
    private TextMeshPro fpsText;
    private int lastDecodedCount;
    private int renderedFrames;
    private float intervalStart;

    void Awake()
    {
        subscriber = GetComponent<ROSStereoSubscriber>();
    }

    void OnEnable()
    {
        if (fpsText == null)
            CreateText();
        fpsText.gameObject.SetActive(true);
        fpsText.text = "";

        lastDecodedCount = subscriber.DecodedFrameCount;
        renderedFrames = 0;
        intervalStart = Time.realtimeSinceStartup;
        PlaceText();
    }

    void OnDisable()
    {
        if (fpsText != null)
            fpsText.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (fpsText != null)
            Destroy(fpsText.gameObject);
    }

    private void CreateText()
    {
        // Sibling of the quad, not a child: the quad's non-uniform scale would stretch the text
        var go = new GameObject("FPS Overlay");
        go.transform.SetParent(transform.parent, false);

        fpsText = go.AddComponent<TextMeshPro>();
        fpsText.fontSize = fontSize;
        fpsText.alignment = TextAlignmentOptions.BottomRight;
        fpsText.textWrappingMode = TextWrappingModes.NoWrap;
        fpsText.color = textColor;
        fpsText.outlineWidth = 0.2f;
        fpsText.outlineColor = Color.black;
        fpsText.rectTransform.pivot = new Vector2(1f, 0f);
        fpsText.rectTransform.sizeDelta = new Vector2(3f, 0.5f);
    }

    private void PlaceText()
    {
        // Built-in Quad is 1x1 in local space, centred, visible from -Z
        Vector3 s = transform.localScale;
        Vector3 corner = new Vector3(s.x * 0.5f - margin.x, -s.y * 0.5f + margin.y, -0.01f);
        fpsText.transform.localRotation = transform.localRotation;
        fpsText.transform.localPosition = transform.localPosition + transform.localRotation * corner;
    }

    void Update()
    {
        renderedFrames++;
        float elapsed = Time.realtimeSinceStartup - intervalStart;
        if (elapsed < updateInterval)
            return;

        int decoded = subscriber.DecodedFrameCount;
        fpsText.text = $"Video {(decoded - lastDecodedCount) / elapsed:F1} fps\nRender {renderedFrames / elapsed:F0} fps";
        PlaceText();

        lastDecodedCount = decoded;
        renderedFrames = 0;
        intervalStart = Time.realtimeSinceStartup;
    }
}
