using Rust.Ai.Gen2.Nav;
using UnityEngine;

namespace ConVar;

[Factory("rustnav")]
public class RustNav : ConsoleSystem
{
	[ServerVar]
	public static float drawRadius = 100f;

	[ServerVar]
	public static float drawRefreshRate = 1f;

	[ServerVar]
	public static int drawTileBudget = 5;

	[ServerVar]
	public static int drawManifestInterval = 10;

	private const float traceLength = 500f;

	public static int numThreads = 4;

	[ServerVar]
	public static bool enableVerboseLogs = false;

	[ServerVar]
	public static void Draw(Arg arg)
	{
		if (AI.useUnityNavmesh)
		{
			arg.ReplyWith("Restart the server with command line argument -useNewNavmesh");
			return;
		}
		if ((Object)(object)RustNavigation.Instance == (Object)null || RustNavigation.Instance.DefaultNavmesh == null || !RustNavigation.Instance.DefaultNavmesh.IsValid())
		{
			arg.ReplyWith("Navmesh is not initialized");
			return;
		}
		BasePlayer basePlayer = ArgEx.Player(arg);
		if (arg.GetBool(0, !basePlayer.IsInvoking(basePlayer.DrawNavmesh)))
		{
			basePlayer.ResetNavmeshDrawState();
			RustNavigation.AddDrawViewer(basePlayer);
			basePlayer.InvokeRepeating(basePlayer.DrawNavmesh, 0f, drawRefreshRate);
		}
		else
		{
			basePlayer.CancelInvoke(basePlayer.DrawNavmesh);
			RustNavigation.RemoveDrawViewer(basePlayer);
			basePlayer.ResetNavmeshDrawState();
			basePlayer.ClientRPC(RpcTarget.Player("StopDrawingNavmesh", basePlayer));
		}
	}

	[ServerVar]
	public static void SetNumThreads(Arg arg)
	{
		if (!RustNavigation.EnsureNewNavmesh())
		{
			arg.ReplyWith("Restart the server with command line argument -useNewNavmesh");
		}
		else if (!((Object)(object)RustNavigation.Instance == (Object)null))
		{
			if (arg.GetInt(0, numThreads) == numThreads)
			{
				arg.ReplyWith($"RustNav.numThreads is already set to {numThreads}");
				return;
			}
			RustNavigation.Instance.ResetTileBuilder();
			arg.ReplyWith($"Set RustNav.numThreads to {numThreads}");
		}
	}

	[ServerVar]
	public static void Rebuild(Arg arg)
	{
		if (!RustNavigation.EnsureNewNavmesh())
		{
			arg.ReplyWith("Restart the server with command line argument -useNewNavmesh");
			return;
		}
		bool synchronous = arg.GetBool(0);
		RustNavigation.Instance.RebuildDefaultNavmesh(synchronous);
	}

	private static RustNavmesh FindNavmeshAtPosition(Vector3 position)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (!RustNavigation.EnsureNewNavmesh())
		{
			return null;
		}
		IndependantNavmesh independantNavmesh = IndependantNavmesh.FindNavmeshAtPosition(position);
		if ((Object)(object)independantNavmesh == (Object)null || independantNavmesh.Navmesh == null || !independantNavmesh.Navmesh.IsValid())
		{
			return RustNavigation.Instance.DefaultNavmesh;
		}
		return independantNavmesh.Navmesh;
	}

	[ServerVar]
	public static void DebugPath(Arg arg)
	{
	}

	[ServerVar]
	public static void DebugRaycast(Arg arg)
	{
	}

	[ServerVar]
	public static void DebugSample(Arg arg)
	{
	}

	[ServerVar]
	public static void DebugRebuildTile(Arg arg)
	{
	}

	[ServerVar]
	public static void DebugSave(Arg arg)
	{
	}

	[ServerVar]
	public static void DebugLoad(Arg arg)
	{
	}
}
