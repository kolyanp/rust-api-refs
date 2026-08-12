using System.Collections.Generic;
using UnityEngine;

public class ClientPressureGauge : FacepunchBehaviour, IClientComponent
{
	public List<Transform> pressureGaugeHandles;

	public Vector2 pressureGaugeRotationRange;

	public ClientPressureGauge()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		pressureGaugeRotationRange = new Vector2(-135f, 135f);
		base._002Ector();
	}
}
