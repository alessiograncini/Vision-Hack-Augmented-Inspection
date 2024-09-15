using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;

public interface ICommand
{
    void Execute();
}

public class MoveForwardCommand : ICommand
{
    public void Execute()
    {
        if (Object.FindObjectOfType<RobotController>() == null)
        {
            Debug.LogError("RobotController not found.");
            return;
        }

        RobotController robotController = Object.FindObjectOfType<RobotController>();
        robotController.MoveForward();
        Debug.Log("Executing Move Forward");
        // Add your movement logic here
    }
}

public class MoveBackwardCommand : ICommand
{
    public void Execute()
    {
        if (Object.FindObjectOfType<RobotController>() == null)
        {
            Debug.LogError("RobotController not found.");
            return;
        }

        RobotController robotController = Object.FindObjectOfType<RobotController>();
        robotController.MoveBackward();
        Debug.Log("Executing Move Backward");
        // Add your movement logic here
    }
}

public class TurnLeftCommand : ICommand
{
    public void Execute()
    {
        if (Object.FindObjectOfType<RobotController>() == null)
        {
            Debug.LogError("RobotController not found.");
            return;
        }

        RobotController robotController = Object.FindObjectOfType<RobotController>();
        robotController.RotateLeft();
        Debug.Log("Executing Turn Left");
        // Add your turning logic here
    }
}

public class TurnRightCommand : ICommand
{
    public void Execute()
    {
        if (Object.FindObjectOfType<RobotController>() == null)
        {
            Debug.LogError("RobotController not found.");
            return;
        }

        RobotController robotController = Object.FindObjectOfType<RobotController>();
        robotController.RotateRight();
        Debug.Log("Executing Turn Right");
        // Add your turning logic here
    }
}

public class StopCommand : ICommand
{
    public void Execute()
    {
        if (Object.FindObjectOfType<RobotController>() == null)
        {
            Debug.LogError("RobotController not found.");
            return;
        }

        RobotController robotController = Object.FindObjectOfType<RobotController>();
        robotController.SendStop();
        Debug.Log("Executing Stop");
        // Add your stopping logic here
    }
}

public class StartCommand : ICommand
{
    public void Execute()
    {
        if (Object.FindObjectOfType<RobotController>() == null)
        {
            Debug.LogError("RobotController not found.");
            return;
        }

        RobotController robotController = Object.FindObjectOfType<RobotController>();
        robotController.SendStart();
        // Add your starting logic here
    }
}

public class LookAroundCommand : ICommand
{
    private OpenAIService openAIService;
    private string serverUrl = "http://192.168.4.38:5002/get_frame"; // Update this to match your server URL

    public LookAroundCommand(OpenAIService service)
    {
        openAIService = service;
    }

    public void Execute()
    {
        Debug.Log("Executing Look Around");
        if (openAIService != null)
        {
            openAIService.StartCoroutine(FetchAndProcessImage());
        }
    }

    private IEnumerator FetchAndProcessImage()
    {

        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(serverUrl))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to fetch image: " + www.error);
                yield break;
            }

            Texture2D texture = DownloadHandlerTexture.GetContent(www);
            byte[] jpgData = texture.EncodeToJPG();
            string base64Image = System.Convert.ToBase64String(jpgData);

            // Now use the base64Image with the OpenAI service
            openAIService.StartCoroutine(openAIService.DescribeImage(base64Image, (description) =>
            {
                // You could display the description in the UI or handle it as needed
                // Call the TTS service to generate speech from the description
                if (!string.IsNullOrEmpty(description))
                {
                    openAIService.StartCoroutine(openAIService.GenerateSpeech(description, "alloy", (audioClip) =>
                    {
                        if (audioClip != null)
                        {
                            // Play the generated audio
                            AudioSource audioSource = openAIService.GetComponent<AudioSource>();
                            if (audioSource == null)
                            {
                                audioSource = openAIService.gameObject.AddComponent<AudioSource>();
                            }

                            audioSource.clip = audioClip;
                            audioSource.Play();
                        }
                        else
                        {
                            Debug.LogError("Failed to generate speech.");
                        }
                    }));
                }
            }));
        }
    }
}

public class CommandExecutor : MonoBehaviour
{
    private Dictionary<string, ICommand> commandMap;

    private void Awake()
    {
        OpenAIService openAIService = GetComponent<OpenAIService>();

        commandMap = new Dictionary<string, ICommand>
        {
            { "forward", new MoveForwardCommand() },
            { "backward", new MoveBackwardCommand() },
            { "left", new TurnLeftCommand() },
            { "right", new TurnRightCommand() },
            { "stop", new StopCommand() },
            { "start", new StartCommand() },
            { "look", new LookAroundCommand(openAIService) }
        };
    }

    public void ExecuteCommand(string command)
    {
        command = command.ToLower();
        if (commandMap.ContainsKey(command))
        {
            commandMap[command].Execute();
        }
        else
        {
            Debug.Log("Unknown command: " + command);
        }
    }
}

/*
public void Execute()
{
    Debug.Log("Executing Look Around");
    if (openAIService != null)
    {
        // You can call the DescribeImage function from OpenAIService to get an image description if needed.
        openAIService.StartCoroutine(openAIService.DescribeImage("https://upload.wikimedia.org/wikipedia/commons/thumb/d/dd/Gfp-wisconsin-madison-the-nature-boardwalk.jpg/2560px-Gfp-wisconsin-madison-the-nature-boardwalk.jpg", (description) =>
        {
            Debug.Log("Image description: " + description);
            // You could display the description in the UI or handle it as needed
            // Call the TTS service to generate speech from the description
            if (!string.IsNullOrEmpty(description))
            {
                openAIService.StartCoroutine(openAIService.GenerateSpeech(description, "alloy", (audioClip) =>
                {
                    if (audioClip != null)
                    {
                        // Play the generated audio
                        AudioSource audioSource = openAIService.GetComponent<AudioSource>();
                        if (audioSource == null)
                        {
                            audioSource = openAIService.gameObject.AddComponent<AudioSource>();
                        }
                        audioSource.clip = audioClip;
                        audioSource.Play();
                    }
                    else
                    {
                        Debug.LogError("Failed to generate speech.");
                    }
                }));
            }
        }));
    }
}
}


*/