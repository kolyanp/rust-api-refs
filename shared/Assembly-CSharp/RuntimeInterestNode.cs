using System.Runtime.CompilerServices;
using UnityEngine;

public class RuntimeInterestNode : IAIPathInterestNode
{
	[CompilerGenerated]
	private Vector3 _003CPosition_003Ek__BackingField;

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

	public float NextVisitTime { get; set; }

	public RuntimeInterestNode(Vector3 position)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		base._002Ector();
		Position = position;
	}
}
