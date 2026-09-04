using System;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class OrientableLight : SimpleLight
{
	public Transform pivotOrigin;

	public Transform yaw;

	public Transform pitch;

	public bool pivotAutoAdjust;

	[Space]
	public Vector2 pitchClamp;

	public Vector2 yawClamp;

	[Space]
	public float serverLerpSpeed;

	public float clientLerpSpeed;

	[Space]
	public GameObjectRef reorientEffect;

	public const Flags Flag_FacingDown = Flags.Reserved18;

	private float pitchAmount;

	private float yawAmount;

	private float lastPitchAmount;

	private float lastYawAmount;

	public static Phrase TipPhrase;

	private bool IsFacingDown => HasFlag(Flags.Reserved18);

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("OrientableLight.OnRpcMessage"))
		{
			if (rpc == 3353964129u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_SetDir"));
				}
				using (TimeWarning.New("SERVER_SetDir"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3353964129u, "SERVER_SetDir", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(3353964129u, "SERVER_SetDir", this, player, 3f))
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
							SERVER_SetDir(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in SERVER_SetDir");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override int ConsumptionAmount()
	{
		return 5;
	}

	public void UpdateRotation(float delta)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		if (base.isServer)
		{
			Quaternion val = Quaternion.Euler(pitchAmount, 0f, 0f);
			Quaternion val2 = Quaternion.Euler(0f, yawAmount, 0f);
			((Component)pitch).transform.localRotation = Mathx.Lerp(((Component)pitch).transform.localRotation, val, serverLerpSpeed, delta);
			((Component)yaw).transform.localRotation = Mathx.Lerp(((Component)yaw).transform.localRotation, val2, serverLerpSpeed, delta);
		}
	}

	private void ResetRotation()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		lastPitchAmount = 0f;
		lastYawAmount = 0f;
		((Component)yaw).transform.localRotation = Quaternion.identity;
		((Component)pitch).transform.localRotation = Quaternion.identity;
		SetPivot();
	}

	private void SetPivot()
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (pivotAutoAdjust)
		{
			if (IsFacingDown)
			{
				pivotOrigin.localRotation = Quaternion.Euler(90f, 0f, 180f);
			}
			else
			{
				pivotOrigin.localRotation = Quaternion.identity;
			}
		}
	}

	public override void ServerInit()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		bool b = Mathf.Abs(Vector3.Dot(((Component)this).transform.forward, Vector3.up)) > 0.9f;
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved18, b);
		}
		SetPivot();
	}

	public void ServerTick()
	{
		if (!base.IsDestroyed)
		{
			UpdateRotation(Time.deltaTime);
		}
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		ResetRotation();
		TryScheduleServerTick();
		ClientRPC(RpcTarget.Player("CLIENT_OnDeployed", deployedBy));
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(3uL)]
	public void SERVER_SetDir(RPCMessage msg)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = msg.player;
		if (player.CanBuild())
		{
			Vector3 val = Vector3Ex.Direction(player.eyes.position, ((Component)yaw).transform.position);
			Ray val2 = player.eyes.HeadRay();
			Vector3 normWorld = ((Ray)(ref val2)).direction;
			Vector3 normalized = ((Vector3)(ref normWorld)).normalized;
			normWorld = Vector3.Lerp(val, normalized, 0.3f);
			Vector3 normalized2 = ((Vector3)(ref normWorld)).normalized;
			Quaternion val3 = Quaternion.LookRotation(Quaternion.Inverse(pivotOrigin.rotation) * normalized2);
			Vector3 val4 = BaseMountable.ConvertVector(((Quaternion)(ref val3)).eulerAngles);
			float num = val4.x;
			float num2 = val4.y;
			if (!IsFacingDown)
			{
				num = Mathf.Clamp(num, pitchClamp.x, pitchClamp.y);
				num2 = Mathf.Clamp(num2, yawClamp.x, yawClamp.y);
			}
			pitchAmount += Mathf.DeltaAngle(pitchAmount, num);
			yawAmount += Mathf.DeltaAngle(yawAmount, num2);
			if (reorientEffect.isValid)
			{
				string resourcePath = reorientEffect.resourcePath;
				Vector3 position = ((Component)this).transform.position;
				normWorld = default(Vector3);
				Effect.server.Run(resourcePath, position, normWorld);
			}
			TryScheduleServerTick();
			SendNetworkUpdate();
		}
	}

	private void TryScheduleServerTick()
	{
		if (lastPitchAmount != pitchAmount || lastYawAmount != yawAmount)
		{
			InvokeRepeating(ServerTick, 0f, 0f);
			Invoke(delegate
			{
				CancelInvoke(ServerTick);
			}, 5f);
		}
		lastPitchAmount = pitchAmount;
		lastYawAmount = yawAmount;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.msg.rcEntity == null)
		{
			info.msg.rcEntity = Pool.Get<RCEntity>();
		}
		info.msg.rcEntity.aim.x = pitchAmount;
		info.msg.rcEntity.aim.y = yawAmount;
		info.msg.rcEntity.aim.z = 0f;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.rcEntity != null && base.isServer)
		{
			pitchAmount = info.msg.rcEntity.aim.x;
			yawAmount = info.msg.rcEntity.aim.y;
		}
	}

	public OrientableLight()
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		pivotAutoAdjust = true;
		pitchClamp = new Vector2(-50f, 50f);
		yawClamp = new Vector2(-50f, 50f);
		serverLerpSpeed = 15f;
		clientLerpSpeed = 10f;
		base._002Ector();
	}

	static OrientableLight()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		TipPhrase = new Phrase("gametip_spotlight", "Use a hammer to adjust the spotlight direction");
	}
}
