using System.Collections;
using GorillaNetworking;
using UnityEngine;

public class PurchaseItemButton : GorillaPressableButton
{
	public string buttonSide;

	public override void ButtonActivation()
	{
		base.ButtonActivation();
		CosmeticsController.instance.PressPurchaseItemButton(this);
		StartCoroutine(ButtonColorUpdate());
	}

	private IEnumerator ButtonColorUpdate()
	{
		Debug.Log("did this happen?");
		buttonRenderer.material = pressedMaterial;
		yield return new WaitForSeconds(debounceTime);
		buttonRenderer.material = (isOn ? pressedMaterial : unpressedMaterial);
	}
}
