using GorillaNetworking;

public class ModeSelectButton : GorillaPressableButton
{
	public string gameMode;

	public override void ButtonActivation()
	{
		base.ButtonActivation();
		GorillaComputer.instance.OnModeSelectButtonPress(gameMode);
	}
}
