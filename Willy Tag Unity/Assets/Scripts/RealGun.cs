using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using easyInputs;

public class RealGun : MonoBehaviour
{
    //made by TheIrishSanta
    public GameObject shootpoint;
    public PhotonView view;
    private BulletSped bulletScript;
    public GameObject NormalBullet;

    private void Start()
    {
        bulletScript = NormalBullet.GetComponent<BulletSped>();
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Shoot();
        }

        if (EasyInputs.GetTriggerButtonDown(EasyHand.LeftHand)){
            Shoot();
        }
    }
    public void Shoot()
    {
        if (view.IsMine)
        {
            PhotonNetwork.Instantiate(NormalBullet.gameObject.name, shootpoint.transform.position, Quaternion.identity, 0);
        }
    }
}
