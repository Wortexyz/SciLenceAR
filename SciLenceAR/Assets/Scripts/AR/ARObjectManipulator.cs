using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;

public class ARObjectManipulator : MonoBehaviour
{
    public enum Mode { None, Move, Rotate, Scale }
    public Mode currentMode = Mode.None;

    [Header("Scale Limits")]
    public float minScale = 0.3f;
    public float maxScale = 2.0f;

    private ARRaycastManager raycastManager;
    private float initialRotationY;
    private Vector3 initialScale;

    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Awake()
    {
        raycastManager = FindObjectOfType<ARRaycastManager>();
        initialRotationY = transform.eulerAngles.y;
        initialScale = transform.localScale;
    }

    void Update()
    {
        if (currentMode != Mode.Move) return;
        if (Input.touchCount == 0) return;

        Touch t = Input.GetTouch(0);
        if (t.phase == TouchPhase.Moved)
        {
            MoveByTarget(t.position);
        }
    }

    void MoveByTarget(Vector2 screenPos)
    {
        if (raycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon))
        {
            transform.position = hits[0].pose.position;
        }
    }

    // ---------- UI CALLED ----------
    public void SetMoveMode() => currentMode = Mode.Move;
    public void SetRotateMode() => currentMode = Mode.Rotate;
    public void SetScaleMode() => currentMode = Mode.Scale;
    public void DisableMode() => currentMode = Mode.None;

    public void RotateBySlider(float value)
    {
        if (currentMode != Mode.Rotate) return;
        float y = Mathf.Lerp(-180f, 180f, value);
        transform.rotation = Quaternion.Euler(0, y, 0);
    }

    public void ScaleBySlider(float value)
    {
        if (currentMode != Mode.Scale) return;
        float s = Mathf.Lerp(minScale, maxScale, value);
        transform.localScale = Vector3.one * s;
    }

    public void ResetRotation()
    {
        transform.rotation = Quaternion.Euler(0, initialRotationY, 0);
    }

    public void ResetScale()
    {
        transform.localScale = initialScale;
    }
}
