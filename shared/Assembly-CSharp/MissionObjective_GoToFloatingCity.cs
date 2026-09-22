using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class MissionObjective_GoToFloatingCity : MissionObjective
{
	[Tooltip("Distance threshold to player for objective to complete (if distance is less that this value).")]
	[InspectorName("Distance For Completion (m)")]
	public float distanceForCompletion = 50f;

	[Tooltip("Distance threshold to player for objective to reset (if distance is greater than this value).")]
	[InspectorName("Distance For Reset (m)")]
	public float distanceForReset = 50f;

	[Tooltip("If true, disregards distance on the y-plane.")]
	public bool use2DDistance = true;

	private float sqrDistanceForCompletion;

	private float sqrDistanceForReset;

	private void OnEnable()
	{
		CacheSqrDistanceForCompletion();
	}

	private void CacheSqrDistanceForCompletion()
	{
		sqrDistanceForCompletion = distanceForCompletion * distanceForCompletion;
		sqrDistanceForReset = distanceForReset * distanceForReset;
	}

	public override bool IsObjectiveValid(int index, BaseMission.MissionInstance instance)
	{
		if ((Object)(object)PointEntity<DeepSeaManager>.ServerInstance == (Object)null)
		{
			return false;
		}
		if (!PointEntity<DeepSeaManager>.ServerInstance.IsOpen())
		{
			return false;
		}
		for (int i = 0; i < DeepSeaManager.ServerFloatingCities.Count; i++)
		{
			if ((Object)(object)DeepSeaManager.ServerFloatingCities[i] != (Object)null)
			{
				return true;
			}
		}
		return false;
	}

	public override void ServerObjectiveStarted(BasePlayer playerFor, int index, BaseMission.MissionInstance instance)
	{
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		base.ServerObjectiveStarted(playerFor, index, instance);
		DeepSeaFloatingCity deepSeaFloatingCity = null;
		int count = DeepSeaManager.ServerFloatingCities.Count;
		PooledList<int> val = Pool.Get<PooledList<int>>();
		try
		{
			((List<int>)(object)val).Capacity = count;
			for (int i = 0; i < count; i++)
			{
				((List<int>)(object)val).Add(i);
			}
			for (int j = 0; j < count; j++)
			{
				int num = Random.Range(j, count);
				int index2 = j;
				PooledList<int> val2 = val;
				int index3 = num;
				int num2 = ((List<int>)(object)val)[num];
				int num3 = ((List<int>)(object)val)[j];
				int num4 = (((List<int>)(object)val)[index2] = num2);
				num4 = (((List<int>)(object)val2)[index3] = num3);
				int num7 = ((List<int>)(object)val)[num];
				DeepSeaFloatingCity deepSeaFloatingCity2 = DeepSeaManager.ServerFloatingCities[num7];
				if ((Object)(object)deepSeaFloatingCity2 != (Object)null)
				{
					deepSeaFloatingCity = deepSeaFloatingCity2;
					break;
				}
			}
			if ((Object)(object)deepSeaFloatingCity == (Object)null)
			{
				Debug.LogError((object)("Mission " + ((Object)instance.GetMission()).name + " failed to find a floating city"), (Object)(object)instance.GetMission());
				return;
			}
			SetObjectiveWorldLocation(index, instance, ((Component)deepSeaFloatingCity).transform.position);
			playerFor.MissionsDirty();
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	protected override void DoServerThink(int index, BaseMission.MissionInstance instance, BasePlayer assignee, float delta)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if (!CanProgress(index, instance) || (IsCompleted(index, instance) && instance.objectiveStatuses[index].blockReset))
		{
			return;
		}
		Vector3 objectiveWorldLocation = GetObjectiveWorldLocation(index, instance);
		float num = (use2DDistance ? Vector3Ex.SqrMagnitude2D(objectiveWorldLocation - ((Component)assignee).transform.position) : Vector3.SqrMagnitude(objectiveWorldLocation - ((Component)assignee).transform.position));
		bool completed = instance.objectiveStatuses[index].completed;
		bool flag = num <= sqrDistanceForCompletion;
		if (completed != flag)
		{
			if (flag)
			{
				CompleteObjective(index, instance, assignee);
			}
			else if (num >= sqrDistanceForReset)
			{
				ResetObjective(index, instance, assignee);
			}
		}
	}
}
