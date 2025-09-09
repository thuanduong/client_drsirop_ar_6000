using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public static class WebService
{
    public static UnityWebRequest CreateWebRequestWithBody(string request, string json)
    {
        var uwr = new UnityWebRequest(request, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        uwr.uploadHandler = new UploadHandlerRaw(jsonToSend);
        uwr.downloadHandler = new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");
        return uwr;
    }
    public static UnityWebRequest CreateWebRequest(string request)
    {
        var uwr = new UnityWebRequest(request, "POST");
        uwr.downloadHandler = new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");
        return uwr;
    }

    public static UnityWebRequest CreateWebRequestTokenWithBody(string request, string token, string json)
    {
        var uwr = new UnityWebRequest(request, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        uwr.uploadHandler = new UploadHandlerRaw(jsonToSend);
        uwr.downloadHandler = new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Authorization", "Bearer " + token);
        uwr.SetRequestHeader("Content-Type", "application/json");

        string authHeader = uwr.GetRequestHeader("Authorization");
        Debug.Log("Header " + authHeader);

        Debug.Log("Body " + json);
        return uwr;
    }
    public static UnityWebRequest CreateWebRequestToken(string request, string token)
    {
        var uwr = new UnityWebRequest(request, "POST");
        uwr.downloadHandler = new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Authorization", "Bearer " + token);
        uwr.SetRequestHeader("Content-Type", "application/json");
        return uwr;
    }


    public static UnityWebRequest CreateGetWebRequest(string request)
    {
        var uwr = new UnityWebRequest(request, "GET");
        uwr.downloadHandler = new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");
        return uwr;
    }

    public static UnityWebRequest CreateGetWebRequestToken(string request, string token)
    {
        var uwr = new UnityWebRequest(request, "GET");
        uwr.downloadHandler = new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Authorization", "Bearer " + token);
        uwr.SetRequestHeader("Content-Type", "application/json");
        return uwr;
    }
}
