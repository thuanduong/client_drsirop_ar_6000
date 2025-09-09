using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class UIComponentListWithTemplates<Template, TemplateEntity> : UIComponent<UIComponentListWithTemplates<Template, TemplateEntity>.Entity> where TemplateEntity : new()
                                                                                                                   where Template : UIComponent<TemplateEntity>
{
    [Serializable]
    public partial class Entity
    {
        public List<int> typeOfEntities;
        public List<TemplateEntity> entities;
    }

    private void Awake()
    {
        var len = this.templates.Length;
        for (int i = 0; i < len; i++)
            templates[i].gameObject.SetActive(false);

    }

    private void OnDestroy()
    {
        __cts.SafeCancelAndDispose();
    }


    public Template[] templates;
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
            var len = this.entity.entities.Count;
            for (int i = 0; i < len; i++)
            {
                var item = this.entity.entities[i];
                var type = this.entity.typeOfEntities[i];

                var instance = CreateTemplate(type);
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

    private Template CreateTemplate(int type)
    {
        var template = templates[type];
        return Instantiate<Template>(template, template.transform.parent);
    }

    public void SetEntity(TemplateEntity[] entities)
    {
        base.SetEntity(new Entity()
        {
            entities = entities.ToList()
        });
    }

    public void SetEntity(List<TemplateEntity> entities)
    {
        base.SetEntity(new Entity()
        {
            entities = entities
        });
    }

    protected void addEntity(int type, TemplateEntity n_entity)
    {
        if (entity != null)
        {
            entity.typeOfEntities.Add(type);
            entity.entities.Add(n_entity);

            var instance = CreateTemplate(type);
            instance.gameObject.SetActive(true);
            instance.SetEntity(n_entity);
            instanceList.Add(instance);
        }
    }

    public virtual void AddEntity(int type, TemplateEntity n_entity)
    {
        addEntity(type, n_entity);
    }

    private async UniTask createAsync()
    {
        int counter = 0;
        try
        {
            var len = this.entity.entities.Count;
            for (int i = 0; i < len; i++)
            {
                var item = this.entity.entities[i];
                var type = this.entity.typeOfEntities[i];

                var instance = CreateTemplate(type);
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

