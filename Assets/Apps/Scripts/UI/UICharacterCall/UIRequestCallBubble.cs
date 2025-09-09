using UnityEngine;

public class UIRequestCallBubble : PopupEntity<UIRequestCallBubble.Entity>
{
    public class Entity
    {
        public ButtonEntity btnStartCall;
    }

    public UIButtonComponent btnStartCall;

    protected override void OnSetEntity()
    {
        btnStartCall.SetEntity(entity.btnStartCall);
    }
}
