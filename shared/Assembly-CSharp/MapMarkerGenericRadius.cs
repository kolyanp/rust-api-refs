using Network;
using ProtoBuf;
using UnityEngine;

public class MapMarkerGenericRadius : MapMarker
{
	public float radius;

	public Color color1;

	public Color color2;

	public float alpha;

	public override bool OnRpcMessage(BasePlayer player, uint rpc, Message msg)
	{
		using (TimeWarning.New("MapMarkerGenericRadius.OnRpcMessage"))
		{
		}
		return base.OnRpcMessage(player, rpc, msg);
	}

	public void SendUpdate(bool fullUpdate = true)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		float a = color1.a;
		ClientRPC(arg1: new Vector3(color1.r, color1.g, color1.b), arg3: new Vector3(color2.r, color2.g, color2.b), target: RpcTarget.NetworkGroup("MarkerUpdate"), arg2: a, arg4: alpha, arg5: radius);
	}

	public override AppMarker GetAppMarkerData()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		AppMarker appMarkerData = base.GetAppMarkerData();
		appMarkerData.radius = radius;
		appMarkerData.color1 = Color.op_Implicit(color1);
		appMarkerData.color2 = Color.op_Implicit(color2);
		appMarkerData.alpha = alpha;
		return appMarkerData;
	}
}
