using UnityEngine;
using System.Collections.Generic;

public interface ICommand
{
    void Execute();
}

public class MoveForwardCommand : ICommand
{
    public void Execute()
    {
        Debug.Log("Executing Move Forward");
        // Add your movement logic here
    }
}

public class MoveBackwardCommand : ICommand
{
    public void Execute()
    {
        Debug.Log("Executing Move Backward");
        // Add your movement logic here
    }
}

public class TurnLeftCommand : ICommand
{
    public void Execute()
    {
        Debug.Log("Executing Turn Left");
        // Add your turning logic here
    }
}

public class TurnRightCommand : ICommand
{
    public void Execute()
    {
        Debug.Log("Executing Turn Right");
        // Add your turning logic here
    }
}

public class StopCommand : ICommand
{
    public void Execute()
    {
        Debug.Log("Executing Stop");
        // Add your stopping logic here
    }
}

public class LookAroundCommand : ICommand
{
    private OpenAIService openAIService;

    public LookAroundCommand(OpenAIService service)
    {
        openAIService = service;
    }

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
            Debug.LogError("Unknown command: " + command);
        }
    }
}