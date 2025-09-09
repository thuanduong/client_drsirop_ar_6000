using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IMonoBehaviour
{ }


public interface IStartable
{
    void Start();
}

public interface IUpdatable
{
    void Update();
}

public interface IDestroyable
{
    void Destroy();
}