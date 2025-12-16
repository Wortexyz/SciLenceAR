using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class ContentPanelController : MonoBehaviour
{
    [Header("UI Elements")]
    public RawImage rawImage;
    public Slider seekSlider;
    public Button playPauseButton;
    public TMP_Text notesText;
    public Button pdfButton;
    public Button arButton;

    [HideInInspector]
    public VideoController videoController;

    public PDFDownloader pdfDownloader;

    public void Bind(VideoClip clip, string notes, string pdfUrl)
    {
        // Assign notes
        if (notesText != null)
            notesText.text = notes;

        // PDF button binding with per-topic filename
        pdfButton.onClick.RemoveAllListeners();
        pdfButton.onClick.AddListener(() =>
        {
            if (pdfDownloader != null && !string.IsNullOrEmpty(pdfUrl) && clip != null)
            {
                string safeFileName = clip.name + ".pdf";
                pdfDownloader.DownloadPDF(pdfUrl, safeFileName);
            }
            else
            {
                Debug.LogError("PDFDownloader, PDF URL, or VideoClip is missing");
            }
        });

        // AR button placeholder
        arButton.onClick.RemoveAllListeners();
        arButton.onClick.AddListener(() =>
        {
            Debug.Log("AR Button Pressed");
        });

        // Connect UI to VideoController
        if (videoController != null)
        {
            videoController.displayRawImage = rawImage;
            videoController.seekSlider = seekSlider;
            videoController.playPauseButton = playPauseButton;

            videoController.LoadAndPlay(clip);
        }
        else
        {
            Debug.LogError("VideoController is NULL in ContentPanelController");
        }
    }
}
