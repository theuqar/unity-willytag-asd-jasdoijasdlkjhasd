using System;
using UnityEngine;
using UnityEngine.UI;

namespace GorillaModManager
{
	// Token: 0x02000003 RID: 3
	public class Nametag : MonoBehaviour
	{
		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000006 RID: 6 RVA: 0x0000214C File Offset: 0x0000034C
		public Text Text
		{
			get
			{
				return this._text;
			}
		}

		// Token: 0x06000007 RID: 7 RVA: 0x00002068 File Offset: 0x00000268
		private void Awake()
		{
			this.Init();
		}

		// Token: 0x06000008 RID: 8 RVA: 0x00002164 File Offset: 0x00000364
		public void ChangeText(string name)
		{
			bool flag = this._text == null;
			if (flag)
			{
				this.Init();
			}
			this._text.gameObject.SetActive(true);
			this._text.text = name;
		}

		// Token: 0x06000009 RID: 9 RVA: 0x00002072 File Offset: 0x00000272
		private void Init()
		{
			this._text = base.GetComponent<Text>();
		}

		// Token: 0x04000004 RID: 4
		private Text _text;
	}
}