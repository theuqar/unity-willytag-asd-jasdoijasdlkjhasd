using UnityEngine;
using UnityEngine.UI;

public class GorillaPlayerLineButton : MonoBehaviour
{
	public enum ButtonType
	{
		HateSpeech = 0,
		Cheating = 1,
		Toxicity = 2,
		Mute = 3,
		Report = 4,
		Cancel = 5
	}

	public GorillaPlayerScoreboardLine parentLine;

	public ButtonType buttonType;

	public bool isOn;

	public Material offMaterial;

	public Material onMaterial;

	public string offText;

	public string onText;

	public Text myText;

	public float debounceTime = 0.25f;

	public float touchTime;

	public bool testPress;

	private void Update()
	{
		if (testPress)
		{
			testPress = false;
			if (buttonType == ButtonType.Mute)
			{
				isOn = !isOn;
			}
			parentLine.PressButton(isOn, buttonType);
		}
	}

	private void OnTriggerEnter(Collider collider)
	{
		if (!base.enabled || !(touchTime + debounceTime < Time.time))
		{
			return;
		}
		touchTime = Time.time;
		Debug.Log("collision detected" + collider, collider);
		if (!(collider.GetComponentInParent<GorillaTriggerColliderHandIndicator>() != null))
		{
			return;
		}
		GorillaTriggerColliderHandIndicator component = collider.GetComponent<GorillaTriggerColliderHandIndicator>();
		Debug.Log("buttan press");
		if (buttonType == ButtonType.Mute)
		{
			isOn = !isOn;
		}
		if (buttonType == ButtonType.Mute || buttonType == ButtonType.HateSpeech || buttonType == ButtonType.Cheating || buttonType == ButtonType.Cancel || parentLine.canPressNextReportButton)
		{
			parentLine.PressButton(isOn, buttonType);
			if (component != null)
			{
				GorillaTagger.Instance.StartVibration(component.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (buttonType != ButtonType.Mute && other.GetComponentInParent<GorillaTriggerColliderHandIndicator>() != null)
		{
			parentLine.canPressNextReportButton = true;
		}
	}

	public void UpdateColor()
	{
		if (isOn)
		{
			GetComponent<MeshRenderer>().material = onMaterial;
			myText.text = onText;
		}
		else
		{
			GetComponent<MeshRenderer>().material = offMaterial;
			myText.text = offText;
		}
	}
}
