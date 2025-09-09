using UnityEngine;
using TMPro;

public class UIARChatInputText : PopupEntity<UIARChatInputText.Entity>
{
    public class Entity
    {
        public ButtonEntity btnInputVoice;
        public ButtonEntity btnSend;
    }

    public UIButtonComponent btnInputVoice;
    public UIButtonComponent btnSend;
    public TMP_InputField inputField;

    protected override void OnSetEntity()
    {
        btnInputVoice.SetEntity(entity.btnInputVoice);
        btnSend.SetEntity(entity.btnSend);
        inputField.text = "";
    }

    public void ClearInput()
    {
        inputField.text = "";
    }

    public string GetInput()
    {
        return inputField.text;
    }

    public bool IsOn { get; set; } = false;
}
