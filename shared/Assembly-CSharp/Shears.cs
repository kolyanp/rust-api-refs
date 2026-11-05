using Rust.Ai.Gen2;
using UnityEngine;

public class Shears : BaseMelee
{
	public override void GetAttackStats(HitInfo info)
	{
		base.GetAttackStats(info);
		if ((Object)(object)info.HitEntity != (Object)null && info.HitEntity is Sheep)
		{
			info.damageTypes.Clear();
			info.CanGather = true;
			info.DoHitEffects = false;
		}
	}
}
