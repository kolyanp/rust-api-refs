using System;
using Facepunch.Nexus;
using Facepunch.Nexus.Models;

public class NexusClanEventHandler : INexusClanEventListener
{
	private readonly NexusClanBackend _backend;

	private readonly IClanChangeSink _changeSink;

	public NexusClanEventHandler(NexusClanBackend backend, IClanChangeSink changeSink)
	{
		_backend = backend ?? throw new ArgumentNullException("backend");
		_changeSink = changeSink ?? throw new ArgumentNullException("changeSink");
	}

	public void OnDisbanded(in ClanDisbandedEvent args)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		IClanChangeSink changeSink = _changeSink;
		ClanDisbandedEvent val = args;
		changeSink.ClanDisbanded(((ClanDisbandedEvent)(ref val)).ClanId);
		val = args;
		foreach (ulong member in ((ClanDisbandedEvent)(ref val)).Members)
		{
			_changeSink.MembershipChanged(member, (long?)null);
		}
	}

	public void OnInvitation(in ClanInvitedEvent args)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		IClanChangeSink changeSink = _changeSink;
		ClanInvitedEvent val = args;
		ulong playerId = ((ClanInvitedEvent)(ref val)).PlayerId;
		val = args;
		changeSink.InvitationCreated(playerId, ((ClanInvitedEvent)(ref val)).ClanId);
	}

	public void OnJoined(in ClanJoinedEvent args)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		IClanChangeSink changeSink = _changeSink;
		ClanJoinedEvent val = args;
		ulong playerId = ((ClanJoinedEvent)(ref val)).PlayerId;
		val = args;
		changeSink.MembershipChanged(playerId, (long?)((ClanJoinedEvent)(ref val)).ClanId);
	}

	public void OnKicked(in ClanKickedEvent args)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		IClanChangeSink changeSink = _changeSink;
		ClanKickedEvent val = args;
		changeSink.MembershipChanged(((ClanKickedEvent)(ref val)).PlayerId, (long?)null);
	}

	public void OnChanged(in ClanChangedEvent args)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		NexusClanBackend backend = _backend;
		ClanChangedEvent val = args;
		backend.UpdateWrapper(((ClanChangedEvent)(ref val)).ClanId);
		IClanChangeSink changeSink = _changeSink;
		val = args;
		changeSink.ClanChanged(((ClanChangedEvent)(ref val)).ClanId, (ClanDataSource)(-1));
	}

	public void OnUnload(in long clanId)
	{
		_backend.RemoveWrapper(clanId);
	}

	void INexusClanEventListener.OnDisbanded(in ClanDisbandedEvent args)
	{
		OnDisbanded(in args);
	}

	void INexusClanEventListener.OnInvitation(in ClanInvitedEvent args)
	{
		OnInvitation(in args);
	}

	void INexusClanEventListener.OnJoined(in ClanJoinedEvent args)
	{
		OnJoined(in args);
	}

	void INexusClanEventListener.OnKicked(in ClanKickedEvent args)
	{
		OnKicked(in args);
	}

	void INexusClanEventListener.OnChanged(in ClanChangedEvent args)
	{
		OnChanged(in args);
	}

	void INexusClanEventListener.OnUnload(in long clanId)
	{
		OnUnload(in clanId);
	}
}
