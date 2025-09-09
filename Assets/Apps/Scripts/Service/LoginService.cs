using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using Core.Model;

public class LoginService : IDisposable
{
    private readonly IDIContainer container;
    private static LoginService instance;
    public static LoginService Instance => instance;
       


    private LoginService(IDIContainer container)
    {
        this.container = container;
    }

    public void Dispose()
    {
        instance = default;
    }

    public static LoginService Instantiate(IDIContainer container)
    {
        if (instance == default)
        {
            instance = new LoginService(container);
        }
        return instance;
    }

}
