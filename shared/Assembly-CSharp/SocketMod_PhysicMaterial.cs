using UnityEngine;

public class SocketMod_PhysicMaterial : SocketMod
{
	public PhysicsMaterial[] ValidMaterials;

	private PhysicsMaterial foundMaterial;

	public override bool DoCheck(ref Construction.Placement place)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = place.position;
		Vector3 eulerAngles = place.rotation.eulerAngles;
		Vector3 val = position + eulerAngles.normalized * 0.5f;
		eulerAngles = place.rotation.eulerAngles;
		RaycastHit val2 = default;
		if (Physics.Raycast(val, -eulerAngles.normalized, ref val2, 1f, 161546240, (QueryTriggerInteraction)1))
		{
			foundMaterial = ColliderEx.GetMaterialAt(val2.collider, val2.point);
			PhysicsMaterial[] validMaterials = ValidMaterials;
			for (int i = 0; i < validMaterials.Length; i++)
			{
				if ((Object)(object)validMaterials[i] == (Object)(object)foundMaterial)
				{
					return true;
				}
			}
		}
		return false;
	}
}
