using UnityEngine;

public class DropBox : Mailbox
{
	public Transform EyePoint;

	public override bool PlayerIsOwner(BasePlayer player)
	{
		return PlayerBehind(player);
	}

	public bool PlayerBehind(BasePlayer player)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		Vector3 forward = ((Component)this).transform.forward;
		Vector3 val = ((Component)player).transform.position - ((Component)this).transform.position;
		float num = Vector3.Dot(forward, ((Vector3)(ref val)).normalized);
		bool flag = GamePhysics.LineOfSight(player.eyes.position, EyePoint.position, 2162688);
		return num <= 0f && flag;
	}

	public bool PlayerInfront(BasePlayer player)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		Vector3 forward = ((Component)this).transform.forward;
		Vector3 val = ((Component)player).transform.position - ((Component)this).transform.position;
		return Vector3.Dot(forward, ((Vector3)(ref val)).normalized) >= 0.7f;
	}

	public override bool SupportsChildDeployables()
	{
		return true;
	}
}
