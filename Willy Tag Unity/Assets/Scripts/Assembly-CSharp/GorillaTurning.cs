using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GorillaTurning : GorillaTriggerBox
{
	public Material redMaterial;

	public Material blueMaterial;

	public Material greenMaterial;

	public Material transparentBlueMaterial;

	public Material transparentRedMaterial;

	public Material transparentGreenMaterial;

	public MeshRenderer smoothTurnBox;

	public MeshRenderer snapTurnBox;

	public MeshRenderer noTurnBox;

	public string currentChoice;

	public float currentSpeed;

	private void Awake()
	{
	}
}
