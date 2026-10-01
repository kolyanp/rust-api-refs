using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Development.Attributes;
using Facepunch;
using UnityEngine;

[ResetStaticFields]
public class HitboxSystem : MonoBehaviour, IPrefabPreProcess
{
	[Serializable]
	public class HitboxShape
	{
		public struct JobStruct
		{
			public Matrix4x4 transform;

			public Matrix4x4 inverseTransform;

			public Vector3 size;

			public HitboxDefinition.Type type;
		}

		public Transform bone;

		public HitboxDefinition.Type type;

		public Matrix4x4 localTransform;

		public PhysicsMaterial colliderMaterial;

		private Matrix4x4 transform;

		private Matrix4x4 inverseTransform;

		public Matrix4x4 Transform
		{
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return transform;
			}
		}

		public Vector3 Position
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				//IL_000b: Unknown result type (might be due to invalid IL or missing references)
				return transform.MultiplyPoint(Vector3.zero);
			}
		}

		public Quaternion Rotation
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				return transform.rotation;
			}
		}

		public Vector3 Size
		{
			[CompilerGenerated]
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return field;
			}
			[CompilerGenerated]
			private set
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				field = value;
			}
		}

		public void UpdateTransform()
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			using (TimeWarning.New("HitboxSystem.UpdateTransform"))
			{
				transform = bone.localToWorldMatrix * localTransform;
				Size = transform.lossyScale;
				transform = Matrix4x4.TRS(Position, Rotation, Vector3.one);
				inverseTransform = transform.inverse;
			}
		}

		public Vector3 TransformPoint(Vector3 pt)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			return transform.MultiplyPoint(pt);
		}

		public Vector3 InverseTransformPoint(Vector3 pt)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			return inverseTransform.MultiplyPoint(pt);
		}

		public Vector3 TransformDirection(Vector3 pt)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			return transform.MultiplyVector(pt);
		}

		public Vector3 InverseTransformDirection(Vector3 pt)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			return inverseTransform.MultiplyVector(pt);
		}

		public JobStruct GetJobStruct()
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			return new JobStruct
			{
				transform = transform,
				inverseTransform = inverseTransform,
				size = Size,
				type = type
			};
		}

		public bool Trace(Ray ray, out RaycastHit hit, float forgivness = 0f, float maxDistance = float.PositiveInfinity)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0083: Unknown result type (might be due to invalid IL or missing references)
			//IL_008a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
			using (TimeWarning.New("Hitbox.Trace"))
			{
				ray.origin = InverseTransformPoint(ray.origin);
				ray.direction = InverseTransformDirection(ray.direction);
				if (type == HitboxDefinition.Type.BOX)
				{
					AABB val = new AABB(Vector3.zero, Size);
					if (!val.Trace(ray, ref hit, forgivness, maxDistance))
					{
						return false;
					}
				}
				else
				{
					Capsule val2 = new Capsule(Vector3.zero, Size.x, Size.y * 0.5f);
					if (!val2.Trace(ray, ref hit, forgivness, maxDistance))
					{
						return false;
					}
				}
				hit.point = TransformPoint(hit.point);
				hit.normal = TransformDirection(hit.normal);
				return true;
			}
		}

		public Bounds GetBounds()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			//IL_0041: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			Matrix4x4 val = Transform;
			for (int i = 0; i < 3; i++)
			{
				for (int j = 0; j < 3; j++)
				{
					val[i, j] = Mathf.Abs(val[i, j]);
				}
			}
			Bounds result = default;
			Matrix4x4 val2 = Transform;
			result.center = val2.MultiplyPoint(Vector3.zero);
			result.extents = val.MultiplyVector(Size);
			return result;
		}
	}

	public List<HitboxShape> hitboxes = new List<HitboxShape>();

	bool IPrefabPreProcess.CanRunDuringBundling => false;

	public void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		List<HitboxDefinition> list = Pool.Get<List<HitboxDefinition>>();
		((Component)this).GetComponentsInChildren<HitboxDefinition>(list);
		if (serverside)
		{
			foreach (HitboxDefinition item2 in list)
			{
				if (preProcess != null)
				{
					preProcess.RemoveComponent((Component)(object)item2);
				}
			}
			if (preProcess != null)
			{
				preProcess.RemoveComponent((Component)(object)this);
			}
		}
		if (clientside)
		{
			hitboxes.Clear();
			foreach (HitboxDefinition item3 in list.OrderBy((HitboxDefinition x) => x.priority))
			{
				HitboxShape item = new HitboxShape
				{
					bone = ((Component)item3).transform,
					localTransform = item3.LocalMatrix,
					colliderMaterial = item3.physicMaterial,
					type = item3.type
				};
				hitboxes.Add(item);
				if (preProcess != null)
				{
					preProcess.RemoveComponent((Component)(object)item3);
				}
			}
		}
		Pool.FreeUnmanaged<HitboxDefinition>(ref list);
	}
}
