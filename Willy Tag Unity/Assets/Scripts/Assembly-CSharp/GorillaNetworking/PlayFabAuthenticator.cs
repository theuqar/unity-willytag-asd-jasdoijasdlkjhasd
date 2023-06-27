using System;
using System.Collections.Generic;
using System.Text;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.Internal;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaNetworking
{
	public class PlayFabAuthenticator : MonoBehaviour
	{
		public static volatile PlayFabAuthenticator instance;

		public bool isTestAccount;

		public string testAccountName;

		public GorillaNetworkJoinTrigger testJoin;

		public string testRoomToJoin;

		public string testGameMode;

		public string _playFabPlayerIdCache;

		private string _displayName;

		public string userID;

		private string orgScopedID;

		private string userToken;

		public GorillaComputer gorillaComputer;

		private byte[] m_Ticket;

		private uint m_pcbTicket;

		public Text debugText;

		public bool screenDebugMode;

		public bool loginFailed;

		public GameObject emptyObject;

		private HAuthTicket m_HAuthTicket;

		private byte[] ticketBlob = new byte[1024];

		private uint ticketSize;

		public string expectedTitleID;

		public List<GameObject> Cosmetics;

		protected Callback<GetAuthSessionTicketResponse_t> m_GetAuthSessionTicketResponse;

		public void Awake()
		{
			PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime = "00bd3fdb-3656-42bc-b104-5f8d009ca4d9";
			PhotonNetwork.PhotonServerSettings.AppSettings.AppIdVoice = "828c624e-e87b-4119-81c7-2137e93f7b8e";
			PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion = "live1110";
			PlayFabSettings.TitleId = "3862D";
			if (instance == null)
			{
				instance = this;
			}
			else if (instance != this)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
			byte[] payload = new byte[1];
			PlayFabHttp.SimplePostCall("https://3862D.playfabapi.com/", payload, delegate
			{
			}, delegate
			{
			});
			if (screenDebugMode)
			{
				debugText.text = "";
			}
			Debug.Log("doing steam thing");
			OnGetAuthSessionTicketResponse();
			AuthenticateWithPlayFab();
			PlayFabSettings.DisableFocusTimeCollection = true;
		}

		public void AuthenticateWithPlayFab()
		{
			if (!loginFailed)
			{
				Debug.Log("authenticating with plafyab!");
				{
					Debug.Log("trying to auth with steam");
					m_HAuthTicket = SteamUser.GetAuthSessionTicket(ticketBlob, ticketBlob.Length, out ticketSize);
				}
			}
		}

		private void RequestPhotonToken(LoginResult obj)
		{
			LogMessage("PlayFab authenticated. Requesting photon token...");
			_playFabPlayerIdCache = obj.PlayFabId;
			PlayFabClientAPI.GetPhotonAuthenticationToken(new GetPhotonAuthenticationTokenRequest
			{
				PhotonApplicationId = PhotonNetwork.PhotonServerSettings.AppSettings.AppIdRealtime
			}, AuthenticateWithPhoton, OnPlayFabError);
		}

		private void AuthenticateWithPhoton(GetPhotonAuthenticationTokenResult obj)
		{
			LogMessage("Photon token acquired: " + obj.PhotonCustomAuthenticationToken + "  Authentication complete.");
			AuthenticationValues authenticationValues = new AuthenticationValues();
			authenticationValues.AuthType = CustomAuthenticationType.Custom;
			authenticationValues.AddAuthParameter("username", _playFabPlayerIdCache);
			authenticationValues.AddAuthParameter("token", obj.PhotonCustomAuthenticationToken);
			PhotonNetwork.AuthValues = authenticationValues;
			GetPlayerDisplayName(_playFabPlayerIdCache);
			PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
			{
				FunctionName = "AddOrRemoveDLCOwnership",
				FunctionParameter = new { }
			}, delegate
			{
				Debug.Log("got results! updating!");
				GorillaTagger.Instance.offlineVRRig.GetUserCosmeticsAllowed();
			}, delegate(PlayFabError error)
			{
				Debug.Log("Got error retrieving user data:");
				Debug.Log(error.GenerateErrorReport());
				GorillaTagger.Instance.offlineVRRig.GetUserCosmeticsAllowed();
			});
			if (CosmeticsController.instance != null)
			{
				Debug.Log("itinitalizing cosmetics");
				CosmeticsController.instance.Initialize();
			}
			gorillaComputer.OnConnectedToMasterStuff();
			PhotonNetworkController.instance.InitiateConnection();
		}

		private void OnPlayFabError(PlayFabError obj)
		{
			LogMessage(obj.ErrorMessage);
			Debug.Log(obj.ErrorMessage);
			loginFailed = true;
			if (obj.ErrorMessage == "The account making this request is currently banned")
			{
				using (Dictionary<string, List<string>>.Enumerator enumerator = obj.ErrorDetails.GetEnumerator())
				{
					if (enumerator.MoveNext())
					{
						KeyValuePair<string, List<string>> current = enumerator.Current;
						if (current.Value[0] != "Indefinite")
						{
							gorillaComputer.GeneralFailureMessage("YOU HAVE BEEN BANNED. YOU WILL NOT BE ABLE TO PLAY UNTIL THE BAN EXPIRES.\nREASON: " + current.Key + "\nHOURS LEFT: " + (int)((DateTime.Parse(current.Value[0]) - DateTime.UtcNow).TotalHours + 1.0));
						}
						else
						{
							gorillaComputer.GeneralFailureMessage("YOU HAVE BEEN BANNED INDEFINITELY.\nREASON: " + current.Key);
						}
					}
					return;
				}
			}
			if (obj.ErrorMessage == "The IP making this request is currently banned")
			{
				using (Dictionary<string, List<string>>.Enumerator enumerator2 = obj.ErrorDetails.GetEnumerator())
				{
					if (enumerator2.MoveNext())
					{
						KeyValuePair<string, List<string>> current2 = enumerator2.Current;
						if (current2.Value[0] != "Indefinite")
						{
							gorillaComputer.GeneralFailureMessage("THIS IP HAS BEEN BANNED. YOU WILL NOT BE ABLE TO PLAY UNTIL THE BAN EXPIRES.\nREASON: " + current2.Key + "\nHOURS LEFT: " + (int)((DateTime.Parse(current2.Value[0]) - DateTime.UtcNow).TotalHours + 1.0));
						}
						else
						{
							gorillaComputer.GeneralFailureMessage("THIS IP HAS BEEN BANNED INDEFINITELY.\nREASON: " + current2.Key);
						}
					}
					return;
				}
			}
			gorillaComputer.GeneralFailureMessage(gorillaComputer.unableToConnect);
		}

		public void LogMessage(string message)
		{
		}

		private void GetPlayerDisplayName(string playFabId)
		{
			PlayFabClientAPI.GetPlayerProfile(new GetPlayerProfileRequest
			{
				PlayFabId = playFabId,
				ProfileConstraints = new PlayerProfileViewConstraints
				{
					ShowDisplayName = true
				}
			}, delegate(GetPlayerProfileResult result)
			{
				_displayName = result.PlayerProfile.DisplayName;
			}, delegate(PlayFabError error)
			{
				Debug.LogError(error.GenerateErrorReport());
			});
		}

		public void SetDisplayName(string playerName)
		{
			if (_displayName == null || (_displayName.Length > 4 && _displayName.Substring(0, _displayName.Length - 4) != playerName))
			{
				PlayFabClientAPI.UpdateUserTitleDisplayName(new UpdateUserTitleDisplayNameRequest
				{
					DisplayName = playerName
				}, delegate
				{
					_displayName = playerName;
				}, delegate(PlayFabError error)
				{
					Debug.LogError(error.GenerateErrorReport());
				});
			}
		}

		public void ScreenDebug(string debugString)
		{
			Debug.Log(debugString);
			if (screenDebugMode)
			{
				Text text = debugText;
				text.text = text.text + debugString + "\n";
			}
		}

		public void ScreenDebugClear()
		{
			debugText.text = "";
		}

		public string GetSteamAuthTicket()
		{
			Array.Resize(ref ticketBlob, (int)ticketSize);
			StringBuilder stringBuilder = new StringBuilder();
			byte[] array = ticketBlob;
			foreach (byte b in array)
			{
				stringBuilder.AppendFormat("{0:x2}", b);
			}
			return stringBuilder.ToString();
		}

		private void OnGetAuthSessionTicketResponse()
		{
			PlayFabClientAPI.LoginWithCustomID(new LoginWithCustomIDRequest
			{
				CreateAccount = true,
				CustomId = PlayFabSettings.DeviceUniqueIdentifier
			}, RequestPhotonToken, OnPlayFabError);
		}
		void Start()
		{
			if (PlayFabSettings.TitleId != expectedTitleID)
			{
				Application.Quit();
			}
		}
	}
}
