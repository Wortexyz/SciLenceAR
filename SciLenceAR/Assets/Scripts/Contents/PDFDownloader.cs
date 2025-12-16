using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using System.IO;

public class PDFDownloader : MonoBehaviour
{
    [Header("UI")]
    public GameObject downloadPanel;
    public Text progressText;

    public void DownloadPDF(string url, string safeFileName)
    {
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogError("PDFDownloader: URL is empty");
            return;
        }

        if (downloadPanel != null)
            downloadPanel.SetActive(true);

        StartCoroutine(DownloadRoutine(url, safeFileName));
    }

    private IEnumerator DownloadRoutine(string url, string safeFileName)
    {
        if (progressText != null)
            progressText.text = "Preparing download...";

#if UNITY_ANDROID && !UNITY_EDITOR
        string downloadPath = Path.Combine("/storage/emulated/0/Download", safeFileName);
#else
        string downloadPath = Path.Combine(Application.persistentDataPath, safeFileName);
#endif

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.timeout = 60;

        var operation = request.SendWebRequest();

        while (!operation.isDone)
        {
            if (progressText != null)
            {
                float percent = Mathf.Clamp01(request.downloadProgress) * 100f;
                progressText.text = $"Downloading... {percent:0}%";
            }
            yield return null;
        }

        bool writeSuccess = false;

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                File.WriteAllBytes(downloadPath, request.downloadHandler.data);
                writeSuccess = true;
                Debug.Log("PDF saved to: " + downloadPath);
            }
            catch (IOException e)
            {
                Debug.LogError("File write error: " + e.Message);
                if (progressText != null)
                    progressText.text = "Storage permission error.";
            }
        }
        else
        {
            Debug.LogError("PDF DOWNLOAD ERROR: " + request.error);
            if (progressText != null)
                progressText.text = "Download failed.";
        }

       
        if (writeSuccess)
        {
            if (progressText != null)
                progressText.text = "Download complete. Opening file...";

            yield return new WaitForSeconds(0.7f);
            Application.OpenURL(downloadPath);
        }

        yield return new WaitForSeconds(1.2f);

        if (downloadPanel != null)
            downloadPanel.SetActive(false);
    }
}
