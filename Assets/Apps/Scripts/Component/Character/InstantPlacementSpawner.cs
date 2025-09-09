using System.Collections.Generic;
using System.Threading;

using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Cysharp.Threading.Tasks;
using UnityEngine.EventSystems;

public class InstantPlacementSpawner : MonoBehaviour
{
    public GameObject arAnchorPrefab;
    public Camera mainCamera;
    public bool autoSpawn = false;

    private ARRaycastManager arRaycastManager;
    private ARAnchorManager arAnchorManager;
    private static List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private GameObject spawnedAnchorObject;
    private bool isSpawning = false;
    private CancellationTokenSource cts;

    private bool m_AttemptSpawn;

    void Start()
    {
        arRaycastManager = GetComponent<ARRaycastManager>();
        arAnchorManager = GetComponent<ARAnchorManager>();

        if (arRaycastManager == null || arAnchorManager == null)
        {
            Debug.LogError("ARRaycastManager or ARAnchorManager component not found on XR Origin.");
        }

        cts = new CancellationTokenSource();
        if (autoSpawn)
            _ = Spawn();
    }

    private void OnDisable()
    {
        cts.SafeCancelAndDispose();
        cts = default;
    }


    public async UniTask Spawn()
    {
        try
        {
            while (spawnedAnchorObject == null)
            {
                await TryToSpawn(cts.Token);
            }
        }
        catch { }

    }

    async UniTask TryToSpawn(CancellationToken cancellationToken)
    {
        if (spawnedAnchorObject != null)
        {
            return;
        }


        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        if (arRaycastManager.Raycast(screenCenter, hits, TrackableType.AllTypes))
        {
            ARRaycastHit hit = hits[0];
            isSpawning = true;

            var addAnchorTask = await arAnchorManager.TryAddAnchorAsync(hit.pose).AsUniTask().AttachExternalCancellation(cancellationToken);

            ARAnchor newAnchor = addAnchorTask.value;

            if (newAnchor != null)
            {
                if (spawnedAnchorObject != default) Destroy(spawnedAnchorObject);

                spawnedAnchorObject = Instantiate(arAnchorPrefab, newAnchor.transform);
                spawnedAnchorObject.gameObject.SetActive(false);
                //var local = spawnedAnchorObject.transform.localPosition;
                //local.y = 0;
                //spawnedAnchorObject.transform.localPosition = local;

                
                await UniTask.Delay(100, cancellationToken: cancellationToken);

                bool f = false;
                while (f == false)
                {
                    var billboard = spawnedAnchorObject.GetComponentInChildren<BillboardComponent>();
                    if (billboard != null)
                    {
                        billboard.SetCamera(mainCamera);
                        f = true;
                    }
                    else
                        await UniTask.Delay(100, cancellationToken: cancellationToken);
                }

                Vector3 cameraPosition = Camera.main.transform.position;

                Vector3 direction = -(cameraPosition - spawnedAnchorObject.transform.position);

                direction.y = 0;

                Quaternion newRotation = Quaternion.LookRotation(direction);

                spawnedAnchorObject.transform.rotation = newRotation;
                spawnedAnchorObject.gameObject.SetActive(true);

                Debug.Log("Spawned object on ARAnchor.");
            }

            

            isSpawning = false;
        }

        await UniTask.Delay(100, cancellationToken: cancellationToken);
    }

}
