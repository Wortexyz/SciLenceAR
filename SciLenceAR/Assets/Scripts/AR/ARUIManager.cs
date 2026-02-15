using UnityEngine;
using UnityEngine.UI;

public class ARUIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject mainPanel;
    public GameObject rotatePanel;
    public GameObject scalePanel;

    [Header("Sliders")]
    public Slider rotateSlider;
    public Slider scaleSlider;

    private ARObjectManipulator currentObject;

    // Called from ARPosition after object is spawned
    public void RegisterObject(GameObject spawned)
    {
        currentObject = spawned.GetComponent<ARObjectManipulator>();
        ShowMain();
    }

    void HideAll()
    {
        mainPanel.SetActive(false);
        rotatePanel.SetActive(false);
        scalePanel.SetActive(false);
    }

    public void ShowMain()
    {
        HideAll();
        mainPanel.SetActive(true);
        currentObject?.DisableMode();
    }

    // ---------------- MOVE ----------------
    // Touch + plane based (NO panel needed)
    public void EnableMove()
    {
        currentObject?.SetMoveMode();
    }

    // ---------------- ROTATE ----------------
    public void OpenRotate()
    {
        HideAll();
        rotatePanel.SetActive(true);
        rotateSlider.value = 0.5f;
        currentObject?.SetRotateMode();
    }

    // ---------------- SCALE ----------------
    public void OpenScale()
    {
        HideAll();
        scalePanel.SetActive(true);
        scaleSlider.value = 0.5f;
        currentObject?.SetScaleMode();
    }

    // ---------------- DONE ----------------
    public void Done()
    {
        ShowMain();
    }

    // ---------------- SLIDER CALLBACKS ----------------
    public void OnRotateChanged(float v)
    {
        currentObject?.RotateBySlider(v);
    }

    public void OnScaleChanged(float v)
    {
        currentObject?.ScaleBySlider(v);
    }

    public void ResetRotation()
    {
        currentObject?.ResetRotation();
    }

    public void ResetScale()
    {
        currentObject?.ResetScale();
    }
}
