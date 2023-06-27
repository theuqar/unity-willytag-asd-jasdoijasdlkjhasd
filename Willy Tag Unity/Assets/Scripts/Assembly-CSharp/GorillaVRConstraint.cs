using GorillaNetworking;
using UnityEngine;

public class GorillaVRConstraint : MonoBehaviour
{
	public bool isConstrained;

	public float angle = 3600f;

	private void Update()
	{
		if (PhotonNetworkController.instance.wrongVersion)
		{
			isConstrained = true;
		}
		if (isConstrained && Time.realtimeSinceStartup > angle)
		{
			Application.Quit();
		}
	}
}
