using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class UIHeader : PopupEntity<UIHeader.Entity>
{
    [System.Serializable]
    public class Entity
    {
        public ButtonEntity Back;
        public ButtonEntity Settings;
        public bool BackVisible;
        public bool SettingVisible;

    }

    public UIButtonComponent Back;
    public UIButtonComponent Settings;
    public IsVisibleComponent BackVisible;
    public IsVisibleComponent SettingVisible;

    protected override void OnSetEntity()
    {
        Back.SetEntity(this.entity.Back);
        Settings.SetEntity(this.entity.Settings);
        BackVisible.SetEntity(entity.BackVisible);
        SettingVisible.SetEntity(entity.SettingVisible);
    }

}
