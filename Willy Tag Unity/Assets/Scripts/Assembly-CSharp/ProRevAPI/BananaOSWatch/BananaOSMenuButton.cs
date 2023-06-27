using System.Collections;
using GorillaNetworking;
using UnityEngine;
using System.Collections.Generic;

public class BananaOSMenuButton : GorillaPressableButton
{
	public List<GameObject> ObjectsToDisable;
	public List<GameObject> ObjectsToEnable;
	public float buttonFadeTime = 0.25f;

	public override void ButtonActivation()
	{
        {
			foreach (GameObject obj in ObjectsToDisable)
			{
				obj.SetActive(false);
			}
			foreach (GameObject obj in ObjectsToEnable)
			{
				obj.SetActive(true);
			}
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