using System;
using System.Linq;
using ConVar;
using Network;
using Rust;
using UnityEngine;

public class WildlifeHazard : BaseCombatEntity, IReceivePlayerTickListener
{
	public const Flags Flag_IsCorpse = Flags.Reserved8;

	[ServerVar(Help = "(Generated) Interval in seconds between client-side tick updates for wildlife hazard entities")]
	public static float ClientTickRate = 0.1f;

	[ServerVar(Help = "(Generated) Global multiplier applied to wildlife hazard reaction time; higher values make hazards slower to react to player presence")]
	public static float ReactionTimeMultiplier = 1f;

	[ServerVar(Help = "(Generated) Global multiplier applied to the per-tick probability that a wildlife hazard attempts to reposition")]
	public static float ChanceToRepositionMultiplier = 1f;

	[ServerVar(Help = "(Generated) Global multiplier for the radius used when choosing a new reposition destination for a wildlife hazard")]
	public static float RepositionRadiusMultiplier = 1f;

	[ServerVar(Help = "(Generated) Global multiplier applied to the cooldown timer between wildlife hazard reposition attempts")]
	public static float RepositionTimerMultiplier = 1f;

	[ServerVar(Help = "(Generated) Maximum number of position candidates sampled when a wildlife hazard searches for a valid reposition destination")]
	public static int RepositionAttempts = 5;

	[Header("Wildlife Hazard")]
	public BUTTON ReactionSaveButton;

	public float SavingReactionTime = 2f;

	public float Damage = 20f;

	public float HazardInterval = 10f;

	public DamageType DamageType = DamageType.Bite;

	public float ChanceToReposition = 0.5f;

	public float RepositionDelay = 1.25f;

	public float RepositionTimer = 2f;

	public float RepositionRadiusMin = 2f;

	public float RepositionRadiusMax = 4f;

	public Transform ClientArtRoot;

	public TriggerQTE QTETrigger;

	public TriggerBase ClientTrigger;

	public float LookSpeed = 10f;

	public float MinTurnDegrees = 45f;

	public float MinFastTurnDistance = 2f;

	public float MaxWaterDepth = 0.1f;

	public float SlitherDuration = 1f;

	public float SlitherSpeed = 2f;

	public GameObjectRef CorpsePrefab;

	public GameObjectRef BitFX;

	[Header("Wildlife Hazad Visuals")]
	public Animator Animator;

	public GameObjectRef PrefabRepositionEffect;

	public GameObjectRef PrefabReappearEffect;

	[Header("Wildlife Hazard Audio")]
	public SoundDefinition HazardTriggeredSound;

	public bool PlayAlertSounds = true;

	public SoundDefinition AlertIntervalSound;

	public SoundDefinition AttackSound;

	public float AlertSoundMinInterval = 3f;

	public float AlertSoundMaxInterval = 5f;

	public SoundDefinition RepositionDisappearSound;

	public SoundDefinition RepositionReappearSound;

	[Header("Wildlife Hazard Corpse")]
	public ResourceDispenser DeadResourceDispenser;

	public ProtectionProperties DeadProtectionProperties;

	[Tooltip("If enabled, only triggers for one player at a time")]
	public bool SingularInteraction = true;

	public float AttackRange = 1.5f;

	public float AlertToIdleCooldown = 5f;

	protected const int placementMask = 8388608;

	protected const int blockMask = 1075904769;

	protected Vector3 repositionLookAtPos;

	protected Vector3 repositionTo;

	protected int failedRepositionAttempts;

	public override bool IsNpc => true;

	public bool IsCorpse => HasFlag(Flags.Reserved8);

