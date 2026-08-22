using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using Carbon.Base;
using Carbon.Components;
using Carbon.Pooling;
using Facepunch;
using HarmonyLib;
using JetBrains.Annotations;
using Network;
using Oxide.Core;
using Oxide.Core.Plugins;
using Oxide.Game.Rust.Cui;
using Rust.Ai;
using SilentOrbit.ProtocolBuffers;
using UnityEngine;

namespace Carbon.Modules;

public class VanishModule : CarbonModule<VanishConfig, EmptyModuleData>
{
	public class VanishedPlayer : FacepunchBehaviour
	{
		public BasePlayer player;

		private void Start()
		{
			((MonoBehaviour)this).InvokeRepeating("UpdateNetworkGroups", 1f, 5f);
		}

		private void UpdateNetworkGroups()
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			if (!((Object)(object)player == (Object)null) && player.IsConnected)
			{
				((BaseNetworkable)player).net.UpdateGroups(((Component)player).transform.position, ((BaseNetworkable)player).networkRange);
			}
		}

		public void Init(BasePlayer player)
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			this.player = player;
			((Component)this).gameObject.layer = 3;
			((Component)this).gameObject.transform.localPosition = Vector3.zero;
			((Component)this).gameObject.transform.localRotation = Quaternion.identity;
			CapsuleCollider val = player.colliderValue.Get();
			CapsuleCollider val2 = ((Component)this).gameObject.AddComponent<CapsuleCollider>();
			val2.center = val.center;
			val2.radius = val.radius;
			val2.height = val.height;
			val2.direction = val.direction;
			((Collider)val2).isTrigger = true;
			List<Collider> list = Pool.Get<List<Collider>>();
			Vis.Components<Collider>(((Component)this).gameObject.transform.position, val2.radius, list, -1, (QueryTriggerInteraction)2);
			foreach (Collider item in list)
			{
				OnTriggerEnter(item);
			}
			Pool.FreeUnmanaged<Collider>(ref list);
		}

		private void OnTriggerEnter(Collider collider)
		{
			TriggerParent component = ((Component)collider).gameObject.GetComponent<TriggerParent>();
			if (!((Object)(object)component == (Object)null))
			{
				((TriggerBase)component).OnEntityEnter((BaseEntity)(object)player);
			}
		}

		private void OnTriggerExit(Collider collider)
		{
			TriggerParent component = ((Component)collider).gameObject.GetComponent<TriggerParent>();
			if (!((Object)(object)component == (Object)null))
			{
				((TriggerBase)component).OnEntityLeave((BaseEntity)(object)player);
			}
		}
	}

	[AutoPatch]
	[HarmonyPatch(typeof(Item), "SetItemOwnership", new Type[]
	{
		typeof(BasePlayer),
		typeof(Phrase)
	})]
	public static class OwnershipPatch
	{
		[UsedImplicitly]
		public static bool Prefix(BasePlayer player, Phrase reason)
		{
			return !Singleton.IsPlayerVanished(player);
		}
	}

	[AutoPatch]
	[HarmonyPatch(typeof(StorageContainer), "CanBeLooted", new Type[] { typeof(BasePlayer) })]
	public static class StorageContainerPatch
	{
		[UsedImplicitly]
		[HarmonyPrefix]
		public static bool Prefix(ref bool __result, BasePlayer player)
		{
			if ((Object)(object)player != (Object)null && Singleton != null && ((BaseModule)Singleton).IsEnabled() && Singleton.IsPlayerVanished(player) && ((CarbonModule<VanishConfig, EmptyModuleData>)Singleton).Permissions.UserHasPermission(player.UserIDString, ((CarbonModule<VanishConfig, EmptyModuleData>)Singleton).ConfigInstance.VanishUnlockWhileVanishedPermission))
			{
				__result = true;
				return false;
			}
			return true;
		}
	}

	[AutoPatch]
	[HarmonyPatch(typeof(BasePlayer), "Teleport", new Type[] { typeof(Vector3) })]
	public static class TeleportPatch
	{
		[UsedImplicitly]
		[HarmonyPrefix]
		public static bool Prefix(BasePlayer __instance, Vector3 position)
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			if (Singleton == null || !((BaseModule)Singleton).IsEnabled() || !Singleton.IsPlayerVanished(__instance))
			{
				return true;
			}
			__instance.MovePosition(position, false);
			((BaseEntity)__instance).ClientRPC(RpcTarget.Player("ForcePositionTo", __instance), position);
			return false;
		}
	}

	[AutoPatch]
	[HarmonyPatch(typeof(BaseEntity), "SignalBroadcast", new Type[]
	{
		typeof(Signal),
		typeof(string),
		typeof(Connection),
		typeof(string),
		typeof(float)
	})]
	public static class SignalBroadcastPatch
	{
		[UsedImplicitly]
		[HarmonyPrefix]
		public static bool Prefix(Connection sourceConnection)
		{
			if (sourceConnection == null)
			{
				return true;
			}
			if (Singleton != null && ((BaseModule)Singleton).IsEnabled())
			{
				return !Singleton.IsPlayerVanished(sourceConnection.userid);
			}
			return true;
		}
	}

	[AutoPatch]
	[HarmonyPatch(typeof(BaseNetworkable), "GetConnectionsWithin", new Type[]
	{
		typeof(Vector3),
		typeof(float),
		typeof(bool)
	})]
	public static class GetConnectionsWithinPatch
	{
		[UsedImplicitly]
		[HarmonyTranspiler]
		public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Expected O, but got Unknown
			//IL_016c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0172: Expected O, but got Unknown
			//IL_018f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0195: Expected O, but got Unknown
			//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ae: Expected O, but got Unknown
			CodeMatcher val = new CodeMatcher(instructions, generator).MatchStartForward((CodeMatch[])(object)new CodeMatch[1]
			{
				new CodeMatch((OpCode?)OpCodes.Ldsfld, (object)AccessTools.Field(typeof(BasePlayer), "invisPlayers"), (string)null)
			}).ThrowIfInvalid("Could not find BasePlayer.invisPlayers in BaseNetworkable.GetConnectionsWithin").MatchStartForward((CodeMatch[])(object)new CodeMatch[1] { CodeMatch.Calls(AccessTools.PropertyGetter(typeof(Component), "transform")) })
				.ThrowIfInvalid("Could not find invis player transform access in BaseNetworkable.GetConnectionsWithin");
			int pos = val.Pos;
			val.MatchStartForward((CodeMatch[])(object)new CodeMatch[1] { CodeMatch.Calls(AccessTools.PropertyGetter(typeof(Vector3), "sqrMagnitude")) }).ThrowIfInvalid("Could not find invis player distance check in BaseNetworkable.GetConnectionsWithin");
			List<CodeInstruction> list = val.Instructions().ToList();
			int index = pos - 1;
			CodeInstruction val2 = list[index];
			Label? label = null;
			for (int i = val.Pos; i < list.Count; i++)
			{
				CodeInstruction val3 = list[i];
				if ((val3.opcode == OpCodes.Bgt_Un || val3.opcode == OpCodes.Bgt_Un_S) && val3.operand is Label value)
				{
					label = value;
					break;
				}
			}
			if (!label.HasValue)
			{
				throw new InvalidOperationException("Could not find invis player loop continue label in BaseNetworkable.GetConnectionsWithin");
			}
			list.InsertRange(index, new _003C_003Ez__ReadOnlyArray<CodeInstruction>((CodeInstruction[])(object)new CodeInstruction[3]
			{
				new CodeInstruction(val2.opcode, val2.operand),
				new CodeInstruction(OpCodes.Call, (object)AccessTools.Method(typeof(VanishModule), "IsValidInvisPlayerConnection", (Type[])null, (Type[])null)),
				new CodeInstruction(OpCodes.Brfalse, (object)label.Value)
			}));
			return list;
		}
	}

	[AutoPatch]
	[HarmonyPatch(typeof(EffectNetwork), "Send", new Type[]
	{
		typeof(Effect),
		typeof(EntityNetworkRange)
	})]
	public static class EffectNetworkPatch
	{
		[UsedImplicitly]
		[HarmonyPrefix]
		public static bool Prefix(Effect effect, EntityNetworkRange networkRange)
		{
			if (effect == null || ((EffectData)effect).source == 0L)
			{
				return true;
			}
			if (Singleton != null && ((BaseModule)Singleton).IsEnabled())
			{
				return !Singleton.IsPlayerVanished(((EffectData)effect).source);
			}
			return true;
		}
	}

	[AutoPatch]
	[HarmonyPatch(typeof(StorageContainer), "ShouldRequireAuthIfNoCodelock")]
	private static class StorageContainerPatch2
	{
		[UsedImplicitly]
		[HarmonyPrefix]
		private static bool Prefix(ref bool __result, BaseEntity container)
		{
			if (!(container is ContainerIOEntity))
			{
				return true;
			}
			if (Singleton == null || !((BaseModule)Singleton).IsEnabled())
			{
				return true;
			}
			BasePlayer lastLooter = Singleton._lastLooter;
			Singleton._lastLooter = null;
			if ((Object)(object)lastLooter != (Object)null && Singleton.IsPlayerVanished(lastLooter) && ((CarbonModule<VanishConfig, EmptyModuleData>)Singleton).Permissions.UserHasPermission(lastLooter.UserIDString, ((CarbonModule<VanishConfig, EmptyModuleData>)Singleton).ConfigInstance.VanishUnlockWhileVanishedPermission))
			{
				__result = false;
				return false;
			}
			return true;
		}
	}

	private static VanishModule Singleton;

	private readonly Handler Handler;

	private Dictionary<ulong, Vector3> _vanishedPlayers;

	private BasePlayer _lastLooter;

	private readonly GameObjectRef _drownEffect;

	private readonly GameObjectRef _fallDamageEffect;

	private readonly GameObjectRef _emptyEffect;

	public override string Name => "Vanish";

	public override Type Type => typeof(VanishModule);

	public override VersionNumber Version
	{
		get
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			return new VersionNumber(1, 0, 0);
		}
	}

	public override bool ForceModded => false;

	public override bool EnabledByDefault => false;

	public override void OnServerInit(bool initial)
	{
		Singleton = this;
		base.OnServerInit(initial);
		if (initial)
		{
			((CarbonModule<VanishConfig, EmptyModuleData>)this).OnEnabled(true);
		}
	}

	public override void OnEnabled(bool initialized)
	{
		base.OnEnabled(initialized);
		if (initialized)
		{
			base.Permissions.RegisterPermission(base.ConfigInstance.VanishPermission, (BaseHookable)(object)this);
			base.Permissions.RegisterPermission(base.ConfigInstance.VanishUnlockWhileVanishedPermission, (BaseHookable)(object)this);
			base.Permissions.RegisterPermission(base.ConfigInstance.PermanentVanishPermission, (BaseHookable)(object)this);
			((Plugin)Community.Runtime.Core).cmd.AddCovalenceCommand(base.ConfigInstance.VanishCommand, (BaseHookable)(object)this, "Vanish", (string)null, (object)null, new string[1] { base.ConfigInstance.VanishPermission }, (string[])null, -1, 0, false, false, true, false);
		}
	}

	public override void OnDisabled(bool initialized)
	{
		base.OnDisabled(initialized);
		ulong[] array = _vanishedPlayers.Keys.ToArray();
		foreach (ulong num in array)
		{
			BasePlayer val = BasePlayer.FindByID(num);
			if ((Object)(object)val != (Object)null)
			{
				DoVanish(val, wants: false, withUI: true, toggleNoclip: true, ignorePermanentVanish: true);
			}
		}
		_vanishedPlayers.Clear();
	}

	public bool IsPlayerVanished(ulong playerId)
	{
		return _vanishedPlayers.ContainsKey(playerId);
	}

	public bool IsPlayerVanished(BasePlayer player)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)player != (Object)null)
		{
			return _vanishedPlayers.ContainsKey(EncryptedValue<ulong>.op_Implicit(player.userID));
		}
		return false;
	}

	public Vector3 GetVanishedPlayerPosition(ulong playerId)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (!_vanishedPlayers.TryGetValue(playerId, out var value))
		{
			return Vector3.zero;
		}
		return value;
	}

	public Vector3 GetVanishedPlayerPosition(BasePlayer player)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)player != (Object)null) || !_vanishedPlayers.TryGetValue(EncryptedValue<ulong>.op_Implicit(player.userID), out var value))
		{
			return Vector3.zero;
		}
		return value;
	}

	private object CanUseLockedEntity(BasePlayer player, BaseLock @lock)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		if (_vanishedPlayers.ContainsKey(EncryptedValue<ulong>.op_Implicit(player.userID)) && base.Permissions.UserHasPermission(player.UserIDString, base.ConfigInstance.VanishUnlockWhileVanishedPermission))
		{
			return true;
		}
		return null;
	}

	private object OnPlayerAttack(BasePlayer player, HitInfo hit)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if (hit == null || (Object)(object)hit.Initiator == (Object)null || (Object)(object)hit.HitEntity == (Object)null)
		{
			return null;
		}
		BaseEntity initiator = hit.Initiator;
		BasePlayer val = (BasePlayer)(object)((initiator is BasePlayer) ? initiator : null);
		if (val != null && hit.HitEntity.OwnerID != EncryptedValue<ulong>.op_Implicit(val.userID) && !base.ConfigInstance.CanDamageWhenVanished && _vanishedPlayers.ContainsKey(EncryptedValue<ulong>.op_Implicit(val.userID)))
		{
			BasePlayer obj = BasePlayer.FindByID(hit.HitEntity.OwnerID);
			player.ChatMessage("You're vanished. You may not damage this entity owned by " + (((obj != null) ? obj.displayName : null) ?? hit.HitEntity.OwnerID.ToString()) + ".");
			return false;
		}
		return null;
	}

	private object CanBradleyApcTarget(BradleyAPC apc, BasePlayer player)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		if (_vanishedPlayers.ContainsKey(EncryptedValue<ulong>.op_Implicit(player.userID)))
		{
			return false;
		}
		return null;
	}

	private void OnPlayerSleepEnded(BasePlayer self)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (base.Permissions.UserHasPermission(self.UserIDString, base.ConfigInstance.PermanentVanishPermission))
		{
			DoVanish(self, wants: true);
		}
		else if (_vanishedPlayers.ContainsKey(EncryptedValue<ulong>.op_Implicit(self.userID)))
		{
			DoVanish(self, wants: true);
		}
	}

	private void CanLootEntity(BasePlayer player, ContainerIOEntity container)
	{
		_lastLooter = player;
	}

	private static void SendEffectTo(string effect, BasePlayer player)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)player == (Object)null))
		{
			Effect reusableInstance = Effect.reusableInstance;
			reusableInstance.Init((Type)0, (BaseEntity)(object)player, 0u, Vector3.up, Vector3.zero, (Connection)null);
			((EffectData)reusableInstance).pooledstringid = StringPool.Get(effect);
			NetWrite val = ((BaseNetwork)Net.sv).StartWrite();
			val.PacketID((Type)13);
			ProtoStreamExtensions.WriteToStream((IProto)(object)reusableInstance, (Stream)(object)val, false, 2097152);
			val.Send(new SendInfo(((BaseNetworkable)player).net.connection));
			reusableInstance.Clear();
		}
	}

	public unsafe void DoVanish(BasePlayer player, bool wants, bool withUI = true, bool toggleNoclip = true, bool ignorePermanentVanish = false)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0379: Unknown result type (might be due to invalid IL or missing references)
		//IL_0389: Unknown result type (might be due to invalid IL or missing references)
		//IL_038e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0393: Unknown result type (might be due to invalid IL or missing references)
		//IL_0398: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0495: Unknown result type (might be due to invalid IL or missing references)
		//IL_049c: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_050e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02dc: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)player == (Object)null)
		{
			return;
		}
		bool flag = _vanishedPlayers.ContainsKey(EncryptedValue<ulong>.op_Implicit(player.userID));
		if (!wants && !flag)
		{
			return;
		}
		if (!wants && _vanishedPlayers.TryGetValue(EncryptedValue<ulong>.op_Implicit(player.userID), out var value))
		{
			if (!ignorePermanentVanish && base.Permissions.UserHasPermission(player.UserIDString, base.ConfigInstance.PermanentVanishPermission))
			{
				player.ChatMessage("You're permanently vanished due to your permission and/or group.");
				return;
			}
			_vanishedPlayers.Remove(EncryptedValue<ulong>.op_Implicit(player.userID));
			if (base.ConfigInstance.TeleportBackOnUnvanish)
			{
				player.Teleport(value);
			}
		}
		else if (wants && !flag)
		{
			_vanishedPlayers.Add(EncryptedValue<ulong>.op_Implicit(player.userID), ((Component)player).transform.position);
		}
		if (wants)
		{
			_clearTriggers(player);
			player.PauseFlyHackDetection(float.MaxValue);
			player.PauseSpeedHackDetection(float.MaxValue);
			player.PauseTickDistanceDetection(float.MaxValue);
			player.PauseVehicleNoClipDetection(float.MaxValue);
			AntiHack.ShouldIgnore(player);
			player.fallDamageEffect = _emptyEffect;
			player.drownEffect = _emptyEffect;
			((BaseNetworkable)player)._limitedNetworking = true;
			((BaseEntity)player).syncPosition = false;
			player.isInvisible = true;
			if (!BasePlayer.invisPlayers.Contains(player))
			{
				BasePlayer.invisPlayers.Add(player);
			}
			Query.Server.RemovePlayer(player);
			player.DisablePlayerCollider();
			PooledList<Connection> val = Pool.Get<PooledList<Connection>>();
			try
			{
				((List<Connection>)(object)val).AddRange(Net.sv.connections.Where((Connection connection) => connection.connected && connection.isAuthenticated && connection.player is BasePlayer && (Object)(object)connection.player != (Object)(object)player));
				((BaseNetworkable)player).OnNetworkSubscribersLeave((List<Connection>)(object)val);
				((Component)player).transform.localScale = Vector3.zero;
				SimpleAIMemory.AddIgnorePlayer(player);
				if (!flag && base.ConfigInstance.WhooshSoundOnVanish)
				{
					if (base.ConfigInstance.BroadcastVanishSounds)
					{
						server.Run(base.ConfigInstance.Effect.Vanishing, ((Component)player).transform.position, default(Vector3), (Connection)null, false, (List<Connection>)null, 0, (Type)0);
					}
					else
					{
						SendEffectTo(base.ConfigInstance.Effect.Vanishing, player);
					}
				}
				if (withUI)
				{
					_drawUI(player);
				}
				if (!flag && base.ConfigInstance.EnableLogs)
				{
					base.Puts((object)$"{player} just vanished at {((Component)player).transform.position}");
				}
				if (((!flag && base.ConfigInstance.ToggleNoclipOnVanish) & toggleNoclip) && ((BaseNetworkable)player).net.connection.authLevel != 0 && !player.IsFlying)
				{
					player.SendConsoleCommand("noclip", Array.Empty<object>());
				}
				EnsureVanishComponent(player);
				if (!flag)
				{
					HookCaller.CallStaticHook(778631450u, (object)player);
				}
				return;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		((Component)player).transform.localScale = Vector3.one;
		BasePlayer.ResetAntiHack(player, AntiHack.PlayerStates, AntiHack.PlayerNoclipStates, AntiHack.PlayerSpeedhackStates, AntiHack.PlayerFlyhackStates);
		((BaseEntity)player).syncPosition = true;
		((BaseNetworkable)player)._limitedNetworking = false;
		player.isInvisible = false;
		BasePlayer.invisPlayers.Remove(player);
		Query.Server.RemovePlayer(player);
		Query.Server.AddPlayer(player);
		player.EnablePlayerCollider();
		((BaseNetworkable)player).SendNetworkUpdate((NetworkQueue)0);
		HeldEntity heldEntity = player.GetHeldEntity();
		if (heldEntity != null)
		{
			((BaseNetworkable)heldEntity).SendNetworkUpdate((NetworkQueue)0);
		}
		SimpleAIMemory.RemoveIgnorePlayer(player);
		player.drownEffect = _drownEffect;
		player.fallDamageEffect = _fallDamageEffect;
		((BaseEntity)player).ForceUpdateTriggers(true, false, true);
		if (base.ConfigInstance.GutshotScreamOnUnvanish)
		{
			if (base.ConfigInstance.BroadcastVanishSounds)
			{
				server.Run(base.ConfigInstance.Effect.Unvanishing, ((Component)player).transform.position, default(Vector3), (Connection)null, false, (List<Connection>)null, 0, (Type)0);
			}
			else
			{
				SendEffectTo(base.ConfigInstance.Effect.Unvanishing, player);
			}
		}
		CUI val2 = default(CUI);
		((CUI)(ref val2))._002Ector(Handler);
		try
		{
			((CUI)(ref val2)).Destroy("vanishui", player);
			if (base.ConfigInstance.EnableLogs)
			{
				base.Puts((object)$"{player} unvanished at {((Component)player).transform.position}");
			}
			if ((base.ConfigInstance.ToggleNoclipOnUnvanish & toggleNoclip) && ((BaseNetworkable)player).net.connection.authLevel != 0 && player.IsFlying)
			{
				player.SendConsoleCommand("noclip", Array.Empty<object>());
			}
			DestroyVanishComponents(player);
			if (flag)
			{
				HookCaller.CallStaticHook(3385747762u, (object)player);
			}
		}
		finally
		{
			((IDisposable)(*(CUI*)(&val2))/*cast due to constrained. prefix*/).Dispose();
		}
	}

	private static void EnsureVanishComponent(BasePlayer player)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		if (!((Object)(object)((Component)player).GetComponentInChildren<VanishedPlayer>() != (Object)null))
		{
			GameObject val = new GameObject("Vanish Collider");
			val.transform.SetParent(((Component)player).transform, true);
			val.AddComponent<VanishedPlayer>().Init(player);
		}
	}

	private static void DestroyVanishComponents(BasePlayer player)
	{
		VanishedPlayer[] componentsInChildren = ((Component)player).GetComponentsInChildren<VanishedPlayer>();
		foreach (VanishedPlayer vanishedPlayer in componentsInChildren)
		{
			if ((Object)(object)vanishedPlayer != (Object)null)
			{
				Object.Destroy((Object)(object)((Component)vanishedPlayer).gameObject);
			}
		}
	}

	private static bool IsValidInvisPlayerConnection(BasePlayer player)
	{
		try
		{
			return (Object)(object)player != (Object)null && player.Connection != null && (Object)(object)((Component)player).transform != (Object)null;
		}
		catch
		{
			return false;
		}
	}

	private void Vanish(BasePlayer player, string cmd, string[] args)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		DoVanish(player, !_vanishedPlayers.ContainsKey(EncryptedValue<ulong>.op_Implicit(player.userID)));
	}

	private void _clearTriggers(BasePlayer player)
	{
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		if (((BaseEntity)player).triggers != null && ((BaseEntity)player).triggers.Count > 0)
		{
			foreach (TriggerBase trigger in ((BaseEntity)player).triggers)
			{
				trigger.OnEntityLeave((BaseEntity)(object)player);
			}
		}
		foreach (PatrolHelicopter item in ((IEnumerable)BaseNetworkable.serverEntities).OfType<PatrolHelicopter>())
		{
			if (!((Object)(object)item.myAI == (Object)null) && !((Object)(object)item.myAI.strafe_target != (Object)(object)player))
			{
				Logger.Warn((object)$"Patrol Helicopter at {((Component)item).transform.position} ended player strafe for '{player.Connection}'");
				item.myAI.State_OrbitStrafe_Leave();
				item.myAI.State_Strafe_Leave();
			}
		}
	}

	private unsafe void _drawUI(BasePlayer player)
	{
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		CUI val = default(CUI);
		((CUI)(ref val))._002Ector(Handler);
		try
		{
			CuiElementContainer val2 = ((CUI)(ref val)).CreateContainer("vanishui", "0 0 0 0", 0f, 1f, 0f, 1f, 0f, 0f, 0f, 0f, 0f, 0f, false, false, (ClientPanels)5, "vanishui", true, 0f, (bool?)null);
			if (!string.IsNullOrEmpty(base.ConfigInstance.InvisibleText))
			{
				float[] invisibleTextAnchorX = base.ConfigInstance.InvisibleTextAnchorX;
				float[] invisibleTextAnchorY = base.ConfigInstance.InvisibleTextAnchorY;
				((CUI)(ref val)).CreateText(val2, "vanishui", base.ConfigInstance.InvisibleTextColor, base.ConfigInstance.InvisibleText, base.ConfigInstance.InvisibleTextSize, invisibleTextAnchorX[0], invisibleTextAnchorX[1], invisibleTextAnchorY[0], invisibleTextAnchorY[1], 0f, 0f, 0f, 0f, base.ConfigInstance.InvisibleTextAnchor, (FontTypes)1, (VerticalWrapMode)1, 0f, 0f, false, false, (string)null, (string)null, false, (string)null, (string)null, false, true, 0f, (bool?)null);
			}
			if (!string.IsNullOrEmpty(base.ConfigInstance.InvisibleIconUrl))
			{
				float[] invisibleIconMinAnchor = base.ConfigInstance.InvisibleIconMinAnchor;
				float[] invisibleIconMaxAnchor = base.ConfigInstance.InvisibleIconMaxAnchor;
				float[] invisibleIconMinOffset = base.ConfigInstance.InvisibleIconMinOffset;
				float[] invisibleIconMaxOffset = base.ConfigInstance.InvisibleIconMaxOffset;
				((CUI)(ref val)).CreateClientImage(val2, "vanishui", base.ConfigInstance.InvisibleIconUrl, base.ConfigInstance.InvisibleIconColor, (string)null, invisibleIconMinAnchor[0], invisibleIconMaxAnchor[0], invisibleIconMinAnchor[1], invisibleIconMaxAnchor[1], invisibleIconMinOffset[0], invisibleIconMaxOffset[0], invisibleIconMinOffset[1], invisibleIconMaxOffset[1], 0f, 0f, false, false, (string)null, (string)null, false, (string)null, (string)null, false, true, 0f, (bool?)null);
			}
			((CUI)(ref val)).Send(val2, player);
		}
		finally
		{
			((IDisposable)(*(CUI*)(&val))/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public override object InternalCallHook(uint hook, object[] args)
	{
		//IL_0447: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_032a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0277: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_040b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		int? num = args?.Length;
		object obj = ((num > 0) ? args[0] : null);
		object obj2 = ((num > 1) ? args[1] : null);
		object obj3 = ((num > 2) ? args[2] : null);
		try
		{
			switch (hook)
			{
			case 3161252930u:
			{
				bool flag = ((obj is BradleyAPC || obj == null) ? true : false);
				bool flag12 = flag;
				BradleyAPC apc = ((!flag12) ? ((BradleyAPC)null) : ((BradleyAPC)(obj ?? null)));
				flag = ((obj2 is BasePlayer || obj2 == null) ? true : false);
				bool flag13 = flag;
				BasePlayer player6 = ((!flag13) ? ((BasePlayer)null) : ((BasePlayer)(obj2 ?? null)));
				if (flag12 & flag13)
				{
					return CanBradleyApcTarget(apc, player6);
				}
				break;
			}
			case 1627232611u:
			{
				bool flag = ((obj is BasePlayer || obj == null) ? true : false);
				bool flag10 = flag;
				BasePlayer player5 = ((!flag10) ? ((BasePlayer)null) : ((BasePlayer)(obj ?? null)));
				flag = ((obj2 is ContainerIOEntity || obj2 == null) ? true : false);
				bool flag11 = flag;
				ContainerIOEntity container = ((!flag11) ? ((ContainerIOEntity)null) : ((ContainerIOEntity)(obj2 ?? null)));
				if (flag10 & flag11)
				{
					CanLootEntity(player5, container);
					return null;
				}
				break;
			}
			case 3615154331u:
			{
				bool flag = ((obj is BasePlayer || obj == null) ? true : false);
				bool flag3 = flag;
				BasePlayer player2 = ((!flag3) ? ((BasePlayer)null) : ((BasePlayer)(obj ?? null)));
				flag = ((obj2 is BaseLock || obj2 == null) ? true : false);
				bool flag4 = flag;
				BaseLock val = ((!flag4) ? ((BaseLock)null) : ((BaseLock)(obj2 ?? null)));
				if (flag3 & flag4)
				{
					return CanUseLockedEntity(player2, val);
				}
				break;
			}
			case 1437762689u:
			{
				bool flag = ((obj is BasePlayer || obj == null) ? true : false);
				bool flag14 = flag;
				BasePlayer player7 = ((!flag14) ? ((BasePlayer)null) : ((BasePlayer)(obj ?? null)));
				flag = ((obj2 is HitInfo || obj2 == null) ? true : false);
				bool flag15 = flag;
				HitInfo hit = ((!flag15) ? ((HitInfo)null) : ((HitInfo)(obj2 ?? null)));
				if (flag14 & flag15)
				{
					return OnPlayerAttack(player7, hit);
				}
				break;
			}
			case 3025469128u:
			{
				bool flag = ((obj is BasePlayer || obj == null) ? true : false);
				bool flag9 = flag;
				BasePlayer self = ((!flag9) ? ((BasePlayer)null) : ((BasePlayer)(obj ?? null)));
				if (flag9)
				{
					OnPlayerSleepEnded(self);
					return null;
				}
				break;
			}
			case 2358926271u:
			{
				bool flag = ((obj is BasePlayer || obj == null) ? true : false);
				bool flag6 = flag;
				BasePlayer player4 = ((!flag6) ? ((BasePlayer)null) : ((BasePlayer)(obj ?? null)));
				flag = ((obj2 is string || obj2 == null) ? true : false);
				bool flag7 = flag;
				string cmd = (flag7 ? ((string)(obj2 ?? null)) : null);
				flag = ((obj3 is string[] || obj3 == null) ? true : false);
				bool flag8 = flag;
				string[] args2 = (flag8 ? ((string[])(obj3 ?? null)) : null);
				if (flag6 & flag7 & flag8)
				{
					Vanish(player4, cmd, args2);
					return null;
				}
				break;
			}
			case 2343271809u:
			{
				bool flag = ((obj is BasePlayer || obj == null) ? true : false);
				bool flag5 = flag;
				BasePlayer player3 = ((!flag5) ? ((BasePlayer)null) : ((BasePlayer)(obj ?? null)));
				if (flag5)
				{
					_clearTriggers(player3);
					return null;
				}
				break;
			}
			case 1486685506u:
			{
				bool flag = ((obj is BasePlayer || obj == null) ? true : false);
				bool flag2 = flag;
				BasePlayer player = ((!flag2) ? ((BasePlayer)null) : ((BasePlayer)(obj ?? null)));
				if (flag2)
				{
					_drawUI(player);
					return null;
				}
				break;
			}
			}
		}
		catch (Exception ex)
		{
			Logger.Error((object)string.Format("Failed to call internal hook '{0}' on module '{1} v{2}' [{3}]", new object[4]
			{
				HookStringPool.GetOrAdd(hook),
				((CarbonModule<VanishConfig, EmptyModuleData>)this).Name,
				((BaseHookable)this).Version,
				hook
			}), ex);
			((BaseHookable)this).OnException(hook);
		}
		return null;
	}

	public VanishModule()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected O, but got Unknown
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Expected O, but got Unknown
		//IL_0031: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		//IL_0047: Expected O, but got Unknown
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Expected O, but got Unknown
		Handler = new Handler();
		_vanishedPlayers = new Dictionary<ulong, Vector3>(500);
		GameObjectRef val = new GameObjectRef();
		((ResourceRef<GameObject>)val).guid = "28ad47c8e6d313742a7a2740674a25b5";
		_drownEffect = val;
		GameObjectRef val2 = new GameObjectRef();
		((ResourceRef<GameObject>)val2).guid = "ca14ed027d5924003b1c5d9e523a5fce";
		_fallDamageEffect = val2;
		_emptyEffect = new GameObjectRef();
		base._002Ector();
	}
}
