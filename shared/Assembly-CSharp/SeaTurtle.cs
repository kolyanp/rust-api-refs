using Rust.Ai.Gen2;

public class SeaTurtle : SwimmingNPC, ISimpleHearingReceiver
{
	public const Flags Fleeing = Flags.Reserved8;

	[ServerVar]
	public static float forceSurfaceAmount = 0f;

	[ServerVar(Help = "Population active on the server, per square km", ShowInAdminUI = true)]
	public static float Population = 2f;

	private TimeSince lastLoudNoise;

	private TimeSince timeSinceLastConnectionsCheck;

	private bool noConnectionsDisable;

	public bool RecentlyHeardLoudNoise
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return TimeSince.op_Implicit(lastLoudNoise) < 10f;
		}
	}

	private float TimeOutConnectionCheck
	{
		get
		{
			if (!noConnectionsDisable)
			{
				return 5f;
			}
			return 15f;
		}
	}

	protected override float ForceSurfaceAmount => forceSurfaceAmount;

	public bool IsFleeing()
	{
		return HasFlag(Flags.Reserved8);
	}

	public override void ServerInit()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		lastLoudNoise = TimeSince.op_Implicit(100f);
	}

	public void OnHeardNoise(NpcNoiseEvent noise)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (noise.Intensity >= NpcNoiseIntensity.Medium)
		{
			lastLoudNoise = TimeSince.op_Implicit(0f);
		}
	}

	public void SetFleeing(bool toggle)
	{
		SetNetworkedFlag(Flags.Reserved8, toggle);
	}

	protected override float GetDesiredSpeed()
	{
		if (!IsFleeing())
		{
			return minSpeed;
		}
		return maxSpeed;
	}

	public override float GetTurnSpeed()
	{
		if (IsFleeing())
		{
			return maxTurnSpeed;
		}
		return base.GetTurnSpeed();
	}
}
