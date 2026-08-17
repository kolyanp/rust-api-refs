using System;
using System.Collections.Generic;
using System.Text;
using ConVar;
using Development.Attributes;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using Rust;
using Rust.Safety;
using UnityEngine;
using UnityEngine.Assertions;

[ResetStaticFields]
public class SleepingBag : DecayEntity
{
	public enum BagAssignMode
	{
		Allowed = 0,
		TeamAndFriendlyContacts = 1,
		None = 2,
		LAST = None
	}

	public enum BagResultType
	{
		Ok,
		TooManyBags,
		BagBlocked,
		TargetIsPlayingTutorial
	}

	public struct CanAssignBedResult
	{
		public BagResultType Result;

		public int Count;

		public int Max;
	}

	public enum SleepingBagResetReason
	{
		Respawned,
		Placed,
		Death
	}

	[ReplicatedVar]
	public static bool UseTeamLabels;

	[NonSerialized]
	public ulong deployerUserID;

	public bool CountsTowardsBagLimit;

	public GameObject renameDialog;

	public GameObject assignDialog;

	public float secondsBetweenReuses;

	public bool perPlayerRespawnCooldown;

	private Dictionary<ulong, float> playerCooldowns;

	public string niceName;

	public Vector3 spawnOffset;

	public RespawnType RespawnType;

	public bool isStatic;

	public bool canBePublic;

	public bool canReassignToFriends;

	public bool clearHostile;

	public const Flags IsPublicFlag = Flags.Reserved3;

	public const Flags DestroyAfterUseFlag = Flags.Reserved14;

	public static Phrase bagLimitPhrase;

	public static Phrase bagLimitReachedPhrase;

	public static Phrase teammateBagPhrase;

	public Phrase assignOtherBagPhrase;

	public Phrase assignedBagPhrase;

	public Phrase cannotAssignBedNoPlayerPhrase;

	public Phrase cannotAssignBedPhrase;

	public Phrase cannotMakeBedPhrase;

	public Phrase bedAssigningBlocked;

	public static Phrase tutorialPhrase;

	public float unlockTime;

	private bool notifyPlayerOnServerInit;

	public static ListHashSet<SleepingBag> sleepingBags;

	private static Dictionary<ulong, List<SleepingBag>> bagsPerPlayer;

	public bool showOnCompass { get; private set; }

	public bool favourite { get; private set; }

	public virtual float unlockSeconds
	{
		get
		{
			if (unlockTime < Time.realtimeSinceStartup)
			{
				return 0f;
			}
			return unlockTime - Time.realtimeSinceStartup;
		}
	}

