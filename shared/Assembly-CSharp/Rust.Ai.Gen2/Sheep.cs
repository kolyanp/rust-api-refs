using System;
using System.Runtime.CompilerServices;
using System.Threading;
using ConVar;
using Facepunch;
using Facepunch.Rust;
using Network;
using ProtoBuf;
using UnityEngine;

namespace Rust.Ai.Gen2;

[SoftRequireComponent(typeof(CowFSM))]
public class Sheep : LivestockAnimal
{
	[ServerVar(Help = "Population active on the server, per square km", ShowInAdminUI = true)]
	public static float Population = 2f;

	[Tooltip("What shearing the whole sheep yields, handed over a quarter at a time so the four quadrants add up to it. An entry too small to round to one gives nothing")]
	public ItemAmount[] ShearItems = Array.Empty<ItemAmount>();

	[Tooltip("How long (in seconds) after being sheared before the wool grows back. Every quadrant regrows together")]
	public float ShearCooldown = 300f;

	[Tooltip("Optional effect played on the sheep each time a quarter of its fleece is sheared")]
	public GameObjectRef ShearEffect;

	[Tooltip("Optional wool thrown off the sheep each time a quarter of its fleece is sheared, tinted to its fleece by a LivestockWoolTint")]
	public GameObjectRef ShearWoolEffect;

	public const Flags ShearReady = Flags.Reserved9;

	public const int ShearTimerId = 6;

	private const float WoolGrace = 0.12f;

	private LivestockFleece fleece;

	private static readonly Phrase WildSheepPhrase = new Phrase("livestock.wild.sheep", "Wild sheep");

	private static readonly Phrase WildRamPhrase = new Phrase("livestock.wild.ram", "Wild ram");

	private static readonly Phrase WildLambPhrase = new Phrase("livestock.wild.lamb", "Wild lamb");

	private PersistentTimer shearTimer;

	private int __sync_ShornQuadrants;

	[Sync(Autosave = true)]
	public int ShornQuadrants
	{
		[CompilerGenerated]
		get
		{
			return __sync_ShornQuadrants;
		}
		[CompilerGenerated]
		set
		{
			if (!IsSyncVarEqual(__sync_ShornQuadrants, value))
			{
				__sync_ShornQuadrants = value;
				byte nameID = __GetWeaverID("ShornQuadrants");
				QueueSyncVar(nameID);
			}
		}
	}

	public LivestockFleece Fleece
	{
		get
		{
			if (!((Object)(object)fleece != (Object)null))
			{
				return fleece = ((Component)this).GetComponent<LivestockFleece>();
			}
			return fleece;
		}
	}

	public override int ShornFleece => ShornQuadrants;

	public int ShornCount
	{
		get
		{
			int num = 0;
			for (int i = 0; i < 4; i++)
			{
				if (!HasWool((LivestockFleece.Quadrant)i))
				{
					num++;
				}
			}
			return num;
		}
	}

	public float ShearInterval => ShearCooldown / GeneScale(LivestockGene.Yield);

	public override string Categorize()
	{
		return "Sheep";
	}

	protected override Phrase WildLabel()
	{
		if (IsInfant())
		{
			return WildLambPhrase;
		}
		if (!IsMale)
		{
			return WildSheepPhrase;
		}
		return WildRamPhrase;
	}

	public bool HasWool(LivestockFleece.Quadrant quadrant)
	{
		return (ShornQuadrants & (1 << (int)quadrant)) == 0;
	}

	public bool CanBeSheared(BasePlayer player)
	{
		if ((Object)(object)player != (Object)null && !IsDead() && !IsInfant())
		{
			return ShornQuadrants != 15;
		}
		return false;
	}

	public bool IsShearTool(AttackEntity weapon)
	{
		BaseMelee baseMelee = weapon as BaseMelee;
		if ((Object)(object)baseMelee == (Object)null || baseMelee.gathering == null)
		{
			return false;
		}
		ResourceDispenser.GatherPropertyEntry gatherInfoFromIndex = baseMelee.GetGatherInfoFromIndex(ResourceDispenser.GatherType.Flesh);
		if (gatherInfoFromIndex != null && gatherInfoFromIndex.gatherDamage > 0f)
		{
			return gatherInfoFromIndex.destroyFraction <= 0f;
		}
		return false;
	}

