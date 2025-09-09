using UnityEngine;

public class UIARChatItemChat : UIComponent<UIARChatItemChat.Entity>
{
    public class Entity
    {
        public string message;
        public UIImageComponent.Entity avatar;
    }

    public UIARChatMaxWidthSetter message;
    public UIImageComponent avatar;

    protected override void OnSetEntity()
    {
        message.SetText(entity.message);
        if (avatar != null) avatar.SetEntity(entity.avatar);
    }

    public void SetText(string text)
    {
        this.entity.message = text;
        message.SetText(text);
    }
}
