using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    [SerializeField] private GameObject homePanel;
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private GameObject registrationPanel;
    [SerializeField] private GameObject subjectPanel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Utility to hide all then show one
    private void ShowOnly(GameObject panel)
    {
        if (homePanel) homePanel.SetActive(panel == homePanel);
        if (loginPanel) loginPanel.SetActive(panel == loginPanel);
        if (registrationPanel) registrationPanel.SetActive(panel == registrationPanel);
        if (subjectPanel) subjectPanel.SetActive(panel == subjectPanel);
    }

    public void OpenHomePanel() => ShowOnly(homePanel);
    public void OpenLoginPanel() => ShowOnly(loginPanel);
    public void OpenRegistrationPanel() => ShowOnly(registrationPanel);
    public void OpenSubjectPanel() => ShowOnly(subjectPanel);
}