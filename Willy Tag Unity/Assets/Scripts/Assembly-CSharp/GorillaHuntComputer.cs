using System;
using GorillaLocomotion;
using GorillaNetworking;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class GorillaHuntComputer : MonoBehaviour
{
	public Text text;

	public Image material;

	public Image hat;

	public Image face;

	public Image badge;

	public Photon.Realtime.Player myTarget;

	public Photon.Realtime.Player tempTarget;

	public VRRig myRig;

	public Sprite tempSprite;

	public CosmeticsController.CosmeticItem tempItem;

	private void Update()
	{
		if (!PhotonNetwork.InRoom || !(GorillaGameManager.instance != null) || !(GorillaGameManager.instance.gameObject.GetComponent<GorillaHuntManager>() != null))
		{
			return;
		}
		if (!GorillaGameManager.instance.gameObject.GetComponent<GorillaHuntManager>().huntStarted)
		{
			if (GorillaGameManager.instance.gameObject.GetComponent<GorillaHuntManager>().waitingToStartNextHuntGame && GorillaGameManager.instance.gameObject.GetComponent<GorillaHuntManager>().currentTarget.Contains(PhotonNetwork.LocalPlayer) && !GorillaGameManager.instance.gameObject.GetComponent<GorillaHuntManager>().currentHunted.Contains(PhotonNetwork.LocalPlayer) && GorillaGameManager.instance.gameObject.GetComponent<GorillaHuntManager>().countDownTime == 0)
			{
				material.gameObject.SetActive(value: false);
				hat.gameObject.SetActive(value: false);
				face.gameObject.SetActive(value: false);
				badge.gameObject.SetActive(value: false);
				text.text = "YOU WON! CONGRATS, HUNTER!";
			}
			else if (GorillaGameManager.instance.gameObject.GetComponent<GorillaHuntManager>().countDownTime != 0)
			{
				material.gameObject.SetActive(value: false);
				hat.gameObject.SetActive(value: false);
				face.gameObject.SetActive(value: false);
				badge.gameObject.SetActive(value: false);
				text.text = "GAME STARTING IN:\n" + GorillaGameManager.instance.gameObject.GetComponent<GorillaHuntManager>().countDownTime + "...";
			}
			else
			{
				material.gameObject.SetActive(value: false);
				hat.gameObject.SetActive(value: false);
				face.gameObject.SetActive(value: false);
				badge.gameObject.SetActive(value: false);
				text.text = "WAITING TO START";
			}
			return;
		}
		myTarget = GorillaGameManager.instance.GetComponent<GorillaHuntManager>().GetTargetOf(PhotonNetwork.LocalPlayer);
		if (myTarget == null)
		{
			material.gameObject.SetActive(value: false);
			hat.gameObject.SetActive(value: false);
			face.gameObject.SetActive(value: false);
			badge.gameObject.SetActive(value: false);
			text.text = "YOU ARE DEAD\nTAG OTHERS\nTO SLOW THEM";
		}
		else if (GorillaGameManager.instance.FindVRRigForPlayer(myTarget) != null)
		{
			myRig = GorillaGameManager.instance.FindVRRigForPlayer(myTarget).GetComponent<VRRig>();
			if (myRig != null)
			{
				material.material = myRig.materialsToChangeTo[myRig.setMatIndex];
				text.text = "TARGET:\n" + NormalizeName(doIt: true, myRig.photonView.Owner.NickName) + "\nDISTANCE: " + Mathf.CeilToInt((GorillaLocomotion.Player.Instance.headCollider.transform.position - myRig.transform.position).magnitude) + "M";
				SetImage(myRig.hat, ref hat);
				SetImage(myRig.face, ref face);
				SetImage(myRig.badge, ref badge);
				material.gameObject.SetActive(value: true);
			}
		}
	}

	private void SetImage(string itemDisplayName, ref Image image)
	{
		tempItem = CosmeticsController.instance.GetItemFromDict(CosmeticsController.instance.GetItemNameFromDisplayName(itemDisplayName));
		if (tempItem.displayName != "NOTHING" && myRig != null && myRig.concatStringOfCosmeticsAllowed != null && myRig.concatStringOfCosmeticsAllowed.Contains(tempItem.itemName))
		{
			image.gameObject.SetActive(value: true);
			image.sprite = tempItem.itemPicture;
		}
		else
		{
			image.gameObject.SetActive(value: false);
		}
	}

	public string NormalizeName(bool doIt, string text)
	{
		if (doIt)
		{
			text = new string(Array.FindAll(text.ToCharArray(), (char c) => char.IsLetterOrDigit(c)));
			if (text.Length > 11)
			{
				text = text.Substring(0, 11);
			}
			text = text.ToUpper();
		}
		return text;
	}
}
