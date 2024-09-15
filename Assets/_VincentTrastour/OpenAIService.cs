using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.Networking;
using Newtonsoft.Json;  // Import Newtonsoft.Json for JSON handling

public class OpenAIService : MonoBehaviour
{
    [SerializeField] private string openAIAPIKey;
    private string openAIEndpoint = "https://api.openai.com/v1/chat/completions";
    private string openAITTSEndpoint = "https://api.openai.com/v1/audio/speech";

    // Function to handle user input parsing
    public IEnumerator ParseUserInput(string userInput, System.Action<string> onComplete)
    {
        string prompt = "You are a helpful assistant. Detect if there is a command in the user's message and respond accordingly. "
                        + "Here is the list of the commands we are waiting for: "
                        + "'move forward', 'move backward', 'turn left', 'turn right', 'stop', 'tell me what you see', 'start', 'stop'. "
                        + "Answer with just one word so I can use that as a command detection. If there is no command, answer with 'none'."
                        + "'start' = start, 'stop' = stop, 'move forward' = 'forward', 'move backward' = 'backward', 'turn left' = 'left', 'turn right' = 'right', 'tell me what you see' = 'look'.";

        string jsonRequestBody = JsonConvert.SerializeObject(new
        {
            model = "gpt-4o-mini",
            messages = new[]
            {
                new { role = "system", content = prompt },
                new { role = "user", content = userInput }
            },
            max_tokens = 10,
            temperature = 0.5
        });

        yield return SendOpenAIRequest(openAIEndpoint, jsonRequestBody, onComplete);
    }

    // Function to describe an image
    public IEnumerator DescribeImage(string base64Image, System.Action<string> onComplete)
    {

        string jsonRequestBody = JsonConvert.SerializeObject(new
        {
            model = "gpt-4o-mini",
            messages = new[]
            {
                new
                {
                    role = "user",
                    content = new object[]
                    {
                        new { type = "text", text = "What's in this image? be short" },
                        new { type = "image_url", image_url = new { url = $"data:image/jpeg;base64,{base64Image}" } } // Updated to accept base64 string
                    }
                }
            },
            max_tokens = 300
        });

        yield return SendOpenAIRequest(openAIEndpoint, jsonRequestBody, response =>
        {
            // Debugging: Log the response from OpenAI
            Debug.Log("OpenAI Response: " + response);
            onComplete(response);
        });
    }

    // Coroutine to generate speech from text and download the speech file
    public IEnumerator GenerateSpeech(string text, string voice, System.Action<AudioClip> onComplete)
    {
        string jsonRequestBody = JsonConvert.SerializeObject(new
        {
            model = "tts-1",
            input = text,
            voice = voice
        });

        using (UnityWebRequest request = SetupUnityWebRequest(openAITTSEndpoint, jsonRequestBody))
        {
            request.downloadHandler = new DownloadHandlerFile(Path.Combine(Application.persistentDataPath, "speech.mp3")); // Save the speech to speech.mp3

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // Load the audio file as an AudioClip and play it
                StartCoroutine(LoadAudioClip(Path.Combine(Application.persistentDataPath, "speech.mp3"), onComplete));
            }
            else
            {
                onComplete(null);
            }
        }
    }

    // Coroutine to load an AudioClip from a file
    private IEnumerator LoadAudioClip(string filePath, System.Action<AudioClip> onComplete)
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file://" + filePath, AudioType.MPEG))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                onComplete(clip);  // Return the loaded audio clip
            }
            else
            {
                onComplete(null);
            }
        }
    }

    // Helper method to send OpenAI request
    // Helper method to send OpenAI request
    private IEnumerator SendOpenAIRequest(string endpoint, string jsonRequestBody, System.Action<string> onComplete)
    {
        using (UnityWebRequest request = SetupUnityWebRequest(endpoint, jsonRequestBody))
        {

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseJson = request.downloadHandler.text;

                // Parse the response using Newtonsoft.Json
                var response = JsonConvert.DeserializeObject<ChatCompletionResponse>(responseJson);
                string result = response.choices[0].message.content.Trim().ToLower();
                onComplete(result);
            }
            else
            {
                // Log the error for debugging
                Debug.LogError("OpenAI request failed: " + request.error);
                onComplete(null);
            }
        }
    }

    // Helper method to setup UnityWebRequest
    private UnityWebRequest SetupUnityWebRequest(string endpoint, string jsonRequestBody)
    {
        UnityWebRequest request = new UnityWebRequest(endpoint, "POST");
        byte[] bodyRaw = new System.Text.UTF8Encoding().GetBytes(jsonRequestBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + openAIAPIKey);
        return request;
    }
}

// Class to map OpenAI API responses using Newtonsoft.Json
public class ChatCompletionResponse
{
    public ChatChoice[] choices { get; set; }
}

public class ChatChoice
{
    public ChatMessage message { get; set; }
}

public class ChatMessage
{
    public string role { get; set; }
    public string content { get; set; }
}