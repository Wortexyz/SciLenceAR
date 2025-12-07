using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public AudioSource audioSource;

    [HideInInspector] public RawImage displayRawImage;
    [HideInInspector] public Slider seekSlider;
    [HideInInspector] public Button playPauseButton;

    private bool isPrepared = false;
    private bool listenersAssigned = false;

    void Awake()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.playOnAwake = false;
        videoPlayer.waitForFirstFrame = false;

        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.controlledAudioTrackCount = 1;
        videoPlayer.EnableAudioTrack(0, true);
        videoPlayer.SetTargetAudioSource(0, audioSource);
    }

    void Update()
    {
        if (!isPrepared || videoPlayer.length <= 0)
            return;

        if (seekSlider != null)
            seekSlider.SetValueWithoutNotify((float)(videoPlayer.time / videoPlayer.length));
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
            Debug.LogError("VideoController: VideoClip is NULL");
            return;
        }

        isPrepared = false;

        videoPlayer.Stop();
        videoPlayer.clip = clip;

        videoPlayer.prepareCompleted -= OnPrepared;
        videoPlayer.prepareCompleted += OnPrepared;
        videoPlayer.errorReceived -= OnError;
        videoPlayer.errorReceived += OnError;

        videoPlayer.Prepare();
    }

    void AssignListeners()
    {
        if (playPauseButton != null)
            playPauseButton.onClick.AddListener(TogglePlayPause);

        if (seekSlider != null)
        {
            seekSlider.minValue = 0f;
            seekSlider.maxValue = 1f;
            seekSlider.onValueChanged.AddListener(Seek);
        }
    }

    void OnPrepared(VideoPlayer vp)
    {
        isPrepared = true;
        vp.Play();
    }

    void Seek(float value)
    {
        if (!isPrepared || videoPlayer.length <= 0)
            return;

        double targetTime = value * videoPlayer.length;

        if (videoPlayer.canSetTime)
        {
            videoPlayer.time = targetTime;
        }
        else
        {
            Debug.LogError("This video CANNOT SEEK. Re-encode the video.");
        }
    }

    void TogglePlayPause()
    {
        if (!isPrepared)
            return;

        if (videoPlayer.isPlaying)
            videoPlayer.Pause();
        else
            videoPlayer.Play();
    }

    void OnError(VideoPlayer vp, string msg)
    {
        Debug.LogError("VIDEO ERROR: " + msg);
    }

    public void StopVideo()
    {
        if (videoPlayer != null)
            videoPlayer.Stop();
    }
}
