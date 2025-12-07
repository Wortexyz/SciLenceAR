using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using System.IO;

public class PDFDownloader : MonoBehaviour
{
    public GameObject downloadPanel;
    public Slider progressBar;
    public Text progressText;

    public void DownloadPDF(string url, string safeFileName)
    {
        if (!gameObject.activeInHierarchy)
            gameObject.SetActive(true);

        StartCoroutine(DownloadRoutine(url, safeFileName));
    }

    IEnumerator DownloadRoutine(string url, string safeFileName)
    {
        if (downloadPanel != null)
            downloadPanel.SetActive(true);

        if (progressBar != null)
            progressBar.value = 0f;

        if (progressText != null)
            progressText.text = "Starting download...";

        string path = Path.Combine(Application.persistentDataPath, safeFileName);

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.timeout = 60;

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            File.WriteAllBytes(path, request.downloadHandler.data);

            if (progressText != null)
                progressText.text = "Saved To:\n" + path;

            Debug.Log("PDF Saved At: " + path);

            Application.OpenURL(path);
        }
        else
        {
            if (progressText != null)
                progressText.text = "Download failed!";

            Debug.LogError("PDF DOWNLOAD ERROR: " + request.error);
            Debug.LogError("URL USED: " + url);
        }

        yield return new WaitForSeconds(2f);

        if (downloadPanel != null)
            downloadPanel.SetActive(false);
    }
}
