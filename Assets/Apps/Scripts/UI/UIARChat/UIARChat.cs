using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections;

public class UIARChat : PopupEntity<UIARChat.Entity>
{
    public class Entity
    {
        public ButtonEntity btnBack;
        public UIARChatListItemChat.Entity transcripts;
        public UIARChatInputText.Entity inputText;
        public UIARChatInputVoice.Entity inputVoice;

    }

    public UIButtonComponent btnBack;
    public UIARChatListItemChat transcripts;
    public UIARChatInputText inputText;
    public UIARChatInputVoice inputVoice;

    private int chatCounter;
    private ConcurrentQueue<(string, string, bool)> messageQueue = new ConcurrentQueue<(string, string, bool)>();

    private Coroutine pnCoroutine;
    private float waitTime;
    private float currentTime;

    protected override void OnSetEntity()
    {
        btnBack.SetEntity(entity.btnBack);
        transcripts.SetEntity(entity.transcripts);
        inputText.SetEntity(entity.inputText);
        inputVoice.SetEntity(entity.inputVoice);
    }

    public void AddMessageQueue(string role, string message, bool isFinal)
    {
        messageQueue.Enqueue((role, message, isFinal));
    }

    void Update()
    {
        while (messageQueue.TryDequeue(out var msg))
        {
            OnMessageQueue(msg.Item1, msg.Item2, msg.Item3);
        }
    }

    public void AddMessage(int type, string message, UIImageComponent.Entity img = null)
    {
        var e = new UIARChatItemChat.Entity() { avatar = img, message = message };
        transcripts.AddEntity(type, e);
        delayToUpdate();
    }

    void delayToUpdate()
    {
        if (pnCoroutine != null)
        {
            currentTime = 0;
        }
        else
        {
            waitTime = 0.1f;
            pnCoroutine = StartCoroutine(doWaitForUpdate());
        }
    }
    
    IEnumerator doWaitForUpdate()
    {
        transcripts.Container.SetActive(false);
        while (currentTime < waitTime)
        {
            currentTime += Time.deltaTime;
            yield return null;
        }
        transcripts.Container.SetActive(true);
        pnCoroutine = null;
    }


    public async UniTask ShowInputText(CancellationToken cancellationToken)
    {
        if (inputVoice.IsOn)
        {
            await inputVoice.Out().AttachExternalCancellation(cancellationToken);
            inputVoice.IsOn = false;
        }
        await inputText.In().AttachExternalCancellation(cancellationToken);
        inputText.IsOn = true;
    }

    public async UniTask ShowInputVoice(CancellationToken cancellationToken)
    {
        if (inputText.IsOn)
        {
            await inputText.Out().AttachExternalCancellation(cancellationToken);
            inputText.IsOn = false;
        }
        await inputVoice.In().AttachExternalCancellation(cancellationToken);
        inputVoice.IsOn = true;
    }

    public async UniTask RefreshList(CancellationToken cancellationToken)
    {
        await transcripts.RefreshList(cancellationToken);
    }

    private void OnMessageQueue(string role, string txt, bool isFinal)
    {
        if (isFinal)
        {
            HandleNewMessage(role, chatCounter, txt);
            chatCounter++;
        }
        //else
        //    HandleNewMessage(role, chatCounter, txt);
    }

    void HandleNewMessage(string role, int index, string message)
    {
        bool isPlayer = role == "user" ? true : false;
        AddMessage(isPlayer ? 1 : 0, message);
    }

    public void ActiveRecordVoice(bool val)
    {
        this.inputVoice.ActiveRecording(val);
    }
}
