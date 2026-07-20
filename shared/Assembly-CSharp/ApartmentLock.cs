using Facepunch;
using ProtoBuf;
using UnityEngine;

public class ApartmentLock : BaseLock
{
	public ApartmentRoom Room;

	private bool HasAccess(BasePlayer player)
	{
		if ((Object)(object)Room == (Object)null)
		{
			return false;
		}
		if (!Room.IsAuthed(player.userID))
		{
			return Room.IsBreakInActive();
		}
		return true;
	}

	private bool IsFrontDoorAndPlayerInside(BasePlayer player)
	{
		if ((Object)(object)Room == (Object)null)
		{
			return false;
		}
		if (IsFrontDoor())
		{
			return Room.IsInsideRoom(player);
		}
		return false;
	}

	private bool IsFrontDoor()
	{
		if ((Object)(object)Room == (Object)null || (Object)(object)Room.FrontDoor == (Object)null)
		{
			return false;
		}
		return (Object)(object)GetParentEntity() == (Object)(object)Room.FrontDoor;
	}

	public override bool OnTryToOpen(BasePlayer player)
	{
		if (HasAccess(player))
		{
			return true;
		}
		if (IsFrontDoorAndPlayerInside(player))
		{
			ScheduleAutoClose();
			return true;
		}
		return false;
	}

	public override bool OnTryToClose(BasePlayer player)
	{
		if (HasAccess(player))
		{
			if (IsFrontDoor())
			{
				CancelAutoClose();
			}
			return true;
		}
		return false;
	}

	private void ScheduleAutoClose()
	{
		CancelAutoClose();
		ApartmentDoor apartmentDoor = Room?.FrontDoor;
		if ((Object)(object)apartmentDoor != (Object)null)
		{
			apartmentDoor.Invoke(apartmentDoor.CloseRequest, 5f);
		}
	}

	private void CancelAutoClose()
	{
		ApartmentDoor apartmentDoor = Room?.FrontDoor;
		if ((Object)(object)apartmentDoor != (Object)null)
		{
			apartmentDoor.CancelInvoke(apartmentDoor.CloseRequest);
		}
	}

	public override bool HasLockPermission(BasePlayer player)
	{
		return HasAccess(player);
	}

	public override bool GetPlayerLockPermission(BasePlayer player)
	{
		return HasAccess(player);
	}

	public override void Load(LoadInfo info)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (base.isServer && info.msg.apartmentLock != null)
		{
			NetworkableId apartmentId = info.msg.apartmentLock.apartmentId;
			Room = BaseNetworkable.serverEntities.Find(apartmentId) as ApartmentRoom;
			if ((Object)(object)Room == (Object)null)
			{
				Debug.LogWarning((object)$"ApartmentLock {this} couldn't find apartment room '{apartmentId}' when loading");
			}
		}
	}

	public override void Save(SaveInfo info)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		info.msg.apartmentLock = Pool.Get<ApartmentLock>();
		if ((Object)(object)Room != (Object)null)
		{
			info.msg.apartmentLock.apartmentId = Room.net.ID;
		}
	}
}
