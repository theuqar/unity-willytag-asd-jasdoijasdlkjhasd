using System.Collections;
using GorillaNetworking;
using UnityEngine;

public class EarlyAccessButton : GorillaPressableButton
{
	private void Awake()
	{
		base.enabled = false;
		GetComponent<BoxCollider>().enabled = false;
		buttonRenderer.material = pressedMaterial;
		myText.text = "IN APP PURCHASE NOT AVAILABLE! CHECK THE STEAM STORE!";
	}

	public void Update()
	{
		if (PhotonNetworkController.instance != null && PhotonNetworkController.instance.wrongVersion)
		{
			base.enabled = false;
			GetComponent<BoxCollider>().enabled = false;
			buttonRenderer.material = pressedMaterial;
			myText.text = "UNAVAILABLE";
		}
	}

	public override void ButtonActivation()
	{
	}

	public void AlreadyOwn()
	{
		base.enabled = false;
		GetComponent<BoxCollider>().enabled = false;
		buttonRenderer.material = pressedMaterial;
		myText.text = "YOU OWN THE SUPPORTER PACK! THANK YOU!";
	}

	private IEnumerator ButtonColorUpdate()
	{
		Debug.Log("did this happen?");
		buttonRenderer.material = pressedMaterial;
		yield return new WaitForSeconds(debounceTime);
		buttonRenderer.material = (isOn ? pressedMaterial : unpressedMaterial);
	}
}
