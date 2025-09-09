using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SmartScrollRect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    ScrollRect currentScroll;
    public void OnPointerDown(PointerEventData eventData)
    {
        var paScroll = eventData.pointerCurrentRaycast.gameObject.GetComponentInParent<ScrollRect>();
        if (paScroll != default)
        {
            if (paScroll.gameObject == this.gameObject) return;
            currentScroll = paScroll;
            paScroll.enabled = false;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (currentScroll != default)
        {
            currentScroll.enabled = true;
            currentScroll = default;
        }
    }
}
