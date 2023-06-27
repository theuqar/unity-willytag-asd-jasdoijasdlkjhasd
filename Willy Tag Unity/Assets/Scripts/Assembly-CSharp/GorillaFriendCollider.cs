using System.Collections.Generic;
using GorillaLocomotion;
using Photon.Pun;
using UnityEngine;

public class GorillaFriendCollider : MonoBehaviour
{
	public List<string> playerIDsCurrentlyTouching = new List<string>();

	public CapsuleCollider thisCapsule;

	public void Awake()
	{
		thisCapsule = GetComponent<CapsuleCollider>();
	}

	public void LateUpdate()
	{
		Collider[] array = Physics.OverlapSphere(base.transform.position, thisCapsule.radius, LayerMask.GetMask("Gorilla Tag Collider") | LayerMask.GetMask("Gorilla Body Collider"));
		playerIDsCurrentlyTouching.Clear();
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			if (collider.GetComponentInParent<PhotonView>() != null && !playerIDsCurrentlyTouching.Contains(collider.GetComponentInParent<PhotonView>().Owner.UserId))
			{
				playerIDsCurrentlyTouching.Add(collider.GetComponentInParent<PhotonView>().Owner.UserId);
			}
			else if ((bool)collider.GetComponentInParent<Player>() && !playerIDsCurrentlyTouching.Contains(PhotonNetwork.LocalPlayer.UserId))
			{
				playerIDsCurrentlyTouching.Add(PhotonNetwork.LocalPlayer.UserId);
			}
		}
	}
}
