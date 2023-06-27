using System;
using UnityEngine;
using UnityEngine.XR;

[Serializable]
public class VRMap
{
	public XRNode vrTargetNode;

	public Transform overrideTarget;

	public Transform rigTarget;

	public Vector3 trackingPositionOffset;

	public Vector3 trackingRotationOffset;

	public Transform headTransform;

	public Vector3 syncPos;

	public Quaternion syncRotation;

	public float calcT;

	public void MapOther(float lerpValue)
	{
		rigTarget.localPosition = Vector3.Lerp(rigTarget.localPosition, syncPos, lerpValue);
		rigTarget.localRotation = Quaternion.Lerp(rigTarget.localRotation, syncRotation, lerpValue);
	}

	public void MapMine(float ratio, Transform playerOffsetTransform)
	{
		InputDevice deviceAtXRNode = InputDevices.GetDeviceAtXRNode(vrTargetNode);
		deviceAtXRNode.TryGetFeatureValue(CommonUsages.deviceRotation, out var value);
		deviceAtXRNode.TryGetFeatureValue(CommonUsages.devicePosition, out var value2);
		rigTarget.rotation = value * Quaternion.Euler(trackingRotationOffset);
		if (overrideTarget != null)
		{
			rigTarget.RotateAround(overrideTarget.position, Vector3.up, playerOffsetTransform.eulerAngles.y);
			rigTarget.position = overrideTarget.position + rigTarget.rotation * trackingPositionOffset;
		}
		else
		{
			rigTarget.position = value2 + rigTarget.rotation * trackingPositionOffset + playerOffsetTransform.position;
			rigTarget.RotateAround(playerOffsetTransform.position, Vector3.up, playerOffsetTransform.eulerAngles.y);
		}
	}

	public virtual void MapOtherFinger(float handSync, float lerpValue)
	{
		calcT = handSync;
		LerpFinger(lerpValue);
	}

	public virtual void MapMyFinger(float lerpValue)
	{
	}

	public virtual void LerpFinger(float lerpValue)
	{
	}
}
