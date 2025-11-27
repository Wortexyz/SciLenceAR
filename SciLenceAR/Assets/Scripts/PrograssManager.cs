using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine;

public class ProgressManager : MonoBehaviour
{
    FirebaseAuth auth;
    FirebaseFirestore db;

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
        Debug.Log($"Saved progress for {contentId}");
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

    public async Task MarkCompletedAsync(string contentId)
    {
        if (Uid == null) return;
        var docRef = db.Collection("users").Document(Uid).Collection("progress").Document(contentId);
        var updates = new Dictionary<string, object>() {
            {"completed", true},
            {"completedAt", Timestamp.GetCurrentTimestamp()},
            {"updatedAt", Timestamp.GetCurrentTimestamp()}
        };
        await docRef.SetAsync(updates, SetOptions.MergeAll);
    }
}

public class UserProgress
{
    public string contentId;
    public double lastPlayTime;
    public bool lastWasPlaying;
    public bool notesRead;
    public bool completed;
}
