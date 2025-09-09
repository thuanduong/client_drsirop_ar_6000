using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIPopupError : PopupEntity<UIPopupError.Entity>
{
    [System.Serializable]
    public class Entity
    {
        public string title;
        public string message;
        public string buttonTitle;
        public ButtonEntity confirmBtn;
    }

    public TextMeshProUGUI title;
    public TextMeshProUGUI message;
    public TextMeshProUGUI buttonTitle;
    public UIButtonComponent confirmBtn;

    protected override void OnSetEntity()
    {
        title.text = this.entity.title;
        message.text = this.entity.message;
        if (!string.IsNullOrEmpty(this.entity.buttonTitle))
            buttonTitle.text = this.entity.buttonTitle;
        confirmBtn.SetEntity(this.entity.confirmBtn);
    }
}
