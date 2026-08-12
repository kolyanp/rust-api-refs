using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Facepunch.Extend;
using Facepunch.Nexus;
using Facepunch.Nexus.Models;
using UnityEngine;

public class NexusClanWrapper : IClan
{
	private const int MaxChatScrollback = 20;

	[CompilerGenerated]
	private Color32 _003CColor_003Ek__BackingField;

	public readonly NexusClan Internal;

	private readonly NexusClanChatCollector _chatCollector;

	private readonly List<ClanRole> _roles;

	private readonly List<ClanMember> _members;

	private readonly List<ClanInvite> _invites;

	private readonly List<ClanChatEntry> _chatHistory;

	private RealTimeSince _sinceLastRefresh;

	public long ClanId => Internal.ClanId;

	public string Name => Internal.Name;

	public long Created => Internal.Created;

	public ulong Creator => NexusClanUtil.GetSteamId(Internal.Creator);

	public string Motd { get; private set; }

	public long MotdTimestamp { get; private set; }

	public ulong MotdAuthor { get; private set; }

	public byte[] Logo { get; private set; }

	public Color32 Color
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003CColor_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003CColor_003Ek__BackingField = value;
		}
	}

	public long Score => Internal.Score;

	public IReadOnlyList<ClanRole> Roles => _roles;

	public IReadOnlyList<ClanMember> Members => _members;

	public int MaxMemberCount { get; private set; }

	public IReadOnlyList<ClanInvite> Invites => _invites;

	public NexusClanWrapper(NexusClan clan, NexusClanChatCollector chatCollector)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		base._002Ector();
		Internal = clan ?? throw new ArgumentNullException("clan");
		_chatCollector = chatCollector ?? throw new ArgumentNullException("chatCollector");
		_roles = new List<ClanRole>();
		_members = new List<ClanMember>();
		_invites = new List<ClanInvite>();
		_chatHistory = new List<ClanChatEntry>(20);
		_sinceLastRefresh = RealTimeSince.op_Implicit(0f);
		UpdateValuesInternal();
	}

	public void UpdateValuesInternal()
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		NexusClanUtil.GetMotd(Internal, out var motd, out var motdTimestamp, out var motdAuthor);
		Motd = motd;
		MotdTimestamp = motdTimestamp;
		MotdAuthor = motdAuthor;
		NexusClanUtil.GetBanner(Internal, out var logo, out var color);
		Logo = logo;
		Color = color;
		List.Resize<ClanRole>(_roles, Internal.Roles.Count);
		for (int i = 0; i < _roles.Count; i++)
		{
			_roles[i] = NexusClanUtil.ToClanRole(Internal.Roles[i]);
		}
		List.Resize<ClanMember>(_members, Internal.Members.Count);
		for (int j = 0; j < _members.Count; j++)
		{
			_members[j] = NexusClanUtil.ToClanMember(Internal.Members[j]);
		}
		MaxMemberCount = Internal.MaxMemberCount;
		List.Resize<ClanInvite>(_invites, Internal.Invites.Count);
		for (int k = 0; k < _invites.Count; k++)
		{
			_invites[k] = NexusClanUtil.ToClanInvite(Internal.Invites[k]);
		}
	}

	public async ValueTask RefreshIfStale()
	{
		if (RealTimeSince.op_Implicit(_sinceLastRefresh) > 30f)
		{
			_sinceLastRefresh = RealTimeSince.op_Implicit(0f);
			try
			{
				await Internal.Refresh();
				UpdateValuesInternal();
			}
			catch (Exception ex)
			{
				Debug.LogError((object)$"Failed to refresh nexus clan ID {ClanId}");
				Debug.LogException(ex);
			}
		}
	}

	public async ValueTask<ClanValueResult<ClanLogs>> GetLogs(int limit, ulong bySteamId)
	{
		NexusClanResult<List<ClanLogEntry>> val = await Internal.GetLogs(NexusClanUtil.GetPlayerId(bySteamId), limit);
		List<ClanLogEntry> source = default(List<ClanLogEntry>);
		if (val.IsSuccess && val.TryGetResponse(ref source))
		{
			return ClanValueResult<ClanLogs>.op_Implicit(new ClanLogs
			{
				ClanId = ClanId,
				Entries = ((IEnumerable<ClanLogEntry>)source).Select((Func<ClanLogEntry, ClanLogEntry>)delegate(ClanLogEntry e)
				{
					//IL_0002: Unknown result type (might be due to invalid IL or missing references)
					//IL_0063: Unknown result type (might be due to invalid IL or missing references)
					return new ClanLogEntry
					{
						Timestamp = ((ClanLogEntry)(ref e)).Timestamp * 1000,
						EventKey = ((ClanLogEntry)(ref e)).EventKey,
						Arg1 = ((ClanLogEntry)(ref e)).Arg1,
						Arg2 = ((ClanLogEntry)(ref e)).Arg2,
						Arg3 = ((ClanLogEntry)(ref e)).Arg3,
						Arg4 = ((ClanLogEntry)(ref e)).Arg4
					};
				}).ToList()
			});
		}
		return ClanValueResult<ClanLogs>.op_Implicit(NexusClanUtil.ToClanResult(val.ResultCode));
	}

	public async ValueTask<ClanResult> UpdateLastSeen(ulong steamId)
	{
		return NexusClanUtil.ToClanResult(await Internal.UpdateLastSeen(NexusClanUtil.GetPlayerId(steamId)));
	}

	public async ValueTask<ClanResult> SetMotd(string newMotd, ulong bySteamId)
	{
		if (!CheckRole(bySteamId, delegate(ClanRole r)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return r.CanSetMotd;
		}))
		{
			return (ClanResult)5;
		}
		string playerId = NexusClanUtil.GetPlayerId(bySteamId);
		NexusClan obj = Internal;
		ClanVariablesUpdate val = default(ClanVariablesUpdate);
		((ClanVariablesUpdate)(ref val)).Variables = new List<VariableUpdate>(2)
		{
			new VariableUpdate("motd", newMotd, (bool?)null, (bool?)null),
			new VariableUpdate("motd_author", playerId, (bool?)null, (bool?)null)
		};
		((ClanVariablesUpdate)(ref val)).EventKey = "set_motd";
		((ClanVariablesUpdate)(ref val)).Arg1 = playerId;
		((ClanVariablesUpdate)(ref val)).Arg2 = newMotd;
		return NexusClanUtil.ToClanResult(await obj.UpdateVariables(val));
	}

	public async ValueTask<ClanResult> SetLogo(byte[] newLogo, ulong bySteamId)
	{
		if (!CheckRole(bySteamId, delegate(ClanRole r)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return r.CanSetLogo;
		}))
		{
			return (ClanResult)5;
		}
		string playerId = NexusClanUtil.GetPlayerId(bySteamId);
		NexusClan obj = Internal;
		ClanVariablesUpdate val = default(ClanVariablesUpdate);
		((ClanVariablesUpdate)(ref val)).Variables = new List<VariableUpdate>(1)
		{
			new VariableUpdate("logo", (Memory<byte>)newLogo, (bool?)null, (bool?)null)
		};
		((ClanVariablesUpdate)(ref val)).EventKey = "set_logo";
		((ClanVariablesUpdate)(ref val)).Arg1 = playerId;
		return NexusClanUtil.ToClanResult(await obj.UpdateVariables(val));
	}

	public async ValueTask<ClanResult> SetColor(Color32 newColor, ulong bySteamId)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (!CheckRole(bySteamId, delegate(ClanRole r)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return r.CanSetLogo;
		}))
		{
			return (ClanResult)5;
		}
		string playerId = NexusClanUtil.GetPlayerId(bySteamId);
		NexusClan obj = Internal;
		ClanVariablesUpdate val = default(ClanVariablesUpdate);
		((ClanVariablesUpdate)(ref val)).Variables = new List<VariableUpdate>(1)
		{
			new VariableUpdate("color", newColor.ToInt32().ToString("G"), (bool?)null, (bool?)null)
		};
		((ClanVariablesUpdate)(ref val)).EventKey = "set_color";
		((ClanVariablesUpdate)(ref val)).Arg1 = playerId;
		((ClanVariablesUpdate)(ref val)).Arg2 = newColor.ToHex();
		return NexusClanUtil.ToClanResult(await obj.UpdateVariables(val));
	}

	public async ValueTask<ClanResult> Invite(ulong steamId, ulong bySteamId)
	{
		return NexusClanUtil.ToClanResult(await Internal.Invite(NexusClanUtil.GetPlayerId(steamId), NexusClanUtil.GetPlayerId(bySteamId)));
	}

	public async ValueTask<ClanResult> CancelInvite(ulong steamId, ulong bySteamId)
	{
		return NexusClanUtil.ToClanResult(await Internal.CancelInvite(NexusClanUtil.GetPlayerId(steamId), NexusClanUtil.GetPlayerId(bySteamId)));
	}

	public async ValueTask<ClanResult> AcceptInvite(ulong steamId)
	{
		return NexusClanUtil.ToClanResult(await Internal.AcceptInvite(NexusClanUtil.GetPlayerId(steamId)));
	}

	public async ValueTask<ClanResult> Kick(ulong steamId, ulong bySteamId)
	{
		return NexusClanUtil.ToClanResult(await Internal.Kick(NexusClanUtil.GetPlayerId(steamId), NexusClanUtil.GetPlayerId(bySteamId)));
	}

	public async ValueTask<ClanResult> SetPlayerRole(ulong steamId, int newRoleId, ulong bySteamId)
	{
		return NexusClanUtil.ToClanResult(await Internal.SetPlayerRole(NexusClanUtil.GetPlayerId(steamId), newRoleId, NexusClanUtil.GetPlayerId(bySteamId)));
	}

	public async ValueTask<ClanResult> SetPlayerNotes(ulong steamId, string notes, ulong bySteamId)
	{
		if (!CheckRole(bySteamId, delegate(ClanRole r)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return r.CanSetPlayerNotes;
		}))
		{
			return (ClanResult)5;
		}
		string playerId = NexusClanUtil.GetPlayerId(steamId);
		string playerId2 = NexusClanUtil.GetPlayerId(bySteamId);
		NexusClan obj = Internal;
		ClanVariablesUpdate val = default(ClanVariablesUpdate);
		((ClanVariablesUpdate)(ref val)).Variables = new List<VariableUpdate>(1)
		{
			new VariableUpdate("notes", notes, (bool?)null, (bool?)null)
		};
		((ClanVariablesUpdate)(ref val)).EventKey = "set_notes";
		((ClanVariablesUpdate)(ref val)).Arg1 = playerId2;
		((ClanVariablesUpdate)(ref val)).Arg2 = playerId;
		((ClanVariablesUpdate)(ref val)).Arg3 = notes;
		return NexusClanUtil.ToClanResult(await obj.UpdatePlayerVariables(playerId, val));
	}

	public async ValueTask<ClanResult> CreateRole(ClanRole role, ulong bySteamId)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		return NexusClanUtil.ToClanResult(await Internal.CreateRole(NexusClanUtil.ToRoleParameters(role), NexusClanUtil.GetPlayerId(bySteamId)));
	}

	public async ValueTask<ClanResult> UpdateRole(ClanRole role, ulong bySteamId)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		return NexusClanUtil.ToClanResult(await Internal.UpdateRole(role.RoleId, NexusClanUtil.ToRoleParameters(role), NexusClanUtil.GetPlayerId(bySteamId)));
	}

	public async ValueTask<ClanResult> SwapRoleRanks(int roleIdA, int roleIdB, ulong bySteamId)
	{
		return NexusClanUtil.ToClanResult(await Internal.SwapRoleRanks(roleIdA, roleIdB, NexusClanUtil.GetPlayerId(bySteamId)));
	}

	public async ValueTask<ClanResult> DeleteRole(int roleId, ulong bySteamId)
	{
		return NexusClanUtil.ToClanResult(await Internal.DeleteRole(roleId, NexusClanUtil.GetPlayerId(bySteamId)));
	}

	public async ValueTask<ClanResult> Disband(ulong bySteamId)
	{
		return NexusClanUtil.ToClanResult(await Internal.Disband(NexusClanUtil.GetPlayerId(bySteamId)));
	}

	public async ValueTask<ClanValueResult<ClanScoreEvents>> GetScoreEvents(int limit, ulong bySteamId)
	{
		NexusClanResult<List<ClanScoreEventEntry>> val = await Internal.GetScoreEvents(NexusClanUtil.GetPlayerId(bySteamId), limit);
		List<ClanScoreEventEntry> source = default(List<ClanScoreEventEntry>);
		if (val.IsSuccess && val.TryGetResponse(ref source))
		{
			return ClanValueResult<ClanScoreEvents>.op_Implicit(new ClanScoreEvents
			{
				ClanId = ClanId,
				ScoreEvents = ((IEnumerable<ClanScoreEventEntry>)source).Select((Func<ClanScoreEventEntry, ClanScoreEvent>)delegate(ClanScoreEventEntry e)
				{
					//IL_0002: Unknown result type (might be due to invalid IL or missing references)
					//IL_0026: Unknown result type (might be due to invalid IL or missing references)
					//IL_0097: Unknown result type (might be due to invalid IL or missing references)
					return new ClanScoreEvent
					{
						Timestamp = ((ClanScoreEventEntry)(ref e)).Timestamp * 1000,
						Type = (ClanScoreEventType)((ClanScoreEventEntry)(ref e)).Type,
						Score = ((ClanScoreEventEntry)(ref e)).Score,
						Multiplier = ((ClanScoreEventEntry)(ref e)).Multiplier,
						SteamId = NexusClanUtil.TryGetSteamId(((ClanScoreEventEntry)(ref e)).PlayerId),
						OtherSteamId = NexusClanUtil.TryGetSteamId(((ClanScoreEventEntry)(ref e)).OtherPlayerId),
						OtherClanId = ((ClanScoreEventEntry)(ref e)).OtherClanId,
						Arg1 = ((ClanScoreEventEntry)(ref e)).Arg1,
						Arg2 = ((ClanScoreEventEntry)(ref e)).Arg2
					};
				}).ToList()
			});
		}
		return ClanValueResult<ClanScoreEvents>.op_Implicit(NexusClanUtil.ToClanResult(val.ResultCode));
	}

	public ValueTask<ClanResult> AddScoreEvent(ClanScoreEvent scoreEvent)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Expected I4, but got Unknown
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		NexusClan obj = Internal;
		NewClanScoreEventEntry val = default(NewClanScoreEventEntry);
		((NewClanScoreEventEntry)(ref val)).Type = (int)scoreEvent.Type;
		((NewClanScoreEventEntry)(ref val)).Score = scoreEvent.Score;
		((NewClanScoreEventEntry)(ref val)).Multiplier = scoreEvent.Multiplier;
		((NewClanScoreEventEntry)(ref val)).PlayerId = NexusClanUtil.GetPlayerId(scoreEvent.SteamId);
		((NewClanScoreEventEntry)(ref val)).OtherPlayerId = NexusClanUtil.GetPlayerId(scoreEvent.OtherSteamId);
		((NewClanScoreEventEntry)(ref val)).OtherClanId = scoreEvent.OtherClanId;
		((NewClanScoreEventEntry)(ref val)).Arg1 = scoreEvent.Arg1;
		((NewClanScoreEventEntry)(ref val)).Arg2 = scoreEvent.Arg2;
		obj.AddScoreEvent(val);
		return new ValueTask<ClanResult>((ClanResult)1);
	}

	public ValueTask<ClanValueResult<ClanChatScrollback>> GetChatScrollback()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		lock (_chatHistory)
		{
			return new ValueTask<ClanValueResult<ClanChatScrollback>>(ClanValueResult<ClanChatScrollback>.op_Implicit(new ClanChatScrollback
			{
				ClanId = ClanId,
				Entries = _chatHistory.ToList()
			}));
		}
	}

	public ValueTask<ClanResult> SendChatMessage(string name, string message, ulong bySteamId)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		if (!List.TryFindWith<ClanMember, ulong>((IReadOnlyCollection<ClanMember>)_members, (Func<ClanMember, ulong>)delegate(ClanMember m)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return m.SteamId;
		}, bySteamId, (IEqualityComparer<ulong>)null).HasValue)
		{
			return new ValueTask<ClanResult>((ClanResult)0);
		}
		ClanValidatorResult val = ClanValidator.ValidateChatMessage(message);
		if (!((ClanValidatorResult)(ref val)).Success)
		{
			return new ValueTask<ClanResult>(ClanValidator.ToClanResult(((ClanValidatorResult)(ref val)).Error));
		}
		ClanChatEntry entry = new ClanChatEntry
		{
			SteamId = bySteamId,
			Name = name,
			Message = ((ClanValidatorResult)(ref val)).Value,
			Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
		};
		AddScrollback(in entry);
		_chatCollector.OnClanChatMessage(ClanId, entry);
		return new ValueTask<ClanResult>((ClanResult)1);
	}

	public void AddScrollback(in ClanChatEntry entry)
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		lock (_chatHistory)
		{
			if (_chatHistory.Count >= 20)
			{
				_chatHistory.RemoveAt(0);
			}
			_chatHistory.Add(entry);
		}
	}

	private bool CheckRole(ulong steamId, Func<ClanRole, bool> roleTest)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		ClanMember? val = List.TryFindWith<ClanMember, ulong>((IReadOnlyCollection<ClanMember>)_members, (Func<ClanMember, ulong>)delegate(ClanMember m)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return m.SteamId;
		}, steamId, (IEqualityComparer<ulong>)null);
		if (!val.HasValue)
		{
			return false;
		}
		ClanRole? val2 = List.TryFindWith<ClanRole, int>((IReadOnlyCollection<ClanRole>)_roles, (Func<ClanRole, int>)delegate(ClanRole r)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return r.RoleId;
		}, val.Value.RoleId, (IEqualityComparer<int>)null);
		if (!val2.HasValue)
		{
			return false;
		}
		if (val2.Value.Rank != 1)
		{
			return roleTest(val2.Value);
		}
		return true;
	}
}
