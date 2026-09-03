using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ConVar;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class CopyPasteEntity : PointEntity
{
	public static CopyPasteEntity ServerInstance;

	public const string ClientDirectory = "copypaste";

	public const string FileExtension = ".data";

	public unsafe override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("CopyPasteEntity.OnRpcMessage"))
		{
			if (rpc == 2913956655u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Paste"));
				}
				using (TimeWarning.New("Paste"))
				{
					FieldOrderValidationScope val = msg.read.SuspendProtoFieldOrderValidation();
					try
					{
						FieldOperationLimitSuspensionScope val2 = msg.read.SuspendProtoFieldOperationLimit();
						try
						{
							using (TimeWarning.New("Call"))
							{
								RPCMessage rpc2 = new RPCMessage
								{
									connection = msg.connection,
									player = player,
									read = msg.read
								};
								Paste(rpc2);
							}
						}
						catch (Exception ex)
						{
							Debug.LogException(ex);
							player.Kick("RPC Error in Paste");
						}
						finally
						{
							((IDisposable)(*(FieldOperationLimitSuspensionScope*)(&val2))/*cast due to constrained. prefix*/).Dispose();
						}
					}
					finally
					{
						((IDisposable)(*(FieldOrderValidationScope*)(&val))/*cast due to constrained. prefix*/).Dispose();
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public static string MakeFilenameSafe(string input)
	{
		return Regex.Replace(input, "[^a-zA-Z0-9_\\- ]", "");
	}

	public static string GetClientPath(string fileName)
	{
		return Path.Combine("copypaste", MakeFilenameSafe(fileName) + ".data");
	}

	public static byte[] LoadScreenshot(string fileName)
	{
		string clientPath = GetClientPath(fileName);
		if (!File.Exists(clientPath))
		{
			return null;
		}
		CopyPasteEntityInfo val = CopyPasteEntityInfo.Deserialize(File.ReadAllBytes(clientPath));
		try
		{
			return val.screenshot;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static string[] GetLocalPasteNames()
	{
		return (from x in Directory.GetFiles("copypaste", "*.data")
			select Path.GetFileNameWithoutExtension(x)).ToArray();
	}

	[RPC_Server.IgnoreProtoFieldOperationLimit]
	[RPC_Server.IgnoreProtoFieldOrder]
	[RPC_Server]
	public void Paste(RPCMessage rpc)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		if (!rpc.player.IsAdmin)
		{
			return;
		}
		PasteRequest val = rpc.read.Proto<PasteRequest>((PasteRequest)null);
		CopyPasteEntityInfo pasteData = val.pasteData;
		CopyPaste.PasteOptions pasteOptions = new CopyPaste.PasteOptions(val);
		List<BaseEntity> list = new List<BaseEntity>();
		List<Vector3> list2 = new List<Vector3>();
		if (val.pasteOffsets == null || val.pasteOffsets.Count == 0)
		{
			list2.Add(Vector3.zero);
		}
		else
		{
			list2.AddRange(val.pasteOffsets);
		}
		foreach (Vector3 item in list2)
		{
			pasteOptions.HeightOffset = new Vector3(0f, pasteOptions.HeightOffset.y, 0f) + item;
			List<BaseEntity> list3 = CopyPaste.PasteEntities(pasteData, pasteOptions, rpc.player.userID);
			if (pasteOptions.Players)
			{
				foreach (BaseEntity item2 in list3)
				{
					if (item2 is BasePlayer)
					{
						item2.ForceUpdateTriggers(enter: true, exit: false, invoke: false);
					}
				}
			}
			list.AddRange(list3);
		}
		if (list.Count > 0)
		{
			CopyPaste.playerHistory.AddToHistory(rpc.player.userID, list);
		}
		if (list2.Count == 1)
		{
			rpc.player.ConsoleMessage($"Pasted {list.Count} entities");
		}
		else
		{
			rpc.player.ConsoleMessage($"Pasted {list.Count} entities ({list2.Count} groups)");
		}
	}

	public void OnEnable()
	{
		if (base.isServer)
		{
			if ((Object)(object)ServerInstance != (Object)null)
			{
				Debug.LogError((object)"Major fuckup! CopyPasteEntity spawned twice, Contact Developers!");
				Object.Destroy((Object)(object)((Component)this).gameObject);
			}
			else
			{
				ServerInstance = this;
			}
		}
	}

	public void OnDestroy()
	{
		if (base.isServer)
		{
			ServerInstance = null;
		}
	}
}
