using System.Collections.Generic;
using UnityEngine;

public class CaveNetworkGroupLayerOverride : MonoBehaviour, IServerComponent
{
	private struct OverrideData
	{
		public Vector3 Position;

		public float RadiusSquared;
	}

	public float Radius = 10f;

	private static readonly HashSet<CaveNetworkGroupLayerOverride> _all = new HashSet<CaveNetworkGroupLayerOverride>();

	private static bool _isDirty;

	private static readonly List<OverrideData> _overrides = new List<OverrideData>();

	protected void OnEnable()
	{
		_all.Add(this);
		_isDirty = true;
	}

	protected void OnDisable()
	{
		_all.Remove(this);
		_isDirty = true;
	}

	public static bool Includes(Vector3 pos)
	{
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		if (_isDirty)
		{
			_overrides.Clear();
			foreach (CaveNetworkGroupLayerOverride item in _all)
			{
				_overrides.Add(new OverrideData
				{
					Position = ((Component)item).transform.position,
					RadiusSquared = item.Radius * item.Radius
				});
			}
		}
		foreach (OverrideData @override in _overrides)
		{
			Vector3 val = @override.Position - pos;
			if (((Vector3)(ref val)).sqrMagnitude <= @override.RadiusSquared)
			{
				return true;
			}
		}
		return false;
	}

	protected void OnDrawGizmosSelected()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(((Component)this).transform.position, Radius);
	}
}
