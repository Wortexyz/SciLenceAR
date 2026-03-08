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
    }

    void OnEnable()
    {
        if (auth != null)
            auth.StateChanged += OnAuthStateChanged;
    }

    void OnDisable()
    {
        if (auth != null)
            auth.StateChanged -= OnAuthStateChanged;
    }

    private async void OnAuthStateChanged(object sender, EventArgs e)
    {
        if (auth.CurrentUser != null)
        {
            Debug.Log("[ProgressManager] User signed in. Fetching progress...");
            await UpdateAllSubjectProgressUI();
        }
        else
        {
            Debug.Log("[ProgressManager] User signed out. Resetting progress UI.");
            ResetAllProgressUI();
        }
    }

    string Uid
    {
        get
        {
            var u = auth.CurrentUser;
            if (u == null) return null;
            return u.UserId;
        }
    }

    // --- BUTTON-READY METHOD ---
    public void MarkCompletedForButton(string commaSeparatedInput)
    {
        if (string.IsNullOrEmpty(commaSeparatedInput)) return;

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
    public async Task MarkCompletedAsync(string contentId, string subject)
    {
        if (string.IsNullOrEmpty(Uid)) return;

        // Combine subject and contentId to ensure the Document ID is 100% unique
        string uniqueDocId = $"{subject}_{contentId}";

        var docRef = db.Collection("users").Document(Uid).Collection("progress").Document(uniqueDocId);

        var updates = new Dictionary<string, object>() {
            {"completed", true},
            {"subject", subject},
            {"originalContentId", contentId},
            {"completedAt", Timestamp.GetCurrentTimestamp()},
            {"updatedAt", Timestamp.GetCurrentTimestamp()}
        };

        await docRef.SetAsync(updates, SetOptions.MergeAll);

        await Task.Delay(500);
        await UpdateAllSubjectProgressUI();
    }

    public async Task UpdateAllSubjectProgressUI()
    {
        if (string.IsNullOrEmpty(Uid))
        {
            ResetAllProgressUI();
            return;
        }

        await RefreshSubjectUI("Physics", physicsUI, profilePhysicsUI);
        await RefreshSubjectUI("Chemistry", chemistryUI, profileChemistryUI);
        await RefreshSubjectUI("Biology", biologyUI, profileBiologyUI);
    }

    private async Task RefreshSubjectUI(string subjectName, SubjectUI panelUI, SubjectUI profileUI)
    {
        if (string.IsNullOrEmpty(Uid) || db == null) return;

        // Reset UI to 0 BEFORE querying so old user data never shows
        UpdateUISlot(panelUI, 0f);
        UpdateUISlot(profileUI, 0f);

        try
        {
            Query query = db.Collection("users").Document(Uid).Collection("progress")
                .WhereEqualTo("subject", subjectName)
                .WhereEqualTo("completed", true);

            // Fetch from Server to bypass local cache issues
            QuerySnapshot snapshot = await query.GetSnapshotAsync(Source.Server);

            int completedCount = snapshot.Count;
            float percentage = Mathf.Clamp01((float)completedCount / totalLessonsPerSubject);

            if (completedCount > 0)
            {
                UpdateUISlot(panelUI, percentage);
                UpdateUISlot(profileUI, percentage);
            }
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
            if (ui.progressBar.type != Image.Type.Filled)
                ui.progressBar.type = Image.Type.Filled;

            ui.progressBar.fillAmount = fillAmount;
        }

        if (ui.percentageText != null)
        {
            ui.percentageText.text = $"{(fillAmount * 100):0}%";
        }
    }

    private void ResetAllProgressUI()
    {
        UpdateUISlot(physicsUI, 0f);
        UpdateUISlot(profilePhysicsUI, 0f);
        UpdateUISlot(chemistryUI, 0f);
        UpdateUISlot(profileChemistryUI, 0f);
        UpdateUISlot(biologyUI, 0f);
        UpdateUISlot(profileBiologyUI, 0f);
    }
}