using Unity.Mathematics;
using UnityEngine;

public struct CritterLegSpeed
{
	private static readonly int materialMoveToggleId = Shader.PropertyToID("_MoveToggle");

	private static readonly int moveSpeedId = Shader.PropertyToID("_MoveSpeed");

	private static readonly int moveSpeedRestClampId = Shader.PropertyToID("_MoveSpeedRestClamp");

	public float restSpeed;

	public float moveSpeed;

	public float materialMoveToggle;

	public static CritterLegSpeed FromMaterial(Material material)
	{
		float num = material.GetFloat(moveSpeedId);
		return new CritterLegSpeed
		{
			restSpeed = math.clamp(num, 0f, material.GetFloat(moveSpeedRestClampId)),
			moveSpeed = num,
			materialMoveToggle = material.GetFloat(materialMoveToggleId)
		};
	}

	public float Evaluate(float moveToggle)
	{
		return math.lerp(restSpeed, moveSpeed, math.saturate(materialMoveToggle * moveToggle));
	}
}
