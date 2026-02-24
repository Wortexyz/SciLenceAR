using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatesOfMatterManager : MonoBehaviour
{
    [Header("UI & Text")]
    public TextMeshPro stateLabel;

    [Header("Mascot & Audio")]
    public AudioSource mascotAudio;
    public AudioClip solidAudio;
    public AudioClip liquidAudio;
    public AudioClip gasAudio;

    [Header("Molecules")]
    public Rigidbody[] molecules;

    private Slider tempSlider;
    private int currentState = -1;

    void Start()
    {
        tempSlider = GameObject.Find("TemperatureSlider")?.GetComponent<Slider>();

        if (tempSlider != null)
        {
            tempSlider.onValueChanged.AddListener(delegate { CheckTemperature(); });
            tempSlider.value = 0;
            CheckTemperature();
        }
    }

    public void CheckTemperature()
    {
        float temp = tempSlider.value;

        // SOLID (0 - 30)
        if (temp <= 30 && currentState != 0)
        {
            currentState = 0;
            if (stateLabel != null) stateLabel.text = "SOLID";
            PlayAudio(solidAudio);
            MakeSolid();
        }
        // LIQUID (31 - 70)
        else if (temp > 30 && temp <= 70 && currentState != 1)
        {
            currentState = 1;
            if (stateLabel != null) stateLabel.text = "LIQUID";
            PlayAudio(liquidAudio);
            MakeLiquid();
        }
        // GAS (71 - 100)
        else if (temp > 70 && currentState != 2)
        {
            currentState = 2;
            if (stateLabel != null) stateLabel.text = "GAS";
            PlayAudio(gasAudio);
            MakeGas();
        }
    }

    void PlayAudio(AudioClip clip)
    {
        if (clip != null && mascotAudio != null)
        {
            mascotAudio.Stop();
            mascotAudio.clip = clip;
            mascotAudio.Play();
        }
    }

    void MakeSolid()
    {
        foreach (Rigidbody rb in molecules)
        {
            rb.isKinematic = true;
        }
    }

    void MakeLiquid()
    {
        foreach (Rigidbody rb in molecules)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.drag = 0.5f;
        }
    }

    void MakeGas()
    {
        foreach (Rigidbody rb in molecules)
        {
            rb.isKinematic = false;
            rb.useGravity = false;
            rb.drag = 0f;

            // Much smaller initial push so they don't explode through the walls
            Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), Random.Range(0.5f, 1f), Random.Range(-1f, 1f)).normalized;
            rb.AddForce(randomDir * 1.5f, ForceMode.Impulse);
        }
    }

    void FixedUpdate()
    {
        // Add Device Tilt Sloshing ONLY during the Liquid state
        if (currentState == 1)
        {
            Vector3 tilt = Input.acceleration;
            Vector3 tiltForce = new Vector3(tilt.x, 0, tilt.y) * 15f;

            foreach (Rigidbody rb in molecules)
            {
                rb.AddForce(tiltForce, ForceMode.Acceleration);
            }
        }

        // Keep Gas particles bouncing FOREVER, but SAFELY
        if (currentState == 2)
        {
            float maxSpeed = 1.5f; // The strict speed limit!

            foreach (Rigidbody rb in molecules)
            {
                // 1. Give them a tiny gentle push if they slow down too much
                if (rb.velocity.magnitude < 0.5f)
                {
                    Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
                    rb.AddForce(randomDir * 0.2f, ForceMode.Impulse);
                }

                // 2. NEW: If they try to go faster than our maxSpeed, force them to slow down
                if (rb.velocity.magnitude > maxSpeed)
                {
                    rb.velocity = rb.velocity.normalized * maxSpeed;
                }
            }
        }
    }
}