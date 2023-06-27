using System;
using ExitGames.Client.Photon;
using GorillaNetworking;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.PUN;
using Photon.Voice.Unity;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;

public class GorillaPlayerScoreboardLine : MonoBehaviourPunCallbacks, IInRoomCallbacks
{
	public Text playerName;

	public Text playerLevel;

	public Text playerMMR;

	public Image playerSwatch;

	public Texture infectedTexture;

	public Player linePlayer;

	public VRRig playerVRRig;

	public int currentMatIndex;

	public string playerLevelValue;

	public string playerMMRValue;

	public string playerNameValue;

	public int playerActorNumber;

	public bool initialized;

	public GorillaPlayerLineButton muteButton;

	public GorillaPlayerLineButton reportButton;

	public GameObject speakerIcon;

	public bool canPressNextReportButton = true;

	public Text[] texts;

	public SpriteRenderer[] sprites;

	public MeshRenderer[] meshes;

	public Image[] images;


	public void UpdateLevel()
	{
	}

	public void HideShowLine(bool active)
	{
		if (!(playerVRRig != null))
		{
			return;
		}
		Text[] array = texts;
		foreach (Text text in array)
		{
			if (text.enabled != active)
			{
				text.enabled = active;
			}
		}
		SpriteRenderer[] array2 = sprites;
		foreach (SpriteRenderer spriteRenderer in array2)
		{
			if (spriteRenderer.enabled != active)
			{
				spriteRenderer.enabled = active;
			}
		}
		MeshRenderer[] array3 = meshes;
		foreach (MeshRenderer meshRenderer in array3)
		{
			if (meshRenderer.enabled != active)
			{
				meshRenderer.enabled = active;
			}
		}
		Image[] array4 = images;
		foreach (Image image in array4)
		{
			if (image.enabled != active)
			{
				image.enabled = active;
			}
		}
	}

