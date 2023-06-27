using UnityEngine;
using UnityEngine.Animations.Rigging;

public class CalibrationCube : MonoBehaviour
{
	public PrimaryButtonWatcher watcher;

	public GameObject rightController;

	public GameObject leftController;

	public GameObject playerBody;

	private float calibratedLength;

	private float lastCalibratedLength;

	public float minLength = 1f;

	public float maxLength = 2.5f;

	public float baseLength = 1.61f;

	private void Start()
	{
		calibratedLength = baseLength;
	}

	private void OnTriggerEnter(Collider other)
	{
		watcher.primaryButtonPress.AddListener(RecalibrateSize);
	}

	private void OnTriggerExit(Collider other)
	{
		watcher.primaryButtonPress.RemoveListener(RecalibrateSize);
	}

	public void RecalibrateSize(bool pressed)
	{
		lastCalibratedLength = calibratedLength;
		calibratedLength = (rightController.transform.position - leftController.transform.position).magnitude;
		calibratedLength = ((calibratedLength > maxLength) ? maxLength : ((calibratedLength < minLength) ? minLength : calibratedLength));
		float num = calibratedLength / lastCalibratedLength;
		Vector3 localScale = playerBody.transform.localScale;
		playerBody.GetComponentInChildren<RigBuilder>().Clear();
		playerBody.transform.localScale = new Vector3(1f, 1f, 1f);
		playerBody.GetComponentInChildren<TransformReset>().ResetTransforms();
		playerBody.transform.localScale = num * localScale;
		playerBody.GetComponentInChildren<RigBuilder>().Build();
		playerBody.GetComponentInChildren<VRRig>().SetHeadBodyOffset();
		GorillaPlaySpace.Instance.bodyColliderOffset *= num;
		GorillaPlaySpace.Instance.bodyCollider.gameObject.transform.localScale *= num;
	}
}
