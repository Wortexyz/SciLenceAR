using UnityEngine;
using TMPro;

public class NewtonsUIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject controlPanel; // NEW: Drag the parent panel holding your buttons here!

    [Header("UI Text Elements")]
    public TextMeshProUGUI aiSubtitleText;
    public TextMeshProUGUI runButtonText;

    void Start()
    {
        // Hide the buttons when the app starts
        if (controlPanel != null) controlPanel.SetActive(false);
    }

    // --- CALLED BY THE PREFAB WHEN IT SPAWNS ---
    public void ShowPanel()
    {
        if (controlPanel != null) controlPanel.SetActive(true);
    }

    // --- BUTTON CLICKS FROM THE CANVAS ---
    public void ClickLaw1()
    {
        var manager = FindObjectOfType<NewtonsLawsManager>();
        if (manager != null) manager.SwitchToLaw(1);
    }

    public void ClickLaw2()
    {
        var manager = FindObjectOfType<NewtonsLawsManager>();
        if (manager != null) manager.SwitchToLaw(2);
    }

    public void ClickLaw3()
    {
        var manager = FindObjectOfType<NewtonsLawsManager>();
        if (manager != null) manager.SwitchToLaw(3);
    }

    public void ClickRunExperiment()
    {
        var manager = FindObjectOfType<NewtonsLawsManager>();
        if (manager != null) manager.RunCurrentExperiment();
    }

    // --- CALLED BY THE PREFAB TO UPDATE TEXT ---
    public void UpdateSubtitle(string newText)
    {
        if (aiSubtitleText != null) aiSubtitleText.text = newText;
    }

    public void UpdateRunButtonText(string newText)
    {
        if (runButtonText != null) runButtonText.text = newText;
    }
}