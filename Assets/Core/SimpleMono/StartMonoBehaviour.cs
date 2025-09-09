using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartMonoBehaviour : IMonoBehaviour, IStartable, IDisposable
{
    public StartMonoBehaviour()
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

    public virtual void OnStart() { }

    public virtual void OnDestroy() { }
}
