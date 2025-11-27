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
        LoadAllContent();
    }

    void LoadAllContent()
    {
        var all = Resources.LoadAll<ContentDefinition>("Content");
        foreach (var def in all)
        {
            var parent = GetParentForSubject(def.subject);
            if (parent == null) continue;
            CreateCardForContent(def, parent);
        }
    }

    RectTransform GetParentForSubject(string subject)
    {
        if (string.IsNullOrEmpty(subject)) return null;
        subject = subject.ToLower().Trim();
        if (subject == "physics") return physicsContentParent;
        if (subject == "chemistry") return chemistryContentParent;
        if (subject == "biology") return biologyContentParent;
        return null;
    }

    void CreateCardForContent(ContentDefinition def, RectTransform parent)
    {
        var go = Instantiate(lessonCardPrefab, parent);
        go.transform.localScale = Vector3.one;

        var title = go.transform.Find("TitleText")?.GetComponent<Text>();
        if (title != null) title.text = def.title;

        var buttons = go.GetComponentsInChildren<Button>(true);
        foreach (var b in buttons)
        {
            if (b.name == "BtnWatch") b.onClick.AddListener(() => UIManager.I.OnWatchClicked(def));
            else if (b.name == "BtnNotes") b.onClick.AddListener(() => UIManager.I.OnNotesClicked(def));
            else if (b.name == "BtnStartAR") b.onClick.AddListener(() => UIManager.I.OnStartARClicked(def));
        }

        // set status icon if completed (async)
        var pm = FindObjectOfType<ProgressManager>();
        if (pm != null)
        {
            _ = pm.GetProgressAsync(def.contentId).ContinueWith(t =>
            {
                var prog = t.Result;
                if (prog != null && prog.completed)
                {
                    var icon = go.transform.Find("StatusIcon")?.GetComponent<Image>();
                    if (icon != null) icon.color = Color.green;
                }
            });
        }
    }
}
