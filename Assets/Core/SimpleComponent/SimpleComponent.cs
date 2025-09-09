using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISimpleComponent
{
    void OnStart();
    void OnUpdate();
}

public class SimpleComponent : ISimpleComponent
{
    [SerializeField] protected bool isStartable;
    [SerializeField] protected bool isUpdatable;

    public bool IsStartable { get { return isStartable; } set { isStartable = value; } }
    public bool IsUpdatable { get { return isUpdatable; } set { isUpdatable = value; } }

    public virtual void OnStart() { }
    public virtual void OnUpdate() { }
}
