public class SoundPostMuteButton : GorillaPressableButton
{
	public SynchedMusicController musicController;

	public override void ButtonActivation()
	{
		base.ButtonActivation();
		musicController.MuteAudio(this);
	}
}
