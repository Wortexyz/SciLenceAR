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
        Debug.Log("[ProfileManager] OnEnable called - subscribing to auth state and loading profile (if any).");
        if (auth != null)
            auth.StateChanged += OnAuthStateChanged;

        // Always attempt to load (if user is already signed in this will populate UI).
        _ = LoadProfile();
    }

    void OnDisable()
    {
        Debug.Log("[ProfileManager] OnDisable - unsubscribing auth state.");
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
        Debug.Log("[ProfileManager] LoadProfile() start.");

        if (auth == null)
        {
            Debug.LogWarning("[ProfileManager] FirebaseAuth instance is null.");
            if (emailText != null) emailText.text = "Not signed in";
            if (summaryText != null) summaryText.text = "Not signed in";
            return;
        }

        var user = auth.CurrentUser;
        if (user == null)
        {
            Debug.LogWarning("[ProfileManager] No authenticated user found. CurrentUser is null.");
            if (emailText != null) emailText.text = "Not signed in";
            if (summaryText != null) summaryText.text = "Not signed in";
            if (nameInput != null) nameInput.text = "";
            return;
        }

        Debug.Log($"[ProfileManager] Signed in: {user.UserId}  email:{user.Email}");
        if (emailText != null) emailText.text = user.Email ?? "No email";

        if (db == null)
        {
            Debug.LogError("[ProfileManager] Firestore not initialized. Cannot load profile.");
            // Still populate name from auth displayName if available
            if (nameInput != null) nameInput.text = user.DisplayName ?? "";
            return;
        }

        var docRef = db.Collection("users").Document(user.UserId);

        try
        {
            var snap = await docRef.GetSnapshotAsync();
            if (!snap.Exists)
            {
                Debug.Log("[ProfileManager] No profile doc found - creating default profile doc.");
                var initial = new Dictionary<string, object>
                {
                    {"name", user.DisplayName ?? "" },
                    {"email", user.Email ?? "" },
                    {"createdAt", Timestamp.GetCurrentTimestamp()}
                };
                await docRef.SetAsync(initial);
                if (nameInput != null) nameInput.text = initial["name"] as string;
                await UpdateSummaryTextFromProgressCounts();
                Debug.Log("[ProfileManager] Created profile doc for user.");
                return;
            }

            // document exists - read fields
            Debug.Log("[ProfileManager] Profile doc exists. Reading fields...");
            if (snap.ContainsField("name"))
            {
                string name = snap.GetValue<string>("name");
                if (nameInput != null) nameInput.text = name;
                Debug.Log("[ProfileManager] Loaded name: " + name);
            }
            else
            {
                // fallback to auth displayName
                if (nameInput != null) nameInput.text = user.DisplayName ?? "";
                Debug.Log("[ProfileManager] 'name' field not found in profile doc. Used auth.DisplayName fallback.");
            }

            if (snap.ContainsField("email"))
            {
                string storedEmail = snap.GetValue<string>("email");
                Debug.Log("[ProfileManager] Stored email in doc: " + storedEmail);
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
            if (user == null || db == null)
            {
                if (summaryText != null) summaryText.text = "Not signed in";
                return;
            }

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
            Debug.Log("[ProfileManager] Summary updated: " + (string.IsNullOrEmpty(s) ? "No completions" : s));
        }
        catch (Exception ex)
        {
            Debug.LogError("[ProfileManager] Failed to update summary counts: " + ex);
            if (summaryText != null) summaryText.text = "Error loading summary";
        }
    }

    // Save name to Firestore and update FirebaseAuth displayName
    public async void SaveProfile()
    {
        Debug.Log("[ProfileManager] SaveProfile() invoked.");
        var user = auth.CurrentUser;
        if (user == null)
        {
            Debug.LogWarning("[ProfileManager] Save requested but no authenticated user.");
            return;
        }

        if (db == null)
        {
            Debug.LogError("[ProfileManager] Firestore not initialized. Cannot save profile.");
            return;
        }

        string newName = nameInput != null ? nameInput.text : "";

        // Update FirebaseAuth displayName
        try
        {
            var updateProfileTask = user.UpdateUserProfileAsync(new Firebase.Auth.UserProfile { DisplayName = newName });
            await updateProfileTask;
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[ProfileManager] Failed to update Firebase displayName: " + ex);
        }

        // Update Firestore doc (merge)
        var docRef = db.Collection("users").Document(user.UserId);
        var updates = new Dictionary<string, object> { { "name", newName }, { "updatedAt", Timestamp.GetCurrentTimestamp() } };

        try
        {
            await docRef.SetAsync(updates, SetOptions.MergeAll);
            Debug.Log("[ProfileManager] Profile saved. Name=" + newName);
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
        else Debug.Log("[ProfileManager] ClosePanel called but panelRoot not assigned.");
    }
}
