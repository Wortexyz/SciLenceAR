// File: Assets/Scripts/UI/ARFlowManager.cs
// Minimal ARFlowManager with compatibility wrapper StartARForContent.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.EventSystems;
using System.Threading.Tasks;

public class ARFlowManager : MonoBehaviour
{
    public GameObject arRootPrefab;          // Prefabs/ARRoot.prefab
    public Transform arRootParent;           // optional
    public GameObject arOverlayPanel;        // the AROverlayPanel UI

    GameObject arRootInstance;
    GameObject currentContentInstance;

    ARSessionOrigin arSessionOrigin;
    ARRaycastManager raycastManager;
    ARPlaneManager planeManager;

    // Compatibility wrapper — some code calls StartARForContent
    public void StartARForContent(ContentDefinition def)
    {
        // call the primary StartAR method (it is async)
        StartAR(def);
    }

    // Start AR flow for a content definition
    public async void StartAR(ContentDefinition def)
    {
        Debug.Log("Start AR for: " + (def != null ? def.title : "null"));

        if (arRootPrefab == null)
        {
            Debug.LogWarning("ARFlowManager: arRootPrefab not assigned.");
            return;
        }

        if (arRootInstance == null)
        {
            arRootInstance = Instantiate(arRootPrefab, arRootParent);
        }

        arRootInstance.SetActive(true);

        // attempt to find managers
        arSessionOrigin = arRootInstance.GetComponentInChildren<ARSessionOrigin>();
        raycastManager = arRootInstance.GetComponentInChildren<ARRaycastManager>();
        planeManager = arRootInstance.GetComponentInChildren<ARPlaneManager>();

        if (arOverlayPanel != null) arOverlayPanel.SetActive(true);

        var contentRoot = arRootInstance.transform.Find("ARContentRoot");
        if (contentRoot == null)
        {
            Debug.LogError("ARContentRoot not found inside ARRoot prefab.");
            return;
        }

        // destroy previous content
        if (currentContentInstance != null) Destroy(currentContentInstance);

        GameObject prefab = def != null ? def.arPrefab : null;
        if (prefab == null)
        {
            Debug.LogWarning("No AR prefab assigned for content: " + (def != null ? def.contentId : "null"));
            return;
        }

        currentContentInstance = Instantiate(prefab, contentRoot);
        currentContentInstance.transform.localPosition = Vector3.zero;
        currentContentInstance.SetActive(false);

        // If running on device with AR, run tap-to-place; otherwise skip placement when not available
        bool placed = false;
        if (raycastManager != null)
        {
            placed = await RunTapToPlaceFlow();
            if (!placed)
            {
                Debug.LogWarning("Placement not completed, but continuing to show content.");
            }
        }

        currentContentInstance.SetActive(true);
        StartARContent();
    }

    async Task<bool> RunTapToPlaceFlow()
    {
        Debug.Log("Tap on a detected plane to place the content.");
        bool placed = false;
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        while (!placed)
        {
            if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                {
                    await Task.Delay(10); continue;
                }

                Vector2 touch = Input.GetTouch(0).position;
                if (raycastManager.Raycast(touch, hits, UnityEngine.XR.ARSubsystems.TrackableType.PlaneWithinPolygon))
                {
                    var pose = hits[0].pose;
                    var contentRoot = arRootInstance.transform.Find("ARContentRoot");
                    if (contentRoot != null)
                    {
                        contentRoot.position = pose.position;
                        contentRoot.rotation = pose.rotation;
                        placed = true;
                        break;
                    }
                }
            }
            await Task.Delay(50);
        }
        return placed;
    }

    void StartARContent()
    {
        var audio = currentContentInstance?.GetComponentInChildren<AudioSource>();
        if (audio != null) audio.Play();

        var anim = currentContentInstance?.GetComponentInChildren<Animator>();
        if (anim != null) anim.SetTrigger("Play");

        if (planeManager != null) planeManager.enabled = false;
    }

    public async void ExitAR()
    {
        Debug.Log("ExitAR called.");

        if (currentContentInstance != null)
        {
            var audio = currentContentInstance.GetComponentInChildren<AudioSource>();
            if (audio != null) audio.Stop();
            Destroy(currentContentInstance);
            currentContentInstance = null;
        }

        if (arOverlayPanel != null) arOverlayPanel.SetActive(false);

        if (arRootInstance != null)
        {
            var pm = arRootInstance.GetComponentInChildren<ARPlaneManager>();
            if (pm != null) pm.enabled = true;
            arRootInstance.SetActive(false);
        }

        await Task.Yield();
    }
}
