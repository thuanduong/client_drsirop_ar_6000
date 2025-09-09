using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public static class ScrollRectExtensions
{

    public static Vector2 GetSnapToPositionToBringChildIntoView(this ScrollRect instance, RectTransform child)
    {
        Canvas.ForceUpdateCanvases();
        Vector2 viewportLocalPosition = instance.viewport.localPosition;
        Vector2 childLocalPosition = child.localPosition;
        Vector2 result = new Vector2(
            0 - (viewportLocalPosition.x + childLocalPosition.x),
            0 - (viewportLocalPosition.y + childLocalPosition.y)
        );
        return result;
    }
    public static void ScrollToIndex(this ScrollRect scrollRect, Transform[] children, int index, bool isSnap = false, float duration = 0.25f)
    {
        var child = children[index];
        var pos = scrollRect.GetSnapToPositionToBringChildIntoView(child.transform as RectTransform);
        if (isSnap)
            scrollRect.content.localPosition = pos;
        else
        {
            scrollRect.content.DOKill();
            scrollRect.content.DOLocalMove(pos, duration);
        }

    }

}
