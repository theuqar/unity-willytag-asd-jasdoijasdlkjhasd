using System;
using UnityEngine;
using UnityEngine.XR;

[Serializable]
public class VRMapIndex : VRMap
{
	public InputFeatureUsage inputAxis;

	public float triggerTouch;

	public float triggerValue;

	public Transform fingerBone1;

	public Transform fingerBone2;

	public Transform fingerBone3;

	public float closedAngles;

	public Vector3 closedAngle1;

	public Vector3 closedAngle2;

	public Vector3 closedAngle3;

	public Vector3 startingAngle1;

	public Vector3 startingAngle2;

	public Vector3 startingAngle3;

	private InputDevice myInputDevice;

	public override void MapMyFinger(float lerpValue)
	{
		myInputDevice = InputDevices.GetDeviceAtXRNode(vrTargetNode);
		calcT = 0f;
		myInputDevice.TryGetFeatureValue(CommonUsages.trigger, out triggerValue);
		if (myInputDevice.TryGetFeatureValue(CommonUsages.indexTouch, out triggerTouch))
		{
			calcT = 0.1f * triggerTouch;
			calcT += 0.9f * triggerValue;
			LerpFinger(lerpValue);
		}
		else
		{
			calcT = 1f * triggerValue;
			LerpFinger(lerpValue);
		}
	}

	public override void LerpFinger(float lerpValue)
	{
		fingerBone1.localRotation = Quaternion.Lerp(fingerBone1.localRotation, Quaternion.Lerp(Quaternion.Euler(startingAngle1), Quaternion.Euler(closedAngle1), calcT), lerpValue);
		fingerBone2.localRotation = Quaternion.Lerp(fingerBone2.localRotation, Quaternion.Lerp(Quaternion.Euler(startingAngle2), Quaternion.Euler(closedAngle2), calcT), lerpValue);
		fingerBone3.localRotation = Quaternion.Lerp(fingerBone3.localRotation, Quaternion.Lerp(Quaternion.Euler(startingAngle3), Quaternion.Euler(closedAngle3), calcT), lerpValue);
	}
}
