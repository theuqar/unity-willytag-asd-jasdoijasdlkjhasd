// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// GorillaTagger
using System.Collections;
using GorillaLocomotion;
using GorillaNetworking;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Unity;
using UnityEngine;
using UnityEngine.XR;

public class GorillaTagger : MonoBehaviour
{
	public enum StatusEffect
	{
		None,
		Frozen,
		Slowed,
		Dead,
		Infected,
		It
	}

	private static GorillaTagger _instance;

	public float sphereCastRadius;

	public bool inCosmeticsRoom;

	public SphereCollider headCollider;

	public CapsuleCollider bodyCollider;

	private Vector3 lastLeftHandPositionForTag;

	private Vector3 lastRightHandPositionForTag;

	private Vector3 lastBodyPositionForTag;

	private Vector3 lastHeadPositionForTag;

	public Transform rightHandTransform;

	public Transform leftHandTransform;

	public float hapticWaitSeconds = 0.05f;

	public float handTapVolume = 0.1f;

	public float tapCoolDown = 0.15f;

	public float lastLeftTap;

	public float lastRightTap;

	public float tapHapticDuration = 0.05f;

	public float tapHapticStrength = 0.5f;

	public float tagHapticDuration = 0.15f;

	public float tagHapticStrength = 1f;

	public float taggedHapticDuration = 0.35f;

	public float taggedHapticStrength = 1f;

	private bool leftHandTouching;

	private bool rightHandTouching;

	public float taggedTime;

	public float tagCooldown;

	public float slowCooldown = 3f;

	public VRRig myVRRig;

	public VRRig offlineVRRig;

	public GameObject mainCamera;

	public bool testTutorial;

	public bool disableTutorial;

	public bool frameRateUpdated;

	public GameObject leftHandTriggerCollider;

	public GameObject rightHandTriggerCollider;

	public Camera mirrorCamera;

	private Vector3 leftRaycastSweep;

	private Vector3 leftHeadRaycastSweep;

	private Vector3 rightRaycastSweep;

	private Vector3 rightHeadRaycastSweep;

	private Vector3 headRaycastSweep;

	private Vector3 bodyRaycastSweep;

	private InputDevice rightDevice;

	private InputDevice leftDevice;

	private bool primaryButtonPressRight;

	private bool secondaryButtonPressRight;

	private bool primaryButtonPressLeft;

	private bool secondaryButtonPressLeft;

	private RaycastHit hitInfo;

	public Photon.Realtime.Player otherPlayer;

	private Photon.Realtime.Player tryPlayer;

	private Vector3 topVector;

	private Vector3 bottomVector;

	private Vector3 bodyVector;

	private int tempInt;

	private InputDevice inputDevice;

	private bool wasInOverlay;

	public StatusEffect currentStatus;

	public float statusStartTime;

	public float statusEndTime;

	public static GorillaTagger Instance => _instance;

