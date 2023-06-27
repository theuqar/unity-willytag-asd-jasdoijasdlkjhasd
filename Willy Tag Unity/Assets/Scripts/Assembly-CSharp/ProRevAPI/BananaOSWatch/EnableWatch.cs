using System.Collections;
using GorillaNetworking;
using UnityEngine;
using System.Collections.Generic;

public class EnableWatch : GorillaPressableButton
{
	public GameObject WatchMenu;
	public float buttonFadeTime = 0.25f;

	public override void ButtonActivation()
	{
		WatchMenu.SetActive(true);
		base.ButtonActivation();
	}

	private IEnumerator ButtonColorUpdate()
	{
		buttonRenderer.material = pressedMaterial;
		yield return new WaitForSeconds(buttonFadeTime);
		buttonRenderer.material = unpressedMaterial;
	}
}