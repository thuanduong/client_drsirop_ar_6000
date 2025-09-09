using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIPopupYesNoMessage : PopupEntity<UIPopupYesNoMessage.Entity>
{
    [System.Serializable]
    public class Entity
    {
        public string title;
        public string message;
        public ButtonEntity yesBtn;
        public ButtonEntity noBtn;
    }

    public TextMeshProUGUI title;
    public TextMeshProUGUI message;
    public UIButtonComponent yesBtn;
    public UIButtonComponent noBtn;

    protected override void OnSetEntity()
    {

        title.text = this.entity.title;
        message.text = this.entity.message;
        yesBtn.SetEntity(this.entity.yesBtn);
        noBtn.SetEntity(this.entity.noBtn);
    }

}
