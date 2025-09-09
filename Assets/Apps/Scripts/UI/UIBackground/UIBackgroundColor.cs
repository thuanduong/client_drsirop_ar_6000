using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class UIBackgroundColor : PopupEntity<UIBackgroundColor.Entity>
{
    public class Entity
    {
        public Color defaultColor;
    }

    public Image target;

    protected override void OnSetEntity()
    {
        target.color = entity.defaultColor;
    }

    public Tween ChangeColor(Color color, float duration)
    {
        return target.DOColor(color, duration);
    }

}
