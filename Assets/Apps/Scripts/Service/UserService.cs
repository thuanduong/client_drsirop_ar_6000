using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UnityEngine.Networking;
using ParseData;
using Core;

public class UserService : IDisposable
{
    private readonly IDIContainer container;
    private CancellationTokenSource cts;

    private static UserService instance;
    public static UserService Instance => instance;

    public void Save()
    {
        var lm = UserData.Instance.GetOne<UserProfileModel>();
    }

    public void Load()
    {
        var lm = UserData.Instance.GetOne<UserProfileModel>();
        string deviceId = SystemInfo.deviceUniqueIdentifier;
        lm.UserId = deviceId;
        lm.UserName = "Player_" + deviceId.Substring(0, 8);
        //lm.AgentId = "agent_a3e627b1c00e7f95bbc8f34973";
        lm.AgentId = "agent_36209ddfad1025aff987c94d1e";
        UserData.Instance.InsertOrUpdate(lm);
    }

    public static UserService Instantiate(IDIContainer container)
    {
        if (instance == default)
        {
            instance = new UserService(container);
        }
        return instance;
    }

    private UserService(IDIContainer container)
    {
        this.container = container;
    }

    public void Dispose()
    {
        DisposeUtility.SafeDispose(ref cts);
        instance = default;
    }

}
