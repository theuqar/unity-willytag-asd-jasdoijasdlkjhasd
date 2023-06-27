using UnityEngine;

public class CosmeticBoundaryTrigger : GorillaTriggerBox
{
	public VRRig rigRef;

	public void OnTriggerEnter(Collider other)
	{
		rigRef = other.attachedRigidbody.gameObject.GetComponent<VRRig>();
		rigRef.inTryOnRoom = true;
		rigRef.LocalUpdateCosmeticsWithTryon(rigRef.badge, rigRef.face, rigRef.hat, rigRef.tryOnRoomBadge, rigRef.tryOnRoomFace, rigRef.tryOnRoomHat);
	}

	public void OnTriggerExit(Collider other)
	{
		rigRef = other.attachedRigidbody.gameObject.GetComponent<VRRig>();
		rigRef.inTryOnRoom = false;
		rigRef.LocalUpdateCosmeticsWithTryon(rigRef.badge, rigRef.face, rigRef.hat, rigRef.tryOnRoomBadge, rigRef.tryOnRoomFace, rigRef.tryOnRoomHat);
	}
}
