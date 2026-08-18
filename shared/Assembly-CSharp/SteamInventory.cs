using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConVar;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class SteamInventory : EntityComponent<BasePlayer>
{
	private static int workshopSkinCacheVersion;

	public IPlayerItem[] Items;

	private HashSet<string> workshopSkinShortNames;

	private int workshopSkinVersion = -1;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("SteamInventory.OnRpcMessage"))
		{
			if (rpc == 643458331 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - UpdateSteamInventory"));
				}
				using (TimeWarning.New("UpdateSteamInventory"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!BaseEntity.RPC_Server.FromOwner.Test(643458331u, "UpdateSteamInventory", GetBaseEntity(), player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							BaseEntity.RPCMessage msg2 = new BaseEntity.RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							UpdateSteamInventory(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in UpdateSteamInventory");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public static void InvalidateWorkshopSkinCaches()
	{
		workshopSkinCacheVersion++;
	}

	public bool HasItem(int itemid)
	{
		if (!base.baseEntity.DefaultSkinAccess)
		{
			return base.baseEntity.AllSkinsUnlocked;
		}
		if (Items == null)
		{
			return false;
		}
		IPlayerItem[] items = Items;
		for (int i = 0; i < items.Length; i++)
		{
			if (items[i].DefinitionId == itemid)
			{
				return true;
			}
		}
		return false;
	}

	public bool HasWorkshopSkin(ItemDefinition itemDefinition)
	{
		if (!base.baseEntity.DefaultSkinAccess)
		{
			return base.baseEntity.AllSkinsUnlocked;
		}
		if (Items == null)
		{
			return false;
		}
		if (workshopSkinShortNames == null || workshopSkinVersion != workshopSkinCacheVersion)
		{
			if (!PlatformService.Instance.IsValid || PlatformService.Instance.ItemDefinitions == null)
			{
				return false;
			}
			workshopSkinShortNames = new HashSet<string>();
			IPlayerItem[] items = Items;
			foreach (IPlayerItem val in items)
			{
				IPlayerItemDefinition itemDefinition2 = PlatformService.Instance.GetItemDefinition(val.DefinitionId);
				if (itemDefinition2 != null && itemDefinition2.WorkshopId != 0L)
				{
					string itemShortName = itemDefinition2.ItemShortName;
					if (!string.IsNullOrEmpty(itemShortName))
					{
						workshopSkinShortNames.Add(itemShortName);
					}
				}
			}
			workshopSkinVersion = workshopSkinCacheVersion;
		}
		if (!workshopSkinShortNames.Contains(itemDefinition.shortname))
		{
			return workshopSkinShortNames.Contains(((Object)itemDefinition).name);
		}
		return true;
	}

	private void SetItems(IPlayerItem[] items)
	{
		Items = items;
		workshopSkinShortNames = null;
		workshopSkinVersion = -1;
	}

	[BaseEntity.RPC_Server.FromOwner]
	[BaseEntity.RPC_Server]
	private async Task UpdateSteamInventory(BaseEntity.RPCMessage msg)
	{
		byte[] array = msg.read.BytesWithSize();
		if (array == null)
		{
			Debug.LogWarning((object)"UpdateSteamInventory: Data is null");
			return;
		}
		IPlayerInventory val = await PlatformService.Instance.DeserializeInventory(array);
		if (val == null)
		{
			Debug.LogWarning((object)"UpdateSteamInventory: result is null");
		}
		else if ((Object)(object)base.baseEntity == (Object)null)
		{
			Debug.LogWarning((object)"UpdateSteamInventory: player is null");
		}
		else if (!val.BelongsTo((ulong)base.baseEntity.userID))
		{
			Debug.LogWarning((object)$"UpdateSteamPlayer: inventory belongs to someone else (userID={base.baseEntity.userID.Get()})");
		}
		else if (Object.op_Implicit((Object)(object)((Component)this).gameObject))
		{
			SetItems(val.Items.ToArray());
			Interface.CallHook("OnSteamInventoryUpdated", this);
			((IDisposable)val).Dispose();
		}
	}
}
