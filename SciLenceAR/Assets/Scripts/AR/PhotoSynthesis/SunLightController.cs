using UnityEngine;
using System.Collections;
using TMPro; // Note: Use UnityEngine.UI if not using TextMeshPro

public class PhotosynthesisController : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip tapSunClip;
    public AudioClip sunlightExplanationClip;
    public AudioClip tapPlantClip;
    public AudioClip plantExplanationClip;
    public AudioClip tapByproductClip;
    public AudioClip byproductExplanationClip;

    [Header("Instructional Text")]
    public GameObject tapSunText;
    public GameObject tapPlantText;

    [Header("Labels")]
    public GameObject sunLabel;
    public GameObject plantLabel;
    public GameObject byproductLabel;

    [Header("Sun Phase")]
    public GameObject coneObject;
    public GameObject[] sunButtons;

    [Header("Plant Phase")]
    public GameObject[] plantButtons;
    public GameObject waterObject;
    public GameObject carbonParticles;

    [Header("Byproduct Phase")]
    public GameObject byproductButton;
    public GameObject oxygenParticles;
    public GameObject glucoseParticles;

    void Start()
    {
        // Initial State: Only Sun instructions and buttons are active
        ResetAll();
        tapSunText.SetActive(true);
        ToggleGroup(sunButtons, true);

        // Play initial audio
        PlayAudio(tapSunClip);
    }

    // 1. Linked to Sun Buttons
    public void OnSunButtonClick()
    {
        audioSource.Stop(); // Stop "Tap on the Sun"
        tapSunText.SetActive(false); 
        coneObject.SetActive(true);
        sunLabel.SetActive(true);
        ToggleGroup(sunButtons, false); 
        
        StartCoroutine(HandleSunlightExplanation());
    }

    IEnumerator HandleSunlightExplanation()
    {
        PlayAudio(sunlightExplanationClip);
        
        // Wait for explanation to finish
        yield return new WaitWhile(() => audioSource.isPlaying);
        yield return new WaitForSeconds(0.5f); // Brief pause for natural flow

        tapPlantText.SetActive(true); 
        ToggleGroup(plantButtons, true);
        PlayAudio(tapPlantClip);
    }

    // 2. Linked to Plant Buttons
    public void OnPlantButtonClick()
    {
        audioSource.Stop(); // Stop "Tap on the Plant"
        tapPlantText.SetActive(false); 
        waterObject.SetActive(true);
        carbonParticles.SetActive(true);
        plantLabel.SetActive(true);
        
        sunLabel.SetActive(false); 
        ToggleGroup(plantButtons, false);

        StartCoroutine(HandlePlantExplanation());
    }

    IEnumerator HandlePlantExplanation()
    {
        PlayAudio(plantExplanationClip);

        // Wait for explanation to finish
        yield return new WaitWhile(() => audioSource.isPlaying);
        yield return new WaitForSeconds(0.5f);

        byproductButton.SetActive(true);
        PlayAudio(tapByproductClip);
    }

    // 3. Linked to Byproduct Button
    public void OnByproductButtonClick()
    {
        audioSource.Stop(); // Stop "Tap on Byproduct"
        
        // Clean up Plant phase
        waterObject.SetActive(false);
        carbonParticles.SetActive(false);
        plantLabel.SetActive(false);
        
        // Show Byproducts
        oxygenParticles.SetActive(true);
        glucoseParticles.SetActive(true);
        byproductLabel.SetActive(true);
        
        byproductButton.SetActive(false);

        PlayAudio(byproductExplanationClip);
    }

    // Helper Functions
    void PlayAudio(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    void ToggleGroup(GameObject[] objects, bool state)
    {
        foreach (GameObject obj in objects) if(obj != null) obj.SetActive(state);
    }

    void ResetAll()
    {
        tapSunText.SetActive(false);
        tapPlantText.SetActive(false);
        sunLabel.SetActive(false);
        plantLabel.SetActive(false);
        byproductLabel.SetActive(false);
        coneObject.SetActive(false);
        waterObject.SetActive(false);
        carbonParticles.SetActive(false);
        oxygenParticles.SetActive(false);
        glucoseParticles.SetActive(false);
        byproductButton.SetActive(false);
        ToggleGroup(plantButtons, false);
    }
}