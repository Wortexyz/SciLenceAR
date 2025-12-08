using UnityEngine;

public class ChemistryLevelManager : MonoBehaviour
{
    public static ChemistryLevelManager Instance;

    [Header("Panels")]
    public GameObject ChemistryPanel;    // The main subject selection panel (physics/chemistry/biology buttons)
    public GameObject MolecularPanel;
    public GameObject StateMatterPanel;
    public GameObject ChemicalReactionPanel;

    public VideoController MolecularVideoController;
    public VideoController StateMatterVideoController;
    public VideoController ChemicalReactionVideoController;

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
        
    }

    private void HideAll()
    {
        if (ChemistryPanel) ChemistryPanel.SetActive(false);
        if (MolecularPanel) MolecularPanel.SetActive(false);
        if (StateMatterPanel) StateMatterPanel.SetActive(false);
        if (ChemicalReactionPanel) ChemicalReactionPanel.SetActive(false);
    }

    // Opens the main subject selection panel
    public void OpenChemistryPanel()
    {
        HideAll();
        if (ChemistryPanel) ChemistryPanel.SetActive(true);
    }

    // Subject openers (can be used by the subject buttons)
    public void OpenMolecular()
    {
        HideAll();
        if (MolecularPanel) MolecularPanel.SetActive(true);
    }

    public void OpenStateMatter()
    {
        HideAll();
        if (StateMatterPanel) StateMatterPanel.SetActive(true);
    }

    public void OpenChemicalReaction()
    {
        HideAll();
        if (ChemicalReactionPanel) ChemicalReactionPanel.SetActive(true);
    }

    // Call this from any subject panel's Back button to return to the subject selection panel.
    public void BackToChemistryPanel()
    {
        if (MolecularVideoController) MolecularVideoController.StopVideo();
        if (StateMatterVideoController) StateMatterVideoController.StopVideo();
        if (ChemicalReactionVideoController) ChemicalReactionVideoController.StopVideo();
        OpenChemistryPanel();
    }
}