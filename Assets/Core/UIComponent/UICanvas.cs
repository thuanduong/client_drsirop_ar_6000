using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICanvas : MonoBehaviour
{
    public enum UICanvasType
    {
        BackGround,
        Default,
        Header,
        PopUp,
        Loading,
        Debug,
        Error,
        Info,
        BackgroundWorld,
    }

    public static Canvas GetCanvas(UICanvasType canvasType)
    {
        return canvasType switch
        {
            UICanvasType.BackGround => BackgroundCanvas,
            UICanvasType.Default => DefaultCanvas,
            UICanvasType.Header => HeaderCanvas,
            UICanvasType.PopUp => PopUpUICanvas,
            UICanvasType.Loading => LoadingCanvas,
            UICanvasType.Debug => DebugCanvas,
            UICanvasType.Error => ErrorCanvas,
            UICanvasType.Info => ClientInfoCanvas,
            UICanvasType.BackgroundWorld => BackgroundCanvasWorld,
            _ => DefaultCanvas
        };
    }

    [SerializeField]
    private Canvas backgroundCanvas;
    [SerializeField]
    private Canvas defaultCanvas;
    [SerializeField]
    private Canvas headerCanvas;
    [SerializeField]
    private Canvas popUpUICanvas;
    [SerializeField]
    private Canvas loadingCanvas;
    [SerializeField]
    private Canvas debugUICanvas;
    [SerializeField]
    private Canvas errorUICanvas;
    [SerializeField]
    private Canvas clientInfoCanvas;
    [SerializeField]
    private Canvas backgroundCanvasWorld;

    private static Canvas BackgroundCanvas { get; set; }
    private static Canvas DefaultCanvas { get; set; }
    private static Canvas HeaderCanvas { get; set; }
    private static Canvas PopUpUICanvas { get; set; }
    private static Canvas LoadingCanvas { get; set; }
    private static Canvas DebugCanvas { get; set; }
    private static Canvas ErrorCanvas { get; set; }
    private static Canvas ClientInfoCanvas { get; set; }
    private static Canvas BackgroundCanvasWorld { get; set; }

    private void Awake()
    {
        DefaultCanvas = defaultCanvas;
        LoadingCanvas = loadingCanvas;
        BackgroundCanvas = backgroundCanvas;
        HeaderCanvas = headerCanvas;
        PopUpUICanvas = popUpUICanvas;
        DebugCanvas = debugUICanvas;
        ErrorCanvas = errorUICanvas;
        ClientInfoCanvas = clientInfoCanvas;
        BackgroundCanvasWorld = backgroundCanvasWorld;
    }
}
