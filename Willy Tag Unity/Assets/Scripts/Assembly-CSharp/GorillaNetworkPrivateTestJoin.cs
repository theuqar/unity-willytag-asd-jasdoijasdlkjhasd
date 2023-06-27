using System.Collections;
using GorillaLocomotion;
using GorillaNetworking;
using Photon.Pun;
using UnityEngine;

public class GorillaNetworkPrivateTestJoin : GorillaTriggerBox
{
	public static volatile GorillaNetworkPrivateTestJoin instance;

	public GameObject[] makeSureThisIsDisabled;

	public GameObject[] makeSureThisIsEnabled;

	public string gameModeName;

	public PhotonNetworkController photonNetworkController;

	public string componentTypeToAdd;

	public GameObject componentTarget;

	public GorillaLevelScreen[] joinScreens;

	public GorillaLevelScreen[] leaveScreens;

	private Transform tosPition;

	private Transform othsTosPosition;

	private PhotonView fotVew;

	private int count;

	private bool waiting;

	public void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			Object.Destroy(base.gameObject);
		}
		count = 0;
	}

	public void LateUpdate()
	{
		try
		{
			if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.IsVisible)
			{
				if ((!Player.Instance.GetComponent<Rigidbody>().useGravity || Player.Instance.GetComponent<Rigidbody>().isKinematic) && !waiting && !GorillaGameManager.instance.reportedPlayers.Contains(PhotonNetwork.LocalPlayer.UserId))
				{
					StartCoroutine(GracePeriod());
				}
				if ((Player.Instance.jumpMultiplier > GorillaGameManager.instance.fastJumpMultiplier * 2f || Player.Instance.maxJumpSpeed > GorillaGameManager.instance.fastJumpLimit * 2f) && !waiting && !GorillaGameManager.instance.reportedPlayers.Contains(PhotonNetwork.LocalPlayer.UserId))
				{
					StartCoroutine(GracePeriod());
				}
			}
			if (PhotonNetwork.InRoom && GorillaTagger.Instance.otherPlayer != null && GorillaGameManager.instance != null)
			{
				fotVew = GorillaGameManager.instance.FindVRRigForPlayer(GorillaTagger.Instance.otherPlayer);
				if (fotVew != null && GorillaTagger.Instance.myVRRig != null && (fotVew.transform.position - GorillaTagger.Instance.myVRRig.transform.position).magnitude > 8f)
				{
					count++;
					if (count >= 3 && !waiting && !GorillaGameManager.instance.reportedPlayers.Contains(PhotonNetwork.LocalPlayer.UserId))
					{
						StartCoroutine(GracePeriod());
					}
				}
			}
			else
			{
				count = 0;
			}
		}
		catch
		{
		}
	}

	private IEnumerator GracePeriod()
	{
		waiting = true;
		yield return new WaitForSeconds(30f);
		try
		{
			if (PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.IsVisible)
			{
				if (!Player.Instance.GetComponent<Rigidbody>().useGravity || Player.Instance.GetComponent<Rigidbody>().isKinematic)
				{
					GorillaGameManager.instance.suspiciousPlayerId = PhotonNetwork.LocalPlayer.UserId;
					GorillaGameManager.instance.suspiciousPlayerName = PhotonNetwork.LocalPlayer.NickName;
					GorillaGameManager.instance.suspiciousReason = "gorvity bisdabled";
					GorillaGameManager.instance.sendReport = true;
				}
				if (Player.Instance.jumpMultiplier > GorillaGameManager.instance.fastJumpMultiplier * 2f || Player.Instance.maxJumpSpeed > GorillaGameManager.instance.fastJumpLimit * 2f)
				{
					GorillaGameManager.instance.suspiciousPlayerId = PhotonNetwork.LocalPlayer.UserId;
					GorillaGameManager.instance.suspiciousPlayerName = PhotonNetwork.LocalPlayer.NickName;
					GorillaGameManager.instance.suspiciousReason = "jimp 2mcuh." + Player.Instance.jumpMultiplier + "." + Player.Instance.maxJumpSpeed + ".";
					GorillaGameManager.instance.sendReport = true;
				}
			}
			if (PhotonNetwork.InRoom && GorillaTagger.Instance.otherPlayer != null && GorillaGameManager.instance != null)
			{
				fotVew = GorillaGameManager.instance.FindVRRigForPlayer(GorillaTagger.Instance.otherPlayer);
				if (fotVew != null && GorillaTagger.Instance.myVRRig != null && (fotVew.transform.position - GorillaTagger.Instance.myVRRig.transform.position).magnitude > 8f)
				{
					count++;
					if (count >= 3)
					{
						GorillaGameManager.instance.suspiciousPlayerId = PhotonNetwork.LocalPlayer.UserId;
						GorillaGameManager.instance.suspiciousPlayerName = PhotonNetwork.LocalPlayer.NickName;
						GorillaGameManager.instance.suspiciousReason = "tee hee";
						GorillaGameManager.instance.sendReport = true;
					}
				}
			}
			else
			{
				count = 0;
			}
			waiting = false;
		}
		catch
		{
		}
	}
}
