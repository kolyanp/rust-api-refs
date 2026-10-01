using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class RuntimePathNode : IAIPathNode
{
	private HashSet<IAIPathNode> linked = new HashSet<IAIPathNode>();

	public Vector3 Position
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	public bool Straightaway { get; set; }

	public IEnumerable<IAIPathNode> Linked => linked;

	public RuntimePathNode(Vector3 position)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		Position = position;
	}

	public bool IsValid()
	{
		return true;
	}

	public void AddLink(IAIPathNode link)
	{
		linked.Add(link);
	}
}
