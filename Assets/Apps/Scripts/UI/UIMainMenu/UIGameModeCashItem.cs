using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class UIGameModeCashItem : UISliderItem
{
    public class CashEntity : Entity
    {
        public ButtonEntity button;
        public UIImageComponent.Entity Icon;
        public string BuyIn;
    }

    public UIButtonComponent button;
    public UIImageComponent Icon;
    public TextMeshProUGUI BuyIn;

    protected override void OnSetEntity()
    {
        base.OnSetEntity();
        var _entity = entity as CashEntity;
        button.SetEntity(_entity.button);
        Icon.SetEntity(_entity.Icon);
        BuyIn.text = _entity.BuyIn;
    }
}
