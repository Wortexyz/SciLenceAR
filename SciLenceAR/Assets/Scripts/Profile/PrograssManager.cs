using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.UI; 
using TMPro;

public class ProgressManager : MonoBehaviour
{
    FirebaseAuth auth;
    FirebaseFirestore db;

    [Header("UI References")]
    [SerializeField] private SubjectUI physicsUI;
    [SerializeField] private SubjectUI chemistryUI;
    [SerializeField] private SubjectUI biologyUI;
    [SerializeField] private SubjectUI profilePhysicsUI;
    [SerializeField] private SubjectUI profileChemistryUI;
    [SerializeField] private SubjectUI profileBiologyUI;

    [Serializable]
    public struct SubjectUI
    {
        public Image progressBar; 
        public TextMeshProUGUI percentageText; 
    }

    void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
    }

    string Uid
    {
        get
        {
            var u = auth.CurrentUser;
            if (u == null)
            {
                Debug.LogError("No authenticated user. Make sure user signed in.");
                return null;
            }
            return u.UserId;
        }
    }

    // --- NEW BUTTON-READY METHOD (ONE PARAMETER) ---
    // This will definitely show in the dropdown!
    // Enter value in Unity Inspector as: contentId,subject
    public void MarkCompletedForButton(string commaSeparatedInput)
    {
        string[] parts = commaSeparatedInput.Split(',');
        if (parts.Length < 2)
        {
            Debug.LogError("Please enter: contentId,subject (e.g. lesson1,Physics)");
            return;
        }
        
        string cid = parts[0].Trim();
        string sub = parts[1].Trim();
        
        _ = MarkCompletedAsync(cid, sub);
    }

    // --- EXISTING METHODS (UNCHANGED LOGIC) ---

    public async Task SaveProgressAsync(string contentId, double lastPlayTime, bool lastWasPlaying, bool notesRead, string subject = "Unknown", bool completed = false)
    {
        if (Uid == null) return;
        var docRef = db.Collection("users").Document(Uid).Collection("progress").Document(contentId);
        var data = new Dictionary<string, object>
        {
            {"lastPlayTime", lastPlayTime},
            {"lastWasPlaying", lastWasPlaying},
            {"notesRead", notesRead},
            {"completed", completed},
            {"subject", subject},
            {"updatedAt", Timestamp.GetCurrentTimestamp()}
        };
        await docRef.SetAsync(data, SetOptions.MergeAll);
        await UpdateAllSubjectProgressUI();
    }

    public async Task<UserProgress> GetProgressAsync(string contentId)
    {
        if (Uid == null) return null;
        var docRef = db.Collection("users").Document(Uid).Collection("progress").Document(contentId);
        var snapshot = await docRef.GetSnapshotAsync();
        if (!snapshot.Exists) return null;
        var p = new UserProgress();
        p.contentId = contentId;
        p.lastPlayTime = snapshot.ContainsField("lastPlayTime") ? Convert.ToDouble(snapshot.GetValue<double>("lastPlayTime")) : 0;
        p.lastWasPlaying = snapshot.ContainsField("lastWasPlaying") ? snapshot.GetValue<bool>("lastWasPlaying") : false;
        p.notesRead = snapshot.ContainsField("notesRead") ? snapshot.GetValue<bool>("notesRead") : false;
        p.completed = snapshot.ContainsField("completed") ? snapshot.GetValue<bool>("completed") : false;
        return p;
    }

    public async Task MarkCompletedAsync(string contentId, string subject)
    {
        if (Uid == null) return;
        var docRef = db.Collection("users").Document(Uid).Collection("progress").Document(contentId);
        var updates = new Dictionary<string, object>() {
            {"completed", true},
            {"subject", subject}, 
            {"completedAt", Timestamp.GetCurrentTimestamp()},
            {"updatedAt", Timestamp.GetCurrentTimestamp()}
        };
        await docRef.SetAsync(updates, SetOptions.MergeAll);
        await UpdateAllSubjectProgressUI();
    }

    public async Task UpdateAllSubjectProgressUI()
    {
        await RefreshSubjectUI("Physics", physicsUI, profilePhysicsUI);
        await RefreshSubjectUI("Chemistry", chemistryUI, profileChemistryUI);
        await RefreshSubjectUI("Biology", biologyUI, profileBiologyUI);
    }

    private async Task RefreshSubjectUI(string subjectName, SubjectUI panelUI, SubjectUI profileUI)
    {
        if (Uid == null || db == null) return;

        Query query = db.Collection("users").Document(Uid).Collection("progress")
            .WhereEqualTo("subject", subjectName)
            .WhereEqualTo("completed", true);

        QuerySnapshot snapshot = await query.GetSnapshotAsync();
        
        int completedCount = snapshot.Count;
        int totalContent = 3; 
        float percentage = Mathf.Clamp01((float)completedCount / totalContent);

        UpdateUISlot(panelUI, percentage);
        UpdateUISlot(profileUI, percentage);
    }

    private void UpdateUISlot(SubjectUI ui, float fillAmount)
    {
        if (ui.progressBar != null)
            ui.progressBar.fillAmount = fillAmount;

        if (ui.percentageText != null)
            ui.percentageText.text = $"{(fillAmount * 100):0}%";
    }
}

[Serializable]
public class UserProgress
{
    public string contentId;
    public double lastPlayTime;
    public bool lastWasPlaying;
    public bool notesRead;
    public bool completed;
}