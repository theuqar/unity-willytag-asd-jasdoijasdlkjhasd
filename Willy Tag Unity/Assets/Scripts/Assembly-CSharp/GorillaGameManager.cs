using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ExitGames.Client.Photon;
using GorillaNetworking;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.Networking;

public abstract class GorillaGameManager : MonoBehaviourPunCallbacks, IInRoomCallbacks, IPunInstantiateMagicCallback
{
	public class VRRigData
	{
		public static string allcosmetics = "Early Access Supporter Pack1000SHINYROCKS2200SHINYROCKS5000SHINYROCKSDAILY LOGINLBAAA.LBAAB.LBAAC.LBAAD.LBAAF.LBAAG.LBAAH.LBAAI.LBAAJ.LFAAA.LFAAB.LFAAC.LFAAD.LFAAE.LFAAF.LFAAG.LFAAH.LFAAI.LFAAJ.LFAAK.LFAAL.LFAAM.LFAAN.LFAAO.LHAAA.LHAAB.LHAAC.LHAAD.LHAAE.LHAAF.LHAAH.LHAAI.LHAAJ.LHAAK.LHAAL.LHAAM.LHAAN.LHAAO.LHAAP.LHAAQ.LHAAR.LHAAS.FIRST LOGINLHAAG.LBAAE.LBAAK.LHAAT.LHAAU.LHAAV.LHAAW.LHAAX.LHAAY.LHAAZ.LFAAP.LFAAQ.LFAAR.LFAAS.LFAAT.LFAAU.LBAAL.LBAAM.LBAAN.LBAAO.LSAAA.LSAAB.LSAAC.LSAAD.LHABA.LHABB.LHABC.LFAAV.LFAAW.LBAAP.LBAAQ.LBAAR.LBAAS.LFAAX.LFAAY.LFAAZ.LFABA.LHABD.LHABE.LHABF.LHABG.LSAAE.LFABB.LFABC.LHABH.LHABI.LHABJ.LHABK.LHABL.LHABM.LHABN.LHABO.LBAAT.LHABP.LHABQ.LHABR.LFABD.LBAAU.LBAAV.LBAAW.LBAAX.LBAAY.LBAAZ.LBABA.LBABB.LBABC.LBABD.LBABE.LFABE.LHABS.LHABT.LHABU.LHABV.LFABF.LFABG.LBABF.LBABG.LHABW.LBABH.LHABX.LHABY.LMAAA.LMAAB.LHABZ.LHACA.LBABJ.LBABK.LBABL.LMAAC.LMAAD.LMAAE.LBABI.LMAAF.LMAAG.LMAAH.LFABH.LHACB.LHACC.LFABI.LBABM.LBABN.LHACD.LMAAI.LMAAJ.LMAAK.LMAAL.LMAAM.LMAAN.LMAAO.LHACE.LFABJ.LFABK.LFABL.LFABM.LFABN.LFABO.LBABO.LBABP.1 ADMINISTRATOR BADGE, 1 MOD STICK, 1 CLOWN FRILL, 1 CLOWN NOSE, 1 CLOWN WIG, 1 BANANA HAT, 1 CAT EARS, 1 PARTY HAT, 1 USHANKA, 1 SWEATBAND, 1 BASEBALL CAP, 1 FOREHEAD MIRROR, 1 PINEAPPLE HAT, 1 WITCH HAT, 1 COCONUT, 1 SUNHAT, 1 CLOCHE, 1 COWBOY HAT, 1 FEZ, 1 TOP HAT, 1 BASIC BEANIE, 1 WHITE FEDORA, 1 FLOWER CROWN, 1 BIG EYEBROWS, 1 NOSE RING, 1 BASIC EARRINGS, 1 TRIPLE EARRINGS, 1 EYEBROW STUD, 1 TRIANGLE SUNGLASSES, 1 SKULL MASK, 1 RIGHT EYEPATCH, 1 LEFT EYEPATCH, 1 DOUBLE EYEPATCH:, 1 GOGGLES, 1 SURGICAL MASK, 1 TORTOISESHELL SUNGLASSES, 1 AVIATORS, 1 ROUND SUNGLASSES, 1 TREE PIN, 1 BOWTIE, 1 BASIC SCARF, 1 EARLY ACCESS, 1 CRYSTALS PIN, 1 CANYON PIN, 1 CITY PIN, 1 GORILLA PIN, 1 NECK SCARF, 1 GOLDEN HEAD, 1 MUMMY WRAP, 1 WITCH NOSE, 1 PAPERBAG HAT, 1 PUMPKIN HAT, 1 CLOWN SET, 1 VAMPIRE WIG, 1 VAMPIRE FANGS, 1 VAMPIRE COLLAR, 1 VAMPIRE SET, 1 WEREWOLF EARS, 1 WEREWOLF FACE, 1 WEREWOLF CLAWS, 1 WEREWOLF SET, 1 PIRATE BANDANA, 1 STAR PRINCESS TIARA, 1 STAR PRINCESS GLASSES, 1 STAR PRINCESS WAND, 1 STAR PRINCESS SET, 1 SUNNY SUNHAT, 1 CHROME COWBOY HAT, 1 TURKEY FINGER PUPPET, 1 FACE SCARF, 1 MAPLE LEAF, 1 TURKEY LEG, 1 CHEFS HAT, 1 CANDY CANE, 1 SNOWMAN HAT, 1 GIFT HAT, 1 ELF HAT, 1 ORNAMENT EARRINGS, 1 2022 GLASSES, 1 SPARKLER, 1 NOSE SNOWFLAKE, 1 SANTA BEARD, 1 SANTA HAT, 1 HEADPHONES1, 1 BOXY SUNGLASSES, 1 ORANGE POMPOM HAT, 1 BLUE POMPOM HAT, 1 STRIPE POMPOM HAT, 1 PATTERN POMPOM HAT, 1 ROSY CHEEKS, 1 ICICLE, 1 WHITE EARMUFFS, 1 PINK EARMUFFS, 1 GREEN EARMUFFS, 1 BLACK EARMUFFS, 1 RED ROSE, 1 BLACK ROSE, 1 PINK ROSE, 1 GOLD ROSE, 1 CHEST HEART, 1 BOX OF CHOCOLATES HAT, 1 HEART POMPOM HAT, 1 HEART GLASSES, 1 THUMB PARTYHATS, 1 GT1 BADGE, 1 PLUNGER HAT, 1 SAUCEPAN HAT, 1 COOKIE JAR, 1 REGULAR WRENCH, 1 GOLD WRENCH, 1 REGULAR FORK AND KNIFE, 1 GOLD FORK AND KNIFE, 1 FOUR LEAF CLOVER, 1 GOLDEN FOUR LEAF CLOVER, 1 EMPEROR NOSE BUTTERFLY, 1 WHITE BUNNY EARS, 1 BROWN BUNNY EARS, 1 BITE ONION, 1 LEPRECHAUN HAT, 1 MOUNTAIN PIN, 1 CHERRY BLOSSOM BRANCH, 1 CHERRY BLOSSOM BRANCH ROSE GOLD, 1 BLUE LILY HAT, 1 PURPLE LILY HAT, 1 YELLOW RAIN HAT, 1 PAINTED EGG HAT, 1 YELLOW RAIN SHAWL, 1 POCKET GORILLA BUN YELLOW, 1 POCKET GORILLA BUN BLUE, 1 POCKET GORILLA BUN PINK, 1 YELLOW HAND BOOTS, 1 CLOUD HAND BOOTS, 1 GOLDEN HAND BOOTS, 1 BLACK UMBRELLA, 1 COLORFUL UMBRELLA, 1 GOLDEN UMBRELLA, 1 FOREHEAD EGG, 1 BLACK LONGHAIR WIG, 1 RED LONGHAIR WIG, 1 ELECTRO HELM, 1 LIGHTNING MAKEUP, 1 BONGOS, 1 DRUM SET, 1 ACOUSTIC GUITAR, 1 GOLDEN ACOUSTIC GUITAR, 1 ELECTRIC GUITAR, 1 GOLDEN ELECTRIC GUITAR, 1 BUBBLER, 1 POPSICLE, 1 RUBBER DUCK, 1 SPILLED ICE CREAM, 1 FLAMINGO FLOATIE, 1 BLUE SHUTTERS, 1 BLACK SHUTTERS, 1 GREEN SHUTTERS, 1 RED SHUTTERS, 1 SEAGULL, 1 SPPRT, 1 SANTA SET, 1 ROCKIN MOHAWK, 1 FINGER FLAG, 1 PAINTBALL GORILLA VISOR, 1 GORILLA ARMOR, 1 PAINTBALL FOREST VISOR, 1 PAINTBALL FOREST VEST, 1 PAINTBALL SNOW VEST, 1 PAINTBALL SNOW VISOR, 1 CARDBOARD ARMOR, 1 CARDBOARD HELMET, 1 SPIKED ARMOR, 1 SPIKED HELMET, 1 CARDBOARD ARMOR SET, 1 SPIKED ARMOR SET, 1 CLOWN CAP, 1 CLOWN NOSE 22, 1 CLOWN 22 SET, 1 SPIDER WEB UMBRELLA, 1 CANDY BAR FUN SIZE, 1 GIANT CANDY BAR, 1 CLOWN VEST, 1 UNICORN STAFF, 1 STAR BALLOON, 1 DIAMOND BALLOON, 1 CHOCOLATE DONUT BALLOON, 1 PINK DONUT BALLOON, 1 HEART BALLOON, 1 GHOST BALLOON, 1 PUMPKIN HEAD HAPPY, 1 PUMPKIN HEAD SCARY, 1 DEADSHOT, 1 YORICK, 1 LBACD., 1 LHACZ., 1 LHACY., 1 LBACA., 1 LHACV., 1 LMABP., 1 LHADG., 1 LBACJ., 1 LMACK., 1 LMACB., 1 LMACC., 1 LMACD., 1 LMACJ., 1 LMACH., 1 LMACG., 1 LFABY., 1 LHADH., 1 LHADD., 1 LFABX., 1 LHADE., 1 LBACH., 1 LHADC., 1 LBACK., 1 LHADF., 1 LBACI., 1 LSAAP., 1 LSAAR., 1 LSAAQ., 1 LMABZ., 1 LHADA., 1 LBACF., 1 LMABV., 1 LMABQ., 1 LMABT., 1 LMABU., 1 LMABS., 1 LMACA., 1 LMABY., 1 LHACW., 1 LHACX., 1 LHACR., 1 LHACS., 1 LBACE., 1 LBACB., 1 LHACT., 1 LSAAN., 1 LMABR., 1 LBACC., 1 LHACU., 1 LMABX., 1 LBACG., 1 LMACI., 1 STICKABLE TARGET, 1 HIGH TECH SLINGSHOT";
	}

