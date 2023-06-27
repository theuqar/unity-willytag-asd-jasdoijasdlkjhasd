using System.Collections;
using GorillaNetworking;
using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

public class SetMasterButton : GorillaPressableButton
{
	public float buttonFadeTime = 0.25f;

	public override void ButtonActivation()
	{
		{
			PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
		}
		base.ButtonActivation();
		StartCoroutine(ButtonColorUpdate());
	}

	private IEnumerator ButtonColorUpdate()
	{
		buttonRenderer.material = pressedMaterial;
		yield return new WaitForSeconds(buttonFadeTime);
		buttonRenderer.material = unpressedMaterial;
	}
}