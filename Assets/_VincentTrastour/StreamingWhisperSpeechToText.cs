using UnityEngine;
using UnityEngine.UI;
using Whisper;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using TMPro;

public class StreamWhisperSpeechToText : MonoBehaviour
{
    public TextMeshPro transcriptionText;  // UI Text to display the transcribed text
    public TextMeshPro commandText;  // UI Text to display the parsed command
    private OpenAIService openAIService;  // Reference to the OpenAIService
    private CommandExecutor commandExecutor;  // Reference to CommandExecutor

    private WhisperManager whisperManager;  // Reference to WhisperManager
    private AudioClip microphoneClip;  // Microphone recording clip

    private bool isRecording = false;
    private int sampleRate = 16000;  // Sample rate for recording
    private int recordingLength = 30; // Length of the recording buffer in seconds

    private int lastSamplePosition = 0; // Last sample position in the microphone clip
    private List<float> accumulatedSamples = new List<float>(); // Accumulated samples for processing

    // Parameters for silence detection
    private float silenceThreshold = 0.01f; // Threshold for detecting silence (adjust as needed)
    private float silenceDuration = 1.0f;   // Duration of silence to trigger processing (in seconds)
    private float maxRecordingDuration = 10f; // Max duration to force processing (in seconds)

    private float silenceTimer = 0f; // Timer to track duration of silence
    private float speechTimer = 0f;  // Timer to track duration of speech

    async void Start()
    {
        // Get the WhisperManager component attached to the same GameObject
        whisperManager = GetComponent<WhisperManager>();

        // Find the OpenAIService component
        openAIService = GetComponent<OpenAIService>();

        // Find the CommandExecutor component
        commandExecutor = GetComponent<CommandExecutor>();

        transcriptionText.text = "Initializing model...";

        // Initialize the Whisper model
        await whisperManager.InitModel();

        if (!whisperManager.IsLoaded)
        {
            Debug.LogError("Failed to load Whisper model.");
            transcriptionText.text = "Failed to load model.";
        }
        else
        {
            transcriptionText.text = "Press Start Listening to begin...";
        }
    }
    [ContextMenu("StartRecording")]
    public void StartRecording()
    {
        if (!isRecording)
        {
            transcriptionText.text = "Listening...";
            isRecording = true;

            // Start recording with a specified length and sample rate
            microphoneClip = Microphone.Start(null, true, recordingLength, sampleRate);

            lastSamplePosition = 0;
            accumulatedSamples.Clear();

            silenceTimer = 0f;
            speechTimer = 0f;

            StartCoroutine(StreamAudio());
        }
    }

    [ContextMenu("StopRecording")]
    public void StopRecording()
    {
        if (isRecording)
        {
            Microphone.End(null);
            isRecording = false;
            transcriptionText.text = "Stopped listening.";
        }
    }

    private IEnumerator StreamAudio()
    {
        while (isRecording)
        {
            int currentPosition = Microphone.GetPosition(null);

            if (currentPosition < 0 || currentPosition == lastSamplePosition)
            {
                yield return null;
                continue;
            }

            int samplesToRead = 0;

            if (currentPosition > lastSamplePosition)
            {
                samplesToRead = currentPosition - lastSamplePosition;
            }
            else
            {
                samplesToRead = (microphoneClip.samples - lastSamplePosition) + currentPosition;
            }

            float[] samples = new float[samplesToRead];

            microphoneClip.GetData(samples, lastSamplePosition % microphoneClip.samples);

            lastSamplePosition = currentPosition;

            accumulatedSamples.AddRange(samples);

            // Calculate the RMS value of the current samples
            float rmsValue = CalculateRMS(samples);

            // Debug log the RMS value
            // Debug.Log("RMS Value: " + rmsValue);

            // Check if the RMS value is below the silence threshold
            if (rmsValue < silenceThreshold)
            {
                silenceTimer += (float)samples.Length / sampleRate;
                speechTimer = 0f;
            }
            else
            {
                speechTimer += (float)samples.Length / sampleRate;
                silenceTimer = 0f;
            }

            // If silence has been detected for the required duration, process the audio chunk
            if (silenceTimer >= silenceDuration && accumulatedSamples.Count > 0)
            {
                float[] samplesToProcess = accumulatedSamples.ToArray();
                accumulatedSamples.Clear();
                silenceTimer = 0f;
                speechTimer = 0f;

                // Start processing the audio chunk
                yield return ProcessAudioChunk(samplesToProcess);
            }
            else if (speechTimer >= maxRecordingDuration)
            {
                // Force processing if max recording duration is reached
                float[] samplesToProcess = accumulatedSamples.ToArray();
                accumulatedSamples.Clear();
                silenceTimer = 0f;
                speechTimer = 0f;

                yield return ProcessAudioChunk(samplesToProcess);
            }

            yield return null;
        }
    }

