using System;
using UnityEngine;

public class DiveSiteBuoy : BaseEntity
{
	public float UpdateCullRange = 128f;

	public Vector3 RotationRate;

	public float InitialSpawnRange = 32f;

	private Action _updateAction;

	private Action updateAction => UpdateMovement;

	public override void ServerInit()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		if (!BaseNetworkable.HasCloseConnections(((Component)this).transform.position, InitialSpawnRange))
		{
			UpdateMovement();
		}
		InvokeRandomized(CheckForNearbyPlayers, 0f, 10f, 5f);
	}

	private void CheckForNearbyPlayers()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		bool flag = BaseNetworkable.HasCloseConnections(((Component)this).transform.position, UpdateCullRange);
		if (flag && !IsInvoking(updateAction))
		{
			InvokeRepeating(updateAction, 0f, 0f);
		}
		else if (!flag && IsInvoking(updateAction))
		{
			CancelInvoke(updateAction);
		}
		ToggleNetworkPositionTick(flag);
	}

	private void UpdateMovement()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)this).transform.position;
		Quaternion rotation = ((Component)this).transform.rotation;
		position.y = WaterLevel.GetWaterSurface(position, waves: true, volumes: false) + Mathf.Sin(Time.time * 3f) * 0.075f;
		rotation *= Quaternion.Euler(RotationRate * Time.deltaTime);
		((Component)this).transform.SetPositionAndRotation(position, rotation);
	}
}
