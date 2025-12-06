using UnityEngine;

public class PhysicsLevelManager : MonoBehaviour
{
    public static PhysicsLevelManager Instance;

    [Header("Panels")]
    public GameObject PhysicsPanel;    // The main subject selection panel (physics/chemistry/biology buttons)
    public GameObject NewtonLawPanel;
    public GameObject GravityPanel;
    public GameObject ElectricCircuitPanel;

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
        OpenPhysicsPanel();
    }

    private void HideAll()
    {
        if (PhysicsPanel) PhysicsPanel.SetActive(false);
        if (NewtonLawPanel) NewtonLawPanel.SetActive(false);
        if (GravityPanel) GravityPanel.SetActive(false);
        if (ElectricCircuitPanel) ElectricCircuitPanel.SetActive(false);
    }

    // Opens the main subject selection panel
    public void OpenPhysicsPanel()
    {
        HideAll();
        if (PhysicsPanel) PhysicsPanel.SetActive(true);
    }

    // Subject openers (can be used by the subject buttons)
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

    // Call this from any subject panel's Back button to return to the subject selection panel.
    public void BackToPhysicsPanel()
    {
        OpenPhysicsPanel();
    }
}
