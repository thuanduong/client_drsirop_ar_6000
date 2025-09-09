using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Cysharp.Threading.Tasks;

using LiveKit;
using LiveKit.Proto;
using RoomOptions = LiveKit.RoomOptions;

public class RetellClientComponent : MonoBehaviour
{
    private const float UNMUTE_DELAY = 2.0f;
    private const float WAIT_DELAY = 3.0f;

    public event Action OnCallStarted;
    public event Action<string> OnError; // error message
    public event Action OnCallEnded;
    public event Action OnCallReady;
    public event Action OnAgentStartTalking;
    public event Action OnAgentStopTalking;
    public event Action<string, int, string> OnUpdate; // string (JSON) for update events
    public event Action<string> OnMetadata; // string (JSON) for metadata events
    public event Action<string> OnNodeTransition; // string (JSON) for node_transition events

    public bool IsAgentTalking { get; private set; } = false;
    public bool IsRecording { get; private set; } = false;


    Dictionary<string, GameObject> _audioObjects = new();
    List<AudioSource> _audioStreams = new();
    List<RtcAudioSource> _rtcAudioSources = new();

    // --- Room related ---
    private Room room;
    private bool isConnected = false;

    private Coroutine captureAudioCoroutine;
    private RemoteAudioTrack agentAudioTrack;
    private LocalAudioTrack localTrack;
    private AudioSource audioSourceRecord;
    private string microphoneDeviceName;
    private AudioClip recordingClip;
    private MicrophoneSource rtcSource;
    private Coroutine _muteCoroutine, _waitCoroutine;
    

    public async UniTask StartCall(string LiveKitHostUrl, StartCallConfig config, CancellationToken cancellationToken)
    {
        if (isConnected)
        {
            Debug.LogWarning("Already connected to call.");
            return;
        }

        try
        {
//#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
//            RtcAudioSource.DefaultMicrophoneSampleRate = 24000;
//#endif
            var roomOptions = new RoomOptions();
            room = new Room();
            room.DataReceived += DataReceived;
            room.TrackSubscribed += TrackSubscribed;
            room.TrackUnsubscribed += UnTrackSubscribed;
            room.Disconnected += OnDisconnected;
            room.ParticipantDisconnected += OnParticipantDisconnected;

            var connect = room.Connect(LiveKitHostUrl, config.accessToken, roomOptions);
            await connect.WithCancellation(cancellationToken);

            Debug.Log($"Connected to LiveKit room: {room.Name}");

            isConnected = true;
            OnCallStarted?.Invoke();
        }
        catch (Exception err)
        {
            OnError?.Invoke($"Error when start new call: {err.Message}");
            Debug.LogError($"Error when start new call: {err}");
            StopCall();
        }
    }

    public void StopCall()
    {
        if (!isConnected) return;

        isConnected = false;
        OnCallEnded?.Invoke();
        if (localTrack != null)
            localTrack = default;


        if (rtcSource != null)
        {
            rtcSource.Dispose();
        }

        foreach (var item in _audioObjects)
        {
            var source = item.Value.GetComponent<AudioSource>();
            if (source != null)
                source.Stop();
            Destroy(item.Value);
        }

        _audioObjects.Clear();


        foreach (var item in _rtcAudioSources)
        {
            item.Stop();
            item.Dispose();
        }
        _rtcAudioSources.Clear();

        foreach (var item in _audioStreams)
        {
            item.Stop();
        }
        _audioStreams.Clear();



        if (room != null)
        {
            room.Disconnect();
            room = null;
        }

        IsAgentTalking = false;

        if (captureAudioCoroutine != null)
        {
            StopCoroutine(captureAudioCoroutine);
            captureAudioCoroutine = null;
        }

        Debug.Log("The call is ended.");
    }

    private void DataReceived(byte[] data, Participant participant, DataPacketKind kind, string topic)
    {
        var str = System.Text.Encoding.Default.GetString(data);
        //Debug.Log("DataReceived: from " + participant.Identity + ", data " + str);

        if (participant?.Identity != "server") return;

        try
        {
            var eventData = JsonUtility.FromJson<EventBase>(str);

            switch (eventData.event_type)
            {
                case "update":
                    var updateData = JsonUtility.FromJson<UpdateEventBase>(str);
                    if (updateData != null)
                    {
                        if (updateData.transcript != null && updateData.transcript.Length > 0)
                        {
                            var last = updateData.transcript.Last();
                            OnUpdate?.Invoke(last.role, (updateData.transcript.Length - 1), last.content);
                        }
                        else Debug.LogError($"Null Data {str}");
                    }
                    else Debug.LogError($"Null Data {str}");

                    break;
                case "metadata":
                    OnMetadata?.Invoke(str);
                    break;
                case "agent_start_talking":
                    IsAgentTalking = true;
                    OnAgentStartTalking?.Invoke();
                    break;
                case "agent_stop_talking":
                    IsAgentTalking = false;
                    OnAgentStopTalking?.Invoke();
                    break;
                case "node_transition":
                    OnNodeTransition?.Invoke(str);
                    break;
                default:
                    Debug.LogWarning($"Unknown: {str}");
                    break;
            }
        }
        catch (Exception err)
        {
            Debug.LogError($"Error data : {err.Message}");
        }
    }

    private void TrackSubscribed(IRemoteTrack track, RemoteTrackPublication publication, RemoteParticipant participant)
    {
        if (track.Kind == TrackKind.KindAudio && track is RemoteAudioTrack audioTrack)
        {
            agentAudioTrack = audioTrack;
            OnCallReady?.Invoke();

            GameObject audObject = new GameObject(audioTrack.Sid);
            var source = audObject.AddComponent<AudioSource>();
            var stream = new AudioStream(audioTrack, source);
            _audioObjects[audioTrack.Sid] = audObject;
            _audioStreams.Add(source);
        }
    }

