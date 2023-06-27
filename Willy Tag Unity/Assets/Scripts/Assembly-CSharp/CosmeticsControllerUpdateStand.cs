using GorillaNetworking;
using UnityEngine;

public class CosmeticsControllerUpdateStand : MonoBehaviour
{
	public CosmeticsController cosmeticsController;

	public bool FailEntitlement;

	public bool PlayerUnlocked;

	public bool ItemNotGrantedYet;

	public bool ItemSuccessfullyGranted;

	public bool AttemptToConsumeEntitlement;

	public bool EntitlementSuccessfullyConsumed;

	public bool LockSuccessfullyCleared;

	public bool RunDebug;

	public HeadModel[] inventoryHeadModels;

	public void UpdateCosmeticStands()
	{
		cosmeticsController.cosmeticStands = Object.FindObjectsOfType<CosmeticStand>();
		CosmeticStand[] cosmeticStands = cosmeticsController.cosmeticStands;
		foreach (CosmeticStand cosmeticStand in cosmeticStands)
		{
			GameObject gameObject = null;
			for (int j = 0; j < cosmeticStand.thisHeadModel.transform.childCount; j++)
			{
				for (int k = 0; k < cosmeticStand.thisHeadModel.transform.GetChild(j).gameObject.transform.childCount; k++)
				{
					if (cosmeticStand.thisHeadModel.transform.GetChild(j).gameObject.transform.GetChild(k).gameObject.activeInHierarchy)
					{
						gameObject = cosmeticStand.thisHeadModel.transform.GetChild(j).gameObject.transform.GetChild(k).gameObject;
						break;
					}
				}
			}
			if (gameObject != null)
			{
				cosmeticStand.thisCosmeticName = gameObject.name;
			}
		}
	}

	public void UpdateInventoryHeadModels()
	{
		HeadModel[] array = inventoryHeadModels;
		foreach (HeadModel headModel in array)
		{
			for (int j = 0; j < headModel.transform.childCount; j++)
			{
				for (int k = 0; k < headModel.transform.GetChild(j).gameObject.transform.childCount; k++)
				{
					if (!headModel.transform.GetChild(j).gameObject.transform.GetChild(k).gameObject.activeInHierarchy)
					{
						headModel.transform.GetChild(j).gameObject.transform.GetChild(k).gameObject.SetActive(value: true);
					}
				}
			}
		}
	}
}
