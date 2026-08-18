using System;
using System.Collections.Generic;
using System.Linq;
using CompanionServer;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
using UnityEngine;
using UnityEngine.Assertions;

public class RelationshipManager : BaseEntity
{
	public enum RelationshipType
	{
		NONE,
		Acquaintance,
		Friend,
		Enemy
	}

	public class PlayerRelationshipInfo : IPooled, IServerFileReceiver, IPlayerInfo
	{
		public string displayName;

		public ulong player;

		public RelationshipType type;

		public int weight;

		public uint mugshotCrc;

		public string notes;

		public float lastSeenTime;

		[NonSerialized]
		public float lastMugshotTime;

		[NonSerialized]
		public bool awaitingMugshot;

		public ulong UserId => player;

		public string UserName => displayName;

		public bool IsOnline => false;

		public bool IsMe => false;

		public bool IsFriend => false;

		public bool IsPlayingThisGame => true;

		public string ServerEndpoint => string.Empty;

		public void EnterPool()
		{
			Reset();
		}

		public void LeavePool()
		{
			Reset();
		}

		private void Reset()
		{
			displayName = null;
			player = 0uL;
			type = RelationshipType.NONE;
			weight = 0;
			mugshotCrc = 0u;
			notes = "";
			lastMugshotTime = 0f;
			awaitingMugshot = false;
		}

		public PlayerRelationshipInfo ToProto()
		{
			PlayerRelationshipInfo obj = Pool.Get<PlayerRelationshipInfo>();
			obj.playerID = player;
			obj.type = (int)type;
			obj.weight = weight;
			obj.mugshotCrc = mugshotCrc;
			obj.displayName = displayName;
			obj.notes = notes;
			obj.timeSinceSeen = Time.realtimeSinceStartup - lastSeenTime;
			return obj;
		}

		public static PlayerRelationshipInfo FromProto(PlayerRelationshipInfo proto)
		{
			return new PlayerRelationshipInfo
			{
				type = (RelationshipType)proto.type,
				weight = proto.weight,
				displayName = proto.displayName,
				mugshotCrc = proto.mugshotCrc,
				notes = proto.notes,
				player = proto.playerID,
				lastSeenTime = Time.realtimeSinceStartup - proto.timeSinceSeen
			};
		}
	}

	public class PlayerRelationships : IPooled
	{
		public bool dirty;

		public ulong ownerPlayer;

		public Dictionary<ulong, PlayerRelationshipInfo> relations;

		public bool Forget(ulong player)
		{
			if (relations.TryGetValue(player, out var value))
			{
				relations.Remove(player);
				if (value.mugshotCrc != 0)
				{
					ServerInstance.DeleteMugshot(ownerPlayer, player, value.mugshotCrc);
				}
				Pool.Free<PlayerRelationshipInfo>(ref value);
				return true;
			}
			return false;
		}

		public PlayerRelationshipInfo GetRelations(ulong player)
		{
			BasePlayer basePlayer = FindByID(player);
			if (relations.TryGetValue(player, out var value))
			{
				if ((Object)(object)basePlayer != (Object)null)
				{
					value.displayName = basePlayer.displayName;
				}
				return value;
			}
			PlayerRelationshipInfo playerRelationshipInfo = Pool.Get<PlayerRelationshipInfo>();
			if ((Object)(object)basePlayer != (Object)null)
			{
				playerRelationshipInfo.displayName = basePlayer.displayName;
			}
			playerRelationshipInfo.player = player;
			relations.Add(player, playerRelationshipInfo);
			return playerRelationshipInfo;
		}

		public PlayerRelationships()
		{
			LeavePool();
		}

		public void EnterPool()
		{
			ownerPlayer = 0uL;
			if (relations != null)
			{
				Pool.Free<ulong, PlayerRelationshipInfo>(ref relations, true);
			}
		}

		public void LeavePool()
		{
			ownerPlayer = 0uL;
			relations = Pool.Get<Dictionary<ulong, PlayerRelationshipInfo>>();
		}

		public void ClearRelations()
		{
			foreach (KeyValuePair<ulong, PlayerRelationshipInfo> relation in relations)
			{
				var (_, playerRelationshipInfo2) = relation;
				Pool.Free<PlayerRelationshipInfo>(ref playerRelationshipInfo2);
			}
			relations.Clear();
		}
	}

	public class PlayerTeam : IPooled
	{
		public ulong teamID;

		public string joinKey;

		public string teamName;

		public ulong teamLeader;

		public Vector3 firstSpawnLocation;

		public bool usePartySpawn;

		public List<ulong> members = new List<ulong>();

		public List<ulong> invites = new List<ulong>();

		public float teamStartTime;

		public List<Network.Connection> onlineMemberConnections = new List<Network.Connection>();

		public float teamLifetime => Time.realtimeSinceStartup - teamStartTime;

		public BasePlayer GetLeader()
		{
			return FindByID(teamLeader);
		}

		public void SendInvite(BasePlayer player)
		{
			if (invites.Count > 8)
			{
				invites.RemoveRange(0, 1);
			}
			BasePlayer basePlayer = FindByID(teamLeader);
			if (!((Object)(object)basePlayer == (Object)null))
			{
				ulong item = player.userID.Get();
				if (!invites.Contains(item))
				{
					invites.Add(item);
				}
				player.ClientRPC(RpcTarget.Player("CLIENT_PendingInvite", player), basePlayer.displayName, teamLeader, teamID);
			}
		}

		public void SendInvite(BasePlayer player, ulong id)
		{
			if (invites.Count > 8)
			{
				invites.RemoveRange(0, 1);
			}
			BasePlayer basePlayer = FindByID(teamLeader);
			if (!((Object)(object)basePlayer == (Object)null))
			{
				if (!invites.Contains(id))
				{
					invites.Add(id);
				}
				if ((Object)(object)player != (Object)null)
				{
					player.ClientRPC(RpcTarget.Player("CLIENT_PendingInvite", player), basePlayer.displayName, teamLeader, teamID);
				}
			}
		}

		public void AcceptInvite(BasePlayer player)
		{
			if (invites.Contains(player.userID))
			{
				invites.Remove(player.userID);
				AddPlayer(player);
				player.ClearPendingInvite();
			}
		}

		public void RejectInvite(BasePlayer player)
		{
			player.ClearPendingInvite();
			invites.Remove(player.userID);
		}

		public bool AddPlayer(BasePlayer player, bool skipDirtyUpdate = false)
		{
			if (player.currentTeam != 0L)
			{
				return false;
			}
			if (!AddPlayer(player.userID, skipDirtyUpdate))
			{
				return false;
			}
			player.currentTeam = teamID;
			player.SendNetworkUpdate();
			if (SleepingBag.UseTeamLabels)
			{
				using (TimeWarning.New("RelationshipManager.AddPLayer.TeamLabels"))
				{
					SleepingBag.UpdateTeamsBags(members);
				}
			}
			return true;
		}

