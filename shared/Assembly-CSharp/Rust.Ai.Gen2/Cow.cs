using System;
using ConVar;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using ProtoBuf;
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

	[Tooltip("How many milkings an owned cow stores while nobody milks her. Her clock stops once she holds this many")]
	public int MaxMilkCharges = 3;

	public GameObjectRef MilkEffect;

	public const Flags MilkReady = Flags.Reserved9;

	public const int MilkTimerId = 5;

	private static readonly Phrase WildCowPhrase = new Phrase("livestock.wild.cow", "Wild cow");

	private static readonly Phrase WildBullPhrase = new Phrase("livestock.wild.bull", "Wild bull");

	private static readonly Phrase WildCalfPhrase = new Phrase("livestock.wild.calf", "Wild calf");

	private PersistentTimer milkTimer;

	private static readonly Phrase MilkBullPhrase = new Phrase("livestock.milk.bull", "That is a bull");

	public bool IsMilkClockRunning => milkTimer.IsRunning;

	public float MilkInterval => MilkCooldown / GeneScale(LivestockGene.Yield);

	private bool GivesMilk
	{
		get
		{
			if (IsFemale && !IsInfant())
			{
				return !IsDead();
			}
			return false;
		}
	}

	public override bool ProducesYield
	{
		get
		{
			if (base.ProducesYield)
			{
				return IsFemale;
			}
			return false;
		}
	}

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

	protected override Phrase WildLabel()
	{
		if (IsInfant())
		{
			return WildCalfPhrase;
		}
		if (!IsMale)
		{
			return WildCowPhrase;
		}
		return WildBullPhrase;
	}

	public bool OffersMilking()
	{
		if (!IsDead() && IsAdult())
		{
			if (!IsMale)
			{
				return HasFlag(Flags.Reserved9);
			}
			return true;
		}
		return false;
	}

	public bool CanBeMilked(BasePlayer player)
	{
		if ((Object)(object)player != (Object)null && !IsDead() && !IsInfant() && !IsMale)
		{
			return HasFlag(Flags.Reserved9);
		}
		return false;
	}

	public override void ServerInit()
	{
		base.ServerInit();
		milkTimer = new PersistentTimer(timers, 5)
		{
			onElapsed = AddMilkCharge,
			carryOnReplace = false
		};
		if (!Application.isLoadingSave)
		{
			MilkCharges = (GivesMilk ? 1 : 0);
		}
		RefreshProduction();
	}

	protected override void RefreshProduction()
	{
		SyncMilkReady();
		milkTimer.RunWhile(ProducesYield && MilkCharges < MaxMilkCharges, MilkInterval);
	}

	public override void Save(SaveInfo info)
	{
		base.Save(info);
		if (info.forDisk)
		{
			info.msg.livestockAnimal.milkCharges = MilkCharges;
		}
	}

	public override void Load(LoadInfo info)
	{
		if (isServer && info.fromDisk)
		{
			MilkCharges = SavedMilkCharges(info.msg);
		}
		base.Load(info);
	}

	private int SavedMilkCharges(Entity saved)
	{
		int num = ((saved.livestockAnimal != null) ? saved.livestockAnimal.milkCharges : 0);
		if (num == 0 && saved.baseEntity != null && (saved.baseEntity.flags & 0x20000) != 0)
		{
			num = 1;
		}
		return Mathf.Clamp(num, 0, MaxMilkCharges);
	}

	[RPC_Server]
	[RPC_Server.CallsPerSecond(5uL)]
	[RPC_Server.IsVisible(3f)]
	public void MilkCow(RPCMessage msg)
	{
		TryMilk(msg.player);
	}

	public bool TryMilk(BasePlayer player)
	{
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)player == (Object)null || IsDead())
		{
			return false;
		}
		if (IsMale && IsAdult())
		{
			OnGrabbedBy(player);
			if (player.IsConnected)
			{
				player.ShowToast(GameTip.Styles.Error, MilkBullPhrase, false);
			}
			return false;
		}
		if (!CanBeMilked(player))
		{
			return false;
		}
		MilkCharges = Mathf.Max(0, MilkCharges - 1);
		RefreshProduction();
		if ((Object)(object)MilkDef != (Object)null)
		{
			int num = MilkedAmount();
			Item item = ItemManager.Create(MilkDef, num, 0uL, isServerSide: true, 0uL);
			object obj = Interface.CallHook("OnCowMilk", this, player, item);
			if (obj != null)
			{
				item.Remove();
				if (!(obj is bool))
				{
					return false;
				}
				return (bool)obj;
			}
			player.GiveItem(item);
			if (MilkEffect != null && MilkEffect.isValid)
			{
				Effect.server.Run(MilkEffect.resourcePath, this, 0u, Vector3.zero, Vector3.zero);
			}
			Interface.CallHook("OnCowMilked", this, player, item);
			Facepunch.Rust.Analytics.Azure.OnLivestockCowMilked(player, this, num);
		}
		if (!TrustsToHandle(player))
		{
			ReactToUntrustedHandling(player);
		}
		return true;
	}

	public int MilkedAmount()
	{
		return Mathf.Max(1, Mathf.RoundToInt((float)MilkAmount * GeneScale(LivestockGene.Yield)));
	}

	public override bool TryGetYield(out ItemDefinition item, out int amount, out float interval)
	{
		item = MilkDef;
		amount = MilkedAmount();
		interval = MilkInterval;
		if ((Object)(object)item != (Object)null)
		{
			return interval > 0f;
		}
		return false;
	}

	protected override void FillYieldProgress(LivestockAnimal update)
	{
		update.yieldProgress = ((MilkCharges > 0) ? 1f : LivestockAnimal.ProgressOf(milkTimer));
	}

	private void SyncMilkReady()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved9, HasOwner && MilkCharges > 0);
	}

	public override void RestartProductionForGenes()
	{
		if (milkTimer.IsRunning)
		{
			milkTimer.Start(MilkInterval);
		}
	}

	private void AddMilkCharge()
	{
		MilkCharges = Mathf.Min(MaxMilkCharges, MilkCharges + 1);
		RefreshProduction();
	}

	public bool ForceMilkReady()
	{
		if (!ProducesYield)
		{
			return false;
		}
		AddMilkCharge();
		return true;
	}

	static Cow()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected Obj, but got Unknown
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected Obj, but got Unknown
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected Obj, but got Unknown
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected Obj, but got Unknown
	}
}
