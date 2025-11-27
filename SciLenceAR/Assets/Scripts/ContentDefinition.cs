// Assets/Scripts/ContentDefinition.cs
using UnityEngine;

public enum VideoSourceType { DirectMp4, YouTube, None }

[CreateAssetMenu(menuName = "SciLenceAR/ContentDefinition", fileName = "NewContent")]
public class ContentDefinition : ScriptableObject
{
    public string contentId;         // e.g., "phy_gravity_01"
    public string subject;           // "Physics"
    public string title;
    [TextArea(6, 20)] public string notes;

    [Header("Video")]
    public VideoSourceType videoSource = VideoSourceType.DirectMp4;
    [Tooltip("If DirectMp4: full HTTPS to .mp4 (or Google Drive converted link). If YouTube: video id or url.")]
    public string videoUrlOrYouTubeId;

    [Header("Notes / PDF")]
    [Tooltip("Direct download link to PDF (e.g., Google Drive uc?export=download&id=...)")]
    public string pdfUrl;

    [Header("AR")]
    public GameObject arPrefab;
}
