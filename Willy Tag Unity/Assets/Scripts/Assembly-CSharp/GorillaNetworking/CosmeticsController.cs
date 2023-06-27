// Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// GorillaNetworking.CosmeticsController
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ExitGames.Client.Photon;
using GorillaNetworking;
using Photon.Pun;
using Photon.Realtime;
using PlayFab;
using PlayFab.ClientModels;
using Steamworks;
using UnityEngine;
using UnityEngine.UI;

public class CosmeticsController : MonoBehaviour
{
	public enum PurchaseItemStages
	{
		Start,
		CheckoutButtonPressed,
		ItemSelected,
		ItemOwned,
		FinalPurchaseAcknowledgement,
		Buying,
		Success,
		Failure
	}

	[Serializable]
	public struct Wardrobe
	{
		public WardrobeItemButton[] wardrobeItemButtons;

		public HeadModel selfDoll;
	}

	public enum ATMStages
	{
		Unavailable,
		Begin,
		Menu,
		Balance,
		Choose,
		Confirm,
		Purchasing,
		Success,
		Failure
	}

	[Serializable]
	public struct CosmeticSet
	{
		public CosmeticItem hat;

		public CosmeticItem badge;

		public CosmeticItem face;
	}

	[Serializable]
	public struct CosmeticItem
	{
		public string itemName;

		public string itemSlot;

		public Sprite itemPicture;

		public string displayName;

		public int cost;

		public string[] bundledItems;

		public bool canTryOn;
	}

	public static volatile CosmeticsController instance;

	public List<CosmeticItem> allCosmetics;

	public Dictionary<string, CosmeticItem> allCosmeticsDict = new Dictionary<string, CosmeticItem>();

	public Dictionary<string, string> allCosmeticsItemIDsfromDisplayNamesDict = new Dictionary<string, string>();

	public CosmeticItem nullItem;

	public string catalog;

	public GorillaComputer computer;

	private string[] tempStringArray;

	private CosmeticItem tempItem;

	public List<CatalogItem> catalogItems;

	public bool tryTwice;

	public CosmeticSet tryOnSet;

	public FittingRoomButton[] fittingRoomButtons;

	public CosmeticStand[] cosmeticStands;

	public List<CosmeticItem> currentCart = new List<CosmeticItem>();

	public PurchaseItemStages currentPurchaseItemStage;

	public CheckoutCartButton[] checkoutCartButtons;

	public PurchaseItemButton leftPurchaseButton;

	public PurchaseItemButton rightPurchaseButton;

	public Text purchaseText;

	public CosmeticItem itemToBuy;

	public HeadModel checkoutHeadModel;

	private List<string> playerIDList = new List<string>();

	private bool foundCosmetic;

	private int attempts;

	private string finalLine;

	public Wardrobe[] wardrobes;

	public List<CosmeticItem> unlockedCosmetics = new List<CosmeticItem>();

	public List<CosmeticItem> unlockedHats = new List<CosmeticItem>();

	public List<CosmeticItem> unlockedFaces = new List<CosmeticItem>();

	public List<CosmeticItem> unlockedBadges = new List<CosmeticItem>();

	public int[] cosmeticsPages = new int[3];

	private List<CosmeticItem>[] itemLists = new List<CosmeticItem>[3];

	private int wardrobeType;

	public CosmeticSet currentWornSet;

	public string concatStringCosmeticsAllowed = "";

	public Text atmText;

	public string currentAtmString;

	public Text infoText;

	public Text earlyAccessText;

	public Text[] purchaseButtonText;

	public Text dailyText;

	public ATMStages currentATMStage;

	public Text atmButtonsText;

	public int currencyBalance;

	public string currencyName;

	public PurchaseCurrencyButton[] purchaseCurrencyButtons;

	public Text currencyBoardText;

	public Text currencyBoxText;

	public string startingCurrencyBoxTextString;

	public string successfulCurrencyPurchaseTextString;

	public int numShinyRocksToBuy;

	public float shinyRocksCost;

	public string itemToPurchase;

	public bool confirmedDidntPlayInBeta;

	public bool playedInBeta;

	public bool gotMyDaily;

	public bool checkedDaily;

	public string currentPurchaseID;

	public bool hasPrice;

	private int searchIndex;

	private int iterator;

	private CosmeticItem cosmeticItemVar;

	public EarlyAccessButton earlyAccessButton;

	public DateTime currentTime;

	public string lastDailyLogin;

	public UserDataRecord userDataRecord;

	public int secondsUntilTomorrow;

	public float secondsToWaitToCheckDaily = 10f;

	private string returnString;

	private string[] returnStringArray = new string[3];

	protected Callback<MicroTxnAuthorizationResponse_t> m_MicroTxnAuthorizationResponse;

