using UnityEngine;

public sealed class PlayerTarget : IAITarget
{
	public bool StayClose;

	private readonly BasePlayer _player;

	private readonly float _acquiredAt;

	private readonly Transform _boat;

	private const float CLOSE_DIST = 15f;

	private const float FAR_DIST = 55f;

	private const float LOS_DROP_TIME = 10f;

	private float _lastLosSeen;

	public BasePlayer Player => _player;

	public Vector3? Position
	{
		get
		{
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_0084: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0077: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_player == (Object)null)
			{
				return null;
			}
			Vector3 val = ((!_player.HasParent()) ? ((Component)_player).transform.position : ((Component)_player.GetParentEntity()).transform.position);
			Vector3 val2 = _boat.position - val;
			Vector3 normalized = ((Vector3)(ref val2)).normalized;
			return (!StayClose) ? (val + normalized * 55f) : (val + normalized * 15f);
		}
	}

	public PlayerTarget(BasePlayer player, float acquiredAt, Transform boat)
	{
		_player = player;
		_acquiredAt = acquiredAt;
		_boat = boat;
		_lastLosSeen = Time.time;
	}

	public bool IsValid(BoatAI self)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		if (!Position.HasValue)
		{
			return false;
		}
		BasePlayer player = _player;
		if (!self.IsPlayerTargetValid(player))
		{
			if (BoatAI.PRINT_DEBUGS)
			{
				Debug.Log((object)"Leaving seek state - not a valid target anymore.");
			}
			return false;
		}
		if (!self.IsPlayerInRange(player, self.SearchRange * 1.5f))
		{
			if (BoatAI.PRINT_DEBUGS)
			{
				Debug.Log((object)"Leaving seek state - not in range anymore.");
			}
			return false;
		}
		if (Mathf.Abs(((Component)player).transform.position.y - ((Component)self).transform.position.y) > 30f)
		{
			return false;
		}
		if (((Component)player).transform.position.y < 12f)
		{
			return false;
		}
		if (Vector3Ex.Distance2D(((Component)player).transform.position, ((Component)self).transform.position) >= self.SearchRange)
		{
			return false;
		}
		if (self.HasLineOfSightToPlayer(_player))
		{
			_lastLosSeen = Time.time;
		}
		else if (Time.time - _lastLosSeen > 10f)
		{
			if (BoatAI.PRINT_DEBUGS)
			{
				Debug.Log((object)$"Leaving seek state - lost line of sight for {10f}s.");
			}
			return false;
		}
		return true;
	}

	public bool IsReached(BoatAI self)
	{
		return false;
	}
}
