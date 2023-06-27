using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Photon.Pun;

public class CosmeticsControllerFexlar : MonoBehaviour
{
    public bool enableButton;
    public string CosmeticName;

    private float hapticWaitSeconds = 0.05f;

    public NetworkPlayerSpawner networkSpawner;
    private PhotonView photonView;

    //private void Start()
    //{
        //networkSpawner = GameObject.Find("/NetworkManager").GetComponent<NetworkPlayerSpawner>();
    //}

    private void Update()
    {
        if (PhotonNetwork.InRoom)
        {
            if (networkSpawner.isPlayerSpwaned)
            {
                photonView = networkSpawner.spawnedPlayerPrefab.GetComponent<PhotonView>();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (PhotonNetwork.InRoom)
        {
            if (other.gameObject.name == "LeftHand Controller" || other.gameObject.name == "RightHand Controller")
            {
                base.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
                if (enableButton)
                    photonView.RPC("EnableCosmetic", RpcTarget.All, CosmeticName);
                else
                    photonView.RPC("DisableCosmetic", RpcTarget.All, CosmeticName);

                if (other.gameObject.name == "LeftHand Controller")
                    StartVibration(true, 0.7f, 0.15f);
                if (other.gameObject.name == "RightHand Controller")
                    StartVibration(false, 0.7f, 0.15f);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        base.GetComponent<Renderer>().material.SetColor("_Color", Color.white);
    }

    public void StartVibration(bool forLeftController, float amplitude, float duration)
    {
        base.StartCoroutine(this.HapticPulses(forLeftController, amplitude, duration));
    }

    // Token: 0x06000315 RID: 789 RVA: 0x00016512 File Offset: 0x00014712
    private IEnumerator HapticPulses(bool forLeftController, float amplitude, float duration)
    {
        float startTime = Time.time;
        uint channel = 0U;
        InputDevice device;
        if (forLeftController)
        {
            device = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        }
        else
        {
            device = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        }
        while (Time.time < startTime + duration)
        {
            device.SendHapticImpulse(channel, amplitude, this.hapticWaitSeconds);
            yield return new WaitForSeconds(this.hapticWaitSeconds * 0.9f);
        }
        yield break;
    }
}
