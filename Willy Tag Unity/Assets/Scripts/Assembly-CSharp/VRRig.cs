// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// VRRig
using System;
using GorillaLocomotion;
using GorillaNetworking;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class VRRig : MonoBehaviourPun, IPunObservable, IPunInstantiateMagicCallback
{
	public VRMap head;

	public VRMap rightHand;

	public VRMap leftHand;

	public VRMapThumb leftThumb;

	public VRMapIndex leftIndex;

	public VRMapMiddle leftMiddle;

	public VRMapThumb rightThumb;

	public VRMapIndex rightIndex;

	public VRMapMiddle rightMiddle;

	public bool isOfflineVRRig;

	public GameObject mainCamera;

	public Transform playerOffsetTransform;

	public int SDKIndex;

	public bool isMyPlayer;

	public AudioSource leftHandPlayer;

	public AudioSource rightHandPlayer;

	public AudioSource tagSound;

	[SerializeField]
	private float ratio;

	public Transform headConstraint;

	public Vector3 headBodyOffset;

	public GameObject headMesh;

	public Vector3 syncPos;

	public Quaternion syncRotation;

	public AudioClip[] clipToPlay;

	public AudioClip[] handTapSound;

	public int currentMatIndex;

	public int setMatIndex;

	private int tempMatIndex;

	public float lerpValueFingers;

	public float lerpValueBody;

	public GameObject backpack;

	public Transform leftHandTransform;

	public Transform rightHandTransform;

	public SkinnedMeshRenderer mainSkin;

	public Photon.Realtime.Player myPlayer;

	public GameObject spectatorSkin;

	public int handSync;

	public Material[] materialsToChangeTo;

	public float red;

	public float green;

	public float blue;

	public string playerName;

	public Text playerText;

	public bool showName;

	public GameObject[] cosmetics;

	public string concatStringOfCosmeticsAllowed = "";

	public bool initializedCosmetics;

	public string badge;

	public string face;

	public string hat;

	public string tryOnRoomBadge = "NOTHING";

	public string tryOnRoomFace = "NOTHING";

	public string tryOnRoomHat = "NOTHING";

	public bool inTryOnRoom;

	public bool muted;

	private float timeSpawned;

	public float doNotLerpConstant = 1f;

	public string tempString;

	private Photon.Realtime.Player tempPlayer;

	private float[] speedArray;

	public ParticleSystem lavaParticleSystem;

	public ParticleSystem rockParticleSystem;

	public ParticleSystem iceParticleSystem;

	public string tempItemName;

	public CosmeticsController.CosmeticItem tempItem;

	public string tempItemId;

	public int tempItemCost;

	public float bonkTime;

	public float bonkCooldown = 2f;

	public bool isQuitting;

	public GameObject huntComputer;

	public bool kickMe;

	private void Start()
	{
		Application.quitting += Quitting;
		concatStringOfCosmeticsAllowed = "";
		playerText.transform.parent.GetComponent<Canvas>().worldCamera = GorillaTagger.Instance.mainCamera.GetComponent<Camera>();
		materialsToChangeTo[0] = UnityEngine.Object.Instantiate(materialsToChangeTo[0]);
		if (setMatIndex > -1 && setMatIndex < materialsToChangeTo.Length)
		{
			mainSkin.material = materialsToChangeTo[setMatIndex];
		}
		if (!isOfflineVRRig && base.photonView.IsMine)
		{
			hat = PlayerPrefs.GetString("hatCosmetic", "NOTHING");
			face = PlayerPrefs.GetString("faceCosmetic", "NOTHING");
			badge = PlayerPrefs.GetString("badgeCosmetic", "NOTHING");
			red = PlayerPrefs.GetFloat("redValue");
			green = PlayerPrefs.GetFloat("greenValue");
			blue = PlayerPrefs.GetFloat("blueValue");
			InitializeNoobMaterialLocal(red, green, blue);
			playerOffsetTransform = GorillaLocomotion.Player.Instance.turnParent.transform;
			mainCamera = GorillaTagger.Instance.mainCamera;
			leftHand.overrideTarget = GorillaLocomotion.Player.Instance.leftHandFollower;
			rightHand.overrideTarget = GorillaLocomotion.Player.Instance.rightHandFollower;
			SDKIndex = -1;
			ratio = 1f;
			if ((bool)GetComponent<VoiceConnection>() && (bool)GetComponent<Recorder>())
			{
				GetComponent<VoiceConnection>().InitRecorder(GetComponent<Recorder>());
			}
			if (Application.platform == RuntimePlatform.Android && spectatorSkin != null)
			{
				UnityEngine.Object.Destroy(spectatorSkin);
			}
			if (XRSettings.loadedDeviceName == "OpenVR")
			{
				leftHand.trackingPositionOffset = new Vector3(0.02f, -0.06f, 0f);
				leftHand.trackingRotationOffset = new Vector3(-141f, 204f, -27f);
				rightHand.trackingPositionOffset = new Vector3(-0.02f, -0.06f, 0f);
				rightHand.trackingRotationOffset = new Vector3(-141f, 156f, 27f);
			}
		}
		else if (isOfflineVRRig)
		{
			hat = PlayerPrefs.GetString("hatCosmetic", "NOTHING");
			badge = PlayerPrefs.GetString("badgeCosmetic", "NOTHING");
			face = PlayerPrefs.GetString("faceCosmetic", "NOTHING");
			if (Application.platform == RuntimePlatform.Android && spectatorSkin != null)
			{
				UnityEngine.Object.Destroy(spectatorSkin);
			}
			if (XRSettings.loadedDeviceName == "OpenVR")
			{
				leftHand.trackingPositionOffset = new Vector3(0.02f, -0.06f, 0f);
				leftHand.trackingRotationOffset = new Vector3(-141f, 204f, -27f);
				rightHand.trackingPositionOffset = new Vector3(-0.02f, -0.06f, 0f);
				rightHand.trackingRotationOffset = new Vector3(-141f, 156f, 27f);
			}
		}
		else if (!base.photonView.IsMine && !isOfflineVRRig)
		{
			if (spectatorSkin != null)
			{
				UnityEngine.Object.Destroy(spectatorSkin);
			}
			head.syncPos = -headBodyOffset;
			if (UnityEngine.Object.FindObjectOfType<GorillaGameManager>() == null)
			{
				PhotonView.Get(this).RPC("RequestMaterialColor", PhotonView.Get(this).Owner, PhotonNetwork.LocalPlayer, true);
			}
			else
			{
				PhotonView.Get(this).RPC("RequestMaterialColor", PhotonView.Get(this).Owner, PhotonNetwork.LocalPlayer, false);
				base.photonView.RPC("RequestCosmetics", base.photonView.Owner);
			}
			if (GorillaGameManager.instance != null && GorillaGameManager.instance.gameObject.GetComponent<GorillaHuntManager>() != null && !GorillaLocomotion.Player.Instance.inOverlay)
			{
				huntComputer.SetActive(value: true);
			}
			else
			{
				huntComputer.SetActive(value: false);
			}
		}
		if (base.transform.parent == null)
		{
			base.transform.parent = GorillaParent.instance.transform;
		}
	}

	private void LateUpdate()
	{
		if (!isOfflineVRRig && (base.photonView == null || base.photonView.Owner == null || (base.photonView.Owner != null && PhotonNetwork.CurrentRoom.Players.TryGetValue(base.photonView.Owner.ActorNumber, out tempPlayer) && tempPlayer == null)))
		{
			GorillaParent.instance.vrrigs.Remove(this);
			if (base.photonView != null)
			{
				GorillaParent.instance.vrrigDict.Remove(base.photonView.Owner);
			}
			UnityEngine.Object.Destroy(base.gameObject);
		}
		if (GorillaGameManager.instance != null)
		{
			speedArray = GorillaGameManager.instance.LocalPlayerSpeed();
			GorillaLocomotion.Player.Instance.jumpMultiplier = speedArray[1];
			GorillaLocomotion.Player.Instance.maxJumpSpeed = speedArray[0];
		}
		else
		{
			GorillaLocomotion.Player.Instance.jumpMultiplier = 1.1f;
			GorillaLocomotion.Player.Instance.maxJumpSpeed = 6.5f;
		}
		if (isOfflineVRRig || base.photonView.IsMine)
		{
			base.transform.eulerAngles = new Vector3(0f, mainCamera.transform.rotation.eulerAngles.y, 0f);
			base.transform.position = mainCamera.transform.position + headConstraint.rotation * head.trackingPositionOffset + base.transform.rotation * headBodyOffset;
			head.MapMine(ratio, playerOffsetTransform);
			rightHand.MapMine(ratio, playerOffsetTransform);
			leftHand.MapMine(ratio, playerOffsetTransform);
			rightIndex.MapMyFinger(lerpValueFingers);
			rightMiddle.MapMyFinger(lerpValueFingers);
			rightThumb.MapMyFinger(lerpValueFingers);
			leftIndex.MapMyFinger(lerpValueFingers);
			leftMiddle.MapMyFinger(lerpValueFingers);
			leftThumb.MapMyFinger(lerpValueFingers);
		}
		else
		{
			if (kickMe && PhotonNetwork.IsMasterClient)
			{
				kickMe = false;
				PhotonNetwork.CloseConnection(base.photonView.Owner);
			}
			if (Time.time > timeSpawned + doNotLerpConstant)
			{
				base.transform.position = Vector3.Lerp(base.transform.position, syncPos, lerpValueBody * 0.66f);
			}
			else
			{
				base.transform.position = syncPos;
			}
			base.transform.rotation = Quaternion.Lerp(base.transform.rotation, syncRotation, lerpValueBody);
			head.syncPos = base.transform.rotation * -headBodyOffset;
			head.MapOther(lerpValueBody);
			rightHand.MapOther(lerpValueBody);
			leftHand.MapOther(lerpValueBody);
			rightIndex.MapOtherFinger((float)char.GetNumericValue(handSync.ToString().PadLeft(6)[5]) / 10f, lerpValueFingers);
			rightMiddle.MapOtherFinger((float)char.GetNumericValue(handSync.ToString().PadLeft(6)[4]) / 10f, lerpValueFingers);
			rightThumb.MapOtherFinger((float)char.GetNumericValue(handSync.ToString().PadLeft(6)[3]) / 10f, lerpValueFingers);
			leftIndex.MapOtherFinger((float)char.GetNumericValue(handSync.ToString().PadLeft(6)[2]) / 10f, lerpValueFingers);
			leftMiddle.MapOtherFinger((float)char.GetNumericValue(handSync.ToString().PadLeft(6)[1]) / 10f, lerpValueFingers);
			leftThumb.MapOtherFinger((float)char.GetNumericValue(handSync.ToString().PadLeft(6)[0]) / 10f, lerpValueFingers);
			if (!initializedCosmetics && GorillaGameManager.instance != null && GorillaGameManager.instance.playerCosmeticsLookup.TryGetValue(base.photonView.Owner.UserId, out tempString))
			{
				initializedCosmetics = true;
				concatStringOfCosmeticsAllowed = tempString;
				CheckForEarlyAccess();
				SetCosmeticsActive();
			}
		}
		if (!isOfflineVRRig)
		{
			if (PhotonNetwork.IsMasterClient && GorillaGameManager.instance == null)
			{
				PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("gameMode", out var _);
				PhotonNetwork.InstantiateRoomObject("GorillaPrefabs/GorillaTagManager", Vector3.zero, Quaternion.identity, 0);
			}
			tempMatIndex = ((GorillaGameManager.instance != null) ? GorillaGameManager.instance.MyMatIndex(base.photonView.Owner) : 0);
			if (setMatIndex != tempMatIndex)
			{
				setMatIndex = tempMatIndex;
				ChangeMaterialLocal(setMatIndex);
			}
			GetComponent<PhotonVoiceView>().SpeakerInUse.enabled = GorillaComputer.instance.voiceChatOn == "TRUE" && !muted;
		}
	}

	public void OnDestroy()
	{
		if (!isQuitting && base.photonView != null && base.photonView.IsMine && PhotonNetwork.InRoom)
		{
			PhotonNetwork.Instantiate("GorillaPrefabs/Gorilla Player Actual", base.transform.position, base.transform.rotation, 0);
		}
	}

	public void SetHeadBodyOffset()
	{
	}

	public void VRRigResize(float ratioVar)
	{
		ratio *= ratioVar;
	}

	public int ReturnHandPosition()
	{
		return Mathf.FloorToInt(rightIndex.calcT * 9.99f) + Mathf.FloorToInt(rightMiddle.calcT * 9.99f) * 10 + Mathf.FloorToInt(rightThumb.calcT * 9.99f) * 100 + Mathf.FloorToInt(leftIndex.calcT * 9.99f) * 1000 + Mathf.FloorToInt(leftMiddle.calcT * 9.99f) * 10000 + Mathf.FloorToInt(leftThumb.calcT * 9.99f) * 100000;
	}

	void IPunInstantiateMagicCallback.OnPhotonInstantiate(PhotonMessageInfo info)
	{
		timeSpawned = Time.time;
		base.transform.parent = GorillaParent.instance.GetComponent<GorillaParent>().vrrigParent.transform;
		GorillaParent.instance.vrrigs.Add(this);
		if (GorillaParent.instance.vrrigDict.ContainsKey(base.photonView.Owner))
		{
			GorillaParent.instance.vrrigDict[base.photonView.Owner] = this;
		}
		else
		{
			GorillaParent.instance.vrrigDict.Add(base.photonView.Owner, this);
		}
		if (GorillaGameManager.instance != null && GorillaGameManager.instance.GetComponent<PhotonView>().IsMine)
		{
			object value;
			bool didTutorial = base.photonView.Owner.CustomProperties.TryGetValue("didTutorial", out value) && !(bool)value;
			Debug.Log("guy who just joined didnt do the tutorial already: " + didTutorial);
			GorillaGameManager.instance.NewVRRig(base.photonView.Owner, base.photonView.ViewID, didTutorial);
		}
		Debug.Log(base.photonView.Owner.UserId, this);
	}

	void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (!isOfflineVRRig)
		{
			if (stream.IsWriting)
			{
				stream.SendNext(head.rigTarget.localRotation);
				stream.SendNext(rightHand.rigTarget.localPosition);
				stream.SendNext(rightHand.rigTarget.localRotation);
				stream.SendNext(leftHand.rigTarget.localPosition);
				stream.SendNext(leftHand.rigTarget.localRotation);
				stream.SendNext(base.transform.position);
				stream.SendNext(Mathf.RoundToInt(base.transform.rotation.eulerAngles.y));
				stream.SendNext(ReturnHandPosition());
			}
			else
			{
				head.syncRotation = (Quaternion)stream.ReceiveNext();
				rightHand.syncPos = (Vector3)stream.ReceiveNext();
				rightHand.syncRotation = (Quaternion)stream.ReceiveNext();
				leftHand.syncPos = (Vector3)stream.ReceiveNext();
				leftHand.syncRotation = (Quaternion)stream.ReceiveNext();
				syncPos = (Vector3)stream.ReceiveNext();
				syncRotation.eulerAngles = new Vector3(0f, (int)stream.ReceiveNext(), 0f);
				handSync = (int)stream.ReceiveNext();
			}
		}
	}

	[PunRPC]
	public void ChangeMaterial(int materialIndex, PhotonMessageInfo info)
	{
		if (info.Sender == PhotonNetwork.MasterClient)
		{
			ChangeMaterialLocal(materialIndex);
		}
	}

	[PunRPC]
	public void ChangeMaterialLocal(int materialIndex)
	{
		setMatIndex = materialIndex;
		if (setMatIndex > -1 && setMatIndex < materialsToChangeTo.Length)
		{
			mainSkin.material = materialsToChangeTo[setMatIndex];
		}
		if (lavaParticleSystem != null)
		{
			if (!isOfflineVRRig && materialIndex == 2 && lavaParticleSystem.isStopped)
			{
				lavaParticleSystem.Play();
			}
			else if (!isOfflineVRRig && lavaParticleSystem.isPlaying)
			{
				lavaParticleSystem.Stop();
			}
		}
		if (rockParticleSystem != null)
		{
			if (!isOfflineVRRig && materialIndex == 1 && rockParticleSystem.isStopped)
			{
				rockParticleSystem.Play();
			}
			else if (!isOfflineVRRig && rockParticleSystem.isPlaying)
			{
				rockParticleSystem.Stop();
			}
		}
		if (iceParticleSystem != null)
		{
			if (!isOfflineVRRig && materialIndex == 3 && rockParticleSystem.isStopped)
			{
				iceParticleSystem.Play();
			}
			else if (!isOfflineVRRig && iceParticleSystem.isPlaying)
			{
				iceParticleSystem.Stop();
			}
		}
	}

	[PunRPC]
	public void InitializeNoobMaterial(float red, float green, float blue, PhotonMessageInfo info)
	{
		if (info.Sender == base.photonView.Owner)
		{
			red = Mathf.Clamp(red, 0f, 1f);
			green = Mathf.Clamp(green, 0f, 1f);
			blue = Mathf.Clamp(blue, 0f, 1f);
			InitializeNoobMaterialLocal(red, green, blue);
		}
	}

	public void InitializeNoobMaterialLocal(float red, float green, float blue)
	{
		materialsToChangeTo[0].color = new Color(red, green, blue);
		if (base.photonView != null)
		{
			playerText.text = NormalizeName(doIt: true, base.photonView.Owner.NickName);
		}
		else if (showName)
		{
			playerText.text = PlayerPrefs.GetString("playerName");
		}
	}

	public string NormalizeName(bool doIt, string text)
	{
		if (doIt)
		{
			text = new string(Array.FindAll(text.ToCharArray(), (char c) => char.IsLetterOrDigit(c)));
			if (text.Length > 12)
			{
				text = text.Substring(0, 11);
			}
			text = text.ToUpper();
		}
		return text;
	}

	[PunRPC]
	public void SetJumpLimit(float maxJumpSpeed, PhotonMessageInfo info)
	{
		Debug.Log("setting jump limit to " + maxJumpSpeed);
		if (info.Sender.IsMasterClient && maxJumpSpeed >= GorillaGameManager.instance.slowJumpLimit && maxJumpSpeed <= GorillaGameManager.instance.fastJumpLimit)
		{
			SetJumpLimitLocal(maxJumpSpeed);
		}
	}

	public void SetJumpLimitLocal(float maxJumpSpeed)
	{
		GorillaLocomotion.Player.Instance.maxJumpSpeed = maxJumpSpeed;
	}

	[PunRPC]
	public void SetJumpMultiplier(float jumpMultiplier, PhotonMessageInfo info)
	{
		Debug.Log("setting jump multipleir to " + jumpMultiplier);
		if (info.Sender.IsMasterClient && jumpMultiplier >= GorillaGameManager.instance.slowJumpMultiplier && jumpMultiplier <= GorillaGameManager.instance.fastJumpMultiplier)
		{
			SetJumpMultiplierLocal(jumpMultiplier);
		}
	}

	public void SetJumpMultiplierLocal(float jumpMultiplier)
	{
		GorillaLocomotion.Player.Instance.jumpMultiplier = jumpMultiplier;
	}

	[PunRPC]
	public void SetTaggedTime(PhotonMessageInfo info)
	{
		if (GorillaGameManager.instance != null)
		{
			if (info.Sender == PhotonNetwork.MasterClient)
			{
				GorillaTagger.Instance.ApplyStatusEffect(GorillaTagger.StatusEffect.Frozen, GorillaTagger.Instance.tagCooldown);
				GorillaTagger.Instance.StartVibration(forLeftController: true, GorillaTagger.Instance.taggedHapticStrength, GorillaTagger.Instance.taggedHapticDuration);
				GorillaTagger.Instance.StartVibration(forLeftController: false, GorillaTagger.Instance.taggedHapticStrength, GorillaTagger.Instance.taggedHapticDuration);
			}
			else
			{
				GorillaGameManager.instance.suspiciousPlayerId = info.Sender.UserId;
				GorillaGameManager.instance.suspiciousPlayerName = info.Sender.NickName;
				GorillaGameManager.instance.suspiciousReason = "inappropriate tag data being sent";
				GorillaGameManager.instance.sendReport = true;
			}
		}
	}

	[PunRPC]
	public void SetSlowedTime(PhotonMessageInfo info)
	{
		if (!(GorillaGameManager.instance != null))
		{
			return;
		}
		if (info.Sender == PhotonNetwork.MasterClient)
		{
			if (GorillaTagger.Instance.currentStatus != GorillaTagger.StatusEffect.Slowed)
			{
				GorillaTagger.Instance.StartVibration(forLeftController: true, GorillaTagger.Instance.taggedHapticStrength, GorillaTagger.Instance.taggedHapticDuration);
				GorillaTagger.Instance.StartVibration(forLeftController: false, GorillaTagger.Instance.taggedHapticStrength, GorillaTagger.Instance.taggedHapticDuration);
			}
			GorillaTagger.Instance.ApplyStatusEffect(GorillaTagger.StatusEffect.Slowed, GorillaTagger.Instance.slowCooldown);
		}
		else
		{
			GorillaGameManager.instance.suspiciousPlayerId = info.Sender.UserId;
			GorillaGameManager.instance.suspiciousPlayerName = info.Sender.NickName;
			GorillaGameManager.instance.suspiciousReason = "inappropriate tag data being sent";
			GorillaGameManager.instance.sendReport = true;
		}
	}

	[PunRPC]
	public void SetJoinTaggedTime(PhotonMessageInfo info)
	{
		if (info.Sender == PhotonNetwork.MasterClient)
		{
			GorillaTagger.Instance.StartVibration(forLeftController: true, GorillaTagger.Instance.taggedHapticStrength, GorillaTagger.Instance.taggedHapticDuration);
			GorillaTagger.Instance.StartVibration(forLeftController: false, GorillaTagger.Instance.taggedHapticStrength, GorillaTagger.Instance.taggedHapticDuration);
			return;
		}
		GorillaGameManager.instance.suspiciousPlayerId = info.Sender.UserId;
		GorillaGameManager.instance.suspiciousPlayerName = info.Sender.NickName;
		GorillaGameManager.instance.suspiciousReason = "inappropriate tag data being sent";
		GorillaGameManager.instance.sendReport = true;
	}

	[PunRPC]
	public void RequestMaterialColor(Photon.Realtime.Player askingPlayer, bool noneBool)
	{
		PhotonView.Get(this).RPC("InitializeNoobMaterial", askingPlayer, materialsToChangeTo[0].color.r, materialsToChangeTo[0].color.g, materialsToChangeTo[0].color.b);
	}

	[PunRPC]
	public void RequestCosmetics(PhotonMessageInfo info)
	{
		if (base.photonView.IsMine && CosmeticsController.instance != null)
		{
			base.photonView.RPC("UpdateCosmeticsWithTryon", info.Sender, CosmeticsController.instance.currentWornSet.badge.displayName, CosmeticsController.instance.currentWornSet.face.displayName, CosmeticsController.instance.currentWornSet.hat.displayName, CosmeticsController.instance.tryOnSet.badge.displayName, CosmeticsController.instance.tryOnSet.face.displayName, CosmeticsController.instance.tryOnSet.hat.displayName);
		}
	}

	[PunRPC]
	public void PlayTagSound(int soundIndex, float soundVolume)
	{
		tagSound.volume = Mathf.Max(0.25f, soundVolume);
		tagSound.PlayOneShot(clipToPlay[soundIndex]);
	}

	public void Bonk(int soundIndex, float bonkPercent, PhotonMessageInfo info)
	{
		if (bonkTime + bonkCooldown < Time.time)
		{
			bonkTime = Time.time;
			tagSound.volume = bonkPercent * 0.25f;
			tagSound.PlayOneShot(clipToPlay[soundIndex]);
			if (base.photonView.IsMine)
			{
				GorillaTagger.Instance.StartVibration(forLeftController: true, GorillaTagger.Instance.taggedHapticStrength, GorillaTagger.Instance.taggedHapticDuration);
				GorillaTagger.Instance.StartVibration(forLeftController: false, GorillaTagger.Instance.taggedHapticStrength, GorillaTagger.Instance.taggedHapticDuration);
			}
		}
	}

	[PunRPC]
	public void PlayHandTap(int soundIndex, bool isLeftHand, float tapVolume)
	{
		if (soundIndex > -1 && soundIndex < GorillaLocomotion.Player.Instance.materialData.Count)
		{
			if (isLeftHand)
			{
				leftHandPlayer.volume = tapVolume;
				leftHandPlayer.clip = (GorillaLocomotion.Player.Instance.materialData[soundIndex].overrideAudio ? GorillaLocomotion.Player.Instance.materialData[soundIndex].audio : GorillaLocomotion.Player.Instance.materialData[0].audio);
				leftHandPlayer.PlayOneShot(leftHandPlayer.clip);
			}
			else
			{
				rightHandPlayer.volume = tapVolume;
				rightHandPlayer.clip = (GorillaLocomotion.Player.Instance.materialData[soundIndex].overrideAudio ? GorillaLocomotion.Player.Instance.materialData[soundIndex].audio : GorillaLocomotion.Player.Instance.materialData[0].audio);
				rightHandPlayer.PlayOneShot(rightHandPlayer.clip);
			}
		}
	}

	[PunRPC]
	public void UpdateCosmetics(string newBadge, string newFace, string newHat, PhotonMessageInfo info)
	{
		if (info.Sender == base.photonView.Owner)
		{
			LocalUpdateCosmetics(newBadge, newFace, newHat);
		}
	}

	[PunRPC]
	public void UpdateCosmeticsWithTryon(string newBadge, string newFace, string newHat, string tryOnBadge, string tryOnFace, string tryOnHat, PhotonMessageInfo info)
	{
		if (info.Sender == base.photonView.Owner)
		{
			LocalUpdateCosmeticsWithTryon(newBadge, newFace, newHat, tryOnBadge, tryOnFace, tryOnHat);
		}
	}

	public void UpdateAllowedCosmetics()
	{
		if (GorillaGameManager.instance != null && GorillaGameManager.instance.playerCosmeticsLookup.TryGetValue(base.photonView.Owner.UserId, out tempString))
		{
			concatStringOfCosmeticsAllowed = tempString;
			CheckForEarlyAccess();
		}
	}

	public void LocalUpdateCosmetics(string newBadge, string newFace, string newHat)
	{
		badge = newBadge;
		face = newFace;
		hat = newHat;
		if (initializedCosmetics)
		{
			SetCosmeticsActive();
		}
	}

	public void LocalUpdateCosmeticsWithTryon(string newBadge, string newFace, string newHat, string tryOnBadge, string tryOnFace, string tryOnHat)
	{
		badge = newBadge;
		face = newFace;
		hat = newHat;
		tryOnRoomBadge = tryOnBadge;
		tryOnRoomFace = tryOnFace;
		tryOnRoomHat = tryOnHat;
		if (initializedCosmetics)
		{
			SetCosmeticsActive();
		}
	}

	private void CheckForEarlyAccess()
	{
		if (concatStringOfCosmeticsAllowed != null && concatStringOfCosmeticsAllowed.Contains("Early Access Supporter Pack"))
		{
			concatStringOfCosmeticsAllowed += "LBAAE.LFAAM.LFAAN.LHAAA.LHAAK.LHAAL.LHAAM.LHAAN.LHAAO.LHAAP.LHABA.LHABB.";
		}
		initializedCosmetics = true;
	}

	public void SetCosmeticsActive()
	{
		if (CosmeticsController.instance == null)
		{
			return;
		}
		if (isOfflineVRRig || base.photonView.IsMine)
		{
			concatStringOfCosmeticsAllowed = CosmeticsController.instance.concatStringCosmeticsAllowed;
		}
		GameObject[] array = cosmetics;
		foreach (GameObject gameObject in array)
		{
			if (!inTryOnRoom)
			{
				if (concatStringOfCosmeticsAllowed != null && concatStringOfCosmeticsAllowed.Contains(CosmeticsController.instance.GetItemNameFromDisplayName(gameObject.name)))
				{
					gameObject.SetActive(gameObject.name == badge || gameObject.name == face || gameObject.name == hat);
				}
				else
				{
					gameObject.SetActive(value: false);
				}
			}
			else if ((gameObject.name == tryOnRoomBadge || gameObject.name == tryOnRoomFace || gameObject.name == tryOnRoomHat) && CosmeticsController.instance.GetItemFromDict(CosmeticsController.instance.GetItemNameFromDisplayName(gameObject.name)).canTryOn)
			{
				gameObject.SetActive(value: true);
			}
			else if (concatStringOfCosmeticsAllowed != null && concatStringOfCosmeticsAllowed.Contains(CosmeticsController.instance.GetItemNameFromDisplayName(gameObject.name)))
			{
				gameObject.SetActive((gameObject.name == badge && tryOnRoomBadge == "NOTHING") || (gameObject.name == face && tryOnRoomFace == "NOTHING") || (gameObject.name == hat && tryOnRoomHat == "NOTHING"));
			}
			else
			{
				gameObject.SetActive(value: false);
			}
		}
	}

	public void GetUserCosmeticsAllowed()
	{
		if (!(CosmeticsController.instance != null))
		{
			return;
		}
		PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), delegate (GetUserInventoryResult result)
		{
			foreach (ItemInstance item in result.Inventory)
			{
				if (item.CatalogVersion == CosmeticsController.instance.catalog)
				{
					concatStringOfCosmeticsAllowed += item.ItemId;
				}
			}
			Debug.Log("successful result. allowed cosmetics are: " + concatStringOfCosmeticsAllowed);
			CheckForEarlyAccess();
			SetCosmeticsActive();
		}, delegate (PlayFabError error)
		{
			Debug.Log("Got error retrieving user data:");
			Debug.Log(error.GenerateErrorReport());
			initializedCosmetics = true;
			SetCosmeticsActive();
		});
	}

	private void Quitting()
	{
		isQuitting = true;
	}
}