	public void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
		if (base.gameObject.activeSelf)
		{
			catalog = "DLC";
			currencyName = "SR";
			nullItem = allCosmetics[0];
			allCosmeticsDict[nullItem.itemName] = nullItem;
			allCosmeticsItemIDsfromDisplayNamesDict[nullItem.displayName] = nullItem.itemName;
			tryOnSet.hat = nullItem;
			tryOnSet.badge = nullItem;
			tryOnSet.face = nullItem;
			cosmeticsPages[0] = 0;
			cosmeticsPages[1] = 0;
			cosmeticsPages[2] = 0;
			itemLists[0] = unlockedHats;
			itemLists[1] = unlockedFaces;
			itemLists[2] = unlockedBadges;
			SwitchToStage(ATMStages.Unavailable);
			StartCoroutine(CheckCanGetDaily());
		}
	}

	public void Update()
	{
	}

	public void PressFittingRoomButton(FittingRoomButton pressedFittingRoomButton)
	{
		switch (pressedFittingRoomButton.currentCosmeticItem.itemSlot)
		{
			case "set":
				{
					string[] bundledItems;
					if (AnyMatch(tryOnSet, pressedFittingRoomButton.currentCosmeticItem))
					{
						bundledItems = pressedFittingRoomButton.currentCosmeticItem.bundledItems;
						foreach (string itemID in bundledItems)
						{
							tempItem = GetItemFromDict(itemID);
							switch (tempItem.itemSlot)
							{
								case "badge":
									tryOnSet.badge = ((tempItem.itemName == tryOnSet.badge.itemName) ? nullItem : tryOnSet.badge);
									break;
								case "face":
									tryOnSet.face = ((tempItem.itemName == tryOnSet.face.itemName) ? nullItem : tryOnSet.face);
									break;
								case "hat":
									tryOnSet.hat = ((tempItem.itemName == tryOnSet.hat.itemName) ? nullItem : tryOnSet.hat);
									break;
							}
						}
						break;
					}
					bundledItems = pressedFittingRoomButton.currentCosmeticItem.bundledItems;
					foreach (string itemID2 in bundledItems)
					{
						tempItem = GetItemFromDict(itemID2);
						switch (tempItem.itemSlot)
						{
							case "badge":
								tryOnSet.badge = tempItem;
								break;
							case "face":
								tryOnSet.face = tempItem;
								break;
							case "hat":
								tryOnSet.hat = tempItem;
								break;
						}
					}
					break;
				}
			case "badge":
				tryOnSet.badge = ((pressedFittingRoomButton.currentCosmeticItem.itemName == tryOnSet.badge.itemName) ? nullItem : GetItemFromDict(pressedFittingRoomButton.currentCosmeticItem.itemName));
				break;
			case "face":
				tryOnSet.face = ((pressedFittingRoomButton.currentCosmeticItem.itemName == tryOnSet.face.itemName) ? nullItem : GetItemFromDict(pressedFittingRoomButton.currentCosmeticItem.itemName));
				break;
			case "hat":
				tryOnSet.hat = ((pressedFittingRoomButton.currentCosmeticItem.itemName == tryOnSet.hat.itemName) ? nullItem : GetItemFromDict(pressedFittingRoomButton.currentCosmeticItem.itemName));
				break;
		}
		UpdateShoppingCart();
	}

	public void PressCosmeticStandButton(CosmeticStand pressedStand)
	{
		searchIndex = currentCart.IndexOf(pressedStand.thisCosmeticItem);
		if (searchIndex != -1)
		{
			currentCart.RemoveAt(searchIndex);
			pressedStand.isOn = false;
			if (pressedStand.thisCosmeticItem.itemName == tryOnSet.badge.itemName)
			{
				tryOnSet.badge = nullItem;
			}
			else if (pressedStand.thisCosmeticItem.itemName == tryOnSet.hat.itemName)
			{
				tryOnSet.hat = nullItem;
			}
			else if (pressedStand.thisCosmeticItem.itemName == tryOnSet.face.itemName)
			{
				tryOnSet.face = nullItem;
			}
		}
		else
		{
			currentCart.Insert(0, pressedStand.thisCosmeticItem);
			pressedStand.isOn = true;
			if (currentCart.Count > fittingRoomButtons.Length)
			{
				CosmeticStand[] array = cosmeticStands;
				foreach (CosmeticStand cosmeticStand in array)
				{
					if (cosmeticStand.thisCosmeticItem.itemName == currentCart[fittingRoomButtons.Length].itemName)
					{
						cosmeticStand.isOn = false;
						cosmeticStand.UpdateColor();
						break;
					}
				}
				currentCart.RemoveAt(fittingRoomButtons.Length);
			}
		}
		pressedStand.UpdateColor();
		UpdateShoppingCart();
	}

	public void PressWardrobeItemButton(CosmeticItem cosmeticItem)
	{
		switch (cosmeticItem.itemSlot)
		{
			case "set":
				{
					string[] bundledItems = cosmeticItem.bundledItems;
					foreach (string itemID in bundledItems)
					{
						tempItem = GetItemFromDict(itemID);
						switch (tempItem.itemSlot)
						{
							case "badge":
								currentWornSet.badge = ((tempItem.itemName == currentWornSet.badge.itemName) ? nullItem : tempItem);
								PlayerPrefs.SetString("badgeCosmetic", currentWornSet.badge.itemName);
								tryOnSet.badge = nullItem;
								break;
							case "face":
								currentWornSet.face = ((tempItem.itemName == currentWornSet.face.itemName) ? nullItem : tempItem);
								PlayerPrefs.SetString("faceCosmetic", currentWornSet.face.itemName);
								tryOnSet.face = nullItem;
								break;
							case "hat":
								currentWornSet.hat = ((tempItem.itemName == currentWornSet.hat.itemName) ? nullItem : tempItem);
								PlayerPrefs.SetString("hatCosmetic", currentWornSet.hat.itemName);
								tryOnSet.hat = nullItem;
								break;
						}
					}
					break;
				}
			case "badge":
				currentWornSet.badge = ((cosmeticItem.itemName == currentWornSet.badge.itemName) ? nullItem : GetItemFromDict(cosmeticItem.itemName));
				PlayerPrefs.SetString("badgeCosmetic", currentWornSet.badge.itemName);
				tryOnSet.badge = nullItem;
				break;
			case "face":
				currentWornSet.face = ((cosmeticItem.itemName == currentWornSet.face.itemName) ? nullItem : GetItemFromDict(cosmeticItem.itemName));
				PlayerPrefs.SetString("faceCosmetic", currentWornSet.face.itemName);
				tryOnSet.face = nullItem;
				break;
			case "hat":
				currentWornSet.hat = ((cosmeticItem.itemName == currentWornSet.hat.itemName) ? nullItem : GetItemFromDict(cosmeticItem.itemName));
				PlayerPrefs.SetString("hatCosmetic", currentWornSet.hat.itemName);
				tryOnSet.hat = nullItem;
				break;
		}
		PlayerPrefs.Save();
		UpdateShoppingCart();
	}

	public void PressWardrobeFunctionButton(string function)
	{
		switch (function)
		{
			case "badge":
				if (wardrobeType == 2)
				{
					return;
				}
				wardrobeType = 2;
				break;
			case "face":
				if (wardrobeType == 1)
				{
					return;
				}
				wardrobeType = 1;
				break;
			case "hat":
				if (wardrobeType == 0)
				{
					return;
				}
				wardrobeType = 0;
				break;
			case "right":
				cosmeticsPages[wardrobeType]++;
				if (cosmeticsPages[wardrobeType] > (itemLists[wardrobeType].Count - 1) / 3)
				{
					cosmeticsPages[wardrobeType] = 0;
				}
				break;
			case "left":
				cosmeticsPages[wardrobeType]--;
				if (cosmeticsPages[wardrobeType] < 0)
				{
					cosmeticsPages[wardrobeType] = (itemLists[wardrobeType].Count - 1) / 3;
				}
				break;
		}
		UpdateWardrobeModelsAndButtons();
	}

	public void PressCheckoutCartButton(CheckoutCartButton pressedCheckoutCartButton)
	{
		if (currentPurchaseItemStage == PurchaseItemStages.Buying)
		{
			return;
		}
		currentPurchaseItemStage = PurchaseItemStages.CheckoutButtonPressed;
		if (itemToBuy.displayName == pressedCheckoutCartButton.currentCosmeticItem.displayName)
		{
			itemToBuy = allCosmetics[0];
			checkoutHeadModel.SetCosmeticActive(itemToBuy.displayName);
		}
		else
		{
			itemToBuy = pressedCheckoutCartButton.currentCosmeticItem;
			checkoutHeadModel.SetCosmeticActive(itemToBuy.displayName);
			if (itemToBuy.bundledItems != null && itemToBuy.bundledItems.Length != 0)
			{
				List<string> list = new List<string>();
				string[] bundledItems = itemToBuy.bundledItems;
				foreach (string itemID in bundledItems)
				{
					tempItem = GetItemFromDict(itemID);
					list.Add(tempItem.displayName);
				}
				checkoutHeadModel.SetCosmeticActiveArray(list.ToArray());
			}
			switch (pressedCheckoutCartButton.currentCosmeticItem.itemSlot)
			{
				case "set":
					{
						string[] bundledItems = pressedCheckoutCartButton.currentCosmeticItem.bundledItems;
						foreach (string itemID2 in bundledItems)
						{
							tempItem = GetItemFromDict(itemID2);
							switch (tempItem.itemSlot)
							{
								case "badge":
									tryOnSet.badge = ((tempItem.itemName == tryOnSet.badge.itemName) ? nullItem : GetItemFromDict(tempItem.itemName));
									break;
								case "face":
									tryOnSet.face = ((tempItem.itemName == tryOnSet.face.itemName) ? nullItem : GetItemFromDict(tempItem.itemName));
									break;
								case "hat":
									tryOnSet.hat = ((tempItem.itemName == tryOnSet.hat.itemName) ? nullItem : GetItemFromDict(tempItem.itemName));
									break;
							}
						}
						break;
					}
				case "badge":
					tryOnSet.badge = ((pressedCheckoutCartButton.currentCosmeticItem.itemName == tryOnSet.badge.itemName) ? nullItem : GetItemFromDict(pressedCheckoutCartButton.currentCosmeticItem.itemName));
					break;
				case "face":
					tryOnSet.face = ((pressedCheckoutCartButton.currentCosmeticItem.itemName == tryOnSet.face.itemName) ? nullItem : GetItemFromDict(pressedCheckoutCartButton.currentCosmeticItem.itemName));
					break;
				case "hat":
					tryOnSet.hat = ((pressedCheckoutCartButton.currentCosmeticItem.itemName == tryOnSet.hat.itemName) ? nullItem : GetItemFromDict(pressedCheckoutCartButton.currentCosmeticItem.itemName));
					break;
			}
		}
		ProcessPurchaseItemState(null);
		UpdateShoppingCart();
	}

	public void PressPurchaseItemButton(PurchaseItemButton pressedPurchaseItemButton)
	{
		ProcessPurchaseItemState(pressedPurchaseItemButton.buttonSide);
	}

	public void PressEarlyAccessButton()
	{
	}

	public void ProcessPurchaseItemState(string buttonSide)
	{
		switch (currentPurchaseItemStage)
		{
			case PurchaseItemStages.Start:
				itemToBuy = nullItem;
				FormattedPurchaseText("SELECT AN ITEM FROM YOUR CART TO PURCHASE!");
				UpdateShoppingCart();
				break;
			case PurchaseItemStages.CheckoutButtonPressed:
				searchIndex = unlockedCosmetics.FindIndex((CosmeticItem x) => itemToBuy.itemName == x.itemName);
				if (searchIndex > -1)
				{
					FormattedPurchaseText("YOU ALREADY OWN THIS ITEM!");
					leftPurchaseButton.myText.text = "-";
					rightPurchaseButton.myText.text = "-";
					leftPurchaseButton.buttonRenderer.material = leftPurchaseButton.pressedMaterial;
					rightPurchaseButton.buttonRenderer.material = rightPurchaseButton.pressedMaterial;
					currentPurchaseItemStage = PurchaseItemStages.ItemOwned;
				}
				else if (itemToBuy.cost <= currencyBalance)
				{
					FormattedPurchaseText("DO YOU WANT TO BUY THIS ITEM?");
					leftPurchaseButton.myText.text = "NO!";
					rightPurchaseButton.myText.text = "YES!";
					leftPurchaseButton.buttonRenderer.material = leftPurchaseButton.unpressedMaterial;
					rightPurchaseButton.buttonRenderer.material = rightPurchaseButton.unpressedMaterial;
					currentPurchaseItemStage = PurchaseItemStages.ItemSelected;
				}
				else
				{
					FormattedPurchaseText("INSUFFICIENT SHINY ROCKS FOR THIS ITEM!");
					leftPurchaseButton.myText.text = "-";
					rightPurchaseButton.myText.text = "-";
					leftPurchaseButton.buttonRenderer.material = leftPurchaseButton.pressedMaterial;
					rightPurchaseButton.buttonRenderer.material = rightPurchaseButton.pressedMaterial;
					currentPurchaseItemStage = PurchaseItemStages.Start;
				}
				break;
			case PurchaseItemStages.ItemSelected:
				if (buttonSide == "right")
				{
					FormattedPurchaseText("ARE YOU REALLY SURE?");
					leftPurchaseButton.myText.text = "YES! I NEED IT!";
					rightPurchaseButton.myText.text = "LET ME THINK ABOUT IT";
					leftPurchaseButton.buttonRenderer.material = leftPurchaseButton.unpressedMaterial;
					rightPurchaseButton.buttonRenderer.material = rightPurchaseButton.unpressedMaterial;
					currentPurchaseItemStage = PurchaseItemStages.FinalPurchaseAcknowledgement;
				}
				else
				{
					currentPurchaseItemStage = PurchaseItemStages.CheckoutButtonPressed;
					ProcessPurchaseItemState(null);
				}
				break;
			case PurchaseItemStages.FinalPurchaseAcknowledgement:
				if (buttonSide == "left")
				{
					FormattedPurchaseText("PURCHASING ITEM...");
					leftPurchaseButton.myText.text = "-";
					rightPurchaseButton.myText.text = "-";
					leftPurchaseButton.buttonRenderer.material = leftPurchaseButton.pressedMaterial;
					rightPurchaseButton.buttonRenderer.material = rightPurchaseButton.pressedMaterial;
					currentPurchaseItemStage = PurchaseItemStages.Buying;
					PurchaseItem();
				}
				else
				{
					currentPurchaseItemStage = PurchaseItemStages.CheckoutButtonPressed;
					ProcessPurchaseItemState(null);
				}
				break;
			case PurchaseItemStages.Success:
				FormattedPurchaseText("SUCCESS! ENJOY YOUR NEW ITEM!");
				PressWardrobeItemButton(itemToBuy);
				leftPurchaseButton.myText.text = "-";
				rightPurchaseButton.myText.text = "-";
				leftPurchaseButton.buttonRenderer.material = leftPurchaseButton.pressedMaterial;
				rightPurchaseButton.buttonRenderer.material = rightPurchaseButton.pressedMaterial;
				break;
			case PurchaseItemStages.Failure:
				FormattedPurchaseText("ERROR IN PURCHASING ITEM! NO MONEY WAS SPENT. SELECT ANOTHER ITEM.");
				leftPurchaseButton.myText.text = "-";
				rightPurchaseButton.myText.text = "-";
				leftPurchaseButton.buttonRenderer.material = leftPurchaseButton.pressedMaterial;
				rightPurchaseButton.buttonRenderer.material = rightPurchaseButton.pressedMaterial;
				break;
			case PurchaseItemStages.ItemOwned:
			case PurchaseItemStages.Buying:
				break;
		}
	}

	public void FormattedPurchaseText(string finalLineVar)
	{
		finalLine = finalLineVar;
		purchaseText.text = "SELECTION: " + itemToBuy.displayName + "\nITEM COST: " + itemToBuy.cost + "\nYOU HAVE: " + currencyBalance + "\n\n" + finalLine;
	}

	public void PurchaseItem()
	{
		PlayFabClientAPI.PurchaseItem(new PurchaseItemRequest
		{
			ItemId = itemToBuy.itemName,
			Price = itemToBuy.cost,
			VirtualCurrency = currencyName,
			CatalogVersion = catalog
		}, delegate (PurchaseItemResult result)
		{
			if (result.Items.Count > 0)
			{
				foreach (ItemInstance item in result.Items)
				{
					UnlockItem(item.ItemId);
				}
				if (PhotonNetwork.InRoom)
				{
					RaiseEventOptions raiseEventOptions = new RaiseEventOptions();
					WebFlags flags = new WebFlags(1);
					raiseEventOptions.Flags = flags;
					object[] eventContent = new object[0];
					PhotonNetwork.RaiseEvent(9, eventContent, raiseEventOptions, SendOptions.SendReliable);
				}
				StartCoroutine(CheckIfMyCosmeticsUpdated(itemToBuy.itemName));
				currentPurchaseItemStage = PurchaseItemStages.Success;
				currencyBalance -= itemToBuy.cost;
				UpdateShoppingCart();
				ProcessPurchaseItemState(null);
			}
			else
			{
				currentPurchaseItemStage = PurchaseItemStages.Failure;
				ProcessPurchaseItemState(null);
			}
		}, delegate
		{
			currentPurchaseItemStage = PurchaseItemStages.Failure;
			ProcessPurchaseItemState(null);
		});
	}

	private void UnlockItem(string itemIdToUnlock)
	{
		int num = allCosmetics.FindIndex((CosmeticItem x) => itemIdToUnlock == x.itemName);
		if (num > -1)
		{
			unlockedCosmetics.Add(allCosmetics[num]);
			concatStringCosmeticsAllowed += allCosmetics[num].itemName;
			switch (allCosmetics[num].itemSlot)
			{
				case "hat":
					unlockedHats.Add(allCosmetics[num]);
					break;
				case "badge":
					unlockedBadges.Add(allCosmetics[num]);
					break;
				case "face":
					unlockedFaces.Add(allCosmetics[num]);
					break;
			}
		}
	}

	private IEnumerator CheckIfMyCosmeticsUpdated(string itemToBuyID)
	{
		yield return new WaitForSeconds(1f);
		foundCosmetic = false;
		attempts = 0;
		while (!foundCosmetic && attempts < 10 && PhotonNetwork.InRoom)
		{
			playerIDList.Clear();
			playerIDList.Add(PhotonNetwork.LocalPlayer.UserId);
			PlayFabClientAPI.GetSharedGroupData(new GetSharedGroupDataRequest
			{
				Keys = playerIDList,
				SharedGroupId = PhotonNetwork.CurrentRoom.Name + Regex.Replace(PhotonNetwork.CloudRegion, "[^a-zA-Z0-9]", "").ToUpper()
			}, delegate (GetSharedGroupDataResult result)
			{
				attempts++;
				foreach (KeyValuePair<string, SharedGroupDataRecord> datum in result.Data)
				{
					if (datum.Value.Value.Contains(itemToBuyID))
					{
						if (GorillaGameManager.instance != null)
						{
							GorillaGameManager.instance.photonView.RPC("UpdatePlayerCosmetic", RpcTarget.Others);
						}
						foundCosmetic = true;
					}
				}
			}, delegate (PlayFabError error)
			{
				attempts++;
				if (error.Error == PlayFabErrorCode.NotAuthenticated)
				{
					PlayFabAuthenticator.instance.AuthenticateWithPlayFab();
				}
				else if (error.Error == PlayFabErrorCode.AccountBanned)
				{
					Application.Quit();
				}
			});
			yield return new WaitForSeconds(1f);
		}
	}

	public void UpdateWardrobeModelsAndButtons()
	{
		Wardrobe[] array = wardrobes;
		for (int i = 0; i < array.Length; i++)
		{
			Wardrobe wardrobe = array[i];
			wardrobe.wardrobeItemButtons[0].currentCosmeticItem = ((cosmeticsPages[wardrobeType] * 3 < itemLists[wardrobeType].Count) ? itemLists[wardrobeType][cosmeticsPages[wardrobeType] * 3] : nullItem);
			wardrobe.wardrobeItemButtons[1].currentCosmeticItem = ((cosmeticsPages[wardrobeType] * 3 + 1 < itemLists[wardrobeType].Count) ? itemLists[wardrobeType][cosmeticsPages[wardrobeType] * 3 + 1] : nullItem);
			wardrobe.wardrobeItemButtons[2].currentCosmeticItem = ((cosmeticsPages[wardrobeType] * 3 + 2 < itemLists[wardrobeType].Count) ? itemLists[wardrobeType][cosmeticsPages[wardrobeType] * 3 + 2] : nullItem);
			for (iterator = 0; iterator < wardrobe.wardrobeItemButtons.Length; iterator++)
			{
				wardrobe.wardrobeItemButtons[iterator].isOn = wardrobe.wardrobeItemButtons[iterator].currentCosmeticItem.itemName != "null" && AnyMatch(currentWornSet, wardrobe.wardrobeItemButtons[iterator].currentCosmeticItem);
				wardrobe.wardrobeItemButtons[iterator].UpdateColor();
			}
			wardrobe.wardrobeItemButtons[0].controlledModel.SetCosmeticActive(wardrobe.wardrobeItemButtons[0].currentCosmeticItem.displayName);
			wardrobe.wardrobeItemButtons[1].controlledModel.SetCosmeticActive(wardrobe.wardrobeItemButtons[1].currentCosmeticItem.displayName);
			wardrobe.wardrobeItemButtons[2].controlledModel.SetCosmeticActive(wardrobe.wardrobeItemButtons[2].currentCosmeticItem.displayName);
			wardrobe.selfDoll.SetCosmeticActiveArray(SetToStringArray(currentWornSet));
		}
	}

	public void UpdateShoppingCart()
	{
		for (iterator = 0; iterator < fittingRoomButtons.Length; iterator++)
		{
			if (iterator < currentCart.Count)
			{
				fittingRoomButtons[iterator].currentCosmeticItem = currentCart[iterator];
				checkoutCartButtons[iterator].currentCosmeticItem = currentCart[iterator];
				checkoutCartButtons[iterator].isOn = checkoutCartButtons[iterator].currentCosmeticItem.itemName == itemToBuy.itemName;
				fittingRoomButtons[iterator].isOn = AnyMatch(tryOnSet, fittingRoomButtons[iterator].currentCosmeticItem);
			}
			else
			{
				checkoutCartButtons[iterator].currentCosmeticItem = nullItem;
				fittingRoomButtons[iterator].currentCosmeticItem = nullItem;
				checkoutCartButtons[iterator].isOn = false;
				fittingRoomButtons[iterator].isOn = false;
			}
			checkoutCartButtons[iterator].currentImage.sprite = checkoutCartButtons[iterator].currentCosmeticItem.itemPicture;
			fittingRoomButtons[iterator].currentImage.sprite = fittingRoomButtons[iterator].currentCosmeticItem.itemPicture;
			checkoutCartButtons[iterator].UpdateColor();
			fittingRoomButtons[iterator].UpdateColor();
		}
		UpdateWardrobeModelsAndButtons();
		GorillaTagger.Instance.offlineVRRig.LocalUpdateCosmeticsWithTryon(currentWornSet.badge.displayName, currentWornSet.face.displayName, currentWornSet.hat.displayName, tryOnSet.badge.displayName, tryOnSet.face.displayName, tryOnSet.hat.displayName);
		if (GorillaTagger.Instance.myVRRig != null)
		{
			GorillaTagger.Instance.myVRRig.photonView.RPC("UpdateCosmeticsWithTryon", RpcTarget.All, currentWornSet.badge.displayName, currentWornSet.face.displayName, currentWornSet.hat.displayName, tryOnSet.badge.displayName, tryOnSet.face.displayName, tryOnSet.hat.displayName);
		}
	}

	public CosmeticItem GetItemFromDict(string itemID)
	{
		if (!allCosmeticsDict.TryGetValue(itemID, out cosmeticItemVar))
		{
			return nullItem;
		}
		return cosmeticItemVar;
	}

	public string GetItemNameFromDisplayName(string displayName)
	{
		if (!allCosmeticsItemIDsfromDisplayNamesDict.TryGetValue(displayName, out returnString))
		{
			return "null";
		}
		return returnString;
	}

	public bool AnyMatch(CosmeticSet set, CosmeticItem item)
	{
		if (item.itemSlot != "set")
		{
			if (!(set.hat.itemName == item.itemName) && !(set.face.itemName == item.itemName))
			{
				return set.badge.itemName == item.itemName;
			}
			return true;
		}
		if (item.bundledItems.Length == 1)
		{
			return AnyMatch(set, GetItemFromDict(item.bundledItems[0]));
		}
		if (item.bundledItems.Length == 2)
		{
			if (!AnyMatch(set, GetItemFromDict(item.bundledItems[0])))
			{
				return AnyMatch(set, GetItemFromDict(item.bundledItems[1]));
			}
			return true;
		}
		if (item.bundledItems.Length >= 3)
		{
			if (!AnyMatch(set, GetItemFromDict(item.bundledItems[0])) && !AnyMatch(set, GetItemFromDict(item.bundledItems[1])))
			{
				return AnyMatch(set, GetItemFromDict(item.bundledItems[2]));
			}
			return true;
		}
		return false;
	}

	public void Initialize()
	{
		if (base.gameObject.activeSelf)
		{
			GetUserCosmeticsAllowed();
		}
	}

	public void GetLastDailyLogin()
	{
		PlayFabClientAPI.GetUserReadOnlyData(new GetUserDataRequest(), delegate (GetUserDataResult result)
		{
			if (result.Data.TryGetValue("DailyLogin", out userDataRecord))
			{
				lastDailyLogin = userDataRecord.Value;
			}
			else
			{
				lastDailyLogin = "NONE";
				StartCoroutine(GetMyDaily());
			}
		}, delegate (PlayFabError error)
		{
			lastDailyLogin = "FAILED";
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

	private IEnumerator CheckCanGetDaily()
	{
		while (true)
		{
			if (computer.startupMillis != 0L)
			{
				currentTime = new DateTime((GorillaComputer.instance.startupMillis + (long)(Time.realtimeSinceStartup * 1000f)) * 10000);
				Debug.Log("seconds until tomorrow: " + (float)(currentTime.AddDays(1.0).Date - currentTime).TotalSeconds);
				secondsUntilTomorrow = (int)(currentTime.AddDays(1.0).Date - currentTime).TotalSeconds;
				if (lastDailyLogin == null || lastDailyLogin == "")
				{
					GetLastDailyLogin();
				}
				else if (currentTime.ToString("o").Substring(0, 10) == lastDailyLogin)
				{
					checkedDaily = true;
					gotMyDaily = true;
				}
				else if (currentTime.ToString("o").Substring(0, 10) != lastDailyLogin)
				{
					checkedDaily = true;
					gotMyDaily = false;
					StartCoroutine(GetMyDaily());
				}
				else if (lastDailyLogin == "FAILED")
				{
					GetLastDailyLogin();
				}
				secondsToWaitToCheckDaily = (checkedDaily ? 60f : 10f);
				UpdateCurrencyBoard();
				yield return new WaitForSeconds(secondsToWaitToCheckDaily);
			}
			else
			{
				yield return new WaitForSeconds(1f);
			}
		}
	}

	private IEnumerator GetMyDaily()
	{
		yield return new WaitForSeconds(10f);
		PlayFabClientAPI.ExecuteCloudScript(new ExecuteCloudScriptRequest
		{
			FunctionName = "TryDistributeCurrency",
			FunctionParameter = new { }
		}, delegate
		{
			Debug.Log("successfully tried to get daily!");
			GetCurrencyBalance();
			GetLastDailyLogin();
		}, delegate (PlayFabError error)
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

	public void GetUserCosmeticsAllowed()
	{
		PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), delegate (GetUserInventoryResult result)
		{
			PlayFabClientAPI.GetCatalogItems(new GetCatalogItemsRequest
			{
				CatalogVersion = catalog
			}, delegate (GetCatalogItemsResult result2)
			{
				unlockedCosmetics.Clear();
				unlockedHats.Clear();
				unlockedBadges.Clear();
				unlockedFaces.Clear();
				catalogItems = result2.Catalog;
				foreach (CatalogItem catalogItem in catalogItems)
				{
					searchIndex = allCosmetics.FindIndex((CosmeticItem x) => catalogItem.DisplayName == x.displayName);
					if (searchIndex > -1)
					{
						tempStringArray = null;
						hasPrice = false;
						if (catalogItem.Bundle != null)
						{
							tempStringArray = catalogItem.Bundle.BundledItems.ToArray();
						}
						if (catalogItem.VirtualCurrencyPrices.TryGetValue(currencyName, out var value))
						{
							hasPrice = true;
						}
						allCosmetics[searchIndex] = new CosmeticItem
						{
							itemName = catalogItem.ItemId,
							displayName = catalogItem.DisplayName,
							cost = (int)value,
							itemPicture = allCosmetics[searchIndex].itemPicture,
							itemSlot = allCosmetics[searchIndex].itemSlot,
							bundledItems = tempStringArray,
							canTryOn = hasPrice
						};
						allCosmeticsDict[allCosmetics[searchIndex].itemName] = allCosmetics[searchIndex];
						allCosmeticsItemIDsfromDisplayNamesDict[allCosmetics[searchIndex].displayName] = allCosmetics[searchIndex].itemName;
					}
				}
				for (int num = allCosmetics.Count - 1; num > -1; num--)
				{
					tempItem = allCosmetics[num];
					if (tempItem.itemSlot == "set" && tempItem.canTryOn)
					{
						string[] bundledItems = tempItem.bundledItems;
						foreach (string setItemName in bundledItems)
						{
							searchIndex = allCosmetics.FindIndex((CosmeticItem x) => setItemName == x.itemName);
							if (searchIndex > -1)
							{
								tempItem = new CosmeticItem
								{
									itemName = allCosmetics[searchIndex].itemName,
									displayName = allCosmetics[searchIndex].displayName,
									cost = allCosmetics[searchIndex].cost,
									itemPicture = allCosmetics[searchIndex].itemPicture,
									itemSlot = allCosmetics[searchIndex].itemSlot,
									canTryOn = true
								};
								allCosmetics[searchIndex] = tempItem;
								allCosmeticsDict[allCosmetics[searchIndex].itemName] = allCosmetics[searchIndex];
								allCosmeticsItemIDsfromDisplayNamesDict[allCosmetics[searchIndex].displayName] = allCosmetics[searchIndex].itemName;
							}
						}
					}
				}
				foreach (ItemInstance item in result.Inventory)
				{
					if (item.ItemId == "Early Access Supporter Pack")
					{
						unlockedCosmetics.Add(allCosmetics[1]);
						unlockedCosmetics.Add(allCosmetics[10]);
						unlockedCosmetics.Add(allCosmetics[11]);
						unlockedCosmetics.Add(allCosmetics[12]);
						unlockedCosmetics.Add(allCosmetics[13]);
						unlockedCosmetics.Add(allCosmetics[14]);
						unlockedCosmetics.Add(allCosmetics[15]);
						unlockedCosmetics.Add(allCosmetics[31]);
						unlockedCosmetics.Add(allCosmetics[32]);
						unlockedCosmetics.Add(allCosmetics[38]);
						unlockedCosmetics.Add(allCosmetics[67]);
						unlockedCosmetics.Add(allCosmetics[68]);
					}
					else
					{
						searchIndex = allCosmetics.FindIndex((CosmeticItem x) => item.ItemId == x.itemName);
						if (searchIndex > -1)
						{
							unlockedCosmetics.Add(allCosmetics[searchIndex]);
						}
					}
				}
				foreach (CosmeticItem unlockedCosmetic in unlockedCosmetics)
				{
					if (unlockedCosmetic.itemSlot == "hat" && !unlockedHats.Contains(unlockedCosmetic))
					{
						unlockedHats.Add(unlockedCosmetic);
					}
					else if (unlockedCosmetic.itemSlot == "face" && !unlockedFaces.Contains(unlockedCosmetic))
					{
						unlockedFaces.Add(unlockedCosmetic);
					}
					else if (unlockedCosmetic.itemSlot == "badge" && !unlockedBadges.Contains(unlockedCosmetic))
					{
						unlockedBadges.Add(unlockedCosmetic);
					}
					concatStringCosmeticsAllowed += unlockedCosmetic.itemName;
				}
				CosmeticStand[] array = cosmeticStands;
				for (int j = 0; j < array.Length; j++)
				{
					array[j].InitializeCosmetic();
				}
				currencyBalance = result.VirtualCurrency[currencyName];
				playedInBeta = result.VirtualCurrency.TryGetValue("TC", out var value2) && value2 > 0;
				currentWornSet.hat = GetItemFromDict(PlayerPrefs.GetString("hatCosmetic", "NOTHING"));
				currentWornSet.face = GetItemFromDict(PlayerPrefs.GetString("faceCosmetic", "NOTHING"));
				currentWornSet.badge = GetItemFromDict(PlayerPrefs.GetString("badgeCosmetic", "NOTHING"));
				SwitchToStage(ATMStages.Begin);
				ProcessPurchaseItemState(null);
				UpdateShoppingCart();
				UpdateCurrencyBoard();
			}, delegate (PlayFabError error)
			{
				Debug.Log("Got error retrieving catalog:");
				Debug.Log(error.GenerateErrorReport());
				if (error.Error == PlayFabErrorCode.NotAuthenticated)
				{
					PlayFabAuthenticator.instance.AuthenticateWithPlayFab();
				}
				else if (error.Error == PlayFabErrorCode.AccountBanned)
				{
					Application.Quit();
				}
				if (!tryTwice)
				{
					tryTwice = true;
					GetUserCosmeticsAllowed();
				}
			});
		}, delegate (PlayFabError error)
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
			if (!tryTwice)
			{
				tryTwice = true;
				GetUserCosmeticsAllowed();
			}
		});
	}

	public void ProcessATMState(string currencyButton)
	{
		switch (currentATMStage)
		{
			case ATMStages.Begin:
				SwitchToStage(ATMStages.Menu);
				break;
			case ATMStages.Menu:
				switch (currencyButton)
				{
					case "one":
						SwitchToStage(ATMStages.Balance);
						break;
					case "two":
						SwitchToStage(ATMStages.Choose);
						break;
					case "four":
						SwitchToStage(ATMStages.Begin);
						break;
				}
				break;
			case ATMStages.Balance:
				if (currencyButton == "four")
				{
					SwitchToStage(ATMStages.Menu);
				}
				break;
			case ATMStages.Choose:
				switch (currencyButton)
				{
					case "one":
						numShinyRocksToBuy = 1000;
						shinyRocksCost = 4.99f;
						itemToPurchase = "1000SHINYROCKS";
						SwitchToStage(ATMStages.Confirm);
						break;
					case "two":
						numShinyRocksToBuy = 2200;
						shinyRocksCost = 9.99f;
						itemToPurchase = "2200SHINYROCKS";
						SwitchToStage(ATMStages.Confirm);
						break;
					case "three":
						numShinyRocksToBuy = 5000;
						shinyRocksCost = 19.99f;
						itemToPurchase = "5000SHINYROCKS";
						SwitchToStage(ATMStages.Confirm);
						break;
					case "four":
						SwitchToStage(ATMStages.Menu);
						break;
				}
				break;
			case ATMStages.Confirm:
				if (currencyButton == "one")
				{
					Debug.Log("attempting to purchase item through steam");
					PlayFabClientAPI.StartPurchase(new StartPurchaseRequest
					{
						CatalogVersion = catalog,
						Items = new List<ItemPurchaseRequest>
					{
						new ItemPurchaseRequest
						{
							ItemId = itemToPurchase,
							Quantity = 1u,
							Annotation = "Purchased via in-game store"
						}
					}
					}, delegate (StartPurchaseResult result)
					{
						Debug.Log("successfully started purchase. attempted to pay for purchase through steam");
						currentPurchaseID = result.OrderId;
						PlayFabClientAPI.PayForPurchase(new PayForPurchaseRequest
						{
							OrderId = currentPurchaseID,
							ProviderName = "Steam",
							Currency = "RM"
						}, delegate
						{
							Debug.Log("succeeded on sending request for paying with steam! waiting for response");
							m_MicroTxnAuthorizationResponse = Callback<MicroTxnAuthorizationResponse_t>.Create(OnMicroTxnAuthorizationResponse);
						}, delegate (PlayFabError error)
						{
							if (error.Error == PlayFabErrorCode.NotAuthenticated)
							{
								PlayFabAuthenticator.instance.AuthenticateWithPlayFab();
							}
							else if (error.Error == PlayFabErrorCode.AccountBanned)
							{
								Application.Quit();
							}
							Debug.Log("failed to send request to purchase with steam!");
							Debug.Log(error.ToString());
							SwitchToStage(ATMStages.Failure);
						});
					}, delegate (PlayFabError error)
					{
						if (error.Error == PlayFabErrorCode.NotAuthenticated)
						{
							PlayFabAuthenticator.instance.AuthenticateWithPlayFab();
						}
						else if (error.Error == PlayFabErrorCode.AccountBanned)
						{
							Application.Quit();
						}
						Debug.Log("error in starting purchase!");
					});
					SwitchToStage(ATMStages.Purchasing);
				}
				else if (currencyButton == "four")
				{
					SwitchToStage(ATMStages.Choose);
				}
				break;
			default:
				SwitchToStage(ATMStages.Menu);
				break;
			case ATMStages.Unavailable:
			case ATMStages.Purchasing:
				break;
		}
	}

	public void SwitchToStage(ATMStages newStage)
	{
		currentATMStage = newStage;
		switch (newStage)
		{
			case ATMStages.Unavailable:
				atmText.text = "ATM NOT AVAILABLE! PLEASE TRY AGAIN LATER!";
				atmButtonsText.text = "";
				break;
			case ATMStages.Begin:
				atmText.text = "WELCOME! PRESS ANY BUTTON TO BEGIN.";
				atmButtonsText.text = "\n\n\n\n\n\n\n\n\nBEGIN   -->";
				break;
			case ATMStages.Menu:
				atmText.text = "CHECK YOUR BALANCE OR PURCHASE MORE SHINY ROCKS.";
				atmButtonsText.text = "BALANCE-- >\n\n\nPURCHASE-->\n\n\n\n\n\nBACK    -->";
				break;
			case ATMStages.Balance:
				atmText.text = "CURRENT BALANCE:\n\n" + currencyBalance;
				atmButtonsText.text = "\n\n\n\n\n\n\n\n\nBACK    -->";
				break;
			case ATMStages.Choose:
				atmText.text = "CHOOSE AN AMOUNT OF SHINY ROCKS TO PURCHASE.";
				atmButtonsText.text = "$4.99 FOR -->\n1000\n\n$9.99 FOR -->\n2200\n\n$19.99 FOR-->\n5000\n\nBACK -->";
				break;
			case ATMStages.Confirm:
				atmText.text = "YOU HAVE CHOSEN TO PURCHASE " + numShinyRocksToBuy + " SHINY ROCKS FOR $" + shinyRocksCost + ". CONFIRM TO LAUNCH A STEAM WINDOW TO COMPLETE YOUR PURCHASE.";
				atmButtonsText.text = "CONFIRM -->\n\n\n\n\n\n\n\n\nBACK    -->";
				break;
			case ATMStages.Purchasing:
				atmText.text = "PURCHASING IN STEAM...";
				atmButtonsText.text = "";
				break;
			case ATMStages.Success:
				atmText.text = "SUCCESS! NEW SHINY ROCKS BALANCE: " + (currencyBalance + numShinyRocksToBuy);
				atmButtonsText.text = "\n\n\n\n\n\n\n\n\nRETURN  -->";
				break;
			case ATMStages.Failure:
				atmText.text = "PURCHASE CANCELED. NO FUNDS WERE SPENT.";
				atmButtonsText.text = "\n\n\n\n\n\n\n\n\nRETURN  -->";
				break;
		}
	}

	public void PressCurrencyPurchaseButton(string currencyPurchaseSize)
	{
		ProcessATMState(currencyPurchaseSize);
	}

	private void OnMicroTxnAuthorizationResponse(MicroTxnAuthorizationResponse_t pCallback)
	{
		Debug.Log("trying to send mtx request");
		PlayFabClientAPI.ConfirmPurchase(new ConfirmPurchaseRequest
		{
			OrderId = currentPurchaseID
		}, delegate
		{
			Debug.Log("successfully confirmed purchase!");
			SwitchToStage(ATMStages.Success);
			GetCurrencyBalance();
			UpdateCurrencyBoard();
		}, delegate
		{
			Debug.Log("failed to confirm purchase!");
			atmText.text = "PURCHASE CANCELLED!\n\nCURRENT BALANCE IS: ";
			UpdateCurrencyBoard();
			SwitchToStage(ATMStages.Failure);
		});
	}

	public string[] SetToStringArray(CosmeticSet set)
	{
		returnStringArray[0] = set.hat.displayName;
		returnStringArray[1] = set.face.displayName;
		returnStringArray[2] = set.badge.displayName;
		return returnStringArray;
	}

	public void UpdateCurrencyBoard()
	{
		FormattedPurchaseText(finalLine);
		dailyText.text = ((!checkedDaily) ? "CHECKING DAILY ROCKS..." : (gotMyDaily ? "SUCCESSFULLY GOT DAILY ROCKS!" : "WAITING TO GET DAILY ROCKS..."));
		currencyBoardText.text = currencyBalance + "\n\n" + secondsUntilTomorrow / 3600 + " HR, " + secondsUntilTomorrow % 3600 / 60 + "MIN";
	}

	public void GetCurrencyBalance()
	{
		PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(), delegate (GetUserInventoryResult result)
		{
			currencyBalance = result.VirtualCurrency[currencyName];
			UpdateCurrencyBoard();
		}, delegate (PlayFabError error)
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

	public void LeaveSystemMenu()
	{
	}
}
