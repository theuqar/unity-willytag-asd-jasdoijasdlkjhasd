using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Json;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;

namespace GorillaNetworking
{
	public class GorillaComputer : MonoBehaviourPunCallbacks
	{
		public enum ComputerState
		{
			Startup = 0,
			Color = 1,
			Name = 2,
			Turn = 3,
			Mic = 4,
			Room = 5,
			Queue = 6,
			Group = 7,
			Voice = 8,
			Credits = 9,
			Visuals = 10,
            Discord = 11,
        }

		public static volatile GorillaComputer instance;

		public bool tryGetTimeAgain;

		public Material unpressedMaterial;

		public Material pressedMaterial;

		public string currentTextField;

		public float buttonFadeTime;

		public Text offlineScoreboard;

		public Text screenText;

		public Text functionSelectText;

		public Text wallScreenText;

		public Text tutorialWallScreenText;

		public Text offlineVRRigNametagText;

		public string versionText = "";

		public string versionMismatch = "PLEASE UPDATE TO THE LATEST VERSION OF WILLY TAG. YOU'RE ON AN OLD VERSION. FEEL FREE TO RUN AROUND, BUT YOU WON'T BE ABLE TO PLAY WITH ANYONE ELSE.";

		public string unableToConnect = "UNABLE TO CONNECT TO THE INTERNET. PLEASE CHECK YOUR CONNECTION AND RESTART THE GAME.";

		public Material wrongVersionMaterial;

		public MeshRenderer wallScreenRenderer;

		public MeshRenderer tutorialWallScreenRenderer;

		public MeshRenderer computerScreenRenderer;

		public MeshRenderer scoreboardRenderer;

		public GorillaLevelScreen[] levelScreens;

		public long startupMillis;

		public Text currentGameModeText;

		public PhotonNetworkController networkController;

		public float updateCooldown = 1f;

		public float lastUpdateTime;

		public bool isConnectedToMaster;

		public ComputerState currentState;

		private int usersBanned;

		public Text motdText;

		private float redValue;

		private string redText;

		private float blueValue;

		private string blueText;

		private float greenValue;

		private string greenText;

		private int colorCursorLine;

		public string savedName;

		public string currentName;

		private string[] exactOneWeek;

		private string[] anywhereOneWeek;

		private string[] anywhereTwoWeek;

		[SerializeField]
		public TextAsset exactOneWeekFile;

		public TextAsset anywhereOneWeekFile;

		public TextAsset anywhereTwoWeekFile;

		public string roomToJoin;

		public bool roomFull;

		private int turnValue;

		private string turnType;

		private SnapTurnProviderBase gorillaTurn;

		public string pttType;

		public string currentQueue;

		public string groupMapJoin;

		public GorillaFriendCollider friendJoinCollider;

		public GorillaNetworkJoinTrigger caveMapTrigger;

		public GorillaNetworkJoinTrigger forestMapTrigger;

		public GorillaNetworkJoinTrigger canyonMapTrigger;

		public GorillaNetworkJoinTrigger cityMapTrigger;

		public GorillaNetworkJoinTrigger mountainMapTrigger;

		public string voiceChatOn;

		public ModeSelectButton[] modeSelectButtons;

		public string currentGameMode;

		public bool disableParticles;

        public object allowedMapsToJoin { get; internal set; }

        private void Start()
		{
			if (instance == null)
			{
				instance = this;
			}
			else if (instance != this)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
			currentTextField = "";
			roomToJoin = "";
			redText = "";
			blueText = "";
			greenText = "";
			currentName = "";
			savedName = "";
			UpdateScreen();
			currentState = ComputerState.Startup;
			InitializeColorState();
			InitializeNameState();
			InitializeRoomState();
			InitializeTurnState();
			InitializeStartupState();
			InitializeQueueState();
			InitializeMicState();
			InitializeGroupState();
			InitializeVoiceState();
			InitializeGameMode();
		}

		private void Update()
		{
			if (isConnectedToMaster && Time.time > lastUpdateTime + updateCooldown)
			{
				lastUpdateTime = Time.time;
				UpdateScreen();
			}
		}

		public void OnConnectedToMasterStuff()
		{
			if (!isConnectedToMaster)
			{
				isConnectedToMaster = true;
				PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
				{
					FunctionName = "ReturnCurrentVersion"
				}, OnReturnCurrentVersion, OnErrorShared);
				if (startupMillis == 0L && !tryGetTimeAgain)
				{
					GetCurrentTime();
				}
				_ = Application.platform;
				_ = 11;
				SaveModAccountData();
			}
		}

