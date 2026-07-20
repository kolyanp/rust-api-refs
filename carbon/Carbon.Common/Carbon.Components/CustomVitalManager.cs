using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Facepunch;
using ProtoBuf;
using UnityEngine;

namespace Carbon.Components;

public static class CustomVitalManager
{
	public class VitalDictionary<T> where T : IdentifiableVital, new()
	{
		private ListDictionary<uint, T> buffer = (ListDictionary<uint, T>)(object)new ListDictionary<uint, uint>();

		public static uint nextVitalId = 100u;

		public int Count => ((ListDictionary<uint, uint>)(object)buffer).Count;

		public bool HasAny()
		{
			return Count > 0;
		}

		public void GetVitals(List<T> vitals)
		{
			for (int i = 0; i < ((ListDictionary<uint, uint>)(object)buffer).Count; i++)
			{
				vitals.Add(((ListDictionary<uint, uint>)(object)buffer).Values[i]);
			}
		}

		public void AppendVitals(CustomVitals vitals)
		{
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			BufferList<T> values = ((ListDictionary<uint, uint>)(object)buffer).Values;
			for (int i = 0; i < Count; i++)
			{
				T val = values[i];
				val.info.timeLeft = Mathf.Max(val.totalTimeLeft - (int)TimeSince.op_Implicit(val.sinceTimeLeftStarted), 0);
				vitals.vitals.Add(val.info);
			}
		}

		public T AddVital(CustomVitalInfo vital, float expiry = 0f)
		{
			//IL_0069: Expected I4, but got O
			T val = Pool.Get<T>();
			val.id = ++nextVitalId;
			val.info = vital;
			val.expiry = expiry;
			val.SetTimeLeft(vital.timeLeft);
			val.RestartExpiry();
			((ListDictionary<uint, uint>)(object)buffer).Add(val.id, (uint)(int)val);
			return val;
		}

		public bool RemoveVital(T vital)
		{
			if (!((ListDictionary<uint, uint>)(object)buffer).Remove(vital.id))
			{
				return false;
			}
			Pool.Free<T>(ref vital);
			return true;
		}

		public unsafe bool RemoveVital(uint id)
		{
			T val = default(T);
			if (!((ListDictionary<uint, uint>)(object)buffer).TryGetValue(id, ref *(uint*)(&val)))
			{
				return false;
			}
			Pool.Free<T>(ref val);
			return ((ListDictionary<uint, uint>)(object)buffer).Remove(id);
		}

		public bool TryGetVital(uint id, out T vital)
		{
			return ((ListDictionary<uint, uint>)(object)buffer).TryGetValue(id, ref Unsafe.As<T, uint>(ref vital));
		}

		public void ClearVitals()
		{
			for (int i = 0; i < ((ListDictionary<uint, uint>)(object)buffer).Count; i++)
			{
				CustomVitalInfo info = ((ListDictionary<uint, uint>)(object)buffer).Values[i].info;
				Pool.Free<CustomVitalInfo>(ref info);
			}
			((ListDictionary<uint, uint>)(object)buffer).Clear();
		}
	}

	public abstract class IdentifiableVital : IPooled
	{
		public uint id;

		public CustomVitalInfo info;

		public TimeSince sinceTimeLeftStarted;

		public int totalTimeLeft;

		public float expiry;

		protected bool _isPooled;

		protected Action _cachedExpiryAction;

		public bool IsPooled()
		{
			return _isPooled;
		}

		public virtual void SetTimeLeft(int timeLeft)
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			info.timeLeft = timeLeft;
			totalTimeLeft = info.timeLeft;
			sinceTimeLeftStarted = TimeSince.op_Implicit(0f);
		}

		public abstract void SendUpdate();

		public abstract void RemoveSelf();

		public void RestartExpiry()
		{
			if (_cachedExpiryAction == null)
			{
				_cachedExpiryAction = RemoveSelf;
			}
			if (((FacepunchBehaviour)SingletonComponent<ServerMgr>.Instance).IsInvoking(_cachedExpiryAction))
			{
				((FacepunchBehaviour)SingletonComponent<ServerMgr>.Instance).CancelInvoke(_cachedExpiryAction);
			}
			if (expiry > 0f)
			{
				((FacepunchBehaviour)SingletonComponent<ServerMgr>.Instance).Invoke(_cachedExpiryAction, expiry);
			}
		}

		public virtual void EnterPool()
		{
			id = 0u;
			totalTimeLeft = 0;
			expiry = 0f;
			if (info != null)
			{
				Pool.Free<CustomVitalInfo>(ref info);
			}
			_isPooled = true;
		}

