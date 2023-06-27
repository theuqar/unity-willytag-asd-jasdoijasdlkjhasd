using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using GorillaLocomotion;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice;
using PlayFab;
using UnityEngine;
using UnityEngine.Networking;
using Photon.Voice.PUN;

namespace GorillaNetworking
{
	public class PhotonNetworkController : MonoBehaviourPunCallbacks
	{
		public enum ConnectionState
		{
			Initialization = 0,
			WrongVersion = 1,
			DeterminingPingsAndPlayerCount = 2,
			ConnectedAndWaiting = 3,
			DisconnectingFromRoom = 4,
			JoiningPublicRoom = 5,
			JoiningSpecificRoom = 6,
			JoiningFriend = 7,
			InPrivateRoom = 8,
			InPublicRoom = 9
		}

		public enum ConnectionEvent
		{
			InitialConnection = 0,
			OnConnectedToMaster = 1,
			AttemptJoinPublicRoom = 2,
			AttemptJoinSpecificRoom = 3,
			AttemptToCreateRoom = 4,
			Disconnect = 5,
			OnJoinedRoom = 6,
			OnJoinRoomFailed = 7,
			OnJoinRandomFailed = 8,
			OnCreateRoomFailed = 9,
			OnDisconnected = 10,
			FoundFriendToJoin = 11,
			FollowFriendToPub = 12
		}

		public static volatile PhotonNetworkController instance;

		public int incrementCounter;

		private ConnectionState currentState;

		public PlayFabAuthenticator playFabAuthenticator;

		public string[] serverRegions;

		public string gameVersion;

		public bool isPrivate;

		public string customRoomID;

		public GameObject playerOffset;

		public SkinnedMeshRenderer[] offlineVRRig;

		public bool attemptingToConnect;

		private int currentRegionIndex;

		public string currentGameType;

		public bool wrongVersion;

		public bool roomCosmeticsInitialized;

		public GameObject photonVoiceObjectPrefab;

		public Dictionary<string, bool> playerCosmeticsLookup = new Dictionary<string, bool>();

		private bool pastFirstConnection;

		public float defaultLimit;

		public float defaultMultiplier;

		private float lastHeadRightHandDistance;

		private float lastHeadLeftHandDistance;

		private float pauseTime;

		private float disconnectTime = 120f;

		public bool disableAFKKick;

		private float headRightHandDistance;

		private float headLeftHandDistance;

		private Quaternion headQuat;

		private Quaternion lastHeadQuat;

		public GameObject[] disableOnStartup;

		public GameObject[] enableOnStartup;

		private string roomCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYX123456789";

		public bool updatedName;

		private int[] playersInRegion;

		private int[] pingInRegion;

		public List<string> friendIDList = new List<string>();

		private bool successfullyFoundFriend;

		private float startingToLookForFriend;

		private float timeToSpendLookingForFriend = 10f;

		private bool joiningWithFriend;

		private string friendToFollow;

		private bool isRoomFull;

		private bool doesRoomExist;

		private bool createRoom;

		public GorillaNetworkJoinTrigger privateTrigger;

		public GorillaNetworkJoinTrigger currentJoinTrigger;

		public void InitiateConnection()
		{
			ProcessState(ConnectionEvent.InitialConnection);
		}

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
			updatedName = false;
			playersInRegion = new int[serverRegions.Length];
			pingInRegion = new int[serverRegions.Length];
		}

		public void Start()
		{
			StartCoroutine(DisableOnStart());
			serverRegions[0] = "us";
			currentState = ConnectionState.Initialization;
			PhotonNetwork.EnableCloseConnection = false;
		}

