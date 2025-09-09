using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class UIComponentCustomList<Template, TemplateEntity> : UIComponent<UIComponentCustomList<Template, TemplateEntity>.Entity> where TemplateEntity : new()
                                                                                                                   where Template : UIComponent<TemplateEntity>
{
    [Serializable]
    public partial class Entity
    {
        public TemplateEntity[] entities;
        public int[] customTypes; 
    }

    private void Awake()
    {
        DisableTemplates();
    }

    private void OnDestroy()
    {
        __cts.SafeCancelAndDispose();
    }

    private void DisableTemplates()
    {
        foreach (var template in templates)
            template.gameObject.SetActive(false);
    }

    public List<Template> templates;
    public Transform container;
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
            var len = this.entity.entities.Length;
            var lenT = this.entity.customTypes.Length;
            for (int i = 0; i < len; i++)
            {
                var item = this.entity.entities[i];
                int type = 0;
                if (i < lenT) type = this.entity.customTypes[i];
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

    private Template CreateTemplate(int index)
    {
        var _index = index % templates.Count;
        return Instantiate<Template>(templates[_index], container);
    }

    public void SetEntity(TemplateEntity[] entities, int[] customTypes)
    {
        base.SetEntity(new Entity()
        {
            entities = entities,
            customTypes = customTypes,
        });
    }

    private async UniTask createAsync()
    {
        int counter = 0;
        try
        {
            var len = this.entity.entities.Length;
            var lenT = this.entity.customTypes.Length;
            for (int i = 0; i < len; i++)
            {
                var item = this.entity.entities[i];
                int type = 0;
                if (i < lenT) type = this.entity.customTypes[i];
                var instance = CreateTemplate(type);
                instance.gameObject.SetActive(true);
                instance.SetEntity(item);
                instanceList.Add(instance);
                counter++;
                if (counter == 5)
                {
                    counter = 0;
                    await UniTask.DelayFrame(1);
                }
            }
        }
        catch { }
    }
}