	public BasePlayer SingularInteractionPlayer { get; private set; }

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("WildlifeHazard.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved8, b: false);
		}
		((Behaviour)DeadResourceDispenser).enabled = false;
		failedRepositionAttempts = 0;
	}

	public virtual void TriggeredByPlayer(BasePlayer player)
	{
		if (ShouldStartHazard(player))
		{
			StartHazard(player);
		}
	}

	protected virtual bool ShouldStartHazard(BasePlayer player)
	{
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		if (IsCorpse)
		{
			return false;
		}
		if (!IsAlive())
		{
			return false;
		}
		if (IsInvoking(StartReposition))
		{
			return false;
		}
		if (IsInvoking(ReAttackCheck))
		{
			return false;
		}
		if (IsInvoking(FailHazardDelayed))
		{
			return false;
		}
		if (SingularInteraction && (Object)(object)SingularInteractionPlayer != (Object)null)
		{
			return false;
		}
		if (!CanSeeTarget(((Component)player).transform))
		{
			return false;
		}
		return true;
	}

	private void StartHazard(BasePlayer player)
	{
		OnHazardStarted(player);
	}

	protected virtual void OnHazardStarted(BasePlayer player)
	{
		player.AddReceiveTickListener(this);
		if (SingularInteraction)
		{
			SingularInteractionPlayer = player;
		}
		CancelInvoke(ReAttackCheck);
		CancelInvoke(FailHazardDelayed);
		float reactionTime = GetReactionTime(player);
		Invoke(FailHazardDelayed, reactionTime);
		ClientRPC(RpcTarget.Player("Client_StartHazard", player), reactionTime);
	}

	protected void FailHazardDelayed()
	{
		EndHazard(SingularInteractionPlayer, success: false);
	}

	protected void EndHazard(BasePlayer player, bool success)
	{
		if (success)
		{
			OnHazardCompleted(player);
		}
		else
		{
			OnHazardFailed(player);
		}
		OnHazardEnded(player);
	}

	protected virtual void OnHazardCompleted(BasePlayer player)
	{
	}

	protected virtual void OnHazardFailed(BasePlayer player)
	{
	}

	protected virtual void OnHazardEnded(BasePlayer player)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		ClientRPC(RpcTarget.Player("Client_EndHazard", player));
		CancelInvoke(FailHazardDelayed);
		CancelInvoke(ReAttackCheck);
		if ((Object)(object)player != (Object)null)
		{
			player.RemoveReceiveTickListener(this);
		}
		SingularInteractionPlayer = null;
		if (ShouldReposition())
		{
			if (FindSuitableReposition(out var pos))
			{
				failedRepositionAttempts = 0;
				repositionTo = pos;
				repositionLookAtPos = (((Object)(object)player != (Object)null) ? ((Component)player).transform.position : (((Component)this).transform.position + Vector3.forward));
				Invoke(StartReposition, RepositionDelay);
			}
			else
			{
				failedRepositionAttempts++;
				if (failedRepositionAttempts <= 3)
				{
					Invoke(ReAttackCheck, HazardInterval);
				}
				else
				{
					Kill();
				}
			}
		}
		else
		{
			Invoke(ReAttackCheck, HazardInterval);
		}
	}

	private bool ShouldReposition()
	{
		if (IsCorpse)
		{
			return false;
		}
		float num = ChanceToReposition * ChanceToRepositionMultiplier;
		if (num <= 0f)
		{
			return false;
		}
		if (Random.Range(0f, 1f) > num)
		{
			return false;
		}
		return true;
	}

	public virtual void StartReposition()
	{
	}

	private bool FindSuitableReposition(out Vector3 pos)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		bool flag = true;
		int num = 0;
		Vector3 val = default(Vector3);
		while (flag)
		{
			float num2 = Random.Range(RepositionRadiusMin, RepositionRadiusMax) * RepositionRadiusMultiplier;
			float num3 = Random.value * (MathF.PI * 2f);
			((Vector3)(ref val))._002Ector(Mathf.Cos(num3), 0f, Mathf.Sin(num3));
			pos = ((Component)this).transform.position + val * num2;
			bool flag2 = ValidatePosition(ref pos);
			if (flag2)
			{
				return true;
			}
			flag = !flag2 && ++num < RepositionAttempts;
		}
		pos = ((Component)this).transform.position;
		return false;
	}

	private bool ValidatePosition(ref Vector3 pos)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default(RaycastHit);
		if (Physics.Raycast(pos + Vector3.up * 3f, Vector3.down, ref val, 6f, 8388608))
		{
			if (WaterLevel.GetOverallWaterDepth(((RaycastHit)(ref val)).point, waves: true, volumes: false) > MaxWaterDepth)
			{
				return false;
			}
			if (!GamePhysics.LineOfSight(((RaycastHit)(ref val)).point, ((RaycastHit)(ref val)).point + Vector3.up * 4f, 1075904769))
			{
				return false;
			}
			if (!GamePhysics.LineOfSight(((Component)this).transform.position + Vector3.up * 0.25f, ((RaycastHit)(ref val)).point + Vector3.up * 0.25f, 1075904769))
			{
				return false;
			}
			pos = ((RaycastHit)(ref val)).point;
			return true;
		}
		return false;
	}

	private void ReAttackCheck()
	{
		if (IsCorpse || QTETrigger.contents == null || QTETrigger.contents.Count == 0)
		{
			CancelInvoke(ReAttackCheck);
			return;
		}
		GameObject val = QTETrigger.contents.Single();
		if (!((Object)(object)val == (Object)null))
		{
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(val);
			if (!((Object)(object)baseEntity == (Object)null))
			{
				TriggeredByPlayer(baseEntity as BasePlayer);
			}
		}
	}

	public override void OnAttacked(HitInfo info)
	{
		if (base.isServer)
		{
			if (IsCorpse)
			{
				OnCorpseAttacked(info);
			}
			else
			{
				OnAliveAttacked(info);
			}
		}
	}

	private void OnCorpseAttacked(HitInfo info)
	{
		ResetCorpseRemovalTime();
		if (!(info.Weapon is BaseMelee baseMelee) || baseMelee.GetGatherInfoFromIndex(ResourceDispenser.GatherType.Flesh).gatherDamage != 0f)
		{
			DeadResourceDispenser.DoGather(info);
			if (!info.DidGather)
			{
				base.OnAttacked(info);
			}
		}
	}

	private void OnAliveAttacked(HitInfo info)
	{
		base.OnAttacked(info);
		if (base.isServer && Object.op_Implicit((Object)(object)info.InitiatorPlayer) && !info.damageTypes.IsMeleeType())
		{
			info.InitiatorPlayer.LifeStoryShotHit(info.Weapon);
		}
	}

	public override void OnKilled()
	{
		base.OnKilled();
		if ((Object)(object)SingularInteractionPlayer != (Object)null)
		{
			SingularInteractionPlayer.RemoveReceiveTickListener(this);
		}
		CancelHazardInvokes();
	}

	public override void OnDied(HitInfo info)
	{
		if (!base.isServer)
		{
			return;
		}
		ClientRPC(RpcTarget.NetworkGroup("CL_Died"));
		CancelInvoke(ReAttackCheck);
		CancelInvoke(FailHazardDelayed);
		if ((Object)(object)SingularInteractionPlayer != (Object)null)
		{
			SingularInteractionPlayer.RemoveReceiveTickListener(this);
		}
		if (IsCorpse)
		{
			Kill();
			return;
		}
		if (info != null)
		{
			BasePlayer initiatorPlayer = info.InitiatorPlayer;
			if ((Object)(object)initiatorPlayer != (Object)null)
			{
				initiatorPlayer.LifeStoryKill(this);
			}
		}
		TurnIntoCorpse();
	}

	public void TurnIntoCorpse()
	{
		using (FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate))
		{
			flagsUpdateScope.Set(Flags.Reserved8, b: true);
		}
		SetHealth(MaxHealth());
		lifestate = LifeState.Alive;
		((Behaviour)DeadResourceDispenser).enabled = true;
		baseProtection = DeadProtectionProperties;
		sendsHitNotification = false;
		ResetCorpseRemovalTime();
	}

	private void CancelHazardInvokes()
	{
		CancelInvoke(FailHazardDelayed);
		CancelInvoke(StartReposition);
		CancelInvoke(ReAttackCheck);
	}

	public void ResetCorpseRemovalTime()
	{
		ResetCorpseRemovalTime(ConVar.Server.corpsedespawn);
	}

	public void ResetCorpseRemovalTime(float dur)
	{
		using (TimeWarning.New("ResetRemovalTime"))
		{
			if (IsInvoking(RemoveCorpse))
			{
				CancelInvoke(RemoveCorpse);
			}
			Invoke(RemoveCorpse, dur);
		}
	}

	public void RemoveCorpse()
	{
		Kill();
	}

	bool IReceivePlayerTickListener.ShouldRemoveOnPlayerDeath()
	{
		return true;
	}

	void IReceivePlayerTickListener.OnReceivePlayerTick(BasePlayer player, PlayerTick msg)
	{
		if (!((Object)(object)player == (Object)null) && !((Object)(object)player != (Object)(object)SingularInteractionPlayer) && player.serverInput.WasJustPressed(ReactionSaveButton))
		{
			EndHazard(player, success: true);
		}
	}

	public virtual float GetReactionTime(BasePlayer player)
	{
		float num = (((Object)(object)player == (Object)null || player.net == null || player.net.connection == null) ? 0f : ((float)Net.sv.GetAveragePing(player.net.connection) / 1000f));
		return SavingReactionTime * ReactionTimeMultiplier + num;
	}

	public override void Hurt(HitInfo info)
	{
		base.Hurt(info);
		if (base.isServer)
		{
			ClientRPC(RpcTarget.NetworkGroup("CL_Hurt"));
		}
	}

	protected bool CanSeeTarget(Transform targetTransform)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)targetTransform == (Object)null)
		{
			return false;
		}
		if (!GamePhysics.LineOfSight(((Component)this).transform.position + Vector3.up * 0.25f, targetTransform.position + Vector3.up * 0.25f, 1075904769))
		{
			return false;
		}
		return true;
	}
}
