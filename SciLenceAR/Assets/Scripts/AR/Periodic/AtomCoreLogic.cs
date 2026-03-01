using UnityEngine;
using TMPro;

public class AtomCoreLogic : MonoBehaviour
{
    [Header("AR Container & Physics")]
    public Transform spawnPoint;
    public GameObject protonPrefab;

    [Header("Holographic 3D Text (World Space)")]
    public TextMeshPro nameText;
    public TextMeshPro atomicNumberText;
    public TextMeshPro massText;
    public TextMeshPro stateText;
    public TextMeshPro stabilityText;

    [Header("Audio")]
    public AudioSource aiVoiceSource;
    public AudioClip explanationClip;

    private int currentProtons = 0;
    private AtomUIManager uiManager;

    // --- TEXTBOOK DATA FOR 30 ELEMENTS ---
    private string[] elementNames = { "EMPTY", "HYDROGEN (H)", "HELIUM (He)", "LITHIUM (Li)", "BERYLLIUM (Be)", "BORON (B)", "CARBON (C)", "NITROGEN (N)", "OXYGEN (O)", "FLUORINE (F)", "NEON (Ne)", "SODIUM (Na)", "MAGNESIUM (Mg)", "ALUMINUM (Al)", "SILICON (Si)", "PHOSPHORUS (P)", "SULFUR (S)", "CHLORINE (Cl)", "ARGON (Ar)", "POTASSIUM (K)", "CALCIUM (Ca)", "SCANDIUM (Sc)", "TITANIUM (Ti)", "VANADIUM (V)", "CHROMIUM (Cr)", "MANGANESE (Mn)", "IRON (Fe)", "COBALT (Co)", "NICKEL (Ni)", "COPPER (Cu)", "ZINC (Zn)" };
    private string[] elementMasses = { "0", "1.008", "4.002", "6.94", "9.012", "10.81", "12.011", "14.007", "15.999", "18.998", "20.180", "22.990", "24.305", "26.982", "28.085", "30.974", "32.06", "35.45", "39.948", "39.098", "40.078", "44.956", "47.867", "50.942", "51.996", "54.938", "55.845", "58.933", "58.693", "63.546", "65.38" };
    private string[] elementStates = { "NONE", "GAS", "GAS", "SOLID", "SOLID", "SOLID", "SOLID", "GAS", "GAS", "GAS", "GAS", "SOLID", "SOLID", "SOLID", "SOLID", "SOLID", "SOLID", "GAS", "GAS", "SOLID", "SOLID", "SOLID", "SOLID", "SOLID", "SOLID", "SOLID", "SOLID", "SOLID", "SOLID", "SOLID", "SOLID" };

    void Start()
    {
        uiManager = FindObjectOfType<AtomUIManager>();
        if (uiManager != null) uiManager.ShowPanel();

        UpdateHologram();
    }

    public void InjectProton()
    {
        if (currentProtons < 30)
        {
            currentProtons++;

            if (protonPrefab != null && spawnPoint != null)
            {
                // Removed the random offset. Now it drops dead-straight from the spawn point!
                Instantiate(protonPrefab, spawnPoint.position, Quaternion.identity, transform);
            }

            UpdateHologram();
        }
    }

    public void PlayAIExplanation()
    {
        if (aiVoiceSource != null && explanationClip != null)
        {
            aiVoiceSource.Stop();
            aiVoiceSource.clip = explanationClip;
            aiVoiceSource.Play();
        }
    }

    private void UpdateHologram()
    {
        if (atomicNumberText != null) atomicNumberText.text = "ATOMIC NUMBER: " + currentProtons;
        if (nameText != null) nameText.text = elementNames[currentProtons];
        if (massText != null) massText.text = "MASS: " + elementMasses[currentProtons];
        if (stateText != null) stateText.text = "STATE: " + elementStates[currentProtons];

        if (stabilityText != null)
        {
            if (currentProtons == 0)
            {
                stabilityText.text = "STATUS: EMPTY";
                stabilityText.color = Color.gray;
            }
            else
            {
                stabilityText.text = "STATUS: STABLE";
                stabilityText.color = Color.green;
            }
        }
    }
}