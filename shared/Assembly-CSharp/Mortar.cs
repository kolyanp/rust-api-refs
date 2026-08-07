using System;
using ConVar;
using Network;
using Prefabs.Deployable.Mortar;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class Mortar : Cannon
{
	[SerializeField]
	[Header("Mortar")]
	private Vector2 minMaxDistance = new Vector2(0f, 200f);

	[SerializeField]
	private AnimationCurve distanceRandomnessCurve;

	[SerializeField]
	private AnimationCurve distanceRandomnessXCurve;

	[SerializeField]
	private AnimationCurve distanceRandomnessZCurve;

	[SerializeField]
	private Vector2 shotPitchRecoilMinMax = new Vector2(0f, 2.5f);

	[Header("Mortar Animation")]
	public ChildAnimatorSubSystem mortarAnim;

	[SerializeField]
	private AnimationCurve reloadHandIkWeightCurve;

	[SerializeField]
	private AnimationCurve firingHandIkWeightCurve;

	[SerializeField]
	private AnimationCurve reloadPitchBlendCurve;

	[SerializeField]
	private float remoteAimDirSmoothSpeed;

	[Header("Condition")]
	[SerializeField]
	private float conditionLossPerShot;

	[Header("Recoil")]
	[SerializeField]
	private AnimationClip recoilLowAnimation;

	[SerializeField]
	private AnimationClip recoilMediumAnimation;

	[SerializeField]
	private AnimationClip recoilHighAnimation;

	[SerializeField]
	private AnimationCurve recoilPitchCurve;

	[SerializeField]
	private float recoilPitchDuration;

	[Header("Mortar Handle")]
	[SerializeField]
	private Transform handleBone;

	[SerializeField]
	private AnimationCurve handleMinMaxRotation;

	[SerializeField]
	[Header("Display")]
	private MortarDisplay mortarDisplayPrefab;

	[ClientVar(ClientAdmin = true)]
	public static bool DebugDistanceUi;

	public override bool RunInLateUpdate
	{
		get
		{
			if (runInLateUpdate)
			{
				return base.isClient;
			}
			return false;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Mortar.OnRpcMessage"))
		{
			if (rpc == 2658947749u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RequestLightFuse"));
				}
				using (TimeWarning.New("RequestLightFuse"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2658947749u, "RequestLightFuse", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(2658947749u, "RequestLightFuse", this, player, 3f))
						{
							return true;
						}
						if (!RPC_Server.MaxDistance.Test(2658947749u, "RequestLightFuse", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg2 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RequestLightFuse(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RequestLightFuse");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	private float GetDesiredDistance()
	{
		return Mathf.Lerp(minMaxDistance.x, minMaxDistance.y, GetPitch01());
	}

	private float GetPitch01()
	{
		float aimingPitch = GetAimingPitch();
		float num = Mathf.Clamp(Mathf.DeltaAngle(0f, aimingPitch), pitchClamp.x, pitchClamp.y);
		return Mathf.InverseLerp(pitchClamp.x, pitchClamp.y, num);
	}

	private float GetAimingPitch()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (aimDir == Vector3.zero)
		{
			return 0f;
		}
		Quaternion val = Quaternion.LookRotation(aimDir, ((Component)this).transform.up);
		return ((Quaternion)(ref val)).eulerAngles.x;
	}

	protected override bool TryGetPitchOverride(float basePitch, out float overridePitch, out float overrideWeight)
	{
		overridePitch = basePitch;
		overrideWeight = 0f;
		return false;
	}

	protected override bool ShouldApplyAimDir()
	{
		return true;
	}

	protected override bool UnableToStartReloadServer(BasePlayer player)
	{
		if (!base.UnableToStartReloadServer(player))
		{
			return !CanSeeFirePoint(player, 0.05f);
		}
		return true;
	}

	protected override void Server_OnReloadStarted()
	{
	}

	protected override void LoadAmmo(BasePlayer player)
	{
		if (!CanSeeFirePoint(player, 0.05f))
		{
			reloadProgress = 0f;
			return;
		}
		base.LoadAmmo(player);
		if (!IsLoaded())
		{
			reloadProgress = 0f;
			return;
		}
		Fire(player);
		ApplyServerPitchRecoil();
	}

	protected override void Fire(BasePlayer firingPlayer, float minSpeed = 100f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		Vector2 randomFireOffset = GetRandomFireOffset();
		float requiredVelocity = GetRequiredVelocity(randomFireOffset.y);
		ItemModProjectile itemModProjectile = default(ItemModProjectile);
		if (((Component)magazine.ammoType).TryGetComponent<ItemModProjectile>(ref itemModProjectile) && FireProjectile(itemModProjectile.GetOverrideProjectile(this), FirePoint.position, FirePoint.forward, firingPlayer, 0.25f, requiredVelocity, out var projectile))
		{
			SERVER_OnProjectileFired(firingPlayer.Connection, firingPlayer);
			Hurt(conditionLossPerShot, DamageType.Generic, null, useProtection: false);
			if (projectile is MortarServerProjectile projectile2)
			{
				ApplyLateralCurve(projectile2, randomFireOffset.x, requiredVelocity);
			}
		}
	}

	private Vector2 GetRandomFireOffset()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		float pitch = GetPitch01();
		float num = distanceRandomnessCurve.Evaluate(pitch);
		float num2 = Random.Range(0f - num, num) * distanceRandomnessXCurve.Evaluate(pitch);
		float num3 = Random.Range(0f - num, num) * distanceRandomnessZCurve.Evaluate(pitch);
		return new Vector2(num2, num3);
	}

	private void ApplyLateralCurve(MortarServerProjectile projectile, float lateralOffset, float forwardSpeed)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		if (Mathf.Abs(lateralOffset) <= Mathf.Epsilon)
		{
			return;
		}
		Vector3 forward = FirePoint.forward;
		Vector2 val = new Vector2(forward.x, forward.z);
		float magnitude = ((Vector2)(ref val)).magnitude;
		float num = forwardSpeed * magnitude;
		if (!(num <= Mathf.Epsilon))
		{
			float num2 = GetDesiredDistance() / num;
			if (!(num2 <= Mathf.Epsilon))
			{
				Vector3 val2 = new Vector3(forward.x, 0f, forward.z);
				Vector3 normalized = ((Vector3)(ref val2)).normalized;
				Vector3 val3 = Vector3.Cross(Vector3.up, normalized);
				float num3 = 2f * lateralOffset / (num2 * num2);
				projectile.StartLateralCurve(val3 * num3, num2);
			}
		}
	}

	public override void RequestLightFuse(RPCMessage msg)
	{
	}

	private float GetRequiredVelocity(float distanceOffset = 0f)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		GameObject obj = AmmoPrefab.Get();
		ServerProjectile serverProjectile = ((obj != null) ? obj.GetComponent<ServerProjectile>() : null);
		Vector3 initialVelocity = (((Object)(object)serverProjectile != (Object)null) ? serverProjectile.initialVelocity : Vector3.zero);
		float gravityModifier = (((Object)(object)serverProjectile != (Object)null) ? serverProjectile.gravityModifier : 1f);
		float num = (((Object)(object)serverProjectile != (Object)null) ? (serverProjectile.speed + Vector3.Dot(serverProjectile.initialVelocity, FirePoint.forward)) : 0f);
		float num2 = CalculateDesiredLaunchVelocity(FirePoint.position, GetProjectileDestination(distanceOffset), FirePoint.forward, initialVelocity, gravityModifier);
		if (!float.IsFinite(num2))
		{
			return Mathf.Max(num, 0f);
		}
		return Mathf.Max(new float[3] { num2, num, 0f });
	}

	private Vector3 GetProjectileDestination(float distanceOffset = 0f)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.position + ((Component)this).transform.forward * (GetDesiredDistance() + distanceOffset);
	}

	private void ApplyServerPitchRecoil()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		float num = Random.Range(shotPitchRecoilMinMax.x, shotPitchRecoilMinMax.y);
		if (!(num <= Mathf.Epsilon))
		{
			Quaternion val = Quaternion.LookRotation(aimDir, ((Component)this).transform.up);
			Vector3 eulerAngles = ((Quaternion)(ref val)).eulerAngles;
			float num2 = Mathf.Clamp(Mathf.DeltaAngle(0f, eulerAngles.x) - num, pitchClamp.x, pitchClamp.y);
			if (num2 < 0f)
			{
				num2 += 360f;
			}
			eulerAngles.x = num2;
			aimDir = Quaternion.Euler(eulerAngles) * Vector3.forward;
			SendAimDirImmediate(force: true);
		}
	}

	private static float CalculateDesiredLaunchVelocity(Vector3 throwPos, Vector3 targetPos, Vector3 aimDir, Vector3 initialVelocity, float gravityModifier)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		aimDir = ((Vector3)(ref aimDir)).normalized;
		Vector3 val = targetPos - throwPos;
		Vector2 val2 = new Vector2(val.x, val.z);
		float magnitude = ((Vector2)(ref val2)).magnitude;
		float y = val.y;
		val2 = new Vector2(aimDir.x, aimDir.z);
		float magnitude2 = ((Vector2)(ref val2)).magnitude;
		if (magnitude <= Mathf.Epsilon || magnitude2 <= Mathf.Epsilon)
		{
			return 0f;
		}
		float y2 = aimDir.y;
		float num = Physics.gravity.y * gravityModifier;
		float num2 = Vector3.Dot(initialVelocity, aimDir);
		Vector3 val3 = initialVelocity - aimDir * num2;
		Vector2 val4 = new Vector2(aimDir.x, aimDir.z) / magnitude2;
		float num3 = Vector2.Dot(new Vector2(val3.x, val3.z), val4);
		float y3 = val3.y;
		float num4 = y2 / magnitude2;
		float num5 = y - magnitude * num4;
		float num6 = (0f - magnitude) * (y3 - num4 * num3);
		float num7 = -0.5f * num * magnitude * magnitude;
		float num8;
		if (Mathf.Abs(num5) <= Mathf.Epsilon)
		{
			if (Mathf.Abs(num6) <= Mathf.Epsilon)
			{
				return 0f;
			}
			num8 = (0f - num7) / num6;
		}
		else
		{
			float num9 = num6 * num6 - 4f * num5 * num7;
			if (num9 < 0f)
			{
				return 0f;
			}
			float num10 = Mathf.Sqrt(num9);
			float num11 = -0.5f * (num6 + Mathf.Sign(num6) * num10);
			float num12 = num11 / num5;
			float num13 = ((Mathf.Abs(num11) > Mathf.Epsilon) ? (num7 / num11) : float.PositiveInfinity);
			num8 = float.PositiveInfinity;
			if (num12 > 0f)
			{
				num8 = num12;
			}
			if (num13 > 0f && num13 < num8)
			{
				num8 = num13;
			}
			if (!float.IsFinite(num8))
			{
				return 0f;
			}
		}
		return (num8 - num3) / magnitude2;
	}
}
