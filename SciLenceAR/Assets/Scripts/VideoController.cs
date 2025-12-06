using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoController : MonoBehaviour
{
    public VideoPlayer videoPlayer;

    [HideInInspector] public RawImage displayRawImage;
    [HideInInspector] public Slider seekSlider;
    [HideInInspector] public Button playPauseButton;

    private bool isDragging = false;
    private bool isPrepared = false;
    private bool listenersAssigned = false;

    void Awake()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        videoPlayer.source = VideoSource.VideoClip;
    }

    void Update()
    {
        if (isPrepared && videoPlayer.isPlaying && !isDragging && videoPlayer.clip != null)
        {
            seekSlider.value = (float)(videoPlayer.time / videoPlayer.length);
        }
    }

    public void LoadAndPlay(VideoClip clip)
    {
        if (!listenersAssigned)
        {
            AssignListeners();
            listenersAssigned = true;
        }

        if (clip == null)
        {
            Debug.LogWarning("VideoController: No clip assigned");
            return;
        }

        isPrepared = false;

        videoPlayer.Stop();
        videoPlayer.clip = clip;

        videoPlayer.prepareCompleted -= OnPrepared;
        videoPlayer.prepareCompleted += OnPrepared;

        videoPlayer.Prepare();
    }

    void AssignListeners()
    {
        playPauseButton.onClick.AddListener(TogglePlayPause);
        seekSlider.onValueChanged.AddListener(OnSliderChanged);
    }

    void OnPrepared(VideoPlayer vp)
    {
        isPrepared = true;
        vp.Play();
    }

    void TogglePlayPause()
    {
        if (!isPrepared) return;

        if (videoPlayer.isPlaying)
            videoPlayer.Pause();
        else
            videoPlayer.Play();
    }

    void OnSliderChanged(float value)
    {
        if (!isPrepared) return;

        isDragging = true;
        videoPlayer.time = value * videoPlayer.length;
        isDragging = false;
    }

    public void StopVideo()
    {
        videoPlayer.Stop();
    }
}
