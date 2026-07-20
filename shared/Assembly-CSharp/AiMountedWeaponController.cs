using ConVar;
using Rust.Safety;
using UnityEngine;

public class AiMountedWeaponController : FacepunchBehaviour
{
	[SerializeField]
	private float _maxAttackDistance = 200f;

	[SerializeField]
	private float _accuracy = 1f;

	[SerializeField]
	private MountedWeapon _mountedWeapon;

	[SerializeField]
	private bool _invertedForward;

	[SerializeField]
	private bool _flipPitch;

	[SerializeField]
	private Vector3 _offset;

	[ServerVar]
	public static float time_between_bursts = 3f;

	[ServerVar]
	public static float fire_rate = 0.125f;

	[ServerVar]
	public static float burst_length = 3f;

	private MountedWeaponSeat _cachedSeat;

	private BasePlayer _extraTarget;

	private float lastBurstTime = float.NegativeInfinity;

	private float lastFireTime = float.NegativeInfinity;

	public void SetExtraTarget(BasePlayer target)
	{
		_extraTarget = target;
	}

	private void Start()
	{
		InvokeRepeating(UpdateLoop, 0f, 0.05f);
	}

	private void UpdateLoop()
	{
		using (TimeWarning.New("AiMountedWeaponController.UpdateLoop"))
		{
			if (!AI.move || (Object)(object)_mountedWeapon == (Object)null)
			{
				return;
			}
			if ((Object)(object)_cachedSeat == (Object)null)
			{
				_cachedSeat = _mountedWeapon.GetSeat();
			}
			if (!((Object)(object)_cachedSeat == (Object)null) && _cachedSeat.GetMounted() is HumanNPC)
			{
				HumanNPC humanNPC = _cachedSeat.GetMounted() as HumanNPC;
				if (!((Object)(object)humanNPC == (Object)null) && !humanNPC.InSafeZone())
				{
					UpdateTurret(humanNPC);
				}
			}
		}
	}

	private void UpdateTurret(HumanNPC npc)
	{
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)npc.Brain == (Object)null || (Object)(object)npc.Brain.Navigator == (Object)null)
		{
			return;
		}
		BaseEntity baseEntity = npc.Brain.Navigator.FacingDirectionEntity;
		bool flag = false;
		if ((Object)(object)baseEntity == (Object)null)
		{
			flag = true;
		}
		if (!(baseEntity is BasePlayer))
		{
			flag = true;
		}
		if (!(baseEntity as BasePlayer).IsValidAttackTarget())
		{
			flag = true;
		}
		if (flag)
		{
			if (!((Object)(object)_extraTarget != (Object)null) || !_extraTarget.IsValidAttackTarget())
			{
				return;
			}
			baseEntity = _extraTarget;
			if ((Object)(object)_extraTarget == (Object)null)
			{
				return;
			}
		}
		if ((Object)(object)baseEntity == (Object)null)
		{
			return;
		}
		Vector3 position = _mountedWeapon.PitchPivot.position;
		Vector3 val = ((Component)baseEntity).transform.position + _offset - position;
		if (!(Mathf.Abs(((Vector3)(ref val)).magnitude) < 0.1f) && !(Mathf.Abs(((Vector3)(ref val)).sqrMagnitude) > _maxAttackDistance * _maxAttackDistance))
		{
			if (!_mountedWeapon.IsReloading)
			{
				Vector3 modifiedAimConeDirection = AimConeUtil.GetModifiedAimConeDirection(_accuracy, ((Vector3)(ref val)).normalized);
				_mountedWeapon.AimAt(_mountedWeapon.PitchPivot.position, modifiedAimConeDirection, _flipPitch);
			}
			if (Time.time - lastBurstTime > burst_length + time_between_bursts && GamePhysics.LineOfSight(_mountedWeapon.PitchPivot.position, ((Component)baseEntity).transform.position, 1218519297, _mountedWeapon))
			{
				lastBurstTime = Time.time;
			}
			if (Time.time < lastBurstTime + burst_length && Time.time - lastFireTime >= fire_rate && Vector3.Dot(_invertedForward ? (-_mountedWeapon.PitchPivot.forward) : _mountedWeapon.PitchPivot.forward, ((Vector3)(ref val)).normalized) >= 0.9f)
			{
				lastFireTime = Time.time;
				_mountedWeapon.Fire(isAi: true);
			}
			_mountedWeapon.CheckAiReload();
		}
	}
}
