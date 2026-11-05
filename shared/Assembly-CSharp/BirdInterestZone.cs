using System;
using UnityEngine;

public class BirdInterestZone : MonoBehaviour, IServerComponent
{
	public BirdSpecies species = BirdSpecies.Seagull;

	[Tooltip("Flocks prefer higher priority points, then nearer ones.")]
	public float priority;

	[Tooltip("How close an awake player has to be for flocks to come, in metres.")]
	public float playerRadius = 100f;

	[Tooltip("Radius of the circle flocks fly above this, in metres. Below about 15 the flock can't turn tightly enough to hold it.")]
	public float circleRadius = 35f;

	[Tooltip("How high above this flocks circle, in metres.")]
	public float circleHeight = 30f;

	[Tooltip("How long this stays interesting once a player turns up, in seconds, before going on cooldown. Stops busy monuments holding flocks forever.")]
	public float activeDuration = 600f;

	[Tooltip("How long this ignores players after being active, in seconds.")]
	public float cooldown = 900f;

	[NonSerialized]
	public int pointId;

	[NonSerialized]
	public float activeEnd;

	[NonSerialized]
	public float cooldownEnd;

	private Func<BasePlayer, bool> awakePlayerFilter;

	private void OnEnable()
	{
		BirdInterestPointManager.AddZone(this);
	}

	private void OnDisable()
	{
		BirdInterestPointManager.RemoveZone(this);
	}

	public bool HasAwakePlayerNear()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		if (awakePlayerFilter == null)
		{
			awakePlayerFilter = IsAwakePlayerNear;
		}
		bool gridNodesEmpty;
		return BaseEntity.Query.Server.AnyPlayersInSphereFast(((Component)this).transform.position, playerRadius, out gridNodesEmpty, (BasePlayer player) => player.IsNpc, awakePlayerFilter);
	}

	private bool IsAwakePlayerNear(BasePlayer player)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		if (!player.IsSleeping())
		{
			Vector3 val = ((Component)player).transform.position - ((Component)this).transform.position;
			return val.sqrMagnitude <= playerRadius * playerRadius;
		}
		return false;
	}
}
