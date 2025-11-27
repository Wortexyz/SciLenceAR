// File: Assets/Scripts/UI/UIManager.cs
// Updated UIManager for SciLenceAR - adds Instance alias for compatibility.

using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.Networking;

public class UIManager : MonoBehaviour
{
    public static UIManager I;
    // Compatibility alias used by some other scripts in the project.
    public static UIManager Instance => I;

    [Header("Panels (General)")]
    public GameObject subjectPanel;
    public GameObject physicsPanel;
    public GameObject chemistryPanel;
    public GameObject biologyPanel;

    [Header("Video UI")]
    public GameObject videoModal;       // panel which contains the Video UI
    public VideoPlayer videoPlayer;     // VideoPlayer component
    public RawImage videoRawImage;      // RawImage showing the RenderTexture
    public Text videoTitleText;         // title shown above video
    public Button videoCloseButton;     // button to close video

    [Header("Notes UI")]
    public GameObject notesModal;       // notes panel
    public Text notesText;              // text area in notes modal
    public Button downloadPdfButton;    // button to download/open PDF
    public Button markNotesReadButton;  // button to mark notes as read

    [Header("AR/Other")]
    public GameObject arRunningPanel;   // overlay shown when AR is running (Exit button should call ARFlowManager.ExitAR)

    // Internal state
    ContentDefinition currentDef;

