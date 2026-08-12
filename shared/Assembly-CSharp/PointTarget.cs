using UnityEngine;

public sealed class PointTarget : IAITarget
{
	private readonly float _radius;

	public Vector3? Position { get; }

	public PointTarget(Vector3 pos, float radius = 3f)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		base._002Ector();
		Position = pos;
		_radius = Mathf.Max(0.1f, radius);
	}

	public bool IsValid(BoatAI boat)
	{
		return Position.HasValue;
	}

	public bool IsReached(BoatAI boat)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (Position.HasValue)
		{
			return Vector3.Distance(((Component)boat).transform.position, Position.Value) <= _radius;
		}
		return false;
	}
}
