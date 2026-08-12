using UnityEngine;

public class MagnetLiftable : EntityComponent<BaseEntity>
{
	public ItemAmount[] shredResources;

	public bool scaleScrapResourcesByHealth;

	public Vector3 shredDirection;

	public bool requireObjectOff;

	public BasePlayer associatedPlayer { get; private set; }

	public virtual void SetMagnetized(bool wantsOn, BaseMagnet magnetSource, BasePlayer player)
	{
		associatedPlayer = player;
	}

	public MagnetLiftable()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		shredDirection = Vector3.forward;
		base._002Ector();
	}
}
