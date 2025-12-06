using UnityEngine;

public class BiologyLevelManager : MonoBehaviour
{
    public static BiologyLevelManager Instance;

    [Header("Panels")]
    public GameObject BiologyPanel;    // The main subject selection panel (physics/chemistry/biology buttons)
    public GameObject HumanHeartPanel;
    public GameObject CellStructurePanel;
    public GameObject PhotosynthesisPanel;

    private void Awake()
    {
        // Simple singleton (keeps the first instance, destroys duplicates)
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Start by showing the subject selection panel by default
        OpenBiologyPanel();
    }

    private void HideAll()
    {
        if (BiologyPanel) BiologyPanel.SetActive(false);
        if (HumanHeartPanel) HumanHeartPanel.SetActive(false);
        if (CellStructurePanel) CellStructurePanel.SetActive(false);
        if (PhotosynthesisPanel) PhotosynthesisPanel.SetActive(false);
    }

    // Opens the main subject selection panel
    public void OpenBiologyPanel()
    {
        HideAll();
        if (BiologyPanel) BiologyPanel.SetActive(true);
    }

    // Subject openers (can be used by the subject buttons)
    public void OpenHumanHeartpanel()
    {
        HideAll();
        if (HumanHeartPanel) HumanHeartPanel.SetActive(true);
    }

    public void OpenCellStructure()
    {
        HideAll();
        if (CellStructurePanel) CellStructurePanel.SetActive(true);
    }

    public void OpenPhotosynthesis()
    {
        HideAll();
        if (PhotosynthesisPanel) PhotosynthesisPanel.SetActive(true);
    }

    // Call this from any subject panel's Back button to return to the subject selection panel.
    public void BackToBiologyPanel()
    {
        OpenBiologyPanel();
    }
}
