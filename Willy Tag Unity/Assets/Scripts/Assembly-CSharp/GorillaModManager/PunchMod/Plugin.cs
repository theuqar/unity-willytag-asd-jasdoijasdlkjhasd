using System;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

namespace PunchMod
{
	// Token: 0x02000003 RID: 3
	public class Plugin : MonoBehaviour
	{
		// Token: 0x06000006 RID: 6 RVA: 0x000020F0 File Offset: 0x000002F0
		private void Update()
		{
			Keyboard current = Keyboard.current;
			bool wasPressedThisFrame = current.zKey.wasPressedThisFrame;
			if (wasPressedThisFrame)
			{
				this.stop = !this.stop;
				bool flag = this.stop;
				if (flag)
				{
					foreach (PunchHandManager punchHandManager in GameObject.FindObjectsOfType<PunchHandManager>())
					{
						GameObject.Destroy(punchHandManager);
					}
				}
			}
			bool flag2 = !this.stop;
			if (flag2)
			{
				foreach (TwoBoneIKConstraint twoBoneIKConstraint in GameObject.Find("GorillaVRRigs").GetComponentsInChildren<TwoBoneIKConstraint>())
				{
					bool flag3 = !twoBoneIKConstraint.GetComponentInParent<PhotonView>().IsMine;
					if (flag3)
					{
						bool flag4 = !twoBoneIKConstraint.gameObject.GetComponentInChildren<PunchHandManager>();
						if (flag4)
						{
							twoBoneIKConstraint.gameObject.GetComponentInChildren<AudioSource>().gameObject.AddComponent<PunchHandManager>();
						}
						bool flag5 = twoBoneIKConstraint.gameObject.GetComponentInChildren<PunchHandManager>().strength != this.strengths;
						if (flag5)
						{
							twoBoneIKConstraint.gameObject.GetComponentInChildren<PunchHandManager>().strength = this.strengths;
						}
					}
				}
			}
		}

		// Token: 0x04000004 RID: 4
		public bool stop;

		// Token: 0x04000005 RID: 5
		private float strengths = 300f;
	}
}