using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class ARGestureManipulator : MonoBehaviour
{
    [Header("Scale Limits")]
    public float minScale = 0.2f;
    public float maxScale = 3.0f;
    public float scaleSpeed = 0.005f;

    private ARRaycastManager raycastManager;
    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Awake()
    {
        raycastManager = FindObjectOfType<ARRaycastManager>();
    }

    void Update()
    {
        if (Input.touchCount == 0) return;

        // Block AR interaction if the user is touching a UI button (like the "Back" button)
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            return;

        // --- 1 FINGER: MOVE ---
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Moved)
            {
                MoveObject(touch.position);
            }
        }
        // --- 2 FINGERS: SCALE & ROTATE ---
        else if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            if (touch1.phase == TouchPhase.Moved || touch2.phase == TouchPhase.Moved)
            {
                ScaleAndRotateObject(touch1, touch2);
            }
        }
    }

    void MoveObject(Vector2 screenPosition)
    {
        if (raycastManager.Raycast(screenPosition, hits, TrackableType.PlaneWithinPolygon))
        {
            transform.position = hits[0].pose.position;
        }
    }

    void ScaleAndRotateObject(Touch touch1, Touch touch2)
    {
        Vector2 touch1PrevPos = touch1.position - touch1.deltaPosition;
        Vector2 touch2PrevPos = touch2.position - touch2.deltaPosition;

        // --- SCALE ---
        float prevTouchDeltaMag = (touch1PrevPos - touch2PrevPos).magnitude;
        float touchDeltaMag = (touch1.position - touch2.position).magnitude;
        float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

        float currentScale = transform.localScale.x;
        currentScale -= deltaMagnitudeDiff * scaleSpeed;
        currentScale = Mathf.Clamp(currentScale, minScale, maxScale);
        transform.localScale = new Vector3(currentScale, currentScale, currentScale);

        // --- ROTATE ---
        float prevAngle = Mathf.Atan2(touch1PrevPos.y - touch2PrevPos.y, touch1PrevPos.x - touch2PrevPos.x) * Mathf.Rad2Deg;
        float currentAngle = Mathf.Atan2(touch1.position.y - touch2.position.y, touch1.position.x - touch2.position.x) * Mathf.Rad2Deg;
        float angleDelta = Mathf.DeltaAngle(prevAngle, currentAngle);

        transform.Rotate(Vector3.up, angleDelta, Space.World);
    }
}