	private void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			_instance = this;
		}
		if (!disableTutorial && (testTutorial || (PlayerPrefs.GetString("tutorial") != "done" && PhotonNetworkController.instance.gameVersion != "dev")))
		{
			base.transform.parent.position = new Vector3(-140f, 28f, -102f);
			base.transform.parent.eulerAngles = new Vector3(0f, 180f, 0f);
			PlayerPrefs.SetFloat("redValue", Random.value);
			PlayerPrefs.SetFloat("greenValue", Random.value);
			PlayerPrefs.SetFloat("blueValue", Random.value);
			PlayerPrefs.Save();
			UpdateColor(PlayerPrefs.GetFloat("redValue", 0f), PlayerPrefs.GetFloat("greenValue", 0f), PlayerPrefs.GetFloat("blueValue", 0f));
		}
		inputDevice = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
		wasInOverlay = false;
	}

	private void Start()
	{
		if (XRSettings.loadedDeviceName == "OpenVR")
		{
			GorillaLocomotion.Player.Instance.leftHandOffset = new Vector3(-0.02f, 0f, -0.07f);
			GorillaLocomotion.Player.Instance.rightHandOffset = new Vector3(0.02f, 0f, -0.07f);
		}
		bodyVector = new Vector3(0f, bodyCollider.height / 2f - bodyCollider.radius, 0f);
	}

	private void LateUpdate()
	{
		/*
		if (Application.platform != RuntimePlatform.Android)
				{
					if (Mathf.Abs(Time.fixedDeltaTime - 1f / XRDevice.refreshRate) > 0.0001f)
					{
						Time.fixedDeltaTime = 1f / XRDevice.refreshRate;
						GorillaLocomotion.Player.Instance.velocityHistorySize = Mathf.Min(Mathf.FloorToInt(XRDevice.refreshRate * (1f / 12f)), 10);
						if (GorillaLocomotion.Player.Instance.velocityHistorySize > 9)
						{
							GorillaLocomotion.Player.Instance.velocityHistorySize--;
						}
						Debug.Log("new history size: " + GorillaLocomotion.Player.Instance.velocityHistorySize);
						GorillaLocomotion.Player.Instance.InitializeValues();
					}
				}
				if (Application.platform != RuntimePlatform.Android && Mathf.Abs(Time.fixedDeltaTime - 1f / 144f) > 0.0001f)
				{
					Time.fixedDeltaTime = 1f / 144f;
					GorillaLocomotion.Player.Instance.velocityHistorySize = Mathf.Min(Mathf.FloorToInt(12f), 10);
					if (GorillaLocomotion.Player.Instance.velocityHistorySize > 9)
					{
						GorillaLocomotion.Player.Instance.velocityHistorySize--;
					}
					Debug.Log("new history size: " + GorillaLocomotion.Player.Instance.velocityHistorySize);
					GorillaLocomotion.Player.Instance.InitializeValues();
				}
		*/
		leftRaycastSweep = leftHandTransform.position - lastLeftHandPositionForTag;
		leftHeadRaycastSweep = leftHandTransform.position - headCollider.transform.position;
		rightRaycastSweep = rightHandTransform.position - lastRightHandPositionForTag;
		rightHeadRaycastSweep = rightHandTransform.position - headCollider.transform.position;
		headRaycastSweep = headCollider.transform.position - lastHeadPositionForTag;
		bodyRaycastSweep = bodyCollider.transform.position - lastBodyPositionForTag;
		otherPlayer = null;
		if (Physics.SphereCast(lastLeftHandPositionForTag, sphereCastRadius, leftRaycastSweep, out hitInfo, Mathf.Max(leftRaycastSweep.magnitude, sphereCastRadius), LayerMask.GetMask("Gorilla Tag Collider"), QueryTriggerInteraction.Collide) && TryToTag(hitInfo, isBodyTag: false, out tryPlayer))
		{
			otherPlayer = tryPlayer;
		}
		if (Physics.SphereCast(headCollider.transform.position, sphereCastRadius, leftHeadRaycastSweep, out hitInfo, Mathf.Max(leftHeadRaycastSweep.magnitude, sphereCastRadius), LayerMask.GetMask("Gorilla Tag Collider"), QueryTriggerInteraction.Collide) && TryToTag(hitInfo, isBodyTag: false, out tryPlayer))
		{
			otherPlayer = tryPlayer;
		}
		if (Physics.SphereCast(lastRightHandPositionForTag, sphereCastRadius, rightRaycastSweep, out hitInfo, Mathf.Max(rightRaycastSweep.magnitude, sphereCastRadius), LayerMask.GetMask("Gorilla Tag Collider"), QueryTriggerInteraction.Collide) && TryToTag(hitInfo, isBodyTag: false, out tryPlayer))
		{
			otherPlayer = tryPlayer;
		}
		if (Physics.SphereCast(headCollider.transform.position, sphereCastRadius, rightHeadRaycastSweep, out hitInfo, Mathf.Max(rightHeadRaycastSweep.magnitude, sphereCastRadius), LayerMask.GetMask("Gorilla Tag Collider"), QueryTriggerInteraction.Collide) && TryToTag(hitInfo, isBodyTag: false, out tryPlayer))
		{
			otherPlayer = tryPlayer;
		}
		if (Physics.SphereCast(headCollider.transform.position, headCollider.radius * headCollider.transform.localScale.x, headRaycastSweep, out hitInfo, Mathf.Max(headRaycastSweep.magnitude, sphereCastRadius), LayerMask.GetMask("Gorilla Tag Collider"), QueryTriggerInteraction.Collide) && TryToTag(hitInfo, isBodyTag: true, out tryPlayer))
		{
			otherPlayer = tryPlayer;
		}
		topVector = lastBodyPositionForTag + bodyVector;
		bottomVector = lastBodyPositionForTag - bodyVector;
		if (Physics.CapsuleCast(topVector, bottomVector, bodyCollider.radius * 2f, bodyRaycastSweep, out hitInfo, Mathf.Max(bodyRaycastSweep.magnitude, sphereCastRadius), LayerMask.GetMask("Gorilla Tag Collider"), QueryTriggerInteraction.Collide) && TryToTag(hitInfo, isBodyTag: true, out tryPlayer))
		{
			otherPlayer = tryPlayer;
		}
		if (otherPlayer != null && GorillaGameManager.instance != null)
		{
			Debug.Log("tagging someone yeet");
			PhotonView.Get(GorillaGameManager.instance.GetComponent<GorillaGameManager>()).RPC("ReportTagRPC", RpcTarget.MasterClient, otherPlayer);
		}
		if (myVRRig == null && PhotonNetwork.InRoom)
		{
			foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
			{
				if (vrrig != null && !vrrig.isOfflineVRRig && vrrig.photonView != null && vrrig.photonView.IsMine)
				{
					myVRRig = vrrig;
				}
			}
		}
		if (GorillaLocomotion.Player.Instance.IsHandTouching(forLeftHand: true) && !leftHandTouching && Time.time > lastLeftTap + tapCoolDown && !GorillaLocomotion.Player.Instance.inOverlay)
		{
			StartVibration(forLeftController: true, tapHapticStrength, tapHapticDuration);
			tempInt = ((GorillaLocomotion.Player.Instance.leftHandSurfaceOverride != null) ? GorillaLocomotion.Player.Instance.leftHandSurfaceOverride.overrideIndex : GorillaLocomotion.Player.Instance.leftHandMaterialTouchIndex);
			if (PhotonNetwork.InRoom && myVRRig != null)
			{
				PhotonView.Get(myVRRig).RPC("PlayHandTap", RpcTarget.Others, tempInt, true, handTapVolume);
			}
			offlineVRRig.PlayHandTap(tempInt, isLeftHand: true, handTapVolume);
			lastLeftTap = Time.time;
		}
		if (GorillaLocomotion.Player.Instance.IsHandTouching(forLeftHand: false) && !rightHandTouching && Time.time > lastRightTap + tapCoolDown && !GorillaLocomotion.Player.Instance.inOverlay)
		{
			StartVibration(forLeftController: false, tapHapticStrength, tapHapticDuration);
			tempInt = ((GorillaLocomotion.Player.Instance.rightHandSurfaceOverride != null) ? GorillaLocomotion.Player.Instance.rightHandSurfaceOverride.overrideIndex : GorillaLocomotion.Player.Instance.rightHandMaterialTouchIndex);
			if (PhotonNetwork.InRoom && myVRRig != null)
			{
				PhotonView.Get(myVRRig).RPC("PlayHandTap", RpcTarget.Others, tempInt, false, handTapVolume);
			}
			offlineVRRig.PlayHandTap(tempInt, isLeftHand: false, handTapVolume);
			lastRightTap = Time.time;
		}
		CheckEndStatusEffect();
		leftHandTouching = GorillaLocomotion.Player.Instance.IsHandTouching(forLeftHand: true);
		rightHandTouching = GorillaLocomotion.Player.Instance.IsHandTouching(forLeftHand: false);
		lastLeftHandPositionForTag = leftHandTransform.position;
		lastRightHandPositionForTag = rightHandTransform.position;
		lastBodyPositionForTag = bodyCollider.transform.position;
		lastHeadPositionForTag = headCollider.transform.position;
	}

	public bool TryToTag(RaycastHit hitInfo, bool isBodyTag, out Photon.Realtime.Player taggedPlayer)
	{
		if (PhotonNetwork.InRoom && PhotonNetwork.LocalPlayer != hitInfo.collider.GetComponentInParent<PhotonView>().Owner && GorillaGameManager.instance != null && GorillaGameManager.instance.LocalCanTag(PhotonNetwork.LocalPlayer, hitInfo.collider.GetComponentInParent<PhotonView>().Owner) && Time.time > taggedTime + tagCooldown)
		{
			if (!isBodyTag)
			{
				StartVibration((leftHandTransform.position - hitInfo.collider.transform.position).magnitude < (rightHandTransform.position - hitInfo.collider.transform.position).magnitude, tagHapticStrength, tagHapticDuration);
			}
			else
			{
				StartVibration(forLeftController: true, tagHapticStrength, tagHapticDuration);
				StartVibration(forLeftController: false, tagHapticStrength, tagHapticDuration);
			}
			taggedPlayer = hitInfo.collider.GetComponentInParent<PhotonView>().Owner;
			return true;
		}
		taggedPlayer = null;
		return false;
	}

	public void StartVibration(bool forLeftController, float amplitude, float duration)
	{
		StartCoroutine(HapticPulses(forLeftController, amplitude, duration));
	}

	private IEnumerator HapticPulses(bool forLeftController, float amplitude, float duration)
	{
		float startTime = Time.time;
		uint channel = 0u;
		InputDevice device = ((!forLeftController) ? InputDevices.GetDeviceAtXRNode(XRNode.RightHand) : InputDevices.GetDeviceAtXRNode(XRNode.LeftHand));
		while (Time.time < startTime + duration)
		{
			device.SendHapticImpulse(channel, amplitude, hapticWaitSeconds);
			yield return new WaitForSeconds(hapticWaitSeconds * 0.9f);
		}
	}

	public void UpdateColor(float red, float green, float blue)
	{
		offlineVRRig.InitializeNoobMaterialLocal(red, green, blue);
		offlineVRRig.mainSkin.material = offlineVRRig.materialsToChangeTo[0];
	}

	private void OnTriggerEnter(Collider other)
	{
		if (PhotonNetwork.InRoom && other.gameObject.layer == 15)
		{
			other.gameObject.GetComponent<GorillaTriggerBox>().OnBoxTriggered();
		}
		if ((bool)other.GetComponentInChildren<GorillaTriggerBox>())
		{
			other.GetComponentInChildren<GorillaTriggerBox>().OnBoxTriggered();
		}
		else if ((bool)other.GetComponentInParent<GorillaTrigger>())
		{
			other.GetComponentInParent<GorillaTrigger>().OnTriggered();
		}
	}

	public void ShowCosmeticParticles(bool showParticles)
	{
		if (showParticles)
		{
			mainCamera.GetComponent<Camera>().cullingMask |= 1 << LayerMask.NameToLayer("GorillaCosmeticParticle");
			mirrorCamera.cullingMask |= 1 << LayerMask.NameToLayer("GorillaCosmeticParticle");
		}
		else
		{
			mainCamera.GetComponent<Camera>().cullingMask &= ~(1 << LayerMask.NameToLayer("GorillaCosmeticParticle"));
			mirrorCamera.cullingMask &= ~(1 << LayerMask.NameToLayer("GorillaCosmeticParticle"));
		}
	}

	public void ApplyStatusEffect(StatusEffect newStatus, float duration)
	{
		EndStatusEffect(currentStatus);
		currentStatus = newStatus;
		statusEndTime = Time.time + duration;
		switch (newStatus)
		{
			case StatusEffect.Frozen:
				GorillaLocomotion.Player.Instance.disableMovement = true;
				break;
			case StatusEffect.None:
			case StatusEffect.Slowed:
				break;
		}
	}

	public void CheckEndStatusEffect()
	{
		if (Time.time > statusEndTime)
		{
			EndStatusEffect(currentStatus);
		}
	}

	public void EndStatusEffect(StatusEffect effectToEnd)
	{
		switch (effectToEnd)
		{
			case StatusEffect.Frozen:
				GorillaLocomotion.Player.Instance.disableMovement = false;
				currentStatus = StatusEffect.None;
				break;
			case StatusEffect.Slowed:
				currentStatus = StatusEffect.None;
				break;
			case StatusEffect.None:
				break;
		}
	}
}
