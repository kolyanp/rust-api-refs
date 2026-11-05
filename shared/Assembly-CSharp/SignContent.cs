using System.Collections.Generic;
using System.Linq;
using Facepunch;
using Oxide.Core;
using ProtoBuf;

public class SignContent : ImageStorageEntity, IUGCBrowserEntity
{
	private uint imageCrc;

	private List<ulong> editHistory = new List<ulong>();

	protected override uint CrcToLoad => imageCrc;

	public UGCType ContentType => UGCType.ImagePng;

	public uint[] GetContentCRCs
	{
		get
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			Dictionary<ulong, uint> dictionary = Pool.Get<Dictionary<ulong, uint>>();
			FileStorage.server.GetCrcs(net.ID, dictionary);
			uint[] array = new uint[(dictionary.Count > 0) ? (dictionary.Keys.Max() + 1) : 0];
			foreach (KeyValuePair<ulong, uint> item in dictionary)
			{
				array[item.Key] = item.Value;
			}
			Pool.FreeUnmanaged<ulong, uint>(ref dictionary);
			return array;
		}
	}

	public List<ulong> EditingHistory => editHistory;

	public BaseNetworkable UgcEntity => this;

	public string ContentString => string.Empty;

	public override bool ShouldTransferAssociatedFiles => true;

	public override void Load(LoadInfo info)
	{
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		PaintableSign paintableSign = info.msg.paintableSign;
		if (paintableSign != null && paintableSign.crcs?.Count > 0)
		{
			imageCrc = info.msg.paintableSign.crcs[0];
		}
		if (isServer)
		{
			imageCrc = FileStorage.server.GetCrc(net.ID, 0uL);
		}
	}

	public void CopyInfoFromSign(ISignage s, IUGCBrowserEntity b)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		editHistory.Clear();
		foreach (ulong item in b.EditingHistory)
		{
			editHistory.Add(item);
		}
		FileStorage.server.Reassign(s.NetworkID, net.ID);
		imageCrc = FileStorage.server.GetCrc(net.ID, 0uL);
		SendNetworkUpdate();
	}

	public void CopyInfoToSign(ISignage s, IUGCBrowserEntity b)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		FileStorage.server.Reassign(net.ID, s.NetworkID);
		s.RefreshFileCrcs();
		b.EditingHistory.Clear();
		foreach (ulong item in editHistory)
		{
			b.EditingHistory.Add(item);
		}
		Interface.CallHook("OnSignContentCopied", this, s, b);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.msg.paintableSign == null)
		{
			info.msg.paintableSign = Pool.Get<PaintableSign>();
		}
		info.msg.paintableSign.crcs = Pool.Get<List<uint>>();
		if (!info.forDisk)
		{
			info.msg.paintableSign.crcs.Add(imageCrc);
		}
	}

	internal override void DoServerDestroy()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		base.DoServerDestroy();
		FileStorage.server.RemoveAll(net.ID);
	}

	public void ClearContent()
	{
		Kill();
	}
}
