using System;
using UnityEngine;

public sealed class PlayerTarget : IAITarget
{
	public bool StayClose;

	public bool KeepOffshore;

	private readonly BasePlayer _player;

	private readonly float _acquiredAt;

	private readonly Transform _boat;

	private const float CLOSE_DIST = 15f;

	private const float FAR_DIST = 55f;

	public const float LOS_DROP_TIME = 45f;

	private const float SEARCH_DELAY = 3f;

	private const float SEARCH_ANGLE = 30f;

	public BasePlayer Player => _player;

	public float LastSeen { get; set; }

	public Nullable Position
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
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			//IL_0085: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			if ((Object)(object)_player == (Object)null)
			{
				return null;
			}
			Vector3 val = ((!_player.HasParent()) ? ((Component)_player).transform.position : ((Component)_player.GetParentEntity()).transform.position);
			Vector3 val2 = _boat.position - val;
			Vector3 val3 = val2.normalized;
			if (Time.time - LastSeen > 3f)
			{
				val3 = Quaternion.AngleAxis(30f, Vector3.up) * val3;
			}
			val = ((!StayClose) ? (val + val3 * 55f) : (val + val3 * 15f));
			if (KeepOffshore)
			{
				val = BoatAI.PushOffshore(val);
			}
			return val;
		}
	}

	public PlayerTarget(BasePlayer player, float acquiredAt, Transform boat)
	{
		_player = player;
		_acquiredAt = acquiredAt;
		_boat = boat;
		LastSeen = Time.time;
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
		if (((Component)player).transform.position.y < -12f)
		{
			return false;
		}
		if (Vector3Ex.Distance2D(((Component)player).transform.position, ((Component)self).transform.position) >= self.SearchRange)
		{
			return false;
		}
		if (self.HasLineOfSightToPlayer(_player))
		{
			LastSeen = Time.time;
		}
		else if (Time.time - LastSeen > 45f)
		{
			if (BoatAI.PRINT_DEBUGS)
			{
				Debug.Log((object)$"Leaving seek state - lost line of sight for {45f}s.");
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
