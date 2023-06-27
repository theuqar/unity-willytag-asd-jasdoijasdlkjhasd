using UnityEngine;

public class GorillaTriggerBox : MonoBehaviour
{
	public bool triggerBoxOnce;

	public void Update()
	{
		if (triggerBoxOnce)
		{
			triggerBoxOnce = false;
			OnBoxTriggered();
		}
	}

	public virtual void OnBoxTriggered()
	{
	}
}
