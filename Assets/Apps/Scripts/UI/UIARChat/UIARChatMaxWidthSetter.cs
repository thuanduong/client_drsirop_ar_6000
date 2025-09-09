using UnityEngine;
using TMPro;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(TMP_Text))]
public class UIARChatMaxWidthSetter : MonoBehaviour
{
    public RectTransform maxWidthParent;
    public float minWidth = 100f;
    public float paddingLeft = 0f;
    public float paddingRight = 0f;

    private RectTransform rectTransform;
    private TMP_Text textComponent;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        textComponent = GetComponent<TMP_Text>();
    }

    public void SetText(string newText)
    {
        textComponent.text = newText;

        UpdateSize();
    }

    private void UpdateSize()
    {
        if (textComponent == null || maxWidthParent == null)
            return;
        
        textComponent.textWrappingMode = TextWrappingModes.Normal;
        textComponent.ForceMeshUpdate();
        
        float paddingLeftRight = paddingLeft + paddingRight;


        float naturalWidth = textComponent.preferredWidth + paddingLeftRight;

        float maxWidth = maxWidthParent.rect.width - paddingLeftRight;

        bool isTextEmpty = string.IsNullOrWhiteSpace(textComponent.text);
        
        if (isTextEmpty)
        {
            rectTransform.sizeDelta = new Vector2(minWidth, rectTransform.sizeDelta.y);
            textComponent.textWrappingMode = TextWrappingModes.NoWrap;
        }
        else if (naturalWidth > maxWidth)
        {
            textComponent.textWrappingMode = TextWrappingModes.Normal;

            rectTransform.sizeDelta = new Vector2(maxWidth, rectTransform.sizeDelta.y);

        }
        else
        {
            textComponent.textWrappingMode = TextWrappingModes.NoWrap;

            rectTransform.sizeDelta = new Vector2(naturalWidth, rectTransform.sizeDelta.y);
        }

        textComponent.ForceMeshUpdate();
    }
}