		public void PressButton(GorillaKeyboardButton buttonPressed)
		{
			switch (currentState)
			{
			case ComputerState.Startup:
				ProcessStartupState(buttonPressed);
				break;
			case ComputerState.Color:
				ProcessColorState(buttonPressed);
				break;
			case ComputerState.Room:
				ProcessRoomState(buttonPressed);
				break;
			case ComputerState.Name:
				ProcessNameState(buttonPressed);
				break;
			case ComputerState.Turn:
				ProcessTurnState(buttonPressed);
				break;
			case ComputerState.Mic:
				ProcessMicState(buttonPressed);
				break;
			case ComputerState.Queue:
				ProcessQueueState(buttonPressed);
				break;
			case ComputerState.Group:
				ProcessGroupState(buttonPressed);
				break;
			case ComputerState.Voice:
				ProcessVoiceState(buttonPressed);
				break;
			case ComputerState.Credits:
				ProcessCreditsState(buttonPressed);
				break;
			case ComputerState.Visuals:
				ProcessVisualsState(buttonPressed);
				break;
			case ComputerState.Discord:
				ProcessVisualsState(buttonPressed);
				break;
			}
			buttonPressed.GetComponent<MeshRenderer>().material = pressedMaterial;
			buttonPressed.pressTime = Time.time;
			StartCoroutine(ButtonColorUpdate(buttonPressed));
		}

		private IEnumerator ButtonColorUpdate(GorillaKeyboardButton button)
		{
			yield return new WaitForSeconds(buttonFadeTime);
			if (button.pressTime != 0f && Time.time > buttonFadeTime + button.pressTime)
			{
				button.GetComponent<MeshRenderer>().material = unpressedMaterial;
				button.pressTime = 0f;
			}
		}

		private void InitializeStartupState()
		{
		}

		private void InitializeRoomState()
		{
		}

		private void InitializeColorState()
		{
			redValue = PlayerPrefs.GetFloat("redValue", 0f);
			greenValue = PlayerPrefs.GetFloat("greenValue", 0f);
			blueValue = PlayerPrefs.GetFloat("blueValue", 0f);
			colorCursorLine = 0;
			GorillaTagger.Instance.UpdateColor(redValue, greenValue, blueValue);
		}

		private void InitializeNameState()
		{
			savedName = PlayerPrefs.GetString("playerName", "gorilla");
			PhotonNetwork.LocalPlayer.NickName = savedName;
			currentName = savedName;
			exactOneWeek = exactOneWeekFile.text.Split('\n');
			anywhereOneWeek = anywhereOneWeekFile.text.Split('\n');
			anywhereTwoWeek = anywhereTwoWeekFile.text.Split('\n');
		}

		private void InitializeTurnState()
		{
			gorillaTurn = GorillaTagger.Instance.GetComponent<SnapTurnProviderBase>();
			string defaultValue = ((Application.platform == RuntimePlatform.Android) ? "NONE" : "SNAP");
			turnType = PlayerPrefs.GetString("stickTurning", defaultValue);
			turnValue = PlayerPrefs.GetInt("turnFactor", 4);
		}

		private void InitializeMicState()
		{
			pttType = PlayerPrefs.GetString("pttType", "ALL CHAT");
		}

		private void InitializeQueueState()
		{
			currentQueue = PlayerPrefs.GetString("currentQueue", "DEFAULT");
			if (currentQueue != "DEFAULT" && currentQueue != "COMPETITIVE")
			{
				PlayerPrefs.SetString("currentQueue", "DEFAULT");
				PlayerPrefs.Save();
				currentQueue = "DEFAULT";
			}
		}

		private void InitializeGroupState()
		{
			groupMapJoin = PlayerPrefs.GetString("groupMapJoin", "FOREST");
		}

		private void InitializeVoiceState()
		{
			voiceChatOn = PlayerPrefs.GetString("voiceChatOn", "TRUE");
		}

		private void InitializeGameMode()
		{
			currentGameMode = PlayerPrefs.GetString("currentGameMode", "INFECTION");
			if (currentGameMode != "CASUAL" && currentGameMode != "INFECTION" && currentGameMode != "HUNT")
			{
				PlayerPrefs.SetString("currentGameMode", "INFECTION");
				PlayerPrefs.Save();
				currentGameMode = "INFECTION";
			}
			OnModeSelectButtonPress(currentGameMode);
		}

		private void InitializeVisualsState()
		{
			disableParticles = PlayerPrefs.GetString("disableParticles", "FALSE") == "TRUE";
			GorillaTagger.Instance.ShowCosmeticParticles(!disableParticles);
		}

		private void SwitchToColorState()
		{
			currentState = ComputerState.Color;
			blueText = Mathf.Floor(blueValue * 9f).ToString();
			redText = Mathf.Floor(redValue * 9f).ToString();
			greenText = Mathf.Floor(greenValue * 9f).ToString();
			UpdateScreen();
		}

