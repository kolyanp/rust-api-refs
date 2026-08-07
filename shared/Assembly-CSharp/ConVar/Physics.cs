using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ConVar;

[Factory("physics")]
public class Physics : ConsoleSystem
{
	private class PhysxCell
	{
		public Bounds Bounds;

		public Vector2i GridPosition;

		public int Id;

		public List<Collider> Colliders;
	}

	[ServerVar(Help = "The collision detection mode that dropped items and corpses should use")]
	public static int droppedmode = 2;

	[ServerVar(Help = "Send effects to clients when physics objects collide")]
	public static bool sendeffects = true;

	[ServerVar(Help = "(Generated) When enabled, logs ground-watch trigger events to the console, showing when players are detected as off the ground or falling through the world")]
	public static bool groundwatchdebug = false;

	[ServerVar(Help = "(Generated) Number of consecutive ground-watch failures allowed before corrective action is taken on a player who appears to be falling through geometry")]
	public static int groundwatchfails = 1;

	[ServerVar(Help = "(Generated) Seconds between ground-watch checks on a player; lower values detect world-fall issues faster but increase CPU overhead")]
	public static float groundwatchdelay = 0.1f;

	[ServerVar(Help = "The collision detection mode that server-side ragdolls should use")]
	public static int serverragdollmode = 3;

	private const float baseGravity = -9.81f;

	private static bool _serversideragdolls = false;

	[ServerVar(Help = "(Generated) Maximum linear acceleration (m/s^2) that a vehicle towing joint can apply before the joint breaks; prevents unrealistic joint forces during towing")]
	public static float towingmaxlinearaccelfromjoint = 40f;

	[ServerVar(Help = "(Generated) When enabled, players can be temporarily ragdolled by large physics impacts (e.g. explosions) before recovering; disabling keeps players standing")]
	public static bool allowplayertempragdoll = true;

	[ServerVar(Help = "(Generated) When enabled, horses can be temporarily ragdolled by large physics impacts; disabling keeps horses upright during collisions")]
	public static bool allowhorsetempragdoll = true;

	[ServerVar(Help = "(Generated) When enabled, physics transform syncs are batched per frame for efficiency; disable to force immediate per-object sync")]
	[ClientVar(Help = "(Generated) When enabled, physics transform syncs are batched per frame for efficiency; disable to force immediate per-object sync")]
	public static bool batchsynctransforms = true;

	private static bool _treecollision = true;

	private static Bounds _currentBounds;

	public static Bounds DeepSeaDisabledBounds;

	public static Bounds DeepSeaEnabledBounds;

	[ServerVar(Help = "(Generated) Minimum relative velocity at which a physics collision generates a bounce response; lower values cause more objects to bounce on light impacts")]
	public static float bouncethreshold
	{
		get
		{
			return Physics.bounceThreshold;
		}
		set
		{
			Physics.bounceThreshold = value;
		}
	}

	[ServerVar(Help = "(Generated) Energy threshold below which a rigid body is put to sleep by the physics engine; lower values keep more objects awake, higher values reduce CPU usage")]
	public static float sleepthreshold
	{
		get
		{
			return Physics.sleepThreshold;
		}
		set
		{
			Physics.sleepThreshold = value;
		}
	}

	[ServerVar(Help = "The default solver iteration count permitted for any rigid bodies (default 7). Must be positive")]
	public static int solveriterationcount
	{
		get
		{
			return Physics.defaultSolverIterations;
		}
		set
		{
			Physics.defaultSolverIterations = value;
		}
	}

