using UnityEngine;

public class SubjectManager : MonoBehaviour
{
    public static SubjectManager Instance;

    [Header("Panels")]
    public GameObject subjectPanel;    // The main subject selection panel (physics/chemistry/biology buttons)
    public GameObject physicsPanel;
    public GameObject chemistryPanel;
    public GameObject biologyPanel;

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
        OpenSubjectPanel();
    }

    private void HideAll()
    {
        if (subjectPanel) subjectPanel.SetActive(false);
        if (physicsPanel) physicsPanel.SetActive(false);
        if (chemistryPanel) chemistryPanel.SetActive(false);
        if (biologyPanel) biologyPanel.SetActive(false);
    }

    // Opens the main subject selection panel
    public void OpenSubjectPanel()
    {
        HideAll();
        if (subjectPanel) subjectPanel.SetActive(true);
    }

    // Subject openers (can be used by the subject buttons)
    public void OpenPhysics()
    {
        HideAll();
        if (physicsPanel) physicsPanel.SetActive(true);
    }

    public void OpenChemistry()
    {
        HideAll();
        if (chemistryPanel) chemistryPanel.SetActive(true);
    }

    public void OpenBiology()
    {
        HideAll();
        if (biologyPanel) biologyPanel.SetActive(true);
    }

    // Call this from any subject panel's Back button to return to the subject selection panel.
    public VideoController videoController; // assign VideoPlayerGO

    public void BackToSubjectPanel()
    {
        videoController.StopVideo();   
        OpenSubjectPanel();
    }

}
