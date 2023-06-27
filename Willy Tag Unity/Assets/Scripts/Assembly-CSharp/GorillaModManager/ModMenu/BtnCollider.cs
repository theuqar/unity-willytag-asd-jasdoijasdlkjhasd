using System;
using UnityEngine;

namespace WristMenu
{
	// Token: 0x02000002 RID: 2
	internal class BtnCollider : MonoBehaviour
	{
		// Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		private void OnTriggerEnter(Collider collider)
		{
			bool flag = Time.frameCount >= MenuPatch.framePressCooldown + 30;
			bool flag2 = flag;
			bool flag3 = flag2;
			if (flag3)
			{
				MenuPatch.Toggle(this.relatedText);
				MenuPatch.framePressCooldown = Time.frameCount;
			}
		}

		// Token: 0x04000001 RID: 1
		public string relatedText;
	}
}
