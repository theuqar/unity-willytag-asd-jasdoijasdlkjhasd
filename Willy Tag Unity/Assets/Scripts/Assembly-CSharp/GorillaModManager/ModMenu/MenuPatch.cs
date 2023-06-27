using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using GorillaLocomotion;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

namespace WristMenu
{
	// Token: 0x02000004 RID: 4
	internal class MenuPatch : MonoBehaviour
	{
		// Token: 0x06000005 RID: 5 RVA: 0x000020CC File Offset: 0x000002CC
		private static void Prefix(GorillaLocomotion.Player __instance)
		{
			List<InputDevice> list = new List<InputDevice>();
			InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller, list);
			list[0].TryGetFeatureValue(CommonUsages.primaryButton, out MenuPatch.down);
			bool flag = MenuPatch.down;
			bool flag2 = flag;
			if (flag2)
			{
				bool flag3 = !MenuPatch.no;
				bool flag4 = flag3;
				if (flag4)
				{
					foreach (MeshCollider meshCollider in Resources.FindObjectsOfTypeAll<MeshCollider>())
					{
						meshCollider.transform.localScale = meshCollider.transform.localScale / 10000f;
					}
					MenuPatch.no = true;
					MenuPatch.yes = false;
				}
			}
			else
			{
				bool flag5 = !MenuPatch.yes;
				bool flag6 = flag5;
				if (flag6)
				{
					foreach (MeshCollider meshCollider2 in Resources.FindObjectsOfTypeAll<MeshCollider>())
					{
						meshCollider2.transform.localScale = meshCollider2.transform.localScale * 10000f;
					}
					MenuPatch.yes = true;
					MenuPatch.no = false;
				}
			}
			try
			{
				bool flag7 = MenuPatch.maxJumpSpeed == null;
				bool flag8 = flag7;
				bool flag9 = flag8;
				if (flag9)
				{
					MenuPatch.maxJumpSpeed = new float?(__instance.maxJumpSpeed);
					MenuPatch.verified = MenuPatch.CheckVerify();
				}
				List<InputDevice> list2 = new List<InputDevice>();
				InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller, list2);
				list[0].TryGetFeatureValue(CommonUsages.primaryButton, out MenuPatch.down);
				bool flag10 = MenuPatch.gripDown && MenuPatch.menu == null;
				bool flag11 = flag10;
				bool flag12 = flag11;
				if (flag12)
				{
					MenuPatch.Draw();
					bool flag13 = MenuPatch.referance == null;
					bool flag14 = flag13;
					bool flag15 = flag14;
					if (flag15)
					{
						MenuPatch.referance = GameObject.CreatePrimitive(0);
						GameObject.Destroy(MenuPatch.referance.GetComponent<MeshRenderer>());
						MenuPatch.referance.transform.parent = __instance.rightHandTransform;
						MenuPatch.referance.transform.localPosition = new Vector3(0f, -0.1f, 0f);
						MenuPatch.referance.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
					}
				}
				else
				{
					bool flag16 = !MenuPatch.gripDown && MenuPatch.menu != null;
					bool flag17 = flag16;
					bool flag18 = flag17;
					if (flag18)
					{
						GameObject.Destroy(MenuPatch.menu);
						MenuPatch.menu = null;
						GameObject.Destroy(MenuPatch.referance);
						MenuPatch.referance = null;
					}
				}
				bool flag19 = MenuPatch.gripDown && MenuPatch.menu != null;
				bool flag20 = flag19;
				bool flag21 = flag20;
				if (flag21)
				{
					MenuPatch.menu.transform.position = __instance.leftHandTransform.position;
					MenuPatch.menu.transform.rotation = __instance.leftHandTransform.rotation;
				}
				bool flag22 = MenuPatch.verified;
				bool flag23 = flag22;
				bool flag24 = flag23;
				if (flag24)
				{
					bool? flag25 = MenuPatch.buttonsActive[0];
					bool flag26 = true;
					bool flag27 = flag25.GetValueOrDefault() == flag26 & flag25 != null;
					bool flag28 = flag27;
					bool flag29 = flag28;
					if (flag29)
					{
						bool flag30 = false;
						bool flag31 = false;
						list2 = new List<InputDevice>();
						InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller, list2);
						list2[0].TryGetFeatureValue(CommonUsages.primaryButton, out flag30);
						list2[0].TryGetFeatureValue(CommonUsages.primaryButton, out flag31);
						bool flag32 = flag30;
						bool flag33 = flag32;
						bool flag34 = flag33;
						if (flag34)
						{
							__instance.transform.position += __instance.headCollider.transform.forward * Time.deltaTime * 12f;
							__instance.GetComponent<Rigidbody>().velocity = Vector3.zero;
							bool flag35 = !MenuPatch.flying;
							bool flag36 = flag35;
							bool flag37 = flag36;
							if (flag37)
							{
								MenuPatch.flying = true;
							}
						}
						else
						{
							bool flag38 = MenuPatch.flying;
							bool flag39 = flag38;
							bool flag40 = flag39;
							if (flag40)
							{
								__instance.GetComponent<Rigidbody>().velocity = __instance.headCollider.transform.forward * Time.deltaTime * 12f;
								MenuPatch.flying = false;
							}
						}
						bool flag41 = flag31;
						bool flag42 = flag41;
						bool flag43 = flag42;
						if (flag43)
						{
							bool flag44 = !MenuPatch.gravityToggled && __instance.bodyCollider.attachedRigidbody.useGravity;
							bool flag45 = flag44;
							bool flag46 = flag45;
							if (flag46)
							{
								__instance.bodyCollider.attachedRigidbody.useGravity = false;
								MenuPatch.gravityToggled = true;
							}
							else
							{
								bool flag47 = !MenuPatch.gravityToggled && !__instance.bodyCollider.attachedRigidbody.useGravity;
								bool flag48 = flag47;
								bool flag49 = flag48;
								if (flag49)
								{
									__instance.bodyCollider.attachedRigidbody.useGravity = true;
									MenuPatch.gravityToggled = true;
								}
							}
						}
						else
						{
							MenuPatch.gravityToggled = false;
						}
						flag25 = MenuPatch.buttonsActive[5];
						flag26 = true;
						bool flag50 = flag25.GetValueOrDefault() == flag26 & flag25 != null;
						bool flag51 = flag50;
						bool flag52 = flag51;
						if (flag52)
						{
							try
							{
								foreach (VRRig vrrig in Resources.FindObjectsOfTypeAll<VRRig>())
								{
									bool isMine = vrrig.photonView.IsMine;
									bool flag53 = isMine;
									if (flag53)
									{
										vrrig.gameObject.transform.Find("gorilla").GetComponent<Renderer>().enabled = true;
									}
								}
								try
								{
									foreach (VRRig vrrig2 in Resources.FindObjectsOfTypeAll<VRRig>())
									{
										bool isMine2 = vrrig2.photonView.IsMine;
										bool flag54 = isMine2;
										if (flag54)
										{
											GameObject.FindObjectsOfType<VRRig>();
										}
									}
									new Vector3(1f, 1f, 1f);
								}
								catch
								{
								}
							}
							catch
							{
							}
						}
					}
					flag25 = MenuPatch.buttonsActive[1];
					flag26 = true;
					bool flag55 = flag25.GetValueOrDefault() == flag26 & flag25 != null;
					bool flag56 = flag55;
					bool flag57 = flag56;
					if (flag57)
					{
						bool flag58 = false;
						bool flag59 = false;
						list2 = new List<InputDevice>();
						InputDevices.GetDevices(list2);
						InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller, list2);
						list2[0].TryGetFeatureValue(CommonUsages.primaryButton, out flag58);
						list2[0].TryGetFeatureValue(CommonUsages.gripButton, out flag59);
						bool flag60 = flag59;
						bool flag61 = flag60;
						bool flag62 = flag61;
						if (flag62)
						{
							bool flag63 = MenuPatch.pointer == null;
							bool flag64 = flag63;
							bool flag65 = flag64;
							if (flag65)
							{
								MenuPatch.pointer = GameObject.CreatePrimitive(0);
								GameObject.Destroy(MenuPatch.pointer.GetComponent<Rigidbody>());
								GameObject.Destroy(MenuPatch.pointer.GetComponent<SphereCollider>());
								MenuPatch.pointer.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
							}
							GorillaLocomotion.Player player;
							MenuPatch.pointer.GetComponent<Renderer>().material.SetColor("_Color", Color.magenta);
							Hashtable hashtable = new Hashtable();
							hashtable.Add("matIndex", MenuPatch.DefaultMaterial);
							MenuPatch.DefaultMaterial++;
							bool flag67 = MenuPatch.DefaultMaterial >= 7;
							bool flag68 = flag67;
							bool flag69 = flag68;
							if (flag69)
							{
								MenuPatch.DefaultMaterial = 0;
							}
						}
						flag25 = MenuPatch.buttonsActive[2];
						flag26 = true;
						bool flag70 = flag25.GetValueOrDefault() == flag26 & flag25 != null;
						bool flag71 = flag70;
						bool flag72 = flag71;
						if (flag72)
						{
							VRRig[] array5 = (VRRig[])GameObject.FindObjectsOfType(typeof(VRRig));
							foreach (VRRig vrrig3 in array5)
							{
								bool flag73 = !vrrig3.isOfflineVRRig && !vrrig3.isMyPlayer && !vrrig3.photonView.IsMine;
								if (flag73)
								{
									GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
									GameObject.Destroy(gameObject.GetComponent<BoxCollider>());
									GameObject.Destroy(gameObject.GetComponent<Rigidbody>());
									GameObject.Destroy(gameObject.GetComponent<Collider>());
									gameObject.transform.rotation = Quaternion.identity;
									gameObject.transform.localScale = new Vector3(0.04f, 200f, 0.04f);
									gameObject.transform.position = vrrig3.transform.position;
									gameObject.GetComponent<MeshRenderer>().material = vrrig3.mainSkin.material;
									GameObject.Destroy(gameObject, Time.deltaTime);
								}
							}
						}
						flag25 = MenuPatch.buttonsActive[3];
						flag26 = true;
						bool flag74 = flag25.GetValueOrDefault() == flag26 & flag25 != null;
						bool flag75 = flag74;
						bool flag76 = flag75;
						if (flag76)
						{
							GorillaTagger.Instance.taggedTime = 2f;
							__instance.disableMovement = false;
						}
						flag25 = MenuPatch.buttonsActive[4];
						flag26 = true;
						bool flag77 = flag25.GetValueOrDefault() == flag26 & flag25 != null;
						bool flag78 = flag77;
						bool flag79 = flag78;
						if (flag79)
						{
							__instance.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
							try
							{
								foreach (VRRig vrrig4 in Resources.FindObjectsOfTypeAll<VRRig>())
								{
									bool isMine3 = vrrig4.photonView.IsMine;
									bool flag80 = isMine3;
									if (flag80)
									{
										vrrig4.gameObject.transform.Find("gorilla").GetComponent<Renderer>().enabled = false;
									}
								}
								try
								{
									foreach (VRRig vrrig5 in Resources.FindObjectsOfTypeAll<VRRig>())
									{
										bool isMine4 = vrrig5.photonView.IsMine;
										bool flag81 = isMine4;
										if (flag81)
										{
											GameObject.FindObjectsOfType<VRRig>();
										}
									}
									__instance.transform.rotation = Quaternion.Euler(0f, 0f, 180f);
								}
								catch
								{
								}
							}
							catch
							{
							}
						}
						else
						{
							__instance.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
							try
							{
								foreach (VRRig vrrig6 in Resources.FindObjectsOfTypeAll<VRRig>())
								{
									bool isMine5 = vrrig6.photonView.IsMine;
									bool flag82 = isMine5;
									if (flag82)
									{
										vrrig6.gameObject.transform.Find("gorilla").GetComponent<Renderer>().enabled = true;
									}
								}
								try
								{
									foreach (VRRig vrrig7 in Resources.FindObjectsOfTypeAll<VRRig>())
									{
										bool isMine6 = vrrig7.photonView.IsMine;
										bool flag83 = isMine6;
										if (flag83)
										{
											GameObject.FindObjectsOfType<VRRig>();
										}
									}
									__instance.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
								}
								catch
								{
								}
							}
							catch
							{
							}
						}
						flag25 = MenuPatch.buttonsActive[5];
						flag26 = true;
						bool flag84 = flag25.GetValueOrDefault() == flag26 & flag25 != null;
						bool flag85 = flag84;
						bool flag86 = flag85;
						if (flag86)
						{
							GorillaLocomotion.Player.Instance.leftHandOffset = new Vector3(0f, 0f, 0f);
							GorillaLocomotion.Player.Instance.rightHandOffset = new Vector3(0f, 0f, 0f);
							List<InputDevice> list3 = new List<InputDevice>();
							InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller, list);
							list[0].TryGetFeatureValue(CommonUsages.gripButton, out MenuPatch.gain);
							list[0].TryGetFeatureValue(CommonUsages.triggerButton, out MenuPatch.less);
							list[0].TryGetFeatureValue(CommonUsages.primaryButton, out MenuPatch.reset);
							list[0].TryGetFeatureValue(CommonUsages.secondaryButton, out MenuPatch.fastr);
							MenuPatch.timeSinceLastChange += Time.deltaTime;
							bool flag87 = MenuPatch.timeSinceLastChange <= 0.2f;
							if (flag87)
							{
								return;
							}
							GorillaLocomotion.Player.Instance.leftHandOffset = new Vector3(0f, MenuPatch.myVarY1, 0f);
							GorillaLocomotion.Player.Instance.rightHandOffset = new Vector3(0f, MenuPatch.myVarY2, 0f);
							GorillaLocomotion.Player.Instance.maxArmLength = 200f;
							bool flag88 = MenuPatch.gain;
							if (flag88)
							{
								MenuPatch.timeSinceLastChange = 0f;
								MenuPatch.myVarY1 += MenuPatch.gainSpeed;
								MenuPatch.myVarY2 += MenuPatch.gainSpeed;
								bool flag89 = MenuPatch.myVarY1 >= 201f;
								if (flag89)
								{
									MenuPatch.myVarY1 = 200f;
									MenuPatch.myVarY2 = 200f;
								}
							}
							bool flag90 = MenuPatch.less;
							if (flag90)
							{
								MenuPatch.timeSinceLastChange = 0f;
								MenuPatch.myVarY1 -= MenuPatch.gainSpeed;
								MenuPatch.myVarY2 -= MenuPatch.gainSpeed;
								bool flag91 = MenuPatch.myVarY2 <= -6f;
								if (flag91)
								{
									MenuPatch.myVarY1 = -5f;
									MenuPatch.myVarY2 = -5f;
								}
							}
							bool flag92 = MenuPatch.reset;
							if (flag92)
							{
								MenuPatch.timeSinceLastChange = 0f;
								MenuPatch.myVarY1 = 0f;
								MenuPatch.myVarY2 = 0f;
							}
							bool flag93 = MenuPatch.fastr;
							if (flag93)
							{
								bool flag94 = MenuPatch.myVarY1 == 5f;
								if (flag94)
								{
									MenuPatch.myVarY1 = 10f;
									MenuPatch.myVarY2 = 10f;
								}
							}
						}
						else
						{
                            GorillaLocomotion.Player.Instance.leftHandOffset = new Vector3(0f, 0f, 0f);
							GorillaLocomotion.Player.Instance.rightHandOffset = new Vector3(0f, 0f, 0f);
							List<InputDevice> list4 = new List<InputDevice>();
							InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller, list);
							list[0].TryGetFeatureValue(CommonUsages.gripButton, out MenuPatch.gain);
							list[0].TryGetFeatureValue(CommonUsages.triggerButton, out MenuPatch.less);
							list[0].TryGetFeatureValue(CommonUsages.primaryButton, out MenuPatch.reset);
							list[0].TryGetFeatureValue(CommonUsages.secondaryButton, out MenuPatch.fastr);
							MenuPatch.timeSinceLastChange += Time.deltaTime;
							bool flag95 = MenuPatch.timeSinceLastChange <= 0.2f;
							if (flag95)
							{
								return;
							}
							GorillaLocomotion.Player.Instance.leftHandOffset = new Vector3(0f, MenuPatch.myVarY1, 0f);
							GorillaLocomotion.Player.Instance.rightHandOffset = new Vector3(0f, MenuPatch.myVarY2, 0f);
							GorillaLocomotion.Player.Instance.maxArmLength = 75f;
							bool flag96 = MenuPatch.gain;
							if (flag96)
							{
								MenuPatch.timeSinceLastChange = 0f;
								MenuPatch.myVarY1 += MenuPatch.gainSpeed;
								MenuPatch.myVarY2 += MenuPatch.gainSpeed;
								bool flag97 = MenuPatch.myVarY1 >= 75f;
								if (flag97)
								{
									MenuPatch.myVarY1 = 75f;
									MenuPatch.myVarY2 = 75f;
								}
							}
							bool flag98 = MenuPatch.less;
							if (flag98)
							{
								MenuPatch.timeSinceLastChange = 0f;
								MenuPatch.myVarY1 -= MenuPatch.gainSpeed;
								MenuPatch.myVarY2 -= MenuPatch.gainSpeed;
								bool flag99 = MenuPatch.myVarY2 <= 0f;
								if (flag99)
								{
									MenuPatch.myVarY1 = 0f;
									MenuPatch.myVarY2 = 0f;
								}
							}
							bool flag100 = MenuPatch.reset;
							if (flag100)
							{
								MenuPatch.timeSinceLastChange = 0f;
								MenuPatch.myVarY1 = 0f;
								MenuPatch.myVarY2 = 0f;
							}
							bool flag101 = MenuPatch.fastr;
							if (flag101)
							{
								bool flag102 = MenuPatch.myVarY1 == 5f;
								if (flag102)
								{
									MenuPatch.myVarY1 = 10f;
									MenuPatch.myVarY2 = 10f;
								}
							}
						}
						flag25 = MenuPatch.buttonsActive[6];
						flag26 = true;
						bool flag103 = flag25.GetValueOrDefault() == flag26 & flag25 != null;
						bool flag104 = flag103;
						bool flag105 = flag104;
						if (flag105)
						{
							Physics.gravity *= -1f;
						}
						else
						{
							Physics.gravity *= 1f;
						}
					}
				}
			}
			catch
			{
			}
		}

		// Token: 0x06000006 RID: 6 RVA: 0x00003244 File Offset: 0x00001444
		private static bool CheckVerify()
		{
			return true;
		}

		// Token: 0x06000007 RID: 7 RVA: 0x00003258 File Offset: 0x00001458
		private static void AddButton(float offset, string text)
		{
			GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
			GameObject.Destroy(gameObject.GetComponent<Rigidbody>());
			gameObject.GetComponent<BoxCollider>().isTrigger = true;
			gameObject.transform.parent = MenuPatch.menu.transform;
			gameObject.transform.rotation = Quaternion.identity;
			gameObject.transform.localScale = new Vector3(0.09f, 0.8f, 0.08f);
			gameObject.transform.localPosition = new Vector3(0.56f, 0f, 0.35f - offset);
			gameObject.AddComponent<BtnCollider>().relatedText = text;
			int num = -1;
			for (int i = 0; i < MenuPatch.buttons.Length; i++)
			{
				bool flag = text == MenuPatch.buttons[i];
				bool flag2 = flag;
				bool flag3 = flag2;
				if (flag3)
				{
					num = i;
					break;
				}
			}
			bool? flag4 = MenuPatch.buttonsActive[num];
			bool flag5 = false;
			bool flag6 = flag4.GetValueOrDefault() == flag5 & flag4 != null;
			bool flag7 = flag6;
			bool flag8 = flag7;
			if (flag8)
			{
				gameObject.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
			}
			else
			{
				flag4 = MenuPatch.buttonsActive[num];
				flag5 = true;
				bool flag9 = flag4.GetValueOrDefault() == flag5 & flag4 != null;
				bool flag10 = flag9;
				bool flag11 = flag10;
				if (flag11)
				{
					gameObject.GetComponent<Renderer>().material.SetColor("_Color", Color.green);
				}
				else
				{
					gameObject.GetComponent<Renderer>().material.SetColor("_Color", Color.grey);
				}
			}
			Text text2 = new GameObject
			{
				transform =
				{
					parent = MenuPatch.canvasObj.transform
				}
			}.AddComponent<Text>();
			text2.font = (Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font);
			text2.text = text;
			text2.fontSize = 1;
			text2.alignment = (TextAnchor)4;
			text2.resizeTextForBestFit = true;
			text2.resizeTextMinSize = 0;
			RectTransform component = text2.GetComponent<RectTransform>();
			component.localPosition = Vector3.zero;
			component.sizeDelta = new Vector2(0.2f, 0.03f);
			component.localPosition = new Vector3(0.064f, 0f, 0.134f - offset / 2.55f);
			component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
		}

		// Token: 0x06000008 RID: 8 RVA: 0x000034D8 File Offset: 0x000016D8
		public static void Draw()
		{
			GameObject gameObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
			GameObject.Destroy(MenuPatch.menu.GetComponent<Rigidbody>());
			GameObject.Destroy(MenuPatch.menu.GetComponent<BoxCollider>());
			GameObject.Destroy(MenuPatch.menu.GetComponent<Renderer>());
			MenuPatch.menu.transform.localScale = new Vector3(0.1f, 0.3f, 0.4f);
			GameObject gameObject1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
			GameObject.Destroy(gameObject.GetComponent<Rigidbody>());
			GameObject.Destroy(gameObject.GetComponent<BoxCollider>());
			gameObject.transform.parent = MenuPatch.menu.transform;
			gameObject.transform.rotation = Quaternion.identity;
			gameObject.transform.localScale = new Vector3(0.1f, 1.3f, 1f);
			gameObject.GetComponent<Renderer>().material.SetColor("_Color", Color.grey);
			gameObject.transform.position = new Vector3(0.05f, 0f, 0f);
			MenuPatch.canvasObj = new GameObject();
			MenuPatch.canvasObj.transform.parent = MenuPatch.menu.transform;
			Canvas canvas = MenuPatch.canvasObj.AddComponent<Canvas>();
			CanvasScaler canvasScaler = MenuPatch.canvasObj.AddComponent<CanvasScaler>();
			MenuPatch.canvasObj.AddComponent<GraphicRaycaster>();
			canvas.renderMode = RenderMode.WorldSpace;
			canvasScaler.dynamicPixelsPerUnit = 1000f;
			Text text = new GameObject
			{
				transform =
				{
					parent = MenuPatch.canvasObj.transform
				}
			}.AddComponent<Text>();
			text.font = (Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font);
			text.text = "ModMenu";
			text.fontSize = 1;
			text.alignment = (TextAnchor)4;
			text.resizeTextForBestFit = true;
			text.resizeTextMinSize = 0;
			RectTransform component = text.GetComponent<RectTransform>();
			component.localPosition = Vector3.zero;
			component.sizeDelta = new Vector2(0.28f, 0.05f);
			component.position = new Vector3(0.06f, 0f, 0.17f);
			component.rotation = Quaternion.Euler(new Vector3(180f, 90f, 90f));
			bool flag = MenuPatch.verified;
			bool flag2 = flag;
			bool flag3 = flag2;
			if (flag3)
			{
				for (int i = 0; i < MenuPatch.buttons.Length; i++)
				{
					MenuPatch.AddButton((float)i * 0.13f, MenuPatch.buttons[i]);
				}
			}
		}

		// Token: 0x06000009 RID: 9 RVA: 0x00003760 File Offset: 0x00001960
		public static void Toggle(string relatedText)
		{
			int num = -1;
			for (int i = 0; i < MenuPatch.buttons.Length; i++)
			{
				bool flag = relatedText == MenuPatch.buttons[i];
				bool flag2 = flag;
				bool flag3 = flag2;
				if (flag3)
				{
					num = i;
					break;
				}
			}
			bool flag4 = MenuPatch.buttonsActive[num] != null;
			bool flag5 = flag4;
			bool flag6 = flag5;
			if (flag6)
			{
				MenuPatch.buttonsActive[num] = !MenuPatch.buttonsActive[num];
				GameObject.Destroy(MenuPatch.menu);
				MenuPatch.menu = null;
				MenuPatch.Draw();
			}
		}

		// Token: 0x0600000A RID: 10 RVA: 0x0000381C File Offset: 0x00001A1C
		[PunRPC]
		private static void SetMat(int index)
		{
			Hashtable hashtable = new Hashtable();
			hashtable.Add("matIndex", index);
			PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable, null, null);
		}

		// Token: 0x04000002 RID: 2
		private static float timeSinceLastChange = 0f;

		// Token: 0x04000003 RID: 3
		private static float myVarY1 = 0f;

		// Token: 0x04000004 RID: 4
		private static float myVarY2 = 0f;

		// Token: 0x04000005 RID: 5
		private static bool gain = false;

		// Token: 0x04000006 RID: 6
		private static bool less = false;

		// Token: 0x04000007 RID: 7
		private static bool reset = false;

		// Token: 0x04000008 RID: 8
		private static bool fastr = false;

		// Token: 0x04000009 RID: 9
		private static bool speed1 = true;

		// Token: 0x0400000A RID: 10
		private static float gainSpeed = 1f;

		// Token: 0x0400000B RID: 11
		private static bool down = false;

		// Token: 0x0400000C RID: 12
		private static bool no = false;

		// Token: 0x0400000D RID: 13
		private static bool yes = true;

		// Token: 0x0400000E RID: 14
		private static bool first;

		// Token: 0x0400000F RID: 15
		private static GameObject rightBall;

		// Token: 0x04000010 RID: 16
		private static GameObject leftBall;

		// Token: 0x04000011 RID: 17
		private static Vector3 rightOffset;

		// Token: 0x04000012 RID: 18
		private static Vector3 leftOffset;

		// Token: 0x04000013 RID: 19
		private static bool wait;

		// Token: 0x04000014 RID: 20
		private static bool invert;

		// Token: 0x04000015 RID: 21
		private static string[] buttons = new string[]
		{
			"Toggle Super Monke",
			"MUST ENABLE",
			"Beacons",
			"Turn Off Tag Freeze",
			"Upside Down Monke",
			"Long Arm Monke",
			"Gravity Flipper"
		};

		// Token: 0x04000016 RID: 22
		private static bool?[] buttonsActive = new bool?[]
		{
			new bool?(false),
			new bool?(true),
			new bool?(false),
			new bool?(false),
			new bool?(false),
			new bool?(false),
			new bool?(false)
		};

		// Token: 0x04000017 RID: 23
		private static bool gripDown;

		// Token: 0x04000018 RID: 24
		private static GameObject menu = null;

		// Token: 0x04000019 RID: 25
		private static GameObject canvasObj = null;

		// Token: 0x0400001A RID: 26
		private static GameObject referance = null;

		// Token: 0x0400001B RID: 27
		public static int framePressCooldown = 0;

		// Token: 0x0400001C RID: 28
		private static bool verified = false;

		// Token: 0x0400001D RID: 29
		private static GameObject pointer = null;

		// Token: 0x0400001E RID: 30
		private static bool gravityToggled = false;

		// Token: 0x0400001F RID: 31
		private static bool flying = false;

		// Token: 0x04000020 RID: 32
		private static int btnCooldown = 0;

		// Token: 0x04000021 RID: 33
		private static float? maxJumpSpeed = null;

		// Token: 0x04000022 RID: 34
		private static object index;

		// Token: 0x04000023 RID: 35
		public static int BlueMaterial = 5;

		// Token: 0x04000024 RID: 36
		public static int LavaMaterial = 2;

		// Token: 0x04000025 RID: 37
		public static int RockMaterial = 1;

		// Token: 0x04000026 RID: 38
		public static int DefaultMaterial = 5;

		// Token: 0x04000027 RID: 39
		public static int Redmaterial = 3;

		// Token: 0x04000028 RID: 40
		public static int self = 0;
	}
}
