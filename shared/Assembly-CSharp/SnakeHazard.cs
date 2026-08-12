using System.Collections.Generic;
using Network;
using UnityEngine;

public class SnakeHazard : WildlifeHazard
{
	public static Phrase SnakeHazardFailedTipPhrase;

	[ServerVar(Help = "Population active on the server, per square km", ShowInAdminUI = true)]
	public static float Population;

	public List<ModifierDefintion> FailModifierEffects;

	private BasePlayer playerToAttack;

	private float slitherRate = 0.05f;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("SnakeHazard.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	protected override void OnHazardFailed(BasePlayer player)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		base.OnHazardFailed(player);
		if (!((Object)(object)player == (Object)null))
		{
			ClientRPC(RpcTarget.Player("CL_SnakeHazardFailed", player));
			if (GamePhysics.LineOfSight(((Component)this).transform.position + Vector3.up * 0.25f, ((Component)player).transform.position + Vector3.up * 0.25f, 1075904769))
			{
				playerToAttack = player;
				Invoke(ApplyAttackToPlayer, 0.3f);
				ClientRPC(RpcTarget.NetworkGroup("CL_Attack"));
			}
		}
	}

	private void ApplyAttackToPlayer()
	{
		if ((Object)(object)playerToAttack == (Object)null)
		{
			return;
		}
		if (!playerToAttack.OnAttacked(Damage, DamageType, this, ignoreShield: false))
		{
			playerToAttack = null;
			return;
		}
		if (FailModifierEffects != null && (Object)(object)playerToAttack.modifiers != (Object)null)
		{
			playerToAttack.modifiers.Add(FailModifierEffects);
		}
		playerToAttack = null;
	}

	public override void StartReposition()
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		base.StartReposition();
		if (!base.IsCorpse)
		{
			if (base.isServer)
			{
				ClientRPC(RpcTarget.NetworkGroup("CL_RepositionDisappear"), repositionTo);
			}
			InvokeRepeating(SlitherTick, 0.2f, slitherRate);
			Invoke(StartDelayedTeleport, SlitherDuration + 0.2f);
		}
	}

	private void SlitherTick()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = Vector3.MoveTowards(((Component)this).transform.position, repositionTo, SlitherSpeed * slitherRate);
		RaycastHit val2 = default(RaycastHit);
		if (Physics.Raycast(val + Vector3.up * 1f, Vector3.down, ref val2, 5f, 8388608))
		{
			val = ((RaycastHit)(ref val2)).point;
		}
		((Component)this).transform.position = val;
		try
		{
			syncPosition = true;
			NetworkPositionTick();
		}
		finally
		{
			syncPosition = false;
		}
	}

	private void StartDelayedTeleport()
	{
		if (!base.IsCorpse)
		{
			CancelInvoke(SlitherTick);
			using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
			{
				flagsUpdateScope.Set(Flags.Disabled, b: true);
			}
			Invoke(EndDelayedTeleport, 2f);
		}
	}

	private void EndDelayedTeleport()
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Disabled, b: false);
		}
		ServerPosition = repositionTo;
		SendNetworkUpdate_Position();
		if (base.isServer)
		{
			if (PrefabRepositionEffect != null && PrefabRepositionEffect.isValid)
			{
				Effect.server.Run(PrefabReappearEffect.resourcePath, ServerPosition, Vector3.up);
			}
			ClientRPC(RpcTarget.NetworkGroup("CL_RepositionReappear"), repositionLookAtPos);
		}
	}

	protected override bool ShouldStartHazard(BasePlayer player)
	{
		if (!base.ShouldStartHazard(player))
		{
			return false;
		}
		if (IsInvoking(SlitherTick))
		{
			return false;
		}
		if (IsInvoking(StartDelayedTeleport))
		{
			return false;
		}
		if (IsInvoking(EndDelayedTeleport))
		{
			return false;
		}
		return true;
	}

	public override void OnDied(HitInfo info)
	{
		base.OnDied(info);
		CancelSnakeInvokes();
	}

	public override void OnKilled()
	{
		base.OnKilled();
		CancelSnakeInvokes();
	}

	private void CancelSnakeInvokes()
	{
		CancelInvoke(SlitherTick);
		CancelInvoke(StartDelayedTeleport);
		CancelInvoke(EndDelayedTeleport);
		CancelInvoke(ApplyAttackToPlayer);
	}

	static SnakeHazard()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		SnakeHazardFailedTipPhrase = new Phrase("toast.snake_hazard_failed", "Jump immediately when a Snake hisses to avoid its attack.");
		Population = 5f;
	}
}
