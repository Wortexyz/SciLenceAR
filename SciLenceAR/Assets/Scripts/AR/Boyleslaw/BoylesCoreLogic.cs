using UnityEngine;

public class BoylesCoreLogic : MonoBehaviour
{
    [Header("Hardware")]
    public Animator modelAnimator;
    public string animationStateName = "Take 001"; // Check your Animator for the exact name!

    [Header("AI Voice System")]
    public AudioSource aiVoiceSource;
    public AudioClip explanationAudio;

    private BoylesUIManager uiManager;

    void Start()
    {
        // 1. FREEZE the animation completely
        if (modelAnimator != null) modelAnimator.speed = 0f;

        // 2. Find the Canvas UI and link up
        uiManager = FindObjectOfType<BoylesUIManager>();
        if (uiManager != null)
        {
            uiManager.ShowPanel();
            uiManager.UpdateSubtitle("[ AI ]: Drag the slider to compress the gas and watch the pressure rise!");

            // Sync the 3D model with wherever the slider is currently sitting
            ScrubTimeline(uiManager.volumeSlider.value);
        }
    }

    // --- CALLED BY THE UI MANAGER ---
    public void ScrubTimeline(float sliderValue)
    {
        if (modelAnimator != null)
        {
            // Forces the animation to jump to the exact percentage of the slider
            modelAnimator.Play(animationStateName, 0, sliderValue);
        }
    }

    // --- CALLED BY THE UI MANAGER ---
    public void PlayAIExplanation()
    {
        if (explanationAudio != null && aiVoiceSource != null)
        {
            aiVoiceSource.Stop();
            aiVoiceSource.clip = explanationAudio;
            aiVoiceSource.Play();
        }
        if (uiManager != null)
        {
            uiManager.UpdateSubtitle("[ AI ]: This is Boyle's Law! As you decrease the volume, the pressure increases. Watch the hyperbola form on the graph!");
        }
    }
}