    // Function to calculate RMS value of audio samples
    private float CalculateRMS(float[] samples)
    {
        float sum = 0f;
        for (int i = 0; i < samples.Length; i++)
        {
            sum += samples[i] * samples[i];
        }
        return Mathf.Sqrt(sum / samples.Length);
    }

    private IEnumerator ProcessAudioChunk(float[] samples)
    {
        Debug.Log("Processing audio chunk with sample length: " + samples.Length);

        // Create an AudioClip from the samples
        AudioClip chunkClip = AudioClip.Create("chunk", samples.Length, 1, sampleRate, false);
        chunkClip.SetData(samples, 0);

        // Ensure the Whisper model is loaded
        if (!whisperManager.IsLoaded)
        {
            Debug.LogError("Whisper model is not loaded.");
            transcriptionText.text = "Model not loaded.";
            yield break;
        }

        // Transcribe the audio chunk asynchronously
        var transcriptionTask = ProcessAudio(chunkClip);

        // Wait for the transcription to complete without blocking the main thread
        yield return new WaitUntil(() => transcriptionTask.IsCompleted);

        if (transcriptionTask.Exception != null)
        {
            Debug.LogError("Transcription task failed: " + transcriptionTask.Exception);
            yield break;
        }

        var transcription = transcriptionTask.Result;

        Debug.Log("-=-=-====-=-=-=--=-=-=--=");
        Debug.Log("Transcription:->" + transcription);
        Debug.Log("-=-=-====-=-=-=--=-=-=--=");

        if (!string.IsNullOrEmpty(transcription) && !transcription.Contains("BLANK_AUDIO"))
        {
            transcriptionText.text = "You said: " + transcription;

            // Stop recording while processing the command or normal text response
            StopRecording();

            StartCoroutine(openAIService.ParseUserInput(transcription, (parsedCommand) =>
            {
                Debug.Log("Parsed Command: " + parsedCommand);

                if (!string.IsNullOrEmpty(parsedCommand) && parsedCommand != "none")
                {
                    commandExecutor.ExecuteCommand(parsedCommand);  // Execute the command
                    // Resume recording after command execution
                    StartCoroutine(ResumeRecordingAfterCommand());
                }
                else
                {
                    // Handle normal text response
                    StartCoroutine(openAIService.GetResponseForText(transcription, (response) =>
                    {
                        if (!string.IsNullOrEmpty(response))
                        {
                            StartCoroutine(openAIService.GenerateSpeech(response, "alloy", (audioClip) =>
                            {
                                if (audioClip != null)
                                {
                                    // Play the generated audio and block further input until done
                                    AudioSource audioSource = openAIService.GetComponent<AudioSource>();
                                    if (audioSource == null)
                                    {
                                        audioSource = openAIService.gameObject.AddComponent<AudioSource>();
                                    }

                                    audioSource.clip = audioClip;
                                    audioSource.Play();

                                    // Block further input until the audio is done playing
                                    StartCoroutine(BlockInputUntilAudioComplete(audioSource));
                                }
                                else
                                {
                                    Debug.LogError("Failed to generate speech.");
                                    // Resume recording if TTS fails
                                    StartRecording();
                                }
                            }));
                        }
                        else
                        {
                            // Resume recording if no response
                            StartRecording();
                        }
                    }));
                }
            }));
        }
        else
        {
            Debug.LogWarning("Transcription was empty or blank audio.");
            // Resume recording if transcription is empty or blank audio
            StartRecording();
        }
    }

    private IEnumerator BlockInputUntilAudioComplete(AudioSource audioSource)
    {
        while (audioSource.isPlaying)
        {
            yield return null;
        }
        transcriptionText.text = "Press Start Listening to begin...";
        // Resume recording after TTS playback is complete
        StartRecording();
    }

    private IEnumerator ResumeRecordingAfterCommand()
    {
        // Wait for a short duration to ensure command execution is complete
        yield return new WaitForSeconds(1.0f);
        StartRecording();
    }

    // Asynchronous helper function to process audio with Whisper
    private async Task<string> ProcessAudio(AudioClip clip)
    {
        try
        {
            Debug.Log("Processing audio...");

            var result = await whisperManager.GetTextAsync(clip);

            if (result == null)
            {
                Debug.LogError("WhisperManager returned a null result.");
                return null;
            }

            if (result.Segments == null)
            {
                Debug.LogError("WhisperManager result has null Segments.");
                return null;
            }

            if (result.Segments.Count == 0)
            {
                Debug.LogWarning("WhisperManager returned zero segments.");
                return null;
            }

            Debug.Log("WhisperManager returned " + result.Segments.Count + " segments.");

            string transcription = "";
            foreach (var segment in result.Segments)
            {
                transcription += segment.Text;
                // Debug.Log("Segment text: " + segment.Text);
            }

            return transcription;
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Exception in ProcessAudio: " + ex);
            return null;
        }
    }
}
