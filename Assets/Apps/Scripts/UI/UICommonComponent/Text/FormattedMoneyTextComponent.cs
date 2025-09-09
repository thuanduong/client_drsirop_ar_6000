using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Globalization;

public class FormattedMoneyTextComponent : UIComponent<FormattedMoneyTextComponent.Entity>
{
    public class Entity
    {
        public double Money;

        public Entity()
        {

        }

        public Entity(long value)
        {
            Money = value;
        }

        public Entity(int value)
        {
            Money = (double)value;
        }

        public Entity(double value)
        {
            Money = value;
        }
    }

    public TextMeshProUGUI text;
    public Format format = Format.Simple;
    public string TextFormat;

    public enum Format
    {
        Simple,
        SimpleWithFormat,
        Number,
        Money,
        MoneyWithFormat
    }

    protected override void OnSetEntity()
    {
        text.text = GetFormat(this.entity.Money, format, TextFormat);
    }

    public void SetEntity(object value)
    {
        if (value is FormattedMoneyTextComponent.Entity entity)
        {
            this.entity = entity;
        }
        else
        {
            this.entity = new Entity()
            {
                Money = (double)value,
            };
        }
        OnSetEntity();
    }

    void Reset()
    {
        text ??= this.GetComponent<TextMeshProUGUI>();
    }

    private string GetFormat(double Value, Format format, string textFormat = "")
    {
        return format switch
        {
            Format.Simple => Value.ToMoney(),
            Format.SimpleWithFormat => string.Format(textFormat, Value.ToMoney()),
            Format.Number => Value.ToString(),
            Format.Money => Value.ToString("N0", new CultureInfo("de-DE")),
            Format.MoneyWithFormat => string.Format(textFormat, Value.ToString("N0", new CultureInfo("de-DE"))),
            _ => "{0:00}"
        };
    }

}
