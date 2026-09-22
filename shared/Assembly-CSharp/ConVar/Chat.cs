using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CircularBuffer;
using CompanionServer;
using Facepunch;
using Facepunch.Math;
using Facepunch.Rust;
using Network;
using Oxide.Core;
using UnityEngine;

namespace ConVar;

[Factory("chat")]
public class Chat : ConsoleSystem
{
	public enum ChatChannel
	{
		Global = 0,
		Team = 1,
		Server = 2,
		Cards = 3,
		Local = 4,
		Clan = 5,
		ExternalDM = 6,
		MaxValue = ExternalDM
	}

	public struct ChatEntry
	{
		public ChatChannel Channel { get; set; }

		public string Message { get; set; }

		public string UserId { get; set; }

		public string Username { get; set; }

		public string Color { get; set; }

		public int Time { get; set; }
	}

	[ServerVar(Help = "(Generated) Maximum distance in metres within which local chat messages are visible to nearby players; messages from beyond this range are not received")]
	public static float localChatRange = 100f;

	[ReplicatedVar]
	public static bool globalchat = true;

	[ReplicatedVar]
	public static bool localchat = false;

	private const float textVolumeBoost = 0.2f;

	[ReplicatedVar]
	public static bool hideChatInTutorial = true;

	[ClientVar(Help = "(Generated) When enabled, this system is globally active; disable to deactivate the system for the current session")]
	[ServerVar(Help = "(Generated) When enabled, this system is globally active; disable to deactivate the system for the current session")]
	public static bool enabled = true;

	[ServerVar(Help = "Number of messages to keep in memory for chat history")]
	public static int historysize = 1000;

	public static CircularBuffer<ChatEntry> History = new CircularBuffer<ChatEntry>(historysize);

	[ServerVar(Help = "(Generated) When enabled, all chat messages are written to the server log file in addition to being broadcast to players")]
	public static bool serverlog = true;

	public static void BroadcastPlayerAction(BasePlayer subject, string action)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		if (Interface.CallHook("OnPlayerActionBroadcast", subject, action) != null)
		{
			return;
		}
		Enumerator<BasePlayer> enumerator = BasePlayer.activePlayerList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				if (!((Object)(object)current == (Object)null) && current.net?.connection != null)
				{
					string text = StringEx.EscapeRichText(((Object)(object)subject != (Object)null) ? NameHelper.GetPlayerNameStreamSafe(current, subject) : "SERVER", false);
					ConsoleNetwork.SendClientCommand(current.net.connection, "chat.add", 2, 0, "<color=#eee>SERVER</color> " + text + " " + action);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		RecordPlayerAction((((Object)(object)subject != (Object)null) ? subject.displayName : "SERVER") + " " + action, subject);
	}

	public static void BroadcastPlayerAction(BasePlayer subjectA, string middle, BasePlayer subjectB, string suffix)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (Interface.CallHook("OnPlayerActionBroadcast", subjectA, middle, subjectB, suffix) != null)
		{
			return;
		}
		Enumerator<BasePlayer> enumerator = BasePlayer.activePlayerList.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				BasePlayer current = enumerator.Current;
				if (!((Object)(object)current == (Object)null) && current.net?.connection != null)
				{
					string text = StringEx.EscapeRichText(((Object)(object)subjectA != (Object)null) ? NameHelper.GetPlayerNameStreamSafe(current, subjectA) : "SERVER", false);
					string text2 = StringEx.EscapeRichText(((Object)(object)subjectB != (Object)null) ? NameHelper.GetPlayerNameStreamSafe(current, subjectB) : "SERVER", false);
					ConsoleNetwork.SendClientCommand(current.net.connection, "chat.add", 2, 0, "<color=#eee>SERVER</color> " + text + middle + text2 + suffix);
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		string obj = (((Object)(object)subjectA != (Object)null) ? subjectA.displayName : "SERVER");
		string text3 = (((Object)(object)subjectB != (Object)null) ? subjectB.displayName : "SERVER");
		RecordPlayerAction(obj + middle + text3 + suffix, subjectA);
	}

