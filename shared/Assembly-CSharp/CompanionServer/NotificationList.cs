using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ConVar;
using Facepunch;
using Network;
using Newtonsoft.Json;
using UnityEngine;

namespace CompanionServer;

public class NotificationList
{
	private const string ApiEndpoint = "https://companion-rust.facepunch.com/api/push/send";

	private readonly HashSet<ulong> _subscriptions = new HashSet<ulong>();

	private double _lastSend;

	public bool AddSubscription(ulong steamId)
	{
		if (steamId == 0L)
		{
			return false;
		}
		if (_subscriptions.Count >= 50)
		{
			return false;
		}
		return _subscriptions.Add(steamId);
	}

	public bool RemoveSubscription(ulong steamId)
	{
		return _subscriptions.Remove(steamId);
	}

	public bool HasSubscription(ulong steamId)
	{
		return _subscriptions.Contains(steamId);
	}

	public List<ulong> ToList()
	{
		List<ulong> list = Pool.Get<List<ulong>>();
		foreach (ulong subscription in _subscriptions)
		{
			list.Add(subscription);
		}
		return list;
	}

	public void LoadFrom(List<ulong> steamIds)
	{
		_subscriptions.Clear();
		if (steamIds == null)
		{
			return;
		}
		foreach (ulong steamId in steamIds)
		{
			_subscriptions.Add(steamId);
		}
	}

	public void IntersectWith(HashSet<ulong> players)
	{
		List<ulong> list = Pool.Get<List<ulong>>();
		foreach (ulong player in players)
		{
			list.Add(player);
		}
		_subscriptions.IntersectWith(list);
		Pool.FreeUnmanaged<ulong>(ref list);
	}

	public Task<NotificationSendResult> SendNotification(NotificationChannel channel, string title, string body, string type)
	{
		double realtimeSinceStartup = TimeEx.realtimeSinceStartup;
		if (realtimeSinceStartup - _lastSend < 15.0)
		{
			return Task.FromResult(NotificationSendResult.RateLimited);
		}
		Dictionary<string, string> dictionary = Util.TryGetServerPairingData();
		if (dictionary == null)
		{
			return Task.FromResult(NotificationSendResult.Failed);
		}
		if (!string.IsNullOrWhiteSpace(type))
		{
			dictionary["type"] = type;
		}
		_lastSend = realtimeSinceStartup;
		return SendNotificationImpl(_subscriptions, channel, title, body, dictionary);
	}

	public static async Task<NotificationSendResult> SendNotificationTo(ICollection<ulong> steamIds, NotificationChannel channel, string title, string body, Dictionary<string, string> data)
	{
		NotificationSendResult notificationSendResult = await SendNotificationImpl(steamIds, channel, title, body, data);
		if (notificationSendResult == NotificationSendResult.NoTargetsFound)
		{
			notificationSendResult = NotificationSendResult.Sent;
		}
		return notificationSendResult;
	}

	public static async Task<NotificationSendResult> SendNotificationTo(ulong steamId, NotificationChannel channel, string title, string body, Dictionary<string, string> data)
	{
		HashSet<ulong> set = Pool.Get<HashSet<ulong>>();
		set.Clear();
		set.Add(steamId);
		NotificationSendResult result = await SendNotificationImpl(set, channel, title, body, data);
		set.Clear();
		Pool.FreeUnmanaged<ulong>(ref set);
		return result;
	}

	private static async Task<NotificationSendResult> SendNotificationImpl(ICollection<ulong> steamIds, NotificationChannel channel, string title, string body, Dictionary<string, string> data)
	{
		if (!Server.IsEnabled || !App.notifications)
		{
			return NotificationSendResult.Disabled;
		}
		if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(body))
		{
			return NotificationSendResult.Empty;
		}
		if (steamIds.Count == 0)
		{
			return NotificationSendResult.Sent;
		}
		List<List<ulong>> batches = Pool.Get<List<List<ulong>>>();
		List<ulong> list = null;
		foreach (ulong steamId in steamIds)
		{
			if (list == null)
			{
				list = Pool.Get<List<ulong>>();
			}
			list.Add(steamId);
			if (list.Count >= 100)
			{
				batches.Add(list);
				list = null;
			}
		}
		if (list != null && list.Count > 0)
		{
			batches.Add(list);
		}
		NotificationSendResult? errorResult = null;
		bool anySent = false;
		foreach (List<ulong> item in batches)
		{
			List<ulong> batchCopy = item;
			NotificationSendResult notificationSendResult = await SendNotificationBatchImpl(batchCopy, channel, title, body, data);
			Pool.FreeUnmanaged<ulong>(ref batchCopy);
			switch (notificationSendResult)
			{
			case NotificationSendResult.Failed:
				errorResult = NotificationSendResult.Failed;
				break;
			case NotificationSendResult.ServerError:
				if (errorResult != NotificationSendResult.Failed)
				{
					errorResult = NotificationSendResult.ServerError;
				}
				break;
			}
			if (notificationSendResult == NotificationSendResult.Sent)
			{
				anySent = true;
			}
		}
		Pool.FreeUnmanaged<List<ulong>>(ref batches);
		if (data != null)
		{
			data.Clear();
			Pool.FreeUnmanaged<string, string>(ref data);
		}
		if (errorResult.HasValue)
		{
			return errorResult.Value;
		}
		return anySent ? NotificationSendResult.Sent : NotificationSendResult.NoTargetsFound;
	}

	private static async Task<NotificationSendResult> SendNotificationBatchImpl(IEnumerable<ulong> steamIds, NotificationChannel channel, string title, string body, Dictionary<string, string> data)
	{
		PushRequest pushRequest = Pool.Get<PushRequest>();
		pushRequest.ServerToken = Server.Token;
		pushRequest.Channel = channel;
		pushRequest.Title = title;
		pushRequest.Body = body;
		pushRequest.Data = data;
		pushRequest.SteamIds = Pool.Get<List<ulong>>();
		foreach (ulong steamId in steamIds)
		{
			pushRequest.SteamIds.Add(steamId);
		}
		string content = JsonConvert.SerializeObject((object)pushRequest);
		Pool.Free<PushRequest>(ref pushRequest);
		try
		{
			StringContent content2 = new StringContent(content, Encoding.UTF8, "application/json");
			HttpResponseMessage httpResponseMessage = await WebUtil.HttpClient.PostAsync("https://companion-rust.facepunch.com/api/push/send", content2);
			if (!httpResponseMessage.IsSuccessStatusCode)
			{
				DebugEx.LogWarning($"Failed to send notification: {httpResponseMessage.StatusCode}", (StackTraceLogType)0);
				return NotificationSendResult.ServerError;
			}
			if (httpResponseMessage.StatusCode == HttpStatusCode.Accepted)
			{
				return NotificationSendResult.NoTargetsFound;
			}
			return NotificationSendResult.Sent;
		}
		catch (Exception arg)
		{
			DebugEx.LogWarning($"Exception thrown when sending notification: {arg}", (StackTraceLogType)0);
			return NotificationSendResult.Failed;
		}
	}
}
