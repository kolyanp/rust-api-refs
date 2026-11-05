using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using ConVar;
using Facepunch;
using Network;
using Unity.Mathematics;
using UnityEngine;

namespace Rust.Ai.Gen2;

public class SeagullGroup : BaseNPC2, ISimpleHearingReceiver
{
	public const int MaxBirds = 16;

	public const Flags Flying = Flags.Reserved9;

	public const Flags Landing = Flags.Reserved10;

	[SerializeField]
	private Vector2Int birdCountRange = new Vector2Int(4, 16);

	[SerializeField]
	private GameObjectRef birdCorpse;

	[SerializeField]
	private Vector3 flyingExtents = new Vector3(7f, 5f, 7f);

	[Tooltip("Half size of the bounds a hit on a bird is checked against just after the group takes off, while its birds are still bursting out every way and gathering into the flock.")]
	[SerializeField]
	private Vector3 scatterExtents = new Vector3(55f, 15f, 55f);

	[Tooltip("How long after taking off the group checks hits against scatterExtents, in seconds.")]
	[SerializeField]
	private float scatterTime = 20f;

	[SerializeField]
	private BoxCollider serverCollider;

	[ServerVar(Help = "Seagull groups active on the server, per square km", ShowInAdminUI = true)]
	public static float Population = 0.6f;

	[ServerVar(Help = "Share of the deep sea's seagull land points given a group when it opens", ShowInAdminUI = true)]
	public static float DeepSeaFill = 0.5f;

	private const string PrefabPath = "assets/rust.ai/agents/seagull/seagullgroup.prefab";

	private static uint[] birdColliderIds;

	[NonSerialized]
	public SeagullLandPoint LandPoint;

	[NonSerialized]
	public Vector3 HomePosition;

	[NonSerialized]
	public Vector3 Touchdown;

	private TimeSince timeSinceLoudNoise;

	private TimeSince timeSinceTakeoff;

	private Vector3 landingStart;

	private Vector3 velocity;

	private float lastMoveTime;

	private ushort __sync_AliveBirds;

	private Vector3 __sync_StartlePosition;

	private uint __sync_FlightSeed;

