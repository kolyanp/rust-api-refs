using Facepunch;
using ProtoBuf;
using UnityEngine;

public class ApartmentUpkeepTerminal : StorageContainer
{
	public GameObject assignDialog;

	public ApartmentRoom Apartment { get; set; }

	public override void Save(SaveInfo info)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		if (!info.forDisk)
		{
			info.msg.apartmentUpkeep = Pool.Get<ApartmentUpkeepTerminal>();
			info.msg.apartmentUpkeep.apartmentId = (NetworkableId)(((Object)(object)Apartment != (Object)null) ? Apartment.net.ID : default(NetworkableId));
		}
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
	}
}
