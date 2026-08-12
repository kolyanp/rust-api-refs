using UnityEngine;

public class ChippyMainCharacter : SpriteArcadeEntity
{
	public float speed;

	public float maxSpeed;

	public ChippyBulletEntity bulletPrefab;

	public float fireRate;

	public Vector3 aimDir;

	public ChippyMainCharacter()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		maxSpeed = 0.25f;
		fireRate = 0.1f;
		aimDir = Vector3.up;
		base._002Ector();
	}
}
