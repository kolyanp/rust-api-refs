using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Facepunch;
using Facepunch.Extend;
using Facepunch.Sqlite;
using Oxide.Core;
using UnityEngine;

public class LocalClan : IClan
{
	[StructLayout(LayoutKind.Auto)]
	[CompilerGenerated]
	private struct _003CDisband_003Ed__72 : IAsyncStateMachine
	{
		public int _003C_003E1__state;

		public AsyncValueTaskMethodBuilder<ClanResult> _003C_003Et__builder;

		public LocalClan _003C_003E4__this;

		public ulong bySteamId;

		private void MoveNext()
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			int num = _003C_003E1__state;
			LocalClan localClan = _003C_003E4__this;
			ClanResult result;
			try
			{
				if (!localClan.TryGetRank(bySteamId, out var rank) || rank != 1)
				{
					result = (ClanResult)5;
				}
				else if (localClan._backend.Database.DeleteClan(localClan.ClanId))
				{
					localClan._backend.ClanDisbanded(localClan.ClanId);
					List<ClanMember>.Enumerator enumerator = localClan._members.GetEnumerator();
					try
					{
						while (enumerator.MoveNext())
						{
							ClanMember current = enumerator.Current;
							localClan._backend.MembershipChanged(current.SteamId, null);
						}
					}
					finally
					{
						if (num < 0)
						{
							((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
						}
					}
					result = (ClanResult)1;
				}
				else
				{
					result = (ClanResult)0;
				}
			}
			catch (Exception exception)
			{
				_003C_003E1__state = -2;
				_003C_003Et__builder.SetException(exception);
				return;
			}
			Interface.CallHook("OnClanDisbanded", localClan, bySteamId);
			_003C_003E1__state = -2;
			_003C_003Et__builder.SetResult(result);
		}

		void IAsyncStateMachine.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			this.MoveNext();
		}

		[DebuggerHidden]
		private void SetStateMachine(IAsyncStateMachine stateMachine)
		{
			_003C_003Et__builder.SetStateMachine(stateMachine);
		}

		void IAsyncStateMachine.SetStateMachine(IAsyncStateMachine stateMachine)
		{
			//ILSpy generated this explicit interface implementation from .override directive in SetStateMachine
			this.SetStateMachine(stateMachine);
		}
	}

	private const int MaxChatScrollback = 20;

	[CompilerGenerated]
	private Color32 _003CColor_003Ek__BackingField;

	private readonly LocalClanBackend _backend;

	private readonly List<ClanRole> _roles;

	private readonly List<ClanMember> _members;

	private readonly List<ClanInvite> _invites;

	private readonly List<ClanChatEntry> _chatHistory;

	private RealTimeSince _sinceLastRefresh;

	public long ClanId { get; }

	public string Name { get; private set; }

	public long Created { get; private set; }

	public ulong Creator { get; private set; }

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

	public long Score { get; private set; }

	public IReadOnlyList<ClanRole> Roles => _roles;

	public IReadOnlyList<ClanMember> Members => _members;

	public int MaxMemberCount => _backend.MaxMemberCount;

	public IReadOnlyList<ClanInvite> Invites => _invites;

	public LocalClan(LocalClanBackend backend, long clanId)
	{
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		base._002Ector();
		_backend = backend ?? throw new ArgumentNullException("backend");
		ClanId = clanId;
		_roles = new List<ClanRole>();
		_members = new List<ClanMember>();
		_invites = new List<ClanInvite>();
		_chatHistory = new List<ClanChatEntry>(20);
		_sinceLastRefresh = RealTimeSince.op_Implicit(0f);
	}

