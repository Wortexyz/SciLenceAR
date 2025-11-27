using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using Firebase.Firestore;
using System.Threading.Tasks;

public class ProfileManager : MonoBehaviour
{
    public InputField nameInput;
    public Text emailText;
    public Text summaryText;

    FirebaseAuth auth;
    FirebaseFirestore db;

    void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
    }

    void OnEnable()
    {
        _ = LoadProfile();
    }

    public async Task LoadProfile()
    {
        var user = auth.CurrentUser;
        if (user == null) { Debug.LogError("Not signed in"); return; }
        emailText.text = user.Email;

        var docRef = db.Collection("users").Document(user.UserId);
        var snap = await docRef.GetSnapshotAsync();
        if (snap.Exists && snap.ContainsField("name")) nameInput.text = snap.GetValue<string>("name");
        else
        {
            var data = new { name = user.DisplayName ?? "User", email = user.Email, createdAt = Timestamp.GetCurrentTimestamp() };
            await docRef.SetAsync(data);
            nameInput.text = user.DisplayName ?? "User";
        }

        var progressCol = db.Collection("users").Document(user.UserId).Collection("progress");
        var psnap = await progressCol.GetSnapshotAsync();
        var counts = new System.Collections.Generic.Dictionary<string, int>();
        foreach (var d in psnap.Documents)
        {
            if (d.ContainsField("completed") && d.GetValue<bool>("completed"))
            {
                var subj = d.ContainsField("subject") ? d.GetValue<string>("subject") : "Unknown";
                if (!counts.ContainsKey(subj)) counts[subj] = 0;
                counts[subj]++;
            }
        }

        string s = "";
        foreach (var kv in counts) s += $"{kv.Key}: {kv.Value}\n";
        summaryText.text = string.IsNullOrEmpty(s) ? "No completions yet." : s;
    }

    public async void SaveProfile()
    {
        var user = auth.CurrentUser;
        if (user == null) return;
        var docRef = db.Collection("users").Document(user.UserId);
        var updates = new System.Collections.Generic.Dictionary<string, object> { { "name", nameInput.text } };
        await docRef.SetAsync(updates, SetOptions.MergeAll);
        Debug.Log("Profile saved.");
    }
}
