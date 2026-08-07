using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using Network;
using Oxide.Core;
using UnityEngine;
using UnityEngine.Assertions;

public class BaseMetalDetector : HeldEntity
{
	public enum DetectState
	{
		LongRange,
		SweetSpot
	}

	public DetectState State;

	public float LongRangeDetectionRange = 20f;

	public float SweetSpotDetectionRange = 0.2f;

	public SoundDefinition BeepSoundEffect;

	[ServerVar(Help = "(Generated) Interval in seconds between nearest-detectable-object distance checks performed by the metal detector")]
	public static float NearestDistanceTick = 0.25f;

	[ServerVar(Help = "(Generated) Interval in seconds between long-range detectability checks for buried objects; longer interval saves CPU for distant searches")]
	public static float DetectLongRangeTick = 1f;

	[ServerVar(Help = "(Generated) Minimum distance in metres the player must move before a new long-range detection check is triggered")]
	public static float DetectMinMovementDistance = 1f;

	private List<IMetalDetectable> inRangeValidSources = new List<IMetalDetectable>();

	public IMetalDetectable nearestSource;

	private float nearestSourceDistanceSqr;

	private Vector3 lastDetectPlayerPos;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("BaseMetalDetector.OnRpcMessage"))
		{
			if (rpc == 2192859691u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_RequestFlag"));
				}
				using (TimeWarning.New("RPC_RequestFlag"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(2192859691u, "RPC_RequestFlag", this, player, 2uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(2192859691u, "RPC_RequestFlag", this, player))
						{
							return true;
						}
						long position = msg.read.Position;
						if (!RPC_Server.InputValidation.Test(msg.read.Read<Vector3>()))
						{
							return true;
						}
						msg.read.Position = position;
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
							RPC_RequestFlag(rpc2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_RequestFlag");
					}
				}
				return true;
			}
			if (rpc == 50929187 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - SV_SetSweetspotScanning"));
				}
				using (TimeWarning.New("SV_SetSweetspotScanning"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(50929187u, "SV_SetSweetspotScanning", this, player, 6uL))
						{
							return true;
						}
						if (!RPC_Server.FromOwner.Test(50929187u, "SV_SetSweetspotScanning", this, player))
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
							SV_SetSweetspotScanning(msg2);
						}
					}
					catch (Exception ex2)
					{
						Debug.LogException(ex2);
						player.Kick("RPC Error in SV_SetSweetspotScanning");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void OnHeldChanged()
	{
		base.OnHeldChanged();
		if (IsDeployed())
		{
			StartDetecting();
		}
		else
		{
			StopDetecting();
		}
	}

	private void StartDetecting()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		lastDetectPlayerPos = Vector3.zero;
		if (!IsInvoking(DetectLongRange))
		{
			InvokeRepeating(DetectLongRange, 0f, DetectLongRangeTick);
			InvokeRepeating(SendNearestDistance, 0f, NearestDistanceTick);
		}
	}

	private void StopDetecting()
	{
		CancelInvoke(DetectLongRange);
		CancelInvoke(SendNearestDistance);
		ClearSources();
	}

	private void SendNearestDistance()
	{
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if (!((Object)(object)ownerPlayer == (Object)null))
		{
			float distanceToCenterOrNearestSubSource = GetDistanceToCenterOrNearestSubSource(nearestSource);
			ClientRPC(RpcTarget.Player("CL_UpdateNearest", ownerPlayer), distanceToCenterOrNearestSubSource, (nearestSource != null) ? nearestSource.GetRadius() : 1f);
		}
	}

	private float GetDistanceToCenterOrNearestSubSource(IMetalDetectable source)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (source == null)
		{
			return float.PositiveInfinity;
		}
		Vector3 detectionPoint = GetDetectionPoint();
		return Vector3.Distance(source.GetNearestPosition(detectionPoint), detectionPoint);
	}

	private void ProcessDetectedSources()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)GetOwnerPlayer() == (Object)null)
		{
			nearestSource = null;
		}
		nearestSourceDistanceSqr = float.PositiveInfinity;
		IMetalDetectable metalDetectable = null;
		Vector3 detectionPoint = GetDetectionPoint();
		foreach (IMetalDetectable inRangeValidSource in inRangeValidSources)
		{
			if (inRangeValidSource == null)
			{
				continue;
			}
			foreach (Vector3 scanLocation in inRangeValidSource.GetScanLocations())
			{
				float num = Vector3.SqrMagnitude(scanLocation - detectionPoint);
				if (num < nearestSourceDistanceSqr)
				{
					nearestSourceDistanceSqr = num;
					metalDetectable = inRangeValidSource;
				}
			}
		}
		nearestSource = metalDetectable;
	}

	private void DetectLongRange()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if ((Object)(object)ownerPlayer == (Object)null || (Object)(object)ownerPlayer.GetHeldEntity() != (Object)(object)this)
		{
			StopDetecting();
			return;
		}
		if (Vector3.SqrMagnitude(((Component)ownerPlayer).transform.position - lastDetectPlayerPos) < DetectMinMovementDistance * DetectMinMovementDistance)
		{
			ProcessDetectedSources();
			return;
		}
		DetectSources(ownerPlayer);
		ProcessDetectedSources();
	}

	private void DetectSources(BasePlayer player)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		lastDetectPlayerPos = ((Component)player).transform.position;
		List<IMetalDetectable> list = Pool.Get<List<IMetalDetectable>>();
		if (!player.InSafeZone())
		{
			Vis.Entities(((Component)this).transform.position, LongRangeDetectionRange + 5f, list, 512, (QueryTriggerInteraction)1);
		}
		inRangeValidSources.Clear();
		for (int i = 0; i < list.Count; i++)
		{
			IMetalDetectable metalDetectable = list[i];
			if (metalDetectable.IsValidSource(player))
			{
				inRangeValidSources.Add(metalDetectable);
			}
		}
		Pool.FreeUnmanaged<IMetalDetectable>(ref list);
	}

	private void ClearSources()
	{
		nearestSource = null;
		inRangeValidSources.Clear();
	}

	[RPC_Server]
	[RPC_Server.FromOwner]
	[RPC_Server.CallsPerSecond(2uL)]
	[RPC_Server.InputValidation(new Type[] { typeof(Vector3) })]
	private void RPC_RequestFlag(RPCMessage rpc)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer player = rpc.player;
		if (!((Object)(object)player == (Object)null) && !player.InSafeZone() && nearestSource != null)
		{
			Vector3 val = rpc.read.Vector3();
			Interface.CallHook("OnMetalDetectorFlagRequest", this, val, player);
			if (nearestSource.VerifyScanPosition(((Component)player).transform.position, val, out var spotPos))
			{
				nearestSource.Detected(spotPos);
			}
		}
	}

	[RPC_Server.FromOwner]
	[RPC_Server.CallsPerSecond(6uL)]
	[RPC_Server]
	public void SV_SetSweetspotScanning(RPCMessage msg)
	{
		if ((Object)(object)msg.player == (Object)null || (Object)(object)msg.player != (Object)(object)GetOwnerPlayer())
		{
			return;
		}
		bool b = msg.read.Bit();
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.On, b);
	}

	public Vector3 GetDetectionPoint()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer ownerPlayer = GetOwnerPlayer();
		if ((Object)(object)ownerPlayer == (Object)null)
		{
			return ((Component)this).transform.position;
		}
		Vector3 val = ((Component)ownerPlayer).transform.position + ownerPlayer.eyes.MovementForward() * 0.3f;
		RaycastHit val2 = default(RaycastHit);
		if (Physics.Raycast(val + Vector3.up * 0.5f, Vector3.down, ref val2, 1.5f, 8388608))
		{
			return ((RaycastHit)(ref val2)).point;
		}
		return val;
	}

	public float GetSweetSpotDistancePercent(float distance, float sourceSpawnRadius)
	{
		if (State != DetectState.SweetSpot)
		{
			return 0f;
		}
		if (distance > sourceSpawnRadius)
		{
			return 0f;
		}
		return Mathf.Clamp01(1f - distance / sourceSpawnRadius);
	}
}
