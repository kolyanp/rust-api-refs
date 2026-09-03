using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using AOT;
using ConVar;
using Facepunch;
using UnityEngine;

namespace Rust.Ai.Gen2.Nav;

public class RustNavigation : FacepunchBehaviour, IServerComponent
{
	public NavMeshBuildParams BuildParams = new NavMeshBuildParams(true);

	public NavMeshBuildParams BuildParamsHiRes = new NavMeshBuildParams(true);

	private const string LOG_PREFIX = "[RustNav] ";

	private BackgroundTileBuilder tileBuilder;

	private RustNavmesh _defaultNavmesh;

	private HashSet<IndependantNavmesh> _navmeshes = new HashSet<IndependantNavmesh>();

	private static readonly RecastWrapper.LogCallback logMessageDelegate = LogMessage;

	private static readonly HashSet<BasePlayer> drawViewers = new HashSet<BasePlayer>();

	private static readonly Dictionary<IndependantNavmesh, int> drawNavIds = new Dictionary<IndependantNavmesh, int>();

	private static int nextDrawNavId = 1;

	public const int DefaultNavmeshDrawId = 0;

	public static RustNavigation Instance { get; private set; }

	public RustNavmesh DefaultNavmesh
	{
		get
		{
			if (!EnsureNewNavmesh())
			{
				return null;
			}
			return _defaultNavmesh;
		}
		private set
		{
			if (EnsureNewNavmesh())
			{
				_defaultNavmesh = value;
				if (_defaultNavmesh != null)
				{
					_defaultNavmesh.EmitTileChangeEvents = true;
					_defaultNavmesh.debugName = "default";
				}
				OnDefaultNavmeshInstanceChanged();
			}
		}
	}

	private HashSet<IndependantNavmesh> Navmeshes
	{
		get
		{
			if (!EnsureNewNavmesh())
			{
				return null;
			}
			return _navmeshes;
		}
	}

	[MonoPInvokeCallback(typeof(RecastWrapper.LogCallback))]
	public static void LogMessage(string message)
	{
		if (AI.logIssues && EnsureNewNavmesh())
		{
			Debug.Log((object)("[RustNav] DLL Log: " + message));
		}
	}

	public static bool EnsureNewNavmesh()
	{
		if (AI.useUnityNavmesh)
		{
			LogError("Trying to use new navmesh despite -useNewNavmesh not being set on server boot.");
		}
		return !AI.useUnityNavmesh;
	}

	public static bool EnsureUnityNavmesh()
	{
		if (!AI.useUnityNavmesh)
		{
			LogError("Trying to use unity navmesh despite -useNewNavmesh being set on server boot.");
		}
		return AI.useUnityNavmesh;
	}

	private void Awake()
	{
		if ((Object)(object)Instance == (Object)null)
		{
			Instance = this;
			if (!AI.useUnityNavmesh)
			{
				RecastWrapper.SetLogCallback(logMessageDelegate);
			}
		}
		else
		{
			LogWarning("Multiple RustNavigation instances detected. Destroying...");
			Object.Destroy((Object)(object)((Component)this).gameObject);
		}
	}

	private void ValidateNavmeshes()
	{
		Log("Navmesh tile validation is enabled. This may impact performance.");
		if (DefaultNavmesh != null && DefaultNavmesh.NavMeshHandle != IntPtr.Zero)
		{
			RecastWrapper.ValidateNavMesh(DefaultNavmesh.NavMeshHandle);
		}
		foreach (IndependantNavmesh navmesh in Navmeshes)
		{
			if ((Object)(object)navmesh != (Object)null && navmesh.Navmesh != null && navmesh.Navmesh.NavMeshHandle != IntPtr.Zero)
			{
				RecastWrapper.ValidateNavMesh(navmesh.Navmesh.NavMeshHandle);
			}
		}
	}

