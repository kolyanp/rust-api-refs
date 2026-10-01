using ConVar;
using UnityEngine;

public class BiofuelStirMount : BaseMountable
{
	public override bool ValidDismountPosition(BasePlayer player, Vector3 disPos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		if (GamePhysics.CheckCapsule(GamePhysics.Realm.Server, disPos + Vector3.up * 0.5f, disPos + Vector3.up * 1.3f, 0.5f, 1537286401, (QueryTriggerInteraction)0))
		{
			return false;
		}
		Collider col;
		return !AntiHack.TestNoClipping(player, GetDismountCheckStart(player), disPos + BasePlayer.NoClipOffset(), BasePlayer.NoClipRadius(ConVar.AntiHack.noclip_margin_dismount), ConVar.AntiHack.noclip_backtracking, out col, overlapVehicleLayer: false, GetParentEntity());
	}
}
