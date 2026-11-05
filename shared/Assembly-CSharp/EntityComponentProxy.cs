using UnityEngine;

public abstract class EntityComponentProxy<T> : EntityComponentProxyBase where T : BaseEntity
{
	protected readonly T baseEntity;

	public GameObject gameObject => ((Component)baseEntity).gameObject;

	public Transform transform => ((Component)baseEntity).transform;

	protected EntityComponentProxy(T owner)
	{
		baseEntity = owner;
	}

	public override BaseEntity GetBaseEntity()
	{
		return baseEntity;
	}
}
