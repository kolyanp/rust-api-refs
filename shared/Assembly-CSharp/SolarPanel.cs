using Oxide.Core;
using UnityEngine;

public class SolarPanel : IOEntity
{
	public class SunUpdateWorkQueue : PersistentObjectWorkQueue<SolarPanel>
	{
		protected override void RunJob(SolarPanel entity)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			if (!(TimeSince.op_Implicit(entity.lastSunUpdate) < 5f))
			{
				entity.lastSunUpdate = TimeSince.op_Implicit(Random.Range(-2f, 0f));
				entity.SunUpdate();
			}
		}
	}

	public Transform sunSampler;

	public int maximalPowerOutput = 10;

	public float dot_minimum = 0.1f;

	public float dot_maximum = 0.6f;

	[ServerVar(Saved = true)]
	public static float sunUpdateBudgetMs = 0.05f;

	public static SunUpdateWorkQueue WorkQueue = new SunUpdateWorkQueue();

	private TimeSince lastSunUpdate;

	public override bool IsRootEntity()
	{
		return true;
	}

	public override int MaximalPowerOutput()
	{
		return maximalPowerOutput;
	}

	public override int ConsumptionAmount()
	{
		return 0;
	}

	public override void ServerInit()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		lastSunUpdate = TimeSince.op_Implicit(-4f);
		((PersistentObjectWorkQueue<SolarPanel>)WorkQueue).Add(this);
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		((PersistentObjectWorkQueue<SolarPanel>)WorkQueue).Remove(this);
	}

	public void SunUpdate()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		int num;
		if (TOD_Sky.Instance.IsNight)
		{
			num = 0;
		}
		else
		{
			Vector3 sunDirection = TOD_Sky.Instance.SunDirection;
			float num2 = Vector3.Dot(sunSampler.forward, sunDirection);
			float num3 = Mathf.InverseLerp(dot_minimum, dot_maximum, num2);
			if (num3 > 0f && !IsVisible(sunSampler.position + sunDirection * 100f, 101f))
			{
				num3 = 0f;
			}
			num = Mathf.FloorToInt((float)maximalPowerOutput * num3 * base.healthFraction);
		}
		bool num4 = currentEnergy != num;
		currentEnergy = num;
		if (num4 && Interface.CallHook("OnSolarPanelSunUpdate", this, num) == null)
		{
			MarkDirty();
		}
	}

	public override int GetPassthroughAmount(int outputSlot = 0)
	{
		if (outputSlot != 0)
		{
			return 0;
		}
		return currentEnergy;
	}
}
