using UnityEngine;

public class DiveSite : JunkPile
{
	public Transform bobber;

	public override bool DespawnIfAnyLootTaken => false;

	public override float TimeoutPlayerCheckRadius()
	{
		return 80f;
	}

	public override void Spawn()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		base.Spawn();
		if (Physics.CheckSphere(Vector3Ex.WithY(((Component)this).transform.position, 0f), 5f, 134217728))
		{
			Kill();
		}
		else if ((Object)(object)BoatBuildingStation.GetStationIntersectingOBB(new OBB(Vector3Ex.WithY(((Component)this).transform.position, 0f), ((Component)this).transform.lossyScale, ((Component)this).transform.rotation, bounds), isServer: true) != (Object)null)
		{
			Kill();
		}
	}
}
