using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SimpleMonoBehaviour : IMonoBehaviour, IStartable, IUpdatable, IDisposable
{
    public SimpleMonoBehaviour()
    {
        SimpleMonoBehaviourManager.Register(this);
    }

    public void Dispose()
    {
        SimpleMonoBehaviourManager.Unregister(this);
        OnDestroy();
    }

    // Start is called before the first frame update
    public void Start()
    {
        OnStart();
    }

    // Update is called once per frame
    public void Update()
    {
        OnUpdate();
    }

    public virtual void OnStart() { }

    public virtual void OnUpdate() { }

    public virtual void OnDestroy() { }
}
