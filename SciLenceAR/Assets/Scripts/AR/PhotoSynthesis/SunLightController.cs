using UnityEngine;
using System.Collections;
using TMPro; // Note: Use UnityEngine.UI if not using TextMeshPro

public class PhotosynthesisController : MonoBehaviour
{
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
    }

    // 1. Linked to Sun Buttons
    public void OnSunButtonClick()
    {
        tapSunText.SetActive(false); // Hide "Tap on the Sun" immediately
        coneObject.SetActive(true);
        sunLabel.SetActive(true);
        ToggleGroup(sunButtons, false); 
        
        StartCoroutine(WaitToShowPlantTask());
    }

    IEnumerator WaitToShowPlantTask()
    {
        yield return new WaitForSeconds(2f);
        tapPlantText.SetActive(true); // Show "Tap on the Plant"
        ToggleGroup(plantButtons, true);
    }

    // 2. Linked to Plant Buttons
    public void OnPlantButtonClick()
    {
        tapPlantText.SetActive(false); // Hide "Tap on the Plant" immediately
        waterObject.SetActive(true);
        carbonParticles.SetActive(true);
        plantLabel.SetActive(true);
        
        sunLabel.SetActive(false); // Hide Sun Label
        ToggleGroup(plantButtons, false);

        StartCoroutine(WaitToShowByproduct());
    }

    IEnumerator WaitToShowByproduct()
    {
        yield return new WaitForSeconds(2f);
        byproductButton.SetActive(true);
    }

    // 3. Linked to Byproduct Button
    public void OnByproductButtonClick()
    {
        // Clean up Plant phase
        waterObject.SetActive(false);
        carbonParticles.SetActive(false);
        plantLabel.SetActive(false);
        
        // Show Byproducts
        oxygenParticles.SetActive(true);
        glucoseParticles.SetActive(true);
        byproductLabel.SetActive(true);
        
        byproductButton.SetActive(false);
    }

    // Helper Functions
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