		public bool AddPlayer(ulong playerId, bool skipDirtyUpdate = false)
		{
			if (members.Contains(playerId))
			{
				return false;
			}
			if (members.Count >= maxTeamSize)
			{
				return false;
			}
			bool num = members.Count == 0;
			members.Add(playerId);
			ServerInstance.playerToTeam.Add(playerId, this);
			if (!skipDirtyUpdate)
			{
				MarkDirty();
			}
			if (!num)
			{
				Facepunch.Rust.Analytics.Azure.OnTeamChanged("added", teamID, teamLeader, playerId, members);
			}
			ApartmentBuilding.OnTeamMembershipChanged(members, 0uL);
			return true;
		}

		public bool RemovePlayer(ulong playerID)
		{
			if (members.Contains(playerID))
			{
				members.Remove(playerID);
				ServerInstance.playerToTeam.Remove(playerID);
				BasePlayer basePlayer = FindByID(playerID);
				if ((Object)(object)basePlayer != (Object)null)
				{
					basePlayer.ClearTeam();
					basePlayer.BroadcastAppTeamRemoval();
					basePlayer.SendNetworkUpdate();
					if (SleepingBag.UseTeamLabels)
					{
						using (TimeWarning.New("RelationshipManager.RemovePlayer.TeamLabels"))
						{
							SleepingBag.UpdateMyBags(basePlayer.userID.Get());
							SleepingBag.UpdateTeamsBags(members);
						}
					}
				}
				ApartmentBuilding.OnTeamMembershipChanged(members, playerID);
				if (teamLeader == playerID)
				{
					if (members.Count > 0)
					{
						SetTeamLeader(members[0]);
						Facepunch.Rust.Analytics.Azure.OnTeamChanged("removed", teamID, teamLeader, playerID, members);
					}
					else
					{
						Facepunch.Rust.Analytics.Azure.OnTeamChanged("disband", teamID, teamLeader, playerID, members);
						Disband();
					}
				}
				else
				{
					Facepunch.Rust.Analytics.Azure.OnTeamChanged("left", teamID, teamLeader, playerID, members);
				}
				MarkDirty();
				return true;
			}
			return false;
		}

		public void SetTeamLeader(ulong newTeamLeader)
		{
			if (Interface.CallHook("OnTeamMemberPromote", this, newTeamLeader) == null)
			{
				Facepunch.Rust.Analytics.Azure.OnTeamChanged("promoted", teamID, teamLeader, newTeamLeader, members);
				teamLeader = newTeamLeader;
				MarkDirty();
			}
		}

		public void Disband()
		{
			ServerInstance.DisbandTeam(this);
			CompanionServer.Server.TeamChat.Remove(teamID);
		}

		public void MarkDirty()
		{
			foreach (ulong member in members)
			{
				BasePlayer basePlayer = FindByID(member);
				if ((Object)(object)basePlayer != (Object)null)
				{
					basePlayer.UpdateTeam(teamID);
				}
			}
			this.BroadcastAppTeamUpdate();
		}

		public List<Network.Connection> GetOnlineMemberConnections()
		{
			if (members.Count == 0)
			{
				return null;
			}
			onlineMemberConnections.Clear();
			foreach (ulong member in members)
			{
				BasePlayer basePlayer = FindByID(member);
				if (!((Object)(object)basePlayer == (Object)null) && basePlayer.Connection != null)
				{
					onlineMemberConnections.Add(basePlayer.Connection);
				}
			}
			return onlineMemberConnections;
		}

		void IPooled.EnterPool()
		{
			teamID = 0uL;
			teamName = string.Empty;
			teamLeader = 0uL;
			teamStartTime = 0f;
			joinKey = null;
			members.Clear();
			invites.Clear();
			onlineMemberConnections.Clear();
		}

		void IPooled.LeavePool()
		{
		}
	}

	[ReplicatedVar(Default = "true")]
	public static bool contacts;

	public const FileStorage.Type MugshotFileFormat = FileStorage.Type.jpg;

	private const int MugshotResolution = 256;

	private const int MugshotMaxFileSize = 65536;

	private const float MugshotMaxDistance = 50f;

	public Dictionary<ulong, PlayerRelationships> relationships = new Dictionary<ulong, PlayerRelationships>();

	private int lastReputationUpdateIndex;

	private const int seenReputationSeconds = 60;

	private int startingReputation;

	[ServerVar(Help = "(Generated) Time in minutes after which relationship/contacts data for players who have not been seen is forgotten and removed")]
	public static int forgetafterminutes;

	[ServerVar(Help = "(Generated) Maximum number of relationship entries (contacts) each player can store; older entries are evicted when the limit is reached")]
	public static int maxplayerrelationships;

	[ServerVar(Help = "(Generated) Distance in metres within which two players must be for a 'seen' relationship event to be recorded")]
	public static float seendistance;

	[ServerVar(Help = "(Generated) Interval in seconds between mugshot (contact portrait) refresh attempts for known players")]
	public static float mugshotUpdateInterval;

	private static List<BasePlayer> _dirtyRelationshipPlayers;

	private static Phrase RemoteInvitesBlocked;

	public static int maxTeamSize_Internal;

	public Dictionary<ulong, BasePlayer> cachedPlayers = new Dictionary<ulong, BasePlayer>();

	public Dictionary<ulong, PlayerTeam> playerToTeam = new Dictionary<ulong, PlayerTeam>();

	public Dictionary<ulong, PlayerTeam> teams = new Dictionary<ulong, PlayerTeam>();

	[ServerVar(Help = "(Generated) Maximum number of players allowed in a single team; 0 = teams disabled; changing this at runtime updates all active teams")]
	public static int maxTeamSize
	{
		get
		{
			return maxTeamSize_Internal;
		}
		set
		{
			maxTeamSize_Internal = value;
			if (Object.op_Implicit((Object)(object)ServerInstance))
			{
				ServerInstance.SendNetworkUpdate();
			}
		}
	}

	public static RelationshipManager ServerInstance { get; private set; }

