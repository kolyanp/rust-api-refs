using System;
using UnityEngine;

public class NPCMissionProvider : NPCTalking, IMissionProvider
{
	public GameObjectRef MarkerPrefab;

	public BaseMission[] FallbackMissions = Array.Empty<BaseMission>();

	private BufferList<BaseMission> cachedAllMissions;

	public NetworkableId ProviderID()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return net.ID;
	}

	public Vector3 ProviderPosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.position;
	}

	public BufferList<BaseMission> GetAllMissions()
	{
		if (cachedAllMissions == null)
		{
			cachedAllMissions = new BufferList<BaseMission>();
			ConversationData[] array = conversations;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].FindAllMissionAssignments(cachedAllMissions);
			}
			BaseMission[] fallbackMissions = FallbackMissions;
			foreach (BaseMission baseMission in fallbackMissions)
			{
				cachedAllMissions.Add(baseMission);
			}
		}
		return cachedAllMissions;
	}

	public string GetNameToken()
	{
		return NPCName.token;
	}

	public override void ServerInit()
	{
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		if (MarkerPrefab != null && MarkerPrefab.isValid)
		{
			BufferList<BaseMission> allMissions = GetAllMissions();
			if (allMissions.Count > 0)
			{
				MapMarkerMissionProvider obj = GameManager.server.CreateEntity(MarkerPrefab.resourcePath, ((Component)this).transform.position, ((Component)this).transform.rotation) as MapMarkerMissionProvider;
				obj.AssignMissions(ProviderID(), allMissions, NPCName.token);
				obj.Spawn();
			}
		}
		NPCTalking.serverMissionProviders.TryAdd((IMissionProvider)this);
	}

	public override void Server_OnConversationEnded(BasePlayer player)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		player.ProcessMissionEvent(BaseMission.MissionEventType.CONVERSATION, ProviderID(), 0f);
		base.Server_OnConversationEnded(player);
	}

	public override void Server_OnConversationStarted(BasePlayer speakingTo)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		speakingTo.ProcessMissionEvent(BaseMission.MissionEventType.CONVERSATION, ProviderID(), 1f);
		base.Server_OnConversationStarted(speakingTo);
	}

	public override void OnConversationAction(BasePlayer player, string action)
	{
		if (action.StartsWith("assignmission "))
		{
			int num = action.IndexOf(" ");
			BaseMission fromShortName = MissionManifest.GetFromShortName(action.Substring(num + 1));
			if (fromShortName != null && ((IMissionProvider)this).TryGetMission(fromShortName.id, out BaseMission _))
			{
				BaseMission.AssignMission(player, this, fromShortName);
			}
		}
		base.OnConversationAction(player, action);
	}

	public bool Server_HasMissionAvailable(BasePlayer player)
	{
		BufferList<BaseMission> allMissions = GetAllMissions();
		for (int i = 0; i < allMissions.Count; i++)
		{
			if (player.Server_CanAcceptMission((IMissionProvider)this, allMissions[i]))
			{
				return true;
			}
		}
		return false;
	}

	internal override void DoServerDestroy()
	{
		base.DoServerDestroy();
		NPCTalking.serverMissionProviders.Remove((IMissionProvider)this);
	}

	private void DelayKill()
	{
		Kill();
	}

	public void DelayedKill(float timeInSeconds)
	{
		Invoke(DelayKill, timeInSeconds);
	}
}
