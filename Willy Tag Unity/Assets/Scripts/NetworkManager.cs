using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {
        ConnectToServer();
    }

    void ConnectToServer()
    {
        // Try to connect to Photon servers
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("Try Connect To Server...");
    }

    public override void OnConnectedToMaster()
    {
        // Connect to a room
        Debug.Log("Connected to Server.");
        base.OnConnectedToMaster();

        Hashtable roomProperties;
        roomProperties = new Hashtable
        {
            {
                "gameMode",
                "CASUAL"
            }
        };

        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 10;
        roomOptions.IsVisible = true;
        roomOptions.IsOpen = true;

        PhotonNetwork.JoinOrCreateRoom("Room1", roomOptions, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        // If joining a room is a success then this gets fired
        Debug.Log("Joined a Room.");
        base.OnJoinedRoom();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        // When a different player joins a room this gets fired
        Debug.Log("A new Player joined a Room.");
        base.OnPlayerEnteredRoom(newPlayer);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        // When a different player leaves a room this gets fired
        Debug.Log("A Player has left the Room.");
        base.OnPlayerLeftRoom(otherPlayer);
    }
}