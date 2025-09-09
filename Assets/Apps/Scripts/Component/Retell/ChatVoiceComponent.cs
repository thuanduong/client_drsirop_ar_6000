using System;
using WebSocketSharp;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;

using Cysharp.Threading.Tasks;

public class ChatVoiceComponent : MonoBehaviour
{
    private WebSocket ws;
    private bool isRecording = false;
    private CancellationTokenSource cts;

    [Header("Config")]
    public int sampleRate = 16000;
    public int channels = 1;

    public bool IsConnected { get; set; } = false;
    public bool IsRecording => isRecording;

    public event Action<string, bool> OnTranscriptReceived;
    public event Action<string> OnReplyReceived;

    private CancellationTokenSource recordCts;


    private AudioClip micClip;
    private AudioSource audioSource;

    private int lastPos = 0;
    private float updateTimer = 0f;
    private const float updateInterval = 0.1f; // 100ms

    private List<float> pendingSamples = new List<float>();
    private Queue<byte[]> audioQueue = new Queue<byte[]>();
    private bool isSending = false;
    private int silenceFrameCount = 0;
    private int maxSilenceFrames = 50;


    private void Start()
    {
        cts = new CancellationTokenSource();

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;
    }

    public void StartConnection(string serverUrl)
    {
        ConnectWebSocket(serverUrl, cts.Token).Forget();
    }

    void OnDestroy()
    {
        StopStreaming().Forget();
        if (audioSource != null) Destroy(audioSource);
    }

    private void Update()
    {
        UpdateRecord();
    }

    private void EnqueueAudio(byte[] buffer)
    {
        lock (audioQueue)
        {
            audioQueue.Enqueue(buffer);
        }
        if (!isSending)
        {
            isSending = true;
            SendLoop(recordCts.Token).Forget();
        }
    }

