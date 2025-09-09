using UnityEngine;
using TMPro;

public class UIGameModeItem : UIComponent<UIGameModeItem.Entity>
{
    public class Entity {
        public int Id;
        public string Name;
        public UIImageComponent.Entity Img;
        public System.Action<int> Button;
    }

    public TextMeshProUGUI Name;
    public UIButtonComponent Button;
    public UIImageComponent Img;

    private void Awake()
    {
        Button.SetEntity(OnButtonClicked);
    }

    protected override void OnSetEntity()
    {
        Name.SetText(entity.Name);
        Img.SetEntity(entity.Img);
    }

    private void OnButtonClicked()
    {
        if (entity != null)
            entity.Button(entity.Id);
    }
}
