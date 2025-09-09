using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIMainMenu : PopupEntity<UIMainMenu.Entity>
{
    public class Entity
    {
        public ButtonEntity btnStartCall;
        public ButtonEntity btnStartChat;
    }


    public UIButtonComponent btnStartCall;
    public UIButtonComponent btnStartChat;

    protected override void OnSetEntity()
    {
        btnStartCall.SetEntity(entity.btnStartCall);
        btnStartChat.SetEntity(entity.btnStartChat);
    }
}
