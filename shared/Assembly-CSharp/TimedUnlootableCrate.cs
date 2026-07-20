public class TimedUnlootableCrate : LootContainer
{
	public bool unlootableOnSpawn = true;

	public float unlootableDuration = 300f;

	public override void ServerInit()
	{
		base.ServerInit();
		if (unlootableOnSpawn)
		{
			SetUnlootableFor(unlootableDuration);
		}
	}

	public void SetUnlootableFor(float duration)
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.OnFire, b: true);
			flagsUpdateScope.Set(Flags.Locked, b: true);
		}
		unlootableDuration = duration;
		Invoke(MakeLootable, duration);
	}

	public void MakeLootable()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.OnFire, b: false);
		flagsUpdateScope.Set(Flags.Locked, b: false);
	}
}
