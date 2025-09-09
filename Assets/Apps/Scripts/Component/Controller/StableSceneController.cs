using UnityEngine;
using Singleton;

public class StableSceneController : Singleton<StableSceneController>
{
    [SerializeField] Transform horseHolder;

    public Transform HorseHolder => horseHolder;
}
