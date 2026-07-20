using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using AOT;
using ConVar;
using Facepunch;
using UnityEngine;

namespace Rust.Ai.Gen2.Nav;

public class RustNavigation : FacepunchBehaviour, IServerComponent
{
	private const string LOG_PREFIX = "[RustNav] ";

	private BackgroundTileBuilder tileBuilder;

	private RustNavmesh _defaultNavmesh;

	private HashSet<IndependantNavmesh> _navmeshes = new HashSet<IndependantNavmesh>();

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
		set
		{
			if (EnsureNewNavmesh())
			{
				_navmeshes = value;
			}
		}
	}

	[MonoPInvokeCallback(typeof(RecastWrapper.LogCallback))]
	public static void LogMessage(string message)
	{
		if (EnsureNewNavmesh())
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
				RecastWrapper.SetLogCallback(LogMessage);
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
		BackgroundTileBuilder backgroundTileBuilder = tileBuilder;
		bool synchronous2 = synchronous;
		RustNavmesh rustNavmesh = new RustNavmesh(backgroundTileBuilder, null, null, null, shouldBuild: true, synchronous2);
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
			using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create)))
			{
				if (!DefaultNavmesh.Save(writer))
				{
					return false;
				}
			}
			return true;
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
			using (BinaryReader reader = new BinaryReader(File.Open(path, FileMode.Open)))
			{
				if (tileBuilder == null)
				{
					tileBuilder = new BackgroundTileBuilder();
				}
				RustNavmesh rustNavmesh = RustNavmesh.Load(reader, tileBuilder, synchronous);
				if (rustNavmesh == null || !rustNavmesh.IsValid())
				{
					return false;
				}
				if (DefaultNavmesh != null)
				{
					DefaultNavmesh.Dispose();
				}
				DefaultNavmesh = rustNavmesh;
			}
			ValidateNavmeshes();
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
			return DefaultNavmesh != null && DefaultNavmesh.IsValid();
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
}
