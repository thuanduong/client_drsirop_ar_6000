using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ExcelScrollRectComponent : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Content References")]
    public RectTransform fixedContent;
    public RectTransform scrollableContent;
    [SerializeField] Vector2 scrollableAnchor;
    [Header("Scroll Settings")]
    [Range(0.00001f, 2f)]
    public float scrollSpeed = 1f;
    [Range(0.01f, 0.5f)]
    public float decelerationRate = 0.135f;

    private Vector2 _lastDragPosition;
    private Vector2 _velocity;
    [SerializeField] private Vector2 _scrollableContentMinXMaxX;
    [SerializeField] RectTransform Holder;


    private Vector2 _verticalScrollMinYMaxY;

    [SerializeField] RectTransform _viewportRect;

    

    private float _viewportHeight;

    void Awake()
    {

        if (fixedContent == null || scrollableContent == null)
        {
            Debug.LogError("Fixed Content and Scrollable Content must be assigned in the Inspector!");
            enabled = false;
            return;
        }

        fixedContent.pivot = new Vector2(0, 1);
        scrollableContent.pivot = new Vector2(0, 1);

        fixedContent.anchorMin = new Vector2(0, 1);
        fixedContent.anchorMax = new Vector2(0, 1);

        scrollableContent.anchorMin = new Vector2(0, 1);
        scrollableContent.anchorMax = new Vector2(0, 1);

        CalculateScrollLimits();
    }

    public async UniTask Dirty(CancellationToken cancellationToken)
    {
        fixedContent.gameObject.SetActive(false);
        scrollableContent.gameObject.SetActive(false);
        if (Holder != default)
            Holder.gameObject.SetActive(false);
        await UniTask.Delay(100, cancellationToken: cancellationToken);
        fixedContent.gameObject.SetActive(true);
        await UniTask.Delay(100, cancellationToken: cancellationToken);
        scrollableContent.gameObject.SetActive(true);
        await UniTask.Delay(100, cancellationToken: cancellationToken);
        if (Holder != default)
            Holder.gameObject.SetActive(true);
        ResetScrollPosition();
    }

    void OnEnable()
    {
        ResetScrollPosition();
    }

    void Update()
    {
        if (_velocity != Vector2.zero && !IsDragging())
        {

            _velocity = Vector2.Lerp(_velocity, Vector2.zero, decelerationRate * Time.deltaTime * 10f);

            Vector2 currentFixedPos = fixedContent.anchoredPosition;
            Vector2 currentScrollablePos = scrollableContent.anchoredPosition;

            //Vector2 newFixedPosY = currentFixedPos + new Vector2(0, _velocity.y * scrollSpeed * Time.deltaTime);
            //newFixedPosY.y = Mathf.Clamp(newFixedPosY.y, _verticalScrollMinYMaxY.x, _verticalScrollMinYMaxY.y);
            //fixedContent.anchoredPosition = new Vector2(currentFixedPos.x, newFixedPosY.y);

            Vector2 newScrollablePos = currentScrollablePos + _velocity * scrollSpeed * Time.deltaTime;
            newScrollablePos.y = Mathf.Clamp(newScrollablePos.y, _verticalScrollMinYMaxY.x, _verticalScrollMinYMaxY.y);
            newScrollablePos.x = Mathf.Clamp(newScrollablePos.x, _scrollableContentMinXMaxX.x, _scrollableContentMinXMaxX.y);
            scrollableContent.anchoredPosition = newScrollablePos;

            if (_velocity.magnitude < 0.5f)
            {
                _velocity = Vector2.zero;
            }

        }
    }
    private bool IsDragging()
    {
        return EventSystem.current.IsPointerOverGameObject() &&
               EventSystem.current.currentSelectedGameObject == gameObject;
    }

    void CalculateScrollLimits()
    {
        if (_viewportRect == null) return;
        float viewportHeight = _viewportRect.rect.height;
        float fixedContentHeight = fixedContent.rect.height;
        float scrollableContentHeight = scrollableContent.rect.height;

        float contentMaxHeight = Mathf.Max(fixedContentHeight, scrollableContentHeight);
        _viewportHeight = viewportHeight;
        float verticalMinY = 0;

        float verticalMaxY = 0;
        if (contentMaxHeight > viewportHeight)
        {
            verticalMaxY = (contentMaxHeight - viewportHeight); 
        }
        Debug.Log($"RR {verticalMinY} - {verticalMaxY}");

        _verticalScrollMinYMaxY = new Vector2(verticalMinY, verticalMaxY);

        float viewportWidth = _viewportRect.rect.width;
        float fixedContentWidth = fixedContent.rect.width;
        float scrollableContentWidth = scrollableContent.rect.width;

        float initialScrollableXOffset = 0;// fixedContent.rect.width;

        float horizontalMaxX = initialScrollableXOffset;

        float horizontalMinX = initialScrollableXOffset;

        float totalRenderedWidth = fixedContentWidth + scrollableContentWidth;

        if (totalRenderedWidth > viewportWidth)
        {
            horizontalMinX = initialScrollableXOffset - ((scrollableContentWidth - viewportWidth) - (viewportWidth - fixedContentWidth));
        }
        _scrollableContentMinXMaxX = new Vector2(horizontalMinX, horizontalMaxX);
    }

    void OnRectTransformDimensionsChange()
    {
        CalculateScrollLimits();
        _velocity = Vector2.zero;
        //fixedContent.anchoredPosition = new Vector2(fixedContent.anchoredPosition.x, _verticalScrollMinYMaxY.x);
        scrollableContent.anchoredPosition = new Vector2(_scrollableContentMinXMaxX.y, _verticalScrollMinYMaxY.x);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _velocity = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _viewportRect, eventData.position, eventData.pressEventCamera, out _lastDragPosition);
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _viewportRect, eventData.position, eventData.pressEventCamera, out Vector2 currentDragPosition);

        Vector2 delta = currentDragPosition - _lastDragPosition;

        ApplyScrollDelta(delta);

        _lastDragPosition = currentDragPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _velocity = eventData.delta / Time.deltaTime;
    }

    private void ApplyScrollDelta(Vector2 delta)
    {
        if (Mathf.Abs(delta.y) >= Mathf.Abs(delta.x))
        {
            Vector2 newFixedPos = fixedContent.anchoredPosition + new Vector2(0, delta.y * scrollSpeed);
            Vector2 newScrollablePos = scrollableContent.anchoredPosition + new Vector2(0, delta.y * scrollSpeed);

            newFixedPos.y = Mathf.Clamp(newFixedPos.y, _verticalScrollMinYMaxY.x, _verticalScrollMinYMaxY.y);
            newScrollablePos.y = Mathf.Clamp(newScrollablePos.y, _verticalScrollMinYMaxY.x, _verticalScrollMinYMaxY.y);

            //fixedContent.anchoredPosition = newFixedPos;
            scrollableContent.anchoredPosition = newScrollablePos;
        }
        else if (Mathf.Abs(delta.x) > 0)
        {
            Vector2 newScrollablePos = scrollableContent.anchoredPosition + new Vector2(delta.x * scrollSpeed, 0);

            newScrollablePos.x = Mathf.Clamp(newScrollablePos.x, _scrollableContentMinXMaxX.x, _scrollableContentMinXMaxX.y);

            scrollableContent.anchoredPosition = newScrollablePos;
        }
    }

    public void ResetScrollPosition()
    {
        _velocity = Vector2.zero;
        //fixedContent.anchoredPosition = new Vector2(fixedContent.anchoredPosition.x, _verticalScrollMinYMaxY.x);
        scrollableContent.anchoredPosition = new Vector2(_scrollableContentMinXMaxX.y, _verticalScrollMinYMaxY.x);
        CalculateScrollLimits();
    }

}
