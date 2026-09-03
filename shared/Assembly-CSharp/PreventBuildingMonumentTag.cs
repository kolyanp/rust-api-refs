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
			result = default(OBB);
			return false;
		}
		Transform transform = ((Component)volume).transform;
		Collider obj = volume;
		BoxCollider val = (BoxCollider)(object)((obj is BoxCollider) ? obj : null);
		if (val != null)
		{
			result = new OBB(transform, new Bounds(val.center, val.size));
			return true;
		}
		Collider obj2 = volume;
		SphereCollider val2 = (SphereCollider)(object)((obj2 is SphereCollider) ? obj2 : null);
		if (val2 != null)
		{
			Vector3 lossyScale = transform.lossyScale;
			float num = val2.radius * 2f * Mathf.Max(new float[3]
			{
				Mathf.Abs(lossyScale.x),
				Mathf.Abs(lossyScale.y),
				Mathf.Abs(lossyScale.z)
			});
			result = new OBB(transform.TransformPoint(val2.center), Vector3.one * num, Quaternion.identity);
			return true;
		}
		Collider obj3 = volume;
		CapsuleCollider val3 = (CapsuleCollider)(object)((obj3 is CapsuleCollider) ? obj3 : null);
		if (val3 != null)
		{
			float num2 = val3.radius * 2f;
			Vector3 val4 = default(Vector3);
			((Vector3)(ref val4))._002Ector(num2, num2, num2);
			switch (val3.direction)
			{
			case 0:
				val4.x = Mathf.Max(val3.height, num2);
				break;
			case 1:
				val4.y = Mathf.Max(val3.height, num2);
				break;
			default:
				val4.z = Mathf.Max(val3.height, num2);
				break;
			}
			result = new OBB(transform, new Bounds(val3.center, val4));
			return true;
		}
		Bounds bounds = volume.bounds;
		result = new OBB(((Bounds)(ref bounds)).center, ((Bounds)(ref bounds)).size, Quaternion.identity);
		return ((Bounds)(ref bounds)).size != Vector3.zero;
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
