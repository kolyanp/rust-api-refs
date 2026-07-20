using Network;
using UnityEngine;

public class Toolgun : Hammer
{
	public GameObjectRef attackEffect;

	public GameObjectRef beamEffect;

	public GameObjectRef beamImpactEffect;

	public GameObjectRef errorEffect;

	public GameObjectRef beamEffectClassic;

	public GameObjectRef beamImpactEffectClassic;

	public Transform muzzlePoint;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Toolgun.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void DoAttackShared(HitInfo info)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (base.isServer)
		{
			ClientRPC(RpcTarget.NetworkGroup("EffectSpawn"), info.HitPositionWorld, info.HitNormalWorld);
		}
		base.DoAttackShared(info);
	}
}
