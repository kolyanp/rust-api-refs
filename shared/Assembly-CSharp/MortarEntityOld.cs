using System;
using ConVar;
using Facepunch;
using Network;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class MortarEntityOld : StorageContainer
{
	[Header("Mortar")]
	public float MinAngle = 5f;

	public float MaxAngle = 90f;

	public float CurrentAngle = 45f;

	public float AnglePerAdjustment = 5f;

	public float CooldownDuration = 4f;

	public Transform ShellSpawnPoint;

	public Transform BarrelTransform;

	public float VelocityOverride = 20f;

	public float GravityOverride = 1f;

	public float DragOverride;

	public static readonly Flags IsIncreasingAngleFlag = Flags.Reserved10;

	public static readonly Flags CooldownFlag = Flags.Busy;

	public static readonly Flags AdjustmentModeFlag = Flags.Reserved2;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("MortarEntityOld.OnRpcMessage"))
		{
			if (rpc == 3116244173u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - AdjustAngle"));
				}
				using (TimeWarning.New("AdjustAngle"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3116244173u, "AdjustAngle", this, player, 3f))
						{
							return true;
						}
					}
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
							AdjustAngle(rpc2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in AdjustAngle");
					}
				}
				return true;
			}
			if (rpc == 3857190246u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - FireGun"));
				}
				using (TimeWarning.New("FireGun"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3857190246u, "FireGun", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rpc3 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							FireGun(rpc3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in FireGun");
					}
				}
				return true;
			}
			if (rpc == 1766714930 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SetAdjustmentMode"));
				}
				using (TimeWarning.New("SetAdjustmentMode"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1766714930u, "SetAdjustmentMode", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage adjustmentMode = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							SetAdjustmentMode(adjustmentMode);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in SetAdjustmentMode");
					}
				}
				return true;
			}
			if (rpc == 871919740 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SwitchAdjustmentAngle"));
				}
				using (TimeWarning.New("SwitchAdjustmentAngle"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(871919740u, "SwitchAdjustmentAngle", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage rpc4 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							SwitchAdjustmentAngle(rpc4);
						}
					}
					catch (Exception ex4)
					{
						Debug.LogException(ex4);
						player.Kick("RPC Error in SwitchAdjustmentAngle");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		base.inventory.canAcceptItem = CanAcceptItem;
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Open, b: false);
		}
		base.inventory.capacity = 1;
	}

	public bool CanAcceptItem(BasePlayer player, Item item, int slot)
	{
		return IsMortarAmmo(item);
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void FireGun(RPCMessage rpc)
	{
		if (!CanFireGun())
		{
			return;
		}
		if (rpc.player.HasMortarCooldown())
		{
			Debug.LogWarning((object)$"Player {rpc.player} called FireGun() but still has an active personal mortar cooldown");
			return;
		}
		Item activeItem = rpc.player.GetActiveItem();
		if (activeItem == null)
		{
			Debug.LogWarning((object)$"Player {rpc.player} called FireGun() while their hands were empty!");
			return;
		}
		if (!IsMortarAmmo(activeItem))
		{
			Debug.LogWarning((object)$"Player {rpc.player} called FireGun() while holding non-mortar ammo item {activeItem.info.shortname}!");
			return;
		}
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(CooldownFlag, b: true);
		}
		rpc.player.SetMortarCooldown(CooldownDuration);
		Invoke(EndCooldown, CooldownDuration);
		ItemDefinition info = activeItem.info;
		activeItem.UseItem();
		DoShoot(info);
	}

	private void EndCooldown()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(CooldownFlag, b: false);
	}

	private void DoShoot(ItemDefinition ammoDef)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		string resourcePath = ((Component)ammoDef).GetComponent<ItemModProjectile>().GetOverrideProjectile(this).resourcePath;
		BaseEntity baseEntity = GameManager.server.CreateEntity(resourcePath, ShellSpawnPoint.position, ShellSpawnPoint.rotation);
		ServerProjectile component = ((Component)baseEntity).GetComponent<ServerProjectile>();
		if ((Object)(object)component == (Object)null)
		{
			Debug.LogError((object)("MortarEntity.DoShoot() Spawned projectile '" + resourcePath + "' has no ServerProjectile component"));
			return;
		}
		Vector3 overrideVel = ShellSpawnPoint.forward * VelocityOverride;
		component.gravityModifier = GravityOverride;
		component.drag = DragOverride;
		component.InitializeVelocity(overrideVel);
		baseEntity.Spawn();
		Debug.Log((object)$"Launching mortar with velocity of {Math.Round(((Vector3)(ref overrideVel)).magnitude, 1)}m/s with drag of {component.drag} and gravity of {component.gravityModifier}");
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void SwitchAdjustmentAngle(RPCMessage rpc)
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(IsIncreasingAngleFlag, !HasFlag(IsIncreasingAngleFlag));
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void AdjustAngle(RPCMessage rpc)
	{
		bool flag = rpc.read.Bool();
		CurrentAngle = Mathf.Clamp(CurrentAngle + AnglePerAdjustment * (float)(flag ? 1 : (-1)), MinAngle, MaxAngle);
		Debug.Log((object)$"Angle: {CurrentAngle}");
		SendNetworkUpdate();
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void SetAdjustmentMode(RPCMessage rpc)
	{
		bool b = rpc.read.Bool();
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(AdjustmentModeFlag, b);
	}

	private void Update()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)BarrelTransform != (Object)null)
		{
			BarrelTransform.localRotation = Quaternion.Euler(90f - CurrentAngle, 0f, 0f);
		}
	}

	public bool IsHoldingMortarAmmo(BasePlayer player)
	{
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		Item activeItem = player.GetActiveItem();
		if (activeItem != null)
		{
			return IsMortarAmmo(activeItem);
		}
		return false;
	}

	public static bool IsMortarAmmo(Item item)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Invalid comparison between Unknown and I4
		if (item == null)
		{
			return false;
		}
		ItemModProjectile component = ((Component)item.info).GetComponent<ItemModProjectile>();
		if ((Object)(object)component != (Object)null)
		{
			return (component.ammoType & 0x40000) == 262144;
		}
		return false;
	}

	public bool CanFireGun()
	{
		return !HasFlag(CooldownFlag);
	}

	public bool InAdjustmentMode()
	{
		return HasFlag(AdjustmentModeFlag);
	}

	public bool IsHoldingAdjustmentTool(BasePlayer player)
	{
		Item activeItem = player.GetActiveItem();
		if (activeItem != null)
		{
			return activeItem.info.shortname == "pipetool";
		}
		return false;
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.mortar = Pool.Get<MortarData>();
		info.msg.mortar.angle = CurrentAngle;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.mortar != null)
		{
			CurrentAngle = info.msg.mortar.angle;
		}
	}

	public bool IsMortarAdjustable(BasePlayer player)
	{
		if (IsHoldingAdjustmentTool(player))
		{
			return InAdjustmentMode();
		}
		return false;
	}
}
