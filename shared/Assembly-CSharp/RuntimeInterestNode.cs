using System.Runtime.CompilerServices;
using UnityEngine;

public class RuntimeInterestNode : IAIPathInterestNode
{
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

	public float NextVisitTime { get; set; }

	public RuntimeInterestNode(Vector3 position)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		Position = position;
	}
}
