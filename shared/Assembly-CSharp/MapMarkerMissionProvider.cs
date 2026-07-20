using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Facepunch;
using ProtoBuf;
using UnityEngine;

public class MapMarkerMissionProvider : MapMarker
{
	private NetworkableId missionProviderNetId;

	private BufferList<BaseMission> missions = new BufferList<BaseMission>();

	private string nameToken;

	public void AssignMissions(NetworkableId providerNetId, BufferList<BaseMission> missionsToAssign, string nameToken)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		missionProviderNetId = providerNetId;
		missions.Clear();
		for (int i = 0; i < missionsToAssign.Count; i++)
		{
			missions.Add(missionsToAssign[i]);
		}
		this.nameToken = nameToken;
	}

	public override void Save(SaveInfo info)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		info.msg.missionMapMarker = Pool.Get<MissionMapMarker>();
		info.msg.missionMapMarker.missionIds = Pool.Get<List<uint>>();
		Enumerator<BaseMission> enumerator = missions.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BaseMission current = enumerator.Current;
				info.msg.missionMapMarker.missionIds.Add(current.id);
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		info.msg.missionMapMarker.missionProviderNetId = missionProviderNetId;
		info.msg.missionMapMarker.nameToken = nameToken;
	}

	public override void Load(LoadInfo info)
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg.missionMapMarker == null)
		{
			return;
		}
		missions.Clear();
		if (info.msg.missionMapMarker.missionIds != null)
		{
			for (int i = 0; i < info.msg.missionMapMarker.missionIds.Count; i++)
			{
				if (MissionManifest.TryGetFromID(info.msg.missionMapMarker.missionIds[i], out var mission))
				{
					missions.Add(mission);
				}
			}
		}
		missionProviderNetId = info.msg.missionMapMarker.missionProviderNetId;
		if (base.isServer && (!BaseNetworkable.serverEntities.TryGetEntity(missionProviderNetId, out var entity) || !(entity is IMissionProvider)))
		{
			Debug.LogError((object)("Failed to find a mission provider entity from net ID (" + ((object)Unsafe.As<NetworkableId, NetworkableId>(ref missionProviderNetId)/*cast due to constrained. prefix*/).ToString() + ")"));
		}
		nameToken = info.msg.missionMapMarker.nameToken;
	}
}
