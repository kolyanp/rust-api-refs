using System;
using System.Runtime.CompilerServices;
using ConVar;
using Facepunch;
using Network;
using UnityEngine;

public class SwingSeat : BaseVehicleSeat
{
	public Transform pivot;

	public Vector3 swingAxis;

	public float minAngle;

	public float maxAngle;

	public float pumpAcceleration;

	public float gravityRestore;

	public float damping;

	public float launchScale;

	public float launchMinSpeed;

	[Range(0f, 1f)]
	public float launchLift;

	public bool heavyLanding;

	public bool flailOnLaunch;

	public ChildAnimatorSubSystem swingAnimator;

	public AnimationCurve swingBlendCurve;

	public SoundDefinition swingMovementLoopDef;

	public SoundDefinition swingMovementAccentDef;

	public AnimationCurve swingMovementGainCurve;

	public float swingMovementSmoothing;

	public AnimationCurve swingMovementAccentGainCurve;

	public const Flags Flag_Swinging = Flags.Reserved1;

	private const float SwingingVelocityThreshold = 5f;

	private const float SwingingAngleThreshold = 5f;

	private Quaternion pivotBaseRot;

	private bool cachedBaseRot;

	private float velocity;

	private int inputSign;

	private Action swingTick;

	private float __sync_SwingAngle;