		private IEnumerator DisableOnStart()
		{
			yield return new WaitForSeconds(2f);
			ConnectToRegion(serverRegions[0]);
			GameObject[] array = disableOnStartup;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].SetActive(value: false);
			}
			array = enableOnStartup;
			for (int j = 0; j < array.Length; j++)
			{
				array[j].SetActive(value: true);
			}
		}

		public void FixedUpdate()
		{
			headRightHandDistance = (GorillaLocomotion.Player.Instance.headCollider.transform.position - GorillaLocomotion.Player.Instance.rightHandTransform.position).magnitude;
			headLeftHandDistance = (GorillaLocomotion.Player.Instance.headCollider.transform.position - GorillaLocomotion.Player.Instance.leftHandTransform.position).magnitude;
			headQuat = GorillaLocomotion.Player.Instance.headCollider.transform.rotation;
			if (!disableAFKKick && Quaternion.Angle(headQuat, lastHeadQuat) <= 0.01f && Mathf.Abs(headRightHandDistance - lastHeadRightHandDistance) < 0.001f && Mathf.Abs(headLeftHandDistance - lastHeadLeftHandDistance) < 0.001f && pauseTime + disconnectTime < Time.realtimeSinceStartup)
			{
				pauseTime = Time.realtimeSinceStartup;
				GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().transform.parent.position = new Vector3(-64f, 12.745f, -83.04f);
				GorillaLocomotion.Player.Instance.GetComponent<Rigidbody>().transform.localPosition = Vector3.zero;
				GorillaLocomotion.Player.Instance.InitializeValues();
				GameObject[] array = disableOnStartup;
				for (int i = 0; i < array.Length; i++)
				{
					array[i].SetActive(value: false);
				}
				array = enableOnStartup;
				for (int j = 0; j < array.Length; j++)
				{
					array[j].SetActive(value: true);
				}
				ProcessState(ConnectionEvent.Disconnect);
			}
			else if (Quaternion.Angle(headQuat, lastHeadQuat) > 0.01f || Mathf.Abs(headRightHandDistance - lastHeadRightHandDistance) >= 0.001f || Mathf.Abs(headLeftHandDistance - lastHeadLeftHandDistance) >= 0.001f)
			{
				pauseTime = Time.realtimeSinceStartup;
			}
			lastHeadRightHandDistance = headRightHandDistance;
			lastHeadLeftHandDistance = headLeftHandDistance;
			lastHeadQuat = headQuat;
		}

		private void ProcessInitializationState(ConnectionEvent connectionEvent)
		{
			if (connectionEvent == ConnectionEvent.InitialConnection)
			{
				PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = gameVersion;
				currentRegionIndex = 0;
				if (PlayerPrefs.GetString("playerName") != "")
				{
					PhotonNetwork.LocalPlayer.NickName = PlayerPrefs.GetString("playerName");
				}
				else
				{
					PhotonNetwork.LocalPlayer.NickName = "gorilla" + Random.Range(0, 9999).ToString().PadLeft(4, '0');
				}
				PhotonNetwork.AutomaticallySyncScene = true;
				currentState = ConnectionState.DeterminingPingsAndPlayerCount;
				ConnectToRegion(serverRegions[currentRegionIndex]);
			}
			else
			{
				InvalidState(connectionEvent);
			}
		}

		private void ProcessDeterminingPingsAndPlayerCountState(ConnectionEvent connectionEvent)
		{
			switch (connectionEvent)
			{
			case ConnectionEvent.OnConnectedToMaster:
			{
				int ping = PhotonNetwork.GetPing();
				Debug.Log("current ping is " + ping + " on region " + serverRegions[currentRegionIndex] + ". player count is " + PhotonNetwork.CountOfPlayers);
				playersInRegion[currentRegionIndex] = PhotonNetwork.CountOfPlayers;
				pingInRegion[currentRegionIndex] = ping;
				PhotonNetwork.Disconnect();
				break;
			}
			default:
				InvalidState(connectionEvent);
				break;
			case ConnectionEvent.OnDisconnected:
				currentRegionIndex++;
				if (currentRegionIndex >= serverRegions.Length)
				{
					Debug.Log("checked all servers. connecting to server with best ping: " + GetRegionWithLowestPing());
					currentState = ConnectionState.ConnectedAndWaiting;
					if (currentJoinTrigger != null)
					{
						AttemptToJoinPublicRoom(currentJoinTrigger);
					}
					ConnectToRegion(GetRegionWithLowestPing());
				}
				else
				{
					Debug.Log("checking next region");
					ConnectToRegion(serverRegions[currentRegionIndex]);
				}
				break;
			}
		}

		private void ProcessConnectedAndWaitingState(ConnectionEvent connectionEvent)
		{
			switch (connectionEvent)
			{
			case ConnectionEvent.AttemptJoinPublicRoom:
				currentState = ConnectionState.JoiningPublicRoom;
				ProcessState(ConnectionEvent.AttemptJoinPublicRoom);
				break;
			case ConnectionEvent.AttemptJoinSpecificRoom:
				currentState = ConnectionState.JoiningSpecificRoom;
				ProcessState(ConnectionEvent.AttemptJoinSpecificRoom);
				break;
			case ConnectionEvent.Disconnect:
				PhotonNetwork.Disconnect();
				break;
			case ConnectionEvent.OnDisconnected:
				Debug.Log("not sure what happened, reconnecting to region with best ping");
				ConnectToRegion(GetRegionWithLowestPing());
				break;
			default:
				InvalidState(connectionEvent);
				break;
			}
		}

		private void ProcessDisconnectingFromRoomState(ConnectionEvent connectionEvent)
		{
			switch (connectionEvent)
			{
			case ConnectionEvent.OnConnectedToMaster:
				Debug.Log("successfully reconnected to master. waiting on what to do next.");
				break;
			case ConnectionEvent.Disconnect:
				PhotonNetwork.Disconnect();
				break;
			default:
				InvalidState(connectionEvent);
				break;
			case ConnectionEvent.OnDisconnected:
				Debug.Log("just disconnected while trying to disconnect. attempting to reconnect to best region.");
				currentState = ConnectionState.ConnectedAndWaiting;
				ConnectToRegion(GetRegionWithLowestPing());
				break;
			}
		}

		private void ProcessJoiningPublicRoomState(ConnectionEvent connectionEvent)
		{
			switch (connectionEvent)
			{
			case ConnectionEvent.OnConnectedToMaster:
				JoinPublicRoom(joiningWithFriend);
				break;
			case ConnectionEvent.AttemptJoinPublicRoom:
				if (!pastFirstConnection)
				{
					JoinPublicRoom(joiningWithFriend);
				}
				else
				{
					PhotonNetwork.Disconnect();
				}
				break;
			case ConnectionEvent.AttemptJoinSpecificRoom:
				JoinSpecificRoom();
				break;
			case ConnectionEvent.Disconnect:
				DisconnectFromRoom(ConnectionState.DisconnectingFromRoom);
				break;
			case ConnectionEvent.OnJoinedRoom:
				pastFirstConnection = true;
				currentJoinTrigger.UpdateScreens();
				currentState = ConnectionState.InPublicRoom;
				break;
			case ConnectionEvent.OnJoinRandomFailed:
				CreatePublicRoom(joiningWithFriend);
				break;
			case ConnectionEvent.OnCreateRoomFailed:
				CreatePublicRoom(joiningWithFriend);
				break;
			case ConnectionEvent.OnDisconnected:
				if (!joiningWithFriend)
				{
					if (!pastFirstConnection)
					{
						pastFirstConnection = true;
						PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = GetRegionWithLowestPing();
					}
					else
					{
						float value = Random.value;
						int num = 0;
						for (int i = 0; i < playersInRegion.Length; i++)
						{
							num += playersInRegion[i];
						}
						float num2 = 0f;
						int num3;
						for (num3 = -1; num2 < value; num2 += (float)playersInRegion[num3] / (float)num)
						{
							if (num3 >= playersInRegion.Length - 1)
							{
								break;
							}
							num3++;
						}
						PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = serverRegions[num3];
					}
				}
				PhotonNetwork.ConnectUsingSettings();
				break;
			default:
				InvalidState(connectionEvent);
				break;
			}
		}

		private void ProcessJoiningSpecificRoomState(ConnectionEvent connectionEvent)
		{
			switch (connectionEvent)
			{
			case ConnectionEvent.OnConnectedToMaster:
				if (!createRoom)
				{
					Debug.Log("connected to master in the determined region. joining specific room");
					JoinSpecificRoom();
				}
				else
				{
					createRoom = false;
					CreatePrivateRoom();
				}
				break;
			case ConnectionEvent.AttemptJoinPublicRoom:
				currentState = ConnectionState.JoiningPublicRoom;
				JoinPublicRoom(joinWithFriends: false);
				break;
			case ConnectionEvent.AttemptJoinSpecificRoom:
				isRoomFull = false;
				doesRoomExist = false;
				createRoom = false;
				currentRegionIndex = 0;
				PhotonNetwork.Disconnect();
				break;
			case ConnectionEvent.Disconnect:
				DisconnectFromRoom(ConnectionState.DisconnectingFromRoom);
				break;
			case ConnectionEvent.OnJoinedRoom:
				Debug.Log("successfully joined room!");
				currentState = (PhotonNetwork.CurrentRoom.IsVisible ? ConnectionState.InPublicRoom : ConnectionState.InPrivateRoom);
				if (currentState == ConnectionState.InPublicRoom)
				{
					Debug.Log("game mode of room joined is: " + (string)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"]);
					if (((string)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"]).Contains(GorillaComputer.instance.forestMapTrigger.gameModeName))
					{
						currentJoinTrigger = GorillaComputer.instance.forestMapTrigger;
					}
					else if (((string)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"]).Contains(GorillaComputer.instance.caveMapTrigger.gameModeName))
					{
						currentJoinTrigger = GorillaComputer.instance.caveMapTrigger;
					}
					else if (((string)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"]).Contains(GorillaComputer.instance.canyonMapTrigger.gameModeName))
					{
						currentJoinTrigger = GorillaComputer.instance.canyonMapTrigger;
					}
					else if (((string)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"]).Contains(GorillaComputer.instance.cityMapTrigger.gameModeName))
					{
						currentJoinTrigger = GorillaComputer.instance.cityMapTrigger;
					}
					currentJoinTrigger.UpdateScreens();
				}
				break;
			case ConnectionEvent.OnJoinRoomFailed:
				if (doesRoomExist && isRoomFull)
				{
					Debug.Log("cant join, the room is full! going back to best region.");
					GorillaComputer.instance.roomFull = true;
					DisconnectFromRoom(ConnectionState.DisconnectingFromRoom);
				}
				else if (currentRegionIndex == serverRegions.Length - 1)
				{
					createRoom = true;
					PhotonNetwork.Disconnect();
				}
				else
				{
					Debug.Log("room was missing. check the next region");
					currentRegionIndex++;
					PhotonNetwork.Disconnect();
				}
				break;
			case ConnectionEvent.OnCreateRoomFailed:
				Debug.Log("the room probably actually already exists, so maybe it was created just now? either way, give up.");
				DisconnectFromRoom(ConnectionState.ConnectedAndWaiting);
				break;
			case ConnectionEvent.OnDisconnected:
				if (!createRoom)
				{
					Debug.Log("attempt to join master and join a specific room");
					ConnectToRegion(serverRegions[currentRegionIndex]);
				}
				else
				{
					Debug.Log("checked all the rooms, it doesn't exist. lets go back to our fav region and create the room");
					ConnectToRegion(GetRegionWithLowestPing());
				}
				break;
			default:
				InvalidState(connectionEvent);
				break;
			}
		}

		private void ProcessJoiningFriendState(ConnectionEvent connectionEvent)
		{
			switch (connectionEvent)
			{
			case ConnectionEvent.OnConnectedToMaster:
				StartSearchingForFriend(friendToFollow);
				break;
			case ConnectionEvent.AttemptJoinPublicRoom:
				currentState = ConnectionState.JoiningPublicRoom;
				ProcessState(ConnectionEvent.AttemptJoinPublicRoom);
				break;
			case ConnectionEvent.AttemptJoinSpecificRoom:
				JoinSpecificRoom();
				break;
			case ConnectionEvent.Disconnect:
				PhotonNetwork.Disconnect();
				break;
			case ConnectionEvent.OnJoinedRoom:
				currentState = ConnectionState.InPublicRoom;
				if (((string)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"]).Contains(GorillaComputer.instance.forestMapTrigger.gameModeName))
				{
					currentJoinTrigger = GorillaComputer.instance.forestMapTrigger;
				}
				else if (((string)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"]).Contains(GorillaComputer.instance.caveMapTrigger.gameModeName))
				{
					currentJoinTrigger = GorillaComputer.instance.caveMapTrigger;
				}
				else if (((string)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"]).Contains(GorillaComputer.instance.canyonMapTrigger.gameModeName))
				{
					currentJoinTrigger = GorillaComputer.instance.canyonMapTrigger;
				}
				else if (((string)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"]).Contains(GorillaComputer.instance.cityMapTrigger.gameModeName))
				{
					currentJoinTrigger = GorillaComputer.instance.cityMapTrigger;
				}
				currentJoinTrigger.UpdateScreens();
				break;
			case ConnectionEvent.OnJoinRoomFailed:
				currentState = ConnectionState.ConnectedAndWaiting;
				break;
			case ConnectionEvent.OnDisconnected:
				PhotonNetwork.ConnectUsingSettings();
				break;
			case ConnectionEvent.FoundFriendToJoin:
				JoinSpecificRoom();
				break;
			default:
				InvalidState(connectionEvent);
				break;
			}
		}

		private void ProcessInPrivateRoomState(ConnectionEvent connectionEvent)
		{
			switch (connectionEvent)
			{
			case ConnectionEvent.AttemptJoinPublicRoom:
				if (joiningWithFriend)
				{
					DisconnectFromRoom(ConnectionState.JoiningPublicRoom);
				}
				break;
			case ConnectionEvent.AttemptJoinSpecificRoom:
				if (PhotonNetwork.CurrentRoom.Name != customRoomID)
				{
					DisconnectCleanup();
					currentState = ConnectionState.JoiningSpecificRoom;
					currentRegionIndex = 0;
					PhotonNetwork.Disconnect();
				}
				break;
			case ConnectionEvent.Disconnect:
				DisconnectFromRoom(ConnectionState.DisconnectingFromRoom);
				break;
			case ConnectionEvent.OnDisconnected:
				DisconnectCleanup();
				currentState = ConnectionState.DisconnectingFromRoom;
				PhotonNetwork.ConnectUsingSettings();
				break;
			case ConnectionEvent.FollowFriendToPub:
				DisconnectFromRoom(ConnectionState.JoiningFriend);
				break;
			default:
				InvalidState(connectionEvent);
				break;
			}
		}

		private void ProcessInPublicRoomState(ConnectionEvent connectionEvent)
		{
			switch (connectionEvent)
			{
			case ConnectionEvent.AttemptJoinPublicRoom:
				if (!((string)PhotonNetwork.CurrentRoom.CustomProperties["gameMode"]).Contains(currentJoinTrigger.gameModeName))
				{
					DisconnectFromRoom(ConnectionState.JoiningPublicRoom);
				}
				break;
			case ConnectionEvent.AttemptJoinSpecificRoom:
				if (PhotonNetwork.CurrentRoom.Name != customRoomID)
				{
					DisconnectCleanup();
					currentState = ConnectionState.JoiningSpecificRoom;
					currentRegionIndex = 0;
					PhotonNetwork.Disconnect();
				}
				break;
			case ConnectionEvent.Disconnect:
				DisconnectFromRoom(ConnectionState.DisconnectingFromRoom);
				break;
			case ConnectionEvent.OnDisconnected:
				DisconnectCleanup();
				currentState = ConnectionState.DisconnectingFromRoom;
				PhotonNetwork.ConnectUsingSettings();
				break;
			default:
				InvalidState(connectionEvent);
				break;
			}
		}

		private void ProcessWrongVersionState(ConnectionEvent connectionEvent)
		{
			InvalidState(connectionEvent);
		}

		private void ProcessState(ConnectionEvent connectionEvent)
		{
			if (currentState == ConnectionState.Initialization)
			{
				ProcessInitializationState(connectionEvent);
			}
			else if (currentState == ConnectionState.DeterminingPingsAndPlayerCount)
			{
				ProcessDeterminingPingsAndPlayerCountState(connectionEvent);
			}
			else if (currentState == ConnectionState.ConnectedAndWaiting)
			{
				ProcessConnectedAndWaitingState(connectionEvent);
			}
			else if (currentState == ConnectionState.DisconnectingFromRoom)
			{
				ProcessDisconnectingFromRoomState(connectionEvent);
			}
			else if (currentState == ConnectionState.JoiningPublicRoom)
			{
				ProcessJoiningPublicRoomState(connectionEvent);
			}
			else if (currentState == ConnectionState.JoiningSpecificRoom)
			{
				ProcessJoiningSpecificRoomState(connectionEvent);
			}
			else if (currentState == ConnectionState.JoiningFriend)
			{
				ProcessJoiningFriendState(connectionEvent);
			}
			else if (currentState == ConnectionState.InPrivateRoom)
			{
				ProcessInPrivateRoomState(connectionEvent);
			}
			else if (currentState == ConnectionState.InPublicRoom)
			{
				ProcessInPublicRoomState(connectionEvent);
			}
			else if (currentState == ConnectionState.WrongVersion)
			{
				ProcessWrongVersionState(connectionEvent);
			}
		}

		private void InvalidState(ConnectionEvent connectionEvent)
		{
		}

		public void AttemptToJoinPublicRoom(GorillaNetworkJoinTrigger triggeredTrigger)
		{
			currentJoinTrigger = triggeredTrigger;
			joiningWithFriend = false;
			ProcessState(ConnectionEvent.AttemptJoinPublicRoom);
		}

		public void AttemptToJoinSpecificRoom(string roomID)
		{
			customRoomID = roomID;
			joiningWithFriend = false;
			ProcessState(ConnectionEvent.AttemptJoinSpecificRoom);
		}

		private void JoinPublicRoom(bool joinWithFriends)
		{
			PhotonNetwork.LocalPlayer.NickName = GorillaComputer.instance.savedName;
			if (PlayFabClientAPI.IsClientLoggedIn())
			{
				playFabAuthenticator.SetDisplayName(PhotonNetwork.LocalPlayer.NickName);
			}
			ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
			if (currentJoinTrigger.gameModeName != "city")
			{
				hashtable.Add("gameMode", currentJoinTrigger.gameModeName + GorillaComputer.instance.currentQueue + GorillaComputer.instance.currentGameMode);
			}
			else
			{
				hashtable.Add("gameMode", currentJoinTrigger.gameModeName + GorillaComputer.instance.currentQueue + "CASUAL");
			}
			PhotonNetwork.AutomaticallySyncScene = true;
			if (joinWithFriends)
			{
				friendIDList.Remove(PhotonNetwork.LocalPlayer.UserId);
				PhotonNetwork.JoinRandomRoom(hashtable, GetRoomSize(currentJoinTrigger.gameModeName), MatchmakingMode.RandomMatching, null, null, friendIDList.ToArray());
			}
			else
			{
				PhotonNetwork.JoinRandomRoom(hashtable, GetRoomSize(currentJoinTrigger.gameModeName), MatchmakingMode.FillRoom, null, null);
			}
		}

		private void JoinSpecificRoom()
		{
			PhotonNetwork.LocalPlayer.NickName = GorillaComputer.instance.savedName;
			if (PlayFabClientAPI.IsClientLoggedIn())
			{
				playFabAuthenticator.SetDisplayName(PhotonNetwork.LocalPlayer.NickName);
			}
			PhotonNetwork.JoinRoom(customRoomID);
		}

		private void DisconnectCleanup()
		{
			if (GorillaParent.instance != null)
			{
				GorillaScoreboardSpawner[] componentsInChildren = GorillaParent.instance.GetComponentsInChildren<GorillaScoreboardSpawner>();
				for (int i = 0; i < componentsInChildren.Length; i++)
				{
					componentsInChildren[i].OnLeftRoom();
				}
			}
			attemptingToConnect = true;
			SkinnedMeshRenderer[] array = offlineVRRig;
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in array)
			{
				if (skinnedMeshRenderer != null)
				{
					skinnedMeshRenderer.enabled = true;
				}
			}
			if (GorillaComputer.instance != null)
			{
				GorillaLevelScreen[] levelScreens = GorillaComputer.instance.levelScreens;
				foreach (GorillaLevelScreen obj in levelScreens)
				{
					obj.UpdateText(obj.startingText, setToGoodMaterial: true);
				}
			}
			GorillaLocomotion.Player.Instance.maxJumpSpeed = 6.5f;
			GorillaLocomotion.Player.Instance.jumpMultiplier = 1.1f;
			GorillaTagger.Instance.offlineVRRig.huntComputer.SetActive(value: false);
		}

		public override void OnConnectedToMaster()
		{
			ProcessState(ConnectionEvent.OnConnectedToMaster);
		}

		public override void OnJoinedRoom()
		{
			PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("gameMode", out var value);
			if (!PhotonNetwork.CurrentRoom.IsVisible)
			{
				currentJoinTrigger = privateTrigger;
				GorillaLevelScreen[] levelScreens = GorillaComputer.instance.levelScreens;
				for (int i = 0; i < levelScreens.Length; i++)
				{
					levelScreens[i].UpdateText("HEY MONKE! THIS IS A PRIVATE ROOM SO PLAY EVERWHERE YOU WANT TO! ENJOY", setToGoodMaterial: true);
				}
			}
			if (PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion != "dev")
			{
				StartCoroutine(UpdatePlayerCount());
			}
			if (PhotonNetwork.IsMasterClient)
			{
				if (value.ToString().Contains("CASUAL") || value.ToString().Contains("INFECTION"))
				{
					PhotonNetwork.InstantiateRoomObject("GorillaPrefabs/GorillaTagManager", playerOffset.transform.position, playerOffset.transform.rotation, 0);
				}
				else if (value.ToString().Contains("HUNT"))
				{
					PhotonNetwork.InstantiateRoomObject("GorillaPrefabs/GorillaHuntManager", playerOffset.transform.position, playerOffset.transform.rotation, 0);
				}
			}
			bool flag = PlayerPrefs.GetString("tutorial", "nope") == "done";
			if (!flag)
			{
				PlayerPrefs.SetString("tutorial", "done");
				PlayerPrefs.Save();
			}
			ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable();
			hashtable.Add("didTutorial", flag);
			PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable);
			PhotonNetwork.Instantiate("GorillaPrefabs/Gorilla Player Actual", playerOffset.transform.position, playerOffset.transform.rotation, 0);
			SkinnedMeshRenderer[] array = offlineVRRig;
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in array)
			{
				if (skinnedMeshRenderer != null)
				{
					skinnedMeshRenderer.enabled = false;
				}
			}
			PhotonNetwork.NickName = PlayerPrefs.GetString("playerName", "gorilla" + Random.Range(0, 9999).ToString().PadLeft(4, '0'));
			ProcessState(ConnectionEvent.OnJoinedRoom);
		}

		public override void OnJoinRoomFailed(short returnCode, string message)
		{
			if (returnCode == 32758)
			{
				doesRoomExist = false;
			}
			else
			{
				doesRoomExist = true;
			}
			if (returnCode == 32765)
			{
				isRoomFull = true;
			}
			else
			{
				isRoomFull = false;
			}
			ProcessState(ConnectionEvent.OnJoinRoomFailed);
		}

		public override void OnJoinRandomFailed(short returnCode, string message)
		{
			ProcessState(ConnectionEvent.OnJoinRandomFailed);
		}

		private void CreatePublicRoom(bool joinWithFriends)
		{
			ExitGames.Client.Photon.Hashtable customRoomProperties = ((!(currentJoinTrigger.gameModeName != "city")) ? new ExitGames.Client.Photon.Hashtable { 
			{
				"gameMode",
				currentJoinTrigger.gameModeName + GorillaComputer.instance.currentQueue + "CASUAL"
			} } : new ExitGames.Client.Photon.Hashtable { 
			{
				"gameMode",
				currentJoinTrigger.gameModeName + GorillaComputer.instance.currentQueue + GorillaComputer.instance.currentGameMode
			} });
			RoomOptions roomOptions = new RoomOptions();
			roomOptions.IsVisible = true;
			roomOptions.IsOpen = true;
			roomOptions.MaxPlayers = GetRoomSize(currentJoinTrigger.gameModeName);
			roomOptions.CustomRoomProperties = customRoomProperties;
			roomOptions.PublishUserId = true;
			roomOptions.CustomRoomPropertiesForLobby = new string[1] { "gameMode" };
			if (joinWithFriends)
			{
				string[] array = friendIDList.ToArray();
				for (int i = 0; i < array.Length; i++)
				{
					_ = array[i];
				}
				friendIDList.Remove(PhotonNetwork.LocalPlayer.UserId);
				PhotonNetwork.CreateRoom(ReturnRoomName(currentJoinTrigger.gameModeName), roomOptions, null, friendIDList.ToArray());
			}
			else
			{
				PhotonNetwork.CreateRoom(ReturnRoomName(currentJoinTrigger.gameModeName), roomOptions);
			}
		}

		private void CreatePrivateRoom()
		{
			currentJoinTrigger = privateTrigger;
			ExitGames.Client.Photon.Hashtable customRoomProperties = new ExitGames.Client.Photon.Hashtable { 
			{
				"gameMode",
				currentJoinTrigger.gameModeName + GorillaComputer.instance.currentQueue + GorillaComputer.instance.currentGameMode
			} };
			RoomOptions roomOptions = new RoomOptions();
			roomOptions.IsVisible = false;
			roomOptions.IsOpen = true;
			roomOptions.MaxPlayers = GetRoomSize(currentJoinTrigger.gameModeName);
			roomOptions.CustomRoomProperties = customRoomProperties;
			roomOptions.PublishUserId = true;
			roomOptions.CustomRoomPropertiesForLobby = new string[1] { "gameMode" };
			PhotonNetwork.CreateRoom(customRoomID, roomOptions);
		}

		public override void OnCreateRoomFailed(short returnCode, string message)
		{
			ProcessState(ConnectionEvent.OnCreateRoomFailed);
		}

		public void ConnectToRegion(string region)
		{
			PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = region;
			PhotonNetwork.ConnectUsingSettings();
		}

		public void AttemptJoinPublicWithFriends(GorillaNetworkJoinTrigger triggeredTrigger)
		{
			currentJoinTrigger = triggeredTrigger;
			joiningWithFriend = true;
			ProcessState(ConnectionEvent.AttemptJoinPublicRoom);
		}

		public void AttemptToFollowFriendIntoPub(string userIDToFollow)
		{
			friendToFollow = userIDToFollow;
			ProcessState(ConnectionEvent.FollowFriendToPub);
		}

		public void AttemptDisconnect()
		{
			ProcessState(ConnectionEvent.Disconnect);
		}

		private void DisconnectFromRoom(ConnectionState newState)
		{
			currentState = newState;
			PhotonNetwork.Disconnect();
		}

		public override void OnDisconnected(DisconnectCause cause)
		{
			DisconnectCleanup();
			ProcessState(ConnectionEvent.OnDisconnected);
		}

		public void WrongVersion()
		{
			wrongVersion = true;
			currentState = ConnectionState.WrongVersion;
		}

		public void OnApplicationQuit()
		{
			if (PhotonNetwork.IsConnected && PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion != "dev")
			{
				StartCoroutine(UpdatePlayerCount(leaving: true));
			}
		}

		private string ReturnRoomName(string currentGameMode)
		{
			if (isPrivate)
			{
				return customRoomID;
			}
			string text = "";
			for (int i = 0; i < 4; i++)
			{
				text += roomCharacters.Substring(Random.Range(0, roomCharacters.Length), 1);
			}
			return text;
		}

		public byte GetRoomSize(string gameModeName)
		{
			if (gameModeName.Contains("ball"))
			{
				return 5;
			}
			return 10;
		}

		public void StartSearchingForFriend(string userID)
		{
			startingToLookForFriend = Time.time;
			successfullyFoundFriend = false;
			StartCoroutine(SearchForFriendToJoin(userID));
		}

		private IEnumerator SearchForFriendToJoin(string userID)
		{
			while (!successfullyFoundFriend && startingToLookForFriend + timeToSpendLookingForFriend > Time.time)
			{
				if (PhotonNetwork.Server == ServerConnection.MasterServer && !PhotonNetwork.InRoom && PhotonNetwork.IsConnectedAndReady)
				{
					PhotonNetwork.FindFriends(new string[1] { userID });
				}
				yield return new WaitForSeconds(0.5f);
			}
			yield return null;
		}

		public override void OnFriendListUpdate(List<FriendInfo> friendList)
		{
			foreach (FriendInfo friend in friendList)
			{
				if (GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(PhotonNetwork.LocalPlayer.UserId) && friend.IsInRoom && friend.Room != "")
				{
					customRoomID = friend.Room;
					successfullyFoundFriend = true;
					ProcessState(ConnectionEvent.FoundFriendToJoin);
				}
			}
		}

		private string GetRegionWithLowestPing()
		{
			int num = 10000;
			int num2 = 0;
			for (int i = 0; i < serverRegions.Length; i++)
			{
				Debug.Log("ping in region " + serverRegions[i] + " is " + pingInRegion[i]);
				if (pingInRegion[i] < num && pingInRegion[i] > 0)
				{
					num = pingInRegion[i];
					num2 = i;
				}
			}
			return serverRegions[num2];
		}

		public int TotalUsers()
		{
			int num = 0;
			int[] array = playersInRegion;
			foreach (int num2 in array)
			{
				num += num2;
			}
			return num;
		}

		private IEnumerator UpdatePlayerCount(bool leaving = false)
		{
			WWWForm wWWForm = new WWWForm();
			wWWForm.AddField("player_count", TotalUsers());
			wWWForm.AddField("room_name", PhotonNetwork.CurrentRoom.Name);
			wWWForm.AddField("game_version", "live");
			wWWForm.AddField("game_name", Application.productName);
			using (UnityWebRequest www = UnityWebRequest.Post("http://ntsfranz.crabdance.com/update_monke_count", wWWForm))
			{
				yield return www.SendWebRequest();
				if (!www.isNetworkError)
				{
					_ = www.isHttpError;
				}
			}
		}
	}
}
