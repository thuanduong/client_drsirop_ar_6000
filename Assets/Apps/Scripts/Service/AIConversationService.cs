using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Text;

using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using Data;

public class AIConversationService : IDisposable
{
    private readonly IDIContainer container;
    private CancellationTokenSource cts;

    private static AIConversationService instance;
    public static AIConversationService Instance => instance;

    private const string createWebCall = "create-web-call";
    private const string createWebChat = "start-web-chat";
    private const string endWebChat = "end-chat";
    private const string sendMessage = "send-mess";

    private const string createWSVoiceChat = "voice_chat";

    private RetellClientComponent retellClient;

    public event Action OnConnectSucceed = ActionUtility.EmptyAction.Instance;
    public event Action<string> OnError = ActionUtility.EmptyAction<string>.Instance;

    public event Action OnCallStarted = ActionUtility.EmptyAction.Instance;
    public event Action OnCallEnded = ActionUtility.EmptyAction.Instance;
    public event Action OnCallReady = ActionUtility.EmptyAction.Instance;

    public event Action OnChatStarted = ActionUtility.EmptyAction.Instance;
    public event Action OnChatEnd = ActionUtility.EmptyAction.Instance;

    public event Action OnAgentStartTalking = ActionUtility.EmptyAction.Instance;
    public event Action OnAgentStopTalking = ActionUtility.EmptyAction.Instance;

    public event Action<string, int, string> OnUpdate = ActionUtility.EmptyAction<string, int, string>.Instance;
    
    public string CurrentCallID { get; private set; }
    private int chatCounter = 0;

    private AIConversationService(IDIContainer container)
    {
        this.container = container;
        cts = new CancellationTokenSource();
    }

    public void Dispose()
    {
        cts.SafeCancelAndDispose();
        UnregisterEvent();

        if (retellClient != null)
            GameObject.Destroy(retellClient);

        instance = default;
    }

    public static AIConversationService Instantiate(IDIContainer container)
    {
        if (instance == default)
        {
            instance = new AIConversationService(container);
        }
        return instance;
    }

    public async UniTask Create(CancellationToken cancellationToken)
    {
        var main = Main.Instance.gameObject;
        var token = cancellationToken != null ? cancellationToken : cts.Token;
        try
        {
            retellClient ??= await ObjectLoader.Instantiate<RetellClientComponent>("Object", main.transform, token: token);

            RegisterEvent();
        }
        catch { }

    }

    private void RegisterEvent()
    {
        if (retellClient != null)
        {
            retellClient.OnCallStarted += OnCallStarted;
            retellClient.OnCallEnded += OnCallEnded;
            retellClient.OnError += OnError;
            retellClient.OnAgentStartTalking += OnAgentStartTalking;
            retellClient.OnAgentStopTalking += OnAgentStopTalking;
            retellClient.OnUpdate += OnUpdate;
        }
    }

    private void UnregisterEvent()
    {
        if (retellClient != null)
        {
            retellClient.OnCallStarted -= OnCallStarted;
            retellClient.OnCallEnded -= OnCallEnded;
            retellClient.OnError -= OnError;
            retellClient.OnAgentStartTalking -= OnAgentStartTalking;
            retellClient.OnAgentStopTalking -= OnAgentStopTalking;
            retellClient.OnUpdate -= OnUpdate;
        }
    }

