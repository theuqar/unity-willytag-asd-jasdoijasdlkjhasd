using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class PlayerName : MonoBehaviour
{
    public PhotonView photonView;
    public TextMeshPro nameTag;

    // Start is called before the first frame update
    void Start()
    {
        if (photonView.IsMine) {
            if (PlayerPrefs.HasKey("username"))
            {
                PhotonNetwork.NickName = PlayerPrefs.GetString("username");
            }
            else
            {
                PlayerPrefs.SetString("username", "Cheetah" + Random.Range(0, 1000).ToString());
                PhotonNetwork.NickName = PlayerPrefs.GetString("username");
            }
        }

        SetName();
    }

    private void SetName()
    {
        nameTag.text = photonView.Owner.NickName;
    }

    // Update is called once per frame
    void Update()
    {
        if (nameTag.text != photonView.Owner.NickName)
            SetName();
    }
}
