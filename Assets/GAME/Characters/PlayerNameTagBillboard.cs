using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class PlayerNameTagBillboard : NetworkBehaviour
{
    CinemachineVirtualCamera mainCamera;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if(!IsOwner) return;
        mainCamera = FindAnyObjectByType<CinemachineVirtualCamera>();
    }
    void Start()
    {
        
    }

    void LateUpdate()
    {
        if(!IsOwner) return;
        transform.LookAt(mainCamera.transform);
        //transform.Rotate(0, 180, 0);
    }
}
