using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class InstantCameraTool : HeldEntity
{
	public ItemDefinition photoItem;

	public GameObjectRef screenshotEffect;

	public SoundDefinition startPhotoSoundDef;

	public SoundDefinition finishPhotoSoundDef;

	[Range(640f, 1920f)]
	public int resolutionX = 640;

	[Range(480f, 1080f)]
	public int resolutionY = 480;

	[Range(10f, 100f)]
	public int quality = 75;

	[Range(0f, 5f)]
	public float cooldownSeconds = 3f;

	[Header("Flash")]
	public GameObjectRef flashEffect;

	public GameObjectRef flashToggleEffect;

	public InstantCameraFlashController localFlash;

	public float flashDuration = 0.2f;

	public TimeSince _sinceLastPhoto;

	private bool hasSentAchievement;

	public const string PhotographPlayerAchievement = "SUMMER_PAPARAZZI";

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("InstantCameraTool.OnRpcMessage"))
		{
			if (rpc == 3122234259u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - TakePhoto"));
				}
				using (TimeWarning.New("TakePhoto"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3122234259u, "TakePhoto", this, player, 3uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(3122234259u, "TakePhoto", this, player))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg2 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							TakePhoto(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in TakePhoto");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		base.OnFlagsChanged(old, next);
		bool flag = (old & Flags.Reserved5) == Flags.Reserved5;
		bool flag2 = (next & Flags.Reserved5) == Flags.Reserved5;
		if (base.isServer && flag != flag2 && flashToggleEffect.isValid)
		{
			EffectNetwork.Send(new Effect(flashToggleEffect.resourcePath, this, 0u, Vector3.zero, Vector3.zero));
		}
	}

	[RPC_Server]
	[RPC_Server.FromOwner]
	[RPC_Server.CallsPerSecond(3uL)]
	private void TakePhoto(RPCMessage msg)
	{
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = msg.player;
		Item item = GetItem();
		if ((Object)(object)player == (Object)null || item == null || item.condition <= 0f)
		{
			return;
		}
		byte[] array = msg.read.BytesWithSize();
		if (array.Length > 102400 || !ImageProcessing.IsValidJPG(array, resolutionX, resolutionY))
		{
			return;
		}
		Item item2 = ItemManager.Create(photoItem, 1, 0uL, isServerSide: true, 0uL);
		if (item2 == null)
		{
			Debug.LogError((object)"Failed to create photo item");
			return;
		}
		item2.SetItemOwnership(msg.player, ItemOwnershipPhrases.Photographed);
		if (!((NetworkableId)(ref item2.instanceData.subEntity)).IsValid)
		{
			item2.Remove();
			Debug.LogError((object)"Photo has no sub-entity");
			return;
		}
		BaseNetworkable baseNetworkable = BaseNetworkable.serverEntities.Find(item2.instanceData.subEntity);
		if ((Object)(object)baseNetworkable == (Object)null)
		{
			item2.Remove();
			Debug.LogError((object)"Sub-entity was not found");
		}
		else if (!(baseNetworkable is PhotoEntity photoEntity))
		{
			item2.Remove();
			Debug.LogError((object)"Sub-entity is not a photo");
		}
		else
		{
			if (Interface.CallHook("OnPhotoCapture", photoEntity, item, player, array) != null)
			{
				return;
			}
			photoEntity.SetImageData(player.userID, array);
			if (!player.inventory.GiveItem(item2))
			{
				item2.Drop(player.GetDropPosition(), player.GetDropVelocity());
			}
			EffectNetwork.Send(new Effect(screenshotEffect.resourcePath, ((Component)this).transform.position, ((Component)this).transform.forward, msg.connection));
			if (HasFlag(Flags.Reserved5))
			{
				EffectNetwork.Send(new Effect(flashEffect.resourcePath, ((Component)localFlash).transform.position, ((Component)localFlash).transform.forward, msg.connection));
			}
			if (!hasSentAchievement && !string.IsNullOrEmpty("SUMMER_PAPARAZZI"))
			{
				Vector3 position = GetOwnerPlayer().eyes.position;
				Vector3 val = GetOwnerPlayer().eyes.HeadForward();
				List<BasePlayer> list = Pool.Get<List<BasePlayer>>();
				Vis.Entities(position + val * 5f, 5f, list, 131072, (QueryTriggerInteraction)2);
				foreach (BasePlayer item3 in list)
				{
					if (item3.isServer && (Object)(object)item3 != (Object)(object)GetOwnerPlayer() && item3.IsVisible(GetOwnerPlayer().eyes.position))
					{
						hasSentAchievement = true;
						GetOwnerPlayer().GiveAchievement("SUMMER_PAPARAZZI");
						break;
					}
				}
				Pool.FreeUnmanaged<BasePlayer>(ref list);
			}
			item.LoseCondition(1f);
			Interface.CallHook("OnPhotoCaptured", photoEntity, item, player, array);
		}
	}

	public override void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		base.OnDeployed(parent, deployedBy, fromItem);
		hasSentAchievement = false;
	}
}