	private static void RecordPlayerAction(string message, BasePlayer subject)
	{
		Record(new ChatEntry
		{
			Channel = ChatChannel.Server,
			Message = message,
			UserId = (((Object)(object)subject != (Object)null) ? subject.UserIDString : "0"),
			Username = (((Object)(object)subject != (Object)null) ? subject.displayName : "SERVER"),
			Color = "#eee",
			Time = Epoch.Current
		});
	}

	public static void Broadcast(string message, string username = "SERVER", string color = "#eee", ulong userid = 0uL)
	{
		if (Interface.CallHook("OnServerMessage", message, username, color, userid) == null)
		{
			string text = StringEx.EscapeRichText(username, false);
			ConsoleNetwork.BroadcastToAllClients("chat.add", 2, 0, "<color=" + color + ">" + text + "</color> " + message);
			Record(new ChatEntry
			{
				Channel = ChatChannel.Server,
				Message = message,
				UserId = userid.ToString(),
				Username = username,
				Color = color,
				Time = Epoch.Current
			});
		}
	}

	[ServerUserVar]
	public static void say(Arg arg)
	{
		if (globalchat)
		{
			sayImpl(ChatChannel.Global, arg);
		}
	}

	[ServerUserVar]
	public static void localsay(Arg arg)
	{
		if (localchat)
		{
			sayImpl(ChatChannel.Local, arg);
		}
	}

	[ServerUserVar]
	public static void teamsay(Arg arg)
	{
		sayImpl(ChatChannel.Team, arg);
	}

	[ServerUserVar]
	public static void cardgamesay(Arg arg)
	{
		sayImpl(ChatChannel.Cards, arg);
	}

	[ServerUserVar]
	public static void clansay(Arg arg)
	{
		sayImpl(ChatChannel.Clan, arg);
	}

	private static void sayImpl(ChatChannel targetChannel, Arg arg)
	{
		if (!enabled)
		{
			arg.ReplyWith("Chat is disabled.");
			return;
		}
		BasePlayer player = ArgEx.Player(arg);
		if (!Object.op_Implicit((Object)(object)player) || (hideChatInTutorial && player.IsInTutorial) || player.HasPlayerFlag(BasePlayer.PlayerFlags.ChatMute))
		{
			return;
		}
		if (!player.IsAdmin && !player.IsDeveloper)
		{
			if (player.NextChatTime == 0f)
			{
				player.NextChatTime = Time.realtimeSinceStartup - 30f;
			}
			if (player.NextChatTime > Time.realtimeSinceStartup)
			{
				player.NextChatTime += 2f;
				float num = player.NextChatTime - Time.realtimeSinceStartup;
				ConsoleNetwork.SendClientCommand(player.net.connection, "chat.add", 2, 0, "You're chatting too fast - try again in " + (num + 0.5f).ToString("0") + " seconds");
				if (num > 120f)
				{
					player.Kick("Chatting too fast");
				}
				return;
			}
		}
		string message = arg.GetString(0, "text");
		ValueTask<bool> valueTask = sayAs(targetChannel, player.userID, player.displayName, message, player);
		Facepunch.Rust.Analytics.Azure.OnChatMessage(player, message, (int)targetChannel);
		player.NextChatTime = Time.realtimeSinceStartup + 1.5f;
		if (valueTask.IsCompletedSuccessfully)
		{
			if (!valueTask.Result)
			{
				player.NextChatTime = Time.realtimeSinceStartup;
			}
			return;
		}
		Task<bool> task = valueTask.AsTask();
		task.GetAwaiter().OnCompleted(delegate
		{
			try
			{
				if (!task.Result)
				{
					player.NextChatTime = Time.realtimeSinceStartup;
				}
			}
			catch (Exception ex)
			{
				Debug.LogError((object)ex);
			}
		});
	}

