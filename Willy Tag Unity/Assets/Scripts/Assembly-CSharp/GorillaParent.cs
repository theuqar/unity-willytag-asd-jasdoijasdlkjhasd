using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

public class GorillaParent : MonoBehaviour
{
	public GameObject tagUI;

	public GameObject playerParent;

	public GameObject vrrigParent;

	public static volatile GorillaParent instance;

	public List<VRRig> vrrigs;

	public Dictionary<Player, VRRig> vrrigDict = new Dictionary<Player, VRRig>();

	private int i;

	public void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			Object.Destroy(base.gameObject);
		}
	}

	public void Update()
	{
		for (i = vrrigs.Count - 1; i > -1; i--)
		{
			if (vrrigs[i] == null)
			{
				vrrigs.RemoveAt(i);
			}
		}
	}
}
