using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIPopupSetting : PopupEntity<UIPopupSetting.Entity>
{
    [System.Serializable]
    public class Entity
    {
        public UIProgressBarComponent.Entity sfxSlider;
        public UIProgressBarComponent.Entity bgmSlider;
        public ButtonEntity outerBtn;
        public ButtonEntity closeBtn;
        public ButtonEntity logOutBtn;
    }

    public UIProgressBarComponent sfxSlider;
    public UIProgressBarComponent bgmSlider;
    public UIButtonComponent outerBtn;
    public UIButtonComponent closeBtn;
    public UIButtonComponent logOutBtn;

    protected override void OnSetEntity()
    {
        sfxSlider.SetEntity(this.entity.sfxSlider);
        bgmSlider.SetEntity(this.entity.bgmSlider);
        outerBtn.SetEntity(this.entity.outerBtn);
        closeBtn.SetEntity(this.entity.closeBtn);
        logOutBtn.SetEntity(this.entity.logOutBtn);
    }
}
