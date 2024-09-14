using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Text;


/// <summary>
/// Control Spot Robot 
/// </summary>
public class RobotController : MonoBehaviour
{
    private const string baseUrl = "http://192.168.4.38:5000"; // most likely spot IP address but make sure 

    [Header("Movement Settings")]
    [SerializeField] private float forwardSpeed = 1f;
    [SerializeField] private float backwardSpeed = 1f;
    [SerializeField] private float leftSpeed = 1f;
    [SerializeField] private float rightSpeed = 1f;
    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private float defaultDuration = 1f;

    private IEnumerator SendCommand(string command, VelocityArgs args = null)
    {
        string url = baseUrl + "/command";
        string jsonPayload = JsonUtility.ToJson(new CommandPayload(command, args));

        Debug.Log($"Sending JSON: {jsonPayload}"); // Log the JSON being sent

        using (UnityWebRequest www = UnityWebRequest.PostWwwForm(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {www.error}");
                Debug.LogError($"Response: {www.downloadHandler.text}");
            }
            else
            {
                Debug.Log($"Command sent successfully: {command}");
                Debug.Log($"Response: {www.downloadHandler.text}");
            }
        }
    }

    [ContextMenu("SendStart")]
    public void SendStart()
    {
        StartCoroutine(SendCommand("start"));
    }
    [ContextMenu("SendStop")]
    public void SendStop()
    {
        StartCoroutine(SendCommand("stop"));
    }

    public void Move(float v_x, float v_y, float v_rot, float cmd_duration)
    {
        VelocityArgs args = new VelocityArgs(v_x, v_y, v_rot, cmd_duration);
        StartCoroutine(SendCommand("velocity", args));
    }
    
    [ContextMenu("MoveForward")]
    public void MoveForward() => Move(forwardSpeed, 0, 0, defaultDuration);
    [ContextMenu("MoveBackward")]
    public void MoveBackward() => Move(-backwardSpeed, 0, 0, defaultDuration);
    [ContextMenu("MoveLeft")]
    public void MoveLeft() => Move(0, -leftSpeed, 0, defaultDuration);
    [ContextMenu("MoveRight")]
    public void MoveRight() => Move(0, rightSpeed, 0, defaultDuration);
    [ContextMenu("RotateLeft")]
    public void RotateLeft() => Move(0, 0, -rotationSpeed, defaultDuration);
    [ContextMenu("RotateRight")]
    public void RotateRight() => Move(0, 0, rotationSpeed, defaultDuration);

    public void CustomMove(float v_x, float v_y, float v_rot, float duration)
    {
        Move(v_x, v_y, v_rot, duration);
    }
}

[System.Serializable]
public class CommandPayload
{
    public string command;
    public VelocityArgs args;

    public CommandPayload(string command, VelocityArgs args)
    {
        this.command = command;
        this.args = args;
    }
}

[System.Serializable]
public class VelocityArgs
{
    public float v_x;
    public float v_y;
    public float v_rot;
    public float cmd_duration;

    public VelocityArgs(float v_x, float v_y, float v_rot, float cmd_duration)
    {
        this.v_x = v_x;
        this.v_y = v_y;
        this.v_rot = v_rot;
        this.cmd_duration = cmd_duration;
    }
}
