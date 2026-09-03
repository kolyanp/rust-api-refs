using System;
using System.Runtime.CompilerServices;
using System.Text;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using Rust;
using Rust.Ai.Gen2.Nav;
using UnityEngine;

public class MonumentBlocker : StagedResourceEntity, LookatHealth.IHealthBarDisplay
{
	[Serializable]
	public class GibbableStageData
	{
		[Tooltip("Destruction stage index that this gibbable is spawned for.")]
		public int stage;

		[Tooltip("Local path of the Gibbable component to spawn.")]
		public string gibbablePath;
	}

	[Tooltip("Gibs spawned on the client when we move into a destruction stage.")]
	[Header("Monument Blocker")]
	public GibbableStageData[] gibbableStages = Array.Empty<GibbableStageData>();

	[Tooltip("Gibs spawned on the client when the blocker is fully destroyed. Falls back to the default gibbing if empty.")]
	public string gibbableOnKilledPath;

	public bool CanDecay = true;

	public float DecayGracePeriodMinutes = 720f;

	public float DecayPerMinuteTick = 1f;

	private float gracePeriodTimer;

	private TimeSince lastDecayTick;

	private float pendingGatherDamage;

	private float __sync_HealthSync;

	[Sync(Autosave = true)]
	public float HealthSync
	{
		[CompilerGenerated]
		get
		{
			return __sync_HealthSync;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_HealthSync, value))
			{
				__sync_HealthSync = value;
				byte nameID = __GetWeaverID("HealthSync");
				QueueSyncVar(nameID);
			}
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("MonumentBlocker.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ServerInit()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		RebuildNavigation();
		HealthSync = health;
		if (CanDecay)
		{
			lastDecayTick = TimeSince.op_Implicit(0f);
			InvokeRepeating(DecayTick, 60f, 60f);
		}
	}

	public override void DestroyShared()
	{
		base.DestroyShared();
		if (base.isServer)
		{
			RebuildNavigation();
		}
	}

