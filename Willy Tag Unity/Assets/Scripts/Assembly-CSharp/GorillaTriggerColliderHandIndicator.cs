using UnityEngine;

public class GorillaTriggerColliderHandIndicator : MonoBehaviour
{
	public bool isLeftHand;

	public GorillaThrowableController throwableController;

	private void OnTriggerEnter(Collider other)
	{
		if (throwableController != null)
		{
			throwableController.GrabbableObjectHover(isLeftHand);
		}
	}
}
