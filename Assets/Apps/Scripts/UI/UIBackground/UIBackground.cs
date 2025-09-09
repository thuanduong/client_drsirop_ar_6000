using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBackground : PopupEntity<UIBackground.Entity>
{
    public class Entity
    {
        public Sprite sprite;
    }

    public Image image;
    public IsVisibleComponent DefaultBG;

    protected override void OnSetEntity()
    {
        image.sprite = entity.sprite;
        if (entity.sprite == default)
        {
            DefaultBG.SetEntity(true);
        }
        else
            DefaultBG.SetEntity(false);
    }
}
