using UnityEngine;
using UnityEngine.Events;

public class EntityFlag_Toggle : EntityComponent<BaseEntity>, IOnPostNetworkUpdate, IOnSendNetworkUpdate, IPrefabPreProcess
{
	private enum FlagCheck
	{
		All,
		Any
	}

	public bool runClientside;

	[Tooltip("Server-side only works if the EntityFlag_Toggle is on the same GameObject as the Entity")]
	public bool runServerside;

	public BaseEntity.Flags flag;

	[Tooltip("If multiple flags are defined in 'flag', should they all be set, or any?")]
	[SerializeField]
	private FlagCheck flagCheck;

	[SerializeField]
	[Tooltip("Specify any flags that must NOT be on for this toggle to be on")]
	private BaseEntity.Flags notFlag;

	[SerializeField]
	private UnityEvent onFlagEnabled;

	[SerializeField]
	private UnityEvent onFlagDisabled;

	internal bool hasRunOnce;

	internal bool lastToggleOn;

	bool IPrefabPreProcess.CanRunDuringBundling => true;

	protected void OnDisable()
	{
		hasRunOnce = false;
		lastToggleOn = false;
	}

	public void DoUpdate(BaseEntity entity)
	{
		bool flag = ((flagCheck == FlagCheck.All) ? entity.HasFlag(this.flag) : entity.HasAnyFlag(this.flag));
		if (entity.HasAnyFlag(notFlag))
		{
			flag = false;
		}
		if (!hasRunOnce || flag != lastToggleOn)
		{
			hasRunOnce = true;
			lastToggleOn = flag;
			if (flag)
			{
				onFlagEnabled.Invoke();
			}
			else
			{
				onFlagDisabled.Invoke();
			}
			OnStateToggled(flag);
		}
	}

	protected virtual void OnStateToggled(bool state)
	{
	}

	public void OnPostNetworkUpdate(BaseEntity entity)
	{
		if (!((Object)(object)base.baseEntity != (Object)(object)entity) && runClientside)
		{
			DoUpdate(entity);
		}
	}

	public void OnSendNetworkUpdate(BaseEntity entity)
	{
		if (runServerside)
		{
			DoUpdate(entity);
		}
	}

	public void PreProcess(IPrefabProcessor process, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		if ((!clientside || !runClientside) && (!serverside || !runServerside))
		{
			process.RemoveComponent((Component)(object)this);
		}
	}

	public EntityFlag_Toggle()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Expected O, but got Unknown
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Expected O, but got Unknown
		runClientside = true;
		runServerside = true;
		onFlagEnabled = new UnityEvent();
		onFlagDisabled = new UnityEvent();
		base._002Ector();
	}
}
