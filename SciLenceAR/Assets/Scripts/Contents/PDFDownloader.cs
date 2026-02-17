using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;
using System.IO;
using NativeFilePickerNamespace;
using NativeShareNamespace; // Namespace for the newly added Native Share plugin

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

        // Step 1: Always download to internal persistent path first
        string tempPath = Path.Combine(Application.persistentDataPath, safeFileName);

        UnityWebRequest request = UnityWebRequest.Get(url);
        request.timeout = 60;

        var operation = request.SendWebRequest();

        // Progress loop stays outside try-catch to avoid CS1626
        while (!operation.isDone)
        {
            if (progressText != null)
            {
                float percent = Mathf.Clamp01(request.downloadProgress) * 100f;
                progressText.text = $"Downloading... {percent:0}%";
            }
            yield return null; 
        }

        if (request.result == UnityWebRequest.Result.Success)
        {
            bool writeSuccess = false;
            try
            {
                File.WriteAllBytes(tempPath, request.downloadHandler.data);
                writeSuccess = true;
                Debug.Log("PDF temporary save successful at: " + tempPath);
            }
            catch (IOException e)
            {
                Debug.LogError("File write error: " + e.Message);
                if (progressText != null) progressText.text = "Storage error.";
            }

            if (writeSuccess)
            {
                if (progressText != null)
                    progressText.text = "Select save folder...";

                yield return new WaitForSeconds(0.5f);

                // ACTION A: Native File Picker - User chooses permanent folder (e.g. Downloads)
                NativeFilePicker.ExportFile(tempPath, (success) =>
                {
                    if (success)
                    {
                        Debug.Log("File saved successfully to public storage.");
                        
                        if (progressText != null)
                            progressText.text = "Saved! Opening...";

                        // ACTION B: Native Share - Trigger "Open With" app list immediately
                        new NativeShare()
                            .AddFile(tempPath)
                            .SetTitle("Open PDF")
                            .SetCallback((result, shareTarget) => 
                            {
                                // Clean up temp file after user interacts with the share sheet
                                if (File.Exists(tempPath)) File.Delete(tempPath);
                                if (downloadPanel != null) downloadPanel.SetActive(false);
                            })
                            .Share();
                    }
                    else
                    {
                        // User cancelled save, clean up temp file
                        if (File.Exists(tempPath)) File.Delete(tempPath);
                        if (downloadPanel != null) downloadPanel.SetActive(false);
                    }
                });
            }
        }
        else
        {
            Debug.LogError("PDF DOWNLOAD ERROR: " + request.error);
            if (progressText != null)
                progressText.text = "Download failed.";
            
            yield return new WaitForSeconds(1.5f);
            if (downloadPanel != null) downloadPanel.SetActive(false);
        }
    }
}