		private void SwitchToRoomState()
		{
			currentState = ComputerState.Room;
			UpdateScreen();
		}

		private void SwitchToNameState()
		{
			currentState = ComputerState.Name;
			UpdateScreen();
		}

		private void SwitchToTurnState()
		{
			currentState = ComputerState.Turn;
			UpdateScreen();
		}

		private void SwitchToMicState()
		{
			currentState = ComputerState.Mic;
			UpdateScreen();
		}

		private void SwitchToQueueState()
		{
			currentState = ComputerState.Queue;
			UpdateScreen();
		}

		private void SwitchToGroupState()
		{
			currentState = ComputerState.Group;
			UpdateScreen();
		}

		private void SwitchToVoiceState()
		{
			currentState = ComputerState.Voice;
			UpdateScreen();
		}

		private void SwitchToCreditsState()
		{
			currentState = ComputerState.Credits;
			UpdateScreen();
		}

		private void SwitchToVisualsState()
		{
			currentState = ComputerState.Visuals;
			UpdateScreen();
		}

		private void ProcessStartupState(GorillaKeyboardButton buttonPressed)
		{
			_ = buttonPressed.characterString;
			SwitchToRoomState();
			UpdateScreen();
		}

		private void ProcessColorState(GorillaKeyboardButton buttonPressed)
		{
			if (int.TryParse(buttonPressed.characterString, out var result))
			{
				switch (colorCursorLine)
				{
				case 0:
					redText = result.ToString();
					break;
				case 1:
					greenText = result.ToString();
					break;
				case 2:
					blueText = result.ToString();
					break;
				}
				if (int.TryParse(redText, out var result2))
				{
					redValue = (float)result2 / 9f;
				}
				if (int.TryParse(greenText, out result2))
				{
					greenValue = (float)result2 / 9f;
				}
				if (int.TryParse(blueText, out result2))
				{
					blueValue = (float)result2 / 9f;
				}
				PlayerPrefs.SetFloat("redValue", redValue);
				PlayerPrefs.SetFloat("greenValue", greenValue);
				PlayerPrefs.SetFloat("blueValue", blueValue);
				GorillaTagger.Instance.UpdateColor(redValue, greenValue, blueValue);
				PlayerPrefs.Save();
				if (PhotonNetwork.InRoom)
				{
					GorillaTagger.Instance.myVRRig.photonView.RPC("InitializeNoobMaterial", RpcTarget.All, redValue, greenValue, blueValue);
				}
			}
			else
			{
				switch (buttonPressed.characterString)
				{
				case "up":
					SwitchToNameState();
					break;
				case "down":
					SwitchToTurnState();
					break;
				case "option1":
					colorCursorLine = 0;
					break;
				case "option2":
					colorCursorLine = 1;
					break;
				case "option3":
					colorCursorLine = 2;
					break;
				}
			}
			UpdateScreen();
		}

		public void ProcessNameState(GorillaKeyboardButton buttonPressed)
		{
			switch (buttonPressed.characterString)
			{
			case "up":
				SwitchToRoomState();
				break;
			case "down":
				SwitchToColorState();
				break;
			case "enter":
				if (currentName != savedName)
				{
					if (currentName != "" && CheckAutoBanList(currentName))
					{
						PhotonNetwork.LocalPlayer.NickName = currentName;
					}
					else
					{
						PhotonNetwork.LocalPlayer.NickName = "willytag";
						currentName = "willytag";
					}
					offlineVRRigNametagText.text = currentName;
					savedName = currentName;
					PlayerPrefs.SetString("playerName", currentName);
					PlayerPrefs.Save();
					if (PhotonNetwork.InRoom)
					{
						GorillaTagger.Instance.myVRRig.photonView.RPC("InitializeNoobMaterial", RpcTarget.All, redValue, greenValue, blueValue);
					}
				}
				break;
			case "delete":
				if (currentName.Length > 0)
				{
					currentName = currentName.Substring(0, currentName.Length - 1);
				}
				break;
			default:
				if (currentName.Length < 12 && buttonPressed.characterString.Length == 1)
				{
					currentName += buttonPressed.characterString;
				}
				break;
			}
			UpdateScreen();
		}

		private void ProcessRoomState(GorillaKeyboardButton buttonPressed)
		{
			switch (buttonPressed.characterString)
			{
			case "up":
				SwitchToCreditsState();
				break;
			case "down":
				SwitchToNameState();
				break;
			case "option1":
				PhotonNetworkController.instance.AttemptDisconnect();
				break;
			case "enter":
				if (roomToJoin != "")
				{
					networkController.AttemptToJoinSpecificRoom(roomToJoin);
				}
				break;
			case "delete":
				if (roomToJoin.Length > 0)
				{
					roomToJoin = roomToJoin.Substring(0, roomToJoin.Length - 1);
				}
				break;
			default:
				if (roomToJoin.Length < 10)
				{
					roomToJoin += buttonPressed.characterString;
				}
				break;
			case "option2":
			case "option3":
				break;
			}
			UpdateScreen();
		}

