using UnityEngine;
using UnityEngine.Video;

public class TopicLoader : MonoBehaviour
{
    public ContentPanelController panelController;
    public VideoController videoController;

    [TextArea(3, 10)]
    public string notesText;

    public VideoClip videoClip;  // <-- NEW
    public string pdfUrl;

    void OnEnable()
    {
        panelController.videoController = videoController;
        panelController.Bind(videoClip, notesText, pdfUrl);
    }
}
