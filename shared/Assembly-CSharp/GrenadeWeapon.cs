public class GrenadeWeapon : ThrownWeapon
{
	public const Flags Flag_IsPreparingDrop = Flags.Reserved1;

	public const string AttachSignalArg = "attach";

	private static bool NextDropState(bool primaryDown, bool secondaryDown, bool wasPreparingDrop)
	{
		if (primaryDown)
		{
			return false;
		}
		if (secondaryDown)
		{
			return true;
		}
		return wasPreparingDrop;
	}

	public override void ServerTick(BasePlayer byPlayer)
	{
		base.ServerTick(byPlayer);
		InputState serverInput = byPlayer.serverInput;
		bool wasPreparingDrop = NextDropState(serverInput.WasDown(BUTTON.FIRE_PRIMARY), serverInput.WasDown(BUTTON.FIRE_SECONDARY), HasFlag(Flags.Reserved1));
		bool b = NextDropState(serverInput.IsDown(BUTTON.FIRE_PRIMARY), serverInput.IsDown(BUTTON.FIRE_SECONDARY), wasPreparingDrop);
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags);
		flagsUpdateScope.Set(Flags.Reserved1, b);
	}
}
