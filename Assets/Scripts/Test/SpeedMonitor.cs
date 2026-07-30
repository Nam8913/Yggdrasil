using UnityEngine;

/// <summary>
/// Gắn vào bất kỳ GameObject nào để theo dõi tốc độ di chuyển mỗi frame.
/// Hiển thị: vị trí hiện tại, delta position mỗi frame, tốc độ (units/s).
/// </summary>
public class SpeedMonitor : MonoBehaviour
{
    [Header("Display")]
    [SerializeField] private int fontSize = 16;
    [SerializeField] private Vector2 labelOffset = new Vector2(0, 20);

    [SerializeField]
    [TextArea(5, 10)]
    string debugText = "";

    private Vector3 _lastPosition;
    private float _deltaThisFrame;
    private float _speedPerSecond;
    private float _avgSpeed;
    private int _frameCount;
    private float _totalDistance;

    private GUIStyle _labelStyle;
    private GUIStyle _valueStyle;

    private void Start()
    {
        _lastPosition = transform.position;
    }

    private void LateUpdate()
    {
        Vector3 current = transform.position;
        _deltaThisFrame = Vector3.Distance(current, _lastPosition);
        _speedPerSecond = _deltaThisFrame / Time.deltaTime;
        _frameCount++;
        _totalDistance += _deltaThisFrame;
        _avgSpeed = _totalDistance / Time.time;
        _lastPosition = current;
    }

    private void OnGUI()
    {
        if (_labelStyle == null)
        {
            _labelStyle = new GUIStyle(GUI.skin.label) { fontSize = fontSize };
            _valueStyle = new GUIStyle(GUI.skin.label) { fontSize = fontSize, fontStyle = FontStyle.Bold };
        }

        Vector3 screenPos = Camera.main != null
            ? Camera.main.WorldToScreenPoint(transform.position + (Vector3)labelOffset * 0.01f)
            : transform.position;

        // Use world-to-screen for placement, fallback to screen coords
        float x = screenPos.x;
        float y = Screen.height - screenPos.y;

        GUI.Label(new Rect(x, y, 300, 20), $"[{gameObject.name}]", _labelStyle);
        GUI.Label(new Rect(x, y + 20, 300, 20), $"Delta/frame: {_deltaThisFrame:F6}", _labelStyle);
        GUI.Label(new Rect(x, y + 40, 300, 20), $"Speed: {_speedPerSecond:F4} u/s", _labelStyle);
        GUI.Label(new Rect(x, y + 60, 300, 20), $"Avg: {_avgSpeed:F4} u/s", _labelStyle);

        debugText = $"[{gameObject.name}]\nDelta/frame: {_deltaThisFrame:F6}\nSpeed: {_speedPerSecond:F4} u/s\nAvg: {_avgSpeed:F4} u/s";
    }
}
