using System;
using System.Collections.Generic;
using ConVar;
using Network;
using Oxide.Core;
using Rust;
using UnityEngine;
using UnityEngine.Assertions;

public class AdventCalendar : BaseCombatEntity
{
	[Serializable]
	public class DayReward
	{
		public ItemAmount[] rewards;

		public AlternativeReward[] alternativeRewards;
	}

	[Serializable]
	public class AlternativeReward
	{
		public ItemAmount[] rewards;

		public Era era;
	}

	public int startMonth;

	public int startDay;

	public DayReward[] days;

	public GameObject[] crosses;

	public static List<AdventCalendar> all = new List<AdventCalendar>();

	public static Dictionary<ulong, List<int>> playerRewardHistory = new Dictionary<ulong, List<int>>();

	public static readonly Phrase CheckLater = new Phrase("adventcalendar.checklater", "You've already claimed today's gift. Come back tomorrow.");

	public static readonly Phrase EventOver = new Phrase("adventcalendar.eventover", "The Advent Calendar event is over. See you next year.");

	public GameObjectRef giftEffect;

	public GameObjectRef boxCloseEffect;

	[ServerVar]
	public static int overrideAdventCalendarDay = 0;

	[ServerVar]
	public static int overrideAdventCalendarMonth = 0;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("AdventCalendar.OnRpcMessage"))
		{
			if (rpc == 1911254136 && (Object)(object)player != (Object)null)
			{
				Assert.IsTrue(player.isServer, "SV_RPC Message is using a clientside player!");
				if (Global.developer > 2)
				{
					Debug.Log((object)("SV_RPCMessage: " + ((object)player)?.ToString() + " - RPC_RequestGift"));
				}
				using (TimeWarning.New("RPC_RequestGift"))
				{
					using (TimeWarning.New("Conditions"))
					{
						if (!RPC_Server.CallsPerSecond.Test(1911254136u, "RPC_RequestGift", this, player, 1uL))
						{
							return true;
						}
						if (!RPC_Server.IsVisible.Test(1911254136u, "RPC_RequestGift", this, player, 3f))
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
							RPC_RequestGift(msg2);
						}
					}
					catch (Exception ex)
					{
						Debug.LogException(ex);
						player.Kick("RPC Error in RPC_RequestGift");
					}
				}
				return true;
			}
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public override void ServerInit()
	{
		base.ServerInit();
		all.Add(this);
	}

	public override void DestroyShared()
	{
		all.Remove(this);
		base.DestroyShared();
	}

	public void AwardGift(BasePlayer player)
	{
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		if (Interface.CallHook("OnAdventGiftAward", this, player) != null)
		{
			return;
		}
		DateTime now = DateTime.Now;
		int num = ((overrideAdventCalendarDay > 0) ? overrideAdventCalendarDay : now.Day) - startDay;
		if (((overrideAdventCalendarMonth > 0) ? overrideAdventCalendarMonth : now.Month) != startMonth || num < 0 || num >= days.Length)
		{
			return;
		}
		if (!playerRewardHistory.ContainsKey(player.userID))
		{
			playerRewardHistory.Add(player.userID, new List<int>());
		}
		playerRewardHistory[player.userID].Add(num);
		Effect.server.Run(giftEffect.resourcePath, ((Component)player).transform.position);
		Effect.server.Run(boxCloseEffect.resourcePath, ((Component)this).transform.position + Vector3.up * 1.5f);
		DayReward dayReward = days[num];
		ItemAmount[] rewards = dayReward.rewards;
		if ((int)ConVar.Server.Era != 0 && dayReward.alternativeRewards != null)
		{
			AlternativeReward[] alternativeRewards = dayReward.alternativeRewards;
			foreach (AlternativeReward alternativeReward in alternativeRewards)
			{
				if (alternativeReward.era == ConVar.Server.Era)
				{
					rewards = alternativeReward.rewards;
					break;
				}
			}
		}
		foreach (ItemAmount itemAmount in rewards)
		{
			if (itemAmount.itemDef.IsAllowed((EraRestriction)2))
			{
				player.GiveItem(ItemManager.CreateByItemID(itemAmount.itemid, Mathf.CeilToInt(itemAmount.amount), 0uL, 0uL).SetItemOwnership(player, ItemOwnershipPhrases.AdventCalendar), GiveItemReason.PickedUp);
			}
		}
		Interface.CallHook("OnAdventGiftAwarded", this, player);
	}

	public bool WasAwardedTodaysGift(BasePlayer player)
	{
		object obj = Interface.CallHook("CanBeAwardedAdventGift", this, player);
		if (obj is bool)
		{
			return !(bool)obj;
		}
		if (!playerRewardHistory.ContainsKey(player.userID))
		{
			return false;
		}
		DateTime now = DateTime.Now;
		if (((overrideAdventCalendarMonth > 0) ? overrideAdventCalendarMonth : now.Month) != startMonth)
		{
			return true;
		}
		int num = ((overrideAdventCalendarDay > 0) ? overrideAdventCalendarDay : now.Day) - startDay;
		if (num < 0 || num >= days.Length)
		{
			return true;
		}
		if (playerRewardHistory[player.userID].Contains(num))
		{
			return true;
		}
		return false;
	}

	[RPC_Server.CallsPerSecond(1uL)]
	[RPC_Server.IsVisible(3f)]
	[RPC_Server]
	public void RPC_RequestGift(RPCMessage msg)
	{
		BasePlayer player = msg.player;
		if (WasAwardedTodaysGift(player))
		{
			player.ShowToast(GameTip.Styles.Red_Normal, CheckLater, false);
		}
		else
		{
			AwardGift(player);
		}
	}
}
