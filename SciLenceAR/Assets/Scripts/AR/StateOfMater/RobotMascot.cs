using UnityEngine;
using System.Collections;

public class RobotMascot : MonoBehaviour
{
    [Header("Components")]
    public Animator anim;
    public AudioSource audioSource;

    [Header("Voice Lines")]
    public AudioClip introVoice;
    public AudioClip solidVoice;
    public AudioClip liquidVoice;
    public AudioClip gasVoice;

    private float idleTimer = 0f;

    [Header("Idle Behaviors")]
    public float timeUntilDance = 15f;
    private bool isDancing = false;

    private Transform arCamera;

    void Start()
    {
        arCamera = Camera.main.transform;
        StartCoroutine(IntroSequence());
    }

    void Update()
    {
        // 1. ALWAYS LOOK AT USER (Unless dancing)
        if (!isDancing && arCamera != null)
        {
            Vector3 lookPos = arCamera.position;
            lookPos.y = transform.position.y;
            transform.LookAt(lookPos);
        }

        // 2. IDLE DANCE BEHAVIOR
        if (!audioSource.isPlaying && !isDancing)
        {
            idleTimer += Time.deltaTime;

            if (idleTimer > timeUntilDance)
            {
                StartCoroutine(DanceRoutine());
            }
        }
        else
        {
            // Reset the timer as long as the robot is busy
            idleTimer = 0f;
        }
    }

    IEnumerator IntroSequence()
    {
        yield return new WaitForSeconds(0.5f);
        PlayVoice(introVoice);
    }

    // --- CALLED BY THE SLIDER MANAGER ---
    public void ExplainState(int stateIndex)
    {
        StopAllCoroutines();
        isDancing = false;
        idleTimer = 0f;

        AudioClip clipToPlay = null;
        if (stateIndex == 0) clipToPlay = solidVoice;
        if (stateIndex == 1) clipToPlay = liquidVoice;
        if (stateIndex == 2) clipToPlay = gasVoice;

        PlayVoice(clipToPlay);
    }

    IEnumerator DanceRoutine()
    {
        isDancing = true;
        anim.SetInteger("State", 3); // 3 = Dance

        yield return new WaitForSeconds(5f); // Dance for 5 seconds

        isDancing = false;
        idleTimer = 0f;
        anim.SetInteger("State", 0); // Back to Idle
    }

    // --- AUDIO & ANIMATION HELPERS ---
    void PlayVoice(AudioClip clip)
    {
        if (clip == null) return;

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();

        // Start the quick talking gesture
        StartCoroutine(TalkOnceRoutine());
    }

    IEnumerator TalkOnceRoutine()
    {
        anim.SetInteger("State", 1); // 1 = Talk

        // Wait exactly 2.5 seconds (a nice short gesture)
        // It does NOT wait for the audio to finish!
        yield return new WaitForSeconds(2.5f);

        anim.SetInteger("State", 0); // Go back to Idle (0) while audio keeps playing
    }
}