	internal static string GetNameColor(ulong userId, BasePlayer player = null)
	{
		ServerUsers.UserGroup userGroup = ServerUsers.Get(userId)?.group ?? ServerUsers.UserGroup.None;
		bool flag = userGroup == ServerUsers.UserGroup.Owner || userGroup == ServerUsers.UserGroup.Moderator;
		bool num = (((Object)(object)player != (Object)null) ? player.IsDeveloper : DeveloperList.Contains(userId));
		string result = "#5af";
		if (flag)
		{
			result = "#af5";
		}
		if (num)
		{
			result = "#fa5";
		}
		return result;
	}

	internal static async ValueTask<bool> sayAs(ChatChannel targetChannel, ulong userId, string username, string message, BasePlayer player = null)
	{
		if (!Object.op_Implicit((Object)(object)player))
		{
			player = null;
		}
		if (!enabled)
		{
			return false;
		}
		if ((Object)(object)player != (Object)null && player.HasPlayerFlag(BasePlayer.PlayerFlags.ChatMute))
		{
			return false;
		}
		if ((ServerUsers.Get(userId)?.group ?? ServerUsers.UserGroup.None) == ServerUsers.UserGroup.Banned)
		{
			return false;
		}
		string strChatText = message.Replace("\n", "").Replace("\r", "").Replace("\v", "")
			.Replace("\u2028", "")
			.Replace("\u2029", "")
			.Trim();
		if (strChatText.Length > 128)
		{
			strChatText = strChatText.Substring(0, 128);
		}
		if (strChatText.Length <= 0)
		{
			return false;
		}
		object obj = Interface.CallHook("IOnPlayerChat", userId, username, strChatText, targetChannel, player);
		if (obj is bool)
		{
			return (bool)obj;
		}
		if (strChatText.StartsWith("/") || strChatText.StartsWith("\\"))
		{
			return false;
		}
		strChatText = StringEx.EscapeRichText(strChatText, false);
		if (Server.emojiOwnershipCheck && strChatText.Contains(":") && (Object)(object)player != (Object)null)
		{
			List<(TmProEmojiRedirector.EmojiSub, int)> list = Pool.Get<List<(TmProEmojiRedirector.EmojiSub, int)>>();
			TmProEmojiRedirector.FindEmojiSubstitutions(strChatText, RustEmojiLibrary.Instance, list, richText: false, isServer: true);
			foreach (var item in list)
			{
				if (!item.Item1.targetEmojiResult.CanBeUsedBy(player, player.userID))
				{
					strChatText = strChatText.Replace(":" + item.Item1.targetEmoji + ":", string.Empty);
				}
			}
			Pool.FreeUnmanaged<(TmProEmojiRedirector.EmojiSub, int)>(ref list);
			if (strChatText.Length <= 0)
			{
				return false;
			}
		}
		if (serverlog)
		{
			ServerConsole.PrintColoured("[" + targetChannel.ToString() + "] " + username + " : " + strChatText, ConsoleColor.Green);
		}
		string strName = StringEx.EscapeRichText(username, false);
		string nameColor = GetNameColor(userId, player);
		Record(new ChatEntry
		{
			Channel = targetChannel,
			Message = strChatText,
			UserId = (((Object)(object)player != (Object)null) ? player.UserIDString : userId.ToString()),
			Username = username,
			Color = nameColor,
			Time = Epoch.Current
		});
		switch (targetChannel)
		{
		case ChatChannel.Cards:
		{
			if ((Object)(object)player == (Object)null)
			{
				return false;
			}
			if (!player.isMounted)
			{
				return false;
			}
			BaseCardGameEntity baseCardGameEntity = player.GetMountedVehicle() as BaseCardGameEntity;
			if ((Object)(object)baseCardGameEntity == (Object)null || !(baseCardGameEntity.GameController?.IsAtTable(player) ?? false))
			{
				return false;
			}
			List<Network.Connection> list2 = Pool.Get<List<Network.Connection>>();
			baseCardGameEntity.GameController?.GetConnectionsInGame(list2);
			if (list2.Count > 0)
			{
				ConsoleNetwork.SendClientCommand(list2, "chat.add2", 3, userId, strChatText, strName, nameColor, 1f);
			}
			Pool.FreeUnmanaged<Network.Connection>(ref list2);
			return true;
		}
		case ChatChannel.Global:
			ConsoleNetwork.BroadcastToAllClients("chat.add2", 0, userId, strChatText, strName, nameColor, 1f);
			return true;
		case ChatChannel.Local:
		{
			if (!((Object)(object)player != (Object)null))
			{
				break;
			}
			float num = localChatRange * localChatRange;
			Enumerator<BasePlayer> enumerator2 = BasePlayer.activePlayerList.GetEnumerator();
			try
			{
				while (enumerator2.MoveNext())
				{
					BasePlayer current2 = enumerator2.Current;
					Vector3 val3 = ((Component)current2).transform.position - ((Component)player).transform.position;
					float sqrMagnitude = ((Vector3)(ref val3)).sqrMagnitude;
					if (!(sqrMagnitude > num))
					{
						ConsoleNetwork.SendClientCommand(current2.net.connection, "chat.add2", 4, userId, strChatText, strName, nameColor, Mathf.Clamp01(sqrMagnitude / num + 0.2f));
					}
				}
			}
			finally
			{
				((IDisposable)enumerator2/*cast due to constrained. prefix*/).Dispose();
			}
			return true;
		}
		case ChatChannel.Team:
		{
			RelationshipManager.PlayerTeam playerTeam = RelationshipManager.ServerInstance.FindPlayersTeam(userId);
			if (playerTeam == null)
			{
				return false;
			}
			List<Network.Connection> onlineMemberConnections = playerTeam.GetOnlineMemberConnections();
			if (onlineMemberConnections != null)
			{
				ConsoleNetwork.SendClientCommand(onlineMemberConnections, "chat.add2", 1, userId, strChatText, strName, nameColor, 1f);
			}
			playerTeam.BroadcastTeamChat(userId, strName, strChatText, nameColor);
			return true;
		}
		case ChatChannel.Clan:
		{
			ClanManager serverInstance = ClanManager.ServerInstance;
			if ((Object)(object)serverInstance == (Object)null)
			{
				return false;
			}
			if ((Object)(object)player != (Object)null && player.clanId == 0L)
			{
				return false;
			}
			try
			{
				ClanValueResult<IClan> val = ((!((Object)(object)player != (Object)null) || player.clanId == 0L) ? (await serverInstance.Backend.GetByMember(userId)) : (await serverInstance.Backend.Get(player.clanId)));
				ClanValueResult<IClan> val2 = val;
				if (!val2.IsSuccess)
				{
					return false;
				}
				if ((int)(await val2.Value.SendChatMessage(strName, strChatText, userId)) != 1)
				{
					return false;
				}
				return true;
			}
			catch (Exception ex)
			{
				Debug.LogError((object)ex);
				return false;
			}
		}
		}
		return false;
	}

