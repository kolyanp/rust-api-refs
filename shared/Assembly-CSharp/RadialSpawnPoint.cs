using UnityEngine;

public class RadialSpawnPoint : BaseSpawnPoint
{
	[SerializeField]
	[Tooltip("Circle to spawn within")]
	[Header("Position Settings")]
	public float radius = 10f;

	[SerializeField]
	private bool randomYAxisOffsetEnabled;

	[SerializeField]
	private float yAxisOffsetMin;

	[SerializeField]
	private float yAxisOffsetMax;

	[Header("Random Rotation Settings")]
	[SerializeField]
	private bool xRotationEnabled;

	[SerializeField]
	[Range(-180f, 180f)]
	private float xRotationMin = -180f;

	[Range(-180f, 180f)]
	[SerializeField]
	private float xRotationMax = 180f;

	[SerializeField]
	private bool yRotationEnabled = true;

	[SerializeField]
	[Range(-180f, 180f)]
	private float yRotationMin = -180f;

	[SerializeField]
	[Range(-180f, 180f)]
	private float yRotationMax = 180f;

	[SerializeField]
	private bool zRotationEnabled;

	[SerializeField]
	[Range(-180f, 180f)]
	private float zRotationMin = -180f;

	[SerializeField]
	[Range(-180f, 180f)]
	private float zRotationMax = 180f;

	public Quaternion GetRandomRotation()
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		return Quaternion.Euler(xRotationEnabled ? Random.Range(xRotationMin, xRotationMax) : 0f, yRotationEnabled ? Random.Range(yRotationMin, yRotationMax) : 0f, zRotationEnabled ? Random.Range(zRotationMin, zRotationMax) : 0f);
	}

	public override void GetLocation(out Vector3 pos, out Quaternion rot)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		Vector2 val = Random.insideUnitCircle * radius;
		pos = ((Component)this).transform.position + new Vector3(val.x, 0f, val.y);
		rot = GetRandomRotation();
		DropToGround(ref pos, ref rot);
		if (randomYAxisOffsetEnabled)
		{
			pos.y += Random.Range(yAxisOffsetMin, yAxisOffsetMax);
		}
	}

	public override bool HasPlayersIntersecting()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return BaseNetworkable.HasCloseConnections(((Component)this).transform.position, radius + playerCheckMargin);
	}

	public override void ObjectSpawned(SpawnPointInstance instance)
	{
	}

	public override void ObjectRetired(SpawnPointInstance instance)
	{
	}
}
