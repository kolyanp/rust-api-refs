using UnityEngine;

public class RepairableStructure : ConstructableEntity
{
	public const Flags Flag_Repaired = Flags.Reserved1;

	[Tooltip("Root holding the damaged build stages. Switched off once fully repaired.")]
	[Header("Repairable Structure")]
	public GameObject damagedVisuals;

	[Tooltip("Root holding the intact visuals. Only shown once fully repaired.")]
	public GameObject repairedVisuals;

	public GameObjectRef repairedEffect;

	public bool IsRepaired => HasFlag(Flags.Reserved1);

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		bool flag = (old & Flags.Reserved1) == Flags.Reserved1;
		bool flag2 = (next & Flags.Reserved1) == Flags.Reserved1;
		if (flag != flag2)
		{
			UpdateRepairedVisuals();
		}
	}

	private void UpdateRepairedVisuals()
	{
		bool isRepaired = IsRepaired;
		if ((Object)(object)damagedVisuals != (Object)null)
		{
			damagedVisuals.SetActive(!isRepaired);
		}
		if ((Object)(object)repairedVisuals != (Object)null)
		{
			repairedVisuals.SetActive(isRepaired);
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		UpdateRepairedVisuals();
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		SetRepaired(IsFullyBuilt());
	}

	protected override void OnConstructionComplete(BasePlayer player)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		SetRepaired(repaired: true);
		if (repairedEffect.isValid)
		{
			Effect.server.Run(repairedEffect.resourcePath, ((Component)this).transform.position, Vector3.up);
		}
	}

	public void ForceRepairedState(bool repaired)
	{
		ForceBuildProgress(repaired);
		SetRepaired(repaired);
	}

	private void SetRepaired(bool repaired)
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved1, repaired);
	}
}
