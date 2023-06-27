using UnityEngine;
using UnityEngine.UI;

public class GorillaPressableButton : MonoBehaviour
{
	public Material pressedMaterial;

	public Material unpressedMaterial;

	public MeshRenderer buttonRenderer;

	public bool isOn;

	public float debounceTime = 0.25f;

	public float touchTime;

	public bool testPress;

	[TextArea]
	public string offText;

	[TextArea]
	public string onText;

	public Text myText;

	public virtual void Start()
	{
	}

	private void Update()
	{
		if (testPress)
		{
			testPress = false;
			ButtonActivation();
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
		if (collider.GetComponentInParent<GorillaTriggerColliderHandIndicator>() != null)
		{
			GorillaTriggerColliderHandIndicator component = collider.GetComponent<GorillaTriggerColliderHandIndicator>();
			Debug.Log("buttan press");
			ButtonActivation();
			if (component != null)
			{
				GorillaTagger.Instance.StartVibration(component.isLeftHand, GorillaTagger.Instance.tapHapticStrength / 2f, GorillaTagger.Instance.tapHapticDuration);
			}
		}
	}

	public virtual void UpdateColor()
	{
		if (isOn)
		{
			buttonRenderer.material = pressedMaterial;
			myText.text = onText;
		}
		else
		{
			buttonRenderer.material = unpressedMaterial;
			myText.text = offText;
		}
	}

	public virtual void ButtonActivation()
	{
	}
}