	public bool WouldShear(HitInfo info, out LivestockFleece.Quadrant quadrant)
	{
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		quadrant = LivestockFleece.Quadrant.FrontLeft;
		if (info == null || (Object)(object)info.InitiatorPlayer == (Object)null || info.IsProjectile())
		{
			return false;
		}
		if (!IsShearTool(info.Weapon) || !CanBeSheared(info.InitiatorPlayer) || (Object)(object)Fleece == (Object)null)
		{
			return false;
		}
		return TryPickShearQuadrant(info.HitPositionWorld, out quadrant);
	}

	public bool TryPickShearQuadrant(Vector3 worldPoint, out LivestockFleece.Quadrant quadrant)
	{
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		quadrant = LivestockFleece.Quadrant.FrontLeft;
		if ((Object)(object)Fleece == (Object)null)
		{
			return false;
		}
		float num = 0f;
		bool result = false;
		for (int i = 0; i < 4; i++)
		{
			LivestockFleece.Quadrant quadrant2 = (LivestockFleece.Quadrant)i;
			if (HasWool(quadrant2))
			{
				float num2 = Fleece.WeightAt(worldPoint, quadrant2);
				if (!(num2 < 0.12f) && !(num2 <= num))
				{
					num = num2;
					quadrant = quadrant2;
					result = true;
				}
			}
		}
		return result;
	}

	public override void OnAttacked(HitInfo info)
	{
		if (!WouldShear(info, out var quadrant))
		{
			if (info.damageTypes.Total() <= 0f)
			{
				if (!IsDead())
				{
					DoHitNotify(info);
				}
			}
			else
			{
				base.OnAttacked(info);
			}
			return;
		}
		if (isServer)
		{
			if (!Shear(info.InitiatorPlayer, quadrant, WoolYieldScale(info.Weapon)))
			{
				base.OnAttacked(info);
				return;
			}
			WearShearTool(info);
		}
		info.damageTypes.Clear();
		info.DoHitEffects = false;
		if (!IsDead())
		{
			DoHitNotify(info);
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		shearTimer = new PersistentTimer(timers, 6)
		{
			onElapsed = EnableShearing,
			onStarted = SyncShearReady
		};
		SyncShearReady();
	}

	private void WearShearTool(HitInfo info)
	{
		if (info.Weapon is BaseMelee baseMelee)
		{
			baseMelee.LoseCondition(baseMelee.GetConditionLoss());
		}
		info.DidGather = true;
	}

	private static float WoolYieldScale(AttackEntity weapon)
	{
		Item item = weapon.GetItem();
		if (item == null)
		{
			return 1f;
		}
		ItemModShearTool component = ((Component)item.info).GetComponent<ItemModShearTool>();
		if (!((Object)(object)component != (Object)null))
		{
			return 1f;
		}
		return component.WoolYieldScale;
	}

	public bool Shear(BasePlayer player, LivestockFleece.Quadrant quadrant, float woolYieldScale = 1f)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		if (!CanBeSheared(player) || !HasWool(quadrant))
		{
			return false;
		}
		ShornQuadrants |= 1 << (int)quadrant;
		if (ShearEffect != null && ShearEffect.isValid)
		{
			Effect.server.Run(ShearEffect.resourcePath, this, 0u, Vector3.zero, Vector3.zero);
		}
		if (ShearWoolEffect != null && ShearWoolEffect.isValid)
		{
			Effect.server.Run(ShearWoolEffect.resourcePath, this, 0u, Vector3.zero, Vector3.zero);
		}
		GiveShearItems(player, woolYieldScale);
		if (!TrustsToHandle(player))
		{
			ReactToUntrustedHandling(player);
		}
		Analytics.Azure.OnLivestockSheepSheared(player, this);
		if (HasOwner)
		{
			StartRegrowing();
		}
		else
		{
			SyncShearReady();
		}
		return true;
	}

