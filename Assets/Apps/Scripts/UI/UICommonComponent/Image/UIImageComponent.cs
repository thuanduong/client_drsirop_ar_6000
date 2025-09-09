using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIImageComponent : UIComponent<UIImageComponent.Entity>
{
    [System.Serializable]
    public class Entity
    {
        public Sprite sprite;
    }

    public Image image;

    protected override void OnSetEntity()
    {
        image.sprite = this.entity.sprite;
        if (image.sprite == default) image.enabled = false;
        else image.enabled = true;
    }

    private void Reset()
    {
        if (image == default) image = GetComponent<Image>();
    }
}
