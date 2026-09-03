using System;

public static class RustProxies
{
	[Obsolete("Use BasePlayer.SendCompleteSnapshot instead.")]
	public static void SendFullSnapshot(this BasePlayer player)
	{
		player.SendCompleteSnapshot();
	}

	[Obsolete("Use PlayerMetabolism.SendChanges instead.")]
	public static void SendChangesToClient(this PlayerMetabolism metabolism)
	{
		metabolism.SendChanges();
	}

	[Obsolete("Use BaseEntity.StartSetFlags or BaseEntity.SetFlagLocal instead.")]
	public unsafe static void SetFlag(this BaseEntity entity, Flags f, bool b, bool recursive = false, bool networkupdate = true)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		FlagsUpdateScope val = entity.StartSetFlags((FlagsUpdateMode)(networkupdate ? 2 : 0));
		try
		{
			((FlagsUpdateScope)(ref val)).Set(f, b, recursive);
		}
		finally
		{
			((IDisposable)(*(FlagsUpdateScope*)(&val))/*cast due to constrained. prefix*/).Dispose();
		}
	}
}
