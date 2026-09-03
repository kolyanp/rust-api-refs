using UnityEngine;
using UnityEngine.Serialization;

public class TriggerRadiation : TriggerBase
{
	[Tooltip("Higher the tier, higher the radiations.")]
	public Radiation.Tier radiationTier = Radiation.Tier.LOW;

	[Tooltip("The radiation amount is determined by the radiation tier, setting this will ignore the tier and use that value instead.")]
	public float RadiationAmountOverride;

	public bool BypassArmor;

	[Tooltip("Armor scales the dose instead of subtracting from it, so rad gear always reduces this volume but only full rad protection blocks it entirely. Ignored if BypassArmor is set.")]
	public bool ScaleByArmor;

	[Space]
	[Tooltip("The fraction of the radius where we fade in from 0-1 dosage.")]
	[Min(0f)]
	public float falloff = 0.1f;

	public bool usePerAxisFalloff;

	public Vector3 falloffPerAxis;

	[Tooltip("Use sphere collider size instead of the transform scale. For sphere triggers only (doesn't make sense for boxes)")]
	[FormerlySerializedAs("UseColliderRadius")]
	[Space]
	public bool DontScaleRadiationSize;

	public bool UseLOSCheck;

	[Tooltip("For sphere triggers only. If enabled, player will take more rads when close to the center of the volume.")]
	public bool IncreaseDamageNearCenter = true;

	public bool ApplyLocalHeightCheck;

	public bool IgnoreAboveGroundPlayers;

	[Tooltip("For sprinklers needing an half sphere trigger.")]
	public float MinLocalHeight;

	private SphereCollider sphereCollider;

	private BoxCollider boxCollider;

	private float FalloffFraction => Mathf.Clamp(falloff, 0.01f, 1f);

	public bool UseColliderScale => DontScaleRadiationSize;

	private bool UseSphere
	{
		get
		{
			if ((Object)(object)sphereCollider == (Object)null)
			{
				sphereCollider = ((Component)this).GetComponent<SphereCollider>();
			}
			return (Object)(object)sphereCollider != (Object)null;
		}
	}

	private bool UseBox
	{
		get
		{
			if ((Object)(object)boxCollider == (Object)null)
			{
				boxCollider = ((Component)this).GetComponent<BoxCollider>();
			}
			return (Object)(object)boxCollider != (Object)null;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		boxCollider = ((Component)this).GetComponent<BoxCollider>();
		sphereCollider = ((Component)this).GetComponent<SphereCollider>();
	}

	private float GetRadiationRadius()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)sphereCollider != (Object)null)
		{
			if (UseColliderScale)
			{
				return sphereCollider.radius;
			}
			return sphereCollider.radius * Vector3Ex.Max(((Component)this).transform.localScale);
		}
		return 0f;
	}

	private (Vector3 center, Vector3 extents) GetRadiationBounds()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)boxCollider == (Object)null)
		{
			return (center: Vector3.zero, extents: Vector3.zero);
		}
		Vector3 item = ((Component)this).transform.TransformPoint(boxCollider.center);
		Vector3 item2 = Vector3.Scale(boxCollider.size * 0.5f, ((Component)this).transform.lossyScale);
		return (center: item, extents: item2);
	}

	private float GetRadiationAmount()
	{
		if (RadiationAmountOverride > 0f)
		{
			return RadiationAmountOverride;
		}
		return Radiation.GetRadiation(radiationTier);
	}

	public float GetRadiationForPosition(Vector3 position, float radProtection, BaseEntity forEntity)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_021c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0236: Unknown result type (might be due to invalid IL or missing references)
		//IL_0242: Unknown result type (might be due to invalid IL or missing references)
		//IL_0249: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_028f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		if (ApplyLocalHeightCheck && ((Component)this).transform.InverseTransformPoint(position).y < MinLocalHeight)
		{
			return 0f;
		}
		if (IgnoreAboveGroundPlayers && !forEntity.IsUnderground())
		{
			return 0f;
		}
		if (UseLOSCheck && !GamePhysics.LineOfSight(((Component)this).gameObject.transform.position, position, 2097152, 0f, 0.25f))
		{
			return 0f;
		}
		float radiationAmount = GetRadiationAmount();
		float num = 0f;
		if (UseSphere)
		{
			float radiationRadius = GetRadiationRadius();
			float num2 = (IncreaseDamageNearCenter ? Vector3.Distance(((Component)this).gameObject.transform.position, position) : 0f);
			num = Mathf.InverseLerp(radiationRadius, radiationRadius * (1f - FalloffFraction), num2);
		}
		else if (UseBox)
		{
			(Vector3, Vector3) radiationBounds = GetRadiationBounds();
			Vector3 val = Quaternion.Inverse(((Component)this).transform.rotation) * (position - radiationBounds.Item1);
			Vector3 val2 = default(Vector3);
			((Vector3)(ref val2))._002Ector(Mathf.Abs(val.x), Mathf.Abs(val.y), Mathf.Abs(val.z));
			Vector3 item = radiationBounds.Item2;
			Vector3 val3 = radiationBounds.Item2;
			if (usePerAxisFalloff)
			{
				val3.x = Mathf.Max(0f, item.x - falloffPerAxis.x);
				val3.y = Mathf.Max(0f, item.y - falloffPerAxis.y);
				val3.z = Mathf.Max(0f, item.z - falloffPerAxis.z);
			}
			else
			{
				val3 = item * (1f - FalloffFraction);
			}
			if (val2.x <= val3.x && val2.y <= val3.y && val2.z <= val3.z)
			{
				num = 1f;
			}
			else if (val2.x <= item.x && val2.y <= item.y && val2.z <= item.z)
			{
				float num3 = ((item.x == val3.x) ? 1f : Mathf.Clamp01(Mathf.InverseLerp(item.x, val3.x, val2.x)));
				float num4 = ((item.y == val3.y) ? 1f : Mathf.Clamp01(Mathf.InverseLerp(item.y, val3.y, val2.y)));
				float num5 = ((item.z == val3.z) ? 1f : Mathf.Clamp01(Mathf.InverseLerp(item.z, val3.z, val2.z)));
				num = Mathf.Min(num3, Mathf.Min(num4, num5));
			}
			else
			{
				num = 0f;
			}
		}
		float num6;
		if (BypassArmor)
		{
			num6 = radiationAmount;
		}
		else if (ScaleByArmor)
		{
			float num7 = 1f - Mathf.Clamp01(radProtection / (Radiation.MaxExposureProtection * 100f));
			num6 = radiationAmount * num7;
		}
		else
		{
			num6 = Radiation.GetRadiationAfterProtection(radiationAmount, radProtection);
		}
		return num6 * num;
	}

	public override GameObject InterestedInObject(GameObject obj)
	{
		obj = base.InterestedInObject(obj);
		if ((Object)(object)obj == (Object)null)
		{
			return null;
		}
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(obj);
		if ((Object)(object)baseEntity == (Object)null)
		{
			return null;
		}
		if (baseEntity.isClient)
		{
			return null;
		}
		if (!(baseEntity is BaseCombatEntity))
		{
			return null;
		}
		return ((Component)baseEntity).gameObject;
	}
}