    private void UnTrackSubscribed(IRemoteTrack track, RemoteTrackPublication publication, RemoteParticipant participant)
    {
        if (track is RemoteAudioTrack audioTrack)
        {
            agentAudioTrack = default;
            OnCallReady?.Invoke();

            var audObject = _audioObjects[audioTrack.Sid];
            if (audObject != null)
            {
                var source = audObject.GetComponent<AudioSource>();
                source.Stop();
                _audioStreams.Remove(source);
                Destroy(audObject);
            }
            _audioObjects.Remove(audioTrack.Sid);

        }
    }

    private void OnDisconnected(Room room)
    {
        StopCall();
    }

    private void OnParticipantDisconnected(Participant participant)
    {
        if (participant?.Identity == "server")
        {
            StartCoroutine(DelayedStopCall(0.5f));
        }
    }

    private IEnumerator DelayedStopCall(float delay)
    {
        yield return new WaitForSeconds(delay);
        StopCall();
    }

    private void OnApplicationQuit()
    {
        StopCall();
    }

    private void OnDestroy()
    {
        StopCall();
    }

    public void StartRecording()
    {
        if (rtcSource == null)
        {
            Debug.LogError("No audio source.");
            return;
        }

        IsRecording = true;

        rtcSource.Start();
    }

    public void StopRecording()
    {
        IsRecording = false;
        if (rtcSource == default)
        {
            Debug.LogError("No audio source.");
            return;
        }

        rtcSource.Stop();
    }

    public void SetMuted(bool active)
    {
        if (rtcSource == null)
        {
            Debug.LogError("No audio source.");
            return;
        }

        if (active)
        {
            if (_muteCoroutine != null)
            {
                StopCoroutine(_muteCoroutine);
                _muteCoroutine = null;
            }
            rtcSource.SetMute(true);
            IsRecording = false;

        }
        else
        {
            if (_muteCoroutine != null)
            {
                StopCoroutine(_muteCoroutine);
            }

            _muteCoroutine = StartCoroutine(DelayToUnmute(UNMUTE_DELAY));
        }
    }

    private IEnumerator DelayToUnmute(float time)
    {
        yield return new WaitForSeconds(time);

        rtcSource.SetMute(false);
        IsRecording = true;
        _muteCoroutine = null;
    }

    public void WaitAgentTalk()
    {
        if (rtcSource == null)
        {
            Debug.LogError("No audio source.");
            return;
        }

        rtcSource.SetMute(true);
        IsRecording = false;

        if (_muteCoroutine != null)
        {
            StopCoroutine(_muteCoroutine);
        }

        _muteCoroutine = StartCoroutine(DelayToUnmute(WAIT_DELAY));
    }

    public void PublishMicrophone(int index, bool autoRecord)
    {
        StartCoroutine(PublishMicrophoneAsync(index, autoRecord));
    }

    IEnumerator RequestMicrophonePermission()
    {
        yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
        if (Application.HasUserAuthorization(UserAuthorization.Microphone))
        {
        }
    }

    public IEnumerator PublishMicrophoneAsync(int index, bool autoRecord)
    {
#if UNITY_ANDROID || UNITY_IOS
        if (!Application.HasUserAuthorization(UserAuthorization.Microphone))
        {
            yield return RequestMicrophonePermission();
        }
#endif

        if (room == null || room.LocalParticipant == null)
        {
            Debug.LogError("Room was not started!");
            yield break;
        }
        Debug.Log("publicMicrophone!");

        // Publish Microphone
        var localSid = "my-audio-source";
        GameObject audObject = new GameObject(localSid);
        _audioObjects[localSid] = audObject;

        rtcSource = new MicrophoneSource(Microphone.devices[index], audObject);
        Debug.Log($"CreateAudioTrack {rtcSource.SourceType}" );
        localTrack = LocalAudioTrack.CreateAudioTrack("my-audio-track", rtcSource, room);

        var options = new TrackPublishOptions();
        options.AudioEncoding = new AudioEncoding();
        options.AudioEncoding.MaxBitrate = 64000;
        options.Source = TrackSource.SourceMicrophone;

        Debug.Log("PublishTrack!");
        var publish = room.LocalParticipant.PublishTrack(localTrack, options);
        yield return publish;

        if (!publish.IsError)
        {
            Debug.Log("Track published!");
        }

        _rtcAudioSources.Add(rtcSource);

        AudioSource audiosource = audObject.GetComponent<AudioSource>();
        if (audiosource != default)
        {
            audioSourceRecord = audiosource;
            microphoneDeviceName = Microphone.devices[index];
        }

        if (autoRecord)
        {
            StartRecording();
            //rtcSource.SetMute(true);
        }

    }
}

public class StartCallConfig
{
    public string accessToken;
    public int? sampleRate; // nullable int
    public string captureDeviceId; // specific sink id for audio capture device
    public string playbackDeviceId; // specific sink id for audio playback device
    public bool emitRawAudioSamples = false; // receive raw float32 audio samples (for animation).
}

[System.Serializable]
public class EventBase
{
    public string event_type;
}


[System.Serializable]
public class UpdateEventBase
{
    public string event_type;
    public TranscriptItem[] transcript;
}

[System.Serializable]
public class TranscriptItem
{
    public string role;
    public string content;
}