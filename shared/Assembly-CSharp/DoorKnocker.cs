using Network;
using Oxide.Core;
using UnityEngine;

public class DoorKnocker : BaseCombatEntity
{
	public Animator knocker1;

	public Animator knocker2;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("DoorKnocker.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public void Knock(BasePlayer player)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		ClientRPC(RpcTarget.NetworkGroup("ClientKnock"), ((Component)player).transform.position);
		Interface.CallHook("OnDoorKnocked", this, player);
	}
}
