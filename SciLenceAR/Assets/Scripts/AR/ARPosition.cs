using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARPosition : MonoBehaviour
{
    public GameObject placementIndicator;
    public GameObject objectToSpawn;

    private GameObject spawnedObject;
    private ARRaycastManager arRaycastManager;
    private Pose placementPose;
    private bool placementValid = false;

    void Start()
    {
        arRaycastManager = FindObjectOfType<ARRaycastManager>();
    }

    void Update()
    {
        UpdatePlacementPose();
        UpdatePlacementIndicator();

        if (placementValid && spawnedObject == null && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            PlaceObject();
        }
    }

    void UpdatePlacementPose()
    {
        Vector3 screenCenter = Camera.main.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));

        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        arRaycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon);

        placementValid = hits.Count > 0;

        if (placementValid)
        {
            placementPose = hits[0].pose;

            // Align the indicator forward relative to camera, flat on ground
            Vector3 forward = Camera.main.transform.forward;
            forward.y = 0;
            placementPose.rotation = Quaternion.LookRotation(forward);
        }
    }

    void UpdatePlacementIndicator()
    {
        if (spawnedObject == null && placementValid)
        {
            placementIndicator.SetActive(true);
            placementIndicator.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);
        }
        else
        {
            placementIndicator.SetActive(false);
        }
    }

    void PlaceObject()
    {
        spawnedObject = Instantiate(objectToSpawn, placementPose.position, placementPose.rotation);
        placementIndicator.SetActive(false);
    }
}