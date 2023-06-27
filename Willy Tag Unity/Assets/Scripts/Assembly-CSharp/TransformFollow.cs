using UnityEngine;

public class TransformFollow : MonoBehaviour
{
	public Transform transformToFollow;

	public Vector3 offset;

	private void LateUpdate()
	{
		base.transform.rotation = transformToFollow.rotation;
		base.transform.position = transformToFollow.position + transformToFollow.rotation * offset;
	}
}
