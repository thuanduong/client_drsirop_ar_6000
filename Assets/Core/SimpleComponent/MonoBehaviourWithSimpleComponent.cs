using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoBehaviourWithSimpleComponent : MonoBehaviour
{
    [SerializeField] List<SimpleComponent> simpleComponents;

    public List<SimpleComponent> SimpleComponents { get { return simpleComponents; } set { simpleComponents = value; } }

    protected virtual void Start()
    {
        OnStartSimpleMono();
    }

    protected virtual void Update()
    {
        OnUpdateSimpleMono();
    }

    protected void OnStartSimpleMono()
    {
        var mm = simpleComponents.ToArray();
        foreach (var item in mm)
        {
            if (item.IsStartable)
                item.OnStart();
        }
    }

    protected void OnUpdateSimpleMono()
    {
        var mm = simpleComponents.ToArray();
        foreach (var item in mm)
        {
            if (item.IsStartable)
                item.OnUpdate();
        }
    }
}
