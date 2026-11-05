public class CatapultAnimationEvents : FacepunchBehaviour
{
	public Catapult catapult;

	public SoundDefinition reloadClickSoundDef;

	[UnityEvent]
	public void PlayReloadClick()
	{
	}

	[UnityEvent]
	public void PlaySound(SoundDefinition def)
	{
	}
}
