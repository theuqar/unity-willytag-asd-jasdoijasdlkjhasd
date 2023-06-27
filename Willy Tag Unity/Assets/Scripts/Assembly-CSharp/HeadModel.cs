using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadModel : MonoBehaviour
{
	public GameObject[] cosmetics;

	private GameObject objRef;

	private List<GameObject> currentActiveObjects = new List<GameObject>();

	private Dictionary<string, GameObject> cosmeticDict = new Dictionary<string, GameObject>();

	private bool initialized;

	public void Awake()
	{
		StartCoroutine(DisableAfterASecond());
	}

	private IEnumerator DisableAfterASecond()
	{
		yield return new WaitForSeconds(0.1f);
		if (!initialized && base.isActiveAndEnabled)
		{
			initialized = true;
			GameObject[] array = cosmetics;
			foreach (GameObject gameObject in array)
			{
				cosmeticDict.Add(gameObject.name, gameObject);
				SetChildRenderers(gameObject, setEnabled: false);
			}
		}
	}

	public void OnEnable()
	{
		Awake();
	}

	public void SetCosmeticActive(string activeCosmeticName)
	{
		foreach (GameObject currentActiveObject in currentActiveObjects)
		{
			SetChildRenderers(currentActiveObject, setEnabled: false);
		}
		currentActiveObjects.Clear();
		if (cosmeticDict.TryGetValue(activeCosmeticName, out objRef))
		{
			currentActiveObjects.Add(objRef);
			SetChildRenderers(objRef, setEnabled: true);
		}
	}

	public void SetCosmeticActiveArray(string[] activeCosmeticNames)
	{
		foreach (GameObject currentActiveObject in currentActiveObjects)
		{
			SetChildRenderers(currentActiveObject, setEnabled: false);
		}
		currentActiveObjects.Clear();
		foreach (string key in activeCosmeticNames)
		{
			if (cosmeticDict.TryGetValue(key, out objRef))
			{
				currentActiveObjects.Add(objRef);
				SetChildRenderers(objRef, setEnabled: true);
			}
		}
	}

	private void SetChildRenderers(GameObject obj, bool setEnabled)
	{
		MeshRenderer[] componentsInChildren = obj.GetComponentsInChildren<MeshRenderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = setEnabled;
		}
	}
}
