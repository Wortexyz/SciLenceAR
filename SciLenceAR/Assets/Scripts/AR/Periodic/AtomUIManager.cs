using UnityEngine;
using UnityEngine.UI;

public class AtomUIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject controlPanel;

    void Start()
    {
        if (controlPanel != null) controlPanel.SetActive(false);
    }

    public void ShowPanel()
    {
        if (controlPanel != null) controlPanel.SetActive(true);
    }

    public void ClickInjectProton()
    {
        var core = FindObjectOfType<AtomCoreLogic>();
        if (core != null) core.InjectProton();
    }

    public void ClickPlayAudio()
    {
        var core = FindObjectOfType<AtomCoreLogic>();
        if (core != null) core.PlayAIExplanation();
    }
}