using UnityEngine;

public class ARUIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject instructionPanel; // Drag your UI panel here in the inspector

    void Start()
    {
        // Keep the panel hidden until the user places the object
        if (instructionPanel != null)
        {
            instructionPanel.SetActive(false);
        }
    }

    // Called automatically by ARPosition when the object drops
    public void ShowInstructions()
    {
        if (instructionPanel != null)
        {
            instructionPanel.SetActive(true);
        }
    }

    // You will link this method to your UI "Back" button
    public void HideInstructions()
    {
        if (instructionPanel != null)
        {
            instructionPanel.SetActive(false);
        }
    }
}