using UnityEngine;

[CreateAssetMenu(menuName = "Rust/Missions/Move Mission")]
public class MoveMission : BaseMission
{
	public float minDistForMovePoint = 20f;

	public float maxDistForMovePoint = 25f;

	private float minDistFromLocation = 3f;

	public override void MissionStart(MissionInstance instance, BasePlayer assignee)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 onUnitSphere = Random.onUnitSphere;
		onUnitSphere.y = 0f;
		((Vector3)(ref onUnitSphere)).Normalize();
		Vector3 val = ((Component)assignee).transform.position + onUnitSphere * Random.Range(minDistForMovePoint, maxDistForMovePoint);
		val.y = WaterLevel.GetWaterOrTerrainSurface(val, waves: false, volumes: false);
		instance.objectiveStatuses[0].worldLocation = val;
		base.MissionStart(instance, assignee);
	}

	public override void ServerThink(MissionInstance instance, BasePlayer assignee, float delta)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		float num = Vector3.Distance(instance.objectiveStatuses[0].worldLocation, ((Component)assignee).transform.position);
		if (instance.status == MissionStatus.Active && num <= minDistFromLocation)
		{
			MissionSuccess(instance, assignee);
			BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(instance.providerID);
			if (Object.op_Implicit((Object)(object)baseNetworkable))
			{
				instance.objectiveStatuses[0].worldLocation = ((Component)baseNetworkable).transform.position;
			}
		}
		else
		{
			if (instance.status == MissionStatus.Accomplished)
			{
				_ = minDistFromLocation;
			}
			base.ServerThink(instance, assignee, delta);
		}
	}
}
