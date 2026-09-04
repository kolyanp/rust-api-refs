using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using ConVar;
using Development.Attributes;
using Facepunch;
using Facepunch.Extend;
using Facepunch.Rust;
using Network;
using Network.Visibility;
using Oxide.Core;
using ProtoBuf;
using Rust;
using Rust.Workshop;
using Spatial;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class BaseEntity : BaseNetworkable, IOnParentSpawning, IPrefabPreProcess
{
	public class Menu : Attribute
	{
		[Serializable]
		public struct Option
		{
			public Phrase name;

			public Phrase description;

			public Sprite icon;

			public int order;

			public bool usableWhileWounded;
		}

		public class Description : Attribute
		{
			public string token;

			public string english;

			public Description(string t, string e)
			{
				token = t;
				english = e;
			}
		}

		public class Icon : Attribute
		{
			public string icon;

			public Icon(string i)
			{
				icon = i;
			}
		}

		public class ShowIf : Attribute
		{
			public string functionName;

			public ShowIf(string testFunc)
			{
				functionName = testFunc;
			}
		}

		public class DisabledIf : Attribute
		{
			public string functionName;

			public DisabledIf(string testFunc)
			{
				functionName = testFunc;
			}
		}

		public class Priority : Attribute
		{
			public string functionName;

			public Priority(string priorityFunc)
			{
				functionName = priorityFunc;
			}
		}

		public class UsableWhileWounded : Attribute
		{
		}

		public string TitleToken;

		public string TitleEnglish;

		public string UseVariable;

		public int Order;

		public string ProxyFunction;

		public float Time;

		public string OnStart;

		public string OnProgress;

		public string OnCancel;

		public bool LongUseOnly;

		public bool PrioritizeIfNotWhitelisted;

		public bool PrioritizeIfUnlocked;

		public Menu()
		{
		}

		public Menu(string menuTitleToken, string menuTitleEnglish)
		{
			TitleToken = menuTitleToken;
			TitleEnglish = menuTitleEnglish;
		}
	}

	[Flags]
	public enum Flags
	{
		Placeholder = 1,
		On = 2,
		OnFire = 4,
		Open = 8,
		Locked = 0x10,
		Debugging = 0x20,
		Disabled = 0x40,
		Reserved1 = 0x80,
		Reserved2 = 0x100,
		Reserved3 = 0x200,
		Reserved4 = 0x400,
		Reserved5 = 0x800,
		Broken = 0x1000,
		Busy = 0x2000,
		Reserved6 = 0x4000,
		Reserved7 = 0x8000,
		Reserved8 = 0x10000,
		Reserved9 = 0x20000,
		Reserved10 = 0x40000,
		Reserved11 = 0x80000,
		InUse = 0x100000,
		Reserved12 = 0x200000,
		Reserved13 = 0x400000,
		Unused23 = 0x800000,
		Protected = 0x1000000,
		Transferring = 0x2000000,
		Reserved14 = 0x4000000,
		Reserved15 = 0x8000000,
		Reserved16 = 0x10000000,
		Reserved17 = 0x20000000,
		Reserved18 = 0x40000000,
		Reserved19 = ~(Placeholder | On | OnFire | Open | Locked | Debugging | Disabled | Reserved1 | Reserved2 | Reserved3 | Reserved4 | Reserved5 | Broken | Busy | Reserved6 | Reserved7 | Reserved8 | Reserved9 | Reserved10 | Reserved11 | InUse | Reserved12 | Reserved13 | Unused23 | Protected | Transferring | Reserved14 | Reserved15 | Reserved16 | Reserved17 | Reserved18)
	}

	public enum FlagsUpdateMode
	{
		Local,
		SendNetworkUpdate_Flags,
		SendNetworkUpdate,
		SendNetworkUpdateImmediate
	}

	public readonly struct FlagsUpdateScope(BaseEntity owner, FlagsUpdateMode updateMode) : IDisposable
	{
		private readonly BaseEntity owner = owner;

		private readonly Flags oldFlags = owner.flags;

		private readonly FlagsUpdateMode updateMode = updateMode;

		public void Set(Flags f, bool b, bool recursive = false)
		{
			if (b)
			{
				if (owner.HasFlag(f))
				{
					return;
				}
				owner.flags |= f;
			}
			else
			{
				if (!owner.HasFlag(f))
				{
					return;
				}
				owner.flags &= ~f;
			}
			if (!recursive || owner.children == null)
			{
				return;
			}
			int i = 0;
			for (int count = owner.children.Count; i < count; i++)
			{
				using FlagsUpdateScope flagsUpdateScope = owner.children[i].StartSetFlags(updateMode);
				flagsUpdateScope.Set(f, b, recursive: true);
			}
		}

		void IDisposable.Dispose()
		{
			if (oldFlags != owner.flags)
			{
				owner.OnFlagsChanged(oldFlags, owner.flags);
				owner.HandleFlagsUpdateMode(updateMode);
			}
		}
	}

	[Serializable]
	public struct MovementModify
	{
		public float drag;
	}

	private readonly struct QueuedFileRequest(BaseEntity entity, FileStorage.Type type, uint part, uint crc, uint responseFunction, bool? respondIfNotFound, bool isEntityImage = false) : IEquatable<QueuedFileRequest>
	{
		public readonly BaseEntity Entity = entity;

		public readonly FileStorage.Type Type = type;

		public readonly uint Part = part;

		public readonly uint Crc = crc;

		public readonly uint ResponseFunction = responseFunction;

		public readonly bool? RespondIfNotFound = respondIfNotFound;

		public readonly bool IsEntityImage = isEntityImage;

		public bool Equals(QueuedFileRequest other)
		{
			if (object.Equals(Entity, other.Entity) && Type == other.Type && Part == other.Part && Crc == other.Crc && ResponseFunction == other.ResponseFunction && RespondIfNotFound == other.RespondIfNotFound)
			{
				return IsEntityImage == other.IsEntityImage;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is QueuedFileRequest other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			uint num = ((((((((((uint)((((Object)(object)Entity != (Object)null) ? ((object)Entity).GetHashCode() : 0) * 397) ^ (uint)Type) * 397) ^ Part) * 397) ^ Crc) * 397) ^ ResponseFunction) * 397) ^ (uint)RespondIfNotFound.GetHashCode()) * 397;
			bool isEntityImage = IsEntityImage;
			return (int)num ^ isEntityImage.GetHashCode();
		}
	}

	private readonly struct PendingFileRequest(FileStorage.Type type, uint numId, uint crc, IServerFileReceiver receiver) : IEquatable<PendingFileRequest>
	{
		public readonly FileStorage.Type Type = type;

		public readonly uint NumId = numId;

		public readonly uint Crc = crc;

		public readonly IServerFileReceiver Receiver = receiver;

		public readonly float Time = Time.realtimeSinceStartup;

		public bool Equals(PendingFileRequest other)
		{
			if (Type == other.Type && NumId == other.NumId && Crc == other.Crc)
			{
				return object.Equals(Receiver, other.Receiver);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is PendingFileRequest other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return (int)(((((uint)((int)Type * 397) ^ NumId) * 397) ^ Crc) * 397) ^ ((Receiver != null) ? Receiver.GetHashCode() : 0);
		}
	}

	public static class Query
	{
		public enum DistanceCheckType
		{
			None,
			OnlyCenter,
			Bounds
		}

		public class EntityTree
		{
			public Grid<BaseEntity> Grid;

			public Grid<BasePlayer> PlayerGrid;

			public Grid<BaseEntity> BrainGrid;

			public EntityTree(float worldSize)
			{
				Grid = new Grid<BaseEntity>(32, worldSize);
				PlayerGrid = new Grid<BasePlayer>(32, worldSize);
				BrainGrid = new Grid<BaseEntity>(32, worldSize);
			}

			public void Add(BaseEntity ent)
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0013: Unknown result type (might be due to invalid IL or missing references)
				//IL_0019: Unknown result type (might be due to invalid IL or missing references)
				Vector3 position = ((Component)ent).transform.position;
				Grid.Add(ent, position.x, position.z);
			}

			public void AddPlayer(BasePlayer player)
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0013: Unknown result type (might be due to invalid IL or missing references)
				//IL_0019: Unknown result type (might be due to invalid IL or missing references)
				Vector3 position = ((Component)player).transform.position;
				PlayerGrid.Add(player, position.x, position.z);
			}

			public void AddBrain(BaseEntity entity)
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0013: Unknown result type (might be due to invalid IL or missing references)
				//IL_0019: Unknown result type (might be due to invalid IL or missing references)
				Vector3 position = ((Component)entity).transform.position;
				BrainGrid.Add(entity, position.x, position.z);
			}

			public void Remove(BaseEntity ent, bool isPlayer = false)
			{
				Grid.Remove(ent);
				if (isPlayer)
				{
					BasePlayer basePlayer = ent as BasePlayer;
					if ((Object)(object)basePlayer != (Object)null)
					{
						PlayerGrid.Remove(basePlayer);
					}
				}
			}

			public void RemovePlayer(BasePlayer player)
			{
				PlayerGrid.Remove(player);
			}

			public void RemoveBrain(BaseEntity entity)
			{
				if (!((Object)(object)entity == (Object)null))
				{
					BrainGrid.Remove(entity);
				}
			}

			public void Move(BaseEntity ent)
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0013: Unknown result type (might be due to invalid IL or missing references)
				//IL_0019: Unknown result type (might be due to invalid IL or missing references)
				Vector3 position = ((Component)ent).transform.position;
				Grid.Move(ent, position.x, position.z);
				BasePlayer basePlayer = ent as BasePlayer;
				if ((Object)(object)basePlayer != (Object)null)
				{
					MovePlayer(basePlayer);
				}
				if (ent.HasBrain)
				{
					MoveBrain(ent);
				}
			}

			public void MovePlayer(BasePlayer player)
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0013: Unknown result type (might be due to invalid IL or missing references)
				//IL_0019: Unknown result type (might be due to invalid IL or missing references)
				Vector3 position = ((Component)player).transform.position;
				PlayerGrid.Move(player, position.x, position.z);
			}

			public void MoveBrain(BaseEntity entity)
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0013: Unknown result type (might be due to invalid IL or missing references)
				//IL_0019: Unknown result type (might be due to invalid IL or missing references)
				Vector3 position = ((Component)entity).transform.position;
				BrainGrid.Move(entity, position.x, position.z);
			}

			public void SubscribePlayerChanges(Vector3 position, float radius, Action callback)
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				PlayerGrid.Subscribe(position.x, position.z, radius, callback);
			}

			[PoolAnalyzerNonCaching]
			public void GetInSphere<T>(Vector3 position, float distance, List<T> results, DistanceCheckType distanceCheckType = DistanceCheckType.OnlyCenter) where T : BaseEntity
			{
				//IL_0012: Unknown result type (might be due to invalid IL or missing references)
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				using (TimeWarning.New("GetInSphereList"))
				{
					Grid.Query<T>(position.x, position.z, distance, results);
					if (distanceCheckType != DistanceCheckType.None)
					{
						NarrowPhaseReduce(position, distance, results, distanceCheckType == DistanceCheckType.OnlyCenter);
					}
				}
			}

			public int GetInSphere(Vector3 position, float distance, BaseEntity[] results, Func<BaseEntity, bool> filter = null)
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_001d: Unknown result type (might be due to invalid IL or missing references)
				int broadCount = Grid.Query(position.x, position.z, distance, results, filter);
				return NarrowPhaseReduce(position, distance, results, broadCount);
			}

			public int GetInSphereFast(Vector3 position, float distance, BaseEntity[] results, Func<BaseEntity, bool> filter = null)
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				return Grid.Query(position.x, position.z, distance, results, filter);
			}

			[PoolAnalyzerNonCaching]
			public void GetPlayersInSphere(Vector3 position, float distance, List<BasePlayer> results, DistanceCheckType distanceCheckType = DistanceCheckType.OnlyCenter, bool includeHumanoidNpcs = false)
			{
				//IL_0012: Unknown result type (might be due to invalid IL or missing references)
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0055: Unknown result type (might be due to invalid IL or missing references)
				using (TimeWarning.New("GetPlayersInSphereList"))
				{
					PlayerGrid.Query(position.x, position.z, distance, results);
					if (!includeHumanoidNpcs)
					{
						for (int num = results.Count - 1; num >= 0; num--)
						{
							if (results[num].IsNpc)
							{
								results.RemoveAt(num);
							}
						}
					}
					if (distanceCheckType != DistanceCheckType.None)
					{
						NarrowPhaseReduce(position, distance, results, distanceCheckType == DistanceCheckType.OnlyCenter);
					}
				}
			}

			public int GetPlayersInSphere(Vector3 position, float distance, BasePlayer[] results, Func<BasePlayer, bool> filter = null)
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_001d: Unknown result type (might be due to invalid IL or missing references)
				int broadCount = PlayerGrid.Query(position.x, position.z, distance, results, filter);
				return NarrowPhaseReduce(position, distance, results, broadCount);
			}

			public int GetPlayersInSphereFast(Vector3 position, float distance, BasePlayer[] results, Func<BasePlayer, bool> filter = null)
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				return PlayerGrid.Query(position.x, position.z, distance, results, filter);
			}

			[PoolAnalyzerNonCaching]
			public void GetPlayersInSphereFast(Vector3 position, float distance, List<BasePlayer> results, Func<BasePlayer, bool> filter = null)
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				PlayerGrid.Query(position.x, position.z, distance, results, filter);
			}

			public bool AnyPlayersInSphereFast(Vector3 position, float distance, out bool gridNodesEmpty, Func<BasePlayer, bool> ignoreFilter = null, Func<BasePlayer, bool> filter = null)
			{
				//IL_0012: Unknown result type (might be due to invalid IL or missing references)
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				using (TimeWarning.New("AnyPlayersInSphereFast"))
				{
					return PlayerGrid.Any(position.x, position.z, distance, ignoreFilter, filter, ref gridNodesEmpty);
				}
			}

			public void GetBrainsInSphere<T>(Vector3 position, float distance, List<T> results, bool filterPastDistance = true) where T : BaseEntity
			{
				//IL_0012: Unknown result type (might be due to invalid IL or missing references)
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				using (TimeWarning.New("GetBrainsInSphereList"))
				{
					BrainGrid.Query<T>(position.x, position.z, distance, results);
					if (filterPastDistance)
					{
						NarrowPhaseReduce(position, distance, results);
					}
				}
			}

			public int GetBrainsInSphere(Vector3 position, float distance, BaseEntity[] results, Func<BaseEntity, bool> filter = null)
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				//IL_001d: Unknown result type (might be due to invalid IL or missing references)
				int broadCount = BrainGrid.Query(position.x, position.z, distance, results, filter);
				return NarrowPhaseReduce(position, distance, results, broadCount);
			}

			public int GetBrainsInSphereFast(Vector3 position, float distance, BaseEntity[] results, Func<BaseEntity, bool> filter = null)
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				return BrainGrid.Query(position.x, position.z, distance, results, filter);
			}

			[PoolAnalyzerNonCaching]
			public void GetPlayersAndBrainsInSphere(Vector3 position, float distance, List<BaseEntity> results, DistanceCheckType distanceCheckType = DistanceCheckType.OnlyCenter)
			{
				//IL_0012: Unknown result type (might be due to invalid IL or missing references)
				//IL_0018: Unknown result type (might be due to invalid IL or missing references)
				//IL_002b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0031: Unknown result type (might be due to invalid IL or missing references)
				//IL_0042: Unknown result type (might be due to invalid IL or missing references)
				using (TimeWarning.New("GetPlayersAndBrainsInSphereList"))
				{
					PlayerGrid.Query<BaseEntity>(position.x, position.z, distance, results);
					BrainGrid.Query(position.x, position.z, distance, results);
					if (distanceCheckType != DistanceCheckType.None)
					{
						NarrowPhaseReduce(position, distance, results, distanceCheckType == DistanceCheckType.OnlyCenter);
					}
				}
			}

			private int NarrowPhaseReduce<T>(Vector3 position, float radius, T[] results, int broadCount) where T : BaseEntity
			{
				//IL_0050: Unknown result type (might be due to invalid IL or missing references)
				//IL_0055: Unknown result type (might be due to invalid IL or missing references)
				//IL_0059: Unknown result type (might be due to invalid IL or missing references)
				//IL_005a: Unknown result type (might be due to invalid IL or missing references)
				//IL_005f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0060: Unknown result type (might be due to invalid IL or missing references)
				//IL_0065: Unknown result type (might be due to invalid IL or missing references)
				using (TimeWarning.New("NarrowPhaseReduce"))
				{
					int num = broadCount;
					float num2 = radius * radius;
					for (int i = 0; i < num; i++)
					{
						T val = results[i];
						if ((Object)(object)val == (Object)null)
						{
							results[i] = results[num - 1];
							num--;
							i--;
							continue;
						}
						OBB val2 = val.WorldSpaceBounds();
						Vector3 val3 = ((OBB)(ref val2)).ClosestPoint(position) - position;
						if (((Vector3)(ref val3)).sqrMagnitude > num2)
						{
							results[i] = results[num - 1];
							num--;
							i--;
						}
					}
					return num;
				}
			}

			[PoolAnalyzerNonCaching]
			private static void NarrowPhaseReduce<T>(Vector3 position, float radius, List<T> results, bool onlyConsiderCenter = true) where T : BaseEntity
			{
				//IL_005f: Unknown result type (might be due to invalid IL or missing references)
				//IL_0043: Unknown result type (might be due to invalid IL or missing references)
				//IL_0048: Unknown result type (might be due to invalid IL or missing references)
				//IL_004c: Unknown result type (might be due to invalid IL or missing references)
				//IL_004d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0064: Unknown result type (might be due to invalid IL or missing references)
				//IL_0065: Unknown result type (might be due to invalid IL or missing references)
				//IL_006a: Unknown result type (might be due to invalid IL or missing references)
				using (TimeWarning.New("NarrowPhaseReduceList"))
				{
					float num = radius * radius;
					for (int num2 = results.Count - 1; num2 >= 0; num2--)
					{
						T val = results[num2];
						if ((Object)(object)val == (Object)null)
						{
							results.RemoveAt(num2);
						}
						else
						{
							Vector3 val3;
							if (!onlyConsiderCenter)
							{
								OBB val2 = val.WorldSpaceBounds();
								val3 = ((OBB)(ref val2)).ClosestPoint(position);
							}
							else
							{
								val3 = ((Component)val).transform.position;
							}
							Vector3 val4 = val3 - position;
							if (((Vector3)(ref val4)).sqrMagnitude > num)
							{
								results.RemoveAt(num2);
							}
						}
					}
				}
			}

			private static bool IsEntityInRadius<T>(Vector3 position, float radiusSq, T entity) where T : BaseEntity
			{
				//IL_0024: Unknown result type (might be due to invalid IL or missing references)
				//IL_0029: Unknown result type (might be due to invalid IL or missing references)
				//IL_002c: Unknown result type (might be due to invalid IL or missing references)
				//IL_002d: Unknown result type (might be due to invalid IL or missing references)
				//IL_0032: Unknown result type (might be due to invalid IL or missing references)
				//IL_0033: Unknown result type (might be due to invalid IL or missing references)
				//IL_0038: Unknown result type (might be due to invalid IL or missing references)
				using (TimeWarning.New("IsEntityInRadius"))
				{
					if ((Object)(object)entity == (Object)null)
					{
						return false;
					}
					OBB val = entity.WorldSpaceBounds();
					Vector3 val2 = ((OBB)(ref val)).ClosestPoint(position) - position;
					return ((Vector3)(ref val2)).sqrMagnitude < radiusSq;
				}
			}
		}

		public static EntityTree Server;
	}

	public class RPC_Shared : Attribute
	{
	}

	public struct RPCMessage
	{
		public Connection connection;

		public BasePlayer player;

		public NetRead read;
	}

	public class RPC_Server : RPC_Shared
	{
		public abstract class Conditional : Attribute
		{
			public virtual string GetArgs()
			{
				return null;
			}
		}

		public class MaxDistance : Conditional
		{
			private float maximumDistance;

			public bool CheckParent { get; set; }

			public MaxDistance(float maxDist)
			{
				maximumDistance = maxDist;
			}

			public override string GetArgs()
			{
				return maximumDistance.ToString("0.00f") + (CheckParent ? ", true" : "");
			}

			public static bool Test(uint id, string debugName, BaseEntity ent, BasePlayer player, float maximumDistance, bool checkParent = false)
			{
				//IL_004d: Unknown result type (might be due to invalid IL or missing references)
				//IL_007d: Unknown result type (might be due to invalid IL or missing references)
				if ((Object)(object)ent == (Object)null || (Object)(object)player == (Object)null)
				{
					return false;
				}
				object obj = Interface.CallHook("OnEntityDistanceCheck", ent, player, id, debugName, maximumDistance, checkParent);
				if (obj is bool)
				{
					return (bool)obj;
				}
				bool flag = ent.Distance(player.eyes.position) <= maximumDistance;
				if (checkParent && !flag)
				{
					BaseEntity parentEntity = ent.GetParentEntity();
					flag = (Object)(object)parentEntity != (Object)null && parentEntity.Distance(player.eyes.position) <= maximumDistance;
				}
				return flag;
			}
		}

		public class IsVisible : Conditional
		{
			private float maximumDistance;

			public IsVisible(float maxDist)
			{
				maximumDistance = maxDist;
			}

			public override string GetArgs()
			{
				return maximumDistance.ToString("0.00f");
			}

			public static bool Test(uint id, string debugName, BaseEntity ent, BasePlayer player, float maximumDistance)
			{
				//IL_0045: Unknown result type (might be due to invalid IL or missing references)
				//IL_0050: Unknown result type (might be due to invalid IL or missing references)
				//IL_0069: Unknown result type (might be due to invalid IL or missing references)
				//IL_0083: Unknown result type (might be due to invalid IL or missing references)
				if ((Object)(object)ent == (Object)null || (Object)(object)player == (Object)null)
				{
					return false;
				}
				object obj = Interface.CallHook("OnEntityVisibilityCheck", ent, player, id, debugName, maximumDistance);
				if (obj is bool)
				{
					return (bool)obj;
				}
				if (GamePhysics.LineOfSight(player.eyes.center, player.eyes.position, 1218519041))
				{
					if (!ent.IsVisible(player.eyes.HeadRay(), 1218519041, maximumDistance))
					{
						return ent.IsVisible(player.eyes.position, maximumDistance);
					}
					return true;
				}
				return false;
			}
		}

		public class FromOwner : Conditional
		{
			public static bool Test(uint id, string debugName, BaseEntity ent, BasePlayer player)
			{
				//IL_0050: Unknown result type (might be due to invalid IL or missing references)
				//IL_005b: Unknown result type (might be due to invalid IL or missing references)
				//IL_006f: Unknown result type (might be due to invalid IL or missing references)
				//IL_007a: Unknown result type (might be due to invalid IL or missing references)
				//IL_009c: Unknown result type (might be due to invalid IL or missing references)
				//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
				if ((Object)(object)ent == (Object)null || (Object)(object)player == (Object)null)
				{
					return false;
				}
				if (ent.net == null || player.net == null)
				{
					return false;
				}
				object obj = Interface.CallHook("OnEntityFromOwnerCheck", ent, player, id, debugName);
				if (obj is bool)
				{
					return (bool)obj;
				}
				if (ent.net.ID == player.net.ID)
				{
					return true;
				}
				if (ent.parentEntity.uid != player.net.ID)
				{
					BaseEntity parentEntity = ent.GetParentEntity();
					if ((Object)(object)parentEntity != (Object)null && parentEntity.parentEntity.uid == player.net.ID)
					{
						return true;
					}
					return false;
				}
				return true;
			}
		}

		public class IsActiveItem : Conditional
		{
			public static bool Test(uint id, string debugName, BaseEntity ent, BasePlayer player)
			{
				//IL_0050: Unknown result type (might be due to invalid IL or missing references)
				//IL_005b: Unknown result type (might be due to invalid IL or missing references)
				//IL_006f: Unknown result type (might be due to invalid IL or missing references)
				//IL_007a: Unknown result type (might be due to invalid IL or missing references)
				if ((Object)(object)ent == (Object)null || (Object)(object)player == (Object)null)
				{
					return false;
				}
				if (ent.net == null || player.net == null)
				{
					return false;
				}
				object obj = Interface.CallHook("OnEntityActiveCheck", ent, player, id, debugName);
				if (obj is bool)
				{
					return (bool)obj;
				}
				if (ent.net.ID == player.net.ID)
				{
					return true;
				}
				if (ent.parentEntity.uid != player.net.ID)
				{
					return false;
				}
				Item activeItem = player.GetActiveItem();
				if (activeItem == null)
				{
					return false;
				}
				if ((Object)(object)activeItem.GetHeldEntity() != (Object)(object)ent)
				{
					return false;
				}
				return true;
			}
		}

		public class FromMounted : Conditional
		{
			public static bool Test(uint id, string debugName, BaseEntity ent, BasePlayer player)
			{
				//IL_007a: Unknown result type (might be due to invalid IL or missing references)
				//IL_008b: Unknown result type (might be due to invalid IL or missing references)
				//IL_0090: Unknown result type (might be due to invalid IL or missing references)
				//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
				//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
				if ((Object)(object)ent == (Object)null || (Object)(object)player == (Object)null)
				{
					return false;
				}
				if (ent.net == null || player.net == null)
				{
					return false;
				}
				BaseMountable baseMountable = ent as BaseMountable;
				if ((Object)(object)baseMountable == (Object)null)
				{
					baseMountable = ent.parentEntity.Get(serverside: true) as BaseMountable;
				}
				if ((Object)(object)baseMountable != (Object)null)
				{
					NetworkableId? val = baseMountable.GetMounted()?.net?.ID;
					NetworkableId iD = player.net.ID;
					if (val.HasValue && (!val.HasValue || val.GetValueOrDefault() == iD))
					{
						return true;
					}
				}
				return false;
			}
		}

		public class FromOwnerOrMounted : Conditional
		{
			public static bool Test(uint id, string debugName, BaseEntity ent, BasePlayer player)
			{
				if (FromOwner.Test(id, debugName, ent, player))
				{
					return true;
				}
				return FromMounted.Test(id, debugName, ent, player);
			}
		}

		public class InputValidation : Conditional
		{
			private Type[] validTypes;

			public Type[] ValidTypes => validTypes;

			public static bool Test(float f)
			{
				return !FloatEx.IsNaNOrInfinity(f);
			}

			public static bool Test(Vector3 v)
			{
				//IL_0000: Unknown result type (might be due to invalid IL or missing references)
				//IL_000d: Unknown result type (might be due to invalid IL or missing references)
				//IL_001a: Unknown result type (might be due to invalid IL or missing references)
				if (Test(v.x) && Test(v.y))
				{
					return Test(v.z);
				}
				return false;
			}

			public static bool Test(Vector2 v)
			{
				//IL_0000: Unknown result type (might be due to invalid IL or missing references)
				//IL_000d: Unknown result type (might be due to invalid IL or missing references)
				if (Test(v.x))
				{
					return Test(v.y);
				}
				return false;
			}

			public static bool Test(Quaternion q)
			{
				//IL_0000: Unknown result type (might be due to invalid IL or missing references)
				//IL_000d: Unknown result type (might be due to invalid IL or missing references)
				//IL_001a: Unknown result type (might be due to invalid IL or missing references)
				//IL_0027: Unknown result type (might be due to invalid IL or missing references)
				if (Test(q.x) && Test(q.y) && Test(q.z))
				{
					return Test(q.w);
				}
				return false;
			}

			public InputValidation(params Type[] types)
			{
				validTypes = types;
			}
		}

		public class MaxRepeatedElements : Attribute
		{
			public int MaximumElements { get; }

			public MaxRepeatedElements(int maximumElements)
			{
				MaximumElements = maximumElements;
			}
		}

		public class IgnoreConditional : Attribute
		{
			public string functionName;

			public Type[] ignoredAttributeTypes;

			public IgnoreConditional(string testFunc, params Type[] ignoredAttributes)
			{
				functionName = testFunc;
				ignoredAttributeTypes = ignoredAttributes;
			}
		}

		public class IgnoreProtoFieldOrder : Attribute
		{
		}

		public class IgnoreProtoFieldOperationLimit : Attribute
		{
		}

		public class CallsPerSecond : Conditional
		{
			private ulong callsPerSecond;

			public CallsPerSecond(ulong limit)
			{
				callsPerSecond = limit;
			}

			public override string GetArgs()
			{
				return callsPerSecond.ToString();
			}

			public static bool Test(uint id, string debugName, BaseEntity ent, BasePlayer player, ulong callsPerSecond)
			{
				if ((Object)(object)ent == (Object)null || (Object)(object)player == (Object)null)
				{
					return false;
				}
				return player.rpcHistory.TryIncrement(id, callsPerSecond);
			}
		}
	}

	public struct BaseEntityPreserveInfo
	{
		public Vector3 localPosition;

		public Quaternion localRotation;

		public BaseEntity parent;

		public EntityRef[] slots;

		public ulong ownerID;

		public List<ChildPreserveInfo> childPreserveInfos;
	}

	public struct ChildPreserveInfo
	{
		public BaseEntity targetEntity;

		public uint targetBone;

		public Vector3 localPosition;

		public Quaternion localRotation;

		public string targetSocketName;
	}

	public enum Signal
	{
		Attack,
		Alt_Attack,
		DryFire,
		Reload,
		Deploy,
		Flinch_Head,
		Flinch_Chest,
		Flinch_Stomach,
		Flinch_RearHead,
		Flinch_RearTorso,
		Throw,
		Relax,
		Gesture,
		PhysImpact,
		Eat,
		Startled,
		Admire
	}

	public enum Slot
	{
		Lock,
		FireMod,
		UpperModifier,
		MiddleModifier,
		LowerModifier,
		CenterDecoration,
		LowerCenterDecoration,
		StorageMonitor,
		Count
	}

	[Flags]
	public enum TraitFlag
	{
		None = 0,
		Alive = 1,
		Animal = 2,
		Human = 4,
		Interesting = 8,
		Food = 0x10,
		Meat = 0x20,
		Water = Meat
	}

	public static class Util
	{
		public static BaseEntity[] FindTargets(string strFilter, bool onlyPlayers)
		{
			return (from x in BaseNetworkable.serverEntities.Where(delegate(BaseNetworkable x)
				{
					if (x is BasePlayer)
					{
						BasePlayer basePlayer = x as BasePlayer;
						if (string.IsNullOrEmpty(strFilter))
						{
							return true;
						}
						if (strFilter == "!alive" && basePlayer.IsAlive())
						{
							return true;
						}
						if (strFilter == "!sleeping" && basePlayer.IsSleeping())
						{
							return true;
						}
						if (strFilter[0] != '!' && !StringEx.Contains(basePlayer.displayName, strFilter, CompareOptions.IgnoreCase) && !basePlayer.UserIDString.Contains(strFilter))
						{
							return false;
						}
						return true;
					}
					if (onlyPlayers)
					{
						return false;
					}
					if (string.IsNullOrEmpty(strFilter))
					{
						return false;
					}
					return x.ShortPrefabName.Contains(strFilter) ? true : false;
				})
				select x as BaseEntity).ToArray();
		}

		public static BaseEntity[] FindTargetsOwnedBy(ulong ownedBy, string strFilter)
		{
			bool hasFilter = !string.IsNullOrEmpty(strFilter);
			return (from x in BaseNetworkable.serverEntities.Where(delegate(BaseNetworkable x)
				{
					if (x is BaseEntity baseEntity)
					{
						if (baseEntity.OwnerID != ownedBy)
						{
							return false;
						}
						if (!hasFilter || baseEntity.ShortPrefabName.Contains(strFilter))
						{
							return true;
						}
					}
					return false;
				})
				select x as BaseEntity).ToArray();
		}

		public static BaseEntity[] FindTargetsAuthedTo(ulong authId, string strFilter)
		{
			bool hasFilter = !string.IsNullOrEmpty(strFilter);
			return (from x in BaseNetworkable.serverEntities.Where(delegate(BaseNetworkable x)
				{
					if (x is BuildingPrivlidge buildingPrivlidge)
					{
						if (!buildingPrivlidge.IsAuthed(authId))
						{
							return false;
						}
						if (!hasFilter || x.ShortPrefabName.Contains(strFilter))
						{
							return true;
						}
					}
					else if (x is SimplePrivilege simplePrivilege)
					{
						if (!simplePrivilege.IsAuthed(authId))
						{
							return false;
						}
						if (!hasFilter || x.ShortPrefabName.Contains(strFilter))
						{
							return true;
						}
					}
					else if (x is AutoTurret autoTurret)
					{
						if (!autoTurret.IsAuthed(authId))
						{
							return false;
						}
						if (!hasFilter || x.ShortPrefabName.Contains(strFilter))
						{
							return true;
						}
					}
					else if (x is CodeLock codeLock)
					{
						if (!codeLock.whitelistPlayers.Contains(authId))
						{
							return false;
						}
						if (!hasFilter || x.ShortPrefabName.Contains(strFilter))
						{
							return true;
						}
					}
					else if (x is KeyLock keyLock)
					{
						if (keyLock.OwnerID != authId)
						{
							return false;
						}
						if (!hasFilter || x.ShortPrefabName.Contains(strFilter))
						{
							return true;
						}
					}
					else if (x is ModularCar modularCar)
					{
						if (!modularCar.IsLockable || !modularCar.CarLock.HasLockPermission(authId))
						{
							return false;
						}
						if (!hasFilter || x.ShortPrefabName.Contains(strFilter))
						{
							return true;
						}
					}
					return false;
				})
				select x as BaseEntity).ToArray();
		}

		public static T[] FindAll<T>() where T : BaseEntity
		{
			return BaseNetworkable.serverEntities.OfType<T>().ToArray();
		}
	}

	[Flags]
	public enum Axis : byte
	{
		None = 0,
		X = 1,
		Y = 2,
		Z = 4,
		XY = X | Y,
		XZ = X | Z,
		YZ = Y | Z,
		XYZ = XY | Z
	}

	public enum GiveItemReason
	{
		Generic,
		ResourceHarvested,
		PickedUp,
		Crafted
	}

	private static Queue<BaseEntity> globalBroadcastQueue = new Queue<BaseEntity>();

	private static uint globalBroadcastProtocol = 0u;

	private uint broadcastProtocol;

	public List<EntityLink> links;

	private bool linkedToNeighbours;

	internal const int FileRequestMinimumCost = 32768;

	private TimeUntil _transferProtectionRemaining;

	private Action _disableTransferProtectionAction;

	private float cachedBuildingPrivilegeTime;

	private BuildingPrivlidge cachedBuildingPrivilege;

	private Vector3 cachedBuildingPrivilegePosition;

	public const string RpcClientDeprecationNotice = "Use ClientRPC( RpcTarget ) overloads";

	private static bool transferProtectedRpcsResolved;

	private static uint clientLoadingCompleteRpc;

	private static uint clientKeepConnectionAliveRpc;

	[NonSerialized]
	public BaseEntity creatorEntity;

	private bool couldSaveOriginally;

	public int ticksSinceStopped;

	public bool isCallingUpdateNetworkGroup;

	private Action _updateNetworkGroupCallback;

	private int oldPosLSFrame;

	private Vector3 oldPosLS;

	private Axis hasMovedLS;

	private const float EpsilonSqr = 9.9999994E-11f;

	private EntityRef[] entitySlots;

	private const float SYNC_VAR_QUEUE_UPDATE_INTERVAL = 0.0333f;

	private const int SYNC_VAR_QUEUE_MAX_SIZE = 32;

	private uint _serverSyncVarQueue;

	private Action _sendPackedSyncVarQueueAction;

	protected List<TriggerBase> triggers;

	private Action _forceUpdateTriggersCallback;

	protected bool isVisible;

	protected bool isAnimatorVisible;

	protected bool isShadowVisible;

	protected OccludeeSphere localOccludee;

	[Header("BaseEntity")]
	public Bounds bounds;

	public GameObjectRef impactEffect;

	public bool enableSaving;

	public bool syncPosition;

	public Model model;

	public Flags flags;

	[NonSerialized]
	public uint parentBone;

	[NonSerialized]
	public ulong skinID;

	[NonSerialized]
	public ulong attachmentID;

	private List<EntityComponentBase> _components;

	[HideInInspector]
	public bool HasBrain;

	private float nextHeightCheckTime;

	private bool cachedUnderground;

	[NonSerialized]
	public string _name;

	[NonSerialized]
	public bool networkEntityScale;

	public Spawnable _spawnable;

	protected static ExactArrayPool<byte> _autosaveBufferPool = new ExactArrayPool<byte>();

	protected byte[] _autosaveBuffer;

	public static HashSet<BaseEntity> saveList = new HashSet<BaseEntity>();

	public virtual float RealisticMass => 100f;

	protected float TransferProtectionRemaining
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return TimeUntil.op_Implicit(_transferProtectionRemaining);
		}
	}

	protected Action DisableTransferProtectionAction => _disableTransferProtectionAction ?? (_disableTransferProtectionAction = DisableTransferProtection);

	public virtual bool PreserveChildrenWhenReskinning => false;

	public float radiationLevel
	{
		get
		{
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			if (triggers == null)
			{
				return 0f;
			}
			float num = 0f;
			for (int i = 0; i < triggers.Count; i++)
			{
				TriggerRadiation triggerRadiation = triggers[i] as TriggerRadiation;
				if (!((Object)(object)triggerRadiation == (Object)null))
				{
					Vector3 val = GetNetworkPosition();
					BaseEntity baseEntity = GetParentEntity();
					if ((Object)(object)baseEntity != (Object)null)
					{
						val = ((Component)baseEntity).transform.TransformPoint(val);
					}
					num = Mathf.Max(num, triggerRadiation.GetRadiationForPosition(val, RadiationProtection(), this));
				}
			}
			return num;
		}
	}

	public float currentTemperature
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			float num = Climate.GetTemperature(((Component)this).transform.position);
			if (triggers == null)
			{
				return num;
			}
			for (int i = 0; i < triggers.Count; i++)
			{
				TriggerTemperature triggerTemperature = triggers[i] as TriggerTemperature;
				if (!((Object)(object)triggerTemperature == (Object)null))
				{
					num = triggerTemperature.WorkoutTemperature(((Component)this).transform.position, num);
				}
			}
			return num;
		}
	}

	public float currentEnvironmentalWetness
	{
		get
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			if (triggers == null)
			{
				return 0f;
			}
			float num = 0f;
			Vector3 networkPosition = GetNetworkPosition();
			foreach (TriggerBase trigger in triggers)
			{
				if (trigger is TriggerWetness triggerWetness)
				{
					num += triggerWetness.WorkoutWetness(networkPosition);
				}
			}
			return Mathf.Clamp01(num);
		}
	}

	public virtual float PositionTickRate => 0.1f;

	public virtual bool PositionTickFixedTime => false;

	public Action NetworkPosTickCallback { get; protected set; }

	public virtual Vector3 ServerPosition
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return ((Component)this).transform.localPosition;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			if (!(((Component)this).transform.localPosition == value))
			{
				((Component)this).transform.localPosition = value;
				((Component)this).transform.hasChanged = true;
			}
		}
	}

	public virtual Vector3 ServerWorldPosition
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return ((Component)this).transform.position;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			if (!(((Component)this).transform.position == value))
			{
				((Component)this).transform.position = value;
				((Component)this).transform.hasChanged = true;
			}
		}
	}

	public virtual Quaternion ServerRotation
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return ((Component)this).transform.localRotation;
		}
		set
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			if (!(((Component)this).transform.localRotation == value))
			{
				((Component)this).transform.localRotation = value;
				((Component)this).transform.hasChanged = true;
			}
		}
	}

	public virtual Vector3 ServerNavMeshPos
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return ServerWorldPosition;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			ServerWorldPosition = value;
		}
	}

	public virtual Matrix4x4 WorldToNavMeshSpace
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return Matrix4x4.identity;
		}
	}

	public virtual Matrix4x4 NavMeshToWorldSpace
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return Matrix4x4.identity;
		}
	}

	bool IPrefabPreProcess.CanRunDuringBundling => false;

	public virtual TraitFlag Traits => TraitFlag.None;

	public bool IsForceUpdatingTriggers { get; private set; }

	public float Weight { get; protected set; }

	public List<EntityComponentBase> Components
	{
		get
		{
			if (_components == null)
			{
				_components = new List<EntityComponentBase>();
				((Component)this).GetComponentsInChildren<EntityComponentBase>(true, _components);
			}
			return _components;
		}
	}

	public virtual bool IsNpc => false;

	public virtual bool AlsoVisCheckParent => false;

	public virtual bool VisibilityPassesThroughParent => false;

	public ulong OwnerID { get; set; }

	public virtual bool ShouldTransferAssociatedFiles => false;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("BaseEntity.OnRpcMessage"))
		{
			if (rpc == 1552640099 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - BroadcastSignalFromClient"));
				}
				using (TimeWarning.New("BroadcastSignalFromClient"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.FromOwnerOrMounted.Test(1552640099u, "BroadcastSignalFromClient", this, player))
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
							BroadcastSignalFromClient(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in BroadcastSignalFromClient");
					}
				}
				return true;
			}
			if (rpc == 3645147041u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SV_RequestFile"));
				}
				using (TimeWarning.New("SV_RequestFile"))
				{
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
							SV_RequestFile(msg3);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in SV_RequestFile");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public virtual void OnCollision(Collision collision, BaseEntity hitEntity)
	{
		throw new NotImplementedException();
	}

	protected void ReceiveCollisionMessages(bool b)
	{
		if (b)
		{
			((Component)this).gameObject.transform.GetOrAddComponent<EntityCollisionMessage>();
		}
		else
		{
			UnityEngine.TransformEx.RemoveComponent<EntityCollisionMessage>(((Component)this).gameObject.transform);
		}
	}

	public virtual void DebugServer(int rep, float time)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		DebugText(((Component)this).transform.position + Vector3.up * 1f, $"{net?.ID.Value ?? 0}: {((Object)this).name}\n{DebugText()}", Color.white, time);
	}

	public virtual string DebugText()
	{
		return "";
	}

	public void OnDebugStart()
	{
		EntityDebug entityDebug = ((Component)this).gameObject.GetComponent<EntityDebug>();
		if ((Object)(object)entityDebug == (Object)null)
		{
			entityDebug = ((Component)this).gameObject.AddComponent<EntityDebug>();
		}
		((Behaviour)entityDebug).enabled = true;
	}

	protected void DebugText(Vector3 pos, string str, Color color, float time)
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		if (base.isServer)
		{
			ConsoleNetwork.BroadcastToAllClients("ddraw.text", time, color, pos, str);
		}
	}

	public bool HasFlag(Flags f)
	{
		return (flags & f) == f;
	}

	public bool HasAnyFlag(Flags f)
	{
		return (flags & f) > (Flags)0;
	}

	public bool ParentHasFlag(Flags f)
	{
		BaseEntity baseEntity = GetParentEntity();
		if ((Object)(object)baseEntity == (Object)null)
		{
			return false;
		}
		return baseEntity.HasFlag(f);
	}

	public bool IsOn()
	{
		return HasFlag(Flags.On);
	}

	public bool IsOpen()
	{
		return HasFlag(Flags.Open);
	}

	public bool IsOnFire()
	{
		return HasFlag(Flags.OnFire);
	}

	public bool IsLocked()
	{
		return HasFlag(Flags.Locked);
	}

	public override bool IsDebugging()
	{
		return HasFlag(Flags.Debugging);
	}

	public bool IsDisabled()
	{
		if (!HasFlag(Flags.Disabled))
		{
			return ParentHasFlag(Flags.Disabled);
		}
		return true;
	}

	public bool IsBroken()
	{
		return HasFlag(Flags.Broken);
	}

	public bool IsBusy()
	{
		return HasFlag(Flags.Busy);
	}

	public bool IsTransferProtected()
	{
		return HasFlag(Flags.Protected);
	}

	public bool IsTransferring()
	{
		return HasFlag(Flags.Transferring);
	}

	public override string GetLogColor()
	{
		if (base.isServer)
		{
			return "cyan";
		}
		return "yellow";
	}

	public virtual void OnFlagsChanged(Flags old, Flags next)
	{
		if (IsDebugging() && (old & Flags.Debugging) != (next & Flags.Debugging))
		{
			OnDebugStart();
		}
		if (base.isServer)
		{
			if ((next & Flags.OnFire) == Flags.OnFire && (old & Flags.OnFire) != Flags.OnFire)
			{
				SingletonComponent<NpcFireManager>.Instance.Add(this);
			}
			else if ((next & Flags.OnFire) != Flags.OnFire && (old & Flags.OnFire) == Flags.OnFire)
			{
				SingletonComponent<NpcFireManager>.Instance.Remove(this);
			}
		}
	}

	public void SetFlagLocal(Flags f, bool b, bool recursive = false)
	{
		Flags old = flags;
		if (b)
		{
			if (HasFlag(f))
			{
				return;
			}
			flags |= f;
		}
		else
		{
			if (!HasFlag(f))
			{
				return;
			}
			flags &= ~f;
		}
		OnFlagsChanged(old, flags);
		InvalidateNetworkCache();
		if (recursive && children != null)
		{
			int i = 0;
			for (int count = children.Count; i < count; i++)
			{
				children[i].SetFlagLocal(f, b, recursive: true);
			}
		}
	}

	public FlagsUpdateScope StartSetFlags(FlagsUpdateMode updateMode)
	{
		return new FlagsUpdateScope(this, updateMode);
	}

	private void HandleFlagsUpdateMode(FlagsUpdateMode updateMode)
	{
		switch (updateMode)
		{
		case FlagsUpdateMode.Local:
			InvalidateNetworkCache();
			break;
		case FlagsUpdateMode.SendNetworkUpdate_Flags:
			InvalidateNetworkCache();
			SendNetworkUpdate_Flags();
			break;
		case FlagsUpdateMode.SendNetworkUpdate:
			SendNetworkUpdate();
			GlobalNetworkHandler.server?.TrySendNetworkUpdate(this);
			break;
		case FlagsUpdateMode.SendNetworkUpdateImmediate:
			SendNetworkUpdateImmediate();
			GlobalNetworkHandler.server?.TrySendNetworkUpdate(this);
			break;
		}
	}

	public void SendNetworkUpdate_Flags()
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		if (Application.isLoading || Application.isLoadingSave || base.IsDestroyed || net == null || !isSpawned)
		{
			return;
		}
		using (TimeWarning.New("SendNetworkUpdate_Flags"))
		{
			LogEntry(RustLog.EntryType.Network, 3, "SendNetworkUpdate_Flags");
			if (Interface.CallHook("OnEntityFlagsNetworkUpdate", this) == null)
			{
				List<Connection> subscribers = GetSubscribers();
				if (subscribers != null && subscribers.Count > 0)
				{
					NetWrite netWrite = Net.sv.StartWrite();
					netWrite.PacketID(Message.Type.EntityFlags);
					netWrite.EntityID(net.ID);
					netWrite.Int32((int)flags);
					SendInfo info = new SendInfo(subscribers);
					netWrite.Send(info);
				}
				((Component)this).gameObject.SendOnSendNetworkUpdate(this);
			}
		}
	}

	public virtual bool IsOccupied(Socket_Base socket)
	{
		return FindLink(socket)?.IsOccupied() ?? false;
	}

	public bool IsOccupied(string socketName)
	{
		return FindLink(socketName)?.IsOccupied() ?? false;
	}

	public EntityLink FindLink(Socket_Base socket)
	{
		List<EntityLink> entityLinks = GetEntityLinks();
		for (int i = 0; i < entityLinks.Count; i++)
		{
			if (entityLinks[i].socket == socket)
			{
				return entityLinks[i];
			}
		}
		return null;
	}

	public EntityLink FindLink(string socketName)
	{
		List<EntityLink> entityLinks = GetEntityLinks();
		for (int i = 0; i < entityLinks.Count; i++)
		{
			if (entityLinks[i].socket.socketName == socketName)
			{
				return entityLinks[i];
			}
		}
		return null;
	}

	public EntityLink FindLink(string[] socketNames)
	{
		List<EntityLink> entityLinks = GetEntityLinks();
		for (int i = 0; i < entityLinks.Count; i++)
		{
			for (int j = 0; j < socketNames.Length; j++)
			{
				if (entityLinks[i].socket.socketName == socketNames[j])
				{
					return entityLinks[i];
				}
			}
		}
		return null;
	}

	public T FindLinkedEntity<T>() where T : BaseEntity
	{
		List<EntityLink> entityLinks = GetEntityLinks();
		for (int i = 0; i < entityLinks.Count; i++)
		{
			EntityLink entityLink = entityLinks[i];
			for (int j = 0; j < entityLink.connections.Count; j++)
			{
				EntityLink entityLink2 = entityLink.connections[j];
				if (entityLink2.owner is T)
				{
					return entityLink2.owner as T;
				}
			}
		}
		return null;
	}

	public void EntityLinkMessage<T>(Action<T> action) where T : BaseEntity
	{
		List<EntityLink> entityLinks = GetEntityLinks();
		for (int i = 0; i < entityLinks.Count; i++)
		{
			EntityLink entityLink = entityLinks[i];
			for (int j = 0; j < entityLink.connections.Count; j++)
			{
				EntityLink entityLink2 = entityLink.connections[j];
				if (entityLink2.owner is T)
				{
					action(entityLink2.owner as T);
				}
			}
		}
	}

	public void EntityLinkMessage<T, TCaller, TArg>(Action<T, TCaller, TArg> action, TCaller caller, TArg arg) where T : BaseEntity
	{
		List<EntityLink> entityLinks = GetEntityLinks();
		for (int i = 0; i < entityLinks.Count; i++)
		{
			EntityLink entityLink = entityLinks[i];
			for (int j = 0; j < entityLink.connections.Count; j++)
			{
				EntityLink entityLink2 = entityLink.connections[j];
				if (entityLink2.owner is T)
				{
					action(entityLink2.owner as T, caller, arg);
				}
			}
		}
	}

	public void EntityLinkBroadcast<T, S>(Action<T> action, Func<S, bool> canTraverseSocket) where T : BaseEntity where S : Socket_Base
	{
		globalBroadcastProtocol++;
		globalBroadcastQueue.Clear();
		broadcastProtocol = globalBroadcastProtocol;
		globalBroadcastQueue.Enqueue(this);
		if (this is T)
		{
			action(this as T);
		}
		while (globalBroadcastQueue.Count > 0)
		{
			List<EntityLink> entityLinks = globalBroadcastQueue.Dequeue().GetEntityLinks();
			for (int i = 0; i < entityLinks.Count; i++)
			{
				EntityLink entityLink = entityLinks[i];
				if (!(entityLink.socket is S) || !canTraverseSocket(entityLink.socket as S))
				{
					continue;
				}
				for (int j = 0; j < entityLink.connections.Count; j++)
				{
					BaseEntity owner = entityLink.connections[j].owner;
					if (owner.broadcastProtocol != globalBroadcastProtocol)
					{
						owner.broadcastProtocol = globalBroadcastProtocol;
						globalBroadcastQueue.Enqueue(owner);
						if (owner is T)
						{
							action(owner as T);
						}
					}
				}
			}
		}
	}

	public void EntityLinkBroadcast<T>(Action<T> action) where T : BaseEntity
	{
		globalBroadcastProtocol++;
		globalBroadcastQueue.Clear();
		broadcastProtocol = globalBroadcastProtocol;
		globalBroadcastQueue.Enqueue(this);
		if (this is T)
		{
			action(this as T);
		}
		while (globalBroadcastQueue.Count > 0)
		{
			List<EntityLink> entityLinks = globalBroadcastQueue.Dequeue().GetEntityLinks();
			for (int i = 0; i < entityLinks.Count; i++)
			{
				EntityLink entityLink = entityLinks[i];
				for (int j = 0; j < entityLink.connections.Count; j++)
				{
					BaseEntity owner = entityLink.connections[j].owner;
					if (owner.broadcastProtocol != globalBroadcastProtocol)
					{
						owner.broadcastProtocol = globalBroadcastProtocol;
						globalBroadcastQueue.Enqueue(owner);
						if (owner is T)
						{
							action(owner as T);
						}
					}
				}
			}
		}
	}

	public void EntityLinkBroadcast<T, TArg>(Action<T, TArg> action, TArg arg) where T : BaseEntity
	{
		globalBroadcastProtocol++;
		globalBroadcastQueue.Clear();
		broadcastProtocol = globalBroadcastProtocol;
		globalBroadcastQueue.Enqueue(this);
		if (this is T)
		{
			action(this as T, arg);
		}
		while (globalBroadcastQueue.Count > 0)
		{
			List<EntityLink> entityLinks = globalBroadcastQueue.Dequeue().GetEntityLinks();
			for (int i = 0; i < entityLinks.Count; i++)
			{
				EntityLink entityLink = entityLinks[i];
				for (int j = 0; j < entityLink.connections.Count; j++)
				{
					BaseEntity owner = entityLink.connections[j].owner;
					if (owner.broadcastProtocol != globalBroadcastProtocol)
					{
						owner.broadcastProtocol = globalBroadcastProtocol;
						globalBroadcastQueue.Enqueue(owner);
						if (owner is T)
						{
							action(owner as T, arg);
						}
					}
				}
			}
		}
	}

	public void EntityLinkBroadcast()
	{
		globalBroadcastProtocol++;
		globalBroadcastQueue.Clear();
		broadcastProtocol = globalBroadcastProtocol;
		globalBroadcastQueue.Enqueue(this);
		while (globalBroadcastQueue.Count > 0)
		{
			List<EntityLink> entityLinks = globalBroadcastQueue.Dequeue().GetEntityLinks();
			for (int i = 0; i < entityLinks.Count; i++)
			{
				EntityLink entityLink = entityLinks[i];
				for (int j = 0; j < entityLink.connections.Count; j++)
				{
					BaseEntity owner = entityLink.connections[j].owner;
					if (owner.broadcastProtocol != globalBroadcastProtocol)
					{
						owner.broadcastProtocol = globalBroadcastProtocol;
						globalBroadcastQueue.Enqueue(owner);
					}
				}
			}
		}
	}

	public bool ReceivedEntityLinkBroadcast()
	{
		return broadcastProtocol == globalBroadcastProtocol;
	}

	public List<EntityLink> GetEntityLinks(bool linkToNeighbours = true)
	{
		if (Application.isLoadingSave)
		{
			return links;
		}
		if (!linkedToNeighbours & linkToNeighbours)
		{
			LinkToNeighbours();
		}
		return links;
	}

	private void LinkToEntity(BaseEntity other)
	{
		if ((Object)(object)this == (Object)(object)other || links.Count == 0 || other.links.Count == 0)
		{
			return;
		}
		using (TimeWarning.New("LinkToEntity"))
		{
			for (int i = 0; i < links.Count; i++)
			{
				EntityLink entityLink = links[i];
				for (int j = 0; j < other.links.Count; j++)
				{
					EntityLink entityLink2 = other.links[j];
					if (entityLink.CanConnect(entityLink2))
					{
						if (!entityLink.Contains(entityLink2))
						{
							entityLink.Add(entityLink2);
						}
						if (!entityLink2.Contains(entityLink))
						{
							entityLink2.Add(entityLink);
						}
					}
				}
			}
		}
	}

	private void LinkToNeighbours()
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (links.Count == 0)
		{
			return;
		}
		linkedToNeighbours = true;
		using (TimeWarning.New("LinkToNeighbours"))
		{
			List<BaseEntity> list = Pool.Get<List<BaseEntity>>();
			OBB val = WorldSpaceBounds();
			Vis.Entities(val.position, ((Vector3)(ref val.extents)).magnitude + 1f, list, -1, (QueryTriggerInteraction)2);
			for (int i = 0; i < list.Count; i++)
			{
				BaseEntity baseEntity = list[i];
				if (baseEntity.isServer == base.isServer)
				{
					LinkToEntity(baseEntity);
				}
			}
			Pool.FreeUnmanaged<BaseEntity>(ref list);
		}
	}

	private void InitEntityLinks()
	{
		using (TimeWarning.New("InitEntityLinks"))
		{
			if (base.isServer)
			{
				links.AddLinks(this, PrefabAttribute.server.FindAll<Socket_Base>(prefabID));
			}
		}
	}

	private void FreeEntityLinks()
	{
		using (TimeWarning.New("FreeEntityLinks"))
		{
			links.FreeLinks();
			linkedToNeighbours = false;
		}
	}

	public void RefreshEntityLinks()
	{
		using (TimeWarning.New("RefreshEntityLinks"))
		{
			links.ClearLinks();
			LinkToNeighbours();
		}
	}

	public MovementModify GetMovementModify()
	{
		MovementModify result = new MovementModify
		{
			drag = 0f
		};
		if (triggers == null)
		{
			return result;
		}
		foreach (TriggerBase trigger in triggers)
		{
			TriggerMovement triggerMovement = trigger as TriggerMovement;
			if (!((Object)(object)triggerMovement == (Object)null))
			{
				result.drag = Mathf.Max(triggerMovement.movementModify.drag * triggerMovement.GetMovementScale(), result.drag);
			}
		}
		return result;
	}

	[RPC_Server]
	public void SV_RequestFile(RPCMessage msg)
	{
		uint crc = msg.read.UInt32();
		FileStorage.Type type = (FileStorage.Type)msg.read.UInt8();
		string responseFunction = StringPool.Get(msg.read.UInt32());
		uint part = ((msg.read.Unread > 0) ? msg.read.UInt32() : 0u);
		bool respondIfNotFound = msg.read.Unread > 0 && msg.read.Bit();
		ServerFileRequestQueue.Request(msg.connection, this, ServerFileRequestQueue.RequestKind.GenericFile, crc, type, responseFunction, part, respondIfNotFound);
	}

	internal int SendRequestedFile(Connection connection, string funcName, uint crc, FileStorage.Type type, uint part, bool respondIfNotFound)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		byte[] array = FileStorage.server.Get(crc, type, net.ID, part);
		if (array == null)
		{
			if (!respondIfNotFound)
			{
				return 0;
			}
			array = Array.Empty<byte>();
		}
		SendInfo sendInfo = new SendInfo(connection);
		sendInfo.channel = 2;
		sendInfo.method = SendMethod.Reliable;
		SendInfo sendInfo2 = sendInfo;
		ClientRPC(RpcTarget.SendInfo(funcName, sendInfo2), crc, (uint)array.Length, array, part, (byte)type);
		return array.Length;
	}

	public virtual void EnableTransferProtection()
	{
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		if (!IsTransferProtected())
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Protected, b: true);
			}
			List<Connection> subscribers = GetSubscribers();
			if (subscribers != null)
			{
				List<Connection> list = Pool.Get<List<Connection>>();
				foreach (Connection item in subscribers)
				{
					if (!ShouldNetworkTo(item.player as BasePlayer))
					{
						list.Add(item);
					}
				}
				OnNetworkSubscribersLeave(list);
				Pool.FreeUnmanaged<Connection>(ref list);
			}
			float protectionDuration = Nexus.protectionDuration;
			_transferProtectionRemaining = TimeUntil.op_Implicit(protectionDuration);
			Invoke(DisableTransferProtectionAction, protectionDuration);
		}
		foreach (BaseEntity child in children)
		{
			child.EnableTransferProtection();
		}
	}

	public virtual void DisableTransferProtection()
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		BaseEntity baseEntity = GetParentEntity();
		if ((Object)(object)baseEntity != (Object)null && baseEntity.IsTransferProtected())
		{
			baseEntity.DisableTransferProtection();
		}
		if (IsTransferProtected())
		{
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Protected, b: false);
			}
			List<Connection> subscribers = GetSubscribers();
			if (subscribers != null)
			{
				OnNetworkSubscribersEnter(subscribers);
			}
			_transferProtectionRemaining = TimeUntil.op_Implicit(0f);
			CancelInvoke(DisableTransferProtectionAction);
		}
		foreach (BaseEntity child in children)
		{
			child.DisableTransferProtection();
		}
	}

	public void SetParent(BaseEntity entity, bool worldPositionStays = false, bool sendImmediate = false)
	{
		SetParent(entity, 0u, worldPositionStays, sendImmediate);
	}

	public void SetParent(BaseEntity entity, string strBone, bool worldPositionStays = false, bool sendImmediate = false)
	{
		SetParent(entity, (!string.IsNullOrEmpty(strBone)) ? StringPool.Get(strBone) : 0u, worldPositionStays, sendImmediate);
	}

	public bool HasChild(BaseEntity c)
	{
		if ((Object)(object)c == (Object)(object)this)
		{
			return true;
		}
		BaseEntity baseEntity = c.GetParentEntity();
		if ((Object)(object)baseEntity != (Object)null)
		{
			return HasChild(baseEntity);
		}
		return false;
	}

	public void SetParent(BaseEntity entity, uint boneID, bool worldPositionStays = false, bool sendImmediate = false)
	{
		if ((Object)(object)entity != (Object)null)
		{
			if ((Object)(object)entity == (Object)(object)this)
			{
				Debug.LogError((object)("Trying to parent to self " + (object)this), (Object)(object)((Component)this).gameObject);
				return;
			}
			if (HasChild(entity))
			{
				Debug.LogError((object)("Trying to parent to child " + (object)this), (Object)(object)((Component)this).gameObject);
				return;
			}
		}
		LogEntry(RustLog.EntryType.Hierarchy, 2, "SetParent {0} {1}", entity, boneID);
		BaseEntity baseEntity = GetParentEntity();
		if (Object.op_Implicit((Object)(object)baseEntity))
		{
			baseEntity.RemoveChild(this);
		}
		if (base.limitNetworking && (Object)(object)baseEntity != (Object)null && (Object)(object)baseEntity != (Object)(object)entity)
		{
			BasePlayer basePlayer = baseEntity as BasePlayer;
			if (basePlayer.IsValid())
			{
				DestroyOnClient(basePlayer.net.connection);
			}
		}
		if ((Object)(object)entity == (Object)null)
		{
			OnParentChanging(baseEntity, null);
			parentEntity.Set(null);
			((Component)this).transform.SetParent((Transform)null, worldPositionStays);
			parentBone = 0u;
			UpdateNetworkGroup();
			if (sendImmediate)
			{
				SendNetworkUpdateImmediate();
				SendChildrenNetworkUpdateImmediate();
			}
			else
			{
				SendNetworkUpdate();
				SendChildrenNetworkUpdate();
			}
			return;
		}
		Debug.Assert(entity.isServer, "SetParent - child should be a SERVER entity");
		Debug.Assert(entity.net != null, "Setting parent to entity that hasn't spawned yet! (net is null)");
		Debug.Assert(((NetworkableId)(ref entity.net.ID)).IsValid, "Setting parent to entity that hasn't spawned yet! (id = 0)");
		entity.AddChild(this);
		OnParentChanging(baseEntity, entity);
		parentEntity.Set(entity);
		if (boneID != 0 && boneID != StringPool.closest)
		{
			Transform val = entity.FindBone(StringPool.Get(boneID));
			ReparentToTargetBone reparentToTargetBone = default(ReparentToTargetBone);
			if ((Object)(object)val != (Object)null && ((Component)val).TryGetComponent<ReparentToTargetBone>(ref reparentToTargetBone) && (Object)(object)reparentToTargetBone.TargetBone != (Object)null)
			{
				uint num = StringPool.Get(((Object)reparentToTargetBone.TargetBone).name);
				if (num != 0)
				{
					boneID = num;
					val = reparentToTargetBone.TargetBone;
				}
			}
			((Component)this).transform.SetParent(((Object)(object)val != (Object)null) ? val : ((Component)entity).transform, worldPositionStays);
		}
		else
		{
			((Component)this).transform.SetParent(((Component)entity).transform, worldPositionStays);
		}
		parentBone = boneID;
		UpdateNetworkGroup();
		if (sendImmediate)
		{
			SendNetworkUpdateImmediate();
			SendChildrenNetworkUpdateImmediate();
		}
		else
		{
			SendNetworkUpdate();
			SendChildrenNetworkUpdate();
		}
	}

	public void DestroyOnClient(Connection connection)
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		if (children != null)
		{
			foreach (BaseEntity child in children)
			{
				child.DestroyOnClient(connection);
			}
		}
		if (Net.sv.IsConnected())
		{
			if (net.connection == connection && this is BasePlayer)
			{
				Debug.LogError((object)"Attempted to send EntityDestroy to connection's local player, this would cause chaos on the client. Skipping message");
				return;
			}
			NetWrite netWrite = Net.sv.StartWrite();
			netWrite.PacketID(Message.Type.EntityDestroy);
			netWrite.EntityID(net.ID);
			netWrite.UInt8(0);
			netWrite.Send(new SendInfo(connection));
			LogEntry(RustLog.EntryType.Network, 2, "EntityDestroy");
		}
	}

	public void SendChildrenNetworkUpdate()
	{
		if (children == null)
		{
			return;
		}
		foreach (BaseEntity child in children)
		{
			child.UpdateNetworkGroup();
			child.SendNetworkUpdate();
		}
	}

	public void SendChildrenNetworkUpdateImmediate()
	{
		if (children == null)
		{
			return;
		}
		foreach (BaseEntity child in children)
		{
			child.UpdateNetworkGroup();
			child.SendNetworkUpdateImmediate();
		}
	}

	public virtual void SwitchParent(BaseEntity ent)
	{
		Log("SwitchParent Missed " + (object)ent);
	}

	public virtual void OnParentChanging(BaseEntity oldParent, BaseEntity newParent)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		Rigidbody val = default(Rigidbody);
		if (!((Component)this).TryGetComponent<Rigidbody>(ref val) || !Object.op_Implicit((Object)(object)val) || val.isKinematic)
		{
			return;
		}
		if ((Object)(object)oldParent != (Object)null)
		{
			Rigidbody component = ((Component)oldParent).GetComponent<Rigidbody>();
			if ((Object)(object)component == (Object)null || component.isKinematic)
			{
				Rigidbody obj = val;
				obj.linearVelocity += oldParent.GetWorldVelocity();
			}
		}
		if ((Object)(object)newParent != (Object)null)
		{
			Rigidbody component2 = ((Component)newParent).GetComponent<Rigidbody>();
			if ((Object)(object)component2 == (Object)null || component2.isKinematic)
			{
				Rigidbody obj2 = val;
				obj2.linearVelocity -= newParent.GetWorldVelocity();
			}
		}
	}

	protected bool PrivilegeCacheDefaultValue()
	{
		return base.isClient;
	}

	protected static bool IsCacheValid(float cacheTime, float cacheDuration)
	{
		if (cacheTime != 0f)
		{
			return Time.time - cacheTime < cacheDuration;
		}
		return false;
	}

	protected static bool IsCacheValid(float cacheTime, float cacheDuration, Vector3 cachedPosition, Vector3 queryPosition)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		if (IsCacheValid(cacheTime, cacheDuration))
		{
			return cachedPosition == queryPosition;
		}
		return false;
	}

	public virtual EntityPrivilege GetEntityBuildingPrivilege()
	{
		return null;
	}

	public virtual BuildingPrivlidge GetBuildingPrivilege()
	{
		return GetNearestBuildingPrivilege(PrivilegeCacheDefaultValue());
	}

	public virtual BuildingPrivlidge GetBuildingPrivilege(bool cached, float cacheDuration = 1f)
	{
		return GetNearestBuildingPrivilege(cached, cacheDuration);
	}

	public BuildingPrivlidge GetNearestBuildingPrivilege()
	{
		return GetBuildingPrivilege(PrivilegeCacheDefaultValue());
	}

	public BuildingPrivlidge GetNearestBuildingPrivilege(bool cached, float cacheDuration = 1f)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return GetBuildingPrivilege(WorldSpaceBounds(), cached, cacheDuration);
	}

	public BuildingPrivlidge GetBuildingPrivilege(OBB obb)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return GetBuildingPrivilege(obb, PrivilegeCacheDefaultValue());
	}

	public BuildingPrivlidge GetBuildingPrivilege(OBB obb, bool cached, float cacheDuration = 1f, BuildingPrivlidge exclude = null)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_023b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		object obj = Interface.CallHook("OnBuildingPrivilege", this, obb, cached, cacheDuration, exclude);
		if (obj is BuildingPrivlidge)
		{
			return (BuildingPrivlidge)obj;
		}
		if (cached && IsCacheValid(cachedBuildingPrivilegeTime, cacheDuration, cachedBuildingPrivilegePosition, obb.position))
		{
			return cachedBuildingPrivilege;
		}
		BuildingBlock other = null;
		BuildingPrivlidge buildingPrivlidge = null;
		List<BuildingBlock> list = Pool.Get<List<BuildingBlock>>();
		Vis.Entities(obb.position, 16f + ((Vector3)(ref obb.extents)).magnitude, list, 2097152, (QueryTriggerInteraction)2);
		uint num = (((Object)(object)exclude != (Object)null) ? exclude.buildingID : 0u);
		for (int i = 0; i < list.Count; i++)
		{
			BuildingBlock buildingBlock = list[i];
			if (buildingBlock.isServer != base.isServer || !buildingBlock.IsOlderThan(other) || ((OBB)(ref obb)).Distance(buildingBlock.WorldSpaceBounds()) > 16f)
			{
				continue;
			}
			BuildingManager.Building building = buildingBlock.GetBuilding();
			if (building != null && (num == 0 || num != building.ID))
			{
				BuildingPrivlidge dominatingBuildingPrivilege = building.GetDominatingBuildingPrivilege();
				if (!((Object)(object)dominatingBuildingPrivilege == (Object)null))
				{
					other = buildingBlock;
					buildingPrivlidge = dominatingBuildingPrivilege;
				}
			}
		}
		Pool.FreeUnmanaged<BuildingBlock>(ref list);
		using (TimeWarning.New("InvisibleTC"))
		{
			if (BaseGameMode.GetActiveGameMode(base.isServer) is GameModeSoftcore && StorageContainer.dropCorpseOnDeath)
			{
				PooledList<BuildingPrivlidge> val = Pool.Get<PooledList<BuildingPrivlidge>>();
				try
				{
					BuildingPrivlidge.InvisibleAuthGrid.Query(obb.position.x, obb.position.z, 16f + ((Vector3)(ref obb.extents)).magnitude, (List<BuildingPrivlidge>)(object)val);
					foreach (BuildingPrivlidge item in (List<BuildingPrivlidge>)(object)val)
					{
						if (!((Object)(object)item == (Object)null) && item.isServer == base.isServer && (!((Object)(object)exclude != (Object)null) || !((Object)(object)item == (Object)(object)exclude)) && item.Distance(obb.position) < 16f && ((Object)(object)buildingPrivlidge == (Object)null || item.IsOlderThan(buildingPrivlidge)))
						{
							buildingPrivlidge = item;
						}
					}
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
		}
		cachedBuildingPrivilegeTime = Time.time;
		cachedBuildingPrivilege = buildingPrivlidge;
		cachedBuildingPrivilegePosition = obb.position;
		return cachedBuildingPrivilege;
	}

	public unsafe void SV_RPCMessage(uint nameID, Message message)
	{
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		Assert.IsTrue(base.isServer, "Should be server!");
		BasePlayer basePlayer = NetworkPacketEx.Player(message);
		if (!basePlayer.IsValid())
		{
			if (Global.developer > 0)
			{
				Debug.Log((object)("SV_RPCMessage: From invalid player " + (object)basePlayer));
			}
			return;
		}
		if (ConVar.AntiHack.rpcstallmode > 0 && basePlayer.isStalled)
		{
			if (Global.developer > 0)
			{
				Debug.Log((object)("SV_RPCMessage: player is stalled " + (object)basePlayer));
			}
			return;
		}
		if (ConVar.AntiHack.rpcstallmode > 1 && basePlayer.wasStalled)
		{
			if (Global.developer > 0)
			{
				Debug.Log((object)("SV_RPCMessage: player was stalled " + (object)basePlayer));
			}
			return;
		}
		if (basePlayer.IsTransferProtected() && !IsAllowedWhileTransferProtected(nameID))
		{
			if (Global.developer > 0)
			{
				Debug.Log((object)("SV_RPCMessage: player is transfer protected " + (object)basePlayer));
			}
			return;
		}
		FieldOperationLimitScope val = message.read.UseProtoDeserializationLimits();
		try
		{
			(byte[], int) buffer = message.read.GetBuffer();
			if (OnRpcMessage(basePlayer, nameID, message))
			{
				if (!basePlayer.IsRealNull() && Facepunch.Rust.Analytics.Azure.ShouldLogRPC(StringPool.Get(nameID)))
				{
					Facepunch.Rust.Analytics.Azure.OnServerRPC(basePlayer, nameID, buffer.Item1, buffer.Item2);
				}
				return;
			}
			for (int i = 0; i < Components.Count; i++)
			{
				if (Components[i].OnRpcMessage(basePlayer, nameID, message))
				{
					if (!basePlayer.IsRealNull())
					{
						Facepunch.Rust.Analytics.Azure.OnServerRPC(basePlayer, nameID, buffer.Item1, buffer.Item2);
					}
					break;
				}
			}
		}
		finally
		{
			((IDisposable)(*(FieldOperationLimitScope*)(&val))/*cast due to constrained. prefix*/).Dispose();
		}
	}

	private static bool IsAllowedWhileTransferProtected(uint nameID)
	{
		if (!transferProtectedRpcsResolved)
		{
			clientLoadingCompleteRpc = StringPool.Get("ClientLoadingComplete");
			clientKeepConnectionAliveRpc = StringPool.Get("ClientKeepConnectionAlive");
			transferProtectedRpcsResolved = true;
			if (clientLoadingCompleteRpc == 0)
			{
				Debug.LogError((object)"Couldn't resolve the ClientLoadingComplete RPC id - transfer protection will never be released!");
			}
		}
		if (nameID != clientLoadingCompleteRpc || nameID == 0)
		{
			if (nameID == clientKeepConnectionAliveRpc)
			{
				return nameID != 0;
			}
			return false;
		}
		return true;
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite write = ClientRPCStart(target.Function);
			ClientRPCSend(write, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, MemoryStream stream)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			using (TimeWarning.New("Copy Buffer"))
			{
				netWrite.Write(stream.GetBuffer(), 0, (int)stream.Length);
			}
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void GetRpcTargetNetworkGroup(ref RpcTarget target)
	{
		if (target.ToNetworkGroup)
		{
			target.Connections = new SendInfo(net.group.subscribers);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void FreeRPCTarget(RpcTarget target)
	{
		if (target.UsingPooledConnections)
		{
			Pool.FreeUnmanaged<Connection>(ref target.Connections.connections);
		}
	}

	protected NetWrite ClientRPCStart(string funcName)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		NetWrite netWrite = Net.sv.StartWrite();
		netWrite.PacketID(Message.Type.RPCMessage);
		netWrite.EntityID(net.ID);
		netWrite.UInt32(StringPool.Get(funcName));
		return netWrite;
	}

	private void ClientRPCWrite<T>(NetWrite write, T arg)
	{
		NetworkWriteEx.WriteObject(write, arg);
	}

	protected void ClientRPCSend(NetWrite write, SendInfo sendInfo)
	{
		write.Send(sendInfo);
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPCList<T1>(RpcTarget target, List<T1> list)
	{
		if (!Net.sv.IsConnected() || net == null)
		{
			return;
		}
		NetWrite netWrite = ClientRPCStart(target.Function);
		netWrite.Int32(list.Count);
		foreach (T1 item in list)
		{
			ClientRPCWrite(netWrite, item);
		}
		ClientRPCSend(netWrite, target.Connections);
	}

	public virtual bool CanBeReskinned(BasePlayer player)
	{
		return true;
	}

	public virtual bool CanBeRedirectSwapped(BasePlayer player)
	{
		return true;
	}

	public virtual void Reskin_Preserve(ref SprayCan.ReskinPreserveInfo preserveInfo)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		ref BaseEntityPreserveInfo baseEntityPreserve = ref preserveInfo.baseEntityPreserve;
		baseEntityPreserve.localPosition = ((Component)this).transform.localPosition;
		baseEntityPreserve.localRotation = ((Component)this).transform.localRotation;
		baseEntityPreserve.parent = GetParentEntity();
		baseEntityPreserve.slots = GetSlots();
		baseEntityPreserve.ownerID = OwnerID;
		if (this is IItemContainerEntity itemContainerEntity)
		{
			itemContainerEntity.Reskin_Preserve_Container(ref preserveInfo.containerPreserve, this, -1);
		}
		if (PreserveChildrenWhenReskinning)
		{
			Reskin_Preserve_Children(ref baseEntityPreserve);
			return;
		}
		for (int i = 0; i < children.Count; i++)
		{
			if (children[i] is IItemContainerEntity itemContainerEntity2)
			{
				itemContainerEntity2.Reskin_Preserve_Container(ref preserveInfo.containerPreserve, children[i], i);
			}
		}
	}

	private void Reskin_Preserve_Children(ref BaseEntityPreserveInfo preserve)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		preserve.childPreserveInfos = Pool.Get<List<ChildPreserveInfo>>();
		foreach (BaseEntity child in children)
		{
			ChildPreserveInfo item = new ChildPreserveInfo
			{
				targetEntity = child,
				targetBone = child.parentBone,
				localPosition = ((Component)child).transform.localPosition,
				localRotation = ((Component)child).transform.localRotation
			};
			Socket_Specific socket_Specific = PrefabAttribute.server.Find<Socket_Base>(child.prefabID) as Socket_Specific;
			if (socket_Specific != null)
			{
				Socket_Base[] array = PrefabAttribute.server.FindAll<Socket_Base>(prefabID);
				Socket_Specific_Female socket_Specific_Female = null;
				float num = float.MaxValue;
				Socket_Base[] array2 = array;
				foreach (Socket_Base socket_Base in array2)
				{
					if (socket_Base is Socket_Specific_Female socket_Specific_Female2 && Array.IndexOf(socket_Specific_Female2.allowedMaleSockets, socket_Specific.targetSocketName) >= 0)
					{
						Vector3 val = ((Component)this).transform.position + ((Component)this).transform.rotation * socket_Base.worldPosition;
						float num2 = Vector3.Distance(((Component)child).transform.position, val);
						if (num2 < num)
						{
							socket_Specific_Female = socket_Specific_Female2;
							num = num2;
						}
					}
				}
				if (socket_Specific_Female != null)
				{
					item.targetSocketName = socket_Specific_Female.socketName;
				}
			}
			preserve.childPreserveInfos.Add(item);
		}
		foreach (ChildPreserveInfo childPreserveInfo in preserve.childPreserveInfos)
		{
			childPreserveInfo.targetEntity.SetParent(null, worldPositionStays: true);
		}
	}

	public virtual void Reskin_Restore(ref SprayCan.ReskinPreserveInfo preserveInfo)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		ref BaseEntityPreserveInfo baseEntityPreserve = ref preserveInfo.baseEntityPreserve;
		((Component)this).transform.position = baseEntityPreserve.localPosition;
		((Component)this).transform.rotation = baseEntityPreserve.localRotation;
		SetParent(baseEntityPreserve.parent);
		SetSlots(baseEntityPreserve.slots);
		OwnerID = baseEntityPreserve.ownerID;
		if (this is IItemContainerEntity itemContainerEntity)
		{
			itemContainerEntity.Reskin_Restore_Container(ref preserveInfo.containerPreserve, this, -1);
		}
		if (PreserveChildrenWhenReskinning)
		{
			Reskin_Restore_Children(ref baseEntityPreserve);
		}
		else
		{
			for (int i = 0; i < children.Count; i++)
			{
				if (children[i] is IItemContainerEntity itemContainerEntity2)
				{
					itemContainerEntity2.Reskin_Restore_Container(ref preserveInfo.containerPreserve, children[i], i);
				}
			}
		}
		if (preserveInfo.containerPreserve.storageDict != null)
		{
			DropLeftoverItems(ref preserveInfo.containerPreserve);
			Debug.LogError((object)("Was unable to cleanly transfer some items when reskinning to " + base.ShortPrefabName + ", dropped them instead"));
		}
	}

	private void Reskin_Restore_Children(ref BaseEntityPreserveInfo preserve)
	{
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		HashSet<IOEntity> hashSet = Pool.Get<HashSet<IOEntity>>();
		foreach (ChildPreserveInfo childPreserveInfo in preserve.childPreserveInfos)
		{
			childPreserveInfo.targetEntity.SetParent(this, childPreserveInfo.targetBone, worldPositionStays: true);
			bool flag = false;
			if (ConVar.Server.repositionAttachmentsOnReskin)
			{
				flag = TryRepositionChildEntity(childPreserveInfo.targetEntity, childPreserveInfo.targetSocketName, hashSet);
			}
			if (!flag)
			{
				((Component)childPreserveInfo.targetEntity).transform.localPosition = childPreserveInfo.localPosition;
				((Component)childPreserveInfo.targetEntity).transform.localRotation = childPreserveInfo.localRotation;
			}
		}
		foreach (IOEntity item in hashSet)
		{
			item.SendNetworkUpdateImmediate();
			item.NotifyClientsLineChanged();
		}
		Pool.FreeUnmanaged<IOEntity>(ref hashSet);
		Pool.FreeUnmanaged<ChildPreserveInfo>(ref preserve.childPreserveInfos);
	}

	private void DropLeftoverItems(ref IItemContainerEntity.ContainerPreserveInfo preserve)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		foreach (KeyValuePair<IItemContainerEntity.ContainerSet, List<Item>> item in preserve.storageDict)
		{
			foreach (Item item2 in item.Value)
			{
				item2.DropAndTossUpwards(((Component)this).transform.position + Vector3.up * 0.5f);
			}
			List<Item> value = item.Value;
			Pool.FreeUnmanaged<Item>(ref value);
		}
		Pool.FreeUnmanaged<IItemContainerEntity.ContainerSet, List<Item>>(ref preserve.storageDict);
	}

	private bool TryRepositionChildEntity(BaseEntity childEntity, string targetSocketName, HashSet<IOEntity> toUpdate)
	{
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		Socket_Specific socket_Specific = PrefabAttribute.server.Find<Socket_Base>(childEntity.prefabID) as Socket_Specific;
		if (socket_Specific == null)
		{
			return false;
		}
		Socket_Base[] array = PrefabAttribute.server.FindAll<Socket_Base>(prefabID);
		Socket_Specific_Female socket_Specific_Female = null;
		float num = float.MaxValue;
		Socket_Base[] array2 = array;
		foreach (Socket_Base socket_Base in array2)
		{
			if (socket_Base is Socket_Specific_Female socket_Specific_Female2 && Array.IndexOf(socket_Specific_Female2.allowedMaleSockets, socket_Specific.targetSocketName) >= 0)
			{
				if (StringEx.EqualsAfterLastSeparator(targetSocketName, socket_Base.socketName, '/', StringComparison.Ordinal))
				{
					socket_Specific_Female = socket_Specific_Female2;
					break;
				}
				Vector3 val = ((Component)this).transform.position + ((Component)this).transform.rotation * socket_Base.worldPosition;
				float num2 = Vector3.Distance(((Component)childEntity).transform.position, val);
				if (num2 < num)
				{
					socket_Specific_Female = socket_Specific_Female2;
					num = num2;
				}
			}
		}
		if (socket_Specific_Female == null)
		{
			return false;
		}
		Matrix4x4 localToWorldMatrix = ((Component)childEntity).transform.localToWorldMatrix;
		((Component)childEntity).transform.SetLocalPositionAndRotation(socket_Specific_Female.localPosition, socket_Specific_Female.localRotation);
		Matrix4x4 val2 = ((Component)childEntity).transform.worldToLocalMatrix * localToWorldMatrix;
		if (childEntity is IOEntity { outputs: var outputs } iOEntity)
		{
			foreach (IOEntity.IOSlot iOSlot in outputs)
			{
				if (iOSlot.IsConnected() && iOSlot.linePoints != null && iOSlot.linePoints.Length != 0)
				{
					for (int j = 0; j < iOSlot.linePoints.Length - 1; j++)
					{
						iOSlot.linePoints[j] = ((Matrix4x4)(ref val2)).MultiplyPoint3x4(iOSlot.linePoints[j]);
					}
					toUpdate.Add(iOEntity);
				}
			}
			iOEntity.SnapLinesToHandlePositions(toUpdate);
		}
		return true;
	}

	public virtual float RadiationProtection()
	{
		return 0f;
	}

	public virtual float RadiationExposureFraction()
	{
		return 1f;
	}

	public virtual void SetCreatorEntity(BaseEntity newCreatorEntity)
	{
		creatorEntity = newCreatorEntity;
	}

	public virtual Vector3 GetLocalVelocityServer()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.zero;
	}

	public virtual Quaternion GetAngularVelocityServer()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return Quaternion.identity;
	}

	public void EnableGlobalBroadcast(bool wants)
	{
		if (globalBroadcast != wants)
		{
			globalBroadcast = wants;
			UpdateNetworkGroup();
		}
	}

	public void EnableSaving(bool wants)
	{
		if (enableSaving != wants)
		{
			enableSaving = wants;
			if (enableSaving)
			{
				saveList.Add(this);
			}
			else
			{
				saveList.Remove(this);
			}
		}
	}

	public void RestoreCanSave()
	{
		EnableSaving(couldSaveOriginally);
	}

	public override void ServerInit()
	{
		_spawnable = ((Component)this).GetComponent<Spawnable>();
		base.ServerInit();
		if (base.isServer)
		{
			couldSaveOriginally = enableSaving;
			if (enableSaving)
			{
				saveList.Add(this);
			}
			if (flags != 0)
			{
				OnFlagsChanged((Flags)0, flags);
			}
			if (syncPosition && PositionTickRate >= 0f)
			{
				syncPosition = false;
				ToggleNetworkPositionTick(isEnabled: true);
			}
			if (Query.Server != null)
			{
				Query.Server.Add(this);
			}
			if (this is SamSite.ISamSiteTarget item)
			{
				SamSite.ISamSiteTarget.serverList.Add(item);
			}
			if (this is IPowergridEntity powergridEntity)
			{
				PowergridManager.Server_AddPowergridEntity(powergridEntity);
			}
			if (Application.isServerStarted)
			{
				Facepunch.Rust.Analytics.Azure.OnEntitySpawned(this);
			}
		}
	}

	public override void ServerInitPostNetworkGroupAssign()
	{
		if (Components == null)
		{
			return;
		}
		for (int i = 0; i < Components.Count; i++)
		{
			if (!((Object)(object)Components[i] == (Object)null))
			{
				Components[i].ServerInitPostNetworkGroupAssign();
			}
		}
	}

	public virtual void OnPlaced(BasePlayer player)
	{
	}

	protected virtual bool ShouldUpdateNetworkGroup()
	{
		return syncPosition;
	}

	protected virtual bool ShouldUpdateNetworkPosition()
	{
		return syncPosition;
	}

	protected void ToggleNetworkPositionTick(bool isEnabled)
	{
		if (syncPosition == isEnabled)
		{
			return;
		}
		syncPosition = isEnabled;
		if (syncPosition)
		{
			if (NetworkPosTickCallback == null)
			{
				Action action = (NetworkPosTickCallback = NetworkPositionTick);
			}
			if (PositionTickFixedTime)
			{
				InvokeRepeatingFixedTime(NetworkPosTickCallback);
			}
			else
			{
				InvokeRandomized(NetworkPosTickCallback, PositionTickRate, PositionTickRate - PositionTickRate * 0.05f, PositionTickRate * 0.05f);
			}
		}
		else if (PositionTickFixedTime)
		{
			CancelInvokeFixedTime(NetworkPosTickCallback);
		}
		else
		{
			CancelInvoke(NetworkPosTickCallback);
		}
	}

	public void NetworkPositionTick()
	{
		if (!((Component)this).transform.hasChanged)
		{
			if (ticksSinceStopped >= 6)
			{
				return;
			}
			ticksSinceStopped++;
		}
		else
		{
			ticksSinceStopped = 0;
		}
		TransformChanged();
		((Component)this).transform.hasChanged = false;
	}

	protected virtual void TransformChanged()
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		if (Query.Server != null)
		{
			Query.Server.Move(this);
		}
		SingletonComponent<NpcFireManager>.Instance.Move(this);
		if (net == null)
		{
			return;
		}
		InvalidateNetworkCache();
		if (!globalBroadcast && !ValidBounds.Test(this, ((Component)this).transform.position))
		{
			OnInvalidPosition();
			return;
		}
		TryScheduleUpdateNetworkGroup();
		if (ShouldUpdateNetworkPosition())
		{
			SendNetworkUpdate_Position();
			OnPositionalNetworkUpdate();
		}
	}

	protected void TryScheduleUpdateNetworkGroup()
	{
		if (ShouldUpdateNetworkGroup() && !isCallingUpdateNetworkGroup)
		{
			if (_updateNetworkGroupCallback == null)
			{
				_updateNetworkGroupCallback = UpdateNetworkGroup;
			}
			Invoke(_updateNetworkGroupCallback, 5f);
			isCallingUpdateNetworkGroup = true;
		}
	}

	public virtual void OnPositionalNetworkUpdate()
	{
	}

	public override void Spawn()
	{
		base.Spawn();
		if (base.isServer)
		{
			OnParentSpawningEx.BroadcastOnParentSpawning(((Component)this).gameObject);
		}
		for (int i = 0; i < entitySlots.Length; i++)
		{
			entitySlots[i] = default(EntityRef);
		}
	}

	public void OnParentSpawning()
	{
		if (net != null || base.IsDestroyed)
		{
			return;
		}
		if (Application.isLoadingSave)
		{
			Object.Destroy((Object)(object)((Component)this).gameObject);
			return;
		}
		if (GameManager.server.preProcessed.NeedsProcessing(((Component)this).gameObject, PreProcessPrefabOptions.Default_NoResetPosition))
		{
			GameManager.server.preProcessed.ProcessObject(null, ((Component)this).gameObject, PreProcessPrefabOptions.Default_NoResetPosition);
		}
		BaseEntity baseEntity = (((Object)(object)((Component)this).transform.parent != (Object)null) ? ((Component)((Component)this).transform.parent).GetComponentInParent<BaseEntity>() : null);
		Spawn();
		if ((Object)(object)baseEntity != (Object)null)
		{
			SetParent(baseEntity, worldPositionStays: true);
		}
	}

	public void SpawnAsMapEntity()
	{
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		if (net == null && !base.IsDestroyed && (Object)(object)(((Object)(object)((Component)this).transform.parent != (Object)null) ? ((Component)((Component)this).transform.parent).GetComponentInParent<BaseEntity>() : null) == (Object)null)
		{
			if (GameManager.server.preProcessed.NeedsProcessing(((Component)this).gameObject, PreProcessPrefabOptions.Default_NoResetPosition))
			{
				GameManager.server.preProcessed.ProcessObject(null, ((Component)this).gameObject, PreProcessPrefabOptions.Default_NoResetPosition);
			}
			((Component)this).transform.parent = null;
			SceneManager.MoveGameObjectToScene(((Component)this).gameObject, Rust.Server.EntityScene);
			((Component)this).gameObject.SetActive(true);
			Spawn();
		}
	}

	public virtual void PostMapEntitySpawn()
	{
	}

	internal override void DoServerDestroy()
	{
		if (Application.isServerStarted)
		{
			Facepunch.Rust.Analytics.Azure.OnEntityDestroyed(this);
		}
		if (Query.Server != null)
		{
			Query.Server.Remove(this);
		}
		ToggleNetworkPositionTick(isEnabled: false);
		if (enableSaving)
		{
			saveList.Remove(this);
		}
		enableSaving = couldSaveOriginally;
		RemoveFromTriggers();
		if (children != null)
		{
			BaseEntity[] array = children.ToArray();
			foreach (BaseEntity baseEntity in array)
			{
				if (!((Object)(object)baseEntity == (Object)null))
				{
					baseEntity.OnParentRemoved();
				}
			}
		}
		SetParent(null, worldPositionStays: true);
		SingletonComponent<NpcFireManager>.Instance.Remove(this);
		if (this is SamSite.ISamSiteTarget item)
		{
			SamSite.ISamSiteTarget.serverList.Remove(item);
		}
		if (this is IPowergridEntity powergridEntity)
		{
			PowergridManager.Server_RemovePowergridEntity(powergridEntity);
		}
		base.DoServerDestroy();
	}

	internal virtual void OnParentRemoved()
	{
		Kill();
	}

	public virtual void OnInvalidPosition()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		Debug.Log((object)("Invalid Position: " + ((object)this)?.ToString() + " " + ((object)((Component)this).transform.position/*cast due to constrained. prefix*/).ToString() + " (destroying)"));
		Kill();
	}

	public BaseCorpse DropCorpse(string strCorpsePrefab, BasePlayer.PlayerFlags playerFlagsOnDeath = (BasePlayer.PlayerFlags)0, ModelState modelState = null)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		return DropCorpse(strCorpsePrefab, ((Component)this).transform.position, ((Component)this).transform.rotation, playerFlagsOnDeath, modelState);
	}

	public BaseCorpse DropCorpse(string strCorpsePrefab, Vector3 posOnDeath, Quaternion rotOnDeath, BasePlayer.PlayerFlags playerFlagsOnDeath = (BasePlayer.PlayerFlags)0, ModelState modelState = null)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		Assert.IsTrue(base.isServer, "DropCorpse called on client!");
		if (!ConVar.Server.corpses)
		{
			return null;
		}
		if (string.IsNullOrEmpty(strCorpsePrefab))
		{
			return null;
		}
		BaseCorpse baseCorpse = GameManager.server.CreateEntity(strCorpsePrefab) as BaseCorpse;
		if ((Object)(object)baseCorpse == (Object)null)
		{
			Debug.LogWarning((object)("Error creating corpse: " + ((object)((Component)this).gameObject)?.ToString() + " - " + strCorpsePrefab));
			return null;
		}
		baseCorpse.ServerInitCorpse(this, posOnDeath, rotOnDeath, playerFlagsOnDeath, modelState);
		return baseCorpse;
	}

	public override void UpdateNetworkGroup()
	{
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		Assert.IsTrue(base.isServer, "UpdateNetworkGroup called on clientside entity!");
		isCallingUpdateNetworkGroup = false;
		if (net == null || Net.sv == null || Net.sv.visibility == null)
		{
			return;
		}
		using (TimeWarning.New("UpdateNetworkGroup"))
		{
			if (globalBroadcast)
			{
				Group globalNetworkGroup = BaseNetworkable.GetGlobalNetworkGroup(globalNetworkBehavior);
				if (net.SwitchGroup(globalNetworkGroup))
				{
					SendNetworkGroupChange();
				}
				return;
			}
			BaseEntity baseEntity = GetParentEntity();
			if (parentEntity.IsSet() && !baseEntity.IsValid() && ShouldInheritNetworkGroup())
			{
				if (!Application.isLoadingSave)
				{
					Debug.LogWarning((object)("UpdateNetworkGroup: Missing parent entity " + ((object)parentEntity.uid/*cast due to constrained. prefix*/).ToString()));
					if (_updateNetworkGroupCallback == null)
					{
						_updateNetworkGroupCallback = UpdateNetworkGroup;
					}
					Invoke(_updateNetworkGroupCallback, 2f);
					isCallingUpdateNetworkGroup = true;
				}
			}
			else if (ShouldInheritNetworkGroup() && parentEntity.IsSet() && baseEntity.IsValid() && baseEntity.ShouldChildrenInheritNetworkGroup())
			{
				if ((Object)(object)baseEntity != (Object)null)
				{
					if (net.SwitchGroup(baseEntity.net.group))
					{
						SendNetworkGroupChange();
					}
				}
				else
				{
					Debug.LogWarning((object)(((object)((Component)this).gameObject)?.ToString() + ": has parent id - but couldn't find parent! " + parentEntity));
				}
			}
			else if (base.limitNetworking && !(this is BasePlayer))
			{
				if (net.SwitchGroup(BaseNetworkable.LimboNetworkGroup))
				{
					SendNetworkGroupChange();
				}
			}
			else
			{
				base.UpdateNetworkGroup();
			}
		}
	}

	public virtual void Eat(BaseNpc baseNpc, float timeSpent)
	{
		baseNpc.AddCalories(100f);
	}

	public virtual void OnDeployed(BaseEntity parent, BasePlayer deployedBy, Item fromItem)
	{
		List<EntityComponentBase> components = Components;
		int i = 0;
		for (int count = components.Count; i < count; i++)
		{
			components[i].OnEntityDeployed(parent, deployedBy, fromItem);
		}
	}

	public override bool ShouldNetworkTo(BasePlayer player)
	{
		if ((Object)(object)player == (Object)(object)this)
		{
			return true;
		}
		if (IsTransferProtected())
		{
			return false;
		}
		BaseEntity baseEntity = GetParentEntity();
		if (base.limitNetworking)
		{
			if ((Object)(object)baseEntity == (Object)null)
			{
				return false;
			}
			if ((Object)(object)baseEntity != (Object)(object)player)
			{
				return false;
			}
		}
		if (ShouldInheritNetworkGroup() && (Object)(object)baseEntity != (Object)null && baseEntity.ShouldChildrenInheritNetworkGroup())
		{
			return baseEntity.ShouldNetworkTo(player);
		}
		return base.ShouldNetworkTo(player);
	}

	public virtual void AttackerInfo(DeathInfo info)
	{
		info.attackerName = base.ShortPrefabName;
		info.attackerSteamID = 0uL;
		info.inflictorName = "";
	}

	public virtual void Push(Vector3 velocity)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		SetVelocity(velocity);
	}

	public virtual void ApplyInheritedVelocity(Vector3 velocity)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		Rigidbody component = ((Component)this).GetComponent<Rigidbody>();
		if (Object.op_Implicit((Object)(object)component))
		{
			component.linearVelocity = Vector3.Lerp(component.linearVelocity, velocity, 10f * Time.fixedDeltaTime);
			component.angularVelocity *= Mathf.Clamp01(1f - 10f * Time.fixedDeltaTime);
			component.AddForce(-Physics.gravity * Mathf.Clamp01(0.9f), (ForceMode)5);
		}
	}

	public virtual void SetVelocity(Vector3 velocity)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Rigidbody component = ((Component)this).GetComponent<Rigidbody>();
		if (Object.op_Implicit((Object)(object)component))
		{
			component.linearVelocity = velocity;
		}
	}

	public virtual void SetAngularVelocity(Vector3 velocity)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		Rigidbody component = ((Component)this).GetComponent<Rigidbody>();
		if (Object.op_Implicit((Object)(object)component))
		{
			component.angularVelocity = velocity;
		}
	}

	public virtual Vector3 GetDropPosition()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.position;
	}

	public virtual Vector3 GetDropVelocity()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		return GetInheritedDropVelocity() + Vector3.up;
	}

	public virtual bool OnStartBeingLooted(BasePlayer baseEntity)
	{
		return true;
	}

	public virtual string Admin_Who()
	{
		return $"Owner ID: {OwnerID}";
	}

	public virtual bool BuoyancyWake()
	{
		return false;
	}

	public virtual bool BuoyancySleep(bool inWater)
	{
		return false;
	}

	public virtual bool AllowInitChildSupports()
	{
		return false;
	}

	public Axis HasMovedInLS(int frame)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		if (oldPosLSFrame != frame)
		{
			Vector3 localPosition = ((Component)this).transform.localPosition;
			hasMovedLS = ComparePos(oldPosLS, localPosition);
			oldPosLSFrame = frame;
			oldPosLS = localPosition;
		}
		return hasMovedLS;
	}

	public static Axis ComparePos(Vector3 from, Vector3 to, float epsilon = 9.9999994E-11f)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		Axis axis = Axis.None;
		Vector3 val = to - from;
		if (val.x * val.x >= epsilon)
		{
			axis |= Axis.X;
		}
		if (val.y * val.y >= epsilon)
		{
			axis |= Axis.Y;
		}
		if (val.z * val.z >= epsilon)
		{
			axis |= Axis.Z;
		}
		return axis;
	}

	[RPC_Server]
	[RPC_Server.FromOwnerOrMounted]
	private void BroadcastSignalFromClient(RPCMessage msg)
	{
		uint num = StringPool.Get("BroadcastSignalFromClient");
		if (num == 0)
		{
			return;
		}
		BasePlayer player = msg.player;
		if (!((Object)(object)player == (Object)null) && player.rpcHistory.TryIncrement(num, (ulong)ConVar.Server.maxpacketspersecond_rpc_signal))
		{
			Signal signal = (Signal)msg.read.Int32();
			string arg = msg.read.String();
			if (!BroadcastSignalFromClientFilter(signal))
			{
				SignalBroadcast(signal, arg, msg.connection);
				OnReceivedSignalServer(signal, arg);
			}
		}
	}

	protected virtual bool BroadcastSignalFromClientFilter(Signal signal)
	{
		return false;
	}

	protected virtual void OnReceivedSignalServer(Signal signal, string arg)
	{
		SingletonComponent<NpcFireManager>.Instance.OnReceivedSignalServer(this, signal, arg);
	}

	public void SignalBroadcast(Signal signal, string arg, Connection sourceConnection = null)
	{
		if (net != null && net.group != null && !base.limitNetworking && Interface.CallHook("OnSignalBroadcast", this, sourceConnection, signal, arg) == null)
		{
			ClientRPC(RpcTarget.NetworkGroup("SignalFromServerEx", this, SendMethod.Unreliable, Priority.Immediate), (int)signal, arg, sourceConnection?.userid ?? 0);
		}
	}

	public void SignalBroadcast(Signal signal, Connection sourceConnection = null)
	{
		if (net != null && net.group != null)
		{
			ClientRPC(RpcTarget.NetworkGroup("SignalFromServer", this, SendMethod.Unreliable, Priority.Immediate), (int)signal, sourceConnection?.userid ?? 0);
		}
	}

	private bool IsEffectVisibleTo(BasePlayer player)
	{
		if (IsUnderground())
		{
			return player.IsUnderground();
		}
		return true;
	}

	public void SignalBroadcast(Signal signal, string arg, Connection sourceConnection, string fallbackEffect, float maxDistance = 0f)
	{
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		bool flag = maxDistance > 0f;
		if ((!flag || !ConVar.Server.long_distance_sounds) && !ServerOcclusion.OcclusionEnabled)
		{
			SignalBroadcast(signal, arg, sourceConnection);
		}
		else
		{
			if (net == null || net.group == null || net.group.subscribers == null)
			{
				return;
			}
			PooledHashSet<ulong> val = Pool.Get<PooledHashSet<ulong>>();
			try
			{
				PooledList<Connection> val2 = Pool.Get<PooledList<Connection>>();
				try
				{
					PooledList<Connection> val3 = Pool.Get<PooledList<Connection>>();
					try
					{
						foreach (Connection subscriber in net.group.subscribers)
						{
							if (subscriber.player is BasePlayer basePlayer && !((Object)(object)basePlayer == (Object)null))
							{
								((HashSet<ulong>)(object)val).Add(basePlayer.userID.Get());
								if (ShouldNetworkTo(basePlayer))
								{
									((List<Connection>)(object)val2).Add(subscriber);
								}
								else if (IsEffectVisibleTo(basePlayer))
								{
									((List<Connection>)(object)val3).Add(subscriber);
								}
							}
						}
						if (flag)
						{
							using (TimeWarning.New("BaseEntity.Signal.LongDistanceSound"))
							{
								foreach (Connection item in BaseNetworkable.GetConnectionsWithin(((Component)this).transform.position, maxDistance))
								{
									if (item.player is BasePlayer basePlayer2 && !((Object)(object)basePlayer2 == (Object)null) && !((HashSet<ulong>)(object)val).Contains(basePlayer2.userID.Get()) && IsEffectVisibleTo(basePlayer2))
									{
										((List<Connection>)(object)val3).Add(item);
									}
								}
							}
						}
						if (((List<Connection>)(object)val2).Count > 0)
						{
							ClientRPC(RpcTarget.Players("SignalFromServerEx", (List<Connection>)(object)val2, SendMethod.Unreliable, Priority.Immediate), (int)signal, arg, sourceConnection?.userid ?? 0);
						}
						if (((List<Connection>)(object)val3).Count > 0)
						{
							Effect.server.Run(fallbackEffect, ((Component)this).transform.position, ((Component)this).transform.up, sourceConnection, broadcast: false, (List<Connection>)(object)val3);
						}
					}
					finally
					{
						((IDisposable)val3)?.Dispose();
					}
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	protected virtual void OnSkinChanged(ulong oldSkinID, ulong newSkinID)
	{
		if (oldSkinID != newSkinID)
		{
			skinID = newSkinID;
		}
	}

	protected virtual void OnAttachmentChanged(ulong oldAttachment, ulong newAttachment)
	{
		if (attachmentID != newAttachment)
		{
			attachmentID = newAttachment;
		}
	}

	protected virtual void OnSkinPreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		if (clientside && Skinnable.All != null && (Object)(object)Skinnable.FindForEntity(name) != (Object)null)
		{
			WorkshopSkin.Prepare(rootObj);
			MaterialReplacement.Prepare(rootObj);
		}
	}

	public virtual void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		OnSkinPreProcess(preProcess, rootObj, name, serverside, clientside, bundling);
	}

	public virtual bool HasAnySlot()
	{
		for (int i = 0; i < entitySlots.Length; i++)
		{
			if (entitySlots[i].IsValid(base.isServer))
			{
				return true;
			}
		}
		return false;
	}

	public BaseEntity GetSlot(Slot slot)
	{
		return entitySlots[(int)slot].Get(base.isServer);
	}

	public BaseLock GetLock()
	{
		return GetSlot(Slot.Lock) as BaseLock;
	}

	public string GetSlotAnchorName(Slot slot)
	{
		return slot.ToString().ToLower();
	}

	public void SetSlot(Slot slot, BaseEntity ent)
	{
		entitySlots[(int)slot].Set(ent);
		SendNetworkUpdate();
	}

	public EntityRef[] GetSlots()
	{
		return entitySlots;
	}

	public void SetSlots(EntityRef[] newSlots)
	{
		entitySlots = newSlots;
	}

	public virtual bool HasSlot(Slot slot)
	{
		return false;
	}

	protected void QueueSyncVar(byte nameID)
	{
		if (base.isServer)
		{
			if (nameID >= 32)
			{
				Debug.LogError((object)$"nameID {nameID} is out of bitmask range (must be 0-{31})");
				return;
			}
			WarmupSyncVars();
			_serverSyncVarQueue |= (uint)(1 << (int)nameID);
		}
	}

	private void SendPackedSyncVarQueue()
	{
		SV_PackedSyncVarSendQueue();
	}

	private void SyncVarNetSend(NetWrite write, SendInfo sendInfo)
	{
		write.Send(sendInfo);
	}

	public void WarmupSyncVars()
	{
		if (_sendPackedSyncVarQueueAction == null)
		{
			_sendPackedSyncVarQueueAction = SendPackedSyncVarQueue;
		}
		if (!IsInvoking(_sendPackedSyncVarQueueAction))
		{
			Invoke(_sendPackedSyncVarQueueAction, 0.0333f);
		}
	}

	public void StopSyncVars()
	{
		if (IsInvoking(_sendPackedSyncVarQueueAction))
		{
			CancelInvoke(_sendPackedSyncVarQueueAction);
		}
	}

	protected NetWrite SV_PackedSyncVarNetStart()
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("PackedSyncVar"))
		{
			NetWrite netWrite = Net.sv.StartWrite();
			using (TimeWarning.New("Headers"))
			{
				netWrite.PacketID(Message.Type.PackedSyncVar);
				netWrite.EntityID(net.ID);
				netWrite.UInt32(_serverSyncVarQueue);
				return netWrite;
			}
		}
	}

	protected void SV_PackedSyncVarSendQueue()
	{
		if (_serverSyncVarQueue == 0 || Net.sv == null || !Net.sv.IsConnected() || net == null)
		{
			return;
		}
		using (TimeWarning.New("PackedSyncVarQueue"))
		{
			NetWrite netWrite = SV_PackedSyncVarNetStart();
			for (byte b = 0; b < 32; b++)
			{
				if ((_serverSyncVarQueue & (uint)(1 << (int)b)) != 0)
				{
					WriteSyncVar(b, netWrite);
					HandleCache(b);
				}
			}
			_serverSyncVarQueue = 0u;
			SyncVarNetSend(netWrite, new SendInfo(net.group.subscribers));
		}
	}

	private NetWrite SV_SyncVarNetStart(byte nameID)
	{
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("SyncVar"))
		{
			NetWrite netWrite = Net.sv.StartWrite();
			using (TimeWarning.New("Headers"))
			{
				netWrite.PacketID(Message.Type.SyncVar);
				netWrite.EntityID(net.ID);
				netWrite.UInt8(nameID);
				return netWrite;
			}
		}
	}

	protected void SV_SyncVarSend(byte nameID)
	{
		if (Net.sv != null && Net.sv.IsConnected() && net != null)
		{
			NetWrite netWrite = SV_SyncVarNetStart(nameID);
			WriteSyncVar(nameID, netWrite);
			HandleCache(nameID);
			SyncVarNetSend(netWrite, new SendInfo(net.group.subscribers));
		}
	}

	protected void SyncVarNetWrite<T>(NetWrite write, T arg)
	{
		using (TimeWarning.New("Objects"))
		{
			NetworkWriteEx.WriteObject(write, arg);
		}
	}

	private void HandleCache(byte nameID)
	{
		if (ShouldInvalidateCache(nameID))
		{
			InvalidateNetworkCache();
		}
	}

	public bool HasTrait(TraitFlag f)
	{
		return (Traits & f) == f;
	}

	public bool HasAnyTrait(TraitFlag f)
	{
		return (Traits & f) != 0;
	}

	public virtual bool EnterTrigger(TriggerBase trigger)
	{
		if (triggers == null)
		{
			triggers = Pool.Get<List<TriggerBase>>();
		}
		triggers.Add(trigger);
		return true;
	}

	public virtual void LeaveTrigger(TriggerBase trigger)
	{
		if (triggers != null)
		{
			triggers.Remove(trigger);
			if (triggers.Count == 0)
			{
				Pool.FreeUnmanaged<TriggerBase>(ref triggers);
			}
		}
	}

	public void RemoveFromTriggers()
	{
		if (triggers == null)
		{
			return;
		}
		using (TimeWarning.New("RemoveFromTriggers"))
		{
			List<TriggerBase> list = List.ShallowClonePooled<TriggerBase>(triggers);
			foreach (TriggerBase item in list)
			{
				if (Object.op_Implicit((Object)(object)item))
				{
					item.RemoveEntity(this);
				}
			}
			Pool.FreeUnmanaged<TriggerBase>(ref list);
			if (triggers != null && triggers.Count == 0)
			{
				Pool.FreeUnmanaged<TriggerBase>(ref triggers);
			}
		}
	}

	public T FindTrigger<T>() where T : TriggerBase
	{
		if (triggers == null)
		{
			return null;
		}
		foreach (TriggerBase trigger in triggers)
		{
			if (!((Object)(object)(trigger as T) == (Object)null))
			{
				return trigger as T;
			}
		}
		return null;
	}

	public TriggerSafeZoneOverride FindActiveCombatTrigger()
	{
		if (triggers == null)
		{
			return null;
		}
		foreach (TriggerBase trigger in triggers)
		{
			if (trigger is TriggerSafeZoneOverride { IsCombatActive: not false } triggerSafeZoneOverride)
			{
				return triggerSafeZoneOverride;
			}
		}
		return null;
	}

	public bool InSafeCombatZone()
	{
		if (BaseGameMode.TryGetActiveGameMode(base.isServer, out var gameMode) && !gameMode.safeZone)
		{
			return false;
		}
		return (Object)(object)FindActiveCombatTrigger() != (Object)null;
	}

	public bool FindTrigger<T>(out T result) where T : TriggerBase
	{
		result = FindTrigger<T>();
		return (Object)(object)result != (Object)null;
	}

	private void ForceUpdateTriggersAction()
	{
		if (!base.IsDestroyed)
		{
			ForceUpdateTriggers(enter: false, exit: true, invoke: false);
		}
	}

	public void ForceUpdateTriggers(bool enter = true, bool exit = true, bool invoke = true)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		if (this is BasePlayer { isInvisible: not false })
		{
			return;
		}
		List<TriggerBase> list = Pool.Get<List<TriggerBase>>();
		List<TriggerBase> list2 = Pool.Get<List<TriggerBase>>();
		if (triggers != null)
		{
			list.AddRange(triggers);
		}
		Collider componentInChildren = ((Component)this).GetComponentInChildren<Collider>();
		if (componentInChildren is CapsuleCollider)
		{
			CapsuleCollider val = (CapsuleCollider)(object)((componentInChildren is CapsuleCollider) ? componentInChildren : null);
			Vector3 point = ((Component)this).transform.position + new Vector3(0f, val.radius, 0f);
			Vector3 point2 = ((Component)this).transform.position + new Vector3(0f, val.height - val.radius, 0f);
			GamePhysics.OverlapCapsule<TriggerBase>(point, point2, val.radius, list2, 262144, (QueryTriggerInteraction)2);
		}
		else if (componentInChildren is BoxCollider)
		{
			BoxCollider val2 = (BoxCollider)(object)((componentInChildren is BoxCollider) ? componentInChildren : null);
			GamePhysics.OverlapOBB<TriggerBase>(new OBB(((Component)this).transform.position, ((Component)this).transform.lossyScale, ((Component)this).transform.rotation, new Bounds(val2.center, val2.size)), list2, 262144, (QueryTriggerInteraction)2);
		}
		else if (componentInChildren is SphereCollider)
		{
			SphereCollider val3 = (SphereCollider)(object)((componentInChildren is SphereCollider) ? componentInChildren : null);
			GamePhysics.OverlapSphere<TriggerBase>(((Component)this).transform.TransformPoint(val3.center), val3.radius, list2, 262144, (QueryTriggerInteraction)2);
		}
		else
		{
			list2.AddRange(list);
		}
		IsForceUpdatingTriggers = true;
		if (exit)
		{
			foreach (TriggerBase item in list)
			{
				if (!list2.Contains(item))
				{
					item.OnTriggerExit(componentInChildren);
				}
			}
		}
		if (enter)
		{
			foreach (TriggerBase item2 in list2)
			{
				if (!list.Contains(item2))
				{
					item2.OnTriggerEnter(componentInChildren);
				}
			}
		}
		IsForceUpdatingTriggers = false;
		Pool.FreeUnmanaged<TriggerBase>(ref list);
		Pool.FreeUnmanaged<TriggerBase>(ref list2);
		if (invoke)
		{
			if (_forceUpdateTriggersCallback == null)
			{
				_forceUpdateTriggersCallback = ForceUpdateTriggersAction;
			}
			Invoke(_forceUpdateTriggersCallback, Time.time - Time.fixedTime + Time.fixedDeltaTime * 1.5f);
		}
	}

	public virtual bool InHostileWarningZone()
	{
		if (triggers == null)
		{
			return false;
		}
		for (int i = 0; i < triggers.Count; i++)
		{
			TriggerHostileWarningZone triggerHostileWarningZone = triggers[i] as TriggerHostileWarningZone;
			if (!((Object)(object)triggerHostileWarningZone == (Object)null) && triggerHostileWarningZone.WarningEnabled(this))
			{
				return true;
			}
		}
		return false;
	}

	public virtual bool InSafeZone()
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		BaseGameMode activeGameMode = BaseGameMode.GetActiveGameMode(serverside: true);
		if ((Object)(object)activeGameMode != (Object)null && !activeGameMode.safeZone)
		{
			return false;
		}
		float num = 0f;
		Vector3 position = ((Component)this).transform.position;
		if (triggers != null)
		{
			for (int i = 0; i < triggers.Count; i++)
			{
				if (triggers[i] is TriggerSafeZone triggerSafeZone)
				{
					float safeLevel = triggerSafeZone.GetSafeLevel(position);
					if (safeLevel > num)
					{
						num = safeLevel;
					}
				}
			}
		}
		return num > 0f;
	}

	public TriggerParent FindSuitableParent()
	{
		if (triggers == null)
		{
			return null;
		}
		foreach (TriggerBase trigger in triggers)
		{
			if (trigger is TriggerParent triggerParent && triggerParent.ShouldParent(this, bypassOtherTriggerCheck: true))
			{
				return triggerParent;
			}
		}
		return null;
	}

	public virtual BasePlayer ToPlayer()
	{
		return null;
	}

	public override void InitShared()
	{
		base.InitShared();
		InitEntityLinks();
		if (Components == null)
		{
			return;
		}
		for (int i = 0; i < Components.Count; i++)
		{
			if (!((Object)(object)Components[i] == (Object)null))
			{
				Components[i].InitShared();
			}
		}
	}

	public override void DestroyShared()
	{
		base.DestroyShared();
		FreeEntityLinks();
		StopSyncVars();
		if (_autosaveBuffer != null)
		{
			_autosaveBufferPool.Return(_autosaveBuffer);
		}
		if (Components == null)
		{
			return;
		}
		for (int i = 0; i < Components.Count; i++)
		{
			if (!((Object)(object)Components[i] == (Object)null))
			{
				Components[i].DestroyShared();
			}
		}
	}

	public override void ResetState()
	{
		base.ResetState();
		parentBone = 0u;
		OwnerID = 0uL;
		flags = (Flags)0;
		skinID = 0uL;
		attachmentID = 0uL;
		HasBrain = false;
		parentEntity = default(EntityRef);
		ResetSyncVars();
		LookupPrefab();
		if (base.isServer)
		{
			_spawnable = null;
		}
		if (Components == null)
		{
			return;
		}
		for (int i = 0; i < Components.Count; i++)
		{
			if (!((Object)(object)Components[i] == (Object)null))
			{
				Components[i].ResetState();
			}
		}
	}

	public virtual float InheritedVelocityScale()
	{
		return 0f;
	}

	public virtual bool InheritedVelocityDirection()
	{
		return true;
	}

	public virtual Vector3 GetInheritedProjectileVelocity(Vector3 direction)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		BaseEntity baseEntity = parentEntity.Get(base.isServer);
		if ((Object)(object)baseEntity == (Object)null)
		{
			return Vector3.zero;
		}
		if (baseEntity.InheritedVelocityDirection())
		{
			return GetParentVelocity() * baseEntity.InheritedVelocityScale();
		}
		return Mathf.Max(Vector3.Dot(GetParentVelocity() * baseEntity.InheritedVelocityScale(), direction), 0f) * direction;
	}

	public virtual Vector3 GetInheritedThrowVelocity(Vector3 direction)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return GetParentVelocity();
	}

	public virtual Vector3 GetInheritedDropVelocity()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		BaseEntity baseEntity = parentEntity.Get(base.isServer);
		if (!((Object)(object)baseEntity != (Object)null))
		{
			return Vector3.zero;
		}
		return baseEntity.GetWorldVelocity();
	}

	public Vector3 GetParentVelocity()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		BaseEntity baseEntity = parentEntity.Get(base.isServer);
		if (!((Object)(object)baseEntity != (Object)null))
		{
			return Vector3.zero;
		}
		return baseEntity.GetWorldVelocity() + (baseEntity.GetAngularVelocity() * ((Component)this).transform.localPosition - ((Component)this).transform.localPosition);
	}

	public Vector3 GetWorldVelocity()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		BaseEntity baseEntity = parentEntity.Get(base.isServer);
		if (!((Object)(object)baseEntity != (Object)null))
		{
			return GetLocalVelocity();
		}
		return baseEntity.GetWorldVelocity() + (baseEntity.GetAngularVelocity() * ((Component)this).transform.localPosition - ((Component)this).transform.localPosition) + ((Component)baseEntity).transform.TransformDirection(GetLocalVelocity());
	}

	public Vector3 GetLocalVelocity()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (base.isServer)
		{
			return GetLocalVelocityServer();
		}
		return Vector3.zero;
	}

	public Quaternion GetAngularVelocity()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (base.isServer)
		{
			return GetAngularVelocityServer();
		}
		return Quaternion.identity;
	}

	public virtual OBB WorldSpaceBounds()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		return new OBB(((Component)this).transform.position, ((Component)this).transform.lossyScale, ((Component)this).transform.rotation, bounds);
	}

	public Vector3 PivotPoint()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((Component)this).transform.position;
	}

	public Vector3 CenterPoint()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return WorldSpaceBounds().position;
	}

	public Vector3 ClosestPoint(Vector3 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		OBB val = WorldSpaceBounds();
		return ((OBB)(ref val)).ClosestPoint(position);
	}

	public virtual Vector3 TriggerPoint()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return CenterPoint();
	}

	public float Distance(Vector3 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ClosestPoint(position) - position;
		return ((Vector3)(ref val)).magnitude;
	}

	public float SqrDistance(Vector3 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = ClosestPoint(position) - position;
		return ((Vector3)(ref val)).sqrMagnitude;
	}

	public float Distance(BaseEntity other)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return Distance(((Component)other).transform.position);
	}

	public float SqrDistance(BaseEntity other)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return SqrDistance(((Component)other).transform.position);
	}

	public float Distance2D(Vector3 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return Vector3Ex.Magnitude2D(ClosestPoint(position) - position);
	}

	public float SqrDistance2D(Vector3 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		return Vector3Ex.SqrMagnitude2D(ClosestPoint(position) - position);
	}

	public float Distance2D(BaseEntity other)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return Distance(((Component)other).transform.position);
	}

	public float SqrDistance2D(BaseEntity other)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return SqrDistance(((Component)other).transform.position);
	}

	public bool IsVisible(Ray ray, int layerMask, float maxDistance)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		if (Vector3Ex.IsNaNOrInfinity(((Ray)(ref ray)).origin))
		{
			return false;
		}
		if (Vector3Ex.IsNaNOrInfinity(((Ray)(ref ray)).direction))
		{
			return false;
		}
		if (((Ray)(ref ray)).direction == Vector3.zero)
		{
			return false;
		}
		OBB val = WorldSpaceBounds();
		RaycastHit val2 = default(RaycastHit);
		if (!((OBB)(ref val)).Trace(ray, ref val2, maxDistance))
		{
			return false;
		}
		if (GamePhysics.Trace(ray, 0f, out var hitInfo, maxDistance, layerMask, (QueryTriggerInteraction)0))
		{
			BaseEntity entity = RaycastHitEx.GetEntity(hitInfo);
			if ((Object)(object)entity == (Object)(object)this)
			{
				return true;
			}
			if ((Object)(object)entity != (Object)null && Object.op_Implicit((Object)(object)GetParentEntity()) && GetParentEntity().EqualNetID((BaseNetworkable)entity) && (RaycastHitEx.IsOnLayer(hitInfo, (Layer)13) || VisibilityPassesThroughParent))
			{
				return true;
			}
			if (((RaycastHit)(ref hitInfo)).distance <= ((RaycastHit)(ref val2)).distance)
			{
				return false;
			}
		}
		return true;
	}

	public bool IsVisibleSpecificLayers(Vector3 position, Vector3 target, int layerMask, float maxDistance = float.PositiveInfinity)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = target - position;
		float magnitude = ((Vector3)(ref val)).magnitude;
		if (magnitude < Mathf.Epsilon)
		{
			return true;
		}
		Vector3 val2 = val / magnitude;
		Vector3 val3 = val2 * Mathf.Min(magnitude, 0.01f);
		return IsVisible(new Ray(position + val3, val2), layerMask, maxDistance);
	}

	public bool IsVisible(Vector3 position, Vector3 target, float maxDistance = float.PositiveInfinity)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = target - position;
		float magnitude = ((Vector3)(ref val)).magnitude;
		if (magnitude < Mathf.Epsilon)
		{
			return true;
		}
		Vector3 val2 = val / magnitude;
		Vector3 val3 = val2 * Mathf.Min(magnitude, 0.01f);
		maxDistance = Mathf.Min(maxDistance, magnitude + 0.2f);
		return IsVisible(new Ray(position + val3, val2), 1218519041, maxDistance);
	}

	public bool IsVisible(Vector3 position, float maxDistance = float.PositiveInfinity)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		Vector3 target = CenterPoint();
		if (IsVisible(position, target, maxDistance))
		{
			return true;
		}
		Vector3 target2 = ClosestPoint(position);
		if (IsVisible(position, target2, maxDistance))
		{
			return true;
		}
		return false;
	}

	public bool IsVisibleAndCanSee(Vector3 position)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = CenterPoint();
		if (IsVisible(position, val) && CanSee(val, position))
		{
			return true;
		}
		Vector3 val2 = ClosestPoint(position);
		if (IsVisible(position, val2) && CanSee(val2, position))
		{
			return true;
		}
		return false;
	}

	public bool IsVisibleAndCanSeeLegacy(Vector3 position, float maxDistance = float.PositiveInfinity)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = CenterPoint();
		if (IsVisible(position, val, maxDistance) && IsVisible(val, position, maxDistance))
		{
			return true;
		}
		Vector3 val2 = ClosestPoint(position);
		if (IsVisible(position, val2, maxDistance) && IsVisible(val2, position, maxDistance))
		{
			return true;
		}
		return false;
	}

	public bool CanSee(Vector3 fromPos, Vector3 targetPos)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return GamePhysics.LineOfSight(fromPos, targetPos, 1218519041, this);
	}

	public bool CanSee(Vector3 fromPos, Vector3 targetPos, LayerMask additionalLayers)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return GamePhysics.LineOfSight(fromPos, targetPos, 0x48A12001 | LayerMask.op_Implicit(additionalLayers), this);
	}

	public bool IsOlderThan(BaseEntity other)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)other == (Object)null)
		{
			return true;
		}
		_003F val = ((_003F?)net?.ID) ?? default(NetworkableId);
		NetworkableId val2 = (NetworkableId)(((_003F?)other.net?.ID) ?? default(NetworkableId));
		return ((NetworkableId)val).Value < val2.Value;
	}

	public virtual bool IsOutside()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		OBB val = WorldSpaceBounds();
		return IsOutside(val.position);
	}

	public bool IsOutside(Vector3 position)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		bool result = true;
		Vector3 val = position + Vector3.up * 100f;
		val.y = Mathf.Max(val.y, TerrainMeta.HeightMap.GetHeight(val) + 1f);
		RaycastHit val2 = default(RaycastHit);
		if (Physics.Linecast(val, position, ref val2, 161546513, (QueryTriggerInteraction)1))
		{
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(((RaycastHit)(ref val2)).collider);
			if ((Object)(object)baseEntity == (Object)null || !baseEntity.HasEntityInParents(this))
			{
				result = false;
			}
		}
		return result;
	}

	public bool IsUnderground(bool cached = true)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (!cached || Time.realtimeSinceStartup > nextHeightCheckTime)
		{
			cachedUnderground = EnvironmentManager.Check(((Component)this).transform.position, EnvironmentType.Underground);
			nextHeightCheckTime = Time.realtimeSinceStartup + 5f;
		}
		return cachedUnderground;
	}

	public virtual float WaterFactor()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		OBB val = WorldSpaceBounds();
		return WaterLevel.Factor(((OBB)(ref val)).ToBounds(), waves: true, volumes: true, this);
	}

	public virtual float AirFactor()
	{
		if (!(WaterFactor() > 0.85f))
		{
			return 1f;
		}
		return 0f;
	}

	public bool WaterTestFromVolumes(Vector3 pos, out WaterLevel.WaterInfo info)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (triggers == null)
		{
			info = default(WaterLevel.WaterInfo);
			return false;
		}
		for (int i = 0; i < triggers.Count; i++)
		{
			if (triggers[i] is WaterVolume waterVolume && waterVolume.Test(pos, out info))
			{
				return true;
			}
		}
		info = default(WaterLevel.WaterInfo);
		return false;
	}

	public bool IsInWaterVolume(Vector3 pos, out bool natural)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		natural = false;
		if (triggers == null)
		{
			return false;
		}
		for (int i = 0; i < triggers.Count; i++)
		{
			if (triggers[i] is WaterVolume waterVolume && waterVolume.Test(pos, out var _))
			{
				natural = waterVolume.naturalSource;
				return true;
			}
		}
		return false;
	}

	public bool WaterTestFromVolumes(Bounds bounds, out WaterLevel.WaterInfo info)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (triggers == null)
		{
			info = default(WaterLevel.WaterInfo);
			return false;
		}
		for (int i = 0; i < triggers.Count; i++)
		{
			if (triggers[i] is WaterVolume waterVolume && waterVolume.Test(bounds, out info))
			{
				return true;
			}
		}
		info = default(WaterLevel.WaterInfo);
		return false;
	}

	public bool WaterTestFromVolumes(Vector3 start, Vector3 end, float radius, out WaterLevel.WaterInfo info)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		if (triggers == null)
		{
			info = default(WaterLevel.WaterInfo);
			return false;
		}
		for (int i = 0; i < triggers.Count; i++)
		{
			if (triggers[i] is WaterVolume waterVolume && waterVolume.Test(start, end, radius, out info))
			{
				return true;
			}
		}
		info = default(WaterLevel.WaterInfo);
		return false;
	}

	public static void WaterTestFromVolumes(ReadOnlySpan<BaseEntity> entities, ReadOnlySpan<Vector3> starts, ReadOnlySpan<Vector3> ends, ReadOnlySpan<float> radii, Span<WaterLevel.WaterInfo> results)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < entities.Length; i++)
		{
			BaseEntity baseEntity = entities[i];
			ref WaterLevel.WaterInfo reference = ref results[i];
			if (baseEntity.triggers == null || baseEntity.triggers.Count == 0)
			{
				reference.isValid = false;
				continue;
			}
			Vector3 start = starts[i];
			Vector3 end = ends[i];
			float radius = radii[i];
			for (int j = 0; j < baseEntity.triggers.Count && (!(baseEntity.triggers[j] is WaterVolume waterVolume) || !waterVolume.Test(start, end, radius, out reference)); j++)
			{
			}
		}
	}

	public static void WaterTestFromVolumesIndirect(ReadOnlySpan<BaseEntity> entities, ReadOnlySpan<Vector3> starts, ReadOnlySpan<Vector3> ends, ReadOnlySpan<float> radii, ReadOnlySpan<int> indices, Span<WaterLevel.WaterInfo> results)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < indices.Length; i++)
		{
			int index = indices[i];
			BaseEntity baseEntity = entities[index];
			ref WaterLevel.WaterInfo reference = ref results[index];
			if (baseEntity.triggers == null || baseEntity.triggers.Count == 0)
			{
				reference.isValid = false;
				continue;
			}
			Vector3 start = starts[index];
			Vector3 end = ends[index];
			float radius = radii[index];
			for (int j = 0; j < baseEntity.triggers.Count && (!(baseEntity.triggers[j] is WaterVolume waterVolume) || !waterVolume.Test(start, end, radius, out reference)); j++)
			{
			}
		}
	}

	public static void WaterTestFromVolumesIndirect(ReadOnlySpan<BaseEntity> entities, ReadOnlySpan<Vector3> poses, ReadOnlySpan<int> indices, Span<WaterLevel.WaterInfo> results)
	{
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < indices.Length; i++)
		{
			int index = indices[i];
			BaseEntity baseEntity = entities[index];
			ref WaterLevel.WaterInfo reference = ref results[index];
			if (baseEntity.triggers == null || baseEntity.triggers.Count == 0)
			{
				reference.isValid = false;
				continue;
			}
			Vector3 pos = poses[index];
			for (int j = 0; j < baseEntity.triggers.Count && (!(baseEntity.triggers[j] is WaterVolume waterVolume) || !waterVolume.Test(pos, out reference)); j++)
			{
			}
		}
	}

	public virtual bool BlocksWaterFor(BasePlayer player)
	{
		return false;
	}

	public virtual bool ForceChildFullStability()
	{
		return true;
	}

	public virtual float Health()
	{
		return 0f;
	}

	public virtual float MaxHealth()
	{
		return 0f;
	}

	public virtual float AntiHackVelocity()
	{
		return 0f;
	}

	public virtual float AntiHackPadding()
	{
		return 0.1f;
	}

	public virtual float PenetrationResistance(HitInfo info)
	{
		return 100f;
	}

	public virtual GameObjectRef GetImpactEffect(HitInfo info)
	{
		return impactEffect;
	}

	public virtual void OnAttacked(HitInfo info)
	{
	}

	public virtual Item GetItem()
	{
		return null;
	}

	public virtual Item GetItem(ItemId itemId)
	{
		return null;
	}

	public virtual void GiveItem(Item item, GiveItemReason reason = GiveItemReason.Generic, GiveItemOptions options = GiveItemOptions.None)
	{
		item.Remove();
	}

	public virtual bool CanBeLooted(BasePlayer player)
	{
		return !IsTransferring();
	}

	public virtual BaseEntity GetEntity()
	{
		return this;
	}

	public override string ToString()
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		if (_name == null)
		{
			if (base.isServer)
			{
				if (net == null)
				{
					return base.ShortPrefabName;
				}
				_name = $"{base.ShortPrefabName}[{net.ID}]";
			}
			else
			{
				_name = base.ShortPrefabName;
			}
		}
		return _name;
	}

	public virtual string Categorize()
	{
		return "entity";
	}

	public void Log(string str)
	{
		if (base.isClient)
		{
			Debug.Log((object)("<color=#ffa>[" + ((object)this).ToString() + "] " + str + "</color>"), (Object)(object)((Component)this).gameObject);
		}
		else
		{
			Debug.Log((object)("<color=#aff>[" + ((object)this).ToString() + "] " + str + "</color>"), (Object)(object)((Component)this).gameObject);
		}
	}

	public void SetModel(Model mdl)
	{
		if (!((Object)(object)model == (Object)(object)mdl))
		{
			model = mdl;
		}
	}

	public Model GetModel()
	{
		return model;
	}

	public virtual Transform[] GetBones()
	{
		if (Object.op_Implicit((Object)(object)model))
		{
			return model.GetBones();
		}
		return null;
	}

	public virtual Transform FindBone(string strName)
	{
		if (Object.op_Implicit((Object)(object)model))
		{
			return model.FindBone(strName);
		}
		return ((Component)this).transform;
	}

	public virtual uint FindBoneID(Transform boneTransform)
	{
		if (Object.op_Implicit((Object)(object)model))
		{
			return model.FindBoneID(boneTransform);
		}
		return StringPool.closest;
	}

	public virtual Transform FindClosestBone(Vector3 worldPos)
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if (Object.op_Implicit((Object)(object)model))
		{
			return model.FindClosestBone(worldPos);
		}
		return ((Component)this).transform;
	}

	public virtual bool ShouldBlockProjectiles()
	{
		return true;
	}

	public virtual bool ShouldInheritNetworkGroup()
	{
		return true;
	}

	public virtual bool SupportsChildDeployables()
	{
		return false;
	}

	public virtual bool ForceDeployableSetParent()
	{
		return false;
	}

	public virtual bool ShouldAlwaysBlockNoClipChecks()
	{
		return false;
	}

	public virtual bool ShouldUseCastNoClipChecks()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		Vector3 worldVelocity = GetWorldVelocity();
		return ((Vector3)(ref worldVelocity)).magnitude > 0f;
	}

	public bool IsOnMovingObject()
	{
		if (syncPosition)
		{
			return true;
		}
		BaseEntity baseEntity = GetParentEntity();
		if (!((Object)(object)baseEntity != (Object)null))
		{
			return false;
		}
		return baseEntity.IsOnMovingObject();
	}

	public bool HasParentBoat(out BaseBoat parentBoat)
	{
		BaseEntity baseEntity = GetParentEntity();
		parentBoat = null;
		while ((Object)(object)baseEntity != (Object)null)
		{
			if (baseEntity is PlayerBoat || baseEntity is Tugboat)
			{
				parentBoat = baseEntity as BaseBoat;
				return true;
			}
			baseEntity = baseEntity.GetParentEntity();
		}
		return false;
	}

	public void BroadcastEntityMessage(string msg, float radius = 20f, int layerMask = 1218652417)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		if (base.isClient)
		{
			return;
		}
		List<BaseEntity> list = Pool.Get<List<BaseEntity>>();
		Vis.Entities(((Component)this).transform.position, radius, list, layerMask, (QueryTriggerInteraction)2);
		foreach (BaseEntity item in list)
		{
			if (item.isServer)
			{
				item.OnEntityMessage(this, msg);
			}
		}
		Pool.FreeUnmanaged<BaseEntity>(ref list);
	}

	public virtual void OnEntityMessage(BaseEntity from, string msg)
	{
	}

	public T AddComponent<T>() where T : EntityComponentBase
	{
		T val = ((Component)this).gameObject.AddComponent<T>();
		_components.Add(val);
		return val;
	}

	public override void Save(SaveInfo info)
	{
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Unknown result type (might be due to invalid IL or missing references)
		//IL_0326: Unknown result type (might be due to invalid IL or missing references)
		//IL_0342: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_0363: Unknown result type (might be due to invalid IL or missing references)
		//IL_0368: Unknown result type (might be due to invalid IL or missing references)
		//IL_0384: Unknown result type (might be due to invalid IL or missing references)
		//IL_0389: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d2: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		BaseEntity baseEntity = parentEntity.Get(base.isServer);
		info.msg.baseEntity = Pool.Get<BaseEntity>();
		Quaternion val;
		if (info.forDisk)
		{
			if (this is BasePlayer)
			{
				if ((Object)(object)baseEntity == (Object)null || baseEntity.enableSaving)
				{
					info.msg.baseEntity.pos = ((Component)this).transform.localPosition;
					BaseEntity baseEntity2 = info.msg.baseEntity;
					val = ((Component)this).transform.localRotation;
					baseEntity2.rot = ((Quaternion)(ref val)).eulerAngles;
				}
				else
				{
					info.msg.baseEntity.pos = ((Component)this).transform.position;
					BaseEntity baseEntity3 = info.msg.baseEntity;
					val = ((Component)this).transform.rotation;
					baseEntity3.rot = ((Quaternion)(ref val)).eulerAngles;
				}
			}
			else
			{
				info.msg.baseEntity.pos = ((Component)this).transform.localPosition;
				BaseEntity baseEntity4 = info.msg.baseEntity;
				val = ((Component)this).transform.localRotation;
				baseEntity4.rot = ((Quaternion)(ref val)).eulerAngles;
			}
		}
		else
		{
			info.msg.baseEntity.pos = GetNetworkPosition();
			BaseEntity baseEntity5 = info.msg.baseEntity;
			val = GetNetworkRotation();
			baseEntity5.rot = ((Quaternion)(ref val)).eulerAngles;
			info.msg.baseEntity.time = GetNetworkTime(in info.cachedTime);
			if (networkEntityScale)
			{
				TransformHandle handle;
				if (BaseNetworkable.UseParallelSaves)
				{
					BaseEntity baseEntity6 = info.msg.baseEntity;
					handle = base.TransformHandle;
					baseEntity6.scale = Facepunch.Extend.TransformEx.Unsafe.GetLocalScaleMT(in handle);
				}
				else
				{
					BaseEntity baseEntity7 = info.msg.baseEntity;
					handle = base.TransformHandle;
					baseEntity7.scale = ((TransformHandle)(ref handle)).localScale;
				}
			}
		}
		info.msg.baseEntity.flags = (int)flags;
		info.msg.baseEntity.skinid = skinID;
		info.msg.baseEntity.attachmentID = attachmentID;
		if (info.forDisk && this is BasePlayer)
		{
			if ((Object)(object)baseEntity != (Object)null && baseEntity.enableSaving)
			{
				info.msg.parent = Pool.Get<ParentInfo>();
				info.msg.parent.uid = parentEntity.uid;
				info.msg.parent.bone = parentBone;
			}
		}
		else if ((Object)(object)baseEntity != (Object)null)
		{
			info.msg.parent = Pool.Get<ParentInfo>();
			info.msg.parent.uid = parentEntity.uid;
			info.msg.parent.bone = parentBone;
		}
		if (HasAnySlot())
		{
			info.msg.entitySlots = Pool.Get<EntitySlots>();
			info.msg.entitySlots.slotLock = entitySlots[0].uid;
			info.msg.entitySlots.slotFireMod = entitySlots[1].uid;
			info.msg.entitySlots.slotUpperModification = entitySlots[2].uid;
			info.msg.entitySlots.centerDecoration = entitySlots[5].uid;
			info.msg.entitySlots.lowerCenterDecoration = entitySlots[6].uid;
			info.msg.entitySlots.storageMonitor = entitySlots[7].uid;
		}
		if (info.forDisk && Object.op_Implicit((Object)(object)_spawnable))
		{
			_spawnable.Save(info);
		}
		if (info.msg.baseEntity != null)
		{
			AutoSaveSyncVars(info);
		}
		if (ShouldNetworkOwnerInfo() || (OwnerID != 0L && info.forDisk))
		{
			info.msg.ownerInfo = Pool.Get<OwnerInfo>();
			if (info.forDisk)
			{
				info.msg.ownerInfo.steamid = OwnerID;
			}
			else
			{
				info.msg.ownerInfo.steamid = ((OwnerID == info.forConnection.userid) ? info.forConnection.userid : 0);
			}
		}
		if (Components != null)
		{
			for (int i = 0; i < Components.Count; i++)
			{
				if (!((Object)(object)Components[i] == (Object)null))
				{
					Components[i].SaveComponent(info);
				}
			}
		}
		if (info.forTransfer && ShouldTransferAssociatedFiles)
		{
			info.msg.associatedFiles = Pool.Get<AssociatedFiles>();
			info.msg.associatedFiles.files = Pool.Get<List<AssociatedFile>>();
			info.msg.associatedFiles.files.AddRange(FileStorage.server.QueryAllByEntity(net.ID));
		}
	}

	public override bool CanUseNetworkCache(Connection connection)
	{
		if (ShouldNetworkOwnerInfo())
		{
			return false;
		}
		return base.CanUseNetworkCache(connection);
	}

	public virtual bool ShouldNetworkOwnerInfo()
	{
		return false;
	}

	public override void Load(LoadInfo info)
	{
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_034b: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg.baseEntity != null)
		{
			BaseEntity baseEntity = info.msg.baseEntity;
			Flags old = flags;
			if (base.isServer)
			{
				baseEntity.flags &= -33554433;
			}
			flags = (Flags)baseEntity.flags;
			OnFlagsChanged(old, flags);
			OnSkinChanged(skinID, info.msg.baseEntity.skinid);
			OnAttachmentChanged(attachmentID, info.msg.baseEntity.attachmentID);
			if (info.fromDisk)
			{
				if (Vector3Ex.IsNaNOrInfinity(baseEntity.pos))
				{
					Debug.LogWarning((object)(((object)this).ToString() + " has broken position - " + ((object)System.Runtime.CompilerServices.Unsafe.As<Vector3, Vector3>(ref baseEntity.pos)/*cast due to constrained. prefix*/).ToString()));
					baseEntity.pos = Vector3.zero;
				}
				((Component)this).transform.localPosition = baseEntity.pos;
				((Component)this).transform.localRotation = Quaternion.Euler(baseEntity.rot);
			}
		}
		if (info.msg.entitySlots != null)
		{
			entitySlots[0].uid = info.msg.entitySlots.slotLock;
			entitySlots[1].uid = info.msg.entitySlots.slotFireMod;
			entitySlots[2].uid = info.msg.entitySlots.slotUpperModification;
			entitySlots[5].uid = info.msg.entitySlots.centerDecoration;
			entitySlots[6].uid = info.msg.entitySlots.lowerCenterDecoration;
			entitySlots[7].uid = info.msg.entitySlots.storageMonitor;
		}
		else
		{
			for (int i = 0; i < entitySlots.Length; i++)
			{
				entitySlots[i] = default(EntityRef);
			}
		}
		if (info.msg.parent != null)
		{
			if (base.isServer)
			{
				BaseEntity entity = BaseNetworkable.serverEntities.Find(info.msg.parent.uid) as BaseEntity;
				SetParent(entity, info.msg.parent.bone);
			}
			parentEntity.uid = info.msg.parent.uid;
			parentBone = info.msg.parent.bone;
		}
		else
		{
			parentEntity.uid = default(NetworkableId);
			parentBone = 0u;
		}
		if (info.msg.ownerInfo != null)
		{
			OwnerID = info.msg.ownerInfo.steamid;
		}
		if (Object.op_Implicit((Object)(object)_spawnable))
		{
			_spawnable.Load(info);
		}
		if (info.fromTransfer && ShouldTransferAssociatedFiles && info.msg.associatedFiles != null && info.msg.associatedFiles.files != null)
		{
			foreach (AssociatedFile file in info.msg.associatedFiles.files)
			{
				if (FileStorage.server.Store(file.data, (FileStorage.Type)file.type, net.ID, file.numID) != file.crc)
				{
					Debug.LogWarning((object)"Associated file has a different CRC after transfer!");
				}
			}
		}
		if (info.fromDisk && info.msg.baseEntity != null && IsTransferProtected())
		{
			float num = ((info.msg.baseEntity.protection > 0f) ? info.msg.baseEntity.protection : Nexus.protectionDuration);
			_transferProtectionRemaining = TimeUntil.op_Implicit(num);
			Invoke(DisableTransferProtectionAction, num);
		}
		if (info.msg.baseEntity != null)
		{
			AutoLoadSyncVars(info);
		}
		if (Components == null)
		{
			return;
		}
		for (int j = 0; j < Components.Count; j++)
		{
			if (!((Object)(object)Components[j] == (Object)null))
			{
				Components[j].LoadComponent(info);
			}
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, byte arg1, bool arg2)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.UInt8(arg1);
			netWrite.Bool(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, NetworkableId arg1, NetworkableId arg2)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.EntityID(arg1);
			netWrite.EntityID(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, string arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.String(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, ChickenCoopStatusUpdate arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<ChickenCoopStatusUpdate>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, int arg1, int arg2, float arg3, float arg4)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int32(arg1);
			netWrite.Int32(arg2);
			netWrite.Float(arg3);
			netWrite.Float(arg4);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, int arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int32(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, ulong arg1, ulong arg2)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.UInt64(arg1);
			netWrite.UInt64(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, ulong arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.UInt64(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, int arg1, int arg2)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int32(arg1);
			netWrite.Int32(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, Vector3 arg1, float arg2, uint arg3)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Vector3(in arg1);
			netWrite.Float(arg2);
			netWrite.UInt32(arg3);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, NetworkableId arg1)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.EntityID(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, Vector3 arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Vector3(in arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, VendingMachineLongTermStats arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<VendingMachineLongTermStats>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, int arg1, VendingMachinePurchaseHistoryMessage arg2)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int32(arg1);
			netWrite.Proto<VendingMachinePurchaseHistoryMessage>(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, SellOrderContainer arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<SellOrderContainer>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, float arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Float(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, bool arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Bool(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, sbyte arg1, sbyte arg2, sbyte arg3)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int8(arg1);
			netWrite.Int8(arg2);
			netWrite.Int8(arg3);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, uint arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.UInt32(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, uint arg1, string arg2)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.UInt32(arg1);
			netWrite.String(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, ArcadeGame arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<ArcadeGame>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, EntityIdList arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<EntityIdList>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, int arg1, int arg2, bool arg3)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int32(arg1);
			netWrite.Int32(arg2);
			netWrite.Bool(arg3);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, float arg1, string arg2)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Float(arg1);
			netWrite.String(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, int arg1, ConversationResponseStatesList arg2)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int32(arg1);
			netWrite.Proto<ConversationResponseStatesList>(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, int arg1, int arg2, bool arg3, ConversationResponseStatesList arg4)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int32(arg1);
			netWrite.Int32(arg2);
			netWrite.Bool(arg3);
			netWrite.Proto<ConversationResponseStatesList>(arg4);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, bool arg1, Vector3 arg2)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Bool(arg1);
			netWrite.Vector3(in arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, Vector3 arg1, float arg2, Vector3 arg3, float arg4, float arg5)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Vector3(in arg1);
			netWrite.Float(arg2);
			netWrite.Vector3(in arg3);
			netWrite.Float(arg4);
			netWrite.Float(arg5);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, int arg1, NetworkableId arg2)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int32(arg1);
			netWrite.EntityID(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, WireReconnectMessage arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<WireReconnectMessage>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, float arg1, float arg2)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Float(arg1);
			netWrite.Float(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, BasePlayer arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite write = ClientRPCStart(target.Function);
			write.Player(arg1);
			ClientRPCSend(write, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, bool arg1, ulong arg2)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Bool(arg1);
			netWrite.UInt64(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, Vector3 arg1, Vector3 arg2)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Vector3(in arg1);
			netWrite.Vector3(in arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, Vector3 arg1, string arg2)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Vector3(in arg1);
			netWrite.String(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, int arg1, int arg2, int arg3, Vector3 arg4, ReadOnlySpan<byte> arg5)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int32(arg1);
			netWrite.Int32(arg2);
			netWrite.Int32(arg3);
			netWrite.Vector3(in arg4);
			netWrite.Bytes(arg5);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, ReadOnlySpan<byte> arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Bytes(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, string arg1, int arg2)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.String(arg1);
			netWrite.Int32(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, uint arg1, uint arg2, ReadOnlySpan<byte> arg3, uint arg4, byte arg5)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.UInt32(arg1);
			netWrite.UInt32(arg2);
			netWrite.Bytes(arg3);
			netWrite.UInt32(arg4);
			netWrite.UInt8(arg5);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, NetworkableId arg1, ReadOnlySpan<byte> arg2)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.EntityID(arg1);
			netWrite.Bytes(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, string arg1, CopyPasteEntityInfo arg2)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.String(arg1);
			netWrite.Proto<CopyPasteEntityInfo>(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, AIDesign arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<AIDesign>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, int arg1, int arg2, int arg3)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int32(arg1);
			netWrite.Int32(arg2);
			netWrite.Int32(arg3);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, ApartmentTerminalData arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<ApartmentTerminalData>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, NetworkableId arg1, uint arg2, uint arg3, int arg4, int arg5)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.EntityID(arg1);
			netWrite.UInt32(arg2);
			netWrite.UInt32(arg3);
			netWrite.Int32(arg4);
			netWrite.Int32(arg5);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, int arg1, ReadOnlySpan<byte> arg2)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int32(arg1);
			netWrite.Bytes(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, PhoneDirectory arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<PhoneDirectory>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, int arg1, uint arg2)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int32(arg1);
			netWrite.UInt32(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, string arg1, string arg2)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.String(arg1);
			netWrite.String(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, ItemAmountList arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<ItemAmountList>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, int arg1, string arg2, ulong arg3)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int32(arg1);
			netWrite.String(arg2);
			netWrite.UInt64(arg3);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, int arg1, ulong arg2)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int32(arg1);
			netWrite.UInt64(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, int arg1, float arg2)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int32(arg1);
			netWrite.Float(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, int arg1, ItemId arg2)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int32(arg1);
			netWrite.ItemID(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, Vector3 arg1, float arg2)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Vector3(in arg1);
			netWrite.Float(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, int arg1, int arg2, int arg3, float arg4)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int32(arg1);
			netWrite.Int32(arg2);
			netWrite.Int32(arg3);
			netWrite.Float(arg4);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, bool arg1, bool arg2)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Bool(arg1);
			netWrite.Bool(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, ulong arg1, int arg2)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.UInt64(arg1);
			netWrite.Int32(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, DisplayingBoxStorage arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<DisplayingBoxStorage>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, uint arg1, ReadOnlySpan<byte> arg2)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.UInt32(arg1);
			netWrite.Bytes(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, GlobalEntityCollection arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<GlobalEntityCollection>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, GrowableEntity arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<GrowableEntity>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, uint arg1, Vector3 arg2)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.UInt32(arg1);
			netWrite.Vector3(in arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, RoundResults arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<RoundResults>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, CardList arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<CardList>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, NetworkableId arg1, Vector3 arg2, Vector3 arg3)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.EntityID(arg1);
			netWrite.Vector3(in arg2);
			netWrite.Vector3(in arg3);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, PlayerModifiers arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<PlayerModifiers>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, Vector3 arg1, NetworkableId arg2)
	{
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Vector3(in arg1);
			netWrite.EntityID(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, uint arg1, ReadOnlySpan<byte> arg2, string arg3, uint arg4, int arg5)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.UInt32(arg1);
			netWrite.Bytes(arg2);
			netWrite.String(arg3);
			netWrite.UInt32(arg4);
			netWrite.Int32(arg5);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, PlayerTeam arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<PlayerTeam>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, string arg1, ulong arg2, ulong arg3)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.String(arg1);
			netWrite.UInt64(arg2);
			netWrite.UInt64(arg3);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, MapNote arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<MapNote>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, MapNoteList arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<MapNoteList>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, MissionAcceptStatesList arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<MissionAcceptStatesList>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, uint arg1, int arg2)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.UInt32(arg1);
			netWrite.Int32(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, ModelState arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<ModelState>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, uint arg1, ulong arg2)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.UInt32(arg1);
			netWrite.UInt64(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, uint arg1, NetworkableId arg2)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.UInt32(arg1);
			netWrite.EntityID(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, RespawnInformation arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<RespawnInformation>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, Vector3 arg1, int arg2, int arg3)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Vector3(in arg1);
			netWrite.Int32(arg2);
			netWrite.Int32(arg3);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, OceanPaths arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<OceanPaths>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, SpectateTeamInfo arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<SpectateTeamInfo>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, int arg1, Vector3 arg2)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int32(arg1);
			netWrite.Vector3(in arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, float arg1, float arg2, float arg3)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Float(arg1);
			netWrite.Float(arg2);
			netWrite.Float(arg3);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, NetworkableId arg1, string arg2, string arg3)
	{
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.EntityID(arg1);
			netWrite.String(arg2);
			netWrite.String(arg3);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, UpdateItemContainer arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<UpdateItemContainer>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, PlayerUpdateLoot arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<PlayerUpdateLoot>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, PlayerMetabolism arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<PlayerMetabolism>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, Vector3 arg1, Vector3 arg2, int arg3)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Vector3(in arg1);
			netWrite.Vector3(in arg2);
			netWrite.Int32(arg3);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, float arg1, int arg2, float arg3, int arg4)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Float(arg1);
			netWrite.Int32(arg2);
			netWrite.Float(arg3);
			netWrite.Int32(arg4);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, int arg1, int arg2, float arg3, float arg4, float arg5, int arg6, float arg7, float arg8)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int32(arg1);
			netWrite.Int32(arg2);
			netWrite.Float(arg3);
			netWrite.Float(arg4);
			netWrite.Float(arg5);
			netWrite.Int32(arg6);
			netWrite.Float(arg7);
			netWrite.Float(arg8);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, float arg1, float arg2, float arg3, float arg4, int arg5, float arg6, float arg7)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Float(arg1);
			netWrite.Float(arg2);
			netWrite.Float(arg3);
			netWrite.Float(arg4);
			netWrite.Int32(arg5);
			netWrite.Float(arg6);
			netWrite.Float(arg7);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, Vector3 arg1, bool arg2)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Vector3(in arg1);
			netWrite.Bool(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, float arg1, float arg2, byte arg3, float arg4, byte arg5, float arg6)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Float(arg1);
			netWrite.Float(arg2);
			netWrite.UInt8(arg3);
			netWrite.Float(arg4);
			netWrite.UInt8(arg5);
			netWrite.Float(arg6);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, float arg1, float arg2, byte arg3, float arg4, float arg5, float arg6)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Float(arg1);
			netWrite.Float(arg2);
			netWrite.UInt8(arg3);
			netWrite.Float(arg4);
			netWrite.Float(arg5);
			netWrite.Float(arg6);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, float arg1, float arg2, byte arg3, float arg4, float arg5)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Float(arg1);
			netWrite.Float(arg2);
			netWrite.UInt8(arg3);
			netWrite.Float(arg4);
			netWrite.Float(arg5);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, ushort arg1, byte arg2)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.UInt16(arg1);
			netWrite.UInt8(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, short arg1, int arg2, int arg3)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int16(arg1);
			netWrite.Int32(arg2);
			netWrite.Int32(arg3);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, short arg1, short arg2)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int16(arg1);
			netWrite.Int16(arg2);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, float arg1, float arg2, byte arg3, byte arg4, byte arg5)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Float(arg1);
			netWrite.Float(arg2);
			netWrite.UInt8(arg3);
			netWrite.UInt8(arg4);
			netWrite.UInt8(arg5);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, float arg1, byte arg2, float arg3, byte arg4, bool arg5)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Float(arg1);
			netWrite.UInt8(arg2);
			netWrite.Float(arg3);
			netWrite.UInt8(arg4);
			netWrite.Bool(arg5);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, float arg1, byte arg2, float arg3, float arg4)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Float(arg1);
			netWrite.UInt8(arg2);
			netWrite.Float(arg3);
			netWrite.Float(arg4);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, float arg1, byte arg2, int arg3, float arg4)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Float(arg1);
			netWrite.UInt8(arg2);
			netWrite.Int32(arg3);
			netWrite.Float(arg4);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, sbyte arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int8(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, UpdateItem arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<UpdateItem>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, IndustrialConveyorTransfer arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<IndustrialConveyorTransfer>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, ItemFilterList arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<ItemFilterList>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, ClanActionResult arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<ClanActionResult>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, ClanLog arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<ClanLog>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, ClanScoreEvents arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<ClanScoreEvents>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, ClanInvitations arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<ClanInvitations>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, ClanLeaderboard arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<ClanLeaderboard>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, long arg1, string arg2, int arg3, Color32 arg4)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int64(arg1);
			netWrite.String(arg2);
			netWrite.Int32(arg3);
			netWrite.Color32(in arg4);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, long arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Int64(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, string arg1, int arg2, bool arg3)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.String(arg1);
			netWrite.Int32(arg2);
			netWrite.Bool(arg3);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, Tree arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<Tree>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, TreeList arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<TreeList>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, PlayerRelationships arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<PlayerRelationships>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, Ragdoll arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<Ragdoll>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, CustomPie arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<CustomPie>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, CustomVitals arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<CustomVitals>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	[PoolAnalyzerNonCaching]
	public void ClientRPC(RpcTarget target, CommunityEntity_DestroyUIs arg1)
	{
		if (Net.sv.IsConnected() && net != null)
		{
			GetRpcTargetNetworkGroup(ref target);
			NetWrite netWrite = ClientRPCStart(target.Function);
			netWrite.Proto<CommunityEntity_DestroyUIs>(arg1);
			ClientRPCSend(netWrite, target.Connections);
			FreeRPCTarget(target);
		}
	}

	public BaseEntity()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		links = new List<EntityLink>();
		oldPosLSFrame = int.MinValue;
		oldPosLS = Vector3.negativeInfinity;
		entitySlots = new EntityRef[8];
		isVisible = true;
		isAnimatorVisible = true;
		isShadowVisible = true;
		localOccludee = new OccludeeSphere(-1);
		enableSaving = true;
		base._002Ector();
	}
}
