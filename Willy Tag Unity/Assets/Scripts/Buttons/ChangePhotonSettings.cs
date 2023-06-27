using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;

public class ChangePhotonSettings
{
    [Header("Do not use a Realtime app id use a PUN one!!!!")]
    public AppSettings PhotonServerSetting;

    public void OnTriggerCollide()
    {
        PhotonNetwork.Disconnect();
        PhotonNetwork.ConnectUsingSettings(PhotonServerSetting);
    }

}
