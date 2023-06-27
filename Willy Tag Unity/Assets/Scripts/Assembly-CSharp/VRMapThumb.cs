using System;
using UnityEngine;
using UnityEngine.XR;

[Serializable]
public class VRMapThumb : VRMap
{
	public InputFeatureUsage inputAxis;

	public bool primaryButtonTouch;

	public bool primaryButtonPress;

	public bool secondaryButtonTouch;

	public bool secondaryButtonPress;

	public Transform fingerBone1;

	public Transform fingerBone2;

	public Vector3 closedAngle1;

	public Vector3 closedAngle2;

	public Vector3 startingAngle1;

	public Vector3 startingAngle2;

	public override void MapMyFinger(float lerpValue)
	{
		InputDevice deviceAtXRNode = InputDevices.GetDeviceAtXRNode(vrTargetNode);
		calcT = 0f;
		deviceAtXRNode.TryGetFeatureValue(CommonUsages.primaryButton, out primaryButtonPress);
		deviceAtXRNode.TryGetFeatureValue(CommonUsages.primaryTouch, out primaryButtonTouch);
		deviceAtXRNode.TryGetFeatureValue(CommonUsages.secondaryButton, out secondaryButtonPress);
		deviceAtXRNode.TryGetFeatureValue(CommonUsages.secondaryTouch, out secondaryButtonTouch);
		if (primaryButtonPress || secondaryButtonPress)
		{
			calcT = 1f;
		}
		else if (primaryButtonTouch || secondaryButtonTouch)
		{
			calcT = 0.1f;
		}
		LerpFinger(lerpValue);
	}

	public override void LerpFinger(float lerpValue)
	{
		fingerBone1.localRotation = Quaternion.Lerp(fingerBone1.localRotation, Quaternion.Lerp(Quaternion.Euler(startingAngle1), Quaternion.Euler(closedAngle1), calcT), lerpValue);
		fingerBone2.localRotation = Quaternion.Lerp(fingerBone2.localRotation, Quaternion.Lerp(Quaternion.Euler(startingAngle2), Quaternion.Euler(closedAngle2), calcT), lerpValue);
	}
}
