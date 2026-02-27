using UnityEngine;
using System.Collections;

public class NewtonsLawsManager : MonoBehaviour
{
    [Header("Tweakable Forces (Change in Inspector!)")]
    public float skateboardPower = 1.5f;
    public float kickPower = 2.0f;

    [Tooltip("How fast the balloon goes forward (+X)")]
    public float balloonForwardPower = 1.0f;
    [Tooltip("How high the balloon flies (+Y)")]
    public float balloonUpPower = 0.2f;

    [Header("The 3 Setups")]
    public GameObject law1Setup;
    public GameObject law2Setup;
    public GameObject law3Setup;

    [Header("Law 1 Physics (Inertia)")]
    public Rigidbody skateboardRB;
    public Rigidbody crateRB;
    private Vector3 startPosSkateboard;
    private Vector3 startPosCrate;

    [Header("Law 2 Physics (F=ma)")]
    public Rigidbody soccerBallRB;
    public Rigidbody bowlingBallRB;
    private Vector3 startPosSoccer;
    private Vector3 startPosBowling;

    [Header("Law 3 Physics (Action/Reaction)")]
    public Rigidbody balloonRB;
    public ParticleSystem airParticles;
    private Vector3 startPosBalloon;

    [Header("AI Voice System")]
    public AudioSource aiVoiceSource;
    public AudioClip law1Audio;
    public AudioClip law2Audio;
    public AudioClip law3Audio;

    private int currentLaw = 1;
    private NewtonsUIManager uiManager;

    void Start()
    {
        uiManager = FindObjectOfType<NewtonsUIManager>();

        if (uiManager != null) uiManager.ShowPanel();

        // Save starting positions
        if (skateboardRB) startPosSkateboard = skateboardRB.transform.localPosition;
        if (crateRB) startPosCrate = crateRB.transform.localPosition;
        if (soccerBallRB) startPosSoccer = soccerBallRB.transform.localPosition;
        if (bowlingBallRB) startPosBowling = bowlingBallRB.transform.localPosition;
        if (balloonRB) startPosBalloon = balloonRB.transform.localPosition;

        SwitchToLaw(1);
    }

    public void SwitchToLaw(int lawNumber)
    {
        currentLaw = lawNumber;
        StopAllCoroutines();
        aiVoiceSource.Stop();

        if (uiManager != null) uiManager.UpdateSubtitle("[ AI ]: Select 'RUN EXPERIMENT' to begin the simulation.");

        ResetVelocities();

        law1Setup.SetActive(false);
        law2Setup.SetActive(false);
        law3Setup.SetActive(false);

        if (lawNumber == 1)
        {
            law1Setup.SetActive(true);
            skateboardRB.transform.localPosition = startPosSkateboard;
            crateRB.transform.localPosition = startPosCrate;
            if (uiManager != null) uiManager.UpdateRunButtonText("PUSH SKATEBOARD");
        }
        else if (lawNumber == 2)
        {
            law2Setup.SetActive(true);
            soccerBallRB.transform.localPosition = startPosSoccer;
            bowlingBallRB.transform.localPosition = startPosBowling;
            if (uiManager != null) uiManager.UpdateRunButtonText("KICK BALLS");
        }
        else if (lawNumber == 3)
        {
            law3Setup.SetActive(true);
            balloonRB.transform.localPosition = startPosBalloon;
            if (airParticles != null) airParticles.Stop();
            if (uiManager != null) uiManager.UpdateRunButtonText("RELEASE BALLOON");
        }
    }

    public void RunCurrentExperiment()
    {
        if (currentLaw == 1) StartCoroutine(ExecuteLaw1());
        if (currentLaw == 2) StartCoroutine(ExecuteLaw2());
        if (currentLaw == 3) StartCoroutine(ExecuteLaw3());
    }

    IEnumerator ExecuteLaw1()
    {
        PlayAIVoice(law1Audio, "Law of Inertia: An object in motion stays in motion. The rock stops the board, but the crate's inertia keeps it moving!");

        // Pushes exactly in the +X direction of the AR Setup
        Vector3 pushDirection = transform.right;
        skateboardRB.AddForce(pushDirection * skateboardPower, ForceMode.Impulse);
        yield return null;
    }

    IEnumerator ExecuteLaw2()
    {
        PlayAIVoice(law2Audio, "F = ma: We apply the exact same force, but the heavier bowling ball accelerates much slower!");

        Vector3 pushDirection = transform.right;
        soccerBallRB.AddForce(pushDirection * kickPower, ForceMode.Impulse);
        bowlingBallRB.AddForce(pushDirection * kickPower, ForceMode.Impulse);
        yield return null;
    }

    IEnumerator ExecuteLaw3()
    {
        PlayAIVoice(law3Audio, "Action & Reaction: The air shoots backward (action), pushing the balloon forward (reaction)!");
        if (airParticles != null) airParticles.Play();

        balloonRB.useGravity = false; // Turn off world gravity

        // Push forward (+X) AND slightly up (+Y) based on your Inspector settings!
        Vector3 flyDirection = (transform.right * balloonForwardPower) + (transform.up * balloonUpPower);
        balloonRB.AddForce(flyDirection, ForceMode.Impulse);

        yield return new WaitForSeconds(3f);

        if (airParticles != null) airParticles.Stop();
        balloonRB.useGravity = true; // Drop back to the table
    }

    void PlayAIVoice(AudioClip clip, string subtitle)
    {
        if (clip != null)
        {
            aiVoiceSource.Stop();
            aiVoiceSource.clip = clip;
            aiVoiceSource.Play();
        }
        if (uiManager != null) uiManager.UpdateSubtitle("[ AI ]: " + subtitle);
    }

    void ResetVelocities()
    {
        if (skateboardRB) { skateboardRB.velocity = Vector3.zero; skateboardRB.angularVelocity = Vector3.zero; }
        if (crateRB) { crateRB.velocity = Vector3.zero; crateRB.angularVelocity = Vector3.zero; }
        if (soccerBallRB) { soccerBallRB.velocity = Vector3.zero; soccerBallRB.angularVelocity = Vector3.zero; }
        if (bowlingBallRB) { bowlingBallRB.velocity = Vector3.zero; bowlingBallRB.angularVelocity = Vector3.zero; }
        if (balloonRB) { balloonRB.velocity = Vector3.zero; balloonRB.angularVelocity = Vector3.zero; }
    }
}