	[Sync(Autosave = true)]
	public float SwingAngle
	{
		[CompilerGenerated]
		get
		{
			return __sync_SwingAngle;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_SwingAngle, value))
			{
				__sync_SwingAngle = value;
				byte nameID = __GetWeaverID("SwingAngle");
				QueueSyncVar(nameID);
			}
		}
	}

	public bool IsSwinging()
	{
		return HasFlag(Flags.Reserved1);
	}

	private void ApplySwing(float angle)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)pivot == (Object)null))
		{
			if (!cachedBaseRot)
			{
				pivotBaseRot = pivot.localRotation;
				cachedBaseRot = true;
			}
			pivot.localRotation = pivotBaseRot * Quaternion.AngleAxis(angle, swingAxis);
		}
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		UpdateSwingingFlag();
		if (IsSwinging())
		{
			StartSwingTick();
		}
	}

	public override void AttemptMount(BasePlayer player, bool doMountChecks = true)
	{
		if (!IsSwinging())
		{
			base.AttemptMount(player, doMountChecks);
		}
	}

	public override void OnPlayerMounted()
	{
		base.OnPlayerMounted();
		StartSwingTick();
	}

	private void StartSwingTick()
	{
		if (swingTick == null)
		{
			swingTick = SwingTick;
		}
		if (!IsInvokingFixedTime(swingTick))
		{
			InvokeRepeatingFixedTime(swingTick);
		}
	}

	private void UpdateSwingingFlag()
	{
		SetFlag(Flags.Reserved1, Mathf.Abs(velocity) > 5f || Mathf.Abs(SwingAngle) > 5f);
	}

	public override void OnPlayerDismounted(BasePlayer player)
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		base.OnPlayerDismounted(player);
		if ((Object)(object)pivot != (Object)null && (Object)(object)mountAnchor != (Object)null && (Object)(object)player != (Object)null && Mathf.Abs(velocity) > launchMinSpeed)
		{
			Vector3 val = pivot.TransformDirection(swingAxis);
			Vector3 val2 = ((Vector3)(ref val)).normalized * (velocity * (MathF.PI / 180f));
			Vector3 val3 = mountAnchor.position - pivot.position;
			Vector3 val4 = Vector3.Cross(val2, val3) * launchScale;
			val4 += Vector3.up * (((Vector3)(ref val4)).magnitude * launchLift);
			player.Ragdoll(val4, heavyLanding, flailOnLaunch);
			BaseMountable mounted = player.GetMounted();
			if ((Object)(object)mounted != (Object)null)
			{
				GameObjectExtensions.SetIgnoreCollisions(((Component)mounted).gameObject, ((Component)this).gameObject, true);
			}
		}
		inputSign = 0;
	}

	public override void PlayerServerInput(InputState inputState, BasePlayer player)
	{
		base.PlayerServerInput(inputState, player);
		bool flag = inputState.IsDown(BUTTON.BACKWARD);
		bool flag2 = inputState.IsDown(BUTTON.FORWARD);
		inputSign = ((flag != flag2) ? (flag ? 1 : (-1)) : 0);
	}

	private void SwingTick()
	{
		float fixedDeltaTime = Time.fixedDeltaTime;
		velocity += (float)inputSign * pumpAcceleration * fixedDeltaTime;
		velocity -= gravityRestore * Mathf.Sin(SwingAngle * (MathF.PI / 180f)) * fixedDeltaTime;
		velocity -= velocity * damping * fixedDeltaTime;
		float num = SwingAngle + velocity * fixedDeltaTime;
		if (num > maxAngle)
		{
			num = maxAngle;
			velocity = 0f;
		}
		else if (num < minAngle)
		{
			num = minAngle;
			velocity = 0f;
		}
		SwingAngle = num;
		ApplySwing(num);
		if (!AnyMounted() && Mathf.Abs(velocity) < 0.5f && Mathf.Abs(num) < 0.5f)
		{
			velocity = 0f;
			SwingAngle = 0f;
			ApplySwing(0f);
			CancelInvokeFixedTime(swingTick);
		}
		UpdateSwingingFlag();
	}

	protected unsafe override bool WriteSyncVar(byte id, NetWrite writer)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (id == 0)
		{
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: SwingAngle for " + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_SwingAngle);
			return true;
		}
		return base.WriteSyncVar(id, writer);
	}

	protected override bool OnSyncVar(byte id, NetRead reader, bool fromAutoSave = false)
	{
		if (id == 0)
		{
			try
			{
				_ = __sync_SwingAngle;
				float _sync_SwingAngle = reader.Float();
				__sync_SwingAngle = _sync_SwingAngle;
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
			return true;
		}
		return base.OnSyncVar(id, reader, fromAutoSave);
	}

	private byte __GetWeaverID(string propertyName)
	{
		if (propertyName == "SwingAngle")
		{
			return 0;
		}
		return byte.MaxValue;
	}

	protected override void WriteAutoSaveSyncVars(NetWrite writer)
	{
		base.WriteAutoSaveSyncVars(writer);
		WriteSyncVar(0, writer);
	}

	protected override void ReadAutoSaveSyncVars(NetRead reader)
	{
		base.ReadAutoSaveSyncVars(reader);
		OnSyncVar(0, reader, fromAutoSave: true);
	}

	protected override bool AutoSaveSyncVars(SaveInfo save)
	{
		NetWrite netWrite = Net.sv.StartWrite();
		WriteAutoSaveSyncVars(netWrite);
		var (src, num) = netWrite.GetBuffer();
		if (_autosaveBuffer == null)
		{
			_autosaveBuffer = BaseEntity._autosaveBufferPool.Rent(num);
		}
		if (_autosaveBuffer.Length < num)
		{
			BaseEntity._autosaveBufferPool.Return(_autosaveBuffer);
			_autosaveBuffer = BaseEntity._autosaveBufferPool.Rent(num);
		}
		Buffer.BlockCopy(src, 0, _autosaveBuffer, 0, num);
		save.msg.baseEntity.syncVars = _autosaveBuffer;
		Pool.Free<NetWrite>(ref netWrite);
		return true;
	}

	protected override bool AutoLoadSyncVars(LoadInfo load)
	{
		if (load.msg.baseEntity != null && load.msg.baseEntity.syncVars != null)
		{
			NetRead netRead = Pool.Get<NetRead>();
			netRead.Init(load.msg.baseEntity.syncVars.AsSpan());
			ReadAutoSaveSyncVars(netRead);
			Pool.Free<NetRead>(ref netRead);
		}
		return true;
	}

	protected override void ResetSyncVars()
	{
		base.ResetSyncVars();
		__sync_SwingAngle = 0f;
	}

	protected override bool ShouldInvalidateCache(byte id)
	{
		if (id == 0)
		{
			return true;
		}
		return base.ShouldInvalidateCache(id);
	}

	public SwingSeat()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		swingAxis = Vector3.right;
		launchScale = 1f;
		launchMinSpeed = 20f;
		launchLift = 0.25f;
		flailOnLaunch = true;
		swingMovementSmoothing = 4f;
		base._002Ector();
	}
}
