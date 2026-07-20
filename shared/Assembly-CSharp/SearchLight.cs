using System;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;
using VLB;

public class SearchLight : IOEntity
{
	public static class SearchLightFlags
	{
		public const Flags PlayerUsing = Flags.Reserved5;
	}

	public GameObject pitchObject;

	public GameObject yawObject;

	public GameObject eyePoint;

	public SoundPlayer turnLoop;

	public bool needsBuildingPrivilegeToUse = true;

	[SerializeField]
	private GameObject lightParent;

	[SerializeField]
	private Light[] lights;

	[SerializeField]
	private float[] initialLightIntensity;

	[SerializeField]
	private VolumetricLightBeam vlb;

	[SerializeField]
	private GameObject flare;

	[SerializeField]
	private SoundPlayer humLoopSound;

	[SerializeField]
	private SoundPlayer turnOffSound;

	[SerializeField]
	private SoundPlayer turnOnSound;

	[SerializeField]
	private AnimationCurve lightLerpCurve;

	public Vector3 aimDir = Vector3.zero;

	[ClientVar(ClientAdmin = true)]
	public static bool debugVolumetrics;

	public BasePlayer mountedPlayer;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("SearchLight.OnRpcMessage"))
		{
			if (rpc == 3611615802u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_UseLight"));
				}
				using (TimeWarning.New("RPC_UseLight"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.MaxDistance.Test(3611615802u, "RPC_UseLight", this, player, 3f))
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
							RPC_UseLight(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_UseLight");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ResetState()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		aimDir = Vector3.zero;
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if (base.isServer && (old & Flags.Reserved8) != Flags.Reserved8 && (next & Flags.Reserved8) == Flags.Reserved8 && IsFlickering())
		{
			Hurt(25f);
		}
	}

	public override int ConsumptionAmount()
	{
		return 10;
	}

	public void Update()
	{
		if (base.isServer && IsMounted())
		{
			MountedUpdate();
		}
	}

	public void PlayerEnter(BasePlayer player)
	{
		if (IsMounted() && (Object)(object)player != (Object)(object)mountedPlayer)
		{
			return;
		}
		PlayerExit();
		if ((Object)(object)player != (Object)null)
		{
			mountedPlayer = player;
			using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
			flagsUpdateScope.Set(Flags.Reserved5, b: true);
		}
	}

	public void PlayerExit()
	{
		if (Object.op_Implicit((Object)(object)mountedPlayer))
		{
			mountedPlayer = null;
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved5, b: false);
	}

	public void MountedUpdate()
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)mountedPlayer == (Object)null || mountedPlayer.IsSleeping() || !mountedPlayer.IsAlive() || mountedPlayer.IsWounded() || Vector3.Distance(((Component)mountedPlayer).transform.position, ((Component)this).transform.position) > 2f)
		{
			PlayerExit();
			return;
		}
		Vector3 targetAimpoint = eyePoint.transform.position + mountedPlayer.eyes.BodyForward() * 100f;
		SetTargetAimpoint(targetAimpoint);
		SendNetworkUpdate();
	}

	public void SetTargetAimpoint(Vector3 worldPos)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		Transform transform = ((Component)this).transform;
		Vector3 val = worldPos - eyePoint.transform.position;
		aimDir = transform.InverseTransformDirection(((Vector3)(ref val)).normalized);
	}

	public bool IsMounted()
	{
		return (Object)(object)mountedPlayer != (Object)null;
	}

	[RPC_Server]
	[RPC_Server.MaxDistance(3f)]
	public void RPC_UseLight(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		bool flag = msg.read.Bit();
		if ((!flag || !IsMounted()) && (!needsBuildingPrivilegeToUse || msg.player.CanBuild()))
		{
			if (flag)
			{
				PlayerEnter(player);
			}
			else
			{
				PlayerExit();
			}
		}
	}

	public override void OnDied(HitInfo info)
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.On, b: false);
		}
		base.OnDied(info);
	}

	public override void Save(SaveInfo info)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		info.msg.autoturret = Pool.Get<AutoTurret>();
		info.msg.autoturret.aimDir = aimDir;
	}

	public override void Load(LoadInfo info)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg.autoturret != null)
		{
			aimDir = info.msg.autoturret.aimDir;
		}
	}
}
