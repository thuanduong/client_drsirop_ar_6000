using UnityEngine;
using TMPro;

public class UICharacterMessageBubble : PopupEntity<UICharacterMessageBubble.Entity>
{
    public class Entity
    {
        public string message;
    }

    public TextMeshProUGUI message;

    protected override void OnSetEntity()
    {
        message.SetText(entity.message);
    }

    public void SetText(string value)
    {
        if (entity != null)
            entity.message = value;
        message.SetText(value);
    }
}
