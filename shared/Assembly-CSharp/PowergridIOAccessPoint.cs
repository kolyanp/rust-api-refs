using UnityEngine;

public class PowergridIOAccessPoint : IOEntity, IPowergridEntity
{
	[Header("Powergrid IO Access Point")]
	public Phrase displayName;

	[Tooltip("Minimum wire slack enforced on wires connected to this entity's ports.")]
	public float minWireSlack = 0.8f;

	[Header("Electricity Loop Sound")]
	public GameObject soundEmitterObject;

	public SoundDefinition electricityLoopSound;

	private int currentPowergridStage;

	public override bool IsRootEntity()
	{
		return false;
	}

	public override float GetMinWireSlack(bool isInput, int slotIndex)
	{
		return minWireSlack;
	}

	public override int MaximalPowerOutput()
	{
		return 9999;
	}

	public override int ConsumptionAmount()
	{
		return 0;
	}

	public override bool AllowWireConnections()
	{
		if (Powergrid.enabled)
		{
			return base.AllowWireConnections();
		}
		return false;
	}

	public override bool CanSkipWireToolBuildAuthorisation()
	{
		return true;
	}

	public override bool CanBreakConnection(BasePlayer player, int slotIndex, bool isInput)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (!TryGetConnectedTo(slotIndex, isInput, out var connectedTo))
		{
			return base.CanBreakConnection(player, slotIndex, isInput);
		}
		return player.CanBuild(((Component)connectedTo).transform.position, ((Component)connectedTo).transform.rotation, connectedTo.bounds);
	}

	public override Phrase GetDisplayName()
	{
		return displayName;
	}

	bool IPowergridEntity.Server_ShouldConnectToPowergrid()
	{
		return true;
	}

	void IPowergridEntity.Server_OnPowergridStageChanged(int newStage)
	{
		currentPowergridStage = newStage;
		currentEnergy = GetCurrentEnergy();
		MarkDirtyForceUpdateOutputs();
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags);
		flagsUpdateScope.Set(Flags.Reserved8, currentEnergy > 0);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		PowergridManager.Server_AddPowergridAccessPoint(this);
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		PowergridManager.Server_RemovePowergridAccessPoint(this);
	}

	public override int CalculateCurrentEnergy(int inputAmount, int inputSlot)
	{
		return GetCurrentEnergy();
	}

	public override int GetCurrentEnergy()
	{
		if ((Object)(object)PointEntity<PowergridManager>.ServerInstance == (Object)null)
		{
			return 0;
		}
		if (PowergridStageConfig.instance == null)
		{
			return 0;
		}
		if (currentPowergridStage < 1)
		{
			return 0;
		}
		if (!PowergridStageConfig.instance.TryGetStageDataForStage(currentPowergridStage, out var stageData))
		{
			return 0;
		}
		return stageData.powerlineAvailablePower;
	}

	public override bool GetHasPower(int inputAmount, int inputSlot)
	{
		if (Powergrid.enabled && (Object)(object)PointEntity<PowergridManager>.ServerInstance != (Object)null)
		{
			return currentEnergy > 0;
		}
		return false;
	}
}
