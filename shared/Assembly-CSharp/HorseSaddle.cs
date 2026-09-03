using UnityEngine;

public class HorseSaddle : BaseVehicleSeat
{
	[SerializeField]
	private bool isDriver;

	[SerializeField]
	private Transform eyePosRef;

	private RidableHorse _owner;

	protected RidableHorse Owner
	{
		get
		{
			if ((Object)(object)_owner == (Object)null)
			{
				_owner = ((Component)this).GetComponentInParent<RidableHorse>();
			}
			return _owner;
		}
	}

	public override void ResetState()
	{
		base.ResetState();
	}

	public override void VehicleFixedUpdate()
	{
	}

	public override void OnPlayerMounted()
	{
		base.OnPlayerMounted();
		BasePlayer mounted = GetMounted();
		if ((Object)(object)mounted != (Object)null)
		{
			BaseVehicle baseVehicle = VehicleParent();
			if ((Object)(object)baseVehicle != (Object)null)
			{
				baseVehicle.PlayerMounted(mounted, this);
			}
		}
	}
}
