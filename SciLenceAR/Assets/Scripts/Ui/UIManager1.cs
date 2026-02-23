using UnityEngine;
using UnityEngine.UI;

public partial class UIManager1 : MonoBehaviour
{
    [Header("Assign Images in Order")]
    public GameObject[] images;

    // Call this function from your Buttons
    public void ShowImage(int index)
    {
        // First, hide all images to ensure only one shows at a time
        for (int i = 0; i < images.Length; i++)
        {
            images[i].SetActive(false);
        }

        // Check if the index is valid, then show the specific image
        if (index >= 0 && index < images.Length)
        {
            images[index].SetActive(true);
        }
        else
        {
            Debug.LogWarning("Index out of bounds! Check your button settings.");
        }
    }
}