	[Help("Return the last x lines of the console. Default is 200")]
	[ServerVar]
	public static IEnumerable<ChatEntry> tail(Arg arg)
	{
		int num = arg.GetInt(0, 200);
		int num2 = History.Size - num;
		if (num2 < 0)
		{
			num2 = 0;
		}
		return ((IEnumerable<ChatEntry>)History).Skip(num2);
	}

	[ServerVar]
	[Help("Search the console for a particular string")]
	public static IEnumerable<ChatEntry> search(Arg arg)
	{
		string search = arg.GetString(0, null);
		if (search == null)
		{
			return Enumerable.Empty<ChatEntry>();
		}
		return ((IEnumerable<ChatEntry>)History).Where((ChatEntry x) => x.Message.Length < 4096 && StringEx.Contains(x.Message, search, CompareOptions.IgnoreCase));
	}

	public static void Record(ChatEntry ce)
	{
		int num = Mathf.Max(historysize, 10);
		if (History.Capacity != num)
		{
			CircularBuffer<ChatEntry> val = new CircularBuffer<ChatEntry>(num);
			foreach (ChatEntry item in History)
			{
				val.PushBack(item);
			}
			History = val;
		}
		History.PushBack(ce);
		RCon.Broadcast(RCon.LogType.Chat, ce);
	}
}
