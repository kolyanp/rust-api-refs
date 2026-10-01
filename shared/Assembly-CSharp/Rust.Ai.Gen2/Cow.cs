using System;
using ConVar;
using Facepunch.Rust;
using Network;
using UnityEngine;
using UnityEngine.Assertions;

namespace Rust.Ai.Gen2;

[SoftRequireComponent(typeof(CowFSM))]
public class Cow : LivestockAnimal
{
	[ServerVar(Help = "Population active on the server, per square km", ShowInAdminUI = true)]
	public static float Population = 2f;

	public ItemDefinition MilkDef;

	public int MilkAmount = 10;

	[Tooltip("How long (in seconds) after being milked before the cow can be milked again")]
	public float MilkCooldown = 300f;

	[Tooltip("Optional effect played on the cow each time she is milked")]
	public GameObjectRef MilkEffect;

	public const Flags MilkReady = Flags.Reserved9;

	public const int MilkTimerId = 5;

	private PersistentTimer milkTimer;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("Cow.OnRpcMessage"))
		{
			if (rpc == 3387078280u && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - MilkCow"));
				}
				using (TimeWarning.New("MilkCow"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(3387078280u, "MilkCow", this, player, 5uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(3387078280u, "MilkCow", this, player, 3f))
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
							MilkCow(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in MilkCow");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override string Categorize()
	{
		return "Cow";
	}

	public bool CanBeMilked(BasePlayer player)
	{
		if ((Object)(object)player == (Object)null)
		{
			return false;
		}
		if (IsDead())
		{
			return false;
		}
		if (IsInfant())
		{
			return false;
		}
		if (IsMale)
		{
			return false;
		}
		if (!HasFlag(Flags.Reserved9))
		{
			return false;
		}
		return true;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		milkTimer = new PersistentTimer(timers, 5)
		{
			onElapsed = EnableMilking,
			onStarted = () =>
			{
				SetMilkReady(ready: false);
			}
		};
		SetMilkReady(IsFemale);
	}

	[RPC_Server]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server.CallsPerSecond(5uL)]
	public void MilkCow(RPCMessage msg)
	{
		TryMilk(msg.player);
	}

	public bool TryMilk(BasePlayer player)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		if (!CanBeMilked(player))
		{
			return false;
		}
		if (!IsTame)
		{
			OnGrabbedBy(player);
			return false;
		}
		if ((Object)(object)MilkDef != (Object)null)
		{
			int num = MilkedAmount();
			player.GiveItem(ItemManager.Create(MilkDef, num, 0uL, isServerSide: true, 0uL));
			if (MilkEffect != null && MilkEffect.isValid)
			{
				Effect.server.Run(MilkEffect.resourcePath, this, 0u, Vector3.zero, Vector3.zero);
			}
			Analytics.Azure.OnLivestockCowMilked(player, this, num);
		}
		milkTimer.Start(MilkCooldown / GeneScale(LivestockGene.Yield));
		return true;
	}

	public int MilkedAmount()
	{
		return Mathf.Max(1, Mathf.RoundToInt((float)MilkAmount * GeneScale(LivestockGene.Yield)));
	}

	public override bool TryGetYield(out ItemDefinition item, out int amount, out float interval, out float bestOfBreed)
	{
		item = MilkDef;
		amount = MilkedAmount();
		interval = MilkCooldown / GeneScale(LivestockGene.Yield);
		float num = Mathf.Max(0.01f, Livestock.geneYieldGood);
		int num2 = Mathf.Max(1, Mathf.RoundToInt((float)MilkAmount * num));
		float num3 = MilkCooldown / num;
		bestOfBreed = ((interval > 0f && num3 > 0f) ? Mathf.Clamp01((float)amount / interval / ((float)num2 / num3)) : 0f);
		if ((Object)(object)item != (Object)null)
		{
			return interval > 0f;
		}
		return false;
	}

	private void SetMilkReady(bool ready)
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved9, ready);
	}

	private void EnableMilking()
	{
		SetMilkReady(ready: true);
	}

	public bool ForceMilkReady()
	{
		if (IsDead() || IsInfant() || IsMale)
		{
			return false;
		}
		milkTimer.Stop();
		SetMilkReady(ready: true);
		return true;
	}
}
