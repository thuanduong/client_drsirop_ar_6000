using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class ColorTextComponent : UIComponent<ColorTextComponent.Entity>
{
    public class Entity
    {
        public string text;
        public Color color;
    }

    public TextMeshProUGUI text;

    private void Reset()
    {
        text ??= this.GetComponent<TextMeshProUGUI>();

    }

    protected override void OnSetEntity()
    {
        text.text = entity.text;
        text.color = entity.color;
    }

    public void SetText(string text)
    {
        this.entity ??= new Entity();
        this.entity.text = text;
        this.text.text = this.entity.text;
    }

    public void SetColor(Color color)
    {
        this.entity ??= new Entity();
        this.entity.color = color;
        this.text.color = this.entity.color;
    }
}