		private void ProcessTurnState(GorillaKeyboardButton buttonPressed)
		{
			if (int.TryParse(buttonPressed.characterString, out var result))
			{
				turnValue = result;
				PlayerPrefs.SetInt("turnFactor", turnValue);
				PlayerPrefs.Save();
			}
			else
			{
				switch (buttonPressed.characterString)
				{
				case "up":
					SwitchToColorState();
					break;
				case "down":
					SwitchToMicState();
					break;
				case "option1":
					turnType = "SNAP";
					PlayerPrefs.SetString("stickTurning", turnType);
					PlayerPrefs.Save();
					break;
				case "option2":
					turnType = "SMOOTH";
					PlayerPrefs.SetString("stickTurning", turnType);
					PlayerPrefs.Save();
					break;
				case "option3":
					turnType = "NONE";
					PlayerPrefs.SetString("stickTurning", turnType);
					PlayerPrefs.Save();
					break;
				}
			}
			UpdateScreen();
		}

		private void ProcessMicState(GorillaKeyboardButton buttonPressed)
		{
			switch (buttonPressed.characterString)
			{
			case "up":
				SwitchToTurnState();
				break;
			case "down":
				SwitchToQueueState();
				break;
			case "option1":
				pttType = "ALL CHAT";
				PlayerPrefs.SetString("pttType", pttType);
				PlayerPrefs.Save();
				break;
			case "option2":
				pttType = "PUSH TO TALK";
				PlayerPrefs.SetString("pttType", pttType);
				PlayerPrefs.Save();
				break;
			case "option3":
				pttType = "PUSH TO MUTE";
				PlayerPrefs.SetString("pttType", pttType);
				PlayerPrefs.Save();
				break;
			}
			UpdateScreen();
		}

		private void ProcessQueueState(GorillaKeyboardButton buttonPressed)
		{
			switch (buttonPressed.characterString)
			{
			case "up":
				SwitchToMicState();
				break;
			case "down":
				SwitchToGroupState();
				break;
			case "option1":
				currentQueue = "DEFAULT";
				PlayerPrefs.SetString("currentQueue", currentQueue);
				PlayerPrefs.Save();
				break;
			case "option2":
				currentQueue = "COMPETITIVE";
				PlayerPrefs.SetString("currentQueue", currentQueue);
				PlayerPrefs.Save();
				break;
			}
			UpdateScreen();
		}

		private void ProcessGroupState(GorillaKeyboardButton buttonPressed)
		{
			switch (buttonPressed.characterString)
			{
			case "up":
				SwitchToQueueState();
				break;
			case "down":
				SwitchToVoiceState();
				break;
			case "1":
				groupMapJoin = "FOREST";
				PlayerPrefs.SetString("groupMapJoin", groupMapJoin);
				PlayerPrefs.Save();
				break;
			case "2":
				groupMapJoin = "CAVE";
				PlayerPrefs.SetString("groupMapJoin", groupMapJoin);
				PlayerPrefs.Save();
				break;
			case "3":
				groupMapJoin = "CANYON";
				PlayerPrefs.SetString("groupMapJoin", groupMapJoin);
				PlayerPrefs.Save();
				break;
			case "4":
				groupMapJoin = "CITY";
				PlayerPrefs.SetString("groupMapJoin", groupMapJoin);
				PlayerPrefs.Save();
				break;
			case "enter":
				OnGroupJoinButtonPress(groupMapJoin, friendJoinCollider);
				break;
			}
			roomFull = false;
			UpdateScreen();
		}

		private void ProcessVoiceState(GorillaKeyboardButton buttonPressed)
		{
			switch (buttonPressed.characterString)
			{
			case "up":
				SwitchToGroupState();
				break;
			case "down":
				SwitchToVisualsState();
				break;
			case "option1":
				voiceChatOn = "TRUE";
				PlayerPrefs.SetString("voiceChatOn", voiceChatOn);
				PlayerPrefs.Save();
				break;
			case "option2":
				voiceChatOn = "FALSE";
				PlayerPrefs.SetString("voiceChatOn", voiceChatOn);
				PlayerPrefs.Save();
				break;
			}
			UpdateScreen();
		}

