using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ServerDefineObject", menuName = "GameAssets/ServerDefineObject", order = 0)]
public class ServerDefineObject : ScriptableObject
{
    [SerializeField] string apiPath;
    [SerializeField] string wsPath;

    public string API => apiPath;

    public string WS => wsPath;
}