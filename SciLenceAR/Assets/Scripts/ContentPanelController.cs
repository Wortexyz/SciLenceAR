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

    public void Bind(VideoClip clip, string notes, string pdfUrl)
    {
        notesText.text = notes;

        pdfButton.onClick.RemoveAllListeners();
        pdfButton.onClick.AddListener(() => Application.OpenURL(pdfUrl));

        arButton.onClick.RemoveAllListeners();
        arButton.onClick.AddListener(() => Debug.Log("AR Button pressed"));

        videoController.displayRawImage = rawImage;
        videoController.seekSlider = seekSlider;
        videoController.playPauseButton = playPauseButton;

        videoController.LoadAndPlay(clip);
    }

}