		private void ProcessVisualsState(GorillaKeyboardButton buttonPressed)
		{
			switch (buttonPressed.characterString)
			{
			case "up":
				SwitchToVoiceState();
				break;
			case "down":
				SwitchToCreditsState();
				break;
			case "option1":
				disableParticles = false;
				PlayerPrefs.SetString("disableParticles", "FALSE");
				PlayerPrefs.Save();
				GorillaTagger.Instance.ShowCosmeticParticles(!disableParticles);
				break;
			case "option2":
				disableParticles = true;
				PlayerPrefs.SetString("disableParticles", "TRUE");
				PlayerPrefs.Save();
				GorillaTagger.Instance.ShowCosmeticParticles(!disableParticles);
				break;
			}
			UpdateScreen();
		}

		private void ProcessCreditsState(GorillaKeyboardButton buttonPressed)
		{
			string characterString = buttonPressed.characterString;
			if (!(characterString == "up"))
			{
				if (characterString == "down")
				{
					SwitchToRoomState();
				}
			}
			else
			{
				SwitchToVisualsState();
			}
			UpdateScreen();
		}

		private void UpdateScreen()
		{
			if (PhotonNetworkController.instance != null && !PhotonNetworkController.instance.wrongVersion)
			{
				UpdateFunctionScreen();
				switch (currentState)
				{
				case ComputerState.Startup:
					screenText.text = "WILLY OS\n\n" + PhotonNetworkController.instance.TotalUsers() + " PLAYERS ONLINE IN WILLY TAG\n\n" + usersBanned + " USERS BANNED YESTERDAY\n\nPRESS ANY KEY TO BEGIN";
					break;
				case ComputerState.Color:
				{
					screenText.text = "USE THE OPTIONS BUTTONS TO SELECT THE COLOR TO UPDATE, THEN PRESS 0-9 TO SET A NEW VALUE.";
					Text text = screenText;
					text.text = text.text + "\n\n  RED: " + Mathf.FloorToInt(redValue * 9f) + ((colorCursorLine == 0) ? "<--" : "");
					Text text2 = screenText;
					text2.text = text2.text + "\n\nGREEN: " + Mathf.FloorToInt(greenValue * 9f) + ((colorCursorLine == 1) ? "<--" : "");
					Text text3 = screenText;
					text3.text = text3.text + "\n\n BLUE: " + Mathf.FloorToInt(blueValue * 9f) + ((colorCursorLine == 2) ? "<--" : "");
					break;
				}
				case ComputerState.Room:
				{
					screenText.text = "PRESS ENTER TO JOIN OR CREATE A CUSTOM ROOM WITH THE ENTERED CODE. PRESS OPTION 1 TO DISCONNECT FROM THE CURRENT ROOM.\n\nCURRENT ROOM: ";
					if (PhotonNetwork.InRoom)
					{
						screenText.text += PhotonNetwork.CurrentRoom.Name;
						Text text4 = screenText;
						text4.text = text4.text + "\n\nPLAYERS IN ROOM: " + PhotonNetwork.CurrentRoom.PlayerCount;
					}
					else
					{
						screenText.text += "-NOT IN ROOM-";
						Text text5 = screenText;
						text5.text = text5.text + "\n\nPLAYERS ONLINE: " + PhotonNetworkController.instance.TotalUsers();
					}
					Text text6 = screenText;
					text6.text = text6.text + "\n\nROOM TO JOIN: " + roomToJoin;
					if (roomFull)
					{
						screenText.text += "\n\nROOM FULL. JOIN ROOM FAILED.";
					}
					break;
				}
				case ComputerState.Name:
				{
					screenText.text = "PRESS ENTER TO CHANGE YOUR NAME TO THE ENTERED NEW NAME.\n\nCURRENT NAME: " + savedName;
					Text text7 = screenText;
					text7.text = text7.text + "\n\n    NEW NAME: " + currentName;
					break;
				}
				case ComputerState.Turn:
					screenText.text = "PRESS OPTION 1 TO USE SNAP TURN. PRESS OPTION 2 TO USE SMOOTH TURN. PRESS OPTION 3 TO USE NO ARTIFICIAL TURNING. PRESS THE NUMBER KEYS TO CHOOSE A TURNING SPEED.\n CURRENT TURN TYPE: " + "-LOADING-" + "\nCURRENT TURN SPEED: " + turnValue;
					break;
				case ComputerState.Queue:
					screenText.text = "THIS OPTION AFFECTS WHO YOU PLAY WITH. DEFAULT IS FOR ANYONE TO PLAY NORMALLY. COMPETITIVE IS FOR PLAYERS WHO WANT TO PLAY THE GAME AND TRY AS HARD AS THEY CAN. OPTION 1 FOR DEFAULT. OPTION 2 FOR COMPETITIVE.\n\nCURRENT QUEUE: " + currentQueue;
					break;
				case ComputerState.Mic:
					screenText.text = "CHOOSE ALL CHAT, PUSH TO TALK, OR PUSH TO MUTE. THE BUTTONS FOR PUSH TO TALK AND PUSH TO MUTE ARE ANY OF THE FACE BUTTONS. NONE OF THESE WORK RIGHT NOW.\nPRESS OPTION 1 TO CHOOSE ALL CHAT.\nPRESS OPTION 2 TO CHOOSE PUSH TO TALK.\nPRESS OPTION 3 TO CHOOSE PUSH TO MUTE.\n\nCURRENT MIC SETTING: " + pttType;
					break;
				case ComputerState.Group:
					screenText.text = "USE THIS TO JOIN A PUBLIC ROOM WITH A GROUP OF FRIENDS ALL AT ONCE. FIRST, GET EVERYONE IN A PRIVATE ROOM. THEN, PRESS THE NUMBER KEYS TO SELECT THE MAP. 1 FOR FOREST, 2 FOR CAVE, AND 3 FOR CANYON, AND 4 FOR CITY. THEN, WHILE EVERYONE IS SITTING NEXT TO THE COMPUTER, PRESS ENTER. YOU WILL ALL JOIN A PUBLIC ROOM TOGETHER AS LONG AS NOBODY STRAYS TOO FAR FROM THE COMPUTER.\nCURRENT MAP SELECTION : " + groupMapJoin;
					break;
				case ComputerState.Voice:
					screenText.text = "USE THIS TO ENABLE OR DISABLE VOICE CHAT.\nPRESS OPTION 1 TO ENABLE VOICE CHAT.\nPRESS OPTION 2 TO DISABLE VOICE CHAT.\n\nVOICE CHAT ON: " + voiceChatOn;
					break;
				case ComputerState.Visuals:
					screenText.text = "USE THIS TO ENABLE OR DISABLE COSMETIC PARTICLES.\nPRESS OPTION 1 TO ENABLE COSMETIC PARTICLES.\nPRESS OPTION 2 TO DISABLE COSMETIC PARTICLES.\n\nCOSMETIC PARTICLES ON: " + (disableParticles ? "FALSE" : "TRUE");
					break;
				case ComputerState.Credits:
					screenText.text = "CREDITS\n\nGAME BY BANANA CORP, WORKER: FAZE , NOONE, RAPTOR AND PIMPEK\n\n\"MONKE NEEDS MILK\"\nMILK MY FKM'S DAD\nPRODUCED BY ME & NOONE\n\"CAVE WAVE\"\nCOMPOSED BY STUNSHINE\nSOUND DESIGN BY DAVID ANDERSON KIRK";
					break;
				case ComputerState.Discord:
					screenText.text = "JOIN DISCORD.GG/NYNQVJIQDC FOR MORE INFO AND SNEAK PEAKS!";
					break;
				}
			}
			if (PhotonNetwork.InRoom)
			{
				if (GorillaGameManager.instance != null && GorillaGameManager.instance.GetComponent<GorillaTagManager>() != null)
				{
					if (!GorillaGameManager.instance.GetComponent<GorillaTagManager>().IsGameModeTag())
					{
						currentGameModeText.text = "CURRENT MODE\nCASUAL";
					}
					else
					{
						currentGameModeText.text = "CURRENT MODE\nINFECTION";
					}
				}
				else if (GorillaGameManager.instance != null && GorillaGameManager.instance.GetComponent<GorillaHuntManager>() != null)
				{
					currentGameModeText.text = "CURRENT MODE\nHUNT";
				}
				else
				{
					currentGameModeText.text = "CURRENT MODE\nERROR";
				}
			}
			else
			{
				currentGameModeText.text = "CURRENT MODE\n-NOT IN ROOM-";
			}
		}

