using UnityEngine;

public sealed class TransformTarget : IAITarget
{
	private readonly Transform _t;

	private readonly float _radius;

	public Transform Transform => _t;

	public Vector3? Position
	{
		get
		{
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			if (!Object.op_Implicit((Object)(object)_t))
			{
				return null;
			}
			return _t.position;
		}
	}

	public TransformTarget(Transform t, float radius = 0.5f)
	{
		_t = t;
		_radius = Mathf.Max(0.1f, radius);
	}

	public bool IsValid(BoatAI self)
	{
		if ((Object)(object)_t != (Object)null)
		{
			return Position.HasValue;
		}
		return false;
	}

	public bool IsReached(BoatAI self)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)_t != (Object)null)
		{
			return Vector3Ex.Distance2D(((Component)self).transform.position, _t.position) <= _radius;
		}
		return false;
	}
}
