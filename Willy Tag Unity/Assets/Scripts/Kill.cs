/* 
    !!THIS SCRIPT CAN BE ONLY USED WITH PERMISSION BY FEXLAR!!

    Script created by: Fexlar
    Discord: fexlar#0438
    Script Name: Kill.cs
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Kill : MonoBehaviour
{
    public Transform gorillaPlayer;
    public Transform respawnPoint;
    public List<GameObject> boundariesToDisable;
    public float delayBeforeReEnabling;

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.IsChildOf(gorillaPlayer))
        {
            Debug.Log("is child!");
            StartCoroutine(TeleportPlayer());
        }
    }

    IEnumerator TeleportPlayer()
    {

        foreach (GameObject x in boundariesToDisable)
        {
            x.SetActive(false);
        }

        //gorillaPlayer.parent.gameObject.SetActive(false);
        gorillaPlayer.position = respawnPoint.position;
        yield return new WaitForSeconds(delayBeforeReEnabling);
        //gorillaPlayer.parent.gameObject.SetActive(true);

        foreach (GameObject x in boundariesToDisable)
        {
            x.SetActive(true);
        }
    }
}
