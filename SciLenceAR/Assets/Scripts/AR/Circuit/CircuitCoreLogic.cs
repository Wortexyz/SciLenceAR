using UnityEngine;

public class CircuitCoreLogic : MonoBehaviour
{
    [Header("Hardware")]
    public Animator circuitAnimator;

    [Header("AI Voice System")]
    public AudioSource aiVoiceSource;    // NEW: The speaker on your prefab
    public AudioClip explanationAudio;   // NEW: The audio file you generate

    private bool isPowerOn = false;
    private float currentVoltage = 0.5f;
    private CircuitUIManager uiManager;

    void Start()
    {
        uiManager = FindObjectOfType<CircuitUIManager>();

        if (circuitAnimator != null) circuitAnimator.speed = 0f;

        if (uiManager != null)
        {
            uiManager.ShowPanel();
            uiManager.UpdatePowerButtonVisuals(isPowerOn);
            uiManager.UpdateSubtitle("[ AI ]: Press POWER ON to start the circuit, or EXPLAIN to hear how it works!");
            currentVoltage = uiManager.voltageSlider.value;
        }
    }

    public void TogglePower()
    {
        isPowerOn = !isPowerOn;

        if (uiManager != null)
        {
            uiManager.UpdatePowerButtonVisuals(isPowerOn);
            currentVoltage = uiManager.voltageSlider.value;
        }

        if (isPowerOn && circuitAnimator != null)
        {
            circuitAnimator.speed = currentVoltage;
        }
        else if (!isPowerOn && circuitAnimator != null)
        {
            circuitAnimator.speed = 0f;
        }
    }

    public void SetVoltage(float sliderValue)
    {
        currentVoltage = sliderValue;

        if (isPowerOn && circuitAnimator != null)
        {
            circuitAnimator.speed = currentVoltage;
        }
    }

    // NEW: Plays the audio and updates the subtitle!
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
            uiManager.UpdateSubtitle("[ AI ]: Voltage is the pressure pushing the electrons. Increase the voltage, and the current flows faster!");
        }
    }
}