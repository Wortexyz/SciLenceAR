using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARPosition : MonoBehaviour
{
    [Header("Placement")]
    public GameObject placementIndicator;
    public GameObject objectToSpawn;

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
        if (spawnedObject != null) return;

        UpdatePlacementPose();
        UpdatePlacementIndicator();

        if (placementValid &&
            Input.touchCount > 0 &&
            Input.GetTouch(0).phase == TouchPhase.Began)
        {
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
            placementIndicator.transform.SetPositionAndRotation(
                placementPose.position,
                placementPose.rotation);
    }

    void PlaceObject()
    {
        spawnedObject = Instantiate(
            objectToSpawn,
            placementPose.position,
            placementPose.rotation);

        placementIndicator.SetActive(false);
        HidePlaneMeshes();

        FindObjectOfType<ARUIManager>()?.RegisterObject(spawnedObject);
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
