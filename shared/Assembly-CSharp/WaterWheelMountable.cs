using UnityEngine;

public class WaterWheelMountable : BaseMountable
{
	public static readonly Phrase NeedSustenance = new Phrase("waterwheel.needSustenance", "Too hungry to use...");

	public const Flags PlayerRunningInside = Flags.Reserved11;

	public float caloriesRequired = 10f;

	public float calorieDrainPerMinute = 10f;

	public float hydrationDrainPerMinute = 10f;

	private ElectricWaterWheel _waterWheel;

	private static readonly int PushingWaterwheel = Animator.StringToHash("pushingWaterWheel");

	private TimeSince lastToastWarning;

	private ElectricWaterWheel waterWheel
	{
		get
		{
			if ((Object)(object)_waterWheel == (Object)null)
			{
				_waterWheel = GetParentEntity() as ElectricWaterWheel;
			}
			return _waterWheel;
		}
	}

	public override void PlayerServerInput(InputState inputState, BasePlayer player)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		base.PlayerServerInput(inputState, player);
		bool flag = inputState.IsDown(BUTTON.FORWARD);
		bool flag2 = player.metabolism.calories.value < caloriesRequired;
		if (flag2 && TimeSince.op_Implicit(lastToastWarning) > 2f)
		{
			player.ShowToast(GameTip.Styles.Red_Normal, NeedSustenance, false);
			lastToastWarning = TimeSince.op_Implicit(0f);
		}
		if (flag && !flag2)
		{
			player.metabolism.calories.value -= calorieDrainPerMinute / 60f * Time.deltaTime;
			player.metabolism.hydration.value -= hydrationDrainPerMinute / 60f * Time.deltaTime;
			player.metabolism.SendChanges();
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved11, AnyMounted() && !flag2 && flag);
	}

	public override void OnPlayerDismounted(BasePlayer player)
	{
		base.OnPlayerDismounted(player);
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved11, b: false);
	}

	public override void AttemptMount(BasePlayer player, bool doMountChecks = true)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		if (IsMountingFromOpenSide(((Component)player).transform.position))
		{
			base.AttemptMount(player, doMountChecks);
		}
	}

	public bool IsMountingFromOpenSide(Vector3 position)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = position - mountAnchor.position;
		return Vector3.Dot(((Vector3)(ref val)).normalized, ((Component)this).transform.forward) > 0.2f;
	}
}
