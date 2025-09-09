using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMonoBehaviourManager : MonoBehaviour
{
    private static SimpleMonoBehaviourManager _instance;
    public static SimpleMonoBehaviourManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<SimpleMonoBehaviourManager>();
            }
            return _instance;
        }
    }

    private Queue<IMonoBehaviour> _startable = new Queue<IMonoBehaviour>();
    private List<IMonoBehaviour> _updatables = new List<IMonoBehaviour>();

    public void register(IMonoBehaviour mono)
    {   
        if (mono is IStartable && !_startable.Contains(mono))
        {
            _startable.Enqueue(mono);
        }

        if (mono is IUpdatable && !_updatables.Contains(mono))
        {
            _updatables.Add(mono);
        }

    }

    public void unregister(IMonoBehaviour updatable)
    {
        if (_updatables.Contains(updatable))
        {
            _updatables.Remove(updatable);
        }
    }

    private void Update()
    {
        if (_startable.Count > 0)
        {
            while(_startable.Count > 0)
            {
                (_startable.Dequeue() as IStartable).Start();
            }
        }
        
        foreach (var updatable in _updatables)
        {
            (updatable as IUpdatable).Update();
        }
    }

    public static void Register(IMonoBehaviour updatable)
    {
        if (Instance != null)
            Instance.register(updatable);
    }

    public static void Unregister(IMonoBehaviour updatable)
    {
        if (Instance != null)
            Instance.unregister(updatable);
    }
}
