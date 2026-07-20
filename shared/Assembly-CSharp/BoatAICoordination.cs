using System;
using System.Collections.Generic;
using Facepunch;
using UnityEngine;

public static class BoatAICoordination
{
	private static readonly HashSet<BoatAI> ActiveBoats = new HashSet<BoatAI>();

	private static readonly Dictionary<BaseEntity, BoatAI> TargetClaims = new Dictionary<BaseEntity, BoatAI>();

	private static readonly Dictionary<int, ListHashSet<BoatAI>> Groups = new Dictionary<int, ListHashSet<BoatAI>>();

	private static int _nextGroupId = 1;

	public static int GetNextGroupId()
	{
		return _nextGroupId++;
	}

	public static void Register(BoatAI boat)
	{
		ActiveBoats.Add(boat);
	}

	public static void Unregister(BoatAI boat)
	{
		ActiveBoats.Remove(boat);
		PooledList<BaseEntity> val = Pool.Get<PooledList<BaseEntity>>();
		try
		{
			foreach (KeyValuePair<BaseEntity, BoatAI> targetClaim in TargetClaims)
			{
				if ((Object)(object)targetClaim.Value == (Object)(object)boat)
				{
					((List<BaseEntity>)(object)val).Add(targetClaim.Key);
				}
			}
			foreach (BaseEntity item in (List<BaseEntity>)(object)val)
			{
				TargetClaims.Remove(item);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static bool TryClaimTarget(BoatAI boat, BasePlayer ply)
	{
		if (BoatAI.PRINT_DEBUGS)
		{
			Debug.Log((object)("BoatAI " + ((Object)boat).name + " is trying to claim target " + ply.displayName));
		}
		BaseEntity claimEntity = GetClaimEntity(ply);
		if ((Object)(object)claimEntity == (Object)null)
		{
			return false;
		}
		if (!TargetClaims.ContainsKey(claimEntity))
		{
			if (BoatAI.PRINT_DEBUGS)
			{
				Debug.Log((object)(((Object)boat).name + " claimed entity " + ((Object)claimEntity).name));
			}
			TargetClaims[claimEntity] = boat;
			return true;
		}
		return false;
	}

	public static void ReleaseClaim(BoatAI boat, BasePlayer ply)
	{
		BaseEntity claimEntity = GetClaimEntity(ply);
		if (!((Object)(object)claimEntity == (Object)null) && TargetClaims.TryGetValue(claimEntity, out var value) && !((Object)(object)value != (Object)(object)boat))
		{
			TargetClaims.Remove(claimEntity);
			if (BoatAI.PRINT_DEBUGS)
			{
				Debug.Log((object)(((Object)boat).name + " released entity " + ((Object)claimEntity).name));
			}
			BoatAI boatAI = FindFreeBoatWithSameTarget(boat, ply);
			if (Object.op_Implicit((Object)(object)boatAI))
			{
				boatAI.OnTargetClaimAvailable(ply);
			}
		}
	}

	private static BaseEntity GetClaimEntity(BasePlayer ply)
	{
		if ((Object)(object)ply == (Object)null)
		{
			return null;
		}
		if (ply.isMounted)
		{
			BaseMountable mounted = ply.GetMounted();
			if ((Object)(object)mounted != (Object)null)
			{
				return mounted;
			}
		}
		return ply;
	}

	public static bool IsTargetClaimed(BasePlayer ply)
	{
		BaseEntity claimEntity = GetClaimEntity(ply);
		if ((Object)(object)claimEntity == (Object)null)
		{
			return false;
		}
		return TargetClaims.ContainsKey(claimEntity);
	}

	public static bool IsTargetClaimedByAnotherGroup(BoatAI currentBoat, BasePlayer ply)
	{
		BaseEntity claimEntity = GetClaimEntity(ply);
		if ((Object)(object)claimEntity == (Object)null)
		{
			return false;
		}
		if (!TargetClaims.TryGetValue(claimEntity, out var value))
		{
			return false;
		}
		if (currentBoat.GroupId != value.GroupId)
		{
			return true;
		}
		return false;
	}

	private static BoatAI FindFreeBoatWithSameTarget(BoatAI currentBoat, BasePlayer ply)
	{
		foreach (BoatAI activeBoat in ActiveBoats)
		{
			if (!((Object)(object)currentBoat == (Object)(object)activeBoat) && (!TargetClaims.TryGetValue(GetClaimEntity(ply), out var value) || !((Object)(object)value == (Object)(object)activeBoat)) && activeBoat.ActiveTarget is PlayerTarget playerTarget && playerTarget.Player.userID.Get() == ply.userID.Get())
			{
				return activeBoat;
			}
		}
		return null;
	}

	public static void AddToGroup(BoatAI boat, int groupId)
	{
		if (!Groups.TryGetValue(groupId, out var value))
		{
			value = new ListHashSet<BoatAI>();
			Groups[groupId] = value;
		}
		value.Add(boat);
		boat.OnGroupChanged(groupId);
	}

	public static void RemoveFromGroup(BoatAI boat, int groupId)
	{
		if (Groups.TryGetValue(groupId, out var value) && value != null && value.Contains(boat))
		{
			value.Remove(boat);
			boat.OnGroupChanged(-1);
		}
	}

	public static ListHashSet<BoatAI> GetGroupMembers(int groupId)
	{
		if (Groups.TryGetValue(groupId, out var value))
		{
			return value;
		}
		return null;
	}

	public static void WipeCoordination()
	{
		ActiveBoats.Clear();
		TargetClaims.Clear();
		Groups.Clear();
		_nextGroupId = 1;
	}
}