    public async UniTask<bool> RequestCall(string agentId, string userId, CancellationToken cancellationToken)
    {
        try
        {
            var requestData = new RetellConnectRequest { agent_id = agentId, user_id = userId, llm_type = "gpt" };
            string json = JsonUtility.ToJson(requestData);
            var path = ServerDefinePath.GetPath(createWebCall);
            var token = cancellationToken != null ? cancellationToken : cts.Token;
            chatCounter = 0;
            using (var request = WebService.CreateWebRequestWithBody(path, json))
            {
                await request.SendWebRequest().WithCancellation(token);
                if (request.result == UnityWebRequest.Result.Success)
                {
                    string responseJson = request.downloadHandler.text; Debug.Log(responseJson);
                    RetellConnectResponse apiResponse = JsonUtility.FromJson<RetellConnectResponse>(responseJson);

                    string liveKitAccessToken = apiResponse.access_token;
                    string url = apiResponse.url;
                    var callConfig = new StartCallConfig
                    {
                        accessToken = liveKitAccessToken,
                        sampleRate = 24000,
                    };

                    OnConnectSucceed.Invoke();

                    CurrentCallID = apiResponse.call_id;

                    await retellClient.StartCall(url, callConfig, token);
                    Debug.Log("Start Call");
                }
                else
                {
                    string errorResponse = request.downloadHandler.text;
                    try
                    {
                        RetellErrorResponse err = JsonUtility.FromJson<RetellErrorResponse>(errorResponse);
                        Debug.LogError($"err Node.js: {err.error} - {err.details}");
                    }
                    catch
                    {
                        Debug.LogError($"Err JSON: {errorResponse}");
                    }
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            return false;
        }
        return true;
    }

    public void RequestEndCall()
    {
        retellClient.StopCall();
    }

    public async UniTask<bool> RequestChat(string agentId, string userId, CancellationToken cancellationToken)
    {
        try
        {
            var requestData = new RetellConnectRequest { agent_id = agentId, user_id = userId };
            string json = JsonUtility.ToJson(requestData);

            var token = cancellationToken != null ? cancellationToken : cts.Token;
            var path = ServerDefinePath.GetPath(createWebChat);
            chatCounter = 0;
            using (var request = WebService.CreateWebRequestWithBody(path, json))
            {
                await request.SendWebRequest().WithCancellation(token);

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string responseJson = request.downloadHandler.text; Debug.Log(responseJson);
                    RetellStartChatReponse apiResponse = JsonUtility.FromJson<RetellStartChatReponse>(responseJson);
                    CurrentCallID = apiResponse.chat_id;

                    OnChatStarted();
                    OnUpdate("model", chatCounter, apiResponse.initial_message);
                    chatCounter++;
                }
                else
                {
                    string errorResponse = request.downloadHandler.text;
                    try
                    {
                        RetellErrorResponse err = JsonUtility.FromJson<RetellErrorResponse>(errorResponse);
                        Debug.LogError($"err Node.js: {err.error} - {err.details}");
                    }
                    catch
                    {
                        Debug.LogError($"Err JSON: {errorResponse}");
                    }
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            return false;
        }
        return true;
    }

    public async UniTask<bool> RequestEndChat(CancellationToken cancellationToken)
    {
        try
        {
            var path = ServerDefinePath.GetPath($"{endWebChat}/{CurrentCallID}");
            var token = cancellationToken != null ? cancellationToken : cts.Token;
            using (var request = WebService.CreateWebRequest(path))
            {
                await request.SendWebRequest().WithCancellation(token);

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string responseJson = request.downloadHandler.text; Debug.Log(responseJson);
                    RetellEndChatReponse apiResponse = JsonUtility.FromJson<RetellEndChatReponse>(responseJson);
                    CurrentCallID = "";
                    OnChatEnd();
                }
                else
                {
                    string errorResponse = request.downloadHandler.text;
                    try
                    {
                        RetellErrorResponse err = JsonUtility.FromJson<RetellErrorResponse>(errorResponse);
                        Debug.LogError($"err Node.js: {err.error} - {err.details}");
                    }
                    catch
                    {
                        Debug.LogError($"Err JSON: {errorResponse}");
                    }
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            return false;
        }
        return true;
    }

    public async UniTask<bool> RequestSendMessage(string message, CancellationToken cancellationToken)
    {
        try
        {
            var requestData = new RetellSendChatRequest { message = message };
            string json = JsonUtility.ToJson(requestData);
            var path = ServerDefinePath.GetPath($"{sendMessage}/{CurrentCallID}");
            var token = cancellationToken != null ? cancellationToken : cts.Token;
            using (var request = WebService.CreateWebRequestWithBody(path, json))
            {
                OnUpdate("user", chatCounter, message); 
                chatCounter++;

                await request.SendWebRequest().WithCancellation(token);

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string responseJson = request.downloadHandler.text; Debug.Log(responseJson);
                    RetellReceiveChatReponse apiResponse = JsonUtility.FromJson<RetellReceiveChatReponse>(responseJson);
                    OnUpdate("model", chatCounter, apiResponse.message); chatCounter++;
                }
                else
                {
                    string errorResponse = request.downloadHandler.text;
                    Debug.LogError($"Err JSON: {errorResponse}");
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            return false;
        }
        return true;
    }

    public void PublishMicrophone(int index = 0, bool autoRecord = false)
    {
        if (retellClient != null)
            retellClient.PublishMicrophone(index, autoRecord);
    }

    public void StartTalk()
    {
        if (retellClient != null)
            retellClient.SetMuted(true);
        Debug.Log("Start Talk");
    }

    public void StopTalk()
    {
        if (retellClient != null)
            retellClient.SetMuted(false);
        Debug.Log("End Talk");
    }

    public void WaitAgentTalk()
    {
        if (retellClient != null)
            retellClient.WaitAgentTalk();
    }

    public string createWSPath(string userId)
    {
        var path = $"{ServerDefinePath.WS}/{createWSVoiceChat}/{userId}";
        return path;
    }
}
