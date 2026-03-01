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
        
        // Brief delay ensures Firestore has indexed the write before we read it back for the UI
        await Task.Delay(500); 
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

        try 
        {
            Query query = db.Collection("users").Document(Uid).Collection("progress")
                .WhereEqualTo("subject", subjectName)
                .WhereEqualTo("completed", true);

            QuerySnapshot snapshot = await query.GetSnapshotAsync();
            
            int completedCount = snapshot.Count;
            int totalContent = 3; // Ensure this matches your total lesson count
            float percentage = Mathf.Clamp01((float)completedCount / totalContent);

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
        // Must happen on main thread - checking if we are in play mode
        if (ui.progressBar != null)
        {
            // Set image type to Filled automatically if it isn't
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