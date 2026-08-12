using UnityEngine;

public class ClothWindModify : FacepunchBehaviour
{
	public Cloth cloth;

	private Vector3 initialClothForce;

	public Vector3 worldWindScale;

	public Vector3 turbulenceScale;

	public ClothWindModify()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		worldWindScale = Vector3.one;
		turbulenceScale = Vector3.one;
		base._002Ector();
	}
}
