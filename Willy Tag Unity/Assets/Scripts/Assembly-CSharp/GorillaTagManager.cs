using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GorillaTagManager : GorillaGameManager, IInRoomCallbacks, IMatchmakingCallbacks, IPunObservable
{
	public float tagDistanceThreshold = 8f;

	public float tagCoolDown = 5f;

	public int infectedModeThreshold = 4;

	public const byte ReportTagEvent = 1;

	public const byte ReportInfectionTagEvent = 2;

	public List<Player> currentInfected = new List<Player>();

	public int[] currentInfectedArray = new int[10];

	public Player currentIt;

	public Player lastInfectedPlayer;

	public double lastTag;

	public double timeInfectedGameEnded;

	public bool waitingToStartNextInfectionGame;

	public bool isCurrentlyTag;

	public bool isCasual;

	private int tempItInt;

	public object objRef;

	private int iterator1;

	private Player tempPlayer;

	private bool allInfected;

	private List<Player> returnPlayerList = new List<Player>();

	public override void Awake()
	{
		base.Awake();
		if (base.photonView.IsMine)
		{
			isCurrentlyTag = true;
		}
		Debug.Log(PhotonNetwork.CurrentRoom.ToStringFull());
	}

	public void UpdateState()
	{
		if (!base.photonView.IsMine)
		{
			return;
		}
		if (!IsGameModeTag())
		{
			if (currentInfected.Count > 0)
			{
				ClearInfectionState();
			}
			return;
		}
		if (isCurrentlyTag && currentIt == null)
		{
			int num = Random.Range(0, PhotonNetwork.PlayerList.Length);
			ChangeCurrentIt(PhotonNetwork.PlayerList[num]);
		}
		else if (isCurrentlyTag && PhotonNetwork.PlayerList.Length >= infectedModeThreshold)
		{
			SetisCurrentlyTag(newTagSetting: false);
			ClearInfectionState();
			int num2 = Random.Range(0, PhotonNetwork.PlayerList.Length);
			AddInfectedPlayer(PhotonNetwork.PlayerList[num2]);
			lastInfectedPlayer = PhotonNetwork.PlayerList[num2];
		}
		else if (!isCurrentlyTag && PhotonNetwork.PlayerList.Length < infectedModeThreshold)
		{
			ClearInfectionState();
			lastInfectedPlayer = null;
			SetisCurrentlyTag(newTagSetting: true);
			int num3 = Random.Range(0, PhotonNetwork.PlayerList.Length);
			ChangeCurrentIt(PhotonNetwork.PlayerList[num3]);
		}
		else if (!isCurrentlyTag && currentInfected.Count == 0)
		{
			int num4 = Random.Range(0, CurrentInfectionPlayers().Length);
			AddInfectedPlayer(CurrentInfectionPlayers()[num4]);
		}
		else if (!isCurrentlyTag)
		{
			UpdateInfectionState();
		}
		CopyInfectedListToArray();
	}

	public override void InfrequentUpdate()
	{
		if (base.photonView.IsMine)
		{
			base.InfrequentUpdate();
			UpdateState();
		}
	}

	public IEnumerator InfectionEnd()
	{
		while ((double)Time.time < timeInfectedGameEnded + (double)tagCoolDown)
		{
			yield return new WaitForSeconds(0.1f);
		}
		if (!isCurrentlyTag && waitingToStartNextInfectionGame)
		{
			Player[] array = CurrentInfectionPlayers();
			int num = Random.Range(0, array.Length);
			for (int i = 0; i < 10; i++)
			{
				if (array[num] != lastInfectedPlayer)
				{
					break;
				}
				num = Random.Range(0, array.Length);
			}
			ClearInfectionState();
			AddInfectedPlayer(array[num]);
			lastInfectedPlayer = array[num];
			lastTag = Time.time;
		}
		yield return null;
	}

	public void UpdateInfectionState()
	{
		if (!base.photonView.IsMine)
		{
			return;
		}
		allInfected = true;
		Player[] array = CurrentInfectionPlayers();
		foreach (Player player in array)
		{
			if (FindVRRigForPlayer(player) != null && !currentInfected.Contains(player))
			{
				allInfected = false;
			}
		}
		if (!isCurrentlyTag && !waitingToStartNextInfectionGame && allInfected)
		{
			EndInfectionGame();
		}
	}

	public void UpdateTagState()
	{
		if (!base.photonView.IsMine)
		{
			return;
		}
		Player[] playerList = PhotonNetwork.PlayerList;
		foreach (Player player in playerList)
		{
			PhotonView photonView = FindVRRigForPlayer(player);
			if (photonView != null && currentIt == player)
			{
				photonView.RPC("SetTaggedTime", player, null);
				photonView.RPC("PlayTagSound", RpcTarget.All, 0, 0.25f);
			}
		}
	}

	private void EndInfectionGame()
	{
		if (base.photonView.IsMine)
		{
			Player[] array = CurrentInfectionPlayers();
			foreach (Player player in array)
			{
				PhotonView obj = FindVRRigForPlayer(player);
				obj.RPC("SetTaggedTime", player, null);
				obj.RPC("PlayTagSound", player, 2, 0.25f);
			}
			waitingToStartNextInfectionGame = true;
			timeInfectedGameEnded = Time.time;
			StartCoroutine(InfectionEnd());
		}
	}

	private Player[] CurrentInfectionPlayers()
	{
		returnPlayerList.Clear();
		Player[] playerList = PhotonNetwork.PlayerList;
		foreach (Player player in playerList)
		{
			if (!player.CustomProperties.TryGetValue("didTutorial", out obj) || (bool)obj)
			{
				returnPlayerList.Add(player);
			}
		}
		return returnPlayerList.ToArray();
	}

	public override bool LocalCanTag(Player myPlayer, Player otherPlayer)
	{
		if (!IsGameModeTag())
		{
			return false;
		}
		if (isCurrentlyTag)
		{
			if (myPlayer == currentIt)
			{
				return myPlayer != otherPlayer;
			}
			return false;
		}
		if (currentInfected.Contains(myPlayer))
		{
			return !currentInfected.Contains(otherPlayer);
		}
		return false;
	}

	private float InterpolatedInfectedJumpMultiplier(int infectedCount)
	{
		if (PhotonNetwork.PlayerList.Length == 1)
		{
			return 1f;
		}
		return (fastJumpMultiplier - slowJumpMultiplier) / (float)(PhotonNetwork.PlayerList.Length - 1) * (float)(PhotonNetwork.PlayerList.Length - infectedCount) + slowJumpMultiplier;
	}

	private float InterpolatedInfectedJumpSpeed(int infectedCount)
	{
		if (PhotonNetwork.PlayerList.Length == 1)
		{
			return 1f;
		}
		return (fastJumpLimit - slowJumpLimit) / (float)(PhotonNetwork.PlayerList.Length - 1) * (float)(PhotonNetwork.PlayerList.Length - infectedCount) + slowJumpLimit;
	}

	private float InterpolatedNoobJumpMultiplier(int infectedCount)
	{
		if (PhotonNetwork.PlayerList.Length == 1)
		{
			return 1f;
		}
		return (fastJumpMultiplier - slowJumpMultiplier) / (float)(PhotonNetwork.PlayerList.Length - 1) * (float)(infectedCount - 1) * 0.9f + slowJumpMultiplier;
	}

	private float InterpolatedNoobJumpSpeed(int infectedCount)
	{
		if (PhotonNetwork.PlayerList.Length == 1)
		{
			return 1f;
		}
		return (fastJumpLimit - fastJumpLimit) / (float)(PhotonNetwork.PlayerList.Length - 1) * (float)(infectedCount - 1) * 0.9f + slowJumpLimit;
	}

	public override void ReportTag(Player taggedPlayer, Player taggingPlayer)
	{
		if (!base.photonView.IsMine || !IsGameModeTag())
		{
			return;
		}
		PhotonView photonView = FindVRRigForPlayer(taggingPlayer);
		PhotonView photonView2 = FindVRRigForPlayer(taggedPlayer);
		if (photonView == null || photonView2 == null)
		{
			return;
		}
		RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
		WebFlags flags = new WebFlags(1);
		raiseEventOptions.Flags = flags;
		byte b = 0;
		int num = 0;
		if (isCurrentlyTag)
		{
			if (taggingPlayer == currentIt && taggingPlayer != taggedPlayer && (double)Time.time > lastTag + (double)tagCoolDown)
			{
				ChangeCurrentIt(taggedPlayer);
				new ExitGames.Client.Photon.Hashtable();
				lastTag = Time.time;
				b = 1;
			}
		}
		else if (currentInfected.Contains(taggingPlayer) && !currentInfected.Contains(taggedPlayer) && (double)Time.time > lastTag + (double)tagCoolDown)
		{
			if ((photonView.transform.position - photonView2.transform.position).magnitude > tagDistanceThreshold)
			{
				suspiciousReason = "extremely far tag";
				suspiciousPlayerId = taggingPlayer.UserId;
				suspiciousPlayerName = taggingPlayer.NickName;
				sendReport = true;
			}
			num = currentInfected.Count;
			b = 2;
			AddInfectedPlayer(taggedPlayer);
		}
		switch (b)
		{
		case 1:
		{
			object[] eventContent2 = new object[2] { taggingPlayer.UserId, taggedPlayer.UserId };
			PhotonNetwork.RaiseEvent(1, eventContent2, raiseEventOptions, SendOptions.SendReliable);
			break;
		}
		case 2:
		{
			object[] eventContent = new object[3] { taggingPlayer.UserId, taggedPlayer.UserId, num };
			PhotonNetwork.RaiseEvent(2, eventContent, raiseEventOptions, SendOptions.SendReliable);
			break;
		}
		}
	}

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		_ = base.photonView.IsMine;
	}

	[PunRPC]
	public override void NewVRRig(Player player, int vrrigPhotonViewID, bool didntDoTutorial)
	{
		base.NewVRRig(player, vrrigPhotonViewID, didntDoTutorial);
		if (!base.photonView.IsMine || !IsGameModeTag())
		{
			return;
		}
		bool num = isCurrentlyTag;
		UpdateState();
		if (!num && !isCurrentlyTag)
		{
			if (!didntDoTutorial)
			{
				AddInfectedPlayer(player, withTagStop: false);
			}
			UpdateInfectionState();
		}
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		base.OnPlayerLeftRoom(otherPlayer);
		if (base.photonView.IsMine && IsGameModeTag())
		{
			if (isCurrentlyTag && otherPlayer == currentIt)
			{
				int num = Random.Range(0, PhotonNetwork.PlayerList.Length);
				ChangeCurrentIt(PhotonNetwork.PlayerList[num]);
			}
			else if (!isCurrentlyTag && PhotonNetwork.PlayerList.Length >= infectedModeThreshold)
			{
				while (currentInfected.Contains(otherPlayer))
				{
					currentInfected.Remove(otherPlayer);
				}
				CopyInfectedListToArray();
				UpdateInfectionState();
			}
			UpdateState();
		}
		playerVRRigDict.Remove(otherPlayer.ActorNumber);
		playerCosmeticsLookup.Remove(otherPlayer.UserId);
	}

	private void CopyInfectedListToArray()
	{
		for (iterator1 = 0; iterator1 < currentInfectedArray.Length; iterator1++)
		{
			currentInfectedArray[iterator1] = 0;
		}
		for (iterator1 = currentInfected.Count - 1; iterator1 >= 0; iterator1--)
		{
			if (currentInfected[iterator1] == null)
			{
				currentInfected.RemoveAt(iterator1);
			}
		}
		for (iterator1 = 0; iterator1 < currentInfected.Count; iterator1++)
		{
			currentInfectedArray[iterator1] = currentInfected[iterator1].ActorNumber;
		}
	}

	private void CopyInfectedArrayToList()
	{
		currentInfected.Clear();
		for (iterator1 = 0; iterator1 < currentInfectedArray.Length; iterator1++)
		{
			if (currentInfectedArray[iterator1] != 0)
			{
				tempPlayer = PhotonNetwork.LocalPlayer.Get(currentInfectedArray[iterator1]);
				if (tempPlayer != null)
				{
					currentInfected.Add(tempPlayer);
				}
			}
		}
	}

	public void ChangeCurrentIt(Player newCurrentIt)
	{
		lastTag = Time.time;
		currentIt = newCurrentIt;
		UpdateTagState();
	}

	public void SetisCurrentlyTag(bool newTagSetting)
	{
		if (newTagSetting)
		{
			isCurrentlyTag = true;
		}
		else
		{
			isCurrentlyTag = false;
		}
		Player[] playerList = PhotonNetwork.PlayerList;
		foreach (Player player in playerList)
		{
			if (FindVRRigForPlayer(player) != null)
			{
				FindVRRigForPlayer(player).RPC("PlayTagSound", player, 2, 0.25f);
			}
		}
	}

	public void AddInfectedPlayer(Player infectedPlayer)
	{
		if (base.photonView.IsMine)
		{
			currentInfected.Add(infectedPlayer);
			CopyInfectedListToArray();
			PhotonView obj = FindVRRigForPlayer(infectedPlayer);
			obj.RPC("SetTaggedTime", infectedPlayer, null);
			obj.RPC("PlayTagSound", RpcTarget.All, 0, 0.25f);
			UpdateInfectionState();
		}
	}

	public void AddInfectedPlayer(Player infectedPlayer, bool withTagStop)
	{
		if (base.photonView.IsMine)
		{
			currentInfected.Add(infectedPlayer);
			CopyInfectedListToArray();
			PhotonView photonView = FindVRRigForPlayer(infectedPlayer);
			if (!withTagStop)
			{
				photonView.RPC("SetJoinTaggedTime", infectedPlayer, null);
			}
			else
			{
				photonView.RPC("SetTaggedTime", infectedPlayer, null);
			}
			photonView.RPC("PlayTagSound", RpcTarget.All, 0, 0.25f);
			UpdateInfectionState();
		}
	}

	public void ClearInfectionState()
	{
		currentInfected.Clear();
		CopyInfectedListToArray();
		waitingToStartNextInfectionGame = false;
	}

	public bool IsGameModeTag()
	{
		if (currentRoom.CustomProperties.TryGetValue("gameMode", out objRef) && !objRef.ToString().Contains("CASUAL"))
		{
			return !objRef.ToString().Contains("city");
		}
		return false;
	}

	void IInRoomCallbacks.OnMasterClientSwitched(Player newMasterClient)
	{
		if (!base.photonView.IsMine)
		{
			return;
		}
		Player[] playerList = PhotonNetwork.PlayerList;
		foreach (Player player in playerList)
		{
			if (playerCosmeticsLookup.TryGetValue(player.UserId, out tempString) && tempString == "BANNED")
			{
				Debug.Log("this guy needs banned: " + player.NickName);
				PhotonNetwork.CloseConnection(player);
			}
		}
		CopyRoomDataToLocalData();
		UpdateState();
	}

	public void CopyRoomDataToLocalData()
	{
		lastTag = 0.0;
		timeInfectedGameEnded = 0.0;
		waitingToStartNextInfectionGame = false;
		if (IsGameModeTag())
		{
			if (isCurrentlyTag)
			{
				UpdateTagState();
			}
			else
			{
				UpdateInfectionState();
			}
		}
	}

	[PunRPC]
	public override void ReportTagRPC(Player taggedPlayer, PhotonMessageInfo info)
	{
		ReportTag(taggedPlayer, info.Sender);
	}

	void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext(isCurrentlyTag);
			stream.SendNext((currentIt != null) ? currentIt.ActorNumber : 0);
			stream.SendNext(currentInfectedArray[0]);
			stream.SendNext(currentInfectedArray[1]);
			stream.SendNext(currentInfectedArray[2]);
			stream.SendNext(currentInfectedArray[3]);
			stream.SendNext(currentInfectedArray[4]);
			stream.SendNext(currentInfectedArray[5]);
			stream.SendNext(currentInfectedArray[6]);
			stream.SendNext(currentInfectedArray[7]);
			stream.SendNext(currentInfectedArray[8]);
			stream.SendNext(currentInfectedArray[9]);
		}
		else
		{
			isCurrentlyTag = (bool)stream.ReceiveNext();
			tempItInt = (int)stream.ReceiveNext();
			currentIt = ((tempItInt != 0) ? PhotonNetwork.LocalPlayer.Get(tempItInt) : null);
			currentInfectedArray[0] = (int)stream.ReceiveNext();
			currentInfectedArray[1] = (int)stream.ReceiveNext();
			currentInfectedArray[2] = (int)stream.ReceiveNext();
			currentInfectedArray[3] = (int)stream.ReceiveNext();
			currentInfectedArray[4] = (int)stream.ReceiveNext();
			currentInfectedArray[5] = (int)stream.ReceiveNext();
			currentInfectedArray[6] = (int)stream.ReceiveNext();
			currentInfectedArray[7] = (int)stream.ReceiveNext();
			currentInfectedArray[8] = (int)stream.ReceiveNext();
			currentInfectedArray[9] = (int)stream.ReceiveNext();
			CopyInfectedArrayToList();
		}
	}

	public override string GameMode()
	{
		if (!IsGameModeTag())
		{
			return "CASUAL";
		}
		return "INFECTION";
	}

	public override int MyMatIndex(Player forPlayer)
	{
		if (!IsGameModeTag())
		{
			return 0;
		}
		if (isCurrentlyTag && forPlayer == currentIt)
		{
			return 1;
		}
		if (currentInfected.Contains(forPlayer))
		{
			return 2;
		}
		return 0;
	}

	public override float[] LocalPlayerSpeed()
	{
		if (IsGameModeTag())
		{
			if (isCurrentlyTag)
			{
				if (PhotonNetwork.LocalPlayer != currentIt)
				{
					return new float[2] { slowJumpLimit, slowJumpMultiplier };
				}
				return new float[2] { fastJumpLimit, fastJumpMultiplier };
			}
			if (!currentInfected.Contains(PhotonNetwork.LocalPlayer))
			{
				return new float[2]
				{
					InterpolatedNoobJumpSpeed(currentInfected.Count),
					InterpolatedNoobJumpMultiplier(currentInfected.Count)
				};
			}
			return new float[2]
			{
				InterpolatedInfectedJumpSpeed(currentInfected.Count),
				InterpolatedInfectedJumpMultiplier(currentInfected.Count)
			};
		}
		return new float[2] { slowJumpLimit, slowJumpMultiplier };
	}
}
