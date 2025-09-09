using UnityEngine;
using System;
using System.Threading;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Core;

public class ARChatPresenter : IDisposable
{
    private readonly IDIContainer Container;
    private CancellationTokenSource cts;

    private AIConversationService callService;
    private AIConversationService CallService => callService ??= this.Container.Inject<AIConversationService>();

    UIARChat uiChat;

    public System.Action OnBack = ActionUtility.EmptyAction.Instance;

    private List<int> typeOfMessages = new List<int>();
    private List<string> messages = new List<string>();

    private ChatVoiceComponent chatVoice;

    public ARChatPresenter(IDIContainer container)
    {
        this.Container = container;
    }

    public void Dispose()
    {
        cts.SafeCancelAndDispose();
        UILoader.SafeRelease(ref uiChat);

        if (chatVoice != null)
        {
            GameObject.Destroy(chatVoice.gameObject);
            chatVoice = null;
        }
    }

    public async UniTask<bool> Connect(CancellationToken cancellationToken)
    {
        try
        {
            chatVoice ??= await ObjectLoader.Instantiate<ChatVoiceComponent>("Object", Main.Instance.gameObject.transform, token: cancellationToken);
            
            var lm = UserData.Instance.GetOne<UserProfileModel>();
            
            await UniTask.Delay(100, cancellationToken: cancellationToken);

            await chatVoice.ConnectWebSocket(CallService.createWSPath(lm.UserId), cancellationToken);
            
        }
        catch
        {

        }
        return chatVoice.IsConnected;
    }


    public async UniTask Show(CancellationToken cancellationToken)
    {
        try
        {
            if (chatVoice != null)
            {
                chatVoice.gameObject.SetActive(true);
                chatVoice.OnTranscriptReceived += OnTranscriptReceived;
                chatVoice.OnReplyReceived += OnReplyReceived;
            }

            uiChat ??= await UILoader.Instantiate<UIARChat>(UICanvas.UICanvasType.Default, token: cancellationToken);
            await UniTask.Delay(100, cancellationToken: cancellationToken);
            var mmm = await getLastTranscription(cancellationToken);

            var entity = new UIARChat.Entity()
            {
                btnBack = new ButtonEntity(Chat_OnBtnBack, isDisable: true),
                inputText = new UIARChatInputText.Entity()
                {
                    btnInputVoice = new ButtonEntity(InputText_OnInputVoice),
                    btnSend = new ButtonEntity(InputText_OnSend),
                },
                inputVoice = new UIARChatInputVoice.Entity() { 
                    isRecording = false,
                    btnRecord = new ButtonEntity(InputVoice_OnRecord),
                    btnInputText = new ButtonEntity(InputVoice_OnInputText),
                },
                transcripts = new UIARChatListItemChat.Entity() { 
                    entities  = getListMessages(),
                    typeOfEntities = this.typeOfMessages,
                }
            };
            uiChat.SetEntity(entity);
            
            await uiChat.In().AttachExternalCancellation(cancellationToken);
            await UniTask.Delay(100, cancellationToken: cancellationToken);
            await uiChat.RefreshList(cancellationToken);
            await uiChat.ShowInputText(cancellationToken);
        }
        catch(Exception ex)
        {
            Debug.LogError($"Error: {ex.Message}");
        }
    }

    public async UniTask Hide(CancellationToken cancellationToken)
    {
        try
        {
            if (chatVoice != null)
            {
                chatVoice.gameObject.SetActive(false);

                chatVoice.OnTranscriptReceived -= OnTranscriptReceived;
                chatVoice.OnReplyReceived -= OnReplyReceived;
            }

            if (uiChat != null)
            {
                await uiChat.Out().AttachExternalCancellation(cancellationToken);
            }
            
        }
        catch { }
    }

    private void Chat_OnBtnBack()
    {
        //CallService.RequestEndChat();
        if (chatVoice != null)
            chatVoice.StopStreaming().Forget();

        OnBack.Invoke();
    }

    private void InputText_OnInputVoice()
    {
        cts.SafeCancelAndDispose();
        cts = new CancellationTokenSource();
        _ = InputText_OnInputVoiceAsync();
    }

    private async UniTask InputText_OnInputVoiceAsync()
    {
        try
        {
            await uiChat.ShowInputVoice(cts.Token);
        }
        catch
        {

        }
    }

    private void InputText_OnSend()
    {
        cts.SafeCancelAndDispose();
        cts = new CancellationTokenSource();
        _ = OnSendMessage();
    }

    private async UniTask OnSendMessage()
    {
        if (chatVoice == null)
            return;
        var message = uiChat.inputText.GetInput();
        uiChat.inputText.ClearInput();

        try
        {
            await chatVoice.SendTextMessage(message).AttachExternalCancellation(cts.Token);
            uiChat.AddMessageQueue("user", message, true);
        }
        catch
        {

        }
    }

    private void InputVoice_OnInputText()
    {
        cts.SafeCancelAndDispose();
        cts = new CancellationTokenSource();
        _ = InputVoice_OnInputTextAsync();
    }

    private async UniTask InputVoice_OnInputTextAsync()
    {
        try
        {
            await uiChat.ShowInputText(cts.Token);
        }
        catch
        {

        }
    }

    private void InputVoice_OnRecord()
    {
        cts.SafeCancelAndDispose();
        cts = new CancellationTokenSource();
        _ = InputVoice_OnRecordAsync();
    }

    private async UniTask InputVoice_OnRecordAsync()
    {
        try
        {
            if (chatVoice.IsRecording)
            {
                //disable Record
                uiChat.ActiveRecordVoice(false);
                await chatVoice.StopRecord();
            }
            else
            {
                //enable Record
                uiChat.ActiveRecordVoice(true);
                await chatVoice.StartRecord();
            }
        }
        catch
        {

        }
    }


    private async UniTask<bool> getLastTranscription(CancellationToken cancellationToken)
    {
        //this.typeOfMessages.Add(0);
        //this.messages.Add("Good evening. Welcome to The Corner Bistro. Do you have a reservation?");
        //this.typeOfMessages.Add(1);
        //this.messages.Add(" Good evening. No, we don't. Is there a table available for two people?");
        //this.typeOfMessages.Add(0);
        //this.messages.Add("Let me check... Yes, we have a table for you right away. Please follow me.");
        return true;
    }

    private List<UIARChatItemChat.Entity> getListMessages()
    {
        List<UIARChatItemChat.Entity> ml = new List<UIARChatItemChat.Entity>();
        int counter = this.messages.Count;

        for (int i = 0; i < counter; i++)
        {
            var item = this.messages[i];
            var type = this.typeOfMessages[i];
            if (type == 1)
            {
                ml.Add(new UIARChatItemChat.Entity()
                {
                    avatar = new UIImageComponent.Entity() { sprite = null },
                    message = item,
                });
            }
            else
            {
                ml.Add(new UIARChatItemChat.Entity() { avatar = null, message = item });
            }
        }

        return ml;
    }

    private void OnTranscriptReceived(string txt, bool isFinal)
    {
        //messageQueue.Enqueue(("user", txt, isFinal));
        uiChat.AddMessageQueue("user", txt, isFinal);
    }

    private void OnReplyReceived(string txt)
    {
        //messageQueue.Enqueue(("model", txt, true));
        //uiChat.AddMessage(0, txt);
        uiChat.AddMessageQueue("model", txt, true);
    }
}
