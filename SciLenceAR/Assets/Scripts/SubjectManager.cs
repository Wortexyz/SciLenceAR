using UnityEngine;
using Unity.Collections;

public class SubjectManager : MonoBehaviour
{
    public static SubjectManager Instance;

    [Header("Subject Panels")]
    public GameObject physicsPanel;
    public GameObject chemistryPanel;
    public GameObject biologyPanel;

    private void Awake()
    {
        Instance = this;
    }

    private void HideAll()
    {
        if (physicsPanel) physicsPanel.SetActive(false);
        if (chemistryPanel) chemistryPanel.SetActive(false);
        if (biologyPanel) biologyPanel.SetActive(false);
    }

    public void OpenPhysics()
    {
        HideAll();
        if (physicsPanel) physicsPanel.SetActive(true);
    }

    public void OpenChemistry()
    {
        HideAll();
        if (chemistryPanel) chemistryPanel.SetActive(true);
    }

    public void OpenBiology()
    {
        HideAll();
        if (biologyPanel) biologyPanel.SetActive(true);
    }
}