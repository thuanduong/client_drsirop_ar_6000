using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISliderScroll : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    #region Variables

    [Header("Configuration")]

    /// <summary>
    /// Minimum delta drag required to consider a page change (normalized value between 0 and 1).
    /// </summary>
    [Tooltip("Minimum delta drag required to consider a page change (normalized value between 0 and 1)")]
    [SerializeField] private float _minDeltaDrag = 0.1f;

    /// <summary>
    /// Duration (in seconds) for the page snapping animation.
    /// </summary>
    [Tooltip("Duration (in seconds) for the page snapping animation")]
    [SerializeField] private float _snapDuration = 0.3f;

    [SerializeField] private float _scaler = 0.8f;

    [Header("Events")]

    /// <summary>
    /// Event triggered when a page change starts. 
    /// The event arguments are the index of the current page and the index of the target page.
    /// </summary>
    [Tooltip("Event triggered when a page change starts: index current page, index of target page")]
    public UnityEvent<int, int> OnPageChangeStarted;

    /// <summary>
    /// Event triggered when a page change ends. 
    /// The event arguments are the index of the current page and the index of the new active page.
    /// </summary>
    [Tooltip("Event triggered when a page change ends: index of the current page, index of the new active page")]
    public UnityEvent<int, int> OnPageChangeEnded;

    /// <summary>
    /// Gets the rectangle of the ScrollRect component used for scrolling.
    /// </summary>
    public Rect Rect
    {
        get
        {
#if UNITY_EDITOR
            if (_scrollRect == null)
            {
                _scrollRect = FindScrollRect();
            }
#endif
            return ((RectTransform)_scrollRect.transform).rect;
        }
    }

    /// <summary>
    /// Gets the RectTransform of the content being scrolled within the ScrollRect.
    /// </summary>
    public RectTransform Content
    {
        get
        {
#if UNITY_EDITOR
            if (_scrollRect == null)
            {
                _scrollRect = FindScrollRect();
            }
#endif
            return _scrollRect.content;
        }
    }

    private ScrollRect _scrollRect;

    private int _currentPage; // Index of the currently active page.
    private int _targetPage; // Index of the target page during a page change animation.

    private float _startNormalizedPosition; // Normalized position of the scroll bar when drag begins.
    private float _targetNormalizedPosition; // Normalized position of the scroll bar for the target page.
    private float _moveSpeed; // Speed of the scroll bar animation (normalized units per second).

    public int PageCount { get; set; } = 1;
    public List<GameObject> Pages { get; set; }
    public List<GameObject> __Pages;
    private float[] distances;
    private float distance;
    #endregion


    private void Awake()
    {
        _scrollRect = FindScrollRect();
    }

    private void Update()
    {
        if (_moveSpeed == 0 || PageCount == 0 || PageCount == 1) { return; }

        var position = _scrollRect.horizontalNormalizedPosition;
        position += _moveSpeed * Time.deltaTime;

        var min = _moveSpeed > 0 ? position : _targetNormalizedPosition;
        var max = _moveSpeed > 0 ? _targetNormalizedPosition : position;
        position = Mathf.Clamp(position, min, max);

        _scrollRect.horizontalNormalizedPosition = position;

        for (int i = 0; i < distances.Length; i++)
        {
            if (position < distances[i] + (distance / 2) && position > distances[i] - (distance / 2))
            {
                Pages[i].transform.localScale = Vector2.Lerp(Pages[i].transform.localScale, new Vector2(1, 1), 0.1f);
                for(int a = 0; a < distances.Length; a++)
                {
                    if (a != i)
                    {
                        Pages[a].transform.localScale = Vector2.Lerp(Pages[a].transform.localScale, new Vector2(_scaler, _scaler), 0.1f);
                    }
                }
            }
        }

        if (Mathf.Abs(_targetNormalizedPosition - position) < Mathf.Epsilon)
        {
            _moveSpeed = 0;

            OnPageChangeEnded?.Invoke(_currentPage, _targetPage);

            _currentPage = _targetPage;
        }
    }

    public void SetPages(List<GameObject> _pages)
    {
        Pages = _pages;
        PageCount = Pages.Count;
        if (PageCount > 1)
        {
            distances = new float[PageCount];
            distance = 1.0f / (PageCount - 1);
            for (int i = 0; i < PageCount; i++)
            {
                distances[i] = distance * i;
            }
        }
    }

    public void SetPage(int index)
    {
        _scrollRect.horizontalNormalizedPosition = GetTargetPagePosition(index);

        _targetPage = index;
        _currentPage = index;
        OnPageChangeEnded?.Invoke(0, _currentPage);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Store the starting normalized position of the scroll bar.
        _startNormalizedPosition = _scrollRect.horizontalNormalizedPosition;

        // Check if the target page is different from the current page.
        if (_targetPage != _currentPage)
        {
            // If they are different, it means we were potentially in the middle of an animation
            // for a previous page change that got interrupted by this drag. 
            // Therefore, signal the end of the previous page change animation (if any)
            // by invoking the OnPageChangeEnded event.
            // The event arguments are the index of the previous page (_currentPage) 
            // and the index of the target page (_targetPage).
            OnPageChangeEnded?.Invoke(_currentPage, _targetPage);

            // Update the _currentPage variable to reflect the target page,
            // as this is now the intended page after the drag begins.
            _currentPage = _targetPage;
        }

        // Reset the move speed to 0 to stop any ongoing scroll animations.
        // This is necessary because a drag interaction might interrupt an ongoing page change animation.
        _moveSpeed = 0;
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        //var pageWidth = 1f / GetPageCount();
        var pageWidth = distance;

        var pagePosition = _currentPage * pageWidth;

        var currentPosition = _scrollRect.horizontalNormalizedPosition;

        var minPageDrag = pageWidth * _minDeltaDrag;

        var isForwardDrag = _scrollRect.horizontalNormalizedPosition > _startNormalizedPosition;

        var switchPageBreakpoint = pagePosition + (isForwardDrag ? minPageDrag : -minPageDrag);

        var page = _currentPage;
        if (isForwardDrag && currentPosition > switchPageBreakpoint)
        {
            if (page < PageCount - 1)
                page++;
        }
        else if (!isForwardDrag && currentPosition < switchPageBreakpoint)
        {
            if (page > 0)
                page--;
        }

        // Call the ScrollToPage function to initiate the page change animation for the determined page.
        ScrollToPage(page);
    }

    private void ScrollToPage(int page)
    {
        // Calculate the target normalized position for the scroll rect based on the target page index.
        _targetNormalizedPosition = GetTargetPagePosition(page);

        // Calculate the speed required to reach the target position within the snap duration.
        _moveSpeed = (_targetNormalizedPosition - _scrollRect.horizontalNormalizedPosition) / _snapDuration;

        // Update the target page variable to reflect the new target page.
        _targetPage = page;

        // If the target page is different from the current page, 
        // invoke the OnPageChangeStarted event to signal the beginning of the page change animation.
        if (_targetPage != _currentPage)
        {
            OnPageChangeStarted?.Invoke(_currentPage, _targetPage);
        }
    }

    private int GetPageCount()
    {
        return PageCount <= 0 ? 1 : PageCount;
        //var contentWidth = _scrollRect.content.rect.width;
        //var rectWidth = ((RectTransform)_scrollRect.transform).rect.size.x;
        //return Mathf.RoundToInt(contentWidth / rectWidth) - 1;
    }

    private float GetTargetPagePosition(int page)
    {
        if (Pages != default && PageCount > 1)
        {
            return distances[page];
        }
        return page * (1f / GetPageCount());
    }

    private ScrollRect FindScrollRect()
    {
        var scrollRect = GetComponentInChildren<ScrollRect>();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        if (scrollRect == null)
        {
            Debug.LogError("Missing ScrollRect in Children");
        }
#endif
        return scrollRect;
    }

    public void Refresh()
    {
        var position = _scrollRect.horizontalNormalizedPosition;
        for (int i = 0; i < distances.Length; i++)
        {
            if (position < distances[i] + (distance / 2) && position > distances[i] - (distance / 2))
            {
                Pages[i].transform.localScale = new Vector2(1, 1);
                for (int a = 0; a < distances.Length; a++)
                {
                    if (a != i)
                    {
                        Pages[a].transform.localScale = new Vector2(_scaler, _scaler);
                    }
                }
            }
        }
    }
}