	private void RebuildNavigation()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		if (!AI.useUnityNavmesh)
		{
			RustNavigation instance = RustNavigation.Instance;
			if (!((Object)(object)instance == (Object)null))
			{
				OBB val = WorldSpaceBounds();
				instance.RebuildTilesInBounds(((OBB)(ref val)).ToBounds());
			}
		}
	}

	protected override void UpdateNetworkStage()
	{
		int num = stage;
		base.UpdateNetworkStage();
		if (stage != num && !(Health() <= 0f))
		{
			ClientRPC(RpcTarget.NetworkGroup("Client_SpawnGibStage"), stage);
		}
	}

	public override void OnDied(HitInfo info)
	{
		isKilled = true;
		Kill(DestroyMode.Gib);
	}

	protected override void OnHealthChanged()
	{
		base.OnHealthChanged();
		HealthSync = health;
	}

	private void DecayTick()
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		gracePeriodTimer += TimeSince.op_Implicit(lastDecayTick);
		if (gracePeriodTimer > DecayGracePeriodMinutes * 60f)
		{
			OnAttacked(new HitInfo(null, this, DamageType.Decay, DecayPerMinuteTick));
		}
		lastDecayTick = TimeSince.op_Implicit(0f);
	}

	public void AddGracePeriodTime(float seconds)
	{
		gracePeriodTimer = Mathf.Max(0f, gracePeriodTimer + seconds);
	}

	public string GetDebugStatus()
	{
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		float num = DecayGracePeriodMinutes * 60f;
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(string.Format("{0} ({1}) at {2}", base.ShortPrefabName, ((object)System.Runtime.CompilerServices.Unsafe.As<NetworkableId, NetworkableId>(ref net?.ID)/*cast due to constrained. prefix*/).ToString() ?? "no net id", ((Component)this).transform.position));
		stringBuilder.AppendLine(string.Format("  health: {0:0.##} / {1:0.##} ({2:P1}), synced to clients as {3:0.##}", new object[4]
		{
			health,
			MaxHealth(),
			(MaxHealth() > 0f) ? (health / MaxHealth()) : 0f,
			HealthSync
		}));
		stringBuilder.AppendLine($"  stage: {stage}, killed: {isKilled}");
		stringBuilder.AppendLine($"  CanDecay: {CanDecay}, DecayPerMinuteTick: {DecayPerMinuteTick}");
		stringBuilder.AppendLine($"  gracePeriodTimer: {gracePeriodTimer:0.##}s / {num:0.##}s ({DecayGracePeriodMinutes} minutes)");
		if (!CanDecay)
		{
			stringBuilder.AppendLine("  decaying: no - CanDecay is false, so no decay tick is running");
		}
		else if (gracePeriodTimer > num)
		{
			stringBuilder.AppendLine($"  decaying: yes - grace period passed {gracePeriodTimer - num:0.##}s ago");
		}
		else
		{
			stringBuilder.AppendLine($"  decaying: no - {num - gracePeriodTimer:0.##}s of grace period left");
		}
		stringBuilder.AppendLine($"  last decay tick: {TimeSince.op_Implicit(lastDecayTick):0.##}s ago");
		return stringBuilder.ToString();
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.forDisk)
		{
			info.msg.monumentBlocker = Pool.Get<MonumentBlocker>();
			info.msg.monumentBlocker.gracePeriodTimer = gracePeriodTimer;
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.fromDisk)
		{
			if (info.msg.monumentBlocker != null)
			{
				gracePeriodTimer = info.msg.monumentBlocker.gracePeriodTimer;
			}
			if (info.msg.resource != null)
			{
				health = info.msg.resource.health;
				HealthSync = health;
				UpdateNetworkStage();
			}
		}
	}

	public override void OnAttacked(HitInfo info)
	{
		if (base.isServer && !isKilled)
		{
			if (baseProtection != null)
			{
				baseProtection.Scale(info.damageTypes);
			}
			float num = info.damageTypes.Total();
			if (info.CanGather && (Object)(object)resourceDispenser != (Object)null && info.damageTypes.IsMeleeType())
			{
				GiveGatherResources(info, num);
			}
			health -= num;
			if (health <= 0f)
			{
				GiveFinishBonus(info);
				OnDied(info);
			}
			else
			{
				OnHealthChanged();
			}
		}
	}

	private void GiveGatherResources(HitInfo info, float damage)
	{
		pendingGatherDamage += damage;
		pendingGatherDamage -= resourceDispenser.GiveResourcesForDamage(info.InitiatorPlayer, pendingGatherDamage, GetDestroyFraction(info), info.Weapon);
	}

	private void GiveFinishBonus(HitInfo info)
	{
		if (!((Object)(object)resourceDispenser == (Object)null) && !((Object)(object)info.InitiatorPlayer == (Object)null) && info.CanGather && info.damageTypes.IsMeleeType())
		{
			float destroyFraction = GetDestroyFraction(info);
			if (!(destroyFraction >= resourceDispenser.maxDestroyFractionForFinishBonus))
			{
				resourceDispenser.AssignFinishBonus(info.InitiatorPlayer, 1f - destroyFraction, info.Weapon);
			}
		}
	}

	private float GetDestroyFraction(HitInfo info)
	{
		if (!(info.Weapon is BaseMelee baseMelee))
		{
			return 0f;
		}
		return baseMelee.GetGatherInfoFromIndex(resourceDispenser.gatherType)?.destroyFraction ?? 0f;
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
				Debug.Log((object)("SyncVar Writing: HealthSync for " + ((object)(*(NetworkableId*)(&iD))/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_HealthSync);
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
				_ = __sync_HealthSync;
				float _sync_HealthSync = reader.Float();
				__sync_HealthSync = _sync_HealthSync;
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
		if (propertyName == "HealthSync")
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
		__sync_HealthSync = 0f;
	}

	protected override bool ShouldInvalidateCache(byte id)
	{
		if (id == 0)
		{
			return true;
		}
		return base.ShouldInvalidateCache(id);
	}
}
