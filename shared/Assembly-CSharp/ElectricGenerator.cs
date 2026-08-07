using Facepunch;
using ProtoBuf;
using UnityEngine;

public class ElectricGenerator : IOEntity, IPowergridEntity
{
	public enum PowergridDisabledOption
	{
		PoweredOff,
		UseElectricAmount
	}

	[Header("Electric Generator")]
	public float electricAmount = 8f;

	[Tooltip("Powergrid must be at least this stage to be powered on. Stage 0 is powergrid off, meaning this generator would be unaffected by changes in the powergrid.")]
	public int requiredPowergridStage;

	[Tooltip("What should this generator do if requiredPowergridStage > 0 but servervar 'powergrid.enabled' is false?")]
	public PowergridDisabledOption ifPowergridDisabled;

	private int currentPowergridStage;

	public override bool IsRootEntity()
	{
		return true;
	}

	public override int MaximalPowerOutput()
	{
		return Mathf.FloorToInt(electricAmount);
	}

	public override int ConsumptionAmount()
	{
		return 0;
	}

	bool IPowergridEntity.Server_ShouldConnectToPowergrid()
	{
		return requiredPowergridStage > 0;
	}

	void IPowergridEntity.Server_OnPowergridStageChanged(int newStage)
	{
		currentPowergridStage = newStage;
		currentEnergy = GetCurrentEnergy();
		UpdateHasPower(currentEnergy, 0);
		MarkDirtyForceUpdateOutputs();
	}

	public override int CalculateCurrentEnergy(int inputAmount, int inputSlot)
	{
		return GetCurrentEnergy();
	}

	public override int GetCurrentEnergy()
	{
		int result = (int)electricAmount;
		if (requiredPowergridStage > 0)
		{
			if (Powergrid.enabled && (Object)(object)PointEntity<PowergridManager>.ServerInstance != (Object)null)
			{
				if (currentPowergridStage < requiredPowergridStage)
				{
					return 0;
				}
				return result;
			}
			if (ifPowergridDisabled == PowergridDisabledOption.PoweredOff)
			{
				return 0;
			}
		}
		return result;
	}

	public override bool GetHasPower(int inputAmount, int inputSlot)
	{
		if (requiredPowergridStage > 0)
		{
			if (Powergrid.enabled && (Object)(object)PointEntity<PowergridManager>.ServerInstance != (Object)null)
			{
				if (currentPowergridStage >= requiredPowergridStage)
				{
					return base.GetHasPower(inputAmount, inputSlot);
				}
				return false;
			}
			if (ifPowergridDisabled == PowergridDisabledOption.PoweredOff)
			{
				return false;
			}
		}
		return base.GetHasPower(inputAmount, inputSlot);
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		return GetCurrentEnergy();
	}

	public override void UpdateOutputs()
	{
		currentEnergy = GetCurrentEnergy();
		IOSlot[] array = outputs;
		foreach (IOSlot iOSlot in array)
		{
			IOEntity iOEntity = iOSlot.connectedTo.Get();
			if ((Object)(object)iOEntity != (Object)null)
			{
				iOEntity.UpdateFromInput(currentEnergy, iOSlot.connectedToSlot);
			}
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		Invoke(ForcePuzzleReset, 4f);
	}

	private void ForcePuzzleReset()
	{
		PuzzleReset component = ((Component)this).GetComponent<PuzzleReset>();
		if ((Object)(object)component != (Object)null)
		{
			component.DoReset();
			component.ResetTimer();
		}
	}

	public override void Save(SaveInfo info)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		if (!info.forDisk)
		{
			return;
		}
		PuzzleReset puzzleReset = default(PuzzleReset);
		if (((Component)this).TryGetComponent<PuzzleReset>(ref puzzleReset))
		{
			info.msg.puzzleReset = Pool.Get<PuzzleReset>();
			info.msg.puzzleReset.playerBlocksReset = puzzleReset.playersBlockReset;
			if ((Object)(object)puzzleReset.playerDetectionOrigin != (Object)null)
			{
				info.msg.puzzleReset.playerDetectionOrigin = puzzleReset.playerDetectionOrigin.position;
			}
			info.msg.puzzleReset.playerDetectionRadius = puzzleReset.playerDetectionRadius;
			info.msg.puzzleReset.scaleWithServerPopulation = puzzleReset.scaleWithServerPopulation;
			info.msg.puzzleReset.timeBetweenResets = puzzleReset.timeBetweenResets;
			info.msg.puzzleReset.checkSleepingAIZForPlayers = puzzleReset.CheckSleepingAIZForPlayers;
			info.msg.puzzleReset.ignoreAboveGroundPlayers = puzzleReset.ignoreAboveGroundPlayers;
			info.msg.puzzleReset.broadcastResetMessage = puzzleReset.broadcastResetMessage;
			info.msg.puzzleReset.resetPhrase = puzzleReset.resetPhrase?.token ?? "";
			info.msg.puzzleReset.radiationReset = puzzleReset.radiationReset;
			info.msg.puzzleReset.pauseUntilLooted = puzzleReset.pauseUntilLooted;
		}
		info.msg.electricGenerator = Pool.Get<ElectricGenerator>();
		info.msg.electricGenerator.requiredPowergridStage = requiredPowergridStage;
	}

	public override void Load(LoadInfo info)
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (!info.fromDisk)
		{
			return;
		}
		if (info.msg.puzzleReset != null)
		{
			PuzzleReset component = ((Component)this).GetComponent<PuzzleReset>();
			if ((Object)(object)component != (Object)null)
			{
				component.playersBlockReset = info.msg.puzzleReset.playerBlocksReset;
				if ((Object)(object)component.playerDetectionOrigin != (Object)null)
				{
					component.playerDetectionOrigin.position = info.msg.puzzleReset.playerDetectionOrigin;
				}
				component.playerDetectionRadius = info.msg.puzzleReset.playerDetectionRadius;
				component.scaleWithServerPopulation = info.msg.puzzleReset.scaleWithServerPopulation;
				component.timeBetweenResets = info.msg.puzzleReset.timeBetweenResets;
				component.CheckSleepingAIZForPlayers = info.msg.puzzleReset.checkSleepingAIZForPlayers;
				component.ignoreAboveGroundPlayers = info.msg.puzzleReset.ignoreAboveGroundPlayers;
				component.broadcastResetMessage = info.msg.puzzleReset.broadcastResetMessage;
				if (!string.IsNullOrEmpty(info.msg.puzzleReset.resetPhrase))
				{
					Phrase phrase = Translate.GetPhrase(info.msg.puzzleReset.resetPhrase);
					if (phrase != null)
					{
						component.resetPhrase = phrase;
					}
				}
				component.radiationReset = info.msg.puzzleReset.radiationReset;
				component.pauseUntilLooted = info.msg.puzzleReset.pauseUntilLooted;
				component.ResetTimer();
			}
		}
		if (info.msg.electricGenerator != null)
		{
			bool flag = requiredPowergridStage > 0;
			requiredPowergridStage = info.msg.electricGenerator.requiredPowergridStage;
			if (requiredPowergridStage > 0 && !flag)
			{
				PowergridManager.Server_AddPowergridEntity(this);
			}
		}
	}
}
