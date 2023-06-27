using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Photon.Pun;

public class GorillaTargetDrop : MonoBehaviourPunCallbacks
{
    public XRNode controllerNode;
    public GameObject targetObject;
    public GameObject targetPrefab;

    private bool isTargetSpawned = false;
    private GameObject spawnedTarget;

    private Transform targetTransform;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;

    void Start()
    {
        // Store the original transform of the object
        targetTransform = targetObject.transform;
        originalPosition = targetTransform.localPosition;
        originalRotation = targetTransform.localRotation;
        originalScale = targetTransform.localScale;
    }

    void Update()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        InputDevice controller = InputDevices.GetDeviceAtXRNode(controllerNode);

        bool bButtonPressed = false;
        if (controller.TryGetFeatureValue(CommonUsages.primaryButton, out bButtonPressed) && bButtonPressed)
        {
            if (!isTargetSpawned)
            {
                // Disable the target object and spawn the target prefab
                targetObject.SetActive(false);
                photonView.RPC("SpawnTargetRPC", RpcTarget.AllBuffered, targetTransform.position, targetTransform.rotation);
            }
        }
        else if (controller.TryGetFeatureValue(CommonUsages.secondaryButton, out bButtonPressed) && bButtonPressed)
        {
            if (isTargetSpawned)
            {
                // Destroy the spawned target and re-enable the target object
                photonView.RPC("DestroyTargetRPC", RpcTarget.AllBuffered);
                targetObject.SetActive(true);
            }
        }
    }

    [PunRPC]
    void SpawnTargetRPC(Vector3 position, Quaternion rotation)
    {
        // Spawn the target prefab as a PhotonRoomPrefab
        spawnedTarget = PhotonNetwork.Instantiate(targetPrefab.name, position, rotation, 0);
        isTargetSpawned = true;
    }

    [PunRPC]
    void DestroyTargetRPC()
    {
        // Destroy the spawned target and set the target object to active
        PhotonNetwork.Destroy(spawnedTarget);
        isTargetSpawned = false;
    }
}
