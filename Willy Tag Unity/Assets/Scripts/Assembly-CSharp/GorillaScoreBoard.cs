using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class GorillaScoreBoard : MonoBehaviourPunCallbacks, IInRoomCallbacks, IOnEventCallback
{
	public GameObject scoreBoardLinePrefab;

	public int startingYValue;

	public int lineHeight;

	public GorillaGameManager gameManager;

	public string gameType;

	public bool includeMMR;

	public bool isActive;

	public List<GorillaPlayerScoreboardLine> lines;

	public Text boardText;

	public Text buttonText;

	private int i;

	public void Awake()
	{
		PhotonNetwork.AddCallbackTarget(this);
		if (PhotonNetwork.InRoom && GorillaGameManager.instance != null)
		{
			boardText.text = GetBeginningString();
		}
	}

	public string GetBeginningString()
	{
		if (GorillaGameManager.instance != null)
		{
			return "ROOM ID: " + ((!PhotonNetwork.CurrentRoom.IsVisible) ? "-PRIVATE- GAME MODE: " : (PhotonNetwork.CurrentRoom.Name + "    GAME MODE: ")) + GorillaGameManager.instance.GameMode() + "\n   PLAYER      COLOR   MUTE   REPORT";
		}
		return "ROOM ID: " + ((!PhotonNetwork.CurrentRoom.IsVisible) ? "-PRIVATE-" : PhotonNetwork.CurrentRoom.Name) + "\n   PLAYER      COLOR   MUTE   REPORT";
	}

	public void Update()
	{
		for (i = lines.Count - 1; i > -1; i--)
		{
			if (lines[i] == null)
			{
				lines.RemoveAt(i);
			}
		}
		if (PhotonNetwork.CurrentRoom == null || lines.Count == GorillaParent.instance.vrrigs.Count)
		{
			return;
		}
		foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
		{
			Player player = FindPlayerforVRRig(vrrig);
			if (player == null)
			{
				continue;
			}
			bool flag = false;
			foreach (GorillaPlayerScoreboardLine line in lines)
			{
				if (line.playerActorNumber == player.ActorNumber)
				{
					flag = true;
				}
			}
			if (!flag)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(scoreBoardLinePrefab, base.transform);
				lines.Add(gameObject.GetComponent<GorillaPlayerScoreboardLine>());
				if (includeMMR)
				{
					gameObject.GetComponent<GorillaPlayerScoreboardLine>().playerMMR.gameObject.SetActive(value: true);
				}
				gameObject.GetComponent<GorillaPlayerScoreboardLine>().playerActorNumber = player.ActorNumber;
				gameObject.GetComponent<GorillaPlayerScoreboardLine>().linePlayer = player;
				gameObject.GetComponent<GorillaPlayerScoreboardLine>().playerVRRig = vrrig;
				gameObject.GetComponent<GorillaPlayerScoreboardLine>().playerNameValue = player.NickName;
				RedrawPlayerLines();
			}
		}
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		foreach (GorillaPlayerScoreboardLine line in lines)
		{
			if (line.playerActorNumber == otherPlayer.ActorNumber)
			{
				Debug.Log("destroying this", line.gameObject);
				line.enabled = false;
				lines.Remove(line);
				UnityEngine.Object.Destroy(line.gameObject);
				break;
			}
		}
		RedrawPlayerLines();
	}

	public void RedrawPlayerLines()
	{
		lines.Sort((GorillaPlayerScoreboardLine line1, GorillaPlayerScoreboardLine line2) => line1.playerActorNumber.CompareTo(line2.playerActorNumber));
		boardText.text = GetBeginningString();
		buttonText.text = "";
		for (int i = 0; i < lines.Count; i++)
		{
			lines[i].gameObject.GetComponent<RectTransform>().localPosition = new Vector3(0f, startingYValue - lineHeight * i, 0f);
			Text text = boardText;
			text.text = text.text + "\n " + NormalizeName(doIt: true, lines[i].linePlayer.NickName);
			if (lines[i].linePlayer != PhotonNetwork.LocalPlayer)
			{
				if (lines[i].reportButton.isActiveAndEnabled)
				{
					buttonText.text += "MUTE                                REPORT\n";
				}
				else
				{
					buttonText.text += "MUTE                HATE SPEECH    TOXICITY      CHEATING      CANCEL\n";
				}
			}
			else
			{
				buttonText.text += "\n";
			}
		}
	}

	public Player FindPlayerforVRRig(VRRig vRRig)
	{
		if (vRRig != null && vRRig.photonView != null && vRRig.photonView.Owner != null)
		{
			return vRRig.photonView.Owner;
		}
		return null;
	}

	void IOnEventCallback.OnEvent(EventData photonEvent)
	{
		if (photonEvent.Code == 1 || photonEvent.Code == 2 || photonEvent.Code == 3)
		{
			object[] array = (object[])photonEvent.CustomData;
			StartCoroutine(RefreshData(GetActorIDFromUserID((string)array[0]), GetActorIDFromUserID((string)array[1])));
		}
	}

	public IEnumerator RefreshData(int actorNumber1, int actorNumber2)
	{
		yield return new WaitForSeconds(1f);
		foreach (GorillaPlayerScoreboardLine line in lines)
		{
			if (line.playerActorNumber != actorNumber1)
			{
				_ = line.playerActorNumber;
				_ = actorNumber2;
			}
		}
	}

	private int GetActorIDFromUserID(string userID)
	{
		Player[] playerList = PhotonNetwork.PlayerList;
		foreach (Player player in playerList)
		{
			if (player.UserId == userID)
			{
				return player.ActorNumber;
			}
		}
		return -1;
	}

	public string NormalizeName(bool doIt, string text)
	{
		if (doIt)
		{
			text = new string(Array.FindAll(text.ToCharArray(), (char c) => char.IsLetterOrDigit(c)));
			if (text.Length > 12)
			{
				text = text.Substring(0, 10);
			}
			text = text.ToUpper();
		}
		return text;
	}
}