	public bool IsTutorialBag
	{
		get
		{
			if (net != null && net.group != null)
			{
				return net.group.restricted;
			}
			return false;
		}
	}

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("SleepingBag.OnRpcMessage"))
		{
			if (rpc == 3057055788u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - AssignToFriend"));
				}
				using (TimeWarning.New("AssignToFriend"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(3057055788u, "AssignToFriend", this, player, 3f))
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
							AssignToFriend(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in AssignToFriend");
					}
				}
				return true;
			}
			if (rpc == 1335950295 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - Rename"));
				}
				using (TimeWarning.New("Rename"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(1335950295u, "Rename", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg3 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							Rename(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in Rename");
					}
				}
				return true;
			}
			if (rpc == 42669546 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_MakeBed"));
				}
				using (TimeWarning.New("RPC_MakeBed"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(42669546u, "RPC_MakeBed", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg4 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_MakeBed(msg4);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in RPC_MakeBed");
					}
				}
				return true;
			}
			if (rpc == 393812086 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_MakePublic"));
				}
				using (TimeWarning.New("RPC_MakePublic"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(393812086u, "RPC_MakePublic", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg5 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_MakePublic(msg5);
						}
					}
					catch (Exception ex4)
					{
						Debug.LogException(ex4);
						player.Kick("RPC Error in RPC_MakePublic");
					}
				}
				return true;
			}
			if (rpc == 4053585860u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_ShowOnCompass"));
				}
				using (TimeWarning.New("RPC_ShowOnCompass"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.IsVisible.Test(4053585860u, "RPC_ShowOnCompass", this, player, 3f))
						{
							return true;
						}
					}
					try
					{
						using (TimeWarning.New("Call"))
						{
							RPCMessage msg6 = new RPCMessage
							{
								connection = msg.connection,
								player = player,
								read = msg.read
							};
							RPC_ShowOnCompass(msg6);
						}
					}
					catch (Exception ex5)
					{
						Debug.LogException(ex5);
						player.Kick("RPC Error in RPC_ShowOnCompass");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public bool IsPublic()
	{
		return HasFlag(Flags.Reserved3);
	}

	private float EvaluatedSecondsBetweenReuses()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Expected I4, but got Unknown
		float num = 0f;
		RespawnType respawnType = RespawnType;
		switch (respawnType - 1)
		{
		case 0:
		case 2:
			num = ConVar.Server.respawnTimeAdditionBag;
			break;
		case 1:
		case 3:
			num = ConVar.Server.respawnTimeAdditionBed;
			break;
		}
		return secondsBetweenReuses + num;
	}

	public virtual float GetUnlockSeconds(ulong playerID)
	{
		if (playerCooldowns.TryGetValue(playerID, out var value) && value > unlockTime)
		{
			return Mathf.Max(0f, value - Time.realtimeSinceStartup);
		}
		return unlockSeconds;
	}

	public virtual bool ValidForPlayer(ulong playerID, bool ignoreTimers)
	{
		object obj = Interface.CallHook("OnSleepingBagValidCheck", this, playerID, ignoreTimers);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (deployerUserID == playerID)
		{
			if (!ignoreTimers)
			{
				return unlockTime < Time.realtimeSinceStartup;
			}
			return true;
		}
		return false;
	}

	public static CanAssignBedResult? CanAssignBed(BasePlayer player, SleepingBag newBag, ulong targetPlayer, int countOffset = 1, int maxOffset = 0, SleepingBag ignore = null)
	{
		int num = ConVar.Server.max_sleeping_bags + maxOffset;
		if (player.IsInTutorial)
		{
			return null;
		}
		if (num < 0)
		{
			return null;
		}
		int num2 = countOffset;
		BasePlayer basePlayer = BasePlayer.FindByID(targetPlayer);
		BagAssignMode bagAssignMode = (BagAssignMode)Mathf.Clamp(((Object)(object)basePlayer != (Object)null) ? basePlayer.GetInfoInt("client.bagassignmode", 0) : 0, 0, 2);
		int max = num;
		if ((Object)(object)player != (Object)(object)basePlayer)
		{
			switch (bagAssignMode)
			{
			case BagAssignMode.Allowed:
				if (ConVar.Server.fogofwar || !ConVar.Server.mapenabled)
				{
					num--;
				}
				break;
			case BagAssignMode.None:
				return new CanAssignBedResult
				{
					Result = BagResultType.BagBlocked
				};
			case BagAssignMode.TeamAndFriendlyContacts:
			{
				if (!((Object)(object)basePlayer != (Object)null))
				{
					break;
				}
				bool flag = false;
				if (basePlayer.Team != null && basePlayer.Team.members.Contains(player.userID))
				{
					flag = true;
				}
				else
				{
					RelationshipManager.PlayerRelationshipInfo relations = RelationshipManager.ServerInstance.GetRelationships(targetPlayer).GetRelations(player.userID);
					if (relations != null && relations.type == RelationshipManager.RelationshipType.Friend)
					{
						flag = true;
					}
					if (!flag && (Object)(object)ClanManager.ServerInstance != (Object)null && basePlayer.clanId != 0L && basePlayer.clanId == player.clanId)
					{
						flag = true;
					}
				}
				if (!flag)
				{
					return new CanAssignBedResult
					{
						Result = BagResultType.BagBlocked
					};
				}
				break;
			}
			}
			if ((Object)(object)basePlayer != (Object)(object)player && (Object)(object)basePlayer != (Object)null && basePlayer.IsInTutorial)
			{
				return new CanAssignBedResult
				{
					Result = BagResultType.TargetIsPlayingTutorial
				};
			}
		}
		if (bagsPerPlayer.TryGetValue(targetPlayer, out var value))
		{
			foreach (SleepingBag item in value)
			{
				if ((Object)(object)item != (Object)(object)ignore && item.CountsTowardsBagLimit)
				{
					num2++;
					if (num2 > num)
					{
						return new CanAssignBedResult
						{
							Count = num2,
							Max = max,
							Result = BagResultType.TooManyBags
						};
					}
				}
			}
		}
		return new CanAssignBedResult
		{
			Count = num2,
			Max = max,
			Result = BagResultType.Ok
		};
	}

	public static Planner.CanBuildResult? CanBuildBed(BasePlayer player, Construction construction)
	{
		if (!construction.isSleepingBag)
		{
			return null;
		}
		CanAssignBedResult? canAssignBedResult = CanAssignBed(player, null, player.userID);
		if (canAssignBedResult.HasValue)
		{
			if (canAssignBedResult.Value.Result == BagResultType.Ok)
			{
				return new Planner.CanBuildResult
				{
					Result = true,
					Phrase = bagLimitPhrase,
					Arguments = new string[2]
					{
						canAssignBedResult.Value.Count.ToString(),
						canAssignBedResult.Value.Max.ToString()
					}
				};
			}
			return new Planner.CanBuildResult
			{
				Result = false,
				Phrase = bagLimitReachedPhrase
			};
		}
		return null;
	}

	public static PooledList<SleepingBag> FindForPlayer(ulong playerID, bool ignoreTimers = true)
	{
		PooledList<SleepingBag> val = Pool.Get<PooledList<SleepingBag>>();
		if (bagsPerPlayer.TryGetValue(playerID, out var value))
		{
			if (!ignoreTimers)
			{
				foreach (SleepingBag item in value)
				{
					if (item.ValidForPlayer(playerID, ignoreTimers))
					{
						((List<SleepingBag>)(object)val).Add(item);
					}
				}
			}
			else
			{
				foreach (SleepingBag item2 in value)
				{
					((List<SleepingBag>)(object)val).Add(item2);
				}
			}
		}
		if (!ignoreTimers)
		{
			foreach (StaticRespawnArea staticRespawnArea in StaticRespawnArea.staticRespawnAreas)
			{
				if (staticRespawnArea.ValidForPlayer(playerID, ignoreTimers))
				{
					((List<SleepingBag>)(object)val).Add((SleepingBag)staticRespawnArea);
				}
			}
		}
		else
		{
			foreach (StaticRespawnArea staticRespawnArea2 in StaticRespawnArea.staticRespawnAreas)
			{
				((List<SleepingBag>)(object)val).Add((SleepingBag)staticRespawnArea2);
			}
		}
		return val;
	}

	[PoolAnalyzerNonCaching]
	public static void FindForPlayer(ulong playerID, bool ignoreTimers, List<SleepingBag> result)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		Enumerator<SleepingBag> enumerator = sleepingBags.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				SleepingBag current = enumerator.Current;
				if (current.ValidForPlayer(playerID, ignoreTimers))
				{
					result.Add(current);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
	}

	public static void UpdateMyBags(ulong id)
	{
		if (!bagsPerPlayer.TryGetValue(id, out var value))
		{
			return;
		}
		foreach (SleepingBag item in value)
		{
			if (!((Object)(object)item == (Object)null))
			{
				item.SendNetworkUpdate();
			}
		}
	}

	[PoolAnalyzerNonCaching]
	public static void UpdateTeamsBags(List<ulong> ids)
	{
		foreach (ulong id in ids)
		{
			UpdateMyBags(id);
		}
	}

	public static bool SpawnPlayer(BasePlayer player, NetworkableId sleepingBag)
	{
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Invalid comparison between Unknown and I4
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		PooledList<SleepingBag> val = FindForPlayer(player.userID);
		try
		{
			SleepingBag sleepingBag2 = null;
			foreach (SleepingBag item in (List<SleepingBag>)(object)val)
			{
				if (item.ValidForPlayer(player.userID, ignoreTimers: false) && item.net.ID == sleepingBag && item.unlockTime < Time.realtimeSinceStartup)
				{
					sleepingBag2 = item;
					break;
				}
			}
			if ((Object)(object)sleepingBag2 == (Object)null)
			{
				return false;
			}
			object obj = Interface.CallHook("OnPlayerRespawn", player, sleepingBag2);
			if (obj is SleepingBag)
			{
				sleepingBag2 = (SleepingBag)obj;
			}
			if (sleepingBag2 is StaticRespawnArea staticRespawnArea && !staticRespawnArea.IsAuthed(player.userID))
			{
				return false;
			}
			if ((int)sleepingBag2.GetRespawnState(player.userID) != 1)
			{
				return false;
			}
			sleepingBag2.GetSpawnPos(out var pos, out var rot);
			player.RespawnAt(pos, rot, sleepingBag2);
			sleepingBag2.PostPlayerSpawn(player);
			foreach (SleepingBag item2 in (List<SleepingBag>)(object)val)
			{
				SetBagTimer(item2, pos, SleepingBagResetReason.Respawned, player);
			}
			if (sleepingBag2.HasFlag(Flags.Reserved14))
			{
				sleepingBag2.Kill();
			}
			if (sleepingBag2.clearHostile)
			{
				player.SetHostileDuration(0f);
			}
			return true;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static void AddBagForPlayer(SleepingBag bag, ulong user, bool networkUpdate = true)
	{
		if (user == 0L)
		{
			return;
		}
		if (!bagsPerPlayer.TryGetValue(user, out var value))
		{
			value = new List<SleepingBag>();
			bagsPerPlayer[user] = value;
		}
		if (!value.Contains(bag))
		{
			value.Add(bag);
			if (networkUpdate)
			{
				RelationshipManager.FindByID(user)?.SendNetworkUpdate();
			}
		}
	}

	public static void RemoveBagForPlayer(SleepingBag bag, ulong user)
	{
		if (user != 0L && bagsPerPlayer.TryGetValue(user, out var value))
		{
			if (value.Remove(bag))
			{
				RelationshipManager.FindByID(user)?.SendNetworkUpdate();
			}
			if (value.Count == 0)
			{
				bagsPerPlayer.Remove(user);
			}
		}
	}

	public static void OnBagChangedOwnership(SleepingBag bag, ulong oldUser)
	{
		if (bag.deployerUserID != oldUser)
		{
			RemoveBagForPlayer(bag, oldUser);
			AddBagForPlayer(bag, bag.deployerUserID);
		}
	}

	public static void ClearTutorialBagsForPlayer(ulong userId)
	{
		if (userId == 0L || !bagsPerPlayer.TryGetValue(userId, out var _))
		{
			return;
		}
		List<SleepingBag> list = Pool.Get<List<SleepingBag>>();
		foreach (SleepingBag item in bagsPerPlayer[userId])
		{
			if (item.net != null && item.net.group != null && item.net.group.restricted)
			{
				list.Add(item);
			}
		}
		foreach (SleepingBag item2 in list)
		{
			item2.deployerUserID = 0uL;
			RemoveBagForPlayer(item2, userId);
		}
		Pool.FreeUnmanaged<SleepingBag>(ref list);
	}

	public static int GetSleepingBagCount(ulong userId)
	{
		if (userId == 0L)
		{
			return 0;
		}
		if (!bagsPerPlayer.TryGetValue(userId, out var value))
		{
			return 0;
		}
		int num = value.Count;
		foreach (SleepingBag item in value)
		{
			if (!item.CountsTowardsBagLimit)
			{
				num--;
			}
		}
		return num;
	}

	public static bool TrySpawnPlayer(BasePlayer player, NetworkableId sleepingBag, out string errorMessage)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (!player.IsDead())
		{
			errorMessage = "Couldn't spawn - player is not dead!";
			return false;
		}
		if (player.CanRespawn())
		{
			if (SpawnPlayer(player, sleepingBag))
			{
				player.MarkRespawn();
				errorMessage = null;
				return true;
			}
			errorMessage = "Couldn't spawn in sleeping bag!";
			return false;
		}
		errorMessage = "You can't respawn again so quickly, wait a while";
		return false;
	}

	public virtual void SetUnlockTime(float newTime)
	{
		unlockTime = newTime;
	}

	public void SetUnlockTimeForPlayer(ulong player, float time)
	{
		playerCooldowns.TryGetValue(player, out var value);
		playerCooldowns[player] = Mathf.Max(value, time);
	}

	public void ResetUnlockTimeForPlayer(ulong player)
	{
		playerCooldowns.Remove(player);
	}

	public static bool DestroyBag(ulong userID, NetworkableId sleepingBag)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		PooledList<SleepingBag> val = FindForPlayer(userID);
		bool result;
		try
		{
			SleepingBag sleepingBag2 = null;
			foreach (SleepingBag item in (List<SleepingBag>)(object)val)
			{
				if (item.net.ID == sleepingBag)
				{
					sleepingBag2 = item;
					break;
				}
			}
			if ((Object)(object)sleepingBag2 == (Object)null)
			{
				result = false;
			}
			else
			{
				if (Interface.CallHook("OnSleepingBagDestroy", sleepingBag2, userID) != null)
				{
					result = false;
					/*Error near IL_0077: Could not find block for branch target IL_00e8*/;
				}
				RemoveBagForPlayer(sleepingBag2, sleepingBag2.deployerUserID);
				sleepingBag2.deployerUserID = 0uL;
				if (sleepingBag2.HasFlag(Flags.Reserved14))
				{
					sleepingBag2.Kill();
				}
				else
				{
					sleepingBag2.SendNetworkUpdate();
				}
				BasePlayer basePlayer = BasePlayer.FindByID(userID);
				if ((Object)(object)basePlayer != (Object)null)
				{
					basePlayer.SendRespawnOptions();
					Interface.CallHook("OnSleepingBagDestroyed", sleepingBag2, userID);
					Facepunch.Rust.Analytics.Azure.OnBagUnclaimed(basePlayer, sleepingBag2);
				}
				result = true;
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		return result;
	}

	public static void ResetTimersForPlayer(BasePlayer player)
	{
		PooledList<SleepingBag> val = FindForPlayer(player.userID);
		try
		{
			foreach (SleepingBag item in (List<SleepingBag>)(object)val)
			{
				item.unlockTime = 0f;
				item.ResetUnlockTimeForPlayer(player.userID);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public virtual void GetSpawnPos(out Vector3 pos, out Quaternion rot)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		pos = ((Component)this).transform.position + spawnOffset;
		Quaternion rotation = ((Component)this).transform.rotation;
		rot = Quaternion.Euler(0f, ((Quaternion)(ref rotation)).eulerAngles.y, 0f);
	}

	public void SetPublic(bool isPublic)
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved3, isPublic);
	}

	public void SetDeployedBy(BasePlayer player)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)player == (Object)null) && !base.isClient)
		{
			deployerUserID = player.userID;
			SetBagTimer(this, ((Component)this).transform.position, SleepingBagResetReason.Placed, player);
			SendNetworkUpdate();
			notifyPlayerOnServerInit = true;
		}
	}

	public static void OnPlayerDeath(BasePlayer player)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		PooledList<SleepingBag> val = FindForPlayer(player.userID);
		try
		{
			foreach (SleepingBag item in (List<SleepingBag>)(object)val)
			{
				SetBagTimer(item, ((Component)player).transform.position, SleepingBagResetReason.Death, player);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public static void SetBagTimer(SleepingBag bag, Vector3 position, SleepingBagResetReason reason, BasePlayer forPlayer)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(serverside: true);
		float? num = null;
		if ((Object)(object)activeGameMode != (Object)null)
		{
			num = activeGameMode.EvaluateSleepingBagReset(bag, position, reason);
		}
		if (num.HasValue)
		{
			bag.SetUnlockTime(Time.realtimeSinceStartup + num.Value);
			return;
		}
		if (reason == SleepingBagResetReason.Respawned && Vector3.Distance(position, ((Component)bag).transform.position) <= ConVar.Server.respawnresetrange)
		{
			if (bag.perPlayerRespawnCooldown)
			{
				bag.SetUnlockTimeForPlayer(forPlayer.userID, Time.realtimeSinceStartup + bag.EvaluatedSecondsBetweenReuses());
			}
			else
			{
				bag.SetUnlockTime(Time.realtimeSinceStartup + bag.EvaluatedSecondsBetweenReuses());
			}
			bag.SendNetworkUpdate();
		}
		if (reason != SleepingBagResetReason.Placed)
		{
			return;
		}
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		float realtimeSinceStartup2 = Time.realtimeSinceStartup;
		if (bagsPerPlayer.TryGetValue(bag.deployerUserID, out var value) && bag.deployerUserID != 0L)
		{
			foreach (SleepingBag item in value)
			{
				if (!(item.unlockTime < realtimeSinceStartup2) && item.unlockTime > realtimeSinceStartup && Vector3.Distance(((Component)item).transform.position, position) <= ConVar.Server.respawnresetrange)
				{
					realtimeSinceStartup = bag.unlockTime;
				}
			}
		}
		float num2 = Mathf.Max(realtimeSinceStartup, Time.realtimeSinceStartup + bag.EvaluatedSecondsBetweenReuses());
		if ((Object)(object)forPlayer != (Object)null && forPlayer.IsInTutorial)
		{
			num2 = 0f;
		}
		bag.SetUnlockTime(num2);
		bag.SendNetworkUpdate();
	}

	public override void ServerInit()
	{
		base.ServerInit();
		if (!sleepingBags.Contains(this))
		{
			sleepingBags.Add(this);
			if (deployerUserID != 0L)
			{
				AddBagForPlayer(this, deployerUserID, !Application.isLoadingSave);
			}
		}
		if (notifyPlayerOnServerInit)
		{
			notifyPlayerOnServerInit = false;
			NotifyPlayer(deployerUserID);
		}
	}

	public override void OnPlaced(BasePlayer player)
	{
		SetDeployedBy(player);
	}

	public override void OnParentChanging(BaseEntity oldParent, BaseEntity newParent)
	{
		base.OnParentChanging(oldParent, newParent);
		Invoke(DelayedPlayerNotify, 0.1f);
	}

	private void DelayedPlayerNotify()
	{
		NotifyPlayer(deployerUserID);
	}

	public override void PostServerLoad()
	{
		base.PostServerLoad();
		AddBagForPlayer(this, deployerUserID, !Application.isLoadingSave);
	}

	private void NotifyPlayer(ulong id)
	{
		if (id != 0L)
		{
			BasePlayer basePlayer = BasePlayer.FindByID(id);
			if ((Object)(object)basePlayer != (Object)null && basePlayer.IsConnected)
			{
				basePlayer.SendRespawnOptions();
			}
		}
	}

	public override void DoServerDestroy()
	{
		base.DoServerDestroy();
		sleepingBags.Remove(this);
		RemoveBagForPlayer(this, deployerUserID);
		NotifyPlayer(deployerUserID);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		using (TimeWarning.New("SleepingBag.Save"))
		{
			info.msg.sleepingBag = Pool.Get<SleepingBag>();
			info.msg.sleepingBag.name = niceName;
			if (info.forDisk)
			{
				info.msg.sleepingBag.deployerID = deployerUserID;
				info.msg.sleepingBag.showOnCompass = showOnCompass;
				info.msg.sleepingBag.favourite = favourite;
			}
			else
			{
				info.msg.sleepingBag.clientAssigned = deployerUserID == info.forConnection.userid;
				info.msg.sleepingBag.isAssigned = deployerUserID != 0;
				info.msg.sleepingBag.showOnCompass = showOnCompass && deployerUserID == info.forConnection.userid;
			}
			if (!UseTeamLabels)
			{
				return;
			}
			if ((Object)(object)RelationshipManager.ServerInstance != (Object)null && info.forConnection != null)
			{
				try
				{
					RelationshipManager.PlayerTeam playerTeam = RelationshipManager.ServerInstance.FindPlayersTeam(deployerUserID);
					BasePlayer basePlayer = BasePlayer.FindAwakeOrSleepingByID(deployerUserID);
					string teamMemberName = (((Object)(object)basePlayer != (Object)null) ? basePlayer.displayName : "");
					if (deployerUserID == info.forConnection.userid)
					{
						info.msg.sleepingBag.teamMemberName = teamMemberName;
						info.msg.sleepingBag.teamMemberId = deployerUserID;
					}
					else if (playerTeam != null && playerTeam.members.Contains(info.forConnection.userid))
					{
						info.msg.sleepingBag.teamMemberName = teamMemberName;
						info.msg.sleepingBag.teamMemberId = deployerUserID;
					}
					return;
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
					return;
				}
			}
			info.msg.sleepingBag.teamMemberName = "";
		}
	}

	public override bool CanUseNetworkCache(Connection connection)
	{
		return false;
	}

	public override bool ShouldUseCastNoClipChecks()
	{
		return true;
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public void Rename(RPCMessage msg)
	{
		if (!msg.player.CanInteract())
		{
			return;
		}
		string text = msg.read.String();
		if (Interface.CallHook("CanRenameBed", msg.player, this, text) == null)
		{
			text = WordFilter.Filter(text);
			if (string.IsNullOrEmpty(text))
			{
				text = "Unnamed Sleeping Bag";
			}
			if (text.Length > 24)
			{
				text = text.Substring(0, 22) + "..";
			}
			niceName = text;
			SendNetworkUpdate();
			NotifyPlayer(deployerUserID);
		}
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void AssignToFriend(RPCMessage msg)
	{
		if (!msg.player.CanInteract() || deployerUserID != (ulong)msg.player.userID || !canReassignToFriends)
		{
			return;
		}
		ulong num = msg.read.UInt64();
		if (num == 0L || Interface.CallHook("CanAssignBed", msg.player, this, num) != null)
		{
			return;
		}
		if (ConVar.Server.max_sleeping_bags > 0)
		{
			CanAssignBedResult? canAssignBedResult = CanAssignBed(msg.player, this, num);
			if (canAssignBedResult.HasValue)
			{
				BasePlayer basePlayer = RelationshipManager.FindByID(num);
				if (canAssignBedResult.Value.Result == BagResultType.TooManyBags)
				{
					if ((Object)(object)basePlayer == (Object)null)
					{
						msg.player.ShowToast(GameTip.Styles.Error, cannotAssignBedNoPlayerPhrase, false);
					}
					else
					{
						string playerNameStreamSafe = NameHelper.GetPlayerNameStreamSafe(msg.player, basePlayer);
						msg.player.ShowToast(GameTip.Styles.Error, cannotAssignBedPhrase, false, playerNameStreamSafe);
					}
				}
				else if (canAssignBedResult.Value.Result == BagResultType.BagBlocked)
				{
					msg.player.ShowToast(GameTip.Styles.Error, bedAssigningBlocked, false);
				}
				else if (canAssignBedResult.Value.Result == BagResultType.TargetIsPlayingTutorial)
				{
					msg.player.ShowToast(GameTip.Styles.Error, tutorialPhrase, false);
				}
				else
				{
					basePlayer?.ShowToast(GameTip.Styles.Blue_Long, assignedBagPhrase, false, canAssignBedResult.Value.Count.ToString(), canAssignBedResult.Value.Max.ToString());
					msg.player.ShowToast(GameTip.Styles.Blue_Long, bagLimitPhrase, false, (GetSleepingBagCount(msg.player.userID) - 1).ToString(), canAssignBedResult.Value.Max.ToString());
					SendNetworkUpdate();
				}
				if (canAssignBedResult.Value.Result != BagResultType.Ok)
				{
					return;
				}
			}
		}
		AssignToUser(num);
		Facepunch.Rust.Analytics.Azure.OnSleepingBagAssigned(msg.player, this, num);
		SendNetworkUpdate();
	}

	[ServerVar(Help = "(Generated) Reassigns ownership of a sleeping bag (by entity ID) to the calling player; notifies both the old and new owner")]
	public static void AssignToPlayer(ConsoleSystem.Arg arg)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		NetworkableId entityID = ArgEx.GetEntityID(arg, 0);
		(BaseNetworkable.serverEntities.Find(entityID) as SleepingBag).AssignToUser(basePlayer.userID.Get());
	}

	[ServerVar(Help = "(Generated) Clears ownership of a sleeping bag (by entity ID), setting the owner ID to 0 and removing it from the old owner's bag list")]
	public static void ClearFromPlayer(ConsoleSystem.Arg arg)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		NetworkableId entityID = ArgEx.GetEntityID(arg, 0);
		(BaseNetworkable.serverEntities.Find(entityID) as SleepingBag).AssignToUser(0uL);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	public virtual void RPC_MakePublic(RPCMessage msg)
	{
		if (!canBePublic || !msg.player.CanInteract() || (deployerUserID != (ulong)msg.player.userID && !msg.player.CanBuild()))
		{
			return;
		}
		bool flag = msg.read.Bit();
		if (flag == IsPublic() || Interface.CallHook("CanSetBedPublic", msg.player, this) != null)
		{
			return;
		}
		SetPublic(flag);
		if (!IsPublic())
		{
			if (ConVar.Server.max_sleeping_bags > 0)
			{
				CanAssignBedResult? canAssignBedResult = CanAssignBed(msg.player, this, msg.player.userID, 1, 0, this);
				if (canAssignBedResult.HasValue)
				{
					if (canAssignBedResult.Value.Result == BagResultType.Ok)
					{
						msg.player.ShowToast(GameTip.Styles.Blue_Long, bagLimitPhrase, false, canAssignBedResult.Value.Count.ToString(), canAssignBedResult.Value.Max.ToString());
					}
					else
					{
						msg.player.ShowToast(GameTip.Styles.Blue_Long, cannotMakeBedPhrase, false, canAssignBedResult.Value.Count.ToString(), canAssignBedResult.Value.Max.ToString());
					}
					if (canAssignBedResult.Value.Result != BagResultType.Ok)
					{
						return;
					}
				}
			}
			AssignToUser(msg.player.userID, sendNetworkUpdate: false);
			Facepunch.Rust.Analytics.Azure.OnSleepingBagAssigned(msg.player, this, deployerUserID);
		}
		else
		{
			Facepunch.Rust.Analytics.Azure.OnSleepingBagAssigned(msg.player, this, 0uL);
		}
		SendNetworkUpdate();
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void RPC_MakeBed(RPCMessage msg)
	{
		if (!canBePublic || !IsPublic() || !msg.player.CanInteract())
		{
			return;
		}
		if (ConVar.Server.max_sleeping_bags > 0)
		{
			CanAssignBedResult? canAssignBedResult = CanAssignBed(msg.player, this, msg.player.userID, 1, 0, this);
			if (canAssignBedResult.HasValue)
			{
				if (canAssignBedResult.Value.Result != BagResultType.Ok)
				{
					msg.player.ShowToast(GameTip.Styles.Red_Normal, cannotMakeBedPhrase, false);
				}
				else
				{
					msg.player.ShowToast(GameTip.Styles.Blue_Long, bagLimitPhrase, false, canAssignBedResult.Value.Count.ToString(), canAssignBedResult.Value.Max.ToString());
				}
				if (canAssignBedResult.Value.Result != BagResultType.Ok)
				{
					return;
				}
			}
		}
		AssignToUser(msg.player.userID);
		Interface.CallHook("OnBedMade", this, msg.player);
	}

	public void AssignToUser(ulong user, bool sendNetworkUpdate = true)
	{
		ulong num = deployerUserID;
		deployerUserID = user;
		showOnCompass = false;
		favourite = false;
		OnBagChangedOwnership(this, num);
		NotifyPlayer(num);
		NotifyPlayer(deployerUserID);
		if (sendNetworkUpdate)
		{
			SendNetworkUpdate();
		}
	}

	public void SetShownOnCompass(bool show)
	{
		if (show == showOnCompass)
		{
			return;
		}
		if (show && deployerUserID != 0L && bagsPerPlayer.TryGetValue(deployerUserID, out var value))
		{
			foreach (SleepingBag item in value)
			{
				if (!((Object)(object)item == (Object)null) && !((Object)(object)item == (Object)(object)this) && item.showOnCompass)
				{
					item.showOnCompass = false;
					item.SendNetworkUpdate();
				}
			}
		}
		showOnCompass = show;
		SendNetworkUpdate();
		NotifyPlayer(deployerUserID);
	}

	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void RPC_ShowOnCompass(RPCMessage msg)
	{
		if (msg.player.CanInteract() && deployerUserID == (ulong)msg.player.userID)
		{
			bool shownOnCompass = msg.read.Bit();
			SetShownOnCompass(shownOnCompass);
		}
	}

	public void SetFavourite(bool value)
	{
		if (value != favourite)
		{
			favourite = value;
		}
	}

	public static bool SetBagFavourite(ulong userID, NetworkableId sleepingBag, bool favourite)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		PooledList<SleepingBag> val = FindForPlayer(userID);
		try
		{
			foreach (SleepingBag item in (List<SleepingBag>)(object)val)
			{
				if (item.net.ID == sleepingBag && item.deployerUserID == userID)
				{
					item.SetFavourite(favourite);
					BasePlayer.FindByID(userID)?.SendRespawnOptions();
					return true;
				}
			}
			return false;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	protected virtual void PostPlayerSpawn(BasePlayer p)
	{
		p.SendRespawnOptions();
		if (Debugging.bag_respawn_parenting && HasParent())
		{
			BaseEntity baseEntity = GetParentEntity();
			if (baseEntity is PlayerBoat)
			{
				p.SetParent(baseEntity, worldPositionStays: true);
			}
		}
	}

	public virtual RespawnState GetRespawnState(ulong userID)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		if (!WaterLevel.Test(((Component)this).transform.position, waves: true, volumes: false))
		{
			if (!TriggerNoRespawnZone.InAnyNoRespawnZone(((Component)this).transform.position))
			{
				return (RespawnState)1;
			}
			return (RespawnState)4;
		}
		return (RespawnState)3;
	}

	public virtual bool IsMobile()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Invalid comparison between Unknown and I4
		BaseEntity baseEntity = GetParentEntity();
		if ((Object)(object)baseEntity != (Object)null && (baseEntity is BaseVehicle || baseEntity is BoatBuildingBlock))
		{
			return true;
		}
		return (int)RespawnType == 4;
	}

	public override string Admin_Who()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine(base.Admin_Who());
		stringBuilder.AppendLine($"Assigned bag ID: {deployerUserID}");
		stringBuilder.AppendLine("Assigned player name: " + Admin.GetPlayerName(deployerUserID));
		stringBuilder.AppendLine("Bag Name:" + niceName);
		return stringBuilder.ToString();
	}

	public override void OnDeployableCorpseSpawned(BaseEntity corpse)
	{
		base.OnDeployableCorpseSpawned(corpse);
		if (corpse is SleepingBag sleepingBag)
		{
			sleepingBag.deployerUserID = deployerUserID;
			sleepingBag.niceName = niceName;
			using (FlagsUpdateScope flagsUpdateScope = sleepingBag.StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Reserved14, b: true);
			}
			AddBagForPlayer(sleepingBag, deployerUserID);
		}
	}

	public override bool ShouldDropDeployableCorpse(HitInfo info)
	{
		if (!base.ShouldDropDeployableCorpse(info))
		{
			return false;
		}
		if (HasFlag(Flags.Reserved14))
		{
			return false;
		}
		return true;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.sleepingBag != null)
		{
			niceName = info.msg.sleepingBag.name;
			if (base.isServer)
			{
				deployerUserID = info.msg.sleepingBag.deployerID;
				showOnCompass = info.msg.sleepingBag.showOnCompass;
				favourite = info.msg.sleepingBag.favourite;
			}
		}
	}

	protected override bool ShouldDisplayPickupOption(BasePlayer player)
	{
		if (base.ShouldDisplayPickupOption(player) && Check.IsAuthorisedToBuild(player))
		{
			return true;
		}
		if (base.ShouldDisplayPickupOption(player))
		{
			return (ulong)player.userID == deployerUserID;
		}
		return false;
	}

	public SleepingBag()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected O, but got Unknown
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Expected O, but got Unknown
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Expected O, but got Unknown
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Expected O, but got Unknown
		CountsTowardsBagLimit = true;
		secondsBetweenReuses = 300f;
		playerCooldowns = new Dictionary<ulong, float>();
		niceName = "Unnamed Bag";
		spawnOffset = Vector3.zero;
		RespawnType = (RespawnType)1;
		canReassignToFriends = true;
		assignOtherBagPhrase = new Phrase("assigned_other_bag_limit", "You have assigned {0} a bag, they are now at {0}/{1} bags");
		assignedBagPhrase = new Phrase("assigned_bag_limit", "You have been assigned a bag, you are now at {0}/{1} bags");
		cannotAssignBedNoPlayerPhrase = new Phrase("cannot_assign_bag_limit_noplayer", "You cannot assign a bag to this player, they have reached their bag limit!");
		cannotAssignBedPhrase = new Phrase("cannot_assign_bag_limit", "You cannot assign {0} a bag, they have reached their bag limit!");
		cannotMakeBedPhrase = new Phrase("cannot_make_bed_limit", "You cannot take ownership of the bed, you are at your bag limit");
		bedAssigningBlocked = new Phrase("bag_assign_blocked", "That player has blocked bag assignment");
		base._002Ector();
	}

	static SleepingBag()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Expected O, but got Unknown
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Expected O, but got Unknown
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		UseTeamLabels = true;
		bagLimitPhrase = new Phrase("bag_limit_update", "You are now at {0}/{1} bags");
		bagLimitReachedPhrase = new Phrase("bag_limit_reached", "You have reached your bag limit!");
		teammateBagPhrase = new Phrase("teammate_bag", "{0}'s bag");
		tutorialPhrase = new Phrase("bag_assign_tutorial", "Cannot assign bags to players mid-tutorial");
		sleepingBags = new ListHashSet<SleepingBag>();
		bagsPerPlayer = new Dictionary<ulong, List<SleepingBag>>();
	}
}
