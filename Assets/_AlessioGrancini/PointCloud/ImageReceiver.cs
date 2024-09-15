using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.UI;

public class ImageReceiver : MonoBehaviour
{
    [SerializeField]
    private string serverUrl = "http://192.168.4.38:5002/get_frame";
    public float updateInterval = 0.1f; // Update every 0.1 seconds
    public RawImage displayImage;

    private Texture2D texture;

    void Start()
    {
        texture = new Texture2D(416, 416, TextureFormat.RGB24, false);
        displayImage.texture = texture;
        StartCoroutine(FetchImage());
    }

    IEnumerator FetchImage()
    {
        UnityWebRequest www = new UnityWebRequest(serverUrl);
        www.downloadHandler = new DownloadHandlerTexture(true);
        www.disposeUploadHandlerOnDispose = true;
        www.disposeDownloadHandlerOnDispose = true;

        while (true)
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Texture2D downloadedTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
                texture.SetPixels(downloadedTexture.GetPixels());
                texture.Apply();
                displayImage.material.mainTexture = texture;
            }
            else
            {
                Debug.Log("Error: " + www.error);
            }

            www.Dispose();
            www = new UnityWebRequest(serverUrl);
            www.downloadHandler = new DownloadHandlerTexture(true);
            www.disposeUploadHandlerOnDispose = true;
            www.disposeDownloadHandlerOnDispose = true;

            yield return new WaitForSeconds(updateInterval);
        }
    }

    void OnDestroy()
    {
        StopAllCoroutines();
    }
}