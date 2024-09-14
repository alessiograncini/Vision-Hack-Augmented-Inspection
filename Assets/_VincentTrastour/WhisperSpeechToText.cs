using UnityEngine;
using UnityEngine.UI;
using Whisper;
using System.Collections;
using System.Threading.Tasks;

public class WhisperSpeechToText : MonoBehaviour
{
    public Text transcriptionText;  // UI Text to display the transcribed text
    public Text commandText;  // UI Text to display the parsed command
    private WhisperManager whisperManager;  // Reference to WhisperManager
    private AudioClip microphoneClip;  // Store the microphone recording
    private OpenAIService openAIService;  // Reference to the OpenAIService
    private CommandExecutor commandExecutor;  // Reference to CommandExecutor

    private bool isRecording = false;

    void Start()
    {
        // Get the WhisperManager component attached to the same GameObject
        whisperManager = GetComponent<WhisperManager>();
        
        // Find the OpenAIService component
        openAIService = GetComponent<OpenAIService>();

        // Find the CommandExecutor component
        commandExecutor = GetComponent<CommandExecutor>();

        transcriptionText.text = "Press Start Listening to begin...";
    }

    public void StartRecording()
    {
        // Start recording from the microphone
        if (!isRecording)
        {
            transcriptionText.text = "Listening...";
            microphoneClip = Microphone.Start(null, false, 10, 16000);  // Record for up to 10 seconds at 16kHz
            isRecording = true;
        }
    }

    public async void StopRecording()
    {
        // Stop recording
        if (isRecording)
        {
            Microphone.End(null);
            isRecording = false;

            transcriptionText.text = "Processing...";

            // Check if the audio clip was recorded successfully
            if (microphoneClip == null || microphoneClip.length == 0)
            {
                Debug.LogError("Audio clip is null or empty.");
                transcriptionText.text = "Failed to capture audio.";
                return;
            };

            // Convert to mono if necessary
            if (microphoneClip.channels > 1)
            {
                microphoneClip = ConvertToMono(microphoneClip);
                Debug.Log("Converted to mono.");
            }

            // Check sample rate (Whisper expects 16kHz or 44.1kHz)
            if (microphoneClip.frequency != 16000 && microphoneClip.frequency != 44100)
            {
                Debug.LogError("Invalid sample rate. Whisper expects 16kHz or 44.1kHz audio.");
                transcriptionText.text = "Invalid audio format.";
                return;
            }

            // Ensure Whisper model is loaded
            if (!whisperManager.IsLoaded)
            {
                Debug.LogError("Whisper model is not loaded. Please ensure the model file is correct.");
                transcriptionText.text = "Model not loaded.";
                return;
            }

            // Pass the audio clip to Whisper for transcription
            var transcription = await ProcessAudio(microphoneClip);

            if (!string.IsNullOrEmpty(transcription))
            {
                transcriptionText.text = "You said: " + transcription;

                // Send transcription to OpenAI to parse the command
                StartCoroutine(openAIService.ParseUserInput(transcription, (parsedCommand) =>
                {
                    Debug.Log("Parsed Command: " + parsedCommand);

                    if (!string.IsNullOrEmpty(parsedCommand))
                    {
                        commandText.text = "Command: " + parsedCommand;
                        commandExecutor.ExecuteCommand(parsedCommand);  // Execute the command
                    }
                    else
                    {
                        Debug.LogError("Parsed command is null or empty.");
                        commandText.text = "No command detected.";
                    }
                }));
            }
            else
            {
                transcriptionText.text = "Transcription failed.";
            }
        }
    }

    // Helper function to process audio with Whisper
    private async Task<string> ProcessAudio(AudioClip clip)
    {
        // Ensure the model is loaded
        await whisperManager.InitModel();

        // Get the transcription from the recorded audio
        var result = await whisperManager.GetTextAsync(clip);

        // Return the transcription result
        if (result != null && result.Segments.Count > 0)
        {
            return result.Segments[0].Text;  // Get the text from the first segment
        }
        return null;
    }

    // Convert a stereo AudioClip to mono
    private AudioClip ConvertToMono(AudioClip originalClip)
    {
        if (originalClip.channels == 1) 
        {
            // If the audio is already mono, no conversion is needed
            return originalClip;
        }

        // Get the stereo data from the original audio clip
        float[] data = new float[originalClip.samples * originalClip.channels];
        originalClip.GetData(data, 0);

        // Create a new float array to store the mono data
        float[] monoData = new float[originalClip.samples];

        // Convert stereo to mono by averaging the left and right channels
        for (int i = 0; i < originalClip.samples; i++)
        {
            // Average the left (data[i * 2]) and right (data[i * 2 + 1]) channels
            monoData[i] = (data[i * originalClip.channels] + data[i * originalClip.channels + 1]) / 2f;
        }

        // Create a new AudioClip with mono data
        AudioClip monoClip = AudioClip.Create(originalClip.name + "_mono", originalClip.samples, 1, originalClip.frequency, false);
        monoClip.SetData(monoData, 0);

        return monoClip;
    }
}
