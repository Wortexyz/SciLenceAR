using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.UI;

public class ProfileManager : MonoBehaviour
{
    public InputField nameInput; // change to TMP_InputField if you use TextMeshPro
    public Text emailText;
    public Text summaryText;
    public GameObject panelRoot; // assign the ProfilePanel here in Inspector

    FirebaseAuth auth;
    FirebaseFirestore db;

    void Awake()
    {
        Debug.Log("[ProfileManager] Awake()");
        auth = FirebaseAuth.DefaultInstance;
        try
        {
            db = FirebaseFirestore.DefaultInstance;
            Debug.Log("[ProfileManager] Firestore instance acquired.");
        }
        catch (Exception ex)
        {
            Debug.LogError("[ProfileManager] Failed to get Firestore instance: " + ex.Message);
            db = null;
        }
    }

    void OnEnable()
    {
        if (auth != null)
            auth.StateChanged += OnAuthStateChanged;

        _ = LoadProfile();
    }

    void OnDisable()
    {
        if (auth != null)
            auth.StateChanged -= OnAuthStateChanged;
    }

    void OnDestroy()
    {
        if (auth != null)
            auth.StateChanged -= OnAuthStateChanged;
    }

    private void OnAuthStateChanged(object sender, System.EventArgs e)
    {
        Debug.Log("[ProfileManager] Auth state changed. CurrentUser: " + (auth.CurrentUser != null ? auth.CurrentUser.UserId : "null"));
        if (auth.CurrentUser != null)
        {
            _ = LoadProfile();
        }
        else
        {
            // CLEAR UI COMPLETELY ON LOGOUT
            if (emailText != null) emailText.text = "Not signed in";
            if (summaryText != null) summaryText.text = "Not signed in";
            if (nameInput != null) nameInput.text = "";
        }
    }

    public void RefreshProfile()
    {
        _ = LoadProfile();
    }

    public async Task LoadProfile()
    {
        if (auth == null || auth.CurrentUser == null)
        {
            if (emailText != null) emailText.text = "Not signed in";
            if (summaryText != null) summaryText.text = "Not signed in";
            if (nameInput != null) nameInput.text = "";
            return;
        }

        var user = auth.CurrentUser;
        if (emailText != null) emailText.text = user.Email ?? "No email";

        if (db == null)
        {
            if (nameInput != null) nameInput.text = user.DisplayName ?? "";
            return;
        }

        var docRef = db.Collection("users").Document(user.UserId);

        try
        {
            var snap = await docRef.GetSnapshotAsync();
            if (!snap.Exists)
            {
                var initial = new Dictionary<string, object>
                {
                    {"name", user.DisplayName ?? "" },
                    {"email", user.Email ?? "" },
                    {"createdAt", Timestamp.GetCurrentTimestamp()}
                };
                await docRef.SetAsync(initial);
                if (nameInput != null) nameInput.text = initial["name"] as string;
                await UpdateSummaryTextFromProgressCounts();
                return;
            }

            if (snap.ContainsField("name"))
            {
                string name = snap.GetValue<string>("name");
                if (nameInput != null) nameInput.text = name;
            }
            else
            {
                if (nameInput != null) nameInput.text = user.DisplayName ?? "";
            }

            await UpdateSummaryTextFromProgressCounts();
        }
        catch (Exception ex)
        {
            Debug.LogError("[ProfileManager] Error while loading profile: " + ex);
            if (summaryText != null) summaryText.text = "Error loading profile";
        }
    }

    async Task UpdateSummaryTextFromProgressCounts()
    {
        try
        {
            var user = auth.CurrentUser;
            if (user == null || db == null) return;

            var progressCol = db.Collection("users").Document(user.UserId).Collection("progress");
            var psnap = await progressCol.GetSnapshotAsync();
            var counts = new Dictionary<string, int>();

            foreach (var d in psnap.Documents)
            {
                bool completed = d.ContainsField("completed") ? d.GetValue<bool>("completed") : false;
                string subj = d.ContainsField("subject") ? d.GetValue<string>("subject") : "Unknown";
                if (completed)
                {
                    if (!counts.ContainsKey(subj)) counts[subj] = 0;
                    counts[subj]++;
                }
            }

            string s = "";
            foreach (var kv in counts) s += $"{kv.Key}: {kv.Value}\n";
            if (summaryText != null) summaryText.text = string.IsNullOrEmpty(s) ? "No completions yet." : s;
        }
        catch (Exception ex)
        {
            Debug.LogError("[ProfileManager] Failed to update summary counts: " + ex);
        }
    }

    public async void SaveProfile()
    {
        var user = auth.CurrentUser;
        if (user == null || db == null) return;

        string newName = nameInput != null ? nameInput.text : "";

        try
        {
            var updateProfileTask = user.UpdateUserProfileAsync(new Firebase.Auth.UserProfile { DisplayName = newName });
            await updateProfileTask;
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[ProfileManager] Failed to update Firebase displayName: " + ex);
        }

        var docRef = db.Collection("users").Document(user.UserId);
        var updates = new Dictionary<string, object> { { "name", newName }, { "updatedAt", Timestamp.GetCurrentTimestamp() } };

        try
        {
            await docRef.SetAsync(updates, SetOptions.MergeAll);
        }
        catch (Exception ex)
        {
            Debug.LogError("[ProfileManager] Failed saving profile: " + ex);
        }

        await UpdateSummaryTextFromProgressCounts();
    }

    public void ClosePanel()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
    }
}