	[Sync(Autosave = true)]
	public ushort AliveBirds
	{
		[CompilerGenerated]
		get
		{
			return __sync_AliveBirds;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_AliveBirds, value))
			{
				__sync_AliveBirds = value;
				byte nameID = __GetWeaverID("AliveBirds");
				QueueSyncVar(nameID);
			}
		}
	}

	[Sync(Autosave = true)]
	public Vector3 StartlePosition
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return __sync_StartlePosition;
		}
		[CompilerGenerated]
		set
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (!IsSyncVarEqual<Vector3>(__sync_StartlePosition, value))
			{
				__sync_StartlePosition = value;
				byte nameID = __GetWeaverID("StartlePosition");
				QueueSyncVar(nameID);
			}
		}
	}

	[Sync(Autosave = true)]
	public uint FlightSeed
	{
		[CompilerGenerated]
		get
		{
			return __sync_FlightSeed;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_FlightSeed, value))
			{
				__sync_FlightSeed = value;
				byte nameID = __GetWeaverID("FlightSeed");
				QueueSyncVar(nameID);
			}
		}
	}

	public override bool MovesOnNavmesh => false;

	public bool RecentlyHeardLoudNoise
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return TimeSince.op_Implicit(timeSinceLoudNoise) < 10f;
		}
	}

	public bool IsFlying()
	{
		return HasFlag(Flags.Reserved9);
	}

	public bool IsLanding()
	{
		return HasFlag(Flags.Reserved10);
	}

	private static bool IsBirdAlive(ushort aliveBirds, int bird)
	{
		return (aliveBirds & (1 << bird)) != 0;
	}

	private Bounds LocalBirdBounds()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		if (!IsFlying() && !IsLanding())
		{
			return bounds;
		}
		Bounds result = new Bounds(Vector3.zero, flyingExtents * 2f);
		if (isServer && IsLanding())
		{
			result.Encapsulate(new Bounds(((Component)this).transform.InverseTransformPoint(Touchdown) + bounds.center, bounds.size));
			result.Encapsulate(new Bounds(((Component)this).transform.InverseTransformPoint(landingStart), flyingExtents * 2f));
		}
		return result;
	}

	public override OBB WorldSpaceBounds()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = default;
		Quaternion val2 = default;
		((Component)this).transform.GetPositionAndRotation(ref val, ref val2);
		Bounds val3 = LocalBirdBounds();
		if (isServer && IsFlying() && TimeSince.op_Implicit(timeSinceTakeoff) < scatterTime)
		{
			val3.Encapsulate(new Bounds(Vector3.zero, scatterExtents * 2f));
		}
		return new OBB(val, Vector3.one, val2, val3);
	}

	public void OnHeardNoise(NpcNoiseEvent noise)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (noise.Intensity >= NpcNoiseIntensity.Medium)
		{
			timeSinceLoudNoise = TimeSince.op_Implicit(0f);
		}
	}

	private void SetNetworkedFlag(Flags flag, bool value)
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(flag, value);
	}

	public void SetFlying(bool flying)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		SetNetworkedFlag(Flags.Reserved9, flying);
		FitServerCollider();
		if (flying)
		{
			timeSinceTakeoff = TimeSince.op_Implicit(0f);
		}
		else
		{
			velocity = Vector3.zero;
		}
	}

	private void FitServerCollider()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		Bounds val = LocalBirdBounds();
		serverCollider.center = val.center;
		serverCollider.size = val.size;
	}

	public void SetLanding(bool landing)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		landingStart = ((Component)this).transform.position;
		SetNetworkedFlag(Flags.Reserved10, landing);
		FitServerCollider();
	}

	public void MoveTo(Vector3 position, Quaternion rotation)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		float num = Time.time - lastMoveTime;
		lastMoveTime = Time.time;
		velocity = ((num > 0f) ? ((position - ((Component)this).transform.position) / num) : Vector3.zero);
		((Component)this).transform.SetPositionAndRotation(position, rotation);
	}

	public override Vector3 GetLocalVelocityServer()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return velocity;
	}

	public override void ServerInit()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		timeSinceLoudNoise = TimeSince.op_Implicit(float.MaxValue);
		FitServerCollider();
		HomePosition = ((Component)this).transform.position;
		int num = Random.Range(birdCountRange.x, birdCountRange.y + 1);
		AliveBirds = (ushort)((1 << num) - 1);
	}

	public override void Hurt(HitInfo info)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		if (IsDead() || !info.damageTypes.IsConsideredAnAttack())
		{
			return;
		}
		int num = FindHitBird(info.HitBone, AliveBirds);
		if (num < 0)
		{
			return;
		}
		AliveBirds = (ushort)(AliveBirds & ~(1 << num));
		Vector3 posOnDeath = ((info.HitPositionWorld != Vector3.zero) ? info.HitPositionWorld : CenterPoint());
		BaseCorpse baseCorpse = DropCorpse(birdCorpse.resourcePath, posOnDeath, Quaternion.Euler(0f, Random.Range(0f, 360f), 0f));
		if ((Object)(object)baseCorpse != (Object)null)
		{
			baseCorpse.Spawn();
			Rigidbody val = default;
			if (((Component)baseCorpse).TryGetComponent<Rigidbody>(ref val))
			{
				val.linearVelocity = velocity;
			}
		}
		if (AliveBirds == 0)
		{
			Die(info);
			return;
		}
		SenseComponent senseComponent = default;
		if (((Component)this).TryGetComponent<SenseComponent>(ref senseComponent))
		{
			senseComponent.NoteAttacker(info.Initiator);
		}
		SetJustAttacked();
	}

	private static int FindHitBird(uint hitBone, ushort aliveBirds)
	{
		if (birdColliderIds == null)
		{
			birdColliderIds = CreateBirdColliderIds();
		}
		int num = Array.IndexOf(birdColliderIds, hitBone);
		if (num >= 0)
		{
			if (!IsBirdAlive(aliveBirds, num))
			{
				return -1;
			}
			return num;
		}
		int num2 = Random.Range(0, math.countbits((uint)aliveBirds));
		for (num = 0; num < 16; num++)
		{
			if (IsBirdAlive(aliveBirds, num) && num2-- == 0)
			{
				return num;
			}
		}
		return -1;
	}

	public static void OnDeepSeaSpawned()
	{
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		if (!BaseSpawnPoint.spawnPoints.TryGetValue(BaseSpawnPoint.SpawnPointType.Seagull, out var value))
		{
			return;
		}
		foreach (BaseSpawnPoint item in value)
		{
			if (item is SeagullLandPoint seagullLandPoint && seagullLandPoint.IsAvailableTo(null) && DeepSeaManager.IsInsideDeepSea(((Component)seagullLandPoint).transform.position) && !(Random.value >= DeepSeaFill))
			{
				seagullLandPoint.GetLocation(out var pos, out var rot);
				if (GameManager.server.CreateEntity("assets/rust.ai/agents/seagull/seagullgroup.prefab", pos, rot) is SeagullGroup seagullGroup)
				{
					seagullLandPoint.TryClaim(seagullGroup);
					seagullGroup.LandPoint = seagullLandPoint;
					seagullGroup.Spawn();
				}
			}
		}
	}

	private bool ForceLand(SeagullLandPoint point)
	{
		SeagullGroupFSM seagullGroupFSM = default;
		if (!IsFlying() || (Object)(object)point == (Object)null || !((Component)this).TryGetComponent<SeagullGroupFSM>(ref seagullGroupFSM) || !point.TryClaim(this))
		{
			return false;
		}
		if ((Object)(object)LandPoint != (Object)(object)point)
		{
			LandPoint?.Release(this);
		}
		LandPoint = point;
		seagullGroupFSM.SetState(seagullGroupFSM.land);
		return true;
	}

	[ServerVar(Help = "Startles every landed seagull group within the given radius (default 100) of where you are looking.")]
	public static void startle(ConsoleSystem.Arg args)
	{
		PooledList<SeagullGroup> val = Pool.Get<PooledList<SeagullGroup>>();
		try
		{
			if (!TryGetGroupsNearLook(args, 100f, val, out var _))
			{
				return;
			}
			int num = 0;
			SeagullGroupFSM seagullGroupFSM = default;
			foreach (SeagullGroup item in (List<SeagullGroup>)(object)val)
			{
				if (!item.IsFlying() && ((Component)item).TryGetComponent<SeagullGroupFSM>(ref seagullGroupFSM))
				{
					seagullGroupFSM.SetState(seagullGroupFSM.flyAway);
					num++;
				}
			}
			args.ReplyWith($"Startled {num} seagull group(s).");
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ServerVar(Help = "Lands every flying seagull group within the given radius (default 300) of where you are looking, each on its nearest free land point.")]
	public static void land(ConsoleSystem.Arg args)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		PooledList<SeagullGroup> val = Pool.Get<PooledList<SeagullGroup>>();
		try
		{
			if (!TryGetGroupsNearLook(args, 300f, val, out var _))
			{
				return;
			}
			int num = 0;
			foreach (SeagullGroup item in (List<SeagullGroup>)(object)val)
			{
				if (item.ForceLand(SeagullLandPoint.FindNearestFree(item, ((Component)item).transform.position)))
				{
					num++;
				}
			}
			args.ReplyWith($"Landing {num} seagull group(s).");
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	[ServerVar(Help = "Lands the flying seagull group nearest where you are looking, within the given radius (default 300), on the free land point nearest it.")]
	public static void landat(ConsoleSystem.Arg args)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		PooledList<SeagullGroup> val = Pool.Get<PooledList<SeagullGroup>>();
		try
		{
			if (!TryGetGroupsNearLook(args, 300f, val, out var lookPosition))
			{
				return;
			}
			SeagullGroup seagullGroup = null;
			foreach (SeagullGroup item in (List<SeagullGroup>)(object)val)
			{
				if (!item.IsFlying())
				{
					continue;
				}
				if (!((Object)(object)seagullGroup == (Object)null))
				{
					Vector3 val2 = ((Component)item).transform.position - lookPosition;
					float sqrMagnitude = val2.sqrMagnitude;
					val2 = ((Component)seagullGroup).transform.position - lookPosition;
					if (!(sqrMagnitude < val2.sqrMagnitude))
					{
						continue;
					}
				}
				seagullGroup = item;
			}
			SeagullLandPoint seagullLandPoint = (((Object)(object)seagullGroup != (Object)null) ? SeagullLandPoint.FindNearestFree(seagullGroup, lookPosition) : null);
			args.ReplyWith(((Object)(object)seagullGroup != (Object)null && seagullGroup.ForceLand(seagullLandPoint)) ? $"Landing {seagullGroup} at {((Component)seagullLandPoint).transform.position}." : "No flying seagull group or free land point found.");
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private static bool TryGetGroupsNearLook(ConsoleSystem.Arg args, float defaultRadius, PooledList<SeagullGroup> groups, out Vector3 lookPosition)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		if (!BirdFlock.TryGetLookPosition(args, out lookPosition))
		{
			return false;
		}
		Query.Server.GetInSphere(lookPosition, args.GetFloat(0, defaultRadius), (List<SeagullGroup>)(object)groups);
		return true;
	}

	private static string BirdColliderName(int bird)
	{
		return $"seagull_bird_{bird:00}";
	}

	private static uint[] CreateBirdColliderIds()
	{
		uint[] array = new uint[16];
		for (int i = 0; i < 16; i++)
		{
			array[i] = StringPool.Get(BirdColliderName(i));
		}
		return array;
	}

	protected override bool WriteSyncVar(byte id, NetWrite writer)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		switch (id)
		{
		case 0:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: AliveBirds for " + ((object)iD/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_AliveBirds);
			return true;
		case 1:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: StartlePosition for " + ((object)iD/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite<Vector3>(writer, __sync_StartlePosition);
			return true;
		case 2:
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: FlightSeed for " + ((object)iD/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_FlightSeed);
			return true;
		default:
			return base.WriteSyncVar(id, writer);
		}
	}

	protected override bool OnSyncVar(byte id, NetRead reader, bool fromAutoSave = false)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		switch (id)
		{
		case 0:
			try
			{
				_ = __sync_AliveBirds;
				ushort _sync_AliveBirds = reader.UInt16();
				__sync_AliveBirds = _sync_AliveBirds;
			}
			catch (Exception ex2)
			{
				Debug.LogException(ex2);
			}
			return true;
		case 1:
			try
			{
				_ = __sync_StartlePosition;
				Vector3 _sync_StartlePosition = reader.Vector3();
				__sync_StartlePosition = _sync_StartlePosition;
			}
			catch (Exception ex3)
			{
				Debug.LogException(ex3);
			}
			return true;
		case 2:
			try
			{
				_ = __sync_FlightSeed;
				uint _sync_FlightSeed = reader.UInt32();
				__sync_FlightSeed = _sync_FlightSeed;
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
			return true;
		default:
			return base.OnSyncVar(id, reader, fromAutoSave);
		}
	}

	private byte __GetWeaverID(string propertyName)
	{
		return propertyName switch
		{
			"AliveBirds" => (byte)0, 
			"StartlePosition" => (byte)1, 
			"FlightSeed" => (byte)2, 
			_ => byte.MaxValue, 
		};
	}

	protected override void WriteAutoSaveSyncVars(NetWrite writer)
	{
		base.WriteAutoSaveSyncVars(writer);
		WriteSyncVar(0, writer);
		WriteSyncVar(1, writer);
		WriteSyncVar(2, writer);
	}

	protected override void ReadAutoSaveSyncVars(NetRead reader)
	{
		base.ReadAutoSaveSyncVars(reader);
		OnSyncVar(0, reader, fromAutoSave: true);
		OnSyncVar(1, reader, fromAutoSave: true);
		OnSyncVar(2, reader, fromAutoSave: true);
	}

	protected override bool AutoSaveSyncVars(SaveInfo save)
	{
		NetWrite netWrite = Net.sv.StartWrite();
		WriteAutoSaveSyncVars(netWrite);
		(byte[] Buffer, int Length) buffer = netWrite.GetBuffer();
		byte[] item = buffer.Buffer;
		int item2 = buffer.Length;
		byte[] array = _autosaveBuffer;
		if (array == null || array.Length < item2)
		{
			byte[] array2 = BaseEntity._autosaveBufferPool.Rent(item2);
			while (array == null || array.Length < item2)
			{
				byte[] array3 = Interlocked.CompareExchange(ref _autosaveBuffer, array2, array);
				if (array3 == array)
				{
					if (array3 != null)
					{
						BaseEntity._autosaveBufferPool.Return(array3);
					}
					array = array2;
					break;
				}
				array = array3;
			}
			if (array != array2)
			{
				BaseEntity._autosaveBufferPool.Return(array2);
			}
		}
		Buffer.BlockCopy(item, 0, array, 0, item2);
		save.msg.baseEntity.syncVars = array;
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
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		base.ResetSyncVars();
		__sync_AliveBirds = 0;
		__sync_StartlePosition = default;
		__sync_FlightSeed = 0u;
	}

	protected override bool ShouldInvalidateCache(byte id)
	{
		return id switch
		{
			0 => true, 
			1 => true, 
			2 => true, 
			_ => base.ShouldInvalidateCache(id), 
		};
	}

	public SeagullGroup()
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
	}
}
