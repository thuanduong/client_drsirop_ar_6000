using UnityEngine;


public class UIARAsk : PopupEntity<UIARAsk.Entity>
{
    public class Entity
    {
        public ButtonEntity btnEndCall;
        public ButtonEntity btnBack;
    }

    public UIButtonComponent btnEndCall;
    public UIButtonComponent btnBack;

    protected override void OnSetEntity()
    {
        btnEndCall.SetEntity(entity.btnEndCall);
        btnBack.SetEntity(entity.btnBack);
    }
}
