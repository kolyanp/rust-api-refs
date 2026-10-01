using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class EdgeSocket
{
	[FormerlySerializedAs("Socket")]
	public GameObject SocketObject;

	public Socket_Base SocketBase;
}
