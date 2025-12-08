using UnityEngine;

public class PhysicsLevelManager : MonoBehaviour
{
    public static PhysicsLevelManager Instance;

    [Header("Panels")]
    public GameObject PhysicsPanel;
    public GameObject NewtonLawPanel;
    public GameObject GravityPanel;
    public GameObject ElectricCircuitPanel;

    [Header("Video Controllers")]
    public VideoController newtonVideoController;
    public VideoController gravityVideoController;
    public VideoController electricCircuitVideoController;

    private void Awake()
    {
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
        
    }

    private void HideAll()
    {
        if (PhysicsPanel) PhysicsPanel.SetActive(false);
        if (NewtonLawPanel) NewtonLawPanel.SetActive(false);
        if (GravityPanel) GravityPanel.SetActive(false);
        if (ElectricCircuitPanel) ElectricCircuitPanel.SetActive(false);
    }

    public void OpenPhysicsPanel()
    {
        HideAll();
        if (PhysicsPanel) PhysicsPanel.SetActive(true);
    }

    public void OpenNewtonsLaw()
    {
        HideAll();
        if (NewtonLawPanel) NewtonLawPanel.SetActive(true);
    }

    public void OpenGravity()
    {
        HideAll();
        if (GravityPanel) GravityPanel.SetActive(true);
    }

    public void OpenElectricCircuit()
    {
        HideAll();
        if (ElectricCircuitPanel) ElectricCircuitPanel.SetActive(true);
    }

    public void BackToPhysicsPanel()
    {
        // stop all subject videos (safe even if some aren't playing)
        if (newtonVideoController) newtonVideoController.StopVideo();
        if (gravityVideoController) gravityVideoController.StopVideo();
        if (electricCircuitVideoController) electricCircuitVideoController.StopVideo();

        OpenPhysicsPanel();
    }
}