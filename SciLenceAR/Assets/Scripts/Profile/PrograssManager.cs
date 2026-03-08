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

    [Header("Settings")]
    [SerializeField] private int totalLessonsPerSubject = 3;

    [Header("UI References (Assign in Inspector)")]
    [SerializeField] private SubjectUI physicsUI;
    [SerializeField] private SubjectUI chemistryUI;
    [SerializeField] private SubjectUI biologyUI;
    
    [Header("Profile UI References")]
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
        
        // Optional: Disable persistence if you want to ensure it ALWAYS pulls from the cloud
        // FirebaseFirestore.DefaultInstance.Settings.PersistenceEnabled = false;
    }

    // Ensure the UI updates when the game starts
    async void Start()
    {
        if (auth.CurrentUser != null)
        {
            await UpdateAllSubjectProgressUI();
        }
    }

    string Uid
    {
        get
        {
            var u = auth.CurrentUser;
            if (u == null)
            {
                Debug.LogError("ProgressManager: No authenticated user found!");
                return null;
            }
            return u.UserId;
        }
    }

    // --- BUTTON-READY METHOD ---
    public void MarkCompletedForButton(string commaSeparatedInput)
    {
        if (string.IsNullOrEmpty(commaSeparatedInput))
        {
            Debug.LogError("Button Input is empty!");
            return;
        }

        string[] parts = commaSeparatedInput.Split(',');
        if (parts.Length < 2)
        {
            Debug.LogError($"Invalid Button Input: '{commaSeparatedInput}'. Format must be: contentId,Subject");
            return;
        }
        
        string cid = parts[0].Trim();
        string sub = parts[1].Trim();
        
        Debug.Log($"Button Clicked! Mark {cid} as completed for {sub}");
        _ = MarkCompletedAsync(cid, sub);
    }

    // --- FIREBASE LOGIC ---

    public async Task SaveProgressAsync(string contentId, double lastPlayTime, bool lastWasPlaying, bool notesRead, string subject = "Unknown", bool completed = false)
    {
        if (string.IsNullOrEmpty(Uid)) return;
        
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
        if (string.IsNullOrEmpty(Uid)) return null;
        
        var docRef = db.Collection("users").Document(Uid).Collection("progress").Document(contentId);
        var snapshot = await docRef.GetSnapshotAsync();
        
        if (!snapshot.Exists) return null;
        
        var p = new UserProgress();
        p.contentId = contentId;
        p.lastPlayTime = snapshot.ContainsField("lastPlayTime") ? Convert.ToDouble(snapshot.GetValue<object>("lastPlayTime")) : 0;
        p.lastWasPlaying = snapshot.ContainsField("lastWasPlaying") ? snapshot.GetValue<bool>("lastWasPlaying") : false;
        p.notesRead = snapshot.ContainsField("notesRead") ? snapshot.GetValue<bool>("notesRead") : false;
        p.completed = snapshot.ContainsField("completed") ? snapshot.GetValue<bool>("completed") : false;
        return p;
    }

    public async Task MarkCompletedAsync(string contentId, string subject)
    {
        if (string.IsNullOrEmpty(Uid)) return;

        var docRef = db.Collection("users").Document(Uid).Collection("progress").Document(contentId);
        var updates = new Dictionary<string, object>() {
            {"completed", true},
            {"subject", subject}, 
            {"completedAt", Timestamp.GetCurrentTimestamp()},
            {"updatedAt", Timestamp.GetCurrentTimestamp()}
        };
        
        // Use SetAsync with MergeAll to ensure we don't overwrite other fields if they exist
        await docRef.SetAsync(updates, SetOptions.MergeAll);
        
        // Small delay to let the Firestore backend catch up
        await Task.Delay(1000); 
        await UpdateAllSubjectProgressUI();
    }

    public async Task UpdateAllSubjectProgressUI()
    {
        // Added check to ensure we don't update UI if user logged out mid-process
        if (string.IsNullOrEmpty(Uid)) return;

        await RefreshSubjectUI("Physics", physicsUI, profilePhysicsUI);
        await RefreshSubjectUI("Chemistry", chemistryUI, profileChemistryUI);
        await RefreshSubjectUI("Biology", biologyUI, profileBiologyUI);
    }

    private async Task RefreshSubjectUI(string subjectName, SubjectUI panelUI, SubjectUI profileUI)
    {
        if (string.IsNullOrEmpty(Uid) || db == null) return;

        try 
        {
            // We query specifically for documents where the 'subject' matches AND 'completed' is true
            Query query = db.Collection("users").Document(Uid).Collection("progress")
                .WhereEqualTo("subject", subjectName)
                .WhereEqualTo("completed", true);

            // Source.Server forces it to fetch from Firebase Cloud, not local cache
            QuerySnapshot snapshot = await query.GetSnapshotAsync(Source.Server);
            
            int completedCount = snapshot.Count;
            float percentage = Mathf.Clamp01((float)completedCount / totalLessonsPerSubject);

            UpdateUISlot(panelUI, percentage);
            UpdateUISlot(profileUI, percentage);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error fetching {subjectName} progress: {e.Message}");
        }
    }

    private void UpdateUISlot(SubjectUI ui, float fillAmount)
    {
        if (ui.progressBar != null)
        {
            if(ui.progressBar.type != Image.Type.Filled) 
                ui.progressBar.type = Image.Type.Filled;
                
            ui.progressBar.fillAmount = fillAmount;
        }

        if (ui.percentageText != null)
        {
            ui.percentageText.text = $"{(fillAmount * 100):0}%";
        }
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