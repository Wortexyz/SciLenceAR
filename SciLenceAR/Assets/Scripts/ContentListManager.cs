// Assets/Scripts/UI/ContentListManager.cs (Debug)
using UnityEngine;
using UnityEngine.UI;

public class ContentListManager : MonoBehaviour
{
    public RectTransform physicsContentParent;
    public RectTransform chemistryContentParent;
    public RectTransform biologyContentParent;
    public GameObject lessonCardPrefab;

    void Start()
    {
        Debug.Log("[ContentListManager] Start() on " + gameObject.name + " (active=" + gameObject.activeInHierarchy + ")");
        LoadAllContent();
    }

    void LoadAllContent()
    {
        var all = Resources.LoadAll<ContentDefinition>("Content");
        int count = (all == null) ? 0 : all.Length;
        Debug.Log($"[ContentListManager] Resources.LoadAll returned {count} asset(s).");

        if (count == 0)
        {
            Debug.LogWarning("[ContentListManager] No ContentDefinition assets found under Assets/Resources/Content. Make sure they are ScriptableObject assets and inside that folder.");
            return;
        }

        foreach (var def in all)
        {
            Debug.Log($"[ContentListManager] Found asset: id='{def.contentId}' title='{def.title}' subject='{def.subject}'");
            var parent = GetParentForSubject(def.subject);
            Debug.Log($"[ContentListManager] Parent for subject '{def.subject}' = {(parent == null ? "NULL" : parent.name)}");
            if (parent == null) continue;

            if (lessonCardPrefab == null)
            {
                Debug.LogError("[ContentListManager] lessonCardPrefab is NULL. Assign it in the inspector on Appmanager.");
                return;
            }

            var go = Instantiate(lessonCardPrefab, parent);
            go.transform.localScale = Vector3.one;
            Debug.Log("[ContentListManager] Instantiated lesson card for " + def.contentId + " under " + parent.name);
        }
    }

    RectTransform GetParentForSubject(string subject)
    {
        if (string.IsNullOrEmpty(subject)) return null;
        string s = subject.Trim().ToLowerInvariant();
        if (s == "physics") return physicsContentParent;
        if (s == "chemistry") return chemistryContentParent;
        if (s == "biology") return biologyContentParent;
        return null;
    }
}