	protected override void RefreshProduction()
	{
		shearTimer.RunWhile(ProducesYield && ShornQuadrants != 0, ShearInterval);
	}

	private void StartRegrowing()
	{
		shearTimer.Start(ShearInterval);
	}

	public bool TryShear(BasePlayer player)
	{
		for (int i = 0; i < 4; i++)
		{
			if (HasWool((LivestockFleece.Quadrant)i))
			{
				return Shear(player, (LivestockFleece.Quadrant)i);
			}
		}
		return false;
	}

	private void GiveShearItems(BasePlayer player, float woolYieldScale)
	{
		ItemAmount[] shearItems = ShearItems;
		foreach (ItemAmount itemAmount in shearItems)
		{
			if (itemAmount != null && !((Object)(object)itemAmount.itemDef == (Object)null))
			{
				int num = ShornAmount(itemAmount, woolYieldScale);
				if (num > 0)
				{
					Item item = ItemManager.Create(itemAmount.itemDef, num, 0uL, isServerSide: true, 0uL);
					item.SetItemOwnership(player, ItemOwnershipPhrases.GatheredPhrase);
					player.GiveItem(item, GiveItemReason.ResourceHarvested, GiveItemOptions.BackpackOverflow);
				}
			}
		}
	}

	public int FleeceAmount(ItemAmount shearItem, float woolYieldScale = 1f)
	{
		if (shearItem.amount <= 0f)
		{
			return 0;
		}
		return Mathf.RoundToInt(shearItem.amount * GeneScale(LivestockGene.Yield) * woolYieldScale);
	}

	public int ShornAmount(ItemAmount shearItem, float woolYieldScale = 1f)
	{
		int total = FleeceAmount(shearItem, woolYieldScale);
		int shornCount = ShornCount;
		if (shornCount <= 0)
		{
			return 0;
		}
		return QuarterSplit(total, shornCount) - QuarterSplit(total, shornCount - 1);
	}

	public int UnshornAmount(ItemAmount shearItem)
	{
		int num = FleeceAmount(shearItem);
		return num - QuarterSplit(num, ShornCount);
	}

	private static int QuarterSplit(int total, int quarters)
	{
		return total * quarters / 4;
	}

	public override void AddCorpseYield(ResourceDispenser dispenser)
	{
		if ((Object)(object)dispenser == (Object)null || IsInfant())
		{
			return;
		}
		ItemAmount[] shearItems = ShearItems;
		foreach (ItemAmount itemAmount in shearItems)
		{
			if (itemAmount != null && !((Object)(object)itemAmount.itemDef == (Object)null))
			{
				int num = UnshornAmount(itemAmount);
				if (num > 0)
				{
					dispenser.containedItems.Add(new ItemAmount(itemAmount.itemDef, num));
				}
			}
		}
	}

	public override bool TryGetYield(out ItemDefinition item, out int amount, out float interval)
	{
		item = null;
		amount = 0;
		interval = ShearInterval;
		ItemAmount[] shearItems = ShearItems;
		foreach (ItemAmount itemAmount in shearItems)
		{
			if (itemAmount != null && !((Object)(object)itemAmount.itemDef == (Object)null))
			{
				int num = FleeceAmount(itemAmount);
				if (num > amount)
				{
					item = itemAmount.itemDef;
					amount = num;
				}
			}
		}
		if ((Object)(object)item != (Object)null)
		{
			return interval > 0f;
		}
		return false;
	}

	protected override void FillYieldProgress(LivestockAnimal update)
	{
		update.yieldProgress = ((ShornQuadrants == 0) ? 1f : LivestockAnimal.ProgressOf(shearTimer));
	}

	private void SyncShearReady()
	{
		using FlagsUpdateScope flagsUpdateScope = StartSetFlags(FlagsUpdateMode.SendNetworkUpdate);
		flagsUpdateScope.Set(Flags.Reserved9, ShornQuadrants != 15);
	}