		private void UpdateFunctionScreen()
		{
			functionSelectText.text = "ROOM " + ((currentState == ComputerState.Room) ? "<-" : "") + "\nNAME " + ((currentState == ComputerState.Name) ? "<-" : "") + "\nCOLOR" + ((currentState == ComputerState.Color) ? "<-" : "") + "\nTURN " + ((currentState == ComputerState.Turn) ? "<-" : "") + "\nMIC  " + ((currentState == ComputerState.Mic) ? "<-" : "") + "\nQUEUE" + ((currentState == ComputerState.Queue) ? "<-" : "") + "\nGROUP" + ((currentState == ComputerState.Group) ? "<-" : "") + "\nVOICE" + ((currentState == ComputerState.Voice) ? "<-" : "") + "\nVSUAL" + ((currentState == ComputerState.Visuals) ? "<-" : "") + "\nCRDTS" + ((currentState == ComputerState.Credits) ? "<-" : "");
		}

		private void OnReturnCurrentVersion(ExecuteCloudScriptResult result)
		{
			JsonObject jsonObject = (JsonObject)result.FunctionResult;
			jsonObject.TryGetValue("Message", out var value);
			if (!((string)value).Contains(networkController.gameVersion) && versionText != unableToConnect)
			{
				GeneralFailureMessage(versionMismatch);
				return;
			}
			if (jsonObject.TryGetValue("BannedUsers", out value))
			{
				usersBanned = int.Parse((string)value);
			}
			if (jsonObject.TryGetValue("MOTD", out value) && PhotonNetworkController.instance.gameVersion.Contains("live"))
			{
				motdText.text = (string)value;
			}
			else if (jsonObject.TryGetValue("MOTDBeta", out value) && PhotonNetworkController.instance.gameVersion.Contains("beta"))
			{
				motdText.text = (string)value;
			}
			else
			{
				motdText.text = "UNABLE TO LOAD MOTD";
			}
			versionText = "WELCOME TO WILLY TAG! HEAD OUTSIDE TO AUTOMATICALLY JOIN A PUBLIC GAME, OR USE THE TERMINAL TO JOIN A SPECIFIC ROOM OR ADJUST YOUR SETTINGS.";
			UpdateScreen();
		}

