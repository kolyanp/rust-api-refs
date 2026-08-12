using Rust;
using UnityEngine;

public class TunnelDweller : HumanNPC
{
	public static readonly Phrase TunnelDwellerName;

	private const string DWELLER_KILL_STAT = "dweller_kills_while_moving";

	public override string displayName => TunnelDwellerName.translated;

	protected override void OnKilledByPlayer(BasePlayer p)
	{
		base.OnKilledByPlayer(p);
		if (Rust.GameInfo.HasAchievements && (Object)(object)p.GetParentEntity() != (Object)null && p.GetParentEntity() is TrainEngine { CurThrottleSetting: not TrainEngine.EngineSpeeds.Zero, IsMovingOrOn: not false })
		{
			p.stats.Add("dweller_kills_while_moving", 1, Stats.All);
			p.stats.Save(forceSteamSave: true);
		}
	}

	static TunnelDweller()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		TunnelDwellerName = new Phrase("npc_tunneldweller", "Tunnel Dweller");
	}
}