    void Awake()
    {
        if (I == null) { I = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        // hide modals by default
        if (videoModal) videoModal.SetActive(false);
        if (notesModal) notesModal.SetActive(false);
        if (arRunningPanel) arRunningPanel.SetActive(false);

        // wire close button (safety: only if assigned)
        if (videoCloseButton != null) videoCloseButton.onClick.AddListener(CloseVideo);
    }

    #region Subject panel helpers
    // Simple methods to wire to subject buttons
    public void ShowPhysics() { ShowSubjectPanel("Physics"); }
    public void ShowChemistry() { ShowSubjectPanel("Chemistry"); }
    public void ShowBiology() { ShowSubjectPanel("Biology"); }

    public void BackToSubjectList()
    {
        if (subjectPanel) subjectPanel.SetActive(true);
        if (physicsPanel) physicsPanel.SetActive(false);
        if (chemistryPanel) chemistryPanel.SetActive(false);
        if (biologyPanel) biologyPanel.SetActive(false);
    }

    void ShowSubjectPanel(string subject)
    {
        if (subjectPanel) subjectPanel.SetActive(false);
        if (physicsPanel) physicsPanel.SetActive(subject.ToLower() == "physics");
        if (chemistryPanel) chemistryPanel.SetActive(subject.ToLower() == "chemistry");
        if (biologyPanel) biologyPanel.SetActive(subject.ToLower() == "biology");
    }
    #endregion

    #region Watch / Video playback (entry point)
    // Called by ContentListManager when user taps Watch
    public async void OnWatchClicked(ContentDefinition def)
    {
        if (def == null) return;
        currentDef = def;
        if (videoTitleText != null) videoTitleText.text = def.title ?? "Video";

        if (def.videoSource == VideoSourceType.DirectMp4 && !string.IsNullOrEmpty(def.videoUrlOrYouTubeId))
        {
            await PlayDirectVideoWithCache(def);
        }
        else if (def.videoSource == VideoSourceType.YouTube)
        {
            // open externally
            string url = def.videoUrlOrYouTubeId;
            if (!url.StartsWith("http")) url = "https://www.youtube.com/watch?v=" + url;
            Application.OpenURL(url);
        }
        else
        {
            Debug.LogWarning("[UIManager] No video configured for " + def.title);
        }
    }

    // Downloads (if needed) and plays a Direct MP4.
    // Uses caching: file saved to Application.persistentDataPath/contentId.mp4
    async Task PlayDirectVideoWithCache(ContentDefinition def)
    {
        string url = def.videoUrlOrYouTubeId;
        string safeFileName = $"{def.contentId}.mp4";
        string localPath = Path.Combine(Application.persistentDataPath, safeFileName);

        // If file not present, download it
        if (!File.Exists(localPath))
        {
            Debug.Log($"[UIManager] Video not cached yet. Downloading from: {url}");
            bool ok = await DownloadFileAsync(url, localPath);
            if (!ok)
            {
                Debug.LogError("[UIManager] Video download failed. Aborting playback.");
                return;
            }
            Debug.Log($"[UIManager] Video downloaded to {localPath}");
        }
        else
        {
            Debug.Log($"[UIManager] Using cached video at {localPath}");
        }

        // Play from local file (use file:// prefix for VideoPlayer)
        string fileUrl = "file://" + localPath;
        await PlayVideoFromUrl(fileUrl, def);
    }

    // Prepares VideoPlayer, restores saved playtime (if any), and starts playback.
    async Task PlayVideoFromUrl(string urlToPlay, ContentDefinition def)
    {
        if (videoPlayer == null)
        {
            Debug.LogError("[UIManager] No VideoPlayer assigned.");
            return;
        }

        // Prepare
        videoPlayer.url = urlToPlay;
        if (videoModal) videoModal.SetActive(true);

        // In case multiple prepareCompleted handlers could be added, remove all first:
        videoPlayer.prepareCompleted -= OnVideoPrepared; // safe even if not added
        videoPlayer.prepareCompleted += OnVideoPrepared;

        // Prepare and wait until prepared (we'll get the rest in event handler)
        videoPlayer.Prepare();

        // Restore progress asynchronously and apply after prepare is complete.
        ProgressManager pm = FindObjectOfType<ProgressManager>();
        if (pm != null)
        {
            try
            {
                var prog = await pm.GetProgressAsync(def.contentId);
                _pendingProgress = prog;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning("[UIManager] Failed to get progress: " + ex.Message);
                _pendingProgress = null;
            }
        }
        else
        {
            _pendingProgress = null;
        }
    }

    // Temporary place to store progress fetched before prepare completes
    private UserProgress _pendingProgress = null;

    // Called when VideoPlayer finishes preparing
    void OnVideoPrepared(VideoPlayer vp)
    {
        // Seek to saved time if present
        if (_pendingProgress != null)
        {
            double time = _pendingProgress.lastPlayTime;
            bool wasPlaying = _pendingProgress.lastWasPlaying;
            // Clamp time
            if (double.IsInfinity(vp.length) == false && time > vp.length) time = 0;
            vp.time = time;
            if (wasPlaying) vp.Play(); else vp.Pause();
        }
        else
        {
            // default: play
            vp.Play();
        }

        // clear pending
        _pendingProgress = null;
    }

    // Close video and ensure paused
    public void CloseVideo()
    {
        if (videoPlayer != null && videoPlayer.isPlaying) videoPlayer.Pause();
        if (videoModal != null) videoModal.SetActive(false);
    }
    #endregion

    #region Download helpers (video and PDF)
    // Download a remote file (http/https) to localPath. Returns true if success.
    // Uses UnityWebRequest and awaits completion via Task.
    async Task<bool> DownloadFileAsync(string url, string localPath)
    {
        // create directory if needed
        try { Directory.CreateDirectory(Path.GetDirectoryName(localPath)); } catch { }

        using (UnityWebRequest uwr = UnityWebRequest.Get(url))
        {
            uwr.timeout = 60; // seconds
            var op = uwr.SendWebRequest();

            while (!op.isDone)
            {
                await Task.Delay(100);
            }

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    File.WriteAllBytes(localPath, uwr.downloadHandler.data);
                    return true;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("[UIManager] Failed writing file: " + ex.Message);
                    return false;
                }
            }
            else
            {
                Debug.LogError("[UIManager] Download failed: " + uwr.error + "  URL: " + url);
                return false;
            }
        }
    }

    // Downloads the PDF to persistentDataPath and opens it with system viewer.
    public async void DownloadPdfAndOpen(string pdfUrl, string contentId)
    {
        if (string.IsNullOrEmpty(pdfUrl))
        {
            Debug.LogWarning("[UIManager] No PDF URL provided.");
            return;
        }

        string localPath = Path.Combine(Application.persistentDataPath, contentId + ".pdf");
        if (File.Exists(localPath))
        {
            OpenFile(localPath);
            return;
        }

        bool ok = await DownloadFileAsync(pdfUrl, localPath);
        if (ok) OpenFile(localPath);
    }

    void OpenFile(string path)
    {
        // On mobile this should open external PDF viewer
        Application.OpenURL("file://" + path);
    }
    #endregion

    #region Notes modal handling
    // Called by ContentListManager when user taps Notes
    public void OnNotesClicked(ContentDefinition def)
    {
        if (def == null) return;
        currentDef = def;

        if (notesModal) notesModal.SetActive(true);
        if (notesText != null) notesText.text = def.notes ?? "No notes available.";

        // wire download and mark buttons
        if (downloadPdfButton != null)
        {
            downloadPdfButton.onClick.RemoveAllListeners();
            string pdfUrl = def.pdfUrl;
            string cid = def.contentId;
            downloadPdfButton.onClick.AddListener(() => { DownloadPdfAndOpen(pdfUrl, cid); });
        }

        if (markNotesReadButton != null)
        {
            markNotesReadButton.onClick.RemoveAllListeners();
            markNotesReadButton.onClick.AddListener(async () =>
            {
                var pm = FindObjectOfType<ProgressManager>();
                if (pm != null)
                {
                    await pm.SaveProgressAsync(def.contentId, 0, false, true, def.subject);
                    Debug.Log("[UIManager] Notes marked read for " + def.contentId);
                }
            });
        }
    }

    public void CloseNotes() => notesModal.SetActive(false);
    #endregion

    #region Start AR flow
    // Called by LessonCard Start AR button
    public async void OnStartARClicked(ContentDefinition def)
    {
        if (def == null) return;

        // Save video playback state if currently playing / prepared
        double time = 0;
        bool wasPlaying = false;
        if (videoPlayer != null && videoModal != null && videoModal.activeSelf)
        {
            try
            {
                time = videoPlayer.time;
                wasPlaying = videoPlayer.isPlaying;
            }
            catch { time = 0; wasPlaying = false; }
        }

        var pm = FindObjectOfType<ProgressManager>();
        if (pm != null)
        {
            await pm.SaveProgressAsync(def.contentId, time, wasPlaying, true, def.subject);
        }

        // hide UI panels to clear screen
        CloseVideo();
        CloseNotes();

        // show AR running overlay if assigned
        if (arRunningPanel != null) arRunningPanel.SetActive(true);

        var ar = FindObjectOfType<ARFlowManager>();
        if (ar != null)
        {
            ar.StartARForContent(def); // user should implement actual AR in ARFlowManager
        }
        else
        {
            Debug.LogWarning("[UIManager] ARFlowManager not found - no AR will start.");
        }
    }

    // Call this when AR ends (ARFlowManager should call it)
    public async void OnARFinished(ContentDefinition def)
    {
        if (arRunningPanel != null) arRunningPanel.SetActive(false);

        // mark completed in progress (optional)
        var pm = FindObjectOfType<ProgressManager>();
        if (pm != null)
        {
            await pm.MarkCompletedAsync(def.contentId);
        }

        // reopen video to restore state (optional)
        OnWatchClicked(def);
    }
    #endregion

    // Compatibility methods: some other scripts call these names.
public void OpenHomePanel()
{
    // same as going back to the subject list/home
    BackToSubjectList();
}

// Open the subject-panel. Called from UI buttons or other scripts.
// Two overloads provided: one without argument and one with subject name.
public void OpenSubjectPanel()
{
    // Show the main subject panel (hide subject-specific content)
    if (subjectPanel) subjectPanel.SetActive(true);
    if (physicsPanel) physicsPanel.SetActive(false);
    if (chemistryPanel) chemistryPanel.SetActive(false);
    if (biologyPanel) biologyPanel.SetActive(false);
}

public void OpenSubjectPanel(string subject)
{
    // forward to existing helper
    ShowSubjectPanel(subject);
}

}