	public IEnumerator BootstrapBuildNavMesh()
	{
		if (!AI.useUnityNavmesh)
		{
			yield return (object)new WaitUntil((Func<bool>)(() => !AiManager.nav_disable && AI.move));
			if (DefaultNavmesh == null)
			{
				RebuildDefaultNavmesh();
			}
		}
	}

	public void RebuildDefaultNavmesh(bool synchronous = false)
	{
		if (!EnsureNewNavmesh())
		{
			return;
		}
		if (tileBuilder == null)
		{
			tileBuilder = new BackgroundTileBuilder();
		}
		BakeStats.Reset();
		BackgroundTileBuilder backgroundTileBuilder = tileBuilder;
		bool synchronous2 = synchronous;
		RustNavmesh rustNavmesh = new RustNavmesh(backgroundTileBuilder, null, null, null, shouldBuild: true, synchronous2, forceHiRes: false, cullTilesFarFromShore: true);
		if (rustNavmesh == null || !rustNavmesh.IsValid())
		{
			LogError("Failed to build default navmesh");
			return;
		}
		if (DefaultNavmesh != null)
		{
			DefaultNavmesh.Dispose();
		}
		DefaultNavmesh = rustNavmesh;
	}

	public void RebuildTileSynchronous(RustNavmesh navmesh, int tx, int ty)
	{
		if (tileBuilder != null && navmesh != null)
		{
			tileBuilder.EnqueueOnMainThread(navmesh, tx, ty, synchronous: true);
		}
	}

	public void AddNavmesh(IndependantNavmesh navmesh)
	{
		if (EnsureNewNavmesh())
		{
			if (tileBuilder == null)
			{
				tileBuilder = new BackgroundTileBuilder();
			}
			Navmeshes.Add(navmesh);
			navmesh.Rebuild(tileBuilder);
		}
	}

	public void RemoveNavmesh(IndependantNavmesh navmesh)
	{
		if (EnsureNewNavmesh())
		{
			Navmeshes.Remove(navmesh);
		}
	}

	public void Tick()
	{
		if (!AI.useUnityNavmesh && tileBuilder != null)
		{
			tileBuilder.TickOnMainThread();
		}
	}

	private void OnDestroy()
	{
		if ((Object)(object)Instance == (Object)(object)this)
		{
			Instance = null;
		}
		if (!AI.useUnityNavmesh)
		{
			if (DefaultNavmesh != null)
			{
				DefaultNavmesh.Dispose();
				DefaultNavmesh = null;
			}
			if (tileBuilder != null)
			{
				tileBuilder.Dispose();
				tileBuilder = null;
			}
		}
	}

	public static void Log(string message)
	{
		DebugEx.Log("[RustNav] " + message, (StackTraceLogType)0);
	}

	public static void LogError(string message)
	{
		Debug.LogError((object)("[RustNav] " + message));
	}

	public static void LogWarning(string message)
	{
		DebugEx.LogWarning("[RustNav] " + message, (StackTraceLogType)0);
	}

	public static string GetNavMeshSavePath()
	{
		return Path.ChangeExtension(Path.Combine(ConVar.Server.rootFolder, World.SaveFileName), ".navmesh");
	}

	public bool Save(string path)
	{
		using (TimeWarning.New("RustNavigation.Save"))
		{
			if (!EnsureNewNavmesh())
			{
				return false;
			}
			if (DefaultNavmesh == null)
			{
				return false;
			}
			Log("Saving navmesh to path: " + path + "...");
			string directoryName = Path.GetDirectoryName(path);
			if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			return DefaultNavmesh.Save(path);
		}
	}