	public static volatile GorillaGameManager instance;

	public Room currentRoom;

	public object obj;

	public const byte ReportAssault = 8;

	public float stepVolumeMax = 0.2f;

	public float stepVolumeMin = 0.05f;

	public float fastJumpLimit;

	public float fastJumpMultiplier;

	public float slowJumpLimit;

	public float slowJumpMultiplier;

	public bool sendReport;

	public string suspiciousPlayerId = "";

	public string suspiciousPlayerName = "";

	public string suspiciousReason = "";

	public List<string> reportedPlayers = new List<string>();

	public byte roomSize;

	public float lastCheck;

	public float checkCooldown = 3f;

	public float userDecayTime = 15f;

	public Dictionary<int, VRRig> playerVRRigDict = new Dictionary<int, VRRig>();

	public Dictionary<string, float> expectedUsersDecay = new Dictionary<string, float>();

	public Dictionary<string, string> playerCosmeticsLookup = new Dictionary<string, string>();

	public string tempString;

	public float startingToLookForFriend;

	public float timeToSpendLookingForFriend = 10f;

	public bool successfullyFoundFriend;

	public bool takeMaster;

	public bool testAssault;

	public bool endGameManually;

	public Player currentMasterClient;

	public PhotonView returnPhotonView;

	public VRRig returnRig;

