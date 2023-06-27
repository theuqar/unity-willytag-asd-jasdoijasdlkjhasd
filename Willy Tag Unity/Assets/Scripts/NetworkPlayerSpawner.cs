using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class NetworkPlayerSpawner : MonoBehaviourPunCallbacks
{
    [HideInInspector] public GameObject spawnedPlayerPrefab;
    [HideInInspector] public bool isPlayerSpwaned;

    public Transform rigHead;
    public Transform rigLeftHand;
    public Transform rigRightHand;

    private Transform head;
    private Transform leftHand;
    private Transform rightHand;

    private Transform mIK;

    private new PhotonView photonView;

    void Update()
    {
        // Checks if we are in a room before trying to work with our Network Player
        if (PhotonNetwork.InRoom == true)
            if (photonView.IsMine)
            {
                MapPosition(head, rigHead);
                MapPosition(leftHand, rigLeftHand);
                MapPosition(rightHand, rigRightHand);
            }
    }

    void MapPosition(Transform target, Transform rigTarget)
    {
        // Changes position and rotation of specified transforms

        target.position = rigTarget.position;
        target.rotation = rigTarget.rotation;
    }

    public override void OnJoinedRoom()
    {
        // When we join a room it adds a new Network Player
        base.OnJoinedRoom();
        spawnedPlayerPrefab = PhotonNetwork.Instantiate("Network Player", transform.position, transform.rotation);
        isPlayerSpwaned = true;

        // Get our Photon View and other things
        photonView = spawnedPlayerPrefab.GetComponent<PhotonView>();

        head = spawnedPlayerPrefab.transform.Find("Head");
        leftHand = spawnedPlayerPrefab.transform.Find("Left Hand");
        rightHand = spawnedPlayerPrefab.transform.Find("Right Hand");
    }

    public override void OnLeftRoom()
    {
        // When we leave a room it removes our Network Player
        base.OnLeftRoom();
        PhotonNetwork.Destroy(spawnedPlayerPrefab);
        isPlayerSpwaned = false;
    }
}