	public bool Load(string path, bool synchronous = false)
	{
		using (TimeWarning.New("RustNavigation.Load"))
		{
			if (!EnsureNewNavmesh())
			{
				return false;
			}
			Log($"Loading navmesh from path: {path} (synchronous: {synchronous})...");
			if (!File.Exists(path))
			{
				LogWarning("Navmesh file not found at path: " + path);
				return false;
			}
			if (tileBuilder == null)
			{
				tileBuilder = new BackgroundTileBuilder();
			}
			RustNavmesh rustNavmesh = RustNavmesh.Load(path, tileBuilder, synchronous, cullTilesFarFromShore: true);
			if (rustNavmesh == null || !rustNavmesh.IsValid())
			{
				return false;
			}
			if (DefaultNavmesh != null)
			{
				DefaultNavmesh.Dispose();
			}
			DefaultNavmesh = rustNavmesh;
			RustNavMeshAgent.RebindAgentsAfterNavmeshSwap();
			if (AI.checkTileValid)
			{
				ValidateNavmeshes();
			}
			return true;
		}
	}

	public void ResetTileBuilder()
	{
		if (!EnsureNewNavmesh())
		{
			return;
		}
		PooledList<(RustNavmesh, int, int)> val = Pool.Get<PooledList<(RustNavmesh, int, int)>>();
		try
		{
			if (tileBuilder != null)
			{
				tileBuilder.GetPendingTilesOnMainThread((List<(RustNavmesh navmesh, int tx, int ty)>)(object)val);
				tileBuilder.Dispose();
			}
			tileBuilder = new BackgroundTileBuilder();
			if (DefaultNavmesh != null)
			{
				DefaultNavmesh.SetTileBuilder(tileBuilder);
			}
			foreach (IndependantNavmesh navmesh in Navmeshes)
			{
				if ((Object)(object)navmesh != (Object)null && navmesh.Navmesh != null)
				{
					navmesh.Navmesh.SetTileBuilder(tileBuilder);
				}
			}
			foreach (var item in (List<(RustNavmesh, int, int)>)(object)val)
			{
				tileBuilder.EnqueueOnMainThread(item.Item1, item.Item2, item.Item3);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public bool IsDefaultNavmeshBuilt()
	{
		using (TimeWarning.New("RustNavigation.IsDefaultNavmeshBuilt"))
		{
			if (!EnsureNewNavmesh())
			{
				return false;
			}
			return DefaultNavmesh != null && DefaultNavmesh.IsBuilt();
		}
	}

	public string ReportNavmeshStats()
	{
		PooledList<RustNavmesh> val = Pool.Get<PooledList<RustNavmesh>>();
		try
		{
			if (_defaultNavmesh != null && _defaultNavmesh.IsValid())
			{
				((List<RustNavmesh>)(object)val).Add(_defaultNavmesh);
			}
			foreach (IndependantNavmesh navmesh in _navmeshes)
			{
				if ((Object)(object)navmesh != (Object)null && navmesh.Navmesh != null && navmesh.Navmesh.IsValid())
				{
					((List<RustNavmesh>)(object)val).Add(navmesh.Navmesh);
				}
			}
			for (int i = 0; i < ((List<RustNavmesh>)(object)val).Count - 1; i++)
			{
				int num = i;
				for (int j = i + 1; j < ((List<RustNavmesh>)(object)val).Count; j++)
				{
					if (((List<RustNavmesh>)(object)val)[j].workerBuildTicks > ((List<RustNavmesh>)(object)val)[num].workerBuildTicks)
					{
						num = j;
					}
				}
				if (num != i)
				{
					int index = i;
					PooledList<RustNavmesh> val2 = val;
					int index2 = num;
					RustNavmesh rustNavmesh = ((List<RustNavmesh>)(object)val)[num];
					RustNavmesh rustNavmesh2 = ((List<RustNavmesh>)(object)val)[i];
					RustNavmesh rustNavmesh3 = (((List<RustNavmesh>)(object)val)[index] = rustNavmesh);
					rustNavmesh3 = (((List<RustNavmesh>)(object)val2)[index2] = rustNavmesh2);
				}
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine($"=== RustNav navmeshes ({((List<RustNavmesh>)(object)val).Count}) by accumulated worker build time ===");
			if (!RustNav.bakeStatsEnabled)
			{
				stringBuilder.AppendLine("(worker times only accumulate while rustnav.bakestatsenabled is true)");
			}
			double num2 = 1000.0 / (double)Stopwatch.Frequency;
			foreach (RustNavmesh item in (List<RustNavmesh>)(object)val)
			{
				double num3 = (double)item.workerBuildTicks * num2;
				string text = ((item.lastFullBuildSeconds >= 0.0) ? $"{item.lastFullBuildSeconds:F2}s" : "building");
				stringBuilder.AppendLine(string.Format("{0,-40} worker {1,10:F1}ms  tiles {2,6}/{3,-6} first full build {4}", new object[5] { item.debugName, num3, item.NumBuiltTiles, item.TotalTiles, text }));
			}
			return stringBuilder.ToString();
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void RebuildTilesInBounds(Bounds rebuildBounds, bool synchronous = false)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("RustNavigation.RebuildTilesInBounds"))
		{
			if (!EnsureNewNavmesh())
			{
				return;
			}
			PooledList<IndependantNavmesh> val = Pool.Get<PooledList<IndependantNavmesh>>();
			try
			{
				IndependantNavmesh.FindNavmeshesInBounds(rebuildBounds, (List<IndependantNavmesh>)(object)val);
				foreach (IndependantNavmesh item in (List<IndependantNavmesh>)(object)val)
				{
					item.RebuildTilesInBounds(rebuildBounds, synchronous);
				}
				if (IsDefaultNavmeshBuilt())
				{
					DefaultNavmesh.RebuildTilesInBounds(rebuildBounds, synchronous);
				}
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	public static int GetDrawNavId(IndependantNavmesh navmesh)
	{
		if ((Object)(object)navmesh == (Object)null)
		{
			return 0;
		}
		if (!drawNavIds.TryGetValue(navmesh, out var value))
		{
			value = nextDrawNavId++;
			drawNavIds[navmesh] = value;
		}
		return value;
	}

	public static void AddDrawViewer(BasePlayer player)
	{
		if (!((Object)(object)player == (Object)null))
		{
			drawViewers.Add(player);
		}
	}

	public static void RemoveDrawViewer(BasePlayer player)
	{
		if (!((Object)(object)player == (Object)null))
		{
			drawViewers.Remove(player);
		}
	}

	public static bool IsDrawViewer(BasePlayer player)
	{
		if ((Object)(object)player != (Object)null)
		{
			return drawViewers.Contains(player);
		}
		return false;
	}

	public static void NotifyDefaultNavmeshTileChanged(int tx, int ty)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		if (drawViewers.Count == 0 || (Object)(object)Instance == (Object)null)
		{
			return;
		}
		RustNavmesh defaultNavmesh = Instance._defaultNavmesh;
		if (defaultNavmesh == null || !defaultNavmesh.IsValid())
		{
			return;
		}
		Bounds tileBounds = defaultNavmesh.rcCalcTileBounds(new Vector2Int(tx, ty));
		foreach (BasePlayer drawViewer in drawViewers)
		{
			if (!((Object)(object)drawViewer == (Object)null) && ViewerCoversTile(drawViewer, tileBounds))
			{
				drawViewer.MarkNavmeshTileDirty(tx, ty);
			}
		}
	}

	private static void OnDefaultNavmeshInstanceChanged()
	{
		foreach (BasePlayer drawViewer in drawViewers)
		{
			if (!((Object)(object)drawViewer == (Object)null))
			{
				drawViewer.ResetNavmeshDrawState();
			}
		}
	}

	private static bool ViewerCoversTile(BasePlayer viewer, Bounds tileBounds)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		float drawRadius = RustNav.drawRadius;
		Bounds val = default(Bounds);
		((Bounds)(ref val))._002Ector(((Component)viewer).transform.position, new Vector3(drawRadius * 2f, ((Bounds)(ref tileBounds)).size.y, drawRadius * 2f));
		return ((Bounds)(ref val)).Intersects(tileBounds);
	}
}
