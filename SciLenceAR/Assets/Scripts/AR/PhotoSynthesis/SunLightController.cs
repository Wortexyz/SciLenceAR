using UnityEngine;
using System.Collections;

public class SunLightController : MonoBehaviour
{
    [Header("References")]
    public GameObject sunLight;
    public GameObject particleSystemA;
    public GameObject particleSystemB;

    private Coroutine sequenceCoroutine;

    // This is the function you will link to your Buttons
    public void OnSunButtonClick()
    {
        // Toggle the light's current state
        bool isLightNowActive = !sunLight.activeSelf;
        sunLight.SetActive(isLightNowActive);

        if (isLightNowActive)
        {
            // Start the timed sequence
            if (sequenceCoroutine != null) StopCoroutine(sequenceCoroutine);
            sequenceCoroutine = StartCoroutine(RunParticleSequence());
        }
        else
        {
            // Shut everything off immediately if the sun is turned off
            if (sequenceCoroutine != null) StopCoroutine(sequenceCoroutine);
            ResetSystems();
        }
    }

    IEnumerator RunParticleSequence()
    {
        // Wait 2 seconds before activating first particle
        yield return new WaitForSeconds(2f);
        particleSystemA.SetActive(true);

        // Wait another 2 seconds before activating second particle
        yield return new WaitForSeconds(2f);
        particleSystemB.SetActive(true);
    }

    void ResetSystems()
    {
        particleSystemA.SetActive(false);
        particleSystemB.SetActive(false);
    }
}