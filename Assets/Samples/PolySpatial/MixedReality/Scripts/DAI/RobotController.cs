using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class RobotController : MonoBehaviour
{
    private const string serverUrl = "http://192.168.4.38:5000/move_forward";

    
    [ContextMenu("Test Move Robot ")]
    public void MoveRobotForward()
    {
        StartCoroutine(SendMoveForwardRequest());
    }

    private IEnumerator SendMoveForwardRequest()
    {
        using (UnityWebRequest www = UnityWebRequest.PostWwwForm(serverUrl, ""))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error: " + www.error);
            }
            else
            {
                Debug.Log("Request successful!");
                Debug.Log("Response: " + www.downloadHandler.text);
            }
        }
    }
}
