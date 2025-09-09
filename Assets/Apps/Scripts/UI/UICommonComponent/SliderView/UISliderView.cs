using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using Cysharp.Threading.Tasks;

[RequireComponent(typeof(UISliderScroll))]
public class UISliderView : UIComponent<UISliderView.Entity>
{
    public class Entity {
        public UISliderItem.Entity[] entities;
        public int StartPageIndex;
    }
    
    #region Variables

    [Header("References")]

    [SerializeField] private UISliderScroll _scroller;
    [SerializeField] private UISliderDotsIndicator _dotsIndicator;
    [SerializeField] private UISliderItem template;
    [SerializeField] private int _startPageIndex;

    [Header("Events")]
    public UnityEvent<UISliderItem> OnPageChanged;

    public Rect Rect { get { return ((RectTransform)transform).rect; } }

    
    private List<UISliderItem> instanceList = new List<UISliderItem>();

    #endregion

    private void Awake()
    {
        if (template != default)
        {
            template.gameObject.SetActive(false);
        }
    }

    private IEnumerator Start()
    {
        _scroller.OnPageChangeStarted.AddListener(PageScroller_PageChangeStarted);
        _scroller.OnPageChangeEnded.AddListener(PageScroller_PageChangeEnded);

        yield return new WaitForEndOfFrame();
    }

    protected override void OnSetEntity()
    {
        Clear();

        foreach (var item in this.entity.entities)
        {
            var instance = CreateTemplate();
            instance.gameObject.SetActive(true);
            instance.SetEntity(item);
            instanceList.Add(instance);
            _dotsIndicator.Add();
        }

        var pages = instanceList.Select(x => x.gameObject).ToList();
        _scroller.SetPages(pages);

        DelayRefesh().Forget();
    }

    private async UniTask DelayRefesh()
    {
        await UniTask.DelayFrame(1);

        if (entity.StartPageIndex != 0)
            _scroller.SetPage(entity.StartPageIndex);
        _scroller.Refresh();
    }

    private UISliderItem CreateTemplate()
    {
        return Instantiate<UISliderItem>(template, template.transform.parent);
    }

    public void Clear()
    {
        for (int i = 0; i < instanceList.Count; i++)
        {
            if (instanceList[i] == null) { continue; }
#if UNITY_EDITOR
            DestroyImmediate(instanceList[i].gameObject);
#else
            Destroy(instanceList[i].gameObject);
#endif
        }
        instanceList.Clear();

        if (_dotsIndicator != null)
        {
            _dotsIndicator.Clear();
        }
    }

    private void PageScroller_PageChangeStarted(int fromIndex, int toIndex)
    {
        instanceList[fromIndex].ChangingToInactiveState();
        instanceList[toIndex].ChangingToActiveState();
    }

    private void PageScroller_PageChangeEnded(int fromIndex, int toIndex)
    {
        instanceList[fromIndex].ChangeActiveState(false);
        instanceList[toIndex].ChangeActiveState(true);

        if (_dotsIndicator != null)
        {
            _dotsIndicator.ChangeActiveDot(fromIndex, toIndex);
        }

        OnPageChanged?.Invoke(instanceList[toIndex]);
    }
}
