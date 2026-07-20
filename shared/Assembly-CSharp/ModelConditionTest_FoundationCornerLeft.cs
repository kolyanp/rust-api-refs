using UnityEngine;

public class ModelConditionTest_FoundationCornerLeft : ModelConditionTest
{
	private const string square_south = "foundation/sockets/corner/1-l";

	private const string square_north = "foundation/sockets/corner/3-l";

	private const string square_west = "foundation/sockets/corner/2-l";

	private const string square_east = "foundation/sockets/corner/4-l";

	private const string triangle_south = "foundation.triangle/sockets/corner/1-l";

	private const string triangle_northwest = "foundation.triangle/sockets/corner/2-l";

	private const string triangle_northeast = "foundation.triangle/sockets/corner/3-l";

	private string socket = string.Empty;

	protected void OnDrawGizmosSelected()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.matrix = ((Component)this).transform.localToWorldMatrix;
		Gizmos.color = Color.gray;
		Gizmos.DrawWireCube(new Vector3(1.5f, 1.5f, 0f), new Vector3(3f, 3f, 3f));
	}

	protected override void AttributeSetup(GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = worldRotation * Vector3.right;
		if (name.Contains("foundation.triangle"))
		{
			if (val.z < -0.9f)
			{
				socket = "foundation.triangle/sockets/corner/1-l";
			}
			if (val.x < -0.1f)
			{
				socket = "foundation.triangle/sockets/corner/2-l";
			}
			if (val.x > 0.1f)
			{
				socket = "foundation.triangle/sockets/corner/3-l";
			}
			return;
		}
		if (val.z < -0.9f)
		{
			socket = "foundation/sockets/corner/1-l";
		}
		if (val.z > 0.9f)
		{
			socket = "foundation/sockets/corner/3-l";
		}
		if (val.x < -0.9f)
		{
			socket = "foundation/sockets/corner/2-l";
		}
		if (val.x > 0.9f)
		{
			socket = "foundation/sockets/corner/4-l";
		}
	}

	public override bool DoTest(BaseEntity ent)
	{
		EntityLink entityLink = ent.FindLink(socket);
		if (entityLink == null)
		{
			return false;
		}
		for (int i = 0; i < entityLink.connections.Count; i++)
		{
			BuildingBlock buildingBlock = entityLink.connections[i].owner as BuildingBlock;
			if (!((Object)(object)buildingBlock == (Object)null) && (!(buildingBlock.blockDefinition.info.name.token != "foundation") || !(buildingBlock.blockDefinition.info.name.token != "foundation_triangle")))
			{
				return false;
			}
		}
		return true;
	}
}
