using UnityEngine;
using UnityEngine.UI;

public class RawImageComponent : UIComponent<RawImageComponent.Entity>
{
    public class Entity
    {
        public Texture img;
    }

    public RawImage img;
    [Header("Settings")]
    [SerializeField] bool isDisbleWhenNoImg = true;

    private bool currentActive;

    private void Start()
    {
        currentActive = this.img.enabled;
    }

    protected override void OnSetEntity()
    {
        if (this.img != default)
        {
            this.img.texture = this.entity.img;
            
            if (isDisbleWhenNoImg && this.img.texture == default)
            {
                this.img.enabled = false;
            }
            else 
                this.img.enabled = currentActive;
        }
    }

    public void SetImage(Texture img)
    {
        if (this.entity != default) this.entity.img = img;
        this.img.texture = img;

        if (isDisbleWhenNoImg && img == default)
        {
            this.img.enabled = false;
        }
        else
            this.img.enabled = currentActive;
    }
}