	public void Update()
	{
		GameObject networkVoiceObject = GameObject.Find("NetworkVoice");
		if (playerVRRig != null)
		{
			if (!initialized && linePlayer != null)
			{
				initialized = true;
				if (linePlayer != PhotonNetwork.LocalPlayer)
				{
					int @int = PlayerPrefs.GetInt(linePlayer.UserId, 0);
					PlayerPrefs.SetInt(linePlayer.UserId, @int);
					muteButton.isOn = ((@int != 0) ? true : false);
					muteButton.UpdateColor();
					playerVRRig.muted = ((@int != 0) ? true : false);
				}
				else
				{
					muteButton.gameObject.SetActive(value: false);
					reportButton.gameObject.SetActive(value: false);
				}
			}
			if (linePlayer != null)
			{
				if (playerVRRig.setMatIndex != currentMatIndex && playerVRRig.setMatIndex != 0 && playerVRRig.setMatIndex > -1 && playerVRRig.setMatIndex < playerVRRig.materialsToChangeTo.Length)
				{
					playerSwatch.material = playerVRRig.materialsToChangeTo[playerVRRig.setMatIndex];
					currentMatIndex = playerVRRig.setMatIndex;
				}
				if (playerVRRig.setMatIndex == 0 && playerSwatch.material != null)
				{
					playerSwatch.material = null;
					currentMatIndex = 0;
				}
				if (playerName.text != linePlayer.NickName)
				{
					playerName.text = NormalizeName(doIt: true, linePlayer.NickName);
				}
				if (playerMMRValue != playerMMR.text)
				{
					playerMMR.text = playerMMRValue;
				}
				if (playerLevelValue != playerLevel.text)
				{
					playerLevel.text = playerLevelValue;
				}
				if (playerSwatch.color != playerVRRig.materialsToChangeTo[0].color)
				{
					playerSwatch.color = playerVRRig.materialsToChangeTo[0].color;
				}
				if (linePlayer != PhotonNetwork.LocalPlayer.Get(playerActorNumber))
				{
					linePlayer = PhotonNetwork.LocalPlayer.Get(playerActorNumber);
					playerSwatch.color = playerVRRig.materialsToChangeTo[0].color;
					playerSwatch.material = playerVRRig.materialsToChangeTo[playerVRRig.setMatIndex];
				}
				if (GetComponentInParent<GorillaScoreBoard>().includeMMR && !playerMMR.gameObject.activeSelf)
				{
					playerMMR.gameObject.SetActive(value: true);
				}
				if ((playerVRRig.photonView.IsMine && networkVoiceObject.GetComponent<Recorder>().IsCurrentlyTransmitting))
				{
					speakerIcon.SetActive(value: true);
				}
				else
				{
					speakerIcon.SetActive(value: false);
				}
			}
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public void PressButton(bool isOn, GorillaPlayerLineButton.ButtonType buttonType)
	{
		switch (buttonType)
		{
			case GorillaPlayerLineButton.ButtonType.Mute:
				if (linePlayer != null && playerVRRig != null)
				{
					int num = (isOn ? 1 : 0);
					PlayerPrefs.SetInt(linePlayer.UserId, num);
					playerVRRig.muted = ((num != 0) ? true : false);
					PlayerPrefs.Save();
					muteButton.UpdateColor();
				}
				break;
			case GorillaPlayerLineButton.ButtonType.Report:
				SetReportState(reportState: true, buttonType);
				break;
			default:
				SetReportState(reportState: false, buttonType);
				break;
		}
	}

	public void SetReportState(bool reportState, GorillaPlayerLineButton.ButtonType buttonType)
	{
		canPressNextReportButton = buttonType != GorillaPlayerLineButton.ButtonType.Toxicity && buttonType != GorillaPlayerLineButton.ButtonType.Report;
		if (reportState)
		{
			GorillaPlayerLineButton[] componentsInChildren = GetComponentsInChildren<GorillaPlayerLineButton>(includeInactive: true);
			foreach (GorillaPlayerLineButton gorillaPlayerLineButton in componentsInChildren)
			{
				gorillaPlayerLineButton.gameObject.SetActive(gorillaPlayerLineButton.buttonType != GorillaPlayerLineButton.ButtonType.Report);
			}
		}
		else
		{
			GorillaPlayerLineButton[] componentsInChildren = GetComponentsInChildren<GorillaPlayerLineButton>(includeInactive: true);
			foreach (GorillaPlayerLineButton gorillaPlayerLineButton2 in componentsInChildren)
			{
				gorillaPlayerLineButton2.gameObject.SetActive(gorillaPlayerLineButton2.buttonType == GorillaPlayerLineButton.ButtonType.Report || gorillaPlayerLineButton2.buttonType == GorillaPlayerLineButton.ButtonType.Mute);
			}
			if (linePlayer != null && playerVRRig != null && buttonType != GorillaPlayerLineButton.ButtonType.Cancel)
			{
				ReportPlayer(linePlayer.UserId, buttonType, linePlayer.NickName);
				reportButton.isOn = true;
				reportButton.UpdateColor();
			}
		}
		base.transform.parent.GetComponent<GorillaScoreBoard>().RedrawPlayerLines();
	}

	public void GetUserLevel(string myPlayFabeId)
	{
		PlayFabClientAPI.GetUserReadOnlyData(new GetUserDataRequest
		{
			PlayFabId = myPlayFabeId,
			Keys = null
		}, delegate (GetUserDataResult result)
		{
			if (result.Data == null || !result.Data.ContainsKey("PlayerLevel"))
			{
				playerLevelValue = "1";
			}
			else
			{
				playerLevelValue = result.Data["PlayerLevel"].Value;
			}
			if (result.Data == null || !result.Data.ContainsKey("Player1v1MMR"))
			{
				playerMMRValue = "-1";
			}
			else
			{
				playerMMRValue = result.Data["Player1v1MMR"].Value;
			}
			playerLevel.text = playerLevelValue;
			playerMMR.text = playerMMRValue;
		}, delegate
		{
		});
	}

	public void ReportPlayer(string PlayerID, GorillaPlayerLineButton.ButtonType buttonType, string OtherPlayerNickName)
	{
		RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
		WebFlags flags = new WebFlags(1);
		raiseEventOptions.Flags = flags;
		byte eventCode = 50;
		object[] eventContent = new object[5]
		{
			PlayerID,
			buttonType,
			OtherPlayerNickName,
			PhotonNetwork.LocalPlayer.NickName,
			PhotonNetwork.CurrentRoom.IsVisible
		};
		PhotonNetwork.RaiseEvent(eventCode, eventContent, raiseEventOptions, SendOptions.SendReliable);
	}

	public Player FindPlayerforVRRig(VRRig vRRig)
	{
		if (vRRig.photonView != null && vRRig.photonView.Owner != null)
		{
			return vRRig.photonView.Owner;
		}
		return null;
	}

	public string NormalizeName(bool doIt, string text)
	{
		if (doIt)
		{
			text = new string(Array.FindAll(text.ToCharArray(), (char c) => char.IsLetterOrDigit(c)));
			if (text.Length > 12)
			{
				text = text.Substring(0, 12);
			}
			text = text.ToUpper();
		}
		return text;
	}
}