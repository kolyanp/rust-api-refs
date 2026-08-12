using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class RuntimePathNode : IAIPathNode
{
	[CompilerGenerated]
	private Vector3 _003CPosition_003Ek__BackingField;

	private HashSet<IAIPathNode> linked;

	public Vector3 Position
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003CPosition_003Ek__BackingField;
		}
		[CompilerGenerated]
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003CPosition_003Ek__BackingField = value;
		}
	}

	public bool Straightaway { get; set; }

	public IEnumerable<IAIPathNode> Linked => linked;

	public RuntimePathNode(Vector3 position)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		linked = new HashSet<IAIPathNode>();
		base._002Ector();
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
