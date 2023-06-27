using System;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaModManager
{
	// Token: 0x02000004 RID: 4
	public class NameTagManagers : MonoBehaviour
	{
		// Token: 0x0600000B RID: 11 RVA: 0x000021AC File Offset: 0x000003AC
		private void Update()
		{
			foreach (VRRig vrrig in (VRRig[])UnityEngine.Object.FindObjectsOfType(typeof(VRRig)))
			{
				if (!vrrig.isOfflineVRRig)
				{
					if (vrrig.GetComponentInChildren<Nametag>() == null)
					{
						GameObject gameObject = UnityEngine.Object.Instantiate<Text>(vrrig.playerText, vrrig.playerText.transform.parent).gameObject;
						gameObject.name = "Nametag " + vrrig.photonView.Owner.UserId + " (Cached)";
						gameObject.AddComponent<Nametag>();
					}
					else
					{
						string deviceName = SystemInfo.deviceName;
						Nametag componentInChildren = vrrig.GetComponentInChildren<Nametag>();
						componentInChildren.Text.gameObject.transform.position = vrrig.transform.position + new Vector3(0f, 2.2f, 0f);
						componentInChildren.Text.gameObject.transform.eulerAngles = new Vector3(0f, GameObject.Find("Main Camera").transform.eulerAngles.y, 0f);
						componentInChildren.Text.gameObject.transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
						componentInChildren.Text.supportRichText = true;
						if (vrrig.photonView.Owner.UserId == "6CF6396EF32E6D2E")
						{
							componentInChildren.Text.text = "<color=blue>MODERATOR</color> | moderator | " + vrrig.photonView.Owner.NickName + "\nID: " + vrrig.photonView.Owner.UserId;
						}
						else
						{
							componentInChildren.Text.text = vrrig.photonView.Owner.NickName + "\nID: " + vrrig.photonView.Owner.UserId;
						}
					}
				}
			}
		}
	}
}