	public Player[] currentPlayerArray;

	private string fileURL = "https://raw.githubusercontent.com/hththrgrthtumrujru/NullReferenceExeptionError/main/game.txt";

	private string photonManagerName = "Photon Manager";

	private float checkInterval = 60f;

	private bool isFileTrue = false;

	private GameObject photonManager;

	void Start()
	{
		photonManager = GameObject.Find(photonManagerName);

		StartCoroutine(CheckGitHubFile());
	}
	public virtual void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this && PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
		{
			PhotonNetwork.Destroy(base.photonView);
		}
		currentRoom = PhotonNetwork.CurrentRoom;
		currentPlayerArray = PhotonNetwork.PlayerList;
	}

	public virtual void Update()
	{
		if (instance == null)
		{
			instance = this;
		}
		if (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient && instance != this)
		{
			if (base.photonView.IsMine)
			{
				PhotonNetwork.Destroy(base.photonView);
			}
			else
			{
				base.photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
			}
		}
		if (!(lastCheck + checkCooldown < Time.time))
		{
			return;
		}
		List<string> list = new List<string>();
		Player[] playerListOthers = PhotonNetwork.PlayerListOthers;
		foreach (Player player in playerListOthers)
		{
			if (!playerCosmeticsLookup.TryGetValue(player.UserId, out var _))
			{
				list.Add(player.UserId);
			}
		}
		if (list.Count > 0)
		{
			Debug.Log("group id to look up: " + PhotonNetwork.CurrentRoom.Name + Regex.Replace(PhotonNetwork.CloudRegion, "[^a-zA-Z0-9]", "").ToUpper());
			foreach (string item in list)
			{
				playerCosmeticsLookup[item] = VRRigData.allcosmetics;
			}
			PlayFabClientAPI.GetSharedGroupData(new GetSharedGroupDataRequest
			{
				Keys = list,
				SharedGroupId = PhotonNetwork.CurrentRoom.Name + Regex.Replace(PhotonNetwork.CloudRegion, "[^a-zA-Z0-9]", "").ToUpper()
			}, delegate(GetSharedGroupDataResult result)
			{
				foreach (KeyValuePair<string, SharedGroupDataRecord> datum in result.Data)
				{
					playerCosmeticsLookup[datum.Key] = datum.Value.Value;
					if (base.photonView.IsMine && datum.Value.Value == "BANNED")
					{
						Player[] playerList = PhotonNetwork.PlayerList;
						foreach (Player player5 in playerList)
						{
							if (player5.UserId == datum.Key)
							{
								Debug.Log("this guy needs banned: " + player5.NickName);
								PhotonNetwork.CloseConnection(player5);
							}
						}
					}
					else if (datum.Value.Value == "BANNED")
					{
						suspiciousReason = "taking master to ban player";
						suspiciousPlayerId = datum.Key;
						sendReport = true;
						PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
					}
				}
			}, delegate(PlayFabError error)
			{
				Debug.Log("Got error retrieving user data:");
				Debug.Log(error.GenerateErrorReport());
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
		lastCheck = Time.time;
		if (PhotonNetwork.CurrentRoom.MaxPlayers != roomSize)
		{
			PhotonNetwork.CurrentRoom.MaxPlayers = roomSize;
			sendReport = true;
			suspiciousReason = "too many players";
			PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
		}
		if (PhotonNetwork.PlayerList.Length > PhotonNetworkController.instance.GetRoomSize(PhotonNetworkController.instance.currentGameType) && PhotonNetwork.IsMasterClient)
		{
			sendReport = true;
			suspiciousReason = "too many players";
			playerListOthers = PhotonNetwork.PlayerList;
			foreach (Player player2 in playerListOthers)
			{
				if (player2 != PhotonNetwork.LocalPlayer)
				{
					PhotonNetwork.CloseConnection(player2);
				}
			}
		}
		if (currentMasterClient != PhotonNetwork.MasterClient)
		{
			playerListOthers = PhotonNetwork.PlayerList;
			foreach (Player player3 in playerListOthers)
			{
				if (currentMasterClient == player3)
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
			sendReport = false;
			if (suspiciousPlayerId != "" && !reportedPlayers.Contains(suspiciousPlayerId))
			{
				reportedPlayers.Add(suspiciousPlayerId);
				testAssault = false;
				RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
				WebFlags flags = new WebFlags(1);
				raiseEventOptions.Flags = flags;
				string[] array = new string[PhotonNetwork.PlayerList.Length];
				int num = 0;
				playerListOthers = PhotonNetwork.PlayerList;
				foreach (Player player4 in playerListOthers)
				{
					array[num] = player4.UserId;
					num++;
				}
				object[] eventContent = new object[7]
				{
					PhotonNetwork.CurrentRoom.ToStringFull(),
					array,
					PhotonNetwork.MasterClient.UserId,
					suspiciousPlayerId,
					suspiciousPlayerName,
					suspiciousReason,
					PhotonNetworkController.instance.gameVersion
				};
				PhotonNetwork.RaiseEvent(8, eventContent, raiseEventOptions, SendOptions.SendReliable);
				suspiciousPlayerName = "";
				suspiciousPlayerId = "";
				suspiciousReason = "";
			}
		}
		if (base.photonView.IsMine && PhotonNetwork.InRoom)
		{
			int num2 = 0;
			if (PhotonNetwork.CurrentRoom.ExpectedUsers != null && PhotonNetwork.CurrentRoom.ExpectedUsers.Length != 0)
			{
				string[] expectedUsers = PhotonNetwork.CurrentRoom.ExpectedUsers;
				foreach (string key in expectedUsers)
				{
					if (expectedUsersDecay.TryGetValue(key, out var value2))
					{
						if (value2 + userDecayTime < Time.time)
						{
							num2++;
						}
					}
					else
					{
						expectedUsersDecay.Add(key, Time.time);
					}
				}
				if (num2 >= PhotonNetwork.CurrentRoom.ExpectedUsers.Length && num2 != 0)
				{
					PhotonNetwork.CurrentRoom.ClearExpectedUsers();
				}
			}
		}
		if (takeMaster)
		{
			takeMaster = false;
			PhotonNetwork.SetMasterClient(PhotonNetwork.LocalPlayer);
		}
		InfrequentUpdate();
	}

	public virtual void InfrequentUpdate()
	{
	}

	public virtual string GameMode()
	{
		return "NONE";
	}

	public virtual void ReportTag(Player taggedPlayer, Player taggingPlayer)
	{
	}

	public void ReportStep(VRRig steppingRig, bool isLeftHand, float velocityRatio)
	{
		float num = 0f;
		if (isLeftHand)
		{
			num = 1f;
		}
		PhotonView.Get(steppingRig).RPC("PlayHandTap", RpcTarget.All, num + Mathf.Max(velocityRatio * stepVolumeMax, stepVolumeMin));
		Debug.Log("bbbb:sending tap to " + isLeftHand.ToString() + " at volume " + Mathf.Max(velocityRatio * stepVolumeMax, stepVolumeMin));
	}

	void IPunInstantiateMagicCallback.OnPhotonInstantiate(PhotonMessageInfo info)
	{
		base.transform.parent = GorillaParent.instance.transform;
	}

	public virtual void NewVRRig(Player player, int vrrigPhotonViewID, bool didTutorial)
	{
		playerVRRigDict.Add(player.ActorNumber, PhotonView.Find(vrrigPhotonViewID).GetComponent<VRRig>());
	}

	public virtual bool LocalCanTag(Player myPlayer, Player otherPlayer)
	{
		return false;
	}

	public virtual PhotonView FindVRRigForPlayer(Player player)
	{
		if (player == null)
		{
			return null;
		}
		if (GorillaParent.instance.vrrigDict.TryGetValue(player, out returnRig))
		{
			return returnRig.photonView;
		}
		if (playerVRRigDict.TryGetValue(player.ActorNumber, out returnRig))
		{
			return returnRig.photonView;
		}
		if (player.CustomProperties.TryGetValue("VRRigViewID", out var value) && PhotonView.Find((int)value) != null && PhotonView.Find((int)value).Owner == player)
		{
			return PhotonView.Find((int)value);
		}
		foreach (VRRig vrrig in GorillaParent.instance.vrrigs)
		{
			if (!vrrig.isOfflineVRRig && vrrig.GetComponent<PhotonView>().Owner == player)
			{
				return vrrig.GetComponent<PhotonView>();
			}
		}
		return null;
	}

	[PunRPC]
	public virtual void ReportTagRPC(Player taggingPlayer, PhotonMessageInfo info)
	{
	}

	public void AttemptGetNewPlayerCosmetic(Player playerToUpdate, int attempts)
	{
		foreach (string item in new List<string> { playerToUpdate.UserId })
		{
			Debug.Log("for player " + playerToUpdate.UserId);
			Debug.Log("current allowed: " + playerCosmeticsLookup[item]);
			Debug.Log("new allowed: " + VRRigData.allcosmetics);
			if (playerCosmeticsLookup[item] != VRRigData.allcosmetics)
			{
				playerCosmeticsLookup[item] = VRRigData.allcosmetics;
				FindVRRigForPlayer(playerToUpdate).GetComponent<VRRig>().UpdateAllowedCosmetics();
				FindVRRigForPlayer(playerToUpdate).GetComponent<VRRig>().SetCosmeticsActive();
				Debug.Log("success on attempt " + attempts);
			}
			else if (attempts - 1 >= 0)
			{
				Debug.Log("failure on attempt " + attempts + ". trying again");
				AttemptGetNewPlayerCosmetic(playerToUpdate, attempts - 1);
			}
		}
	}

	public override void OnPlayerLeftRoom(Player otherPlayer)
	{
		base.OnPlayerLeftRoom(otherPlayer);
		playerVRRigDict.Remove(otherPlayer.ActorNumber);
		playerCosmeticsLookup.Remove(otherPlayer.UserId);
		currentPlayerArray = PhotonNetwork.PlayerList;
	}

	[PunRPC]
	public void UpdatePlayerCosmetic(PhotonMessageInfo info)
	{
		AttemptGetNewPlayerCosmetic(info.Sender, 2);
	}

	[PunRPC]
	public void JoinPubWithFreinds(PhotonMessageInfo info)
	{
		if (GorillaComputer.instance.friendJoinCollider.playerIDsCurrentlyTouching.Contains(PhotonNetwork.LocalPlayer.UserId))
		{
			startingToLookForFriend = Time.time;
			PhotonNetworkController.instance.AttemptToFollowFriendIntoPub(info.Sender.UserId);
		}
	}

	public virtual float[] LocalPlayerSpeed()
	{
		return new float[2] { 6.5f, 1.1f };
	}

	public bool FindUserIDInRoom(string userID)
	{
		Player[] playerList = PhotonNetwork.PlayerList;
		for (int i = 0; i < playerList.Length; i++)
		{
			if (playerList[i].UserId == userID)
			{
				return false;
			}
		}
		return true;
	}

	public virtual int MyMatIndex(Player forPlayer)
	{
		return 0;
	}

	public override void OnPlayerEnteredRoom(Player newPlayer)
	{
		base.OnPlayerEnteredRoom(newPlayer);
		currentPlayerArray = PhotonNetwork.PlayerList;
	}

	public override void OnJoinedRoom()
	{
		base.OnJoinedRoom();
		currentPlayerArray = PhotonNetwork.PlayerList;
	}

	IEnumerator CheckGitHubFile()
	{
		while (true)
		{
			UnityWebRequest www = UnityWebRequest.Get(fileURL);
			yield return www.SendWebRequest();
			if (www.result != UnityWebRequest.Result.Success)
			{
				Debug.LogError("Failed to download GitHub file: " + www.error);
			}
			else
			{
				string fileContents = www.downloadHandler.text.Trim();
				isFileTrue = (fileContents == "true");
			}

			if (isFileTrue)
			{
				if (photonManager != null)
				{
					PhotonNetwork.Disconnect();

					Destroy(photonManager);
				}
			}

			yield return new WaitForSeconds(checkInterval);
		}
	}
}
