using UnityEngine;

public class UIARChatInputVoice : PopupEntity<UIARChatInputVoice.Entity>
{
    public class Entity
    {
        public ButtonEntity btnInputText;
        public ButtonEntity btnRecord;
        public bool isRecording;
    }

    public UIButtonComponent btnInputText;
    public UIButtonComponent btnRecord;

    public IsVisibleComponent iconDefault;
    public IsVisibleComponent iconRecording;

    protected override void OnSetEntity()
    {
        btnInputText.SetEntity(entity.btnInputText);
        btnRecord.SetEntity(entity.btnRecord);
        iconDefault.SetEntity(!entity.isRecording);
        iconRecording.SetEntity(entity.isRecording);
    }

    public bool IsOn { get; set; } = false;

    public void ActiveRecording(bool active)
    {
        this.entity.isRecording = active;
        iconDefault.SetEntity(!entity.isRecording);
        iconRecording.SetEntity(entity.isRecording);
    }
}
