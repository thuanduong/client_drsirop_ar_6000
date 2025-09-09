using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class UIComponentList<Template, TemplateEntity> : UIComponent<UIComponentList<Template, TemplateEntity>.Entity> where TemplateEntity : new()
                                                                                                                   where Template : UIComponent<TemplateEntity>
{
    [Serializable]
    public partial class Entity
    {
        public TemplateEntity[] entities;
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
    public bool AsyncGenerate = false;
    public List<Template> instanceList = new List<Template>();

    private CancellationTokenSource __cts;

    protected override void OnSetEntity()
    {
        foreach (var instance in instanceList)
        {
            GameObject.Destroy(instance.gameObject);
        }
        instanceList.Clear();
        if (!AsyncGenerate)
        {
            foreach (var item in this.entity.entities)
            {
                var instance = CreateTemplate();
                instance.gameObject.SetActive(true);
                instance.SetEntity(item);
                instanceList.Add(instance);
            }
        }
        else
        {
            __cts.SafeCancelAndDispose();
            __cts = new CancellationTokenSource();
            createAsync().AttachExternalCancellation(__cts.Token).Forget();
        }
    }

    private Template CreateTemplate()
    {
        return Instantiate<Template> (template, template.transform.parent);
    }

    public void SetEntity(TemplateEntity[] entities)
    {
        base.SetEntity(new Entity()
        {
            entities = entities
        });
    }
    
    private async UniTask createAsync()
    {
        int counter = 0;
        try
        {
            foreach (var item in this.entity.entities)
            {
                var instance = CreateTemplate();
                instance.gameObject.SetActive(true);
                instance.SetEntity(item);
                instanceList.Add(instance);
                counter++;
                if (counter == 5)
                {
                    counter = 0;
                    await UniTask.DelayFrame(1, cancellationToken: __cts.Token);
                }
            }
        }
        catch { }
    }
}
