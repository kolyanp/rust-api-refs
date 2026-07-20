using System;
using UnityEngine;

public class Pinata : BaseCombatEntity
{
	[Serializable]
	public struct VisualThreshold
	{
		public GameObject Root;

		public GameObjectRef DestroyEffect;

		[Range(0f, 1f)]
		public float HealthGreaterThan;
	}

	public Transform DropPoint;

	public float DropPointRadius;

	[Header("Hit Animation")]
	public float TotalSwingTime = 0.6f;

	public float SwingForce = 45f;

	public Transform SwingTransform;

	public AnimationCurve SwingCurve = new AnimationCurve((Keyframe[])(object)new Keyframe[3]
	{
		new Keyframe(0f, 0f),
		new Keyframe(0.5f, 1f),
		new Keyframe(1f, 0f)
	});

	[Header("Visual")]
	public VisualThreshold[] Thresholds;

	public LineRenderer Line;

	public Transform DestroyEffectSpawnPos;

	public float HangLength = -1.863f;

	public GameObjectRef FinalDestroyEffect;

	public override void OnDied(HitInfo info)
	{
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		base.OnDied(info);
		ItemAmount[] reward = PrefabAttribute.server.Find<PinataPayouts>(prefabID).GetPayout().Reward;
		foreach (ItemAmount itemAmount in reward)
		{
			if ((Object)(object)itemAmount.itemDef != (Object)null)
			{
				ItemManager.Create(itemAmount.itemDef, (int)itemAmount.amount, 0uL, isServerSide: true, 0uL).CreateWorldObject(DropPoint.position + Random.onUnitSphere * Random.Range(0f, DropPointRadius));
			}
		}
	}
}
