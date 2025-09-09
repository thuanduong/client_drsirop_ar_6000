using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIWaiting : PopupEntity<UIWaiting.Entity>
{
    public class Entity
    {
        public ButtonEntity btnOnBack;
    }

    public UIButtonComponent btnOnBack;

    protected override void OnSetEntity()
    {
        btnOnBack.SetEntity(entity.btnOnBack);
    }
}

