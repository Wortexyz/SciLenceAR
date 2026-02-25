using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatesOfMatterManager : MonoBehaviour
{
    [Header("UI & Text")]
    public TextMeshPro stateLabel;

    [Header("SciLenceAR Assistant")]
    public RobotMascot myRobot; // NEW: Links to the robot's brain

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

            if (myRobot != null) myRobot.ExplainState(0); // Tell Robot to talk about Solid
            MakeSolid();
        }
        // LIQUID (31 - 70)
        else if (temp > 30 && temp <= 70 && currentState != 1)
        {
            currentState = 1;
            if (stateLabel != null) stateLabel.text = "LIQUID";

            if (myRobot != null) myRobot.ExplainState(1); // Tell Robot to talk about Liquid
            MakeLiquid();
        }
        // GAS (71 - 100)
        else if (temp > 70 && currentState != 2)
        {
            currentState = 2;
            if (stateLabel != null) stateLabel.text = "GAS";

            if (myRobot != null) myRobot.ExplainState(2); // Tell Robot to talk about Gas
            MakeGas();
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

            Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), Random.Range(0.5f, 1f), Random.Range(-1f, 1f)).normalized;
            rb.AddForce(randomDir * 1.5f, ForceMode.Impulse);
        }
    }

    void FixedUpdate()
    {
        if (currentState == 1)
        {
            Vector3 tilt = Input.acceleration;
            Vector3 tiltForce = new Vector3(tilt.x, 0, tilt.y) * 15f;
            foreach (Rigidbody rb in molecules) rb.AddForce(tiltForce, ForceMode.Acceleration);
        }

        if (currentState == 2)
        {
            float maxSpeed = 1.5f;
            foreach (Rigidbody rb in molecules)
            {
                if (rb.velocity.magnitude < 0.5f)
                {
                    Vector3 randomDir = new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
                    rb.AddForce(randomDir * 0.2f, ForceMode.Impulse);
                }
                if (rb.velocity.magnitude > maxSpeed)
                {
                    rb.velocity = rb.velocity.normalized * maxSpeed;
                }
            }
        }
    }
}