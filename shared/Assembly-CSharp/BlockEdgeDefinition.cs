using System;
using UnityEngine;

public class BlockEdgeDefinition : PrefabAttribute
{
	public EdgeSocket[] Sockets;

	protected override Type GetIndexedType()
	{
		return typeof(BlockEdgeDefinition);
	}

	public override void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		base.PreProcess(preProcess, rootObj, name, serverside, clientside, bundling);
		if (Sockets == null)
		{
			return;
		}
		EdgeSocket[] sockets = Sockets;
		foreach (EdgeSocket edgeSocket in sockets)
		{
			if (!((Object)(object)edgeSocket.SocketObject == (Object)null))
			{
				edgeSocket.SocketBase = edgeSocket.SocketObject.GetComponent<Socket_Base>();
			}
		}
	}
}