	private void EnableShearing()
	{
		ShornQuadrants = 0;
		SyncShearReady();
	}

	public bool ForceShearReady()
	{
		if (IsDead() || IsInfant())
		{
			return false;
		}
		shearTimer.Stop();
		EnableShearing();
		return true;
	}

	private void OnSyncVar_ShornQuadrants(int? oldValue, int newValue)
	{
	}

	protected override bool WriteSyncVar(byte id, NetWrite writer)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (id == 5)
		{
			if (Global.developer > 2)
			{
				NetworkableId iD = net.ID;
				Debug.Log((object)("SyncVar Writing: ShornQuadrants for " + ((object)iD/*cast due to constrained. prefix*/).ToString()));
			}
			SyncVarNetWrite(writer, __sync_ShornQuadrants);
			return true;
		}
		return base.WriteSyncVar(id, writer);
	}

	protected override bool OnSyncVar(byte id, NetRead reader, bool fromAutoSave = false)
	{
		if (id == 5)
		{
			try
			{
				int? oldValue = __sync_ShornQuadrants;
				int newValue = (__sync_ShornQuadrants = reader.Int32());
				if (fromAutoSave)
				{
					oldValue = null;
				}
				OnSyncVar_ShornQuadrants(oldValue, newValue);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
			}
			return true;
		}
		return base.OnSyncVar(id, reader, fromAutoSave);
	}

	private byte __GetWeaverID(string propertyName)
	{
		if (propertyName == "ShornQuadrants")
		{
			return 5;
		}
		return byte.MaxValue;
	}

	protected override void WriteAutoSaveSyncVars(NetWrite writer)
	{
		base.WriteAutoSaveSyncVars(writer);
		WriteSyncVar(5, writer);
	}

	protected override void ReadAutoSaveSyncVars(NetRead reader)
	{
		base.ReadAutoSaveSyncVars(reader);
		OnSyncVar(5, reader, fromAutoSave: true);
	}

	protected override bool AutoSaveSyncVars(SaveInfo save)
	{
		NetWrite netWrite = Net.sv.StartWrite();
		WriteAutoSaveSyncVars(netWrite);
		(byte[] Buffer, int Length) buffer = netWrite.GetBuffer();
		byte[] item = buffer.Buffer;
		int item2 = buffer.Length;
		byte[] array = _autosaveBuffer;
		if (array == null || array.Length < item2)
		{
			byte[] array2 = BaseEntity._autosaveBufferPool.Rent(item2);
			while (array == null || array.Length < item2)
			{
				byte[] array3 = Interlocked.CompareExchange(ref _autosaveBuffer, array2, array);
				if (array3 == array)
				{
					if (array3 != null)
					{
						BaseEntity._autosaveBufferPool.Return(array3);
					}
					array = array2;
					break;
				}
				array = array3;
			}
			if (array != array2)
			{
				BaseEntity._autosaveBufferPool.Return(array2);
			}
		}
		Buffer.BlockCopy(item, 0, array, 0, item2);
		save.msg.baseEntity.syncVars = array;
		Pool.Free<NetWrite>(ref netWrite);
		return true;
	}

	protected override bool AutoLoadSyncVars(LoadInfo load)
	{
		if (load.msg.baseEntity != null && load.msg.baseEntity.syncVars != null)
		{
			NetRead netRead = Pool.Get<NetRead>();
			netRead.Init(load.msg.baseEntity.syncVars.AsSpan());
			ReadAutoSaveSyncVars(netRead);
			Pool.Free<NetRead>(ref netRead);
		}
		return true;
	}

	protected override void ResetSyncVars()
	{
		base.ResetSyncVars();
		__sync_ShornQuadrants = 0;
	}

	protected override bool ShouldInvalidateCache(byte id)
	{
		if (id == 5)
		{
			return true;
		}
		return base.ShouldInvalidateCache(id);
	}

	static Sheep()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected Obj, but got Unknown
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Expected Obj, but got Unknown
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected Obj, but got Unknown
	}
}
