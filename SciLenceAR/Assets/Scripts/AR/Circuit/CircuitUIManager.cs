using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CircuitUIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject controlPanel;

    [Header("UI Controls")]
    public Slider voltageSlider;
    public Image powerButtonImage;
    public TextMeshProUGUI powerButtonText;

    [Header("AI Subtitles")]
    public TextMeshProUGUI aiSubtitleText; // NEW: Slot for your subtitle text

    void Start()
    {
        if (controlPanel != null) controlPanel.SetActive(false);

        if (voltageSlider != null)
        {
            voltageSlider.onValueChanged.AddListener(OnSliderMoved);
        }
    }

    public void ShowPanel()
    {
        if (controlPanel != null) controlPanel.SetActive(true);
    }

    // --- BUTTON CLICKS FROM THE CANVAS ---
    public void ClickMainPower()
    {
        var core = FindObjectOfType<CircuitCoreLogic>();
        if (core != null) core.TogglePower();
    }

    public void OnSliderMoved(float value)
    {
        var core = FindObjectOfType<CircuitCoreLogic>();
        if (core != null) core.SetVoltage(value);
    }

    // NEW: Function for your "Explain Science" button
    public void ClickExplain()
    {
        var core = FindObjectOfType<CircuitCoreLogic>();
        if (core != null) core.PlayAIExplanation();
    }

    // --- CALLED BY THE PREFAB ---
    public void UpdatePowerButtonVisuals(bool isPowerOn)
    {
        if (isPowerOn)
        {
            powerButtonImage.color = Color.green;
            powerButtonText.text = "POWER: ON";
        }
        else
        {
            powerButtonImage.color = Color.red;
            powerButtonText.text = "POWER: OFF";
        }
    }

    // NEW: Updates the text on screen
    public void UpdateSubtitle(string newText)
    {
        if (aiSubtitleText != null) aiSubtitleText.text = newText;
    }
}