	public bool Refresh(ClanDataSource sources = (ClanDataSource)(-1))
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0004: Invalid comparison between Unknown and I4
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Invalid comparison between Unknown and I4
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Invalid comparison between Unknown and I4
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Invalid comparison between Unknown and I4
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Invalid comparison between Unknown and I4
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Invalid comparison between Unknown and I4
		if ((sources & 1) == 1 || (sources & 2) == 2 || (sources & 4) == 4)
		{
			ClanData? clanData = _backend.Database.ReadClan(ClanId);
			if (!clanData.HasValue)
			{
				return false;
			}
			ClanData value = clanData.Value;
			Name = value.Name;
			Created = value.Created;
			Creator = value.Creator;
			Motd = value.Motd;
			MotdTimestamp = value.MotdTimestamp;
			MotdAuthor = value.MotdAuthor;
			Logo = value.Logo;
			Color = value.Color;
			Score = value.Score;
		}
		if ((sources & 8) == 8)
		{
			List<ClanRole> list = _backend.Database.ListRoles(ClanId);
			if (list.Count == 0)
			{
				Pool.FreeUnmanaged<ClanRole>(ref list);
				return false;
			}
			_roles.Clear();
			_roles.AddRange(list);
			Pool.FreeUnmanaged<ClanRole>(ref list);
		}
		if ((sources & 0x10) == 16)
		{
			List<ClanMember> list2 = _backend.Database.ListMembers(ClanId);
			if (list2.Count == 0)
			{
				Pool.FreeUnmanaged<ClanMember>(ref list2);
				return false;
			}
			_members.Clear();
			_members.AddRange(list2);
			Pool.FreeUnmanaged<ClanMember>(ref list2);
		}
		if ((sources & 0x20) == 32)
		{
			List<ClanInvite> collection = _backend.Database.ListInvites(ClanId);
			_invites.Clear();
			_invites.AddRange(collection);
			Pool.FreeUnmanaged<ClanInvite>(ref collection);
		}
		return true;
	}

	public async ValueTask RefreshIfStale()
	{
		if (RealTimeSince.op_Implicit(_sinceLastRefresh) > 30f)
		{
			_sinceLastRefresh = RealTimeSince.op_Implicit(0f);
			Refresh((ClanDataSource)(-1));
		}
	}

	public async ValueTask<ClanValueResult<ClanLogs>> GetLogs(int limit, ulong bySteamId)
	{
		if (!CheckRole(bySteamId, delegate(ClanRole r)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return r.CanAccessLogs;
		}))
		{
			return ClanValueResult<ClanLogs>.op_Implicit((ClanResult)5);
		}
		List<ClanLogEntry> entries = _backend.Database.ReadLogs(ClanId, limit);
		return new ClanValueResult<ClanLogs>(new ClanLogs
		{
			ClanId = ClanId,
			Entries = entries
		});
	}

	public async ValueTask<ClanResult> UpdateLastSeen(ulong steamId)
	{
		return (ClanResult)(_backend.Database.UpdateMemberLastSeen(ClanId, steamId) ? 1 : 4);
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
		if (newMotd == Motd)
		{
			return (ClanResult)1;
		}
		if (_backend.Database.UpdateClanMotd(ClanId, newMotd, bySteamId))
		{
			_backend.Database.AppendLog(ClanId, "set_motd", bySteamId, newMotd);
			Changed((ClanDataSource)2);
			return (ClanResult)1;
		}
		return (ClanResult)0;
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
		if (Logo != null && Logo.Length == newLogo.Length && Enumerable.SequenceEqual(Logo, newLogo))
		{
			return (ClanResult)1;
		}
		if (_backend.Database.UpdateClanLogo(ClanId, newLogo))
		{
			_backend.Database.AppendLog(ClanId, "set_logo", bySteamId);
			Changed((ClanDataSource)4);
			Interface.CallHook("OnClanLogoChanged", this, newLogo, bySteamId);
			return (ClanResult)1;
		}
		return (ClanResult)0;
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
		if (Color.ToInt32() == newColor.ToInt32())
		{
			return (ClanResult)1;
		}
		if (_backend.Database.UpdateClanColor(ClanId, newColor))
		{
			_backend.Database.AppendLog(ClanId, "set_color", bySteamId, newColor.ToHex());
			Changed((ClanDataSource)1);
			Interface.CallHook("OnClanColorChanged", this, newColor, bySteamId);
			return (ClanResult)1;
		}
		return (ClanResult)0;
	}

	public async ValueTask<ClanResult> Invite(ulong steamId, ulong bySteamId)
	{
		if (_backend.MaxMemberCount > 0 && _members.Count >= _backend.MaxMemberCount)
		{
			return (ClanResult)21;
		}
		if (!CheckRole(bySteamId, delegate(ClanRole r)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return r.CanInvite;
		}))
		{
			return (ClanResult)5;
		}
		if (_backend.Database.CreateInvite(ClanId, steamId, bySteamId))
		{
			_backend.Database.AppendLog(ClanId, "invite", bySteamId, steamId);
			Changed((ClanDataSource)32);
			_backend.InvitationCreated(steamId, ClanId);
			return (ClanResult)1;
		}
		return (ClanResult)0;
	}

	public async ValueTask<ClanResult> CancelInvite(ulong steamId, ulong bySteamId)
	{
		if (steamId != bySteamId && !CheckRole(bySteamId, delegate(ClanRole r)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return r.CanInvite;
		}))
		{
			return (ClanResult)5;
		}
		if (_backend.Database.DeleteInvite(ClanId, steamId))
		{
			if (steamId == bySteamId)
			{
				_backend.Database.AppendLog(ClanId, "decline_invite", bySteamId);
			}
			else
			{
				_backend.Database.AppendLog(ClanId, "cancel_invite", bySteamId, steamId);
			}
			Changed((ClanDataSource)32);
			return (ClanResult)1;
		}
		return (ClanResult)0;
	}

	public async ValueTask<ClanResult> AcceptInvite(ulong steamId)
	{
		if (_backend.MaxMemberCount > 0 && _members.Count >= _backend.MaxMemberCount)
		{
			return (ClanResult)21;
		}
		try
		{
			if (_backend.Database.AcceptInvite(ClanId, steamId))
			{
				_backend.Database.AppendLog(ClanId, "accept_invite", steamId);
				Changed((ClanDataSource)48);
				_backend.MembershipChanged(steamId, ClanId);
				return (ClanResult)1;
			}
		}
		catch (SqliteException ex) when (ex.Result == 2067)
		{
			return (ClanResult)19;
		}
		return (ClanResult)0;
	}

	public async ValueTask<ClanResult> Kick(ulong steamId, ulong bySteamId)
	{
		if (!TryGetRank(steamId, out var rank))
		{
			return (ClanResult)4;
		}
		bool flag = steamId == bySteamId;
		if (!flag)
		{
			if (!CheckRole(bySteamId, delegate(ClanRole r)
			{
				//IL_0000: Unknown result type (might be due to invalid IL or missing references)
				return r.CanKick;
			}))
			{
				return (ClanResult)5;
			}
			if (!TryGetRank(bySteamId, out var rank2))
			{
				return (ClanResult)4;
			}
			if (rank <= rank2 && rank2 != 1)
			{
				return (ClanResult)5;
			}
		}
		else
		{
			if (_members.Count == 1)
			{
				return await Disband(bySteamId);
			}
			if (rank == 1 && OtherLeaderCount(steamId) == 0)
			{
				return (ClanResult)17;
			}
		}
		if (_backend.Database.DeleteMember(ClanId, steamId))
		{
			if (flag)
			{
				Interface.CallHook("OnClanMemberLeft", this, steamId);
				_backend.Database.AppendLog(ClanId, "leave", steamId);
			}
			else
			{
				Interface.CallHook("OnClanMemberKicked", this, steamId, bySteamId);
				_backend.Database.AppendLog(ClanId, "kick", bySteamId, steamId);
			}
			Changed((ClanDataSource)16);
			_backend.MembershipChanged(steamId, null);
			return (ClanResult)1;
		}
		return (ClanResult)0;
	}

	public async ValueTask<ClanResult> SetPlayerRole(ulong steamId, int newRoleId, ulong bySteamId)
	{
		ClanMember? val = List.TryFindWith<ClanMember, ulong>((IReadOnlyCollection<ClanMember>)_members, (Func<ClanMember, ulong>)delegate(ClanMember m)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return m.SteamId;
		}, steamId, (IEqualityComparer<ulong>)null);
		if (!val.HasValue)
		{
			return (ClanResult)4;
		}
		ClanRole? val2 = List.TryFindWith<ClanRole, int>((IReadOnlyCollection<ClanRole>)_roles, (Func<ClanRole, int>)delegate(ClanRole r)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return r.RoleId;
		}, val.Value.RoleId, (IEqualityComparer<int>)null);
		if (!val2.HasValue)
		{
			return (ClanResult)0;
		}
		ClanRole? val3 = List.TryFindWith<ClanRole, int>((IReadOnlyCollection<ClanRole>)_roles, (Func<ClanRole, int>)delegate(ClanRole r)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return r.RoleId;
		}, newRoleId, (IEqualityComparer<int>)null);
		if (!val3.HasValue)
		{
			return (ClanResult)4;
		}
		if (!TryGetRank(bySteamId, out var rank))
		{
			return (ClanResult)4;
		}
		if (val2.Value.Rank <= rank && rank != 1)
		{
			return (ClanResult)5;
		}
		if (val3.Value.Rank <= rank && rank != 1)
		{
			return (ClanResult)5;
		}
		if (!((val3.Value.Rank < val2.Value.Rank) ? CheckRole(bySteamId, delegate(ClanRole r)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return r.CanPromote;
		}) : CheckRole(bySteamId, delegate(ClanRole r)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return r.CanDemote;
		})))
		{
			return (ClanResult)5;
		}
		if (val.Value.RoleId == newRoleId)
		{
			return (ClanResult)1;
		}
		if (rank == 1 && steamId == bySteamId && OtherLeaderCount(steamId) == 0)
		{
			return (ClanResult)18;
		}
		if (_backend.Database.UpdateMemberRole(ClanId, steamId, newRoleId))
		{
			_backend.Database.AppendLog(ClanId, "change_role", bySteamId, steamId, val2.Value.Name, val3.Value.Name);
			Changed((ClanDataSource)16);
			return (ClanResult)1;
		}
		return (ClanResult)0;
	}

	public async ValueTask<ClanResult> SetPlayerNotes(ulong steamId, string newNotes, ulong bySteamId)
	{
		if (!CheckRole(bySteamId, delegate(ClanRole r)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return r.CanSetPlayerNotes;
		}))
		{
			return (ClanResult)5;
		}
		ClanMember? val = List.TryFindWith<ClanMember, ulong>((IReadOnlyCollection<ClanMember>)_members, (Func<ClanMember, ulong>)delegate(ClanMember m)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return m.SteamId;
		}, steamId, (IEqualityComparer<ulong>)null);
		if (!val.HasValue)
		{
			return (ClanResult)4;
		}
		if (val.Value.Notes == newNotes)
		{
			return (ClanResult)1;
		}
		if (_backend.Database.UpdateMemberNotes(ClanId, steamId, newNotes))
		{
			_backend.Database.AppendLog(ClanId, "set_notes", bySteamId, steamId, newNotes);
			Changed((ClanDataSource)16);
			return (ClanResult)1;
		}
		return (ClanResult)0;
	}

	public async ValueTask<ClanResult> CreateRole(ClanRole role, ulong bySteamId)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrWhiteSpace(role.Name))
		{
			return (ClanResult)6;
		}
		if (!TryGetRank(bySteamId, out var rank) || rank != 1)
		{
			return (ClanResult)5;
		}
		try
		{
			if (_backend.Database.CreateRole(ClanId, role).HasValue)
			{
				_backend.Database.AppendLog(ClanId, "create_role", bySteamId, role.Name);
				Changed((ClanDataSource)8);
				return (ClanResult)1;
			}
		}
		catch (SqliteException ex) when (ex.Result == 2067)
		{
			return (ClanResult)13;
		}
		return (ClanResult)0;
	}

	public async ValueTask<ClanResult> UpdateRole(ClanRole role, ulong bySteamId)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrWhiteSpace(role.Name))
		{
			return (ClanResult)6;
		}
		if (!TryGetRank(bySteamId, out var rank) || rank != 1)
		{
			return (ClanResult)5;
		}
		ClanRole? val = List.TryFindWith<ClanRole, int>((IReadOnlyCollection<ClanRole>)_roles, (Func<ClanRole, int>)delegate(ClanRole r)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return r.RoleId;
		}, role.RoleId, (IEqualityComparer<int>)null);
		if (!val.HasValue)
		{
			return (ClanResult)4;
		}
		try
		{
			if ((val.Value.Rank == 1) ? _backend.Database.UpdateRoleName(ClanId, role.RoleId, role.Name) : _backend.Database.UpdateRole(ClanId, role))
			{
				if (role.Name != val.Value.Name)
				{
					_backend.Database.AppendLog(ClanId, "update_role_renamed", bySteamId, val.Value.Name, role.Name);
				}
				else
				{
					_backend.Database.AppendLog(ClanId, "update_role", bySteamId, role.Name);
				}
				Changed((ClanDataSource)8);
				return (ClanResult)1;
			}
		}
		catch (SqliteException ex) when (ex.Result == 2067)
		{
			return (ClanResult)13;
		}
		return (ClanResult)0;
	}

	public async ValueTask<ClanResult> SwapRoleRanks(int roleIdA, int roleIdB, ulong bySteamId)
	{
		if (!TryGetRank(bySteamId, out var rank) || rank != 1)
		{
			return (ClanResult)5;
		}
		ClanRole? val = List.TryFindWith<ClanRole, int>((IReadOnlyCollection<ClanRole>)_roles, (Func<ClanRole, int>)delegate(ClanRole r)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return r.RoleId;
		}, roleIdA, (IEqualityComparer<int>)null);
		if (!val.HasValue)
		{
			return (ClanResult)4;
		}
		ClanRole? val2 = List.TryFindWith<ClanRole, int>((IReadOnlyCollection<ClanRole>)_roles, (Func<ClanRole, int>)delegate(ClanRole r)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return r.RoleId;
		}, roleIdB, (IEqualityComparer<int>)null);
		if (!val2.HasValue)
		{
			return (ClanResult)4;
		}
		if (val.Value.Rank == 1 || val2.Value.Rank == 1)
		{
			return (ClanResult)15;
		}
		if (_backend.Database.SwapRoleRanks(ClanId, roleIdA, roleIdB))
		{
			_backend.Database.AppendLog(ClanId, "swap_roles", bySteamId, val.Value.Name, val2.Value.Name);
			Changed((ClanDataSource)8);
			return (ClanResult)1;
		}
		return (ClanResult)0;
	}

	public async ValueTask<ClanResult> DeleteRole(int roleId, ulong bySteamId)
	{
		if (!TryGetRank(bySteamId, out var rank) || rank != 1)
		{
			return (ClanResult)5;
		}
		ClanRole? val = List.TryFindWith<ClanRole, int>((IReadOnlyCollection<ClanRole>)_roles, (Func<ClanRole, int>)delegate(ClanRole r)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return r.RoleId;
		}, roleId, (IEqualityComparer<int>)null);
		if (!val.HasValue)
		{
			return (ClanResult)4;
		}
		if (val.Value.Rank == 1)
		{
			return (ClanResult)16;
		}
		bool flag;
		try
		{
			flag = _backend.Database.DeleteRole(ClanId, roleId);
		}
		catch (SqliteException ex) when (ex.Result == 787 || ex.Result == 1811)
		{
			return (ClanResult)14;
		}
		if (flag)
		{
			_backend.Database.AppendLog(ClanId, "delete_role", bySteamId, val.Value.Name);
			Changed((ClanDataSource)8);
			return (ClanResult)1;
		}
		return (ClanResult)0;
	}

	[AsyncStateMachine(typeof(_003CDisband_003Ed__72))]
	public ValueTask<ClanResult> Disband(ulong bySteamId)
	{
		_003CDisband_003Ed__72 stateMachine = default(_003CDisband_003Ed__72);
		stateMachine._003C_003Et__builder = AsyncValueTaskMethodBuilder<ClanResult>.Create();
		stateMachine._003C_003E4__this = this;
		stateMachine.bySteamId = bySteamId;
		stateMachine._003C_003E1__state = -1;
		stateMachine._003C_003Et__builder.Start(ref stateMachine);
		return stateMachine._003C_003Et__builder.Task;
	}

	public async ValueTask<ClanValueResult<ClanScoreEvents>> GetScoreEvents(int limit, ulong bySteamId)
	{
		if (!CheckRole(bySteamId, delegate(ClanRole r)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return r.CanAccessScoreEvents;
		}))
		{
			return ClanValueResult<ClanScoreEvents>.op_Implicit((ClanResult)5);
		}
		List<ClanScoreEvent> scoreEvents = _backend.Database.ReadScoreEvents(ClanId, limit);
		return new ClanValueResult<ClanScoreEvents>(new ClanScoreEvents
		{
			ClanId = ClanId,
			ScoreEvents = scoreEvents
		});
	}

	public async ValueTask<ClanResult> AddScoreEvent(ClanScoreEvent scoreEvent)
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (scoreEvent.Score == 0)
		{
			throw new ArgumentException("Score cannot be zero.", "scoreEvent");
		}
		if (scoreEvent.Multiplier == 0)
		{
			throw new ArgumentException("Multiplier cannot be zero.", "scoreEvent");
		}
		_backend.Database.AppendScoreEvent(ClanId, scoreEvent);
		Changed((ClanDataSource)64);
		return (ClanResult)1;
	}

	public async ValueTask<ClanValueResult<ClanChatScrollback>> GetChatScrollback()
	{
		return new ClanValueResult<ClanChatScrollback>(new ClanChatScrollback
		{
			ClanId = ClanId,
			Entries = _chatHistory.ToList()
		});
	}

	public async ValueTask<ClanResult> SendChatMessage(string name, string message, ulong bySteamId)
	{
		if (!TryGetRank(bySteamId, out var _))
		{
			return (ClanResult)0;
		}
		ClanValidatorResult val = ClanValidator.ValidateChatMessage(message);
		if (!((ClanValidatorResult)(ref val)).Success)
		{
			return ClanValidator.ToClanResult(((ClanValidatorResult)(ref val)).Error);
		}
		ClanChatEntry val2 = new ClanChatEntry
		{
			SteamId = bySteamId,
			Name = name,
			Message = ((ClanValidatorResult)(ref val)).Value,
			Time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
		};
		if (_chatHistory.Count >= 20)
		{
			_chatHistory.RemoveAt(0);
		}
		_chatHistory.Add(val2);
		_backend.ClanChatMessage(ClanId, val2);
		return (ClanResult)1;
	}

	private int OtherLeaderCount(ulong excludeSteamId)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		foreach (ClanMember member in _members)
		{
			if (member.SteamId != excludeSteamId && TryGetRank(member.SteamId, out var rank) && rank == 1)
			{
				num++;
			}
		}
		return num;
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

	private bool TryGetRank(ulong steamId, out int rank)
	{
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		ClanMember? val = List.TryFindWith<ClanMember, ulong>((IReadOnlyCollection<ClanMember>)_members, (Func<ClanMember, ulong>)delegate(ClanMember m)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return m.SteamId;
		}, steamId, (IEqualityComparer<ulong>)null);
		if (!val.HasValue)
		{
			rank = int.MaxValue;
			return false;
		}
		ClanRole? val2 = List.TryFindWith<ClanRole, int>((IReadOnlyCollection<ClanRole>)_roles, (Func<ClanRole, int>)delegate(ClanRole r)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			return r.RoleId;
		}, val.Value.RoleId, (IEqualityComparer<int>)null);
		if (!val2.HasValue)
		{
			rank = int.MaxValue;
			return false;
		}
		rank = val2.Value.Rank;
		return true;
	}

	private void Changed(ClanDataSource dataSources)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		_backend.ClanChanged(ClanId, dataSources);
		Refresh(dataSources);
	}
}
