// File: Assets/Scripts/UI/UIManager.cs
// FINAL UPDATED VERSION for SciLenceAR
// Supports InputField + TMP_Text, fixed Reset Password navigation, no duplicates.

using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.Networking;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager I;
    public static UIManager Instance => I;

    [Header("Auth Panels")]
    public GameObject homePanel;
    public GameObject loginPanel;
    public GameObject registerPanel;

    [Header("Reset Password")]
    public GameObject resetPasswordPanel;
    public InputField resetEmailField;       
    public Button resetSubmitButton;
    public TMP_Text resetStatusText;

    [Header("Panels (General)")]
    public GameObject subjectPanel;
    public GameObject physicsPanel;
    public GameObject chemistryPanel;
    public GameObject biologyPanel;
    public GameObject ProfilePannel;

    [Header("Video UI")]
    public GameObject videoModal;
    public VideoPlayer videoPlayer;
    public RawImage videoRawImage;
    public Text videoTitleText;
    public Button videoCloseButton;

    [Header("Notes UI")]
    public GameObject notesModal;
    public Text notesText;
    public Button downloadPdfButton;
    public Button markNotesReadButton;

    [Header("AR/Other")]
    public GameObject arRunningPanel;

  

    void Awake()
    {
       
        I = this;

        

        if (arRunningPanel) arRunningPanel.SetActive(false);

       
        if (homePanel) homePanel.SetActive(true);

        if (subjectPanel) subjectPanel.SetActive(false);
        if (physicsPanel) physicsPanel.SetActive(false);
        if (chemistryPanel) chemistryPanel.SetActive(false);
        if (biologyPanel) biologyPanel.SetActive(false);
        if (resetPasswordPanel) resetPasswordPanel.SetActive(false);
        if (ProfilePannel) ProfilePannel.SetActive(false);
    }

    // -------------------------------------------------------
    // RESET PASSWORD PANEL LOGIC
    // -------------------------------------------------------

    public void OpenResetPasswordPanel()
    {
        if (homePanel) homePanel.SetActive(false);
        if (loginPanel) loginPanel.SetActive(false);
        if (registerPanel) registerPanel.SetActive(false);

        if (resetPasswordPanel) resetPasswordPanel.SetActive(true);

        if (resetStatusText) resetStatusText.text = "";
    }

    public void OpenResetPasswordPanelFromLogin()
    {
        if (loginPanel) loginPanel.SetActive(false);
        if (homePanel) homePanel.SetActive(false);
        if (registerPanel) registerPanel.SetActive(false);

        if (resetPasswordPanel) resetPasswordPanel.SetActive(true);
        if (resetStatusText) resetStatusText.text = "";
    }

    public void CloseResetPasswordPanel()
    {
        if (resetPasswordPanel) resetPasswordPanel.SetActive(false);
        if (homePanel) homePanel.SetActive(true);

        if (resetStatusText) resetStatusText.text = "";
    }

    public void BackToLoginFromReset()
    {
        if (resetPasswordPanel) resetPasswordPanel.SetActive(false);

        if (loginPanel) loginPanel.SetActive(true);
        else if (homePanel) homePanel.SetActive(true);

        if (resetStatusText) resetStatusText.text = "";
    }

    // -------------------------------------------------------
    // HOME & SUBJECT PANELS
    // -------------------------------------------------------

    public void OpenHomePanel()
    {
        if (homePanel) homePanel.SetActive(true);

        if (subjectPanel) subjectPanel.SetActive(false);
        if (physicsPanel) physicsPanel.SetActive(false);
        if (chemistryPanel) chemistryPanel.SetActive(false);
        if (biologyPanel) biologyPanel.SetActive(false);

        if (videoModal) videoModal.SetActive(false);
        if (notesModal) notesModal.SetActive(false);
    }

    public void OpenSubjectPanel()
    {
        if (subjectPanel) subjectPanel.SetActive(true);
        if (physicsPanel) physicsPanel.SetActive(false);
        if (chemistryPanel) chemistryPanel.SetActive(false);
        if (biologyPanel) biologyPanel.SetActive(false);

        if (homePanel) homePanel.SetActive(false);
    }

    public void OpenSubjectPanel(string subject)
    {
        if (subjectPanel) subjectPanel.SetActive(false);

        if (physicsPanel) physicsPanel.SetActive(subject.ToLower() == "physics");
        if (chemistryPanel) chemistryPanel.SetActive(subject.ToLower() == "chemistry");
        if (biologyPanel) biologyPanel.SetActive(subject.ToLower() == "biology");

        if (homePanel) homePanel.SetActive(false);
    }

    public void BackToSubjectList()
    {
        if (subjectPanel) subjectPanel.SetActive(true);
        if (physicsPanel) physicsPanel.SetActive(false);
        if (chemistryPanel) chemistryPanel.SetActive(false);
        if (biologyPanel) biologyPanel.SetActive(false);
    }

    // -------------------------------------------------------
    // PROFILE PANEL - Modified to ensure Singleton Persistence
    // -------------------------------------------------------

    public void OpenProfilePannel()
    {
        
        if (I.ProfilePannel) I.ProfilePannel.SetActive(true);
        if (I.subjectPanel) I.subjectPanel.SetActive(false);

        var pm = FindObjectOfType<ProfileManager>();
        if (pm != null) pm.RefreshProfile();
    }

    public void closeProfilePannel()
    {
        if (I.ProfilePannel) I.ProfilePannel.SetActive(false);
        if (I.subjectPanel) I.subjectPanel.SetActive(true);
    }

    // -------------------------------------------------------
    // VIDEO / NOTES / AR (unchanged)
    // -------------------------------------------------------

    async Task<bool> DownloadFileAsync(string url, string localPath)
    {
        try { Directory.CreateDirectory(Path.GetDirectoryName(localPath)); } catch { }

        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            req.timeout = 60;
            var op = req.SendWebRequest();

            while (!op.isDone)
                await Task.Delay(100);

            if (req.result == UnityWebRequest.Result.Success)
            {
                File.WriteAllBytes(localPath, req.downloadHandler.data);
                return true;
            }
        }
        return false;
    }
}