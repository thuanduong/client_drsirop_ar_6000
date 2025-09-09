using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class UIComponentPoolList<Template, TemplateEntity> : UIComponent<UIComponentPoolList<Template, TemplateEntity>.Entity>, IPoolList where TemplateEntity : new()
                                                                                                                   where Template : UIComponent<TemplateEntity>
{
    [Serializable]
    public partial class Entity
    {
        public TemplateEntity[] entities;
        public int Start;
    }

    private void Awake()
    {
        template.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        __cts.SafeCancelAndDispose();
    }


    public Template template;
    public List<Template> instanceList = new List<Template>();
    public CanvasGroup canvasGroup;
    [Range (1, 200)]public int Length = 10;

    private CancellationTokenSource __cts;


    public int NumChild { get; private set; }
    public int HeadIndex { get; private set; }
    public int TailIndex { get; private set; }

    private List<GameObject> _goList;
    public List<GameObject> InstanceList { get { return _goList; } }

    public System.Action OnFinishUpdate { get; set; } = ActionUtility.EmptyAction.Instance;

    protected override void OnSetEntity()
    {
        foreach (var instance in instanceList)
        {
            GameObject.Destroy(instance.gameObject);
        }
        instanceList.Clear();
        if (_goList == default) _goList = new List<GameObject>();
        _goList.Clear();
        __cts.SafeCancelAndDispose();
        __cts = new CancellationTokenSource();
        createAsync().AttachExternalCancellation(__cts.Token).Forget();
    }

    public void SetStart(int Start)
    {
        entity.Start = Start;
    }

    private Template CreateTemplate()
    {
        return Instantiate<Template>(template, template.transform.parent);
    }

    public void SetEntity(TemplateEntity[] entities, int start = 0)
    {
        base.SetEntity(new Entity()
        {
            entities = entities,
            Start = start,
        });
    }

    private async UniTask createAsync()
    {
        int counter = 0;
        try
        {
            await ActiveCanvas(false, __cts.Token);
            int total = entity.entities.Length;
            var s = Mathf.Clamp(entity.Start, 0, total);
            var e = Mathf.Clamp(s  + Length, 0, total);
            
            HeadIndex = s;
            TailIndex = e;
            NumChild = entity.entities.Length;
            _goList.Clear();
            for (int i = s; i < e; i++)
            {
                var item = entity.entities[i];
                var instance = CreateTemplate();
                instance.gameObject.SetActive(true);
                instance.SetEntity(item);
                instanceList.Add(instance);
                _goList.Add(instance.gameObject);
                counter++;
                if (counter == 5)
                {
                    counter = 0;
                    await UniTask.DelayFrame(1, cancellationToken: __cts.Token);
                }
            }

            OnFinishUpdate();
            await ActiveCanvas(true, __cts.Token);
        }
        catch { }
    }

    private async UniTask ActiveCanvas(bool active, CancellationToken token = default)
    {
        if (canvasGroup == default) return;
        if (active)
        {
            var time = Mathf.Lerp(0, 0.2f, canvasGroup.alpha);
            var sq = canvasGroup.DOFade(1, time).WithCancellation(token);
            await sq;
        }
        else
        {
            var time = Mathf.Lerp(0.2f, 0, canvasGroup.alpha);
            var sq = canvasGroup.DOFade(0, time).WithCancellation(token);
            await sq;
        }
    }

    public void UpdateIndex(int start)
    {
        __cts.SafeCancelAndDispose();
        __cts = new CancellationTokenSource();
        UpdateIndexAsync(start, __cts.Token).AttachExternalCancellation(__cts.Token).Forget();

    }

    public async UniTask UpdateIndexAsync(int start, CancellationToken token)
    {
        if (entity.Start == start) return;
        entity.Start = start;
        int total = entity.entities.Length;
        var s = Mathf.Clamp(entity.Start, 0, total);
        var e = Mathf.Clamp(s + Length, 0, total);
        var pr = Mathf.Clamp(e - Length, 0, total);
        s = pr < s ? pr : s;
        HeadIndex = s;
        TailIndex = e;

        try
        {
            await ActiveCanvas(false, token);
            int counter = 0;
            int mm = 0;
            for (int i = s; i < e; i++)
            {
                var item = entity.entities[i];
                if (mm < instanceList.Count)
                {
                    Template instance = CreateTemplate();
                    instance.gameObject.SetActive(true);
                    instance.SetEntity(item);
                    instanceList.Add(instance);
                }
                else
                {
                    Template instance = instanceList[mm];
                    instance.SetEntity(item);
                }
                mm++;
                counter++;
                if (counter == 5)
                {
                    counter = 0;
                    await UniTask.DelayFrame(1, cancellationToken: token);
                }
            }
            OnFinishUpdate();
            await ActiveCanvas(true, token);
        }
        catch { } 
    }

    private List<GameObject> getListInstance()
    {
        return _goList;
    }

    public bool MoveNext()
    {
        
        int total = entity.entities.Length;
        if (total < 2) return false;
        var s = HeadIndex + 1;
        var e = TailIndex + 1;
        if (e >= total) return false;

        var item = entity.entities[e];
        Template instance = instanceList[0];
        instance.SetEntity(item);
        instanceList.RemoveAt(0);
        instanceList.Add(instance);

        _goList.Remove(instance.gameObject);
        _goList.Add(instance.gameObject);

        entity.Start = s;
        HeadIndex = s;
        TailIndex = e;
        return true;
    }

    public bool MovePrev()
    {
        int total = entity.entities.Length;
        if (total < 2) return false;
        var s = HeadIndex - 1;
        var e = TailIndex - 1;
        if (s < 0) return false;
        var item = entity.entities[s];
        var last = Length < total ? (Length - 1) : (total - 1);
        Template instance = instanceList[last];
        instance.SetEntity(item);
        instanceList.Remove(instance);
        instanceList.Insert(0, instance);

        _goList.Remove(instance.gameObject);
        _goList.Insert(0, instance.gameObject);

        entity.Start = s;
        HeadIndex = s;
        TailIndex = e;
        return true;
    }

}
