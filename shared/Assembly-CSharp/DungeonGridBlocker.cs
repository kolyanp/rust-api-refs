using UnityEngine;

public class DungeonGridBlocker : MonoBehaviour
{
	public Bounds bounds = new Bounds(Vector3.zero, Vector3.one);

	protected void Awake()
	{
		if (Object.op_Implicit((Object)(object)TerrainMeta.Path))
		{
			TerrainMeta.Path.DungeonGridBlockers.Add(this);
		}
	}

	public OBB GetBounds()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		return new OBB(((Component)this).transform.position, ((Component)this).transform.lossyScale, ((Component)this).transform.rotation, bounds);
	}

	public DungeonGridBlocker()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
	}
}
