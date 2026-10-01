using UnityEngine;

public class PreventBuildingMonumentTag : MonoBehaviour
{
	public bool autoFindMonument;

	[SerializeField]
	private MonumentInfo AttachedMonument;

	private static readonly ListHashSet<PreventBuildingMonumentTag> allTags = new ListHashSet<PreventBuildingMonumentTag>();

	private Collider volume;

	private bool hasVolume;

	public static ListHashSet<PreventBuildingMonumentTag> All => allTags;

	private void Awake()
	{
		allTags.TryAdd(this);
	}

	private void OnDestroy()
	{
		allTags.Remove(this);
	}

	public bool TryGetVolume(out OBB result)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_017e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		if (!hasVolume)
		{
			volume = ((Component)this).GetComponent<Collider>();
			hasVolume = true;
		}
		if ((Object)(object)volume == (Object)null)
		{
			result = default;
			return false;
		}
		Transform transform = ((Component)volume).transform;
		Collider val = volume;
		BoxCollider val2 = (BoxCollider)(object)((val is BoxCollider) ? val : null);
		if (val2 != null)
		{
			result = new OBB(transform, new Bounds(val2.center, val2.size));
			return true;
		}
		Collider val3 = volume;
		SphereCollider val4 = (SphereCollider)(object)((val3 is SphereCollider) ? val3 : null);
		if (val4 != null)
		{
			Vector3 lossyScale = transform.lossyScale;
			float num = val4.radius * 2f * Mathf.Max(new float[3]
			{
				Mathf.Abs(lossyScale.x),
				Mathf.Abs(lossyScale.y),
				Mathf.Abs(lossyScale.z)
			});
			result = new OBB(transform.TransformPoint(val4.center), Vector3.one * num, Quaternion.identity);
			return true;
		}
		Collider val5 = volume;
		CapsuleCollider val6 = (CapsuleCollider)(object)((val5 is CapsuleCollider) ? val5 : null);
		if (val6 != null)
		{
			float num2 = val6.radius * 2f;
			Vector3 val7 = new Vector3(num2, num2, num2);
			switch (val6.direction)
			{
			case 0:
				val7.x = Mathf.Max(val6.height, num2);
				break;
			case 1:
				val7.y = Mathf.Max(val6.height, num2);
				break;
			default:
				val7.z = Mathf.Max(val6.height, num2);
				break;
			}
			result = new OBB(transform, new Bounds(val6.center, val7));
			return true;
		}
		Bounds bounds = volume.bounds;
		result = new OBB(bounds.center, bounds.size, Quaternion.identity);
		return bounds.size != Vector3.zero;
	}

	public MonumentInfo GetAttachedMonument()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		if (autoFindMonument && (Object)(object)AttachedMonument == (Object)null)
		{
			MonumentInfo attachedMonument = TerrainMeta.Path.FindClosest(TerrainMeta.Path.Monuments, ((Component)this).transform.position);
			AttachedMonument = attachedMonument;
		}
		return AttachedMonument;
	}

	public void SetMonument(MonumentInfo monument)
	{
		AttachedMonument = monument;
	}
}
