using System;
using System.Collections.Generic;
using Development.Attributes;
using Rust.Ai.Gen2;
using UnityEngine;

[ResetStaticFields]
public class BirdFlock : BaseNPC2
{
	public const Flags Fleeing = Flags.Reserved8;

	public const Flags Interested = Flags.Reserved9;

	[Tooltip("Which interest points draw this flock in.")]
	public BirdSpecies species;

	private static readonly List<BirdFlock> serverFlocks = new List<BirdFlock>();

	private static readonly List<Transform> serverFlockTransforms = new List<Transform>();

	[ServerVar(Help = "Crow flocks per square km. Default is four per 3km map.", ShowInAdminUI = true)]
	public static float crowpopulation = 4f / 9f;

	[ServerVar(Help = "Seagull flocks per square km. Default is four per 3km map.", ShowInAdminUI = true)]
	public static float seagullpopulation = 4f / 9f;

	[ServerVar(Help = "How far away an explosion sends bird flocks fleeing, in metres.")]
	public static float explosionstartleradius = 300f;

	[ServerVar(Help = "How much crow flocks want to circle corpses over other interest points. Below 0 loses to anything else.")]
	public static float corpseinterestpriority = -1f;

	[NonSerialized]
	public Nullable startlePosition;

	[NonSerialized]
	public int interestPointId;

	[NonSerialized]
	public TimeUntil timeUntilInterested;

	public override bool MovesOnNavmesh => false;

	public bool IsFleeing()
	{
		return HasFlag(Flags.Reserved8);
	}

	public bool IsInterested()
	{
		return HasFlag(Flags.Reserved9);
	}

	public override void DestroyShared()
	{
		base.DestroyShared();
		if (isServer)
		{
			int num = serverFlocks.IndexOf(this);
			if (num >= 0)
			{
				serverFlocks.RemoveAt(num);
				serverFlockTransforms.RemoveAt(num);
			}
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		serverFlocks.Add(this);
		serverFlockTransforms.Add(((Component)this).transform);
	}

	public void ReleaseInterestPoint()
	{
		BirdInterestPointManager.Release(this, interestPointId);
		interestPointId = 0;
	}

	public static int StartleFlocks(Vector3 position, float radius)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		for (int i = 0; i < serverFlocks.Count; i++)
		{
			Vector3 val = serverFlockTransforms[i].position - position;
			if (!(val.sqrMagnitude > radius * radius))
			{
				serverFlocks[i].startlePosition = position;
				num++;
			}
		}
		return num;
	}

	public void SetNetworkedFlag(Flags flag, bool value)
	{
		if (HasFlag(flag) == value)
		{
			return;
		}
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate_Flags);
		flagsUpdateScope.Set(flag, value);
	}

	[ServerVar(Help = "Startles every bird flock within the given radius (default 300) of where you are looking, sending them fleeing away from it.")]
	public static void startle(ConsoleSystem.Arg args)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		if (TryGetLookPosition(args, out var position))
		{
			float num = args.GetFloat(0, 300f);
			int num2 = StartleFlocks(position, num);
			args.ReplyWith($"Startled {num2} bird flock(s) within {num}m of {position}.");
		}
	}

	[ServerVar(Help = "Adds a bird interest point where you are looking. Args: crow|seagull, duration in seconds (default 300), priority (default 0).")]
	public static void interest(ConsoleSystem.Arg args)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position;
		if (!Enum.TryParse<BirdSpecies>(args.GetString(0), ignoreCase: true, out var result))
		{
			args.ReplyWith("Usage: birdflock.interest crow|seagull [duration] [priority]");
		}
		else if (TryGetLookPosition(args, out position))
		{
			int num = BirdInterestPointManager.Add(result, position, args.GetFloat(2), args.GetFloat(1, 300f));
			args.ReplyWith($"Added {result} interest point {num} at {position}.");
		}
	}

	public static bool TryGetLookPosition(ConsoleSystem.Arg args, out Vector3 position)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((Object)(object)basePlayer == (Object)null)
		{
			args.ReplyWith("This command has to be run by a player.");
			position = default;
			return false;
		}
		RaycastHit val = default;
		position = (Physics.Raycast(basePlayer.eyes.HeadRay(), ref val, 1000f, 1218652417) ? val.point : ((Component)basePlayer).transform.position);
		return true;
	}
}
