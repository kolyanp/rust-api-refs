using System;
using System.Runtime.CompilerServices;
using ConVar;
using Network;
using UnityEngine;

namespace Rust.Ai.Gen2;

public class Frog : CritterAnimal
{
	[ServerVar(Help = "Population active on the server, per square km", ShowInAdminUI = true)]
	public static float Population = 2f;

	private Vector3 __sync_NavTarget;

	[Sync]
	public Vector3 NavTarget
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return __sync_NavTarget;
		}
		[CompilerGenerated]
		set
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			if (!IsSyncVarEqual<Vector3>(__sync_NavTarget, value))
			{
				__sync_NavTarget = value;
				byte nameID = __GetWeaverID("NavTarget");
				QueueSyncVar(nameID);
			}
		}
	}

	public void SetNavTarget(Vector3 nextPositionWS)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		NavTarget = nextPositionWS;
	}

	protected override bool WriteSyncVar(byte id, NetWrite writer)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (id == 1)
		{
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: NavTarget for " + ((object)iD/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite<Vector3>(writer, __sync_NavTarget);
			return true;
		}
		return base.WriteSyncVar(id, writer);
	}

	protected override bool OnSyncVar(byte id, NetRead reader, bool fromAutoSave = false)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (id == 1)
		{
			try
			{
				_ = __sync_NavTarget;
				Vector3 _sync_NavTarget = reader.Vector3();
				__sync_NavTarget = _sync_NavTarget;
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
		if (propertyName == "NavTarget")
		{
			return 1;
		}
		return byte.MaxValue;
	}

	protected override void WriteAutoSaveSyncVars(NetWrite writer)
	{
		base.WriteAutoSaveSyncVars(writer);
	}

	protected override void ReadAutoSaveSyncVars(NetRead reader)
	{
		base.ReadAutoSaveSyncVars(reader);
	}

	protected override bool AutoSaveSyncVars(SaveInfo save)
	{
		return true;
	}

	protected override bool AutoLoadSyncVars(LoadInfo load)
	{
		return true;
	}

	protected override void ResetSyncVars()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		base.ResetSyncVars();
		__sync_NavTarget = default;
	}

	protected override bool ShouldInvalidateCache(byte id)
	{
		if (id == 1)
		{
			return true;
		}
		return base.ShouldInvalidateCache(id);
	}
}
