using GorillaNetworking;

public class WardrobeItemButton : GorillaPressableButton
{
	public HeadModel controlledModel;

	public CosmeticsController.CosmeticItem currentCosmeticItem;

	public override void ButtonActivation()
	{
		base.ButtonActivation();
		CosmeticsController.instance.PressWardrobeItemButton(currentCosmeticItem);
	}
}
