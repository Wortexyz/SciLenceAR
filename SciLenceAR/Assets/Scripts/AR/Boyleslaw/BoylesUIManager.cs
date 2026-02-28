using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BoylesUIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject controlPanel;

    [Header("UI Controls")]
    public Slider volumeSlider;

    [Header("AI Subtitles")]
    public TextMeshProUGUI aiSubtitleText;

    void Start()
    {
        // Hide panel until the user places the AR object
        if (controlPanel != null) controlPanel.SetActive(false);

        // Setup the slider limits (0 to 50% of the animation)
        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 0.5f;
            volumeSlider.value = 0f;

            // Listen for user dragging the slider
            volumeSlider.onValueChanged.AddListener(OnSliderMoved);
        }
    }

    // --- CALLED BY PREFAB ON SPAWN ---
    public void ShowPanel()
    {
        if (controlPanel != null) controlPanel.SetActive(true);
    }

    // --- CALLED WHEN SLIDER MOVES ---
    public void OnSliderMoved(float value)
    {
        // Find the spawned 3D model and tell it to scrub the animation!
        var core = FindObjectOfType<BoylesCoreLogic>();
        if (core != null) core.ScrubTimeline(value);
    }

    // --- LINK TO "EXPLAIN" BUTTON ---
    public void ClickExplain()
    {
        var core = FindObjectOfType<BoylesCoreLogic>();
        if (core != null) core.PlayAIExplanation();
    }

    public void UpdateSubtitle(string newText)
    {
        if (aiSubtitleText != null) aiSubtitleText.text = newText;
    }
}