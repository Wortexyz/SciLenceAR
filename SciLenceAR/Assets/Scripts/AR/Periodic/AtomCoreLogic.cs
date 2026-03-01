using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class AtomCoreLogic : MonoBehaviour
{
    [Header("AR Container & Physics")]
    public Transform spawnPoint;
    public GameObject protonPrefab;
    public float floatingSpeed = 1.5f; // The strength of the "Nuclear Glue" pull

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

    // We keep track of the protons here to make them float!
    private List<Rigidbody> activeProtons = new List<Rigidbody>();

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

    void FixedUpdate()
    {
        // THE FREE-FLOATING NUCLEUS LOGIC
        foreach (Rigidbody rb in activeProtons)
        {
            if (rb != null)
            {
                // 1. Random jitter (like your States of Matter gas)
                Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
                rb.AddForce(randomDir * 0.1f, ForceMode.Impulse);

                // 2. The Nuclear Glue: Gently pulls them towards the center spawn point!
                // This stops them from hitting the walls and glitching outside.
                if (spawnPoint != null)
                {
                    Vector3 pullToCenter = (spawnPoint.position - rb.position).normalized;
                    rb.AddForce(pullToCenter * floatingSpeed, ForceMode.Force);
                }

                // Speed limit so they don't go crazy
                if (rb.velocity.magnitude > 0.8f)
                {
                    rb.velocity = rb.velocity.normalized * 0.8f;
                }
            }
        }
    }

    public void InjectProton()
    {
        if (currentProtons < 30)
        {
            currentProtons++;

            if (protonPrefab != null && spawnPoint != null)
            {
                // Add a tiny random offset so they don't spawn perfectly inside each other
                Vector3 randomOffset = Random.insideUnitSphere * 0.05f;
                GameObject newProton = Instantiate(protonPrefab, spawnPoint.position + randomOffset, Quaternion.identity, transform);

                Rigidbody rb = newProton.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    // Force the physics to float instantly!
                    rb.useGravity = false;
                    rb.drag = 0f;
                    activeProtons.Add(rb);
                }
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