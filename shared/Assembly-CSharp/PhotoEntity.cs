using System;
using System.Collections.Generic;
using Facepunch;
using ProtoBuf;
using Rust;

public class PhotoEntity : ImageStorageEntity, IUGCBrowserEntity
{
	public ulong PhotographerSteamId { get; private set; }

	public uint ImageCrc { get; private set; }

	protected override uint CrcToLoad => ImageCrc;

	public override bool ShouldTransferAssociatedFiles => true;

	public uint[] GetContentCRCs
	{
		get
		{
			if (ImageCrc == 0)
			{
				return Array.Empty<uint>();
			}
			return new uint[1] { ImageCrc };
		}
	}

	public UGCType ContentType => UGCType.ImageJpg;

	public List<ulong> EditingHistory
	{
		get
		{
			if (PhotographerSteamId == 0)
			{
				return new List<ulong>();
			}
			return new List<ulong> { PhotographerSteamId };
		}
	}

	public BaseNetworkable UgcEntity => this;

	public string ContentString => string.Empty;

	public override void Load(LoadInfo info)
	{
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg.photo != null)
		{
			PhotographerSteamId = info.msg.photo.photographerSteamId;
			ImageCrc = info.msg.photo.imageCrc;
		}
		if (isServer)
		{
			ImageCrc = FileStorage.server.GetCrc(net.ID, 0uL);
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.photo = Pool.Get<Photo>();
		info.msg.photo.photographerSteamId = PhotographerSteamId;
		if (!info.forDisk)
		{
			info.msg.photo.imageCrc = ImageCrc;
		}
	}

	public void SetImageData(ulong steamId, byte[] data)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		ImageCrc = FileStorage.server.Store(net.ID, 0uL, FileStorage.Type.jpg, data);
		PhotographerSteamId = steamId;
		SendNetworkUpdate();
	}

	internal override void DoServerDestroy()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		base.DoServerDestroy();
		if (!Application.isQuitting && net != null)
		{
			FileStorage.server.RemoveAll(net.ID);
		}
	}

	public void ClearContent()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		FileStorage.server.RemoveAll(net.ID);
		ImageCrc = 0u;
		SendNetworkUpdate();
	}
}
