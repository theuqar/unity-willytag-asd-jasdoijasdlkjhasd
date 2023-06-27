// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// GorillaNot
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ExitGames.Client.Photon;
using GorillaNetworking;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class GorillaNot : MonoBehaviourPunCallbacks
{
	private class RPCCallTracker
	{
		public int RPCCalls;

		public int RPCCallsMax;
	}

	public static volatile GorillaNot instance;

	private bool _sendReport;

	private string _suspiciousPlayerId = "";

	private string _suspiciousPlayerName = "";

	private string _suspiciousReason = "";

	internal List<string> reportedPlayers = new List<string>();

	public byte roomSize;

	public float lastCheck;

	public float checkCooldown = 3f;

	public float userDecayTime = 15f;

	public Player currentMasterClient;

	public bool testAssault;

	private const byte ReportAssault = 8;

	private int lowestActorNumber;

	private int calls;

	public int rpcCallLimit = 50;

	public int logErrorMax = 50;

	public int rpcErrorMax = 10;

	private object outObj;

	private Player tempPlayer;

	private int logErrorCount;

	private int stringIndex;

	private string playerID;

	private string playerNick;

	private int lastServerTimestamp;

	private const string InvalidRPC = "invalid RPC stuff";

	private Dictionary<string, Dictionary<string, RPCCallTracker>> userRPCCalls = new Dictionary<string, Dictionary<string, RPCCallTracker>>();

	private ExitGames.Client.Photon.Hashtable hashTable;

	private bool sendReport
	{
		get
		{
			return _sendReport;
		}
		set
		{
			if (!_sendReport)
			{
				_sendReport = true;
			}
		}
	}

	private string suspiciousPlayerId
	{
		get
		{
			return _suspiciousPlayerId;
		}
		set
		{
			if (_suspiciousPlayerId == "")
			{
				_suspiciousPlayerId = value;
			}
		}
	}

	private string suspiciousPlayerName
	{
		get
		{
			return _suspiciousPlayerName;
		}
		set
		{
			if (_suspiciousPlayerName == "")
			{
				_suspiciousPlayerName = value;
			}
		}
	}

	private string suspiciousReason
	{
		get
		{
			return _suspiciousReason;
		}
		set
		{
			if (_suspiciousReason == "")
			{
				_suspiciousReason = value;
			}
		}
	}

	private void Start()
	{
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			Object.Destroy(this);
		}
		StartCoroutine(CheckReports());
		logErrorCount = 0;
		Application.logMessageReceived += LogErrorCount;
	}

	public void LogErrorCount(string logString, string stackTrace, LogType type)
	{
		if (type != 0)
		{
			return;
		}
		logErrorCount++;
		stringIndex = logString.LastIndexOf("Sender is ");
		if (logString.Contains("RPC") && stringIndex >= 0)
		{
			playerID = logString.Substring(stringIndex + 10);
			tempPlayer = null;
			for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
			{
				if (PhotonNetwork.PlayerList[i].UserId == playerID)
				{
					tempPlayer = PhotonNetwork.PlayerList[i];
					break;
				}
			}
			ref Player sender = ref tempPlayer;
			string rpcFunction = "invalid RPC stuff";
			if (!IncrementRPCTracker(in sender, in rpcFunction, in rpcErrorMax))
			{
				SendReport("invalid RPC stuff", tempPlayer.UserId, tempPlayer.NickName);
			}
			tempPlayer = null;
		}
		if (logErrorCount > logErrorMax)
		{
			Debug.unityLogger.logEnabled = false;
		}
	}

	public void SendReport(string susReason, string susId, string susNick)
	{
		suspiciousReason = susReason;
		suspiciousPlayerId = susId;
		suspiciousPlayerName = susNick;
		sendReport = true;
	}

	private IEnumerator CheckReports()
	{
		while (true)
		{
			try
			{
				logErrorCount = 0;
				if (PhotonNetwork.InRoom)
				{
					lastCheck = Time.time;
					lastServerTimestamp = PhotonNetwork.ServerTimestamp;
					if (currentMasterClient != PhotonNetwork.MasterClient || LowestActorNumber() != PhotonNetwork.MasterClient.ActorNumber)
					{
						Player[] playerList = PhotonNetwork.PlayerList;
						foreach (Player player in playerList)
						{
							if (currentMasterClient == player)
							{
								sendReport = true;
								suspiciousReason = "room host force changed";
								suspiciousPlayerId = PhotonNetwork.MasterClient.UserId;
								suspiciousPlayerName = PhotonNetwork.MasterClient.NickName;
							}
						}
						currentMasterClient = PhotonNetwork.MasterClient;
					}
					if (sendReport || testAssault)
					{
						if (suspiciousPlayerId != "" && reportedPlayers.IndexOf(suspiciousPlayerId) == -1)
						{
							reportedPlayers.Add(suspiciousPlayerId);
							testAssault = false;
							RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
							WebFlags flags = new WebFlags(1);
							raiseEventOptions.Flags = flags;
							string[] array = new string[PhotonNetwork.PlayerList.Length];
							int num = 0;
							Player[] playerList = PhotonNetwork.PlayerList;
							foreach (Player player2 in playerList)
							{
								array[num] = player2.UserId;
								num++;
							}
							if (ShouldDisconnectFromRoom())
							{
								StartCoroutine(QuitDelay());
							}
						}
						_sendReport = false;
						_suspiciousPlayerId = "";
						_suspiciousPlayerName = "";
						_suspiciousReason = "";
					}
					foreach (Dictionary<string, RPCCallTracker> item in userRPCCalls.Values.ToList())
					{
						foreach (RPCCallTracker item2 in item.Values.ToList())
						{
							item2.RPCCalls = 0;
						}
					}
				}
			}
			catch
			{
			}
			yield return new WaitForSeconds(1f);
		}
	}

	private int LowestActorNumber()
	{
		lowestActorNumber = PhotonNetwork.LocalPlayer.ActorNumber;
		Player[] playerList = PhotonNetwork.PlayerList;
		foreach (Player player in playerList)
		{
			if (player.ActorNumber < lowestActorNumber)
			{
				lowestActorNumber = player.ActorNumber;
			}
		}
		return lowestActorNumber;
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		base.OnPlayerLeftRoom(otherPlayer);
		if (userRPCCalls.TryGetValue(otherPlayer.UserId, out var _))
		{
			userRPCCalls.Remove(otherPlayer.UserId);
		}
	}

	public static void IncrementRPCCall(PhotonMessageInfo info, [CallerMemberName] string callingMethod = "")
	{
		instance.IncrementRPCCallLocal(info, callingMethod);
	}

	private void IncrementRPCCallLocal(PhotonMessageInfo info, string rpcFunction)
	{
		if (info.SentServerTimestamp >= lastServerTimestamp && !IncrementRPCTracker(in info.Sender, in rpcFunction, in rpcCallLimit))
		{
			SendReport("too many rpc calls! " + rpcFunction, info.Sender.UserId, info.Sender.NickName);
		}
	}

	private bool IncrementRPCTracker(in Player sender, in string rpcFunction, in int callLimit)
	{
		RPCCallTracker rPCCallTracker = GetRPCCallTracker(in sender, in rpcFunction);
		if (rPCCallTracker == null)
		{
			return true;
		}
		rPCCallTracker.RPCCalls++;
		if (rPCCallTracker.RPCCalls > rPCCallTracker.RPCCallsMax)
		{
			rPCCallTracker.RPCCallsMax = rPCCallTracker.RPCCalls;
		}
		if (rPCCallTracker.RPCCalls > callLimit)
		{
			return false;
		}
		return true;
	}

	private RPCCallTracker GetRPCCallTracker(in Player sender, in string rpcFunction)
	{
		if (sender == null || sender.UserId == null)
		{
			return null;
		}
		RPCCallTracker value = null;
		if (!userRPCCalls.TryGetValue(sender.UserId, out var value2))
		{
			value = new RPCCallTracker
			{
				RPCCalls = 0,
				RPCCallsMax = 0
			};
			Dictionary<string, RPCCallTracker> dictionary = new Dictionary<string, RPCCallTracker>();
			dictionary.Add(rpcFunction, value);
			userRPCCalls.Add(sender.UserId, dictionary);
		}
		else if (!value2.TryGetValue(rpcFunction, out value))
		{
			value = new RPCCallTracker
			{
				RPCCalls = 0,
				RPCCallsMax = 0
			};
			value2.Add(rpcFunction, value);
		}
		return value;
	}

	private IEnumerator QuitDelay()
	{
		yield return new WaitForSeconds(1f);
	}

	private void SetToRoomCreatorIfHere()
	{
		tempPlayer = PhotonNetwork.CurrentRoom.GetPlayer(1);
		if (tempPlayer != null)
		{
			suspiciousPlayerId = tempPlayer.UserId;
			suspiciousPlayerName = tempPlayer.NickName;
		}
		else
		{
			suspiciousPlayerId = "n/a";
			suspiciousPlayerName = "n/a";
		}
	}

	private bool ShouldDisconnectFromRoom()
	{
		if (!_suspiciousReason.Contains("too many players") && !_suspiciousReason.Contains("invalid room name"))
		{
			return _suspiciousReason.Contains("invalid game mode");
		}
		return true;
	}

	private void CloseInvalidRoom()
	{
		PhotonNetwork.CurrentRoom.IsOpen = false;
		PhotonNetwork.CurrentRoom.IsVisible = false;
	}
}
