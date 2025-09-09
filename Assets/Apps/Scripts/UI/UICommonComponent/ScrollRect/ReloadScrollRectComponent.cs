using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class ReloadScrollRectComponent : MonoBehaviour
{
    public ScrollRect ScrollRect;
    public ReuseableLayoutGroup ResuseContent;

    private void Start()
    {
        Initialize();
    }

    void OnDestroy()
    {
		if (ScrollRect != default) 
		    ScrollRect.onValueChanged.RemoveListener(OnScroll);
        if (ResuseContent != default)
        {
            ResuseContent.MoveType -= OnUpdateMoveType;
            ResuseContent.MoveTo -= MoveTo;
        }
    }

    void Initialize()
    {
        if (ScrollRect != default)
		    ScrollRect.onValueChanged.AddListener(OnScroll);
        if (ResuseContent != default)
        {
            ResuseContent.MoveType += OnUpdateMoveType;
            ResuseContent.MoveTo += MoveTo;
        }
    }

    void OnScroll(Vector2 value)
    {
        if (ResuseContent != default)
            ResuseContent.OnScroll(value);
    }

    public void Reload()
    {
        ResuseContent.ResizeAndPositionContent();
    }
    public void SetPool(IPoolList pool)
    {
        ResuseContent.Initialize(pool);
    }
    private void OnUpdateMoveType(ScrollRect.MovementType type)
    {
        ScrollRect.movementType = type;
    }
    public void MoveTo(Vector2 offset)
    {
        ScrollRect.verticalNormalizedPosition = offset.y;
    }
}
