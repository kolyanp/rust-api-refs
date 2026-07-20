using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

[ConsoleSystem.Factory("cctv")]
public class CCTV_RC : PoweredRemoteControlEntity, IRemoteControllableClientCallbacks, IRemoteControllable
{
	public class CCTVWorkQueue : PersistentObjectWorkQueue<CCTV_RC>
	{
		protected override void RunJob(CCTV_RC entity)
		{
			entity.ServerTick();
		}
	}

	public Transform pivotOrigin;

	public Transform yaw;

	public Transform pitch;

	public Vector2 pitchClamp = new Vector2(-50f, 50f);

	public Vector2 yawClamp = new Vector2(-50f, 50f);

	public float turnSpeed = 25f;

	public float serverLerpSpeed = 15f;

	public float clientLerpSpeed = 10f;

	public float zoomLerpSpeed = 10f;

	public float[] fovScales;

	public float pitchAmount;

	public float yawAmount;

	public int fovScaleIndex;

	public float fovScaleLerped = 1f;

	public bool hasPTZ = true;

	public AnimationCurve dofCurve = AnimationCurve.Constant(0f, 1f, 0f);

	public float dofApertureMax = 10f;

	public const Flags Flag_HasViewer = Flags.Reserved5;

	public bool disableWhenShot = true;

	[ServerVar(Name = "camera_disable_seconds")]
	public static float CameraDisableSeconds = 300f;

	public SoundDefinition movementLoopSoundDef;

	public AnimationCurve movementLoopGainCurve;

	public float movementLoopSmoothing = 1f;

	public float movementLoopReference = 50f;

	private Sound movementLoop;

	private SoundModulation.Modulator movementLoopGainModulator;

	public SoundDefinition zoomInSoundDef;

	public SoundDefinition zoomOutSoundDef;

	public RealTimeSinceEx timeSinceLastServerTick;

	[ServerVar(Saved = true)]
	public static float inputBudgetMs = 0.05f;

	public static CCTVWorkQueue WorkQueue = new CCTVWorkQueue();

	public override bool RequiresMouse => hasPTZ;

	protected override bool EntityCanPing => true;

	private bool skipMoveUpdate => rcControls == RemoteControllableControls.None;

