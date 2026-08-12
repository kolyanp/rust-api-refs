using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ConVar;
using Facepunch;
using Newtonsoft.Json;
using UnityEngine;

public static class PremiumUtil
{
	public struct PremiumCheckResult(bool isPremium, string failReason = null)
	{
		public bool IsPremium = isPremium;

		public bool Exception = false;

		public bool ShowException = true;

		public string FailReason = failReason;
	}

	[JsonModel]
	public class PremiumStatusResult
	{
		public long RequiredValueUsdCents;

		public long ValueUsdCents;

		[JsonIgnore]
		public bool Exception;

		[JsonIgnore]
		public bool IsPremium
		{
			get
			{
				if (ValueUsdCents >= RequiredValueUsdCents)
				{
					return RequiredValueUsdCents != 0;
				}
				return false;
			}
		}
	}

	[JsonModel]
	private class PremiumCheckRequest
	{
		public List<ulong> SteamIds { get; set; }
	}

	[JsonModel]
	private class PremiumCheckResponse
	{
		public Dictionary<ulong, bool> Results { get; set; }
	}

	public const string KickReason = "premium_account_required";

	public static readonly Phrase KickPhrase;

	public static string PremiumStatusEndpoint;

	private static readonly HttpClient Http;

	public static async Task<PremiumCheckResult> CheckIfPlayerIsPremium(ulong steamId)
	{
		try
		{
			List<ulong> players = Pool.Get<List<ulong>>();
			players.Add(steamId);
			Dictionary<ulong, bool> obj = await CheckIfPlayersArePremium(players);
			Pool.FreeUnmanaged<ulong>(ref players);
			if (!obj.TryGetValue(steamId, out var value))
			{
				Debug.LogError((object)$"Failed to check if user {steamId} is premium due to user not being in the results");
				return new PremiumCheckResult(isPremium: false, "Failed to validate premium status: Missing User in Results");
			}
			if (!value)
			{
				return new PremiumCheckResult(isPremium: false, "premium_account_required");
			}
			return new PremiumCheckResult(isPremium: true);
		}
		catch (HttpRequestException ex)
		{
			Debug.LogError((object)$"Failed to check if user {steamId} is premium due to a network error");
			Debug.LogException((Exception)ex);
			PremiumCheckResult result = new PremiumCheckResult(isPremium: false, "Failed to validate premium status: Network Error");
			result.Exception = true;
			result.ShowException = false;
			return result;
		}
		catch (Exception ex2)
		{
			Debug.LogError((object)$"Failed to check if user {steamId} is premium due to an exception");
			Debug.LogException(ex2);
			PremiumCheckResult result = new PremiumCheckResult(isPremium: false, "Failed to validate premium status: Exception");
			result.Exception = true;
			return result;
		}
	}

	public static async Task<Dictionary<ulong, bool>> CheckIfPlayersArePremium(List<ulong> steamIds)
	{
		if (steamIds == null)
		{
			throw new ArgumentNullException("steamIds");
		}
		if (steamIds.Count == 0)
		{
			throw new ArgumentException("SteamIDs list cannot be empty", "steamIds");
		}
		string content = JsonConvert.SerializeObject((object)new PremiumCheckRequest
		{
			SteamIds = steamIds
		});
		using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, Server.premiumVerifyEndpoint);
		request.Content = new StringContent(content, Encoding.UTF8, "application/json");
		using HttpResponseMessage response = await Http.SendAsync(request);
		response.EnsureSuccessStatusCode();
		string text = await response.Content.ReadAsStringAsync();
		PremiumCheckResponse premiumCheckResponse = JsonConvert.DeserializeObject<PremiumCheckResponse>(text);
		if (premiumCheckResponse?.Results == null || premiumCheckResponse.Results.Count != steamIds.Count)
		{
			throw new Exception("Premium verify endpoint returned malformed response: " + text);
		}
		return premiumCheckResponse.Results;
	}

	static PremiumUtil()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		KickPhrase = new Phrase("premium.kick_phrase", "Your account must have premium status to play on this server.");
		PremiumStatusEndpoint = "https://rust-premium.facepunch.com/api/premium/status";
		Http = new HttpClient();
	}
}