		public virtual void LeavePool()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			sinceTimeLeftStarted = TimeSince.op_Implicit(0f);
			_isPooled = false;
		}
	}

	public class SharedIdentifiableVital : IdentifiableVital
	{
		public override void RemoveSelf()
		{
			if (!_isPooled)
			{
				RemoveSharedVital(id);
			}
		}

		public override void SendUpdate()
		{
			if (!_isPooled)
			{
				SendVitalsToEveryone();
			}
		}
	}

	public class PlayerIdentifiableVital : IdentifiableVital
	{
		public ulong playerId;

		private BasePlayer player;

		public BasePlayer GetPlayer()
		{
			if (!BaseNetworkableEx.IsValid((BaseNetworkable)(object)player))
			{
				player = BasePlayer.FindByID(playerId);
			}
			return player;
		}

		public override void RemoveSelf()
		{
			if (!_isPooled)
			{
				BasePlayer val = GetPlayer();
				if (val != null && BaseNetworkableEx.IsValid((BaseNetworkable)(object)val))
				{
					RemoveVital(val, id);
				}
			}
		}

		public override void SendUpdate()
		{
			if (!_isPooled)
			{
				SendVitals(GetPlayer());
			}
		}

		public override void EnterPool()
		{
			base.EnterPool();
			playerId = 0uL;
			player = null;
		}
	}

	private static VitalDictionary<SharedIdentifiableVital> sharedVitals = new VitalDictionary<SharedIdentifiableVital>();

	private static ListDictionary<ulong, VitalDictionary<PlayerIdentifiableVital>> playerVitals = new ListDictionary<ulong, VitalDictionary<PlayerIdentifiableVital>>();

	public static CustomVitalInfo RentVitalInfo(string icon = null, Color iconColor = default(Color), Color backgroundColor = default(Color), string leftText = null, Color leftTextColor = default(Color), string rightText = null, Color rightTextColor = default(Color), int timeLeft = 0, bool active = true)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		CustomVitalInfo val = Pool.Get<CustomVitalInfo>();
		val.icon = icon;
		val.iconColor = iconColor;
		val.backgroundColor = backgroundColor;
		val.leftText = leftText;
		val.leftTextColor = leftTextColor;
		val.rightText = rightText;
		val.rightTextColor = rightTextColor;
		val.active = active;
		val.timeLeft = timeLeft;
		return val;
	}

	public static T AddVital<T>(BasePlayer player, CustomVitalInfo vital, float expiry = 0f, bool sendUpdate = true) where T : PlayerIdentifiableVital
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		VitalDictionary<PlayerIdentifiableVital> vitalDictionary = default(VitalDictionary<PlayerIdentifiableVital>);
		if (!playerVitals.TryGetValue(EncryptedValue<ulong>.op_Implicit(player.userID), ref vitalDictionary))
		{
			playerVitals.Add(EncryptedValue<ulong>.op_Implicit(player.userID), vitalDictionary = new VitalDictionary<PlayerIdentifiableVital>());
		}
		PlayerIdentifiableVital playerIdentifiableVital = vitalDictionary.AddVital(vital, expiry);
		playerIdentifiableVital.playerId = EncryptedValue<ulong>.op_Implicit(player.userID);
		if (sendUpdate)
		{
			SendVitals(player);
		}
		return playerIdentifiableVital as T;
	}

	public static PlayerIdentifiableVital AddVital(BasePlayer player, CustomVitalInfo vital, float expiry = 0f, bool sendUpdate = true)
	{
		return AddVital<PlayerIdentifiableVital>(player, vital, expiry, sendUpdate);
	}

	public static T AddSharedVital<T>(CustomVitalInfo vital, float expiry = 0f, bool sendUpdate = true) where T : SharedIdentifiableVital
	{
		SharedIdentifiableVital sharedIdentifiableVital = sharedVitals.AddVital(vital, expiry);
		if (sendUpdate)
		{
			SendVitalsToEveryone();
		}
		return sharedIdentifiableVital as T;
	}

	public static SharedIdentifiableVital AddSharedVital(CustomVitalInfo vital, float expiry = 0f, bool sendUpdate = true)
	{
		return AddSharedVital<SharedIdentifiableVital>(vital, expiry, sendUpdate);
	}

	public static VitalDictionary<SharedIdentifiableVital> GetSharedVitals()
	{
		return sharedVitals;
	}

	public static VitalDictionary<PlayerIdentifiableVital> GetPlayerVitals(ulong playerId)
	{
		VitalDictionary<PlayerIdentifiableVital> result = default(VitalDictionary<PlayerIdentifiableVital>);
		if (playerVitals.TryGetValue(playerId, ref result))
		{
			return result;
		}
		return null;
	}

	public static VitalDictionary<PlayerIdentifiableVital> GetPlayerVitals(BasePlayer player)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return GetPlayerVitals(EncryptedValue<ulong>.op_Implicit(player.userID));
	}

	public static int GetTotalPlayerVitalCount(ulong playerId)
	{
		return GetSharedVitals().Count + (GetPlayerVitals(playerId)?.Count ?? 0);
	}

	public static bool TryGetVital<T>(uint id, out T vital) where T : PlayerIdentifiableVital
	{
		BufferList<VitalDictionary<PlayerIdentifiableVital>> values = playerVitals.Values;
		for (int i = 0; i < playerVitals.Count; i++)
		{
			if (values[i].TryGetVital(id, out var vital2))
			{
				vital = vital2 as T;
				return true;
			}
		}
		vital = null;
		return false;
	}

	public static bool TryGetVital<T>(ulong playerId, uint id, out T vital) where T : PlayerIdentifiableVital
	{
		VitalDictionary<PlayerIdentifiableVital> vitalDictionary = default(VitalDictionary<PlayerIdentifiableVital>);
		if (!playerVitals.TryGetValue(playerId, ref vitalDictionary))
		{
			vital = null;
			return false;
		}
		if (vitalDictionary.TryGetVital(id, out var vital2))
		{
			vital = vital2 as T;
			return true;
		}
		vital = null;
		return false;
	}

	public static bool TryGetVital(uint id, out PlayerIdentifiableVital vital)
	{
		return CustomVitalManager.TryGetVital<PlayerIdentifiableVital>(id, out vital);
	}

	public static bool TryGetVital(ulong playerId, uint id, out PlayerIdentifiableVital vital)
	{
		return CustomVitalManager.TryGetVital<PlayerIdentifiableVital>(playerId, id, out vital);
	}

	public static bool TryGetSharedVital<T>(uint id, out T vital) where T : SharedIdentifiableVital
	{
		if (sharedVitals.TryGetVital(id, out var vital2))
		{
			vital = vital2 as T;
			return true;
		}
		vital = null;
		return false;
	}

	public static bool TryGetSharedVital(uint id, out SharedIdentifiableVital vital)
	{
		return CustomVitalManager.TryGetSharedVital<SharedIdentifiableVital>(id, out vital);
	}

	public static bool RemoveVital(BasePlayer player, IdentifiableVital vital, bool sendUpdate = true)
	{
		return RemoveVital(player, vital.id, sendUpdate);
	}

	public static bool RemoveVital(BasePlayer player, uint id, bool sendUpdate = true)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		VitalDictionary<PlayerIdentifiableVital> vitalDictionary = default(VitalDictionary<PlayerIdentifiableVital>);
		if (!BaseNetworkableEx.IsValid((BaseNetworkable)(object)player) || !playerVitals.TryGetValue(EncryptedValue<ulong>.op_Implicit(player.userID), ref vitalDictionary))
		{
			return false;
		}
		if (!vitalDictionary.RemoveVital(id))
		{
			return false;
		}
		if (sendUpdate)
		{
			SendVitals(player);
		}
		return true;
	}

	public static bool RemoveSharedVital(IdentifiableVital vital, bool sendUpdate = true)
	{
		return RemoveSharedVital(vital.id, sendUpdate);
	}

	public static bool RemoveSharedVital(uint id, bool sendUpdate = true)
	{
		if (!sharedVitals.RemoveVital(id))
		{
			return false;
		}
		if (sendUpdate)
		{
			SendVitalsToEveryone();
		}
		return true;
	}

	public static void ClearVitals(BasePlayer player, bool sendUpdate = true)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		VitalDictionary<PlayerIdentifiableVital> vitalDictionary = default(VitalDictionary<PlayerIdentifiableVital>);
		if (BaseNetworkableEx.IsValid((BaseNetworkable)(object)player) && playerVitals.TryGetValue(EncryptedValue<ulong>.op_Implicit(player.userID), ref vitalDictionary))
		{
			vitalDictionary.ClearVitals();
			if (sendUpdate)
			{
				SendVitals(player);
			}
		}
	}

	public static void ClearSharedVitals(bool sendUpdate = true)
	{
		sharedVitals.ClearVitals();
		if (sendUpdate)
		{
			SendVitalsToEveryone();
		}
	}

	public static void SendVitals(BasePlayer player)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (BaseNetworkableEx.IsValid((BaseNetworkable)(object)player))
		{
			CustomVitals val = Pool.Get<CustomVitals>();
			val.vitals = Pool.Get<List<CustomVitalInfo>>();
			VitalDictionary<PlayerIdentifiableVital> vitalDictionary = default(VitalDictionary<PlayerIdentifiableVital>);
			if (playerVitals.TryGetValue(EncryptedValue<ulong>.op_Implicit(player.userID), ref vitalDictionary))
			{
				vitalDictionary.AppendVitals(val);
			}
			sharedVitals.AppendVitals(val);
			CommunityEntity.ServerInstance.SendCustomVitals(player, val);
			Pool.Free<CustomVitalInfo>(ref val.vitals, false);
			Pool.Free<CustomVitals>(ref val);
		}
	}

	public static void SendVitalsToEveryone()
	{
		for (int i = 0; i < BasePlayer.activePlayerList.Count; i++)
		{
			SendVitals(BasePlayer.activePlayerList[i]);
		}
	}
}