    private async UniTaskVoid SendLoop(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            byte[] chunk = null;
            lock (audioQueue)
            {
                if (audioQueue.Count > 0)
                    chunk = audioQueue.Dequeue();
                else
                {
                    isSending = false;
                    return;
                }
            }

            try
            {
                if (ws != null && ws.ReadyState == WebSocketSharp.WebSocketState.Open)
                    ws.Send(chunk);
            }
            catch (Exception ex)
            {
                Debug.LogError("SendLoop error: " + ex.Message);
            }

            await UniTask.Delay(20, cancellationToken: ct);
        }
    }

    void UpdateRecord()
    {
        if (!isRecording || micClip == null) return;

        updateTimer += Time.deltaTime;
        if (updateTimer < updateInterval) return;
        updateTimer = 0f;

        int pos = Microphone.GetPosition(null);
        int diff = pos - lastPos;
        if (diff < 0) diff += micClip.samples;

        if (diff > 0)
        {
            int channels = micClip.channels;
            float[] tempBuffer = new float[micClip.samples * channels];
            micClip.GetData(tempBuffer, 0);

            int length = diff * channels;
            float[] samples = new float[diff * channels];

            int srcIndex = lastPos * channels;
            int safeLength = Mathf.Min(length, tempBuffer.Length - srcIndex);
            if (safeLength > 0)
                Array.Copy(tempBuffer, srcIndex, samples, 0, safeLength);

            if (HasVoice(samples))
            {
                pendingSamples.AddRange(samples);
                silenceFrameCount = 0;

                int minSamples = (int)(sampleRate * channels * 0.5f);
                //if (pendingSamples.Count >= minSamples)
                //{
                //    float[] chunk = pendingSamples.ToArray();
                //    SendAudioChunk(chunk, recordCts.Token).Forget();
                //    //pendingSamples.Clear();
                //    pendingSamples.RemoveRange(0, minSamples);
                //}

                while (pendingSamples.Count >= minSamples)
                {
                    float[] chunk = pendingSamples.GetRange(0, minSamples).ToArray();
                    SendAudioChunk(chunk, recordCts.Token).Forget();
                    pendingSamples.RemoveRange(0, minSamples);
                }

                if (pendingSamples.Count > 0)
                {
                    float[] leftover = pendingSamples.ToArray();
                    SendAudioChunk(leftover, recordCts.Token).Forget();
                    pendingSamples.Clear();
                }
            }
            else
            {
                silenceFrameCount++;
                if (silenceFrameCount <= maxSilenceFrames)
                {
                    pendingSamples.AddRange(samples);

                    int minSamples = (int)(sampleRate * channels * 0.5f);

                    while (pendingSamples.Count >= minSamples)
                    {
                        float[] chunk = pendingSamples.GetRange(0, minSamples).ToArray();
                        SendAudioChunk(chunk, recordCts.Token).Forget();
                        pendingSamples.RemoveRange(0, minSamples);
                    }

                    if (pendingSamples.Count > 0)
                    {
                        float[] leftover = pendingSamples.ToArray();
                        SendAudioChunk(leftover, recordCts.Token).Forget();
                        pendingSamples.Clear();
                    }
                }
                //if (silenceFrameCount >= maxSilenceFrames)
                //{
                //    if (pendingSamples.Count > 0)
                //    {
                //        float[] finalChunk = pendingSamples.ToArray();
                //        SendAudioChunk(finalChunk, recordCts.Token, true).Forget();
                //        pendingSamples.Clear();
                //    }
                //}
            }

            lastPos = pos;
        }
    }

    public async UniTask ConnectWebSocket(string serverUrl, CancellationToken token)
    {
        try
        {
            ws = new WebSocket(serverUrl);
            var tcs = new UniTaskCompletionSource();


            ws.OnMessage += (sender, e) =>
            {
                try
                {
                    if (e.IsText)
                    {
                        var msg = JsonUtility.FromJson<ServerMsg>(e.Data);

                        switch (msg.type)
                        {
                            case "ready":
                                Debug.Log("Server ready, chat_id=" + msg.chat_id);
                                break;
                            case "ack":
                                Debug.Log("ACK: " + msg.status);
                                break;
                            case "transcript":
                                bool isFinal = msg.final;
                                Debug.Log("Transcript: " + msg.text);
                                OnTranscriptReceived?.Invoke(msg.text, isFinal);
                                break;
                            case "reply":
                                Debug.Log("Reply: " + msg.text);
                                OnReplyReceived?.Invoke(msg.text);
                                break;
                            default:
                                Debug.Log("WS Unknown: " + e.Data);
                                break;
                        }
                    }
                    else
                    {
                        Debug.Log("WS Binary len=" + e.RawData.Length);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError("OnMessage parse failed: " + ex.Message);
                }
            };

            ws.OnOpen += (sender, e) =>
            {
                Debug.Log("Connected to WS server");
                IsConnected = true;
                tcs.TrySetResult();
            };

            ws.OnError += (sender, e) =>
            {
                Debug.LogError("WS Error: " + e.Message);
                tcs.TrySetException(new Exception(e.Message));
            };

            ws.OnClose += (sender, e) =>
            {
                Debug.Log("WS Closed");
            };

            ws.Connect();

            await tcs.Task;
        }
        catch (Exception ex)
        {
            Debug.LogError("WebSocket connect failed: " + ex.Message);
        }
    }

    private async UniTask SendAudioChunk(float[] samples, CancellationToken ct, bool isFinalChunk = false)
    {
        int channels = micClip.channels;
        int length = samples.Length / channels;
        int resampleRate = 16000;

        // downmix stereo -> mono
        float[] mono = new float[length];
        for (int i = 0; i < length; i++)
        {
            float sum = 0f;
            for (int c = 0; c < channels; c++)
                sum += samples[i * channels + c];
            mono[i] = sum / channels;
        }

        // resample 44100 -> 16000
        float ratio = (float)mono.Length / (mono.Length * (16000f / sampleRate));
        int newLength = Mathf.CeilToInt(mono.Length * (16000f / sampleRate));
        float[] resampled = new float[newLength];

        for (int i = 0; i < newLength; i++)
        {
            float interp = i * ((float)mono.Length / newLength); 
            int index = (int)interp;
            float frac = interp - index;

            if (index + 1 < mono.Length)
                resampled[i] = Mathf.Lerp(mono[index], mono[index + 1], frac);
            else
                resampled[i] = mono[index];
        }

        if (isFinalChunk)
        {
            int silenceBufferLength = resampleRate / 2;
            Array.Resize(ref resampled, resampled.Length + silenceBufferLength);
        }

        float max = 0.0f;
        foreach(var f in resampled)
        {
            max = Mathf.Max(max, Mathf.Abs(f));
        }

        if (max > 0.0f && max < 0.5f)
        {
            float gain = 0.9f / max;
            for (int i = 0; i < resampled.Length; i++)
                resampled[i] *= gain;
        }


        // float -> PCM16
        byte[] buffer = new byte[resampled.Length * 2];
        int offset = 0;
        foreach (var f in resampled)
        {
            short s = (short)(Mathf.Clamp(f, -1f, 1f) * short.MaxValue);
            buffer[offset++] = (byte)(s & 0xFF);
            buffer[offset++] = (byte)((s >> 8) & 0xFF);
        }


        //ws.Send(buffer);
        EnqueueAudio(buffer);
        await UniTask.Yield(ct);
    }

    private bool HasVoice(float[] samples, float threshold = 0.01f)
    {
        double sum = 0;
        foreach (var s in samples)
            sum += s * s;
        double rms = Math.Sqrt(sum / samples.Length);
        return rms > threshold;
    }

    public async UniTask StartRecord()
    {
        if (ws == null || ws.ReadyState != WebSocketSharp.WebSocketState.Open)
        {
            Debug.LogWarning("Websocket not connected");
            return;
        }

        if (isRecording) return;
        
        lastPos = 0;

        isRecording = true;
        recordCts.SafeCancelAndDispose();
        recordCts = new CancellationTokenSource();

        micClip = Microphone.Start(null, true, 10, sampleRate);
        await UniTask.WaitUntil(() => Microphone.GetPosition(null) > 0);

        audioSource.clip = micClip;
        audioSource.loop = true;
        audioSource.Play();

        int totalSamples = micClip.samples * micClip.channels;
        int clipChannels = micClip.channels;

        lastPos = 0;
        await SendCommand("start");

    }

    public async UniTask StopRecord()
    {
        if (!isRecording) return;

        isRecording = false;
        recordCts.SafeCancelAndDispose();

        if (Microphone.IsRecording(null))
        {
            Microphone.End(null);
        }

        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        await SendCommand("stop");

        Debug.Log("Recording stopped");
    }

    public async UniTask StopStreaming()
    {
        try
        {
            IsConnected = false;

            await StopRecord();

            await SendCommand("cancel");

            if (ws != null && ws.ReadyState == WebSocketState.Open)
                ws.Close();

        }
        catch (Exception ex)
        {
            Debug.LogError("StopStreaming failed: " + ex.Message);
        }
    }

    public async UniTask SendTextMessage(string text)
    {
        try
        {
            if (ws == null || ws.ReadyState != WebSocketState.Open) return;

            var payload = new ClientMsg
            {
                type = "text",
                text = text
            };

            string json = JsonUtility.ToJson(payload);

            await UniTask.SwitchToThreadPool();
            ws.Send(json);
        }
        catch (Exception ex)
        {
            Debug.LogError("SendTextMessage failed: " + ex.Message);
        }
    }

    private async UniTask SendCommand(string cmd, string text = "")
    {
        try
        {
            if (ws == null || ws.ReadyState != WebSocketState.Open) return;

            var payload = new ClientMsg
            {
                type = cmd,
                text = text
            };

            string json = JsonUtility.ToJson(payload);

            await UniTask.SwitchToThreadPool();
            ws.Send(json);
            Debug.Log($"[WS] Sent cmd: {cmd}");
        }
        catch (Exception ex)
        {
            Debug.LogError("SendCommand failed: " + ex.Message);
        }
    }

    [Serializable]
    private class ServerMsg
    {
        public string type;
        public string chat_id;
        public string status;
        public string text;
        public bool final;
    }

    [Serializable]
    private class ClientMsg
    {
        public string type;
        public string text;
    }
}
