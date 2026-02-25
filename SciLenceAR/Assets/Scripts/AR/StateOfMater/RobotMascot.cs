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

    [Header("Movement")]
    public Transform[] walkPoints; // Empty objects around the beaker we will walk to
    public float walkSpeed = 1.0f;

    private int currentWaypoint = 0;
    private bool isWalking = false;
    private float idleTimer = 0f;
    public float timeUntilDance = 15f; // Starts dancing after 15 seconds of doing nothing

    private Transform arCamera;

    void Start()
    {
        // Find the AR camera automatically
        arCamera = Camera.main.transform;

        // Start the intro!
        StartCoroutine(IntroSequence());
    }

    void Update()
    {
        // 1. ALWAYS LOOK AT THE USER (If not walking)
        if (!isWalking && arCamera != null)
        {
            Vector3 lookPos = arCamera.position;
            lookPos.y = transform.position.y; // Keep the robot standing straight, no tilting up/down
            transform.LookAt(lookPos);
        }

        // 2. IDLE DANCE LOGIC
        if (!audioSource.isPlaying && !isWalking)
        {
            idleTimer += Time.deltaTime;
            if (idleTimer > timeUntilDance)
            {
                anim.SetInteger("State", 3); // 3 = Dance
                idleTimer = 0f; // Reset timer
            }
        }
        else
        {
            idleTimer = 0f; // Keep resetting the timer if the robot is busy talking or walking
        }
    }

    IEnumerator IntroSequence()
    {
        yield return new WaitForSeconds(1f); // Wait 1 second after spawning
        PlayVoice(introVoice);
    }

    // Our Manager script will call this when the slider moves
    public void ExplainState(int stateIndex)
    {
        StopAllCoroutines(); // Stop whatever the robot was doing
        isWalking = false;

        // Pick the correct audio
        AudioClip clipToPlay = null;
        if (stateIndex == 0) clipToPlay = solidVoice;
        if (stateIndex == 1) clipToPlay = liquidVoice;
        if (stateIndex == 2) clipToPlay = gasVoice;

        // Walk to a new spot, then start explaining!
        StartCoroutine(WalkThenTalk(clipToPlay));
    }

    IEnumerator WalkThenTalk(AudioClip clip)
    {
        // If we set up walk points, let's walk!
        if (walkPoints.Length > 0)
        {
            isWalking = true;
            anim.SetInteger("State", 2); // 2 = Walk

            // Pick the next spot around the box
            currentWaypoint = (currentWaypoint + 1) % walkPoints.Length;
            Transform target = walkPoints[currentWaypoint];

            // Move towards the target spot
            while (Vector3.Distance(transform.position, target.position) > 0.1f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target.position, walkSpeed * Time.deltaTime);

                // Look where we are walking
                Vector3 walkLook = target.position;
                walkLook.y = transform.position.y;
                transform.LookAt(walkLook);

                yield return null; // Wait for next frame
            }
            isWalking = false;
        }

        // We arrived! Now look at the user and talk.
        PlayVoice(clip);
    }

    void PlayVoice(AudioClip clip)
    {
        if (clip == null) return;

        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.Play();

        anim.SetInteger("State", 1); // 1 = Talk

        CancelInvoke("StopTalking");
        Invoke("StopTalking", clip.length);
    }

    void StopTalking()
    {
        anim.SetInteger("State", 0); // 0 = Idle
    }
}