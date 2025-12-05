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
    public InputField resetEmailField;       // Using Unity InputField (your choice)
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

    private ContentDefinition currentDef;
    private UserProgress _pendingProgress = null;

    void Awake()
    {
        if (I == null)
        {
            I = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (videoModal) videoModal.SetActive(false);
        if (notesModal) notesModal.SetActive(false);
        if (arRunningPanel) arRunningPanel.SetActive(false);

        if (homePanel) homePanel.SetActive(false);
        if (subjectPanel) subjectPanel.SetActive(false);
        if (physicsPanel) physicsPanel.SetActive(false);
        if (chemistryPanel) chemistryPanel.SetActive(false);
        if (biologyPanel) biologyPanel.SetActive(false);

        if (resetPasswordPanel) resetPasswordPanel.SetActive(false);

        if (videoCloseButton != null)
            videoCloseButton.onClick.AddListener(CloseVideo);
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
    // PROFILE PANEL
    // -------------------------------------------------------

    public void OpenProfilePannel()
    {
        if (ProfilePannel) ProfilePannel.SetActive(true);
        if (subjectPanel) subjectPanel.SetActive(false);

        var pm = FindObjectOfType<ProfileManager>();
        if (pm != null) pm.RefreshProfile();
    }

    public void closeProfilePannel()
    {
        if (ProfilePannel) ProfilePannel.SetActive(false);
        if (subjectPanel) subjectPanel.SetActive(true);
    }

    // -------------------------------------------------------
    // VIDEO / NOTES / AR (unchanged)
    // -------------------------------------------------------

    public async void OnWatchClicked(ContentDefinition def)
    {
        if (def == null) return;

        currentDef = def;

        if (videoTitleText != null)
            videoTitleText.text = def.title ?? "Video";

        if (def.videoSource == VideoSourceType.DirectMp4)
        {
            await PlayDirectVideoWithCache(def);
        }
        else if (def.videoSource == VideoSourceType.YouTube)
        {
            string url = def.videoUrlOrYouTubeId;
            if (!url.StartsWith("http"))
                url = "https://www.youtube.com/watch?v=" + url;

            Application.OpenURL(url);
        }
    }

    async Task PlayDirectVideoWithCache(ContentDefinition def)
    {
        string url = def.videoUrlOrYouTubeId;
        string safeFileName = $"{def.contentId}.mp4";
        string localPath = Path.Combine(Application.persistentDataPath, safeFileName);

        if (!File.Exists(localPath))
            await DownloadFileAsync(url, localPath);

        string fileUrl = "file://" + localPath;
        await PlayVideoFromUrl(fileUrl, def);
    }

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

    async Task PlayVideoFromUrl(string url, ContentDefinition def)
    {
        if (!videoPlayer) return;

        videoPlayer.url = url;
        if (videoModal) videoModal.SetActive(true);

        videoPlayer.prepareCompleted -= OnVideoPrepared;
        videoPlayer.prepareCompleted += OnVideoPrepared;

        videoPlayer.Prepare();
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        vp.Play();
    }

    public void CloseVideo()
    {
        if (videoPlayer && videoPlayer.isPlaying)
            videoPlayer.Pause();

        if (videoModal)
            videoModal.SetActive(false);
    }
}
