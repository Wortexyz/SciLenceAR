using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class AppleGravity : MonoBehaviour
{
    public float gravityValue = -9.81f; // Earth is -9.81, Moon is -1.62

    private Rigidbody rb;
    private Vector3 startLocalPosition;
    private Quaternion startLocalRotation;
    private bool isResetting = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // Turn off Unity's default gravity

        // Save the starting position in the air
        startLocalPosition = transform.localPosition;
        startLocalRotation = transform.localRotation;
    }

    void FixedUpdate()
    {
        if (!isResetting)
        {
            // Apply our custom gravity downwards
            Vector3 customGravityVector = Vector3.up * gravityValue * transform.parent.lossyScale.y;
            rb.AddForce(customGravityVector, ForceMode.Acceleration);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if we hit the floor using the tag we will create
        if (!isResetting && collision.gameObject.CompareTag("SimulationFloor"))
        {
            StartCoroutine(ResetAppleRoutine());
        }
    }

    IEnumerator ResetAppleRoutine()
    {
        isResetting = true;

        // Wait 2 seconds
        yield return new WaitForSeconds(5f);

        // Stop movement and snap back to the sky
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.localPosition = startLocalPosition;
        transform.localRotation = startLocalRotation;

        isResetting = false;
    }
}