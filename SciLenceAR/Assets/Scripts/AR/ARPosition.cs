using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;

public class ARPosition : MonoBehaviour
{
    [Header("Placement")]
    public GameObject placementIndicator;
    public GameObject objectToSpawn; // Make sure your 3D prefab has the ARGestureManipulator script attached!

    private GameObject spawnedObject;
    private ARRaycastManager raycastManager;
    private ARPlaneManager planeManager;
    private Pose placementPose;
    private bool placementValid;

    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Start()
    {
        raycastManager = FindObjectOfType<ARRaycastManager>();
        planeManager = FindObjectOfType<ARPlaneManager>();

        if (placementIndicator)
            placementIndicator.SetActive(false);
    }

    void Update()
    {
        // Stop updating placement once the object is spawned
        if (spawnedObject != null) return;

        UpdatePlacementPose();
        UpdatePlacementIndicator();

        if (placementValid && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            // Prevent spawning the object if the user is tapping a UI element
            if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                return;

            PlaceObject();
        }
    }

    void UpdatePlacementPose()
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon);
        placementValid = hits.Count > 0;

        if (placementValid)
        {
            placementPose = hits[0].pose;
            Vector3 camForward = Camera.main.transform.forward;
            camForward.y = 0;
            placementPose.rotation = Quaternion.LookRotation(camForward);
        }
    }

    void UpdatePlacementIndicator()
    {
        if (!placementIndicator) return;

        placementIndicator.SetActive(placementValid);
        if (placementValid)
            placementIndicator.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);
    }

    void PlaceObject()
    {
        spawnedObject = Instantiate(objectToSpawn, placementPose.position, placementPose.rotation);
        placementIndicator.SetActive(false);
        HidePlaneMeshes();

        // Automatically show the instructions right after spawning
        FindObjectOfType<ARUIManager>()?.ShowInstructions();
    }

    void HidePlaneMeshes()
    {
        if (planeManager == null) return;

        foreach (ARPlane plane in planeManager.trackables)
        {
            var mr = plane.GetComponent<MeshRenderer>();
            if (mr) mr.enabled = false;
        }
    }
}