	[ReplicatedVar(Help = "Gravity multiplier", Default = "1.0")]
	public static float gravity
	{
		get
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return Physics.gravity.y / -9.81f;
		}
		set
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			Physics.gravity = new Vector3(0f, value * -9.81f, 0f);
		}
	}

	[ReplicatedVar(Help = "Do ragdoll physics calculations on the server, or use the old client-side system", Saved = true, ShowInAdminUI = true)]
	public static bool serversideragdolls
	{
		get
		{
			return _serversideragdolls;
		}
		set
		{
			_serversideragdolls = value;
			Physics.IgnoreLayerCollision(9, 13, !_serversideragdolls);
			Physics.IgnoreLayerCollision(9, 11, !_serversideragdolls);
			Physics.IgnoreLayerCollision(9, 28, !_serversideragdolls);
		}
	}

	[ClientVar(Help = "(Generated) When enabled, Unity Physics auto-syncs transform changes to physics each frame; disable to manually control when transforms sync")]
	[ServerVar(Help = "(Generated) When enabled, Unity Physics auto-syncs transform changes to physics each frame; disable to manually control when transforms sync")]
	public static bool autosynctransforms
	{
		get
		{
			return Physics.autoSyncTransforms;
		}
		set
		{
			Physics.autoSyncTransforms = value;
		}
	}

	[ReplicatedVar(Help = "Do players and vehicles collide with trees?", Saved = true, ShowInAdminUI = true)]
	public static bool treecollision
	{
		get
		{
			return _treecollision;
		}
		set
		{
			_treecollision = value;
			Physics.IgnoreLayerCollision(15, 30, !_treecollision);
			Physics.IgnoreLayerCollision(12, 30, !_treecollision);
		}
	}

	internal static void ApplyDropped(Rigidbody rigidBody)
	{
		if (droppedmode <= 0)
		{
			rigidBody.collisionDetectionMode = (CollisionDetectionMode)0;
		}
		if (droppedmode == 1)
		{
			rigidBody.collisionDetectionMode = (CollisionDetectionMode)1;
		}
		if (droppedmode == 2)
		{
			rigidBody.collisionDetectionMode = (CollisionDetectionMode)2;
		}
		if (droppedmode >= 3)
		{
			rigidBody.collisionDetectionMode = (CollisionDetectionMode)3;
		}
	}

	[ServerVar(Help = "(Generated) Prints a sorted table of physics cells and how many colliders each contains; helps identify areas with excessive collider density causing physics slowdowns")]
	public static void print_colliders_per_cell(Arg arg)
	{
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		List<PhysxCell> collidersPerBroadphaseCell = GetCollidersPerBroadphaseCell();
		if (collidersPerBroadphaseCell.Count == 0)
		{
			arg.ReplyWith("No colliders found");
		}
		int num = collidersPerBroadphaseCell.Sum((PhysxCell x) => x.Colliders.Count);
		StringBuilder stringBuilder = new StringBuilder();
		PhysxCell[] array = collidersPerBroadphaseCell.OrderByDescending((PhysxCell x) => x.Colliders.Count).ToArray();
		stringBuilder.AppendLine($"Found {num} in {array.Length} cells, cell size {((Bounds)(ref array[0].Bounds)).size}");
		PhysxCell[] array2 = array;
		foreach (PhysxCell physxCell in array2)
		{
			if (physxCell.Colliders.Count != 0)
			{
				stringBuilder.AppendLine(string.Format("Id: {0} Position: {1} Center: {2} Count: {3}", new object[4]
				{
					physxCell.Id,
					physxCell.GridPosition,
					((Bounds)(ref physxCell.Bounds)).center,
					physxCell.Colliders.Count
				}));
			}
		}
		arg.ReplyWith(stringBuilder.ToString());
	}

	[ServerVar(Help = "(Generated) Prints a sorted table of prefab names and their collider counts; identifies prefabs with unusually high collider counts for optimisation")]
	public static void print_colliders_per_prefab(Arg arg)
	{
		ICollection<Collider> collection = null;
		int cellId = arg.GetInt(0, -1);
		if (cellId >= 0 && cellId < 256)
		{
			PhysxCell physxCell = GetCollidersPerBroadphaseCell().FirstOrDefault((PhysxCell x) => x.Id == cellId);
			if (physxCell == null)
			{
				arg.ReplyWith($"Cell Id '{cellId}' not found");
				return;
			}
			collection = physxCell.Colliders;
		}
		if (collection == null)
		{
			collection = GetAllColliders();
		}
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		foreach (Collider item in collection)
		{
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(item);
			string text = "NULL";
			text = ((!((Object)(object)baseEntity == (Object)null) && !baseEntity.IsDestroyed) ? baseEntity.ShortPrefabName : ((Object)item).name);
			dictionary.TryGetValue(text, out var value);
			dictionary[text] = value + 1;
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (collection.Count == 0)
		{
			Debug.Log((object)"No colliders found");
		}
		stringBuilder.AppendLine($"Found {collection.Count} colliders in {dictionary.Count} unique prefabs");
		foreach (KeyValuePair<string, int> item2 in dictionary.OrderByDescending((KeyValuePair<string, int> x) => x.Value))
		{
			stringBuilder.AppendLine($"Entity: {item2.Key} Count: {item2.Value}");
		}
		arg.ReplyWith(stringBuilder.ToString());
	}

	private static ICollection<Collider> GetAllColliders()
	{
		return (from collider in Object.FindObjectsByType<Collider>((FindObjectsInactive)0, (FindObjectsSortMode)0)
			where (Object)(object)collider != (Object)null && (Object)(object)((Component)collider).transform != (Object)null && collider.enabled
			select collider).ToArray();
	}

	private static List<PhysxCell> GetCollidersPerBroadphaseCell()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		ICollection<Collider> allColliders = GetAllColliders();
		int subdivisions = 16;
		Vector3 cellSize = new Vector3(((Bounds)(ref _currentBounds)).size.x / (float)subdivisions, ((Bounds)(ref _currentBounds)).size.y, ((Bounds)(ref _currentBounds)).size.z / (float)subdivisions);
		int num = 0;
		Dictionary<Vector2i, List<Collider>> dictionary = new Dictionary<Vector2i, List<Collider>>();
		foreach (Collider item in allColliders)
		{
			if ((Object)(object)item == (Object)null || (Object)(object)((Component)item).transform == (Object)null || !item.enabled)
			{
				continue;
			}
			Vector2i physxCell = GetPhysxCell(((Component)item).transform.position, cellSize);
			if (physxCell.x < 0 || physxCell.y < 0 || physxCell.x >= subdivisions || physxCell.y >= subdivisions)
			{
				num++;
				continue;
			}
			_ = subdivisions;
			if (!dictionary.TryGetValue(physxCell, out var value))
			{
				value = (dictionary[physxCell] = new List<Collider>());
			}
			value.Add(item);
		}
		return dictionary.Select((KeyValuePair<Vector2i, List<Collider>> x) => new PhysxCell
		{
			GridPosition = x.Key,
			Bounds = new Bounds(((Bounds)(ref _currentBounds)).min + new Vector3((float)x.Key.x * cellSize.x, ((Bounds)(ref _currentBounds)).size.y / 2f, (float)x.Key.y * cellSize.z), cellSize),
			Id = x.Key.x + x.Key.y * subdivisions,
			Colliders = x.Value
		}).ToList();
	}

	private static Vector2i GetPhysxCell(Vector3 position, Vector3 cellSize)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		int num = Mathf.FloorToInt((position.x - ((Bounds)(ref _currentBounds)).min.x) / cellSize.x);
		int num2 = Mathf.FloorToInt((position.z - ((Bounds)(ref _currentBounds)).min.z) / cellSize.z);
		return new Vector2i(num, num2);
	}

	[ServerVar(Help = "(center vec3) (extents vec3)")]
	public static void setbounds(Arg arg)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		Vector3 vector = arg.GetVector3(0);
		Vector3 vector2 = arg.GetVector3(1);
		Bounds val = default(Bounds);
		((Bounds)(ref val)).center = vector;
		((Bounds)(ref val)).extents = vector2;
		Debug.LogWarning((object)"Setting physics bounds disabled temporarily due to issues with Unity 6.3.X - will be re-enabled in a future update when we can verify the fix");
		arg.ReplyWith("Setting physics bounds disabled temporarily due to issues with Unity 6.3.X - will be re-enabled in a future update when we can verify the fix");
	}

	[ServerVar(Help = "(Generated) Prints the combined world-space bounding box of the entity the calling admin is looking at; useful for verifying collider extents")]
	public static void getbounds(Arg arg)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Bounds bounds = GetBounds();
		arg.ReplyWith($"Physics bounds (center={((Bounds)(ref bounds)).center}, extents={((Bounds)(ref bounds)).extents})");
	}

	public static Bounds GetBounds()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return _currentBounds;
	}

	public static void SetBounds(Bounds bounds)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		if (!(_currentBounds == bounds))
		{
			_currentBounds = bounds;
		}
	}

	static Physics()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_0117: Unknown result type (might be due to invalid IL or missing references)
		Bounds val = default(Bounds);
		((Bounds)(ref val)).center = new Vector3(-1500f, 0f, 0f);
		((Bounds)(ref val)).extents = new Vector3(6500f, 4000f, 5000f);
		_currentBounds = val;
		val = default(Bounds);
		((Bounds)(ref val)).center = new Vector3(0f, 0f, 0f);
		((Bounds)(ref val)).extents = new Vector3(5000f, 4000f, 5000f);
		DeepSeaDisabledBounds = val;
		val = default(Bounds);
		((Bounds)(ref val)).center = new Vector3(-1500f, 0f, 0f);
		((Bounds)(ref val)).extents = new Vector3(6500f, 4000f, 5000f);
		DeepSeaEnabledBounds = val;
	}
}
