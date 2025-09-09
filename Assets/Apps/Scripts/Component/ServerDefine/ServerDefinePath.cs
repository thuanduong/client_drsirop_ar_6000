using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ServerDefinePath
{
    public static string API { get; private set; }
    public static string WS { get; private set; }

    public static void SetAPI(string value)
    {
        API = value;
    }

    public static void SetWS(string value)
    {
        WS = value;
    }

    public static string GetPath(string api)
    {
        return $"{API}/{api}";
    }

    public static string GetPath(string category, string api)
    {
        return $"{API}/{category}/{api}";
    }
   
}
