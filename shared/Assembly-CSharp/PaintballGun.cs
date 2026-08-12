using System;
using ConVar;
using Network;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class PaintballGun : BaseProjectile
{
	[Header("Paintball Gun")]
	public Renderer[] worldModelAmmoRenderers;

	private static readonly int shaderProperty_Fill = Shader.PropertyToID("_Fill");

	private static readonly int shaderProperty_Color = Shader.PropertyToID("_Color");

	private static MaterialPropertyBlock ammoRendererBlock;

	private int currentPaintballColor;

	private int paintballColorPreReload;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("PaintballGun.OnRpcMessage"))
		{
			if (rpc == 3661258788u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Server_PaintballColorChanged"));
				}
				using (TimeWarning.New("Server_PaintballColorChanged"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsActiveItem.Test(3661258788u, "Server_PaintballColorChanged", this, player))
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
							Server_PaintballColorChanged(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in Server_PaintballColorChanged");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override int GetImpactEffectNumberValue(HitInfo hitInfo)
	{
		AttackEntity weapon = hitInfo.Weapon;
		if ((Object)(object)weapon != (Object)null)
		{
			Item item = weapon.GetCachedItem();
			if (item != null && item.instanceData != null)
			{
				return item.instanceData.dataInt;
			}
		}
		return base.GetImpactEffectNumberValue(hitInfo);
	}

	public override DamageType GetDamageTypeForEffect(HitInfo info)
	{
		return DamageType.Paintball;
	}

	public override Effect.Type GetEffectType(HitInfo info)
	{
		if (info.HitEntity.IsValid() && info.HitEntity.GetImpactEffect(info).isValid)
		{
			return base.GetEffectType(info);
		}
		return Effect.Type.PaintballSplat;
	}

	public override bool ForceSendMagazine(SaveInfo saveInfo)
	{
		return true;
	}

	public override void DidAttackServerside()
	{
		SendNetworkUpdate();
	}

	[RPC_Server.IsActiveItem]
	[RPC_Server]
	private void Server_PaintballColorChanged(RPCMessage msg)
	{
		if (PaintballColorLookup.instance == null)
		{
			Debug.LogError((object)"Failed to retrieve PaintballColorLookup instance");
			return;
		}
		int num = currentPaintballColor;
		int num2 = msg.read.Int32();
		if (num != num2)
		{
			currentPaintballColor = Mathf.Clamp(num2, 0, PaintballColorLookup.instance.GetColorsCount() - 1);
			BasePlayer player = msg.player;
			player.Server_UpdatePaintballColor(currentPaintballColor);
			if (primaryMagazine.contents > 0)
			{
				player.GiveItem(ItemManager.CreateByItemID(primaryMagazine.ammoType.itemid, primaryMagazine.contents, 0uL, 0uL));
				SetAmmoCount(0);
			}
			SendNetworkUpdateImmediate();
			ItemManager.DoRemoves();
			player.inventory.ServerUpdate(0f);
		}
	}
}
