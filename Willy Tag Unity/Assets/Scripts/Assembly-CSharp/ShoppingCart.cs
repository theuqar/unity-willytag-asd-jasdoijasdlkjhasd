using UnityEngine;

public class ShoppingCart : MonoBehaviour
{
	public static volatile ShoppingCart instance;

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

	private void Start()
	{
	}

	private void Update()
	{
	}
}
