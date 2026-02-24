using UnityEngine;
using UnityEngine.SceneManagement;

public class ARBackHandler : MonoBehaviour
{
    [Header("Settings")]
    public string homeSceneName = "HomeScene";

    [Header("UI References")]
    public GameObject confirmationPanel; // The "Are you sure?" popup

    void Start()
    {
        // Make sure the popup is hidden when the AR scene starts
        if (confirmationPanel != null)
        {
            confirmationPanel.SetActive(false);
        }
    }
    // suiii

    void Update()
    {
        // KeyCode.Escape detects the mobile phone's physical back button or swipe-back gesture
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // If the panel is NOT showing, show it.
            if (!confirmationPanel.activeSelf)
            {
                confirmationPanel.SetActive(true);
            }
            // If the panel IS showing, pressing back again will hide it (cancel)
            else
            {
                confirmationPanel.SetActive(false);
            }
        }
    }

    // Link this to the "YES" button on your popup panel
    public void ConfirmExit()
    {
        SceneManager.LoadScene(homeSceneName);
    }

    // Link this to the "NO" button on your popup panel
    public void CancelExit()
    {
        confirmationPanel.SetActive(false);
    }
}