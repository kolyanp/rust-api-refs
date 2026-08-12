using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public class MountedWeaponSeat : BaseVehicleSeat
{
	private const float MOUNTED_WEAPON_SEARCH_RANGE = 2f;

	private float _searchCount;

	private GameObject _mountedWeaponGameObject;

	private float _nextSearchTime;

	private MountedWeapon _owner;

	public GameObject MountedWeaponGameObject => _mountedWeaponGameObject;

	public MountedWeapon Owner
	{
		get
		{
			if ((Object)(object)_owner == (Object)null)
			{
				_owner = FindMountedWeapon();
				if ((Object)(object)_owner != (Object)null)
				{
					_owner.AssignSeat(this);
				}
				if ((Object)(object)_owner == (Object)null)
				{
					if (Time.time < _nextSearchTime)
					{
						return null;
					}
					if (_searchCount > 30f)
					{
						return null;
					}
					_nextSearchTime = Time.time + 2f;
					_searchCount++;
				}
			}
			return _owner;
		}
	}

	public override void InitShared()
	{
		base.InitShared();
		Invoke(delegate
		{
			if ((Object)(object)Owner != (Object)null)
			{
				Owner.AssignSeat(this);
				eyePositionOverride = Owner.GetCustomEyes();
				eyeCenterOverride = Owner.GetCustomEyes();
			}
		}, 0.05f);
	}

	public override Transform GetEyeOverride()
	{
		if ((Object)(object)Owner == (Object)null)
		{
			return base.GetEyeOverride();
		}
		return Owner.GetCustomEyes();
	}

	private MountedWeapon FindMountedWeapon()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		PooledList<BaseEntity> val = Pool.Get<PooledList<BaseEntity>>();
		try
		{
			Vis.Entities(((Component)this).transform.position, 2f, (List<BaseEntity>)(object)val, -1, (QueryTriggerInteraction)2);
			BaseEntity baseEntity = GetParentEntity();
			foreach (BaseEntity item in (List<BaseEntity>)(object)val)
			{
				if (!((Object)(object)item == (Object)null) && (!((Object)(object)baseEntity != (Object)null) || baseEntity.isServer == item.isServer) && item is MountedWeapon result)
				{
					_mountedWeaponGameObject = ((Component)item).gameObject;
					return result;
				}
			}
			return null;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public override void PlayerServerInput(InputState inputState, BasePlayer player)
	{
		base.PlayerServerInput(inputState, player);
		if ((Object)(object)Owner != (Object)null)
		{
			Owner.PlayerServerInput(inputState, player);
		}
	}

	public override void LightToggle(BasePlayer player)
	{
		if (!((Object)(object)Owner == (Object)null))
		{
			Owner.LightToggle(player);
		}
	}

	public override void OnPlayerDismounted(BasePlayer player)
	{
		base.OnPlayerDismounted(player);
		if ((Object)(object)Owner != (Object)null)
		{
			Owner.OnPlayerDismounted(player);
		}
	}

	public override void OnPlayerMounted()
	{
		base.OnPlayerMounted();
		if ((Object)(object)Owner != (Object)null)
		{
			Owner.OnPlayerMounted();
		}
	}

	public override bool CanHoldItems()
	{
		return false;
	}

	public override Vector2 GetPitchClamp()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return pitchClamp;
	}

	public override Vector2 GetYawClamp()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return yawClamp;
	}
}