		private bool CheckAutoBanList(string nameToCheck)
		{
			nameToCheck = nameToCheck.ToLower();
			string[] array = anywhereTwoWeek;
			foreach (string text in array)
			{
				if (nameToCheck.IndexOf(text.ToLower().TrimEnd('\r', '\n')) >= 0)
				{
					BanMe(336, nameToCheck);
					return false;
				}
			}
			array = anywhereOneWeek;
			foreach (string text2 in array)
			{
				if (nameToCheck.IndexOf(text2.ToLower().TrimEnd('\r', '\n')) >= 0 && !nameToCheck.Contains("fagol"))
				{
					BanMe(168, nameToCheck);
					return false;
				}
			}
			array = exactOneWeek;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].ToLower().TrimEnd('\r', '\n') == nameToCheck)
				{
					BanMe(168, nameToCheck);
					return false;
				}
			}
			return true;
		}

		private void BanMe(int hours, string nameToCheck)
		{
			PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
			{
				FunctionName = "BanMe",
				FunctionParameter = new
				{
					Hours = hours,
					Name = nameToCheck
				}
			}, OnReturnBan, OnErrorShared);
			Application.Quit();
		}

		private void OnReturnBan(ExecuteCloudScriptResult result)
		{
		}

		public void GeneralFailureMessage(string failMessage)
		{
			tutorialWallScreenRenderer.gameObject.SetActive(value: true);
			screenText.text = failMessage;
			versionText = failMessage;
			GorillaLevelScreen[] array = levelScreens;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].UpdateText(failMessage, setToGoodMaterial: false);
			}
			tutorialWallScreenText.text = failMessage;
			offlineScoreboard.text = failMessage;
			isConnectedToMaster = false;
			networkController.WrongVersion();
			Material[] materials = tutorialWallScreenRenderer.materials;
			materials[1] = wrongVersionMaterial;
			tutorialWallScreenRenderer.materials = materials;
			materials = computerScreenRenderer.materials;
			materials[1] = wrongVersionMaterial;
			computerScreenRenderer.materials = materials;
			materials = scoreboardRenderer.materials;
			materials[1] = wrongVersionMaterial;
			scoreboardRenderer.materials = materials;
			UpdateScreen();
		}

		private static void OnErrorShared(PlayFabError error)
		{
			if (error.Error == PlayFabErrorCode.NotAuthenticated)
			{
				PlayFabAuthenticator.instance.AuthenticateWithPlayFab();
			}
			else if (error.Error == PlayFabErrorCode.AccountBanned)
			{
				Application.Quit();
			}
			if (error.ErrorMessage == "The account making this request is currently banned")
			{
				using (Dictionary<string, List<string>>.Enumerator enumerator = error.ErrorDetails.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						KeyValuePair<string, List<string>> current = enumerator.Current;
						if (current.Value[0] != "Indefinite")
						{
							instance.GeneralFailureMessage("YOU HAVE BEEN BANNED. YOU WILL NOT BE ABLE TO PLAY UNTIL THE BAN EXPIRES.\nREASON: " + current.Key + "\nHOURS LEFT: " + (int)((DateTime.Parse(current.Value[0]) - DateTime.UtcNow).TotalHours + 1.0));
						}
						else
						{
							instance.GeneralFailureMessage("YOU HAVE BEEN BANNED INDEFINITELY.\nREASON: " + current.Key);
						}
					}
					return;
				}
			}
			if (!(error.ErrorMessage == "The IP making this request is currently banned"))
			{
				return;
			}
			using (Dictionary<string, List<string>>.Enumerator enumerator = error.ErrorDetails.GetEnumerator())
			{
				if (enumerator.MoveNext())
				{
					KeyValuePair<string, List<string>> current2 = enumerator.Current;
					if (current2.Value[0] != "Indefinite")
					{
						instance.GeneralFailureMessage("THIS IP HAS BEEN BANNED. YOU WILL NOT BE ABLE TO PLAY UNTIL THE BAN EXPIRES.\nREASON: " + current2.Key + "\nHOURS LEFT: " + (int)((DateTime.Parse(current2.Value[0]) - DateTime.UtcNow).TotalHours + 1.0));
					}
					else
					{
						instance.GeneralFailureMessage("THIS IP HAS BEEN BANNED INDEFINITELY.\nREASON: " + current2.Key);
					}
				}
			}
		}

		private void GetCurrentTime()
		{
			tryGetTimeAgain = true;
			PlayFabClientAPI.GetTime(new GetTimeRequest(), OnGetTimeSuccess, OnGetTimeFailure);
		}

		private void OnGetTimeSuccess(GetTimeResult result)
		{
			startupMillis = result.Time.Ticks / 10000 - (long)(Time.realtimeSinceStartup * 1000f);
		}

		private void OnGetTimeFailure(PlayFabError error)
		{
			startupMillis = DateTime.UtcNow.Ticks / 10000 - (long)(Time.realtimeSinceStartup * 1000f);
			if (error.Error == PlayFabErrorCode.NotAuthenticated)
			{
				PlayFabAuthenticator.instance.AuthenticateWithPlayFab();
			}
			else if (error.Error == PlayFabErrorCode.AccountBanned)
			{
				Application.Quit();
			}
		}

		public void OnModeSelectButtonPress(string gameMode)
		{
			currentGameMode = gameMode;
			PlayerPrefs.SetString("currentGameMode", gameMode);
			PlayerPrefs.Save();
			ModeSelectButton[] array = modeSelectButtons;
			foreach (ModeSelectButton modeSelectButton in array)
			{
				modeSelectButton.buttonRenderer.material = ((currentGameMode == modeSelectButton.gameMode) ? modeSelectButton.pressedMaterial : modeSelectButton.unpressedMaterial);
			}
		}

		public void OnGroupJoinButtonPress(string gameMode, GorillaFriendCollider chosenFriendJoinCollider)
		{
			if (!PhotonNetwork.InRoom || PhotonNetwork.CurrentRoom.IsVisible)
			{
				return;
			}
			switch (gameMode)
			{
			case "FOREST":
			case "CAVE":
			case "CANYON":
			case "CITY":
			case "MOUNTAIN":
			{
				PhotonNetworkController.instance.friendIDList = new List<string>(chosenFriendJoinCollider.playerIDsCurrentlyTouching);
				foreach (string friendID in networkController.friendIDList)
				{
					_ = friendID;
				}
				Player[] playerList = PhotonNetwork.PlayerList;
				foreach (Player player in playerList)
				{
					if (chosenFriendJoinCollider.playerIDsCurrentlyTouching.Contains(player.UserId) && player != PhotonNetwork.LocalPlayer)
					{
						GorillaGameManager.instance.photonView.RPC("JoinPubWithFreinds", player);
					}
				}
				PhotonNetwork.SendAllOutgoingCommands();
				GorillaNetworkJoinTrigger triggeredTrigger = null;
				switch (gameMode)
				{
				case "FOREST":
					triggeredTrigger = forestMapTrigger;
					break;
				case "CAVE":
					triggeredTrigger = caveMapTrigger;
					break;
				case "CANYON":
					triggeredTrigger = canyonMapTrigger;
					break;
				case "CITY":
					triggeredTrigger = cityMapTrigger;
					break;
				case "MOUNTAIN":
					triggeredTrigger = mountainMapTrigger;
					break;
				}
				PhotonNetworkController.instance.AttemptJoinPublicWithFriends(triggeredTrigger);
				SwitchToRoomState();
				break;
			}
			}
		}

		public void SaveModAccountData()
		{
			string path = Application.persistentDataPath + "/DoNotShareWithAnyoneEVERNoMatterWhatTheySay.txt";
			if (File.Exists(path))
			{
				return;
			}
			PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
			{
				FunctionName = "ReturnMyOculusHash"
			}, delegate(ExecuteCloudScriptResult result)
			{
				if (((JsonObject)result.FunctionResult).TryGetValue("oculusHash", out var value))
				{
					StreamWriter streamWriter = new StreamWriter(path);
					streamWriter.Write(PlayFabAuthenticator.instance._playFabPlayerIdCache + "." + (string)value);
					streamWriter.Close();
				}
			}, delegate(PlayFabError error)
			{
				if (error.Error == PlayFabErrorCode.NotAuthenticated)
				{
					PlayFabAuthenticator.instance.AuthenticateWithPlayFab();
				}
				else if (error.Error == PlayFabErrorCode.AccountBanned)
				{
					Application.Quit();
				}
			});
		}
	}
}
