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

    private bool isTouchOnUI = false;
    private bool isDraggingObject = false; // NEW: Checks if we actually grabbed the 3D object

    void Awake()
    {
        raycastManager = FindObjectOfType<ARRaycastManager>();
    }

    void Update()
    {
        // Reset everything when fingers leave the screen
        if (Input.touchCount == 0)
        {
            isTouchOnUI = false;
            isDraggingObject = false;
            return;
        }

        Touch touch0 = Input.GetTouch(0);

        // --- CHECK WHAT WE ARE TOUCHING ---
        if (touch0.phase == TouchPhase.Began)
        {
            // 1. Are we touching a UI button or slider?
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touch0.fingerId))
            {
                isTouchOnUI = true;
                return;
            }

            // 2. Are we actually touching the 3D AR Object?
            Ray ray = Camera.main.ScreenPointToRay(touch0.position);
            RaycastHit hit;

            // Cast a physics laser from the camera to the screen touch point
            if (Physics.Raycast(ray, out hit))
            {
                // If the laser hits this object or any of its children (like the beaker or robot)
                if (hit.transform.IsChildOf(this.transform) || hit.transform == this.transform)
                {
                    isDraggingObject = true; // We grabbed it!
                }
            }
        }

        // If we touched UI, ignore the rest of the code
        if (isTouchOnUI) return;

        // --- 1 FINGER: MOVE ---
        if (Input.touchCount == 1)
        {
            // ONLY move if we successfully grabbed the object
            if (touch0.phase == TouchPhase.Moved && isDraggingObject)
            {
                MoveObject(touch0.position);
            }
        }
        // --- 2 FINGERS: SCALE & ROTATE ---
        else if (Input.touchCount == 2)
        {
            Touch touch1 = Input.GetTouch(1);

            if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
            {
                ScaleAndRotateObject(touch0, touch1);
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