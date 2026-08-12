using UnityEngine;

public class TerrainTopologySet : TerrainModifier
{
	[InspectorFlags]
	public Enum TopologyType;

	protected override void Apply(Vector3 position, float opacity, float radius, float fade)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Expected I4, but got Unknown
		if (Object.op_Implicit((Object)(object)TerrainMeta.TopologyMap))
		{
			TerrainMeta.TopologyMap.SetTopology(position, (int)TopologyType, radius, fade);
		}
	}

	public TerrainTopologySet()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		TopologyType = (Enum)512;
		base._002Ector();
	}
}
