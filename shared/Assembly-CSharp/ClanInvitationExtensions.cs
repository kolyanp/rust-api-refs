using System.Collections.Generic;
using Development.Attributes;
using Facepunch;
using ProtoBuf;

public static class ClanInvitationExtensions
{
	[PoolAnalyzerGetWrapper]
	public static ClanInvitations ToProto(this List<ClanInvitation> invitations)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		List<Invitation> list = Pool.Get<List<Invitation>>();
		foreach (ClanInvitation invitation in invitations)
		{
			list.Add(ToProto(invitation));
		}
		ClanInvitations val = Pool.Get<ClanInvitations>();
		val.invitations = list;
		return val;
	}

	[PoolAnalyzerGetWrapper]
	public static Invitation ToProto(this ClanInvitation invitation)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		Invitation val = Pool.Get<Invitation>();
		val.clanId = invitation.ClanId;
		val.recruiter = invitation.Recruiter;
		val.timestamp = invitation.Timestamp;
		return val;
	}
}