	public override bool CanAcceptInput => hasPTZ;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("CCTV_RC.OnRpcMessage"))
		{
			if (rpc == 3353964129u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_SetDir"));
				}
				using (TimeWarning.New("Server_SetDir"))
				{
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
							Server_SetDir(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in Server_SetDir");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override int ConsumptionAmount()
	{
		return 3;
	}

	public override void ServerInit()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		if (!base.isClient)
		{
			if (IsStatic())
			{
				pitchAmount = pitch.localEulerAngles.x;
				yawAmount = yaw.localEulerAngles.y;
				UpdateRCAccess(isOnline: true);
			}
			timeSinceLastServerTick = 0.0;
			if (!skipMoveUpdate)
			{
				((PersistentObjectWorkQueue<CCTV_RC>)WorkQueue).Add(this);
			}
		}
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		if (!skipMoveUpdate)
		{
			((PersistentObjectWorkQueue<CCTV_RC>)WorkQueue).Remove(this);
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		UpdateRotation(10000f);
	}

	public override bool ShouldInheritNetworkGroup()
	{
		if ((Object)(object)((Component)this).GetComponentInParent<CargoShip>() != (Object)null)
		{
			return false;
		}
		return base.ShouldInheritNetworkGroup();
	}

	public override void UserInput(InputState inputState, CameraViewerId viewerID)
	{
		if (UpdateManualAim(inputState))
		{
			SendNetworkUpdate();
		}
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
		info.msg.rcEntity.zoom = fovScaleIndex;
	}

	public override void Hurt(HitInfo info)
	{
		DamageType majorityDamageType = info.damageTypes.GetMajorityDamageType();
		if ((uint)(majorityDamageType - 9) <= 2u || (uint)(majorityDamageType - 15) <= 1u)
		{
			TryDisableCamera();
		}
		base.Hurt(info);
	}

	private void TryDisableCamera()
	{
		CancelInvoke(EndCameraDisable);
		if (disableWhenShot)
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.OnFire, b: true);
			}
			Invoke(EndCameraDisable, CameraDisableSeconds);
		}
	}

	private void EndCameraDisable()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.OnFire, b: false);
	}

	[RPC_Server]
	public void Server_SetDir(RPCMessage msg)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		if (!IsStatic())
		{
			BasePlayer player = msg.player;
			if (player.CanBuild() && player.IsBuildingAuthed() && Interface.CallHook("OnCCTVDirectionChange", this, player) == null)
			{
				Vector3 val = Vector3Ex.Direction(player.eyes.position, ((Component)yaw).transform.position);
				val = ((Component)this).transform.InverseTransformDirection(val);
				Quaternion val2 = Quaternion.LookRotation(val);
				Vector3 val3 = BaseMountable.ConvertVector(((Quaternion)(ref val2)).eulerAngles);
				pitchAmount = Mathf.Clamp(val3.x, pitchClamp.x, pitchClamp.y);
				yawAmount = Mathf.Clamp(val3.y, yawClamp.x, yawClamp.y);
				SendNetworkUpdate();
			}
		}
	}

	public void SetLookingAtPoint(Vector3 point)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3Ex.Direction(point, ((Component)yaw).transform.position);
		val = ((Component)this).transform.InverseTransformDirection(val);
		Quaternion val2 = Quaternion.LookRotation(val);
		Vector3 val3 = BaseMountable.ConvertVector(((Quaternion)(ref val2)).eulerAngles);
		pitchAmount = Mathf.Clamp(val3.x, pitchClamp.x, pitchClamp.y);
		yawAmount = Mathf.Clamp(val3.y, yawClamp.x, yawClamp.y);
		SendNetworkUpdate();
	}

	public override bool InitializeControl(CameraViewerId viewerID)
	{
		bool result = base.InitializeControl(viewerID);
		UpdateViewers();
		return result;
	}

	public override void StopControl(CameraViewerId viewerID)
	{
		base.StopControl(viewerID);
		UpdateViewers();
	}

	public void UpdateViewers()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved5, base.ViewerCount > 0);
	}

	public void ServerTick()
	{
		if (base.isClient || base.IsDestroyed || (double)timeSinceLastServerTick < 0.014999999664723873)
		{
			return;
		}
		float delta = (float)(double)timeSinceLastServerTick;
		timeSinceLastServerTick = 0.0;
		using (TimeWarning.New("UpdateRotation"))
		{
			UpdateRotation(delta);
		}
		if (!HasFlag(Flags.Reserved5) || isStatic)
		{
			return;
		}
		bool b = IsObstructedByGeometry();
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Locked, b);
	}

	private bool IsObstructedByGeometry()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = viewEyes.position - viewEyes.forward * 0.1f;
		List<Collider> list = Pool.Get<List<Collider>>();
		GamePhysics.OverlapSphere(val, 0.17f, list, 2162688, (QueryTriggerInteraction)1);
		bool flag = false;
		foreach (Collider item in list)
		{
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(item);
			if (!((Object)(object)baseEntity != (Object)null) || !((Object)(object)baseEntity == (Object)(object)this))
			{
				flag = true;
			}
		}
		if (!flag)
		{
			flag = GamePhysics.Trace(new Ray(val, viewEyes.forward), 0f, out var _, 0.2f, 2162688, (QueryTriggerInteraction)0);
		}
		Pool.FreeUnmanaged<Collider>(ref list);
		return flag;
	}

	public bool UpdateManualAim(InputState inputState)
	{
		if (!hasPTZ)
		{
			return false;
		}
		float num = 0f - inputState.current.mouseDelta.y;
		float x = inputState.current.mouseDelta.x;
		bool flag = inputState.WasJustPressed(BUTTON.FIRE_PRIMARY);
		pitchAmount = Mathf.Clamp(pitchAmount + num * turnSpeed, pitchClamp.x, pitchClamp.y);
		yawAmount = Mathf.Clamp(yawAmount + x * turnSpeed, yawClamp.x, yawClamp.y) % 360f;
		if (flag)
		{
			fovScaleIndex = (fovScaleIndex + 1) % fovScales.Length;
		}
		return num != 0f || x != 0f || flag;
	}

	public void UpdateRotation(float delta)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		Quaternion val = Quaternion.Euler(pitchAmount, 0f, 0f);
		Quaternion val2 = Quaternion.Euler(0f, yawAmount, 0f);
		float num = ((base.isServer && !base.IsBeingControlled) ? serverLerpSpeed : clientLerpSpeed);
		pitch.localRotation = Mathx.Lerp(pitch.localRotation, val, num, delta);
		yaw.localRotation = Mathx.Lerp(yaw.localRotation, val2, num, delta);
		if (fovScales != null && fovScales.Length != 0)
		{
			if (fovScales.Length > 1)
			{
				fovScaleLerped = Mathx.Lerp(fovScaleLerped, fovScales[fovScaleIndex], zoomLerpSpeed, delta);
			}
			else
			{
				fovScaleLerped = fovScales[0];
			}
		}
		else
		{
			fovScaleLerped = 1f;
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.rcEntity != null)
		{
			int num = Mathf.Clamp((int)info.msg.rcEntity.zoom, 0, fovScales.Length - 1);
			if (base.isServer)
			{
				pitchAmount = info.msg.rcEntity.aim.x;
				yawAmount = info.msg.rcEntity.aim.y;
				fovScaleIndex = num;
			}
		}
	}

	public override float GetFovScale()
	{
		return fovScaleLerped;
	}
}
