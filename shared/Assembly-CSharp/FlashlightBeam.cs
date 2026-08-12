using UnityEngine;

public class FlashlightBeam : MonoBehaviour, IClientComponent
{
	public Vector2 scrollDir;

	public Vector3 localEndPoint;

	public LineRenderer beamRenderer;

	public FlashlightBeam()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		localEndPoint = new Vector3(0f, 0f, 2f);
		((MonoBehaviour)this)._002Ector();
	}
}