	public RelationshipManagerDB Database { get; private set; }

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("RelationshipManager.OnRpcMessage"))
		{
			if (rpc == 1684577101 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_ChangeRelationship"));
				}
				using (TimeWarning.New("SERVER_ChangeRelationship"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1684577101u, "SERVER_ChangeRelationship", this, player, 2uL))
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
							SERVER_ChangeRelationship(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in SERVER_ChangeRelationship");
					}
				}
				return true;
			}
			if (rpc == 1239936737 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_ReceiveMugshot"));
				}
				using (TimeWarning.New("SERVER_ReceiveMugshot"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1239936737u, "SERVER_ReceiveMugshot", this, player, 10uL))
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
							SERVER_ReceiveMugshot(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in SERVER_ReceiveMugshot");
					}
				}
				return true;
			}
			if (rpc == 2178173141u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_SendFreshContacts"));
				}
				using (TimeWarning.New("SERVER_SendFreshContacts"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2178173141u, "SERVER_SendFreshContacts", this, player, 1uL))
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
							SERVER_SendFreshContacts(msg4);
						}
					}
					catch (Exception ex3)
					{
						Debug.LogException(ex3);
						player.Kick("RPC Error in SERVER_SendFreshContacts");
					}
				}
				return true;
			}
			if (rpc == 290196604 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SERVER_UpdatePlayerNote"));
				}
				using (TimeWarning.New("SERVER_UpdatePlayerNote"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(290196604u, "SERVER_UpdatePlayerNote", this, player, 10uL))
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
							SERVER_UpdatePlayerNote(msg5);
						}
					}
					catch (Exception ex4)
					{
						Debug.LogException(ex4);
						player.Kick("RPC Error in SERVER_UpdatePlayerNote");
					}
				}
				return true;
			}
			if (rpc == 2865758366u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - TryCreateTeam"));
				}
				using (TimeWarning.New("TryCreateTeam"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2865758366u, "TryCreateTeam", this, player, 1uL))
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
							TryCreateTeam(rpc2);
						}
					}
					catch (Exception ex5)
					{
						Debug.LogException(ex5);
						player.Kick("RPC Error in TryCreateTeam");
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
		if (contacts)
		{
			InvokeRepeating(UpdateReputations, 0f, 0.05f);
			InvokeRepeating(SendRelationships, 0f, 5f);
		}
	}

	public void UpdateReputations()
	{
		if (!contacts || BasePlayer.activePlayerList.Count == 0)
		{
			return;
		}
		if (lastReputationUpdateIndex >= BasePlayer.activePlayerList.Count)
		{
			lastReputationUpdateIndex = 0;
		}
		BasePlayer basePlayer = BasePlayer.activePlayerList[lastReputationUpdateIndex];
		if (basePlayer.requestingReputationUpdate)
		{
			if (basePlayer.reputation != (basePlayer.reputation = GetReputationFor(basePlayer.userID)))
			{
				basePlayer.SendNetworkUpdate();
			}
			basePlayer.requestingReputationUpdate = false;
		}
		lastReputationUpdateIndex++;
	}

	public int GetReputationFor(ulong playerID)
	{
		int num = startingReputation;
		foreach (PlayerRelationships value2 in relationships.Values)
		{
			if (!value2.relations.TryGetValue(playerID, out var value))
			{
				continue;
			}
			if (value.type == RelationshipType.Friend)
			{
				num++;
			}
			else if (value.type == RelationshipType.Acquaintance)
			{
				if (value.weight > 60)
				{
					num++;
				}
			}
			else if (value.type == RelationshipType.Enemy)
			{
				num--;
			}
		}
		return num;
	}

	[ServerVar(Help = "(Generated) Wipes all relationship contacts data for the calling player")]
	public static void wipecontacts(ConsoleSystem.Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!((Object)(object)basePlayer == (Object)null) && !((Object)(object)ServerInstance == (Object)null))
		{
			ulong num = basePlayer.userID.Get();
			if (ServerInstance.relationships.ContainsKey(num))
			{
				Debug.Log((object)("Wiped contacts for :" + num));
				ServerInstance.relationships.Remove(num);
				ServerInstance.MarkRelationshipsDirtyFor(num);
			}
			else
			{
				Debug.Log((object)("No contacts for :" + num));
			}
		}
	}

	[ServerVar(Help = "(Generated) Wipes all relationship contacts data for every player on the server; admin-only")]
	public static void wipe_all_contacts(ConsoleSystem.Arg arg)
	{
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ArgEx.Player(arg) == (Object)null || (Object)(object)ServerInstance == (Object)null)
		{
			return;
		}
		if (!arg.HasArgs() || arg.Args[0] != StringView.op_Implicit("confirm"))
		{
			Debug.Log((object)"Please append the word 'confirm' at the end of the console command to execute");
			return;
		}
		ServerInstance.relationships.Clear();
		Enumerator<BasePlayer> enumerator = BasePlayer.activePlayerList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				ServerInstance.MarkRelationshipsDirtyFor(current);
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		Debug.Log((object)"Wiped all contacts.");
	}

	public float GetAcquaintanceMaxDist()
	{
		return seendistance;
	}

	public void UpdateAcquaintancesFor(BasePlayer player, float deltaSeconds)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		PlayerRelationships playerRelationships = GetRelationships(player.userID);
		List<BasePlayer> list = Pool.Get<List<BasePlayer>>();
		BaseNetworkable.GetCloseConnections(((Component)player).transform.position, GetAcquaintanceMaxDist(), list);
		foreach (BasePlayer item in list)
		{
			if ((Object)(object)item == (Object)(object)player || !item.IsAlive() || item.IsSleeping() || item.isInvisible || item.limitNetworking)
			{
				continue;
			}
			PlayerRelationshipInfo relations = playerRelationships.GetRelations(item.userID);
			if (!(Vector3.Distance(((Component)player).transform.position, ((Component)item).transform.position) <= GetAcquaintanceMaxDist()))
			{
				continue;
			}
			relations.lastSeenTime = Time.realtimeSinceStartup;
			if (relations.type != RelationshipType.NONE && relations.type != RelationshipType.Acquaintance)
			{
				continue;
			}
			bool flag = false;
			if (BasePlayer.allowRelationshipServerOcclusion && player.SupportsServerOcclusion())
			{
				flag = player.OcclusionGetCachedVisibility(item);
			}
			if (flag)
			{
				flag = player.IsPlayerVisibleToUs(item, Vector3.zero, 1218519041);
			}
			if (flag)
			{
				int num = Mathf.CeilToInt(deltaSeconds);
				if (player.InSafeZone() || item.InSafeZone())
				{
					num = 0;
				}
				if (relations.type != RelationshipType.Acquaintance || (relations.weight < 60 && num > 0))
				{
					SetRelationship(player, item, RelationshipType.Acquaintance, num);
				}
			}
		}
		Pool.FreeUnmanaged<BasePlayer>(ref list);
	}

	public void SetSeen(BasePlayer player, BasePlayer otherPlayer)
	{
		ulong player2 = player.userID.Get();
		ulong player3 = otherPlayer.userID.Get();
		PlayerRelationshipInfo relations = GetRelationships(player2).GetRelations(player3);
		if (relations.type != RelationshipType.NONE)
		{
			relations.lastSeenTime = Time.realtimeSinceStartup;
		}
	}

	public bool CleanupOldContacts(PlayerRelationships ownerRelationships, ulong playerID, RelationshipType relationshipType = RelationshipType.Acquaintance)
	{
		int numberRelationships = GetNumberRelationships(playerID);
		if (numberRelationships < maxplayerrelationships)
		{
			return true;
		}
		List<ulong> list = Pool.Get<List<ulong>>();
		foreach (KeyValuePair<ulong, PlayerRelationshipInfo> relation in ownerRelationships.relations)
		{
			if (relation.Value.type == relationshipType && Time.realtimeSinceStartup - relation.Value.lastSeenTime > (float)forgetafterminutes * 60f)
			{
				list.Add(relation.Key);
				BasePlayer basePlayer = BasePlayer.FindByID(relation.Key);
				if (Object.op_Implicit((Object)(object)basePlayer))
				{
					basePlayer.requestingReputationUpdate = true;
				}
			}
		}
		int count = list.Count;
		foreach (ulong item in list)
		{
			ownerRelationships.Forget(item);
		}
		Pool.FreeUnmanaged<ulong>(ref list);
		return numberRelationships - count < maxplayerrelationships;
	}

	public void ForceRelationshipByID(BasePlayer player, ulong otherPlayerID, RelationshipType newType, int weight, bool sendImmediate = false)
	{
		if (!contacts || (Object)(object)player == (Object)null || (ulong)player.userID == otherPlayerID || player.IsNpc)
		{
			return;
		}
		ulong player2 = player.userID.Get();
		if (HasRelations(player2, otherPlayerID))
		{
			PlayerRelationshipInfo relations = GetRelationships(player2).GetRelations(otherPlayerID);
			if (relations.type != newType)
			{
				relations.weight = 0;
			}
			relations.type = newType;
			relations.weight += weight;
			if (sendImmediate)
			{
				SendRelationshipsFor(player);
			}
			else
			{
				MarkRelationshipsDirtyFor(player);
			}
			BasePlayer basePlayer = BasePlayer.FindByID(otherPlayerID);
			if ((Object)(object)basePlayer != (Object)null)
			{
				basePlayer.requestingReputationUpdate = true;
			}
		}
	}

	public void SetRelationship(BasePlayer player, BasePlayer otherPlayer, RelationshipType type, int weight = 1, bool sendImmediate = false)
	{
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_018e: Unknown result type (might be due to invalid IL or missing references)
		if (!contacts)
		{
			return;
		}
		ulong num = player.userID.Get();
		ulong num2 = otherPlayer.userID.Get();
		if ((Object)(object)player == (Object)null || (Object)(object)player == (Object)(object)otherPlayer || player.IsNpc || ((Object)(object)otherPlayer != (Object)null && otherPlayer.IsNpc) || Interface.CallHook("CanSetRelationship", player, otherPlayer, type, weight) != null)
		{
			return;
		}
		PlayerRelationships playerRelationships = GetRelationships(num);
		otherPlayer.requestingReputationUpdate = true;
		if (!CleanupOldContacts(playerRelationships, num))
		{
			CleanupOldContacts(playerRelationships, num, RelationshipType.Enemy);
		}
		PlayerRelationshipInfo relations = playerRelationships.GetRelations(num2);
		bool flag = false;
		if (relations.type != type)
		{
			flag = true;
			relations.weight = 0;
		}
		relations.type = type;
		relations.weight += weight;
		float num3 = Time.realtimeSinceStartup - relations.lastMugshotTime;
		if (flag || relations.mugshotCrc == 0 || num3 >= mugshotUpdateInterval)
		{
			bool flag2 = otherPlayer.IsAlive();
			bool flag3 = player.SecondsSinceAttacked > 10f && !player.IsAiming;
			if (!player.IsPlayerVisibleToUs(otherPlayer, Vector3.zero, 1218519041))
			{
				flag3 = false;
			}
			float num4 = 100f;
			if (flag3)
			{
				Vector3 val = otherPlayer.eyes.position - player.eyes.position;
				Vector3 normalized = ((Vector3)(ref val)).normalized;
				bool flag4 = Vector3.Dot(player.eyes.HeadForward(), normalized) >= 0.6f;
				float num5 = Vector3Ex.Distance2D(((Component)player).transform.position, ((Component)otherPlayer).transform.position);
				if ((flag2 && num5 < num4) & flag4)
				{
					relations.awaitingMugshot = true;
					ClientRPC(RpcTarget.Player("CLIENT_DoMugshot", player), num2);
					relations.lastMugshotTime = Time.realtimeSinceStartup;
				}
			}
		}
		if (sendImmediate)
		{
			SendRelationshipsFor(player);
		}
		else
		{
			MarkRelationshipsDirtyFor(player);
		}
	}

	public PlayerRelationships GetRelationshipSaveByID(ulong playerID)
	{
		PlayerRelationships val = Pool.Get<PlayerRelationships>();
		PlayerRelationships playerRelationships = GetRelationships(playerID);
		if (playerRelationships != null)
		{
			val.playerID = playerID;
			val.relations = Pool.Get<List<PlayerRelationshipInfo>>();
			{
				foreach (KeyValuePair<ulong, PlayerRelationshipInfo> relation in playerRelationships.relations)
				{
					val.relations.Add(relation.Value.ToProto());
				}
				return val;
			}
		}
		return null;
	}

	public void MarkRelationshipsDirtyFor(ulong playerID)
	{
		BasePlayer basePlayer = FindByID(playerID);
		if (Object.op_Implicit((Object)(object)basePlayer))
		{
			MarkRelationshipsDirtyFor(basePlayer);
		}
	}

	public static void ForceSendRelationships(BasePlayer player)
	{
		if (Object.op_Implicit((Object)(object)ServerInstance))
		{
			ServerInstance.MarkRelationshipsDirtyFor(player);
		}
	}

	public void MarkRelationshipsDirtyFor(BasePlayer player)
	{
		if (!((Object)(object)player == (Object)null) && !_dirtyRelationshipPlayers.Contains(player))
		{
			_dirtyRelationshipPlayers.Add(player);
		}
	}

	public void SendRelationshipsFor(BasePlayer player)
	{
		if (!contacts)
		{
			return;
		}
		ulong playerID = player.userID.Get();
		PlayerRelationships relationshipSaveByID = GetRelationshipSaveByID(playerID);
		try
		{
			ClientRPC(RpcTarget.Player("CLIENT_RecieveLocalRelationships", player), relationshipSaveByID);
		}
		finally
		{
			((IDisposable)relationshipSaveByID)?.Dispose();
		}
	}

	public void SendRelationships()
	{
		if (!contacts)
		{
			return;
		}
		foreach (BasePlayer dirtyRelationshipPlayer in _dirtyRelationshipPlayers)
		{
			if (!((Object)(object)dirtyRelationshipPlayer == (Object)null) && dirtyRelationshipPlayer.IsConnected && !dirtyRelationshipPlayer.IsSleeping())
			{
				SendRelationshipsFor(dirtyRelationshipPlayer);
			}
		}
		_dirtyRelationshipPlayers.Clear();
	}

	public int GetNumberRelationships(ulong player)
	{
		if (relationships.TryGetValue(player, out var value))
		{
			return value.relations.Count;
		}
		return 0;
	}

	public bool HasRelations(ulong player, ulong otherPlayer)
	{
		if (relationships.TryGetValue(player, out var value) && value.relations.ContainsKey(otherPlayer))
		{
			return true;
		}
		return false;
	}

	public PlayerRelationships GetRelationships(ulong player)
	{
		if (relationships.TryGetValue(player, out var value))
		{
			return value;
		}
		PlayerRelationships playerRelationships = Pool.Get<PlayerRelationships>();
		playerRelationships.ownerPlayer = player;
		relationships.Add(player, playerRelationships);
		return playerRelationships;
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(1uL)]
	public void SERVER_SendFreshContacts(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (Object.op_Implicit((Object)(object)player))
		{
			SendRelationshipsFor(player);
		}
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(2uL)]
	public void SERVER_ChangeRelationship(RPCMessage msg)
	{
		EncryptedValue<ulong> userID = msg.player.userID;
		ulong num = msg.read.UInt64();
		int num2 = Mathf.Clamp(msg.read.Int32(), 0, 3);
		PlayerRelationships playerRelationships = GetRelationships(userID);
		playerRelationships.GetRelations(num);
		BasePlayer player = msg.player;
		RelationshipType relationshipType = (RelationshipType)num2;
		if (num2 == 0)
		{
			if (playerRelationships.Forget(num))
			{
				SendRelationshipsFor(player);
			}
			return;
		}
		BasePlayer basePlayer = FindByID(num);
		if ((Object)(object)basePlayer == (Object)null)
		{
			ForceRelationshipByID(player, num, relationshipType, 0, sendImmediate: true);
		}
		else
		{
			SetRelationship(player, basePlayer, relationshipType, 1, sendImmediate: true);
		}
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(10uL)]
	public void SERVER_UpdatePlayerNote(RPCMessage msg)
	{
		EncryptedValue<ulong> userID = msg.player.userID;
		ulong player = msg.read.UInt64();
		string notes = msg.read.String();
		GetRelationships(userID).GetRelations(player).notes = notes;
		MarkRelationshipsDirtyFor(userID);
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(10uL)]
	public void SERVER_ReceiveMugshot(RPCMessage msg)
	{
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		EncryptedValue<ulong> userID = msg.player.userID;
		ulong num = msg.read.UInt64();
		uint num2 = msg.read.UInt32();
		if (!relationships.TryGetValue(userID, out var value) || !value.relations.TryGetValue(num, out var value2))
		{
			return;
		}
		RelationshipType type = value2.type;
		if ((type != RelationshipType.Acquaintance && type != RelationshipType.Friend && type != RelationshipType.Enemy) || !value2.awaitingMugshot)
		{
			return;
		}
		value2.awaitingMugshot = false;
		byte[] array = msg.read.BytesWithSize(65536u);
		if (array != null && ImageProcessing.IsValidJPG(array, 256, 512))
		{
			uint steamIdHash = GetSteamIdHash(userID, num);
			uint num3 = FileStorage.server.Store(array, FileStorage.Type.jpg, net.ID, steamIdHash);
			if (num3 != num2)
			{
				Debug.LogWarning((object)"Client/Server FileStorage CRC differs");
			}
			if (num3 != value2.mugshotCrc)
			{
				FileStorage.server.RemoveExact(value2.mugshotCrc, FileStorage.Type.jpg, net.ID, steamIdHash);
			}
			value2.mugshotCrc = num3;
			MarkRelationshipsDirtyFor(userID);
		}
	}

	private void DeleteMugshot(ulong steamId, ulong targetSteamId, uint crc)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (crc != 0)
		{
			uint steamIdHash = GetSteamIdHash(steamId, targetSteamId);
			FileStorage.server.RemoveExact(crc, FileStorage.Type.jpg, net.ID, steamIdHash);
		}
	}

	public static uint GetSteamIdHash(ulong requesterSteamId, ulong targetSteamId)
	{
		return (uint)(((requesterSteamId & 0xFFFF) << 16) | (targetSteamId & 0xFFFF));
	}

	public int GetMaxTeamSize()
	{
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(serverside: true);
		if (Object.op_Implicit((Object)(object)activeGameMode))
		{
			return activeGameMode.GetMaxRelationshipTeamSize();
		}
		return maxTeamSize;
	}

	public void OnEnable()
	{
		if (base.isServer)
		{
			if ((Object)(object)ServerInstance != (Object)null)
			{
				Debug.LogError((object)"Major fuckup! RelationshipManager spawned twice, Contact Developers!");
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
		Database?.Close();
		Database = null;
	}

	public override void InitShared()
	{
		base.InitShared();
		if (base.isServer)
		{
			try
			{
				Database?.Close();
				Database = null;
				string path = $"{ConVar.Server.rootFolder}/relationship.{287}.db";
				Database = new RelationshipManagerDB();
				Database.Open(path);
				Database.Initialize();
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
		}
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		info.msg.relationshipManager = Pool.Get<RelationshipManager>();
		info.msg.relationshipManager.maxTeamSize = maxTeamSize;
		if (!info.forDisk)
		{
			return;
		}
		info.msg.relationshipManager.teamList = Pool.Get<List<PlayerTeam>>();
		foreach (KeyValuePair<ulong, PlayerTeam> team in teams)
		{
			PlayerTeam value = team.Value;
			if (value == null)
			{
				continue;
			}
			PlayerTeam val = Pool.Get<PlayerTeam>();
			val.teamLeader = value.teamLeader;
			val.teamID = value.teamID;
			val.teamName = value.teamName;
			val.joinKey = value.joinKey;
			val.members = Pool.Get<List<TeamMember>>();
			val.invites = Pool.Get<List<ulong>>();
			foreach (ulong invite in value.invites)
			{
				val.invites.Add(invite);
			}
			foreach (ulong member in value.members)
			{
				TeamMember val2 = Pool.Get<TeamMember>();
				BasePlayer basePlayer = FindByID(member);
				val2.displayName = (((Object)(object)basePlayer != (Object)null) ? basePlayer.displayName : (SingletonComponent<ServerMgr>.Instance.persistance.GetPlayerName(member) ?? "DEAD"));
				val2.userID = member;
				val.members.Add(val2);
			}
			info.msg.relationshipManager.teamList.Add(val);
		}
		info.msg.relationshipManager.relationships = Pool.Get<List<PlayerRelationships>>();
		foreach (ulong key in relationships.Keys)
		{
			_ = relationships[key];
			PlayerRelationships relationshipSaveByID = GetRelationshipSaveByID(key);
			info.msg.relationshipManager.relationships.Add(relationshipSaveByID);
		}
	}

	public void DisbandTeam(PlayerTeam teamToDisband)
	{
		if (Interface.CallHook("OnTeamDisband", teamToDisband) == null)
		{
			teams.Remove(teamToDisband.teamID);
			Interface.CallHook("OnTeamDisbanded", teamToDisband);
			Pool.Free<PlayerTeam>(ref teamToDisband);
		}
	}

	public static BasePlayer FindByID(ulong userID)
	{
		BasePlayer value = null;
		if (ServerInstance.cachedPlayers.TryGetValue(userID, out value))
		{
			if ((Object)(object)value != (Object)null)
			{
				return value;
			}
			ServerInstance.cachedPlayers.Remove(userID);
		}
		BasePlayer basePlayer = BasePlayer.FindByID(userID);
		if (!Object.op_Implicit((Object)(object)basePlayer))
		{
			basePlayer = BasePlayer.FindSleeping(userID);
		}
		if ((Object)(object)basePlayer != (Object)null)
		{
			ServerInstance.cachedPlayers.Add(userID, basePlayer);
		}
		return basePlayer;
	}

	public PlayerTeam FindTeam(ulong TeamID)
	{
		if (teams.ContainsKey(TeamID))
		{
			return teams[TeamID];
		}
		return null;
	}

	public PlayerTeam FindPlayersTeam(ulong userID)
	{
		if (playerToTeam.TryGetValue(userID, out var value))
		{
			return value;
		}
		return null;
	}

	public bool IsPlayerInTeam(ulong userId)
	{
		return playerToTeam.ContainsKey(userId);
	}

	public PlayerTeam FindByJoinKey(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			return null;
		}
		return teams.Values.FirstOrDefault((PlayerTeam x) => x.joinKey == key);
	}

	public PlayerTeam CreatePartyTeam(string joinKey)
	{
		PlayerTeam playerTeam = CreateTeam();
		playerTeam.usePartySpawn = true;
		playerTeam.joinKey = joinKey;
		return playerTeam;
	}

	public PlayerTeam CreateTeam()
	{
		PlayerTeam playerTeam = Pool.Get<PlayerTeam>();
		playerTeam.teamID = Database.IncrementLastTeamIndex();
		playerTeam.teamStartTime = Time.realtimeSinceStartup;
		teams.Add(playerTeam.teamID, playerTeam);
		return playerTeam;
	}

	private PlayerTeam CreateTeam(ulong customId)
	{
		PlayerTeam playerTeam = Pool.Get<PlayerTeam>();
		playerTeam.teamID = customId;
		playerTeam.teamStartTime = Time.realtimeSinceStartup;
		teams.Add(playerTeam.teamID, playerTeam);
		return playerTeam;
	}

	[RPC_Server.CallsPerSecond(1uL)]
	[RPC_Server]
	private void TryCreateTeam(RPCMessage rpc)
	{
		if (maxTeamSize != 0)
		{
			TryCreateTeam(rpc.player);
		}
	}

	[ServerVar(Help = "(Generated) Creates a new team with the calling player as leader; fails if teams are disabled (maxTeamSize == 0)")]
	public static void trycreateteam(ConsoleSystem.Arg arg)
	{
		if (maxTeamSize == 0)
		{
			arg.ReplyWith("Teams are disabled on this server");
		}
		else
		{
			TryCreateTeam(ArgEx.Player(arg));
		}
	}

	private static void TryCreateTeam(BasePlayer player)
	{
		if (player.currentTeam == 0L && Interface.CallHook("OnTeamCreate", player) == null)
		{
			PlayerTeam playerTeam = ServerInstance.CreateTeam();
			playerTeam.teamLeader = player.userID;
			playerTeam.AddPlayer(player);
			Facepunch.Rust.Analytics.Azure.OnTeamChanged("created", playerTeam.teamID, player.userID, player.userID, playerTeam.members);
			Interface.CallHook("OnTeamCreated", player, playerTeam);
		}
	}

	[ServerUserVar]
	public static void promote(ConsoleSystem.Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer.currentTeam == 0L)
		{
			return;
		}
		BasePlayer lookingAtPlayer = GetLookingAtPlayer(basePlayer);
		if (!((Object)(object)lookingAtPlayer == (Object)null) && !lookingAtPlayer.IsDead() && !((Object)(object)lookingAtPlayer == (Object)(object)basePlayer) && lookingAtPlayer.currentTeam == basePlayer.currentTeam)
		{
			PlayerTeam playerTeam = ServerInstance.teams[basePlayer.currentTeam];
			if (playerTeam != null && playerTeam.teamLeader == (ulong)basePlayer.userID)
			{
				playerTeam.SetTeamLeader(lookingAtPlayer.userID);
			}
		}
	}

	[ServerUserVar]
	public static void promote_id(ConsoleSystem.Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (basePlayer.currentTeam == 0L)
		{
			return;
		}
		BasePlayer playerOrSleeperOrBot = ArgEx.GetPlayerOrSleeperOrBot(arg, 0);
		if (!((Object)(object)playerOrSleeperOrBot == (Object)null) && !playerOrSleeperOrBot.IsDead() && !((Object)(object)playerOrSleeperOrBot == (Object)(object)basePlayer) && playerOrSleeperOrBot.currentTeam == basePlayer.currentTeam)
		{
			PlayerTeam playerTeam = ServerInstance.teams[basePlayer.currentTeam];
			if (playerTeam != null && playerTeam.teamLeader == (ulong)basePlayer.userID)
			{
				playerTeam.SetTeamLeader(playerOrSleeperOrBot.userID);
			}
		}
	}

	[ServerUserVar]
	public static void leaveteam(ConsoleSystem.Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!((Object)(object)basePlayer == (Object)null) && basePlayer.currentTeam != 0L)
		{
			PlayerTeam playerTeam = ServerInstance.FindTeam(basePlayer.currentTeam);
			if (playerTeam != null && Interface.CallHook("OnTeamLeave", playerTeam, basePlayer) == null)
			{
				playerTeam.RemovePlayer(basePlayer.userID);
				basePlayer.ClearTeam();
			}
		}
	}

	[ServerUserVar]
	public static void acceptinvite(ConsoleSystem.Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!((Object)(object)basePlayer == (Object)null) && basePlayer.currentTeam == 0L)
		{
			ulong uLong = arg.GetULong(0, 0uL);
			PlayerTeam playerTeam = ServerInstance.FindTeam(uLong);
			if (playerTeam == null)
			{
				basePlayer.ClearPendingInvite();
			}
			else if (Interface.CallHook("OnTeamAcceptInvite", playerTeam, basePlayer) == null)
			{
				playerTeam.AcceptInvite(basePlayer);
			}
		}
	}

	[ServerUserVar]
	public static void rejectinvite(ConsoleSystem.Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!((Object)(object)basePlayer == (Object)null) && basePlayer.currentTeam == 0L)
		{
			ulong uLong = arg.GetULong(0, 0uL);
			PlayerTeam playerTeam = ServerInstance.FindTeam(uLong);
			if (playerTeam == null)
			{
				basePlayer.ClearPendingInvite();
			}
			else if (Interface.CallHook("OnTeamRejectInvite", basePlayer, playerTeam) == null)
			{
				playerTeam.RejectInvite(basePlayer);
			}
		}
	}

	public static BasePlayer GetLookingAtPlayer(BasePlayer source)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit hit = default(RaycastHit);
		if (Physics.Raycast(source.eyes.position, source.eyes.HeadForward(), ref hit, 5f, 1218652417, (QueryTriggerInteraction)1))
		{
			BaseEntity entity = RaycastHitEx.GetEntity(hit);
			if (Object.op_Implicit((Object)(object)entity))
			{
				return ((Component)entity).GetComponent<BasePlayer>();
			}
		}
		return null;
	}

	[ServerVar(Help = "(Generated) Toggles the sleep/wake state of the entity the calling admin player is looking at (within 5m); admin-only")]
	public static void sleeptoggle(ConsoleSystem.Arg arg)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		RaycastHit hit = default(RaycastHit);
		if ((Object)(object)basePlayer == (Object)null || !Physics.Raycast(basePlayer.eyes.position, basePlayer.eyes.HeadForward(), ref hit, 5f, 1218652417, (QueryTriggerInteraction)1))
		{
			return;
		}
		BaseEntity entity = RaycastHitEx.GetEntity(hit);
		if (!Object.op_Implicit((Object)(object)entity))
		{
			return;
		}
		BasePlayer component = ((Component)entity).GetComponent<BasePlayer>();
		if (Object.op_Implicit((Object)(object)component) && (Object)(object)component != (Object)(object)basePlayer && !component.IsNpc)
		{
			if (component.IsSleeping())
			{
				component.EndSleeping();
			}
			else
			{
				component.StartSleeping();
			}
		}
	}

	[ServerUserVar]
	public static void kickmember(ConsoleSystem.Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if ((Object)(object)basePlayer == (Object)null)
		{
			return;
		}
		PlayerTeam playerTeam = ServerInstance.FindTeam(basePlayer.currentTeam);
		if (playerTeam != null && !((Object)(object)playerTeam.GetLeader() != (Object)(object)basePlayer))
		{
			ulong uLong = arg.GetULong(0, 0uL);
			if ((ulong)basePlayer.userID != uLong && Interface.CallHook("OnTeamKick", playerTeam, basePlayer, uLong) == null)
			{
				playerTeam.RemovePlayer(uLong);
			}
		}
	}

	[ServerUserVar]
	public static void sendinvite(ConsoleSystem.Arg arg)
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		PlayerTeam playerTeam = ServerInstance.FindTeam(basePlayer.currentTeam);
		if (playerTeam == null || (Object)(object)playerTeam.GetLeader() == (Object)null || (Object)(object)playerTeam.GetLeader() != (Object)(object)basePlayer)
		{
			return;
		}
		ulong uLong = arg.GetULong(0, 0uL);
		if (uLong == 0L)
		{
			return;
		}
		BasePlayer basePlayer2 = BaseNetworkable.serverEntities.Find(new NetworkableId(uLong)) as BasePlayer;
		if (Object.op_Implicit((Object)(object)basePlayer2) && (Object)(object)basePlayer2 != (Object)(object)basePlayer && !basePlayer2.IsNpc && basePlayer2.currentTeam == 0L)
		{
			float num = 7f;
			if (!(Vector3.Distance(((Component)basePlayer2).transform.position, ((Component)basePlayer).transform.position) > num) && Interface.CallHook("OnTeamMemberInvite", playerTeam, basePlayer, basePlayer2.userID.Get(), false) == null)
			{
				playerTeam.SendInvite(basePlayer2);
			}
		}
	}

	public bool HasPendingInvite(ulong forPlayer, out ulong foundTeamID)
	{
		foundTeamID = 0uL;
		foreach (KeyValuePair<ulong, PlayerTeam> team in teams)
		{
			if (team.Value.invites.Contains(forPlayer))
			{
				foundTeamID = team.Key;
				return true;
			}
		}
		return false;
	}

	public bool GetTeamLeaderInfo(ulong teamID, out string leaderDisplayName, out ulong leaderID)
	{
		leaderDisplayName = string.Empty;
		leaderID = 0uL;
		if (teams.TryGetValue(teamID, out var value))
		{
			BasePlayer basePlayer = BasePlayer.FindAwakeOrSleepingByID(value.teamLeader);
			if ((Object)(object)basePlayer != (Object)null)
			{
				leaderDisplayName = basePlayer.displayName;
				leaderID = basePlayer.userID;
				return true;
			}
		}
		return false;
	}

	[ServerUserVar]
	public static void sendofflineinvite(ConsoleSystem.Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		PlayerTeam playerTeam = ServerInstance.FindTeam(basePlayer.currentTeam);
		if (playerTeam == null || (Object)(object)playerTeam.GetLeader() == (Object)null || (Object)(object)playerTeam.GetLeader() != (Object)(object)basePlayer)
		{
			return;
		}
		ulong uLong = arg.GetULong(0, 0uL);
		if (uLong != 0L)
		{
			BasePlayer basePlayer2 = BasePlayer.FindAwakeOrSleepingByID(uLong);
			if ((Object)(object)basePlayer2 != (Object)null && !basePlayer2.GetInfoBool("client.allowteaminvitesremoteplayers", defaultVal: true))
			{
				basePlayer.ShowToast(GameTip.Styles.Error, RemoteInvitesBlocked, false);
			}
			else if (((Object)(object)basePlayer2 == (Object)null || (!basePlayer2.IsNpc && basePlayer2.currentTeam == 0L)) && Interface.CallHook("OnTeamMemberInvite", playerTeam, basePlayer, uLong, true) == null)
			{
				playerTeam.SendInvite(basePlayer2, uLong);
			}
		}
	}

	[ServerVar(Help = "(Generated) Sends a fake team invite from the given team ID to the calling player; used for testing team invite UI flow")]
	public static void fakeinvite(ConsoleSystem.Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		ulong uLong = arg.GetULong(0, 0uL);
		PlayerTeam playerTeam = ServerInstance.FindTeam(uLong);
		if (playerTeam != null)
		{
			if (basePlayer.currentTeam != 0L)
			{
				Debug.Log((object)"already in team");
			}
			playerTeam.SendInvite(basePlayer);
			Debug.Log((object)"sent bot invite");
		}
	}

	[ServerVar(Help = "(Generated) Adds the calling player to their existing team, creating the team entry if needed")]
	public static void addtoteam(ConsoleSystem.Arg arg)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		PlayerTeam playerTeam = ServerInstance.FindTeam(basePlayer.currentTeam);
		RaycastHit hit = default(RaycastHit);
		if (playerTeam == null || (Object)(object)playerTeam.GetLeader() == (Object)null || (Object)(object)playerTeam.GetLeader() != (Object)(object)basePlayer || !Physics.Raycast(basePlayer.eyes.position, basePlayer.eyes.HeadForward(), ref hit, 5f, 1218652417, (QueryTriggerInteraction)1))
		{
			return;
		}
		BaseEntity entity = RaycastHitEx.GetEntity(hit);
		if (Object.op_Implicit((Object)(object)entity))
		{
			BasePlayer component = ((Component)entity).GetComponent<BasePlayer>();
			if (Object.op_Implicit((Object)(object)component) && (Object)(object)component != (Object)(object)basePlayer && !component.IsNpc)
			{
				playerTeam.AddPlayer(component);
			}
		}
	}

	[ServerVar(Help = "Adds a player to a team whether they are on the server or not")]
	public static void forceaddtoteam(ConsoleSystem.Arg arg)
	{
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (!arg.HasArgs())
		{
			arg.ReplyWith("Format is forceaddtoteam <steamid> <teamid>");
			return;
		}
		ulong uInt = arg.GetUInt64(0, 0uL);
		ulong num = arg.GetUInt64(1, 0uL);
		if (num == 0L)
		{
			if (basePlayer.Team == null)
			{
				PlayerTeam playerTeam = ServerInstance.CreateTeam();
				playerTeam.teamLeader = basePlayer.userID;
				playerTeam.AddPlayer(basePlayer);
				if ((ulong)basePlayer.userID == uInt)
				{
					arg.ReplyWith("Created a new team");
					return;
				}
			}
			num = basePlayer.Team.teamID;
		}
		PlayerTeam playerTeam2 = ServerInstance.FindTeam(num);
		if (playerTeam2 == null)
		{
			arg.ReplyWith($"No team with ID {num} exists");
			return;
		}
		PlayerTeam playerTeam3 = ServerInstance.FindPlayersTeam(uInt);
		if (playerTeam3 != null)
		{
			if (playerTeam3.teamID == playerTeam2.teamID)
			{
				arg.ReplyWith($"{uInt} is already in team {num}");
				return;
			}
			playerTeam3.RemovePlayer(uInt);
		}
		playerTeam2.AddPlayer(uInt);
		arg.ReplyWith($"Added {uInt} to team {num}");
	}

	[ServerVar(Help = "(Generated) Creates a new team with the calling player and adds the specified player (by UID) to it; returns status string")]
	public static string createAndAddToTeam(ConsoleSystem.Arg arg)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(arg);
		uint uInt = arg.GetUInt(0);
		RaycastHit hit = default(RaycastHit);
		if (Physics.Raycast(basePlayer.eyes.position, basePlayer.eyes.HeadForward(), ref hit, 5f, 1218652417, (QueryTriggerInteraction)1))
		{
			BaseEntity entity = RaycastHitEx.GetEntity(hit);
			if (Object.op_Implicit((Object)(object)entity))
			{
				BasePlayer component = ((Component)entity).GetComponent<BasePlayer>();
				if (Object.op_Implicit((Object)(object)component) && (Object)(object)component != (Object)(object)basePlayer && !component.IsNpc)
				{
					if (component.currentTeam != 0L)
					{
						return component.displayName + " is already in a team";
					}
					if (ServerInstance.FindTeam(uInt) != null)
					{
						ServerInstance.FindTeam(uInt).AddPlayer(component);
						return $"Added {component.displayName} to existing team {uInt}";
					}
					PlayerTeam playerTeam = ServerInstance.CreateTeam(uInt);
					playerTeam.teamLeader = component.userID;
					playerTeam.AddPlayer(component);
					return $"Added {component.displayName} to team {uInt}";
				}
			}
		}
		return "Unable to find valid player in front";
	}

	public static bool TeamsEnabled()
	{
		return maxTeamSize > 0;
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (!info.fromDisk || info.msg.relationshipManager == null)
		{
			return;
		}
		foreach (PlayerTeam team in info.msg.relationshipManager.teamList)
		{
			PlayerTeam playerTeam = Pool.Get<PlayerTeam>();
			playerTeam.teamLeader = team.teamLeader;
			playerTeam.teamID = team.teamID;
			playerTeam.teamName = team.teamName;
			playerTeam.joinKey = team.joinKey;
			playerTeam.members = new List<ulong>();
			playerTeam.invites = new List<ulong>();
			foreach (ulong invite in team.invites)
			{
				playerTeam.invites.Add(invite);
			}
			foreach (TeamMember member in team.members)
			{
				playerTeam.members.Add(member.userID);
			}
			teams[playerTeam.teamID] = playerTeam;
		}
		foreach (PlayerTeam value2 in teams.Values)
		{
			foreach (ulong member2 in value2.members)
			{
				playerToTeam[member2] = value2;
				BasePlayer basePlayer = FindByID(member2);
				if ((Object)(object)basePlayer != (Object)null && basePlayer.currentTeam != value2.teamID)
				{
					Debug.LogWarning((object)$"Player {member2} has the wrong teamID: got {basePlayer.currentTeam}, expected {value2.teamID}. Fixing automatically.");
					basePlayer.currentTeam = value2.teamID;
				}
			}
		}
		foreach (PlayerRelationships relationship in info.msg.relationshipManager.relationships)
		{
			ulong playerID = relationship.playerID;
			PlayerRelationships playerRelationships = GetRelationships(playerID);
			playerRelationships.ClearRelations();
			foreach (PlayerRelationshipInfo relation in relationship.relations)
			{
				PlayerRelationshipInfo value = PlayerRelationshipInfo.FromProto(relation);
				playerRelationships.relations.Add(relation.playerID, value);
			}
		}
	}

	static RelationshipManager()
	{
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Expected O, but got Unknown
		contacts = true;
		forgetafterminutes = 960;
		maxplayerrelationships = 128;
		seendistance = 10f;
		mugshotUpdateInterval = 300f;
		_dirtyRelationshipPlayers = new List<BasePlayer>();
		RemoteInvitesBlocked = new Phrase("remote.invites.blocked", "That player has remote invites turned off");
		maxTeamSize_Internal = 8;
	}
}
