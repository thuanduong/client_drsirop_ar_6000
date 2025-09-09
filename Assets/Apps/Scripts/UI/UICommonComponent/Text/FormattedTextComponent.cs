using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class FormattedTextComponent : UIComponent<FormattedTextComponent.Entity>
{
    [Serializable]
    public class Entity
    {
        public object[] param;
        public Entity()
        {

        }

        public Entity(object value)
        {
            param = new object[] { value };
        }
        public Entity(params object[] value)
        {
            param = value;
        }
    }

    public TextMeshProUGUI text;
    [Multiline]
    public string format = "{0}";
    public bool IsColor = false;

    public void SetEntity(object value)
    {
        if (value is FormattedTextComponent.Entity entity)
        {
            this.entity = entity;
        }
        else
        {
            this.entity = new Entity()
            {
                param = new object[] { value }
            };
        }
        OnSetEntity();
    }

    public void SetEntity(params object[] param)
    {
        this.entity ??= new Entity();
        this.entity.param = param;
        OnSetEntity();
    }
    
    public void SetWithArrayEntity(object[] param)
    {
        this.entity ??= new Entity();
        this.entity.param = param;
        OnSetEntity();
    }

    protected override void OnSetEntity()
    {
		if (entity.param != null && entity.param.Length > 0)
		{
            if (IsColor)
            {
                string coloredParam0 = $"<color=#FFE100>     {entity.param[0]}</color>";


                object[] coloredParams = new object[entity.param.Length];
                coloredParams[0] = coloredParam0;


                for (int i = 1; i < entity.param.Length; i++)
                {
                    coloredParams[i] = entity.param[i];
                }


                text.text = string.Format(format, coloredParams);
            }
            else
                text.text = string.Format(format, entity.param);
        }
		else
		{
			
			text.text = string.Format(format);
		}
    }

    void Reset()
    {
        text ??= this.GetComponent<TextMeshProUGUI>();
    }
}
