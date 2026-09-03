using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using FIMSpace.FTools;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FIMSpace.FTail;

[DefaultExecutionOrder(-4)]
[HelpURL("https://assetstore.unity.com/packages/tools/animation/tail-animator-121819")]
[AddComponentMenu("FImpossible Creations/Tail Animator 2")]
public class TailAnimator2 : MonoBehaviour, IDropHandler, IEventSystemHandler, IFHierarchyIcon, IClientComponent
{
	[Serializable]
	public class TailSegment
	{
		public Vector3 ProceduralPosition;

		public Vector3 ProceduralPositionWeightBlended;

		public Quaternion TrueTargetRotation;

		public Quaternion PosRefRotation;

		public Quaternion PreviousPosReferenceRotation;

		public Vector3 PreviousPosition;

		public float BlendValue;

		public Vector3 BoneDimensionsScaled;

		public float BoneLengthScaled;

		public Vector3 InitialLocalPosition;

		public Vector3 InitialLocalPositionInRoot;

		public Quaternion InitialLocalRotationInRoot;

		public Vector3 LocalOffset;

		public Quaternion InitialLocalRotation;

		public float ColliderRadius;

		public bool CollisionContactFlag;

		public float CollisionContactRelevancy;

		public Collision collisionContacts;

		public Vector3 VelocityHelper;

		public Quaternion QVelocityHelper;

		public Vector3 PreviousPush;

		public Quaternion Curving;

		public Vector3 Gravity;

		public Vector3 GravityLookOffset;

		public float LengthMultiplier;

		public float PositionSpeed;

		public float RotationSpeed;

		public float Springiness;

		public float Slithery;

		public float Curling;

		public float Slippery;

		public Quaternion LastKeyframeLocalRotation;

		public Vector3 LastKeyframeLocalPosition;

		[CompilerGenerated]
		private Vector3 _003CLastFinalPosition_003Ek__BackingField;

		[CompilerGenerated]
		private Quaternion _003CLastFinalRotation_003Ek__BackingField;

		[CompilerGenerated]
		private Vector3 _003CDeflection_003Ek__BackingField;

		private float deflectionSmoothVelo;

		[CompilerGenerated]
		private Vector3 _003CDeflectionWorldPosition_003Ek__BackingField;

		public TailSegment ParentBone { get; private set; }

		public TailSegment ChildBone { get; private set; }

		public Transform transform { get; private set; }

		public int Index { get; private set; }

		public float IndexOverlLength { get; private set; }

		public float BoneLength { get; private set; }

		public TailCollisionHelper CollisionHelper { get; internal set; }

		public bool IsDetachable { get; private set; }

		public Vector3 LastFinalPosition
		{
			[CompilerGenerated]
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return _003CLastFinalPosition_003Ek__BackingField;
			}
			[CompilerGenerated]
			private set
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				_003CLastFinalPosition_003Ek__BackingField = value;
			}
		}

		public Quaternion LastFinalRotation
		{
			[CompilerGenerated]
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return _003CLastFinalRotation_003Ek__BackingField;
			}
			[CompilerGenerated]
			private set
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				_003CLastFinalRotation_003Ek__BackingField = value;
			}
		}

		public float DeflectionFactor { get; private set; }

		public Vector3 Deflection
		{
			[CompilerGenerated]
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return _003CDeflection_003Ek__BackingField;
			}
			[CompilerGenerated]
			private set
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				_003CDeflection_003Ek__BackingField = value;
			}
		}

		public float DeflectionSmooth { get; private set; }

		public Vector3 DeflectionWorldPosition
		{
			[CompilerGenerated]
			get
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				return _003CDeflectionWorldPosition_003Ek__BackingField;
			}
			[CompilerGenerated]
			private set
			{
				//IL_0001: Unknown result type (might be due to invalid IL or missing references)
				//IL_0002: Unknown result type (might be due to invalid IL or missing references)
				_003CDeflectionWorldPosition_003Ek__BackingField = value;
			}
		}

		public int DeflectionRelevancy { get; private set; }

		public FImp_ColliderData_Base LatestSelectiveCollision { get; internal set; }

		public TailSegment()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_0059: Unknown result type (might be due to invalid IL or missing references)
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0074: Unknown result type (might be due to invalid IL or missing references)
			//IL_0090: Unknown result type (might be due to invalid IL or missing references)
			//IL_0095: Unknown result type (might be due to invalid IL or missing references)
			//IL_009b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
			//IL_0121: Unknown result type (might be due to invalid IL or missing references)
			//IL_0126: Unknown result type (might be due to invalid IL or missing references)
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0131: Unknown result type (might be due to invalid IL or missing references)
			//IL_0142: Unknown result type (might be due to invalid IL or missing references)
			ProceduralPosition = Vector3.zero;
			ProceduralPositionWeightBlended = Vector3.zero;
			TrueTargetRotation = Quaternion.identity;
			PosRefRotation = Quaternion.identity;
			PreviousPosReferenceRotation = Quaternion.identity;
			BlendValue = 1f;
			InitialLocalPosition = Vector3.zero;
			InitialLocalPositionInRoot = Vector3.zero;
			InitialLocalRotationInRoot = Quaternion.identity;
			LocalOffset = Vector3.zero;
			InitialLocalRotation = Quaternion.identity;
			ColliderRadius = 1f;
			CollisionContactRelevancy = -1f;
			VelocityHelper = Vector3.zero;
			QVelocityHelper = Quaternion.identity;
			PreviousPush = Vector3.zero;
			Curving = Quaternion.identity;
			Gravity = Vector3.zero;
			GravityLookOffset = Vector3.zero;
			LengthMultiplier = 1f;
			PositionSpeed = 1f;
			RotationSpeed = 1f;
			Slithery = 1f;
			Curling = 0.5f;
			Slippery = 1f;
			base._002Ector();
			Index = -1;
			Curving = Quaternion.identity;
			Gravity = Vector3.zero;
			LengthMultiplier = 1f;
			Deflection = Vector3.zero;
			DeflectionFactor = 0f;
			DeflectionRelevancy = -1;
			deflectionSmoothVelo = 0f;
		}

		public TailSegment(Transform transform)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0055: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			this._002Ector();
			if (!((Object)(object)transform == (Object)null))
			{
				this.transform = transform;
				ProceduralPosition = transform.position;
				PreviousPosition = transform.position;
				PosRefRotation = transform.rotation;
				PreviousPosReferenceRotation = PosRefRotation;
				TrueTargetRotation = PosRefRotation;
				ReInitializeLocalPosRot(transform.localPosition, transform.localRotation);
				BoneLength = 0.1f;
			}
		}

		public TailSegment(TailSegment copyFrom)
		{
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0080: Unknown result type (might be due to invalid IL or missing references)
			this._002Ector(copyFrom.transform);
			transform = copyFrom.transform;
			Index = copyFrom.Index;
			IndexOverlLength = copyFrom.IndexOverlLength;
			ProceduralPosition = copyFrom.ProceduralPosition;
			PreviousPosition = copyFrom.PreviousPosition;
			ProceduralPositionWeightBlended = copyFrom.ProceduralPosition;
			PosRefRotation = copyFrom.PosRefRotation;
			PreviousPosReferenceRotation = PosRefRotation;
			TrueTargetRotation = copyFrom.TrueTargetRotation;
			ReInitializeLocalPosRot(copyFrom.InitialLocalPosition, copyFrom.InitialLocalRotation);
		}

		public void ReInitializeLocalPosRot(Vector3 initLocalPos, Quaternion initLocalRot)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			InitialLocalPosition = initLocalPos;
			InitialLocalRotation = initLocalRot;
		}

		public void SetIndex(int i, int tailSegments)
		{
			Index = i;
			if (i < 0)
			{
				IndexOverlLength = 0f;
			}
			else
			{
				IndexOverlLength = (float)i / (float)tailSegments;
			}
		}

		public void SetParentRef(TailSegment parent)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			ParentBone = parent;
			Vector3 val = ProceduralPosition - ParentBone.ProceduralPosition;
			BoneLength = ((Vector3)(ref val)).magnitude;
		}

		public void SetChildRef(TailSegment child)
		{
			ChildBone = child;
		}

		public float GetRadiusScaled()
		{
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			return ColliderRadius * transform.lossyScale.x;
		}

		public void AssignDetachedRootCoords(Transform root)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			InitialLocalPositionInRoot = root.InverseTransformPoint(transform.position);
			InitialLocalRotationInRoot = FEngineering.QToLocal(root.rotation, transform.rotation);
			IsDetachable = true;
		}

		internal Vector3 BlendMotionWeight(Vector3 newPosition)
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			return Vector3.LerpUnclamped(ParentBone.ProceduralPosition + FEngineering.TransformVector(ParentBone.LastKeyframeLocalRotation, ParentBone.transform.lossyScale, LastKeyframeLocalPosition), newPosition, BlendValue);
		}

		internal void PreCalibrate()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			transform.localPosition = InitialLocalPosition;
			transform.localRotation = InitialLocalRotation;
		}

		internal void Validate()
		{
			if (BoneLength == 0f)
			{
				BoneLength = 0.001f;
			}
		}

		public void RefreshKeyLocalPosition()
		{
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			if (Object.op_Implicit((Object)(object)transform))
			{
				LastKeyframeLocalRotation = transform.localRotation;
			}
			else
			{
				LastKeyframeLocalRotation = InitialLocalRotation;
			}
		}

		public void RefreshKeyLocalPositionAndRotation()
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			if (Object.op_Implicit((Object)(object)transform))
			{
				RefreshKeyLocalPositionAndRotation(transform.localPosition, transform.localRotation);
			}
			else
			{
				RefreshKeyLocalPositionAndRotation(InitialLocalPosition, InitialLocalRotation);
			}
		}

		public void RefreshKeyLocalPositionAndRotation(Vector3 p, Quaternion r)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			LastKeyframeLocalPosition = p;
			LastKeyframeLocalRotation = r;
		}

		internal Vector3 ParentToFrontOffset()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			return FEngineering.TransformVector(ParentBone.LastKeyframeLocalRotation, ParentBone.transform.lossyScale, LastKeyframeLocalPosition);
		}

		public void RefreshFinalPos(Vector3 pos)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			LastFinalPosition = pos;
		}

		public void RefreshFinalRot(Quaternion rot)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			LastFinalRotation = rot;
		}

		public bool CheckDeflectionState(float zeroWhenLower, float smoothTime, float delta)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Unknown result type (might be due to invalid IL or missing references)
			//IL_010c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val = LastKeyframeLocalPosition - ParentBone.transform.InverseTransformVector(ProceduralPosition - ParentBone.ProceduralPosition);
			DeflectionFactor = Vector3.Dot(((Vector3)(ref LastKeyframeLocalPosition)).normalized, ((Vector3)(ref val)).normalized);
			if (DeflectionFactor < zeroWhenLower)
			{
				if (smoothTime <= Mathf.Epsilon)
				{
					DeflectionSmooth = 0f;
				}
				else
				{
					DeflectionSmooth = Mathf.SmoothDamp(DeflectionSmooth, 0f - Mathf.Epsilon, ref deflectionSmoothVelo, smoothTime / 1.5f, float.PositiveInfinity, delta);
				}
			}
			else if (smoothTime <= Mathf.Epsilon)
			{
				DeflectionSmooth = 1f;
			}
			else
			{
				DeflectionSmooth = Mathf.SmoothDamp(DeflectionSmooth, 1f, ref deflectionSmoothVelo, smoothTime, float.PositiveInfinity, delta);
			}
			if (DeflectionSmooth <= Mathf.Epsilon)
			{
				return true;
			}
			if (ChildBone.ChildBone != null)
			{
				DeflectionWorldPosition = ChildBone.ChildBone.ProceduralPosition;
			}
			else
			{
				DeflectionWorldPosition = ChildBone.ProceduralPosition;
			}
			return false;
		}

		public bool DeflectionRelevant()
		{
			if (DeflectionRelevancy == -1)
			{
				DeflectionRelevancy = 3;
				return true;
			}
			DeflectionRelevancy = 3;
			return false;
		}

		public bool? DeflectionRestoreState()
		{
			if (DeflectionRelevancy > 0)
			{
				DeflectionRelevancy--;
				if (DeflectionRelevancy == 0)
				{
					DeflectionRelevancy = -1;
					return null;
				}
				return true;
			}
			return false;
		}

		internal void ParamsFrom(TailSegment other)
		{
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			BlendValue = other.BlendValue;
			ColliderRadius = other.ColliderRadius;
			Gravity = other.Gravity;
			LengthMultiplier = other.LengthMultiplier;
			BoneLength = other.BoneLength;
			BoneLengthScaled = other.BoneLengthScaled;
			BoneDimensionsScaled = other.BoneDimensionsScaled;
			collisionContacts = other.collisionContacts;
			CollisionHelper = other.CollisionHelper;
			PositionSpeed = other.PositionSpeed;
			RotationSpeed = other.RotationSpeed;
			Springiness = other.Springiness;
			Slithery = other.Slithery;
			Curling = other.Curling;
			Slippery = other.Slippery;
		}

		internal void ParamsFromAll(TailSegment other)
		{
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0092: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00da: Unknown result type (might be due to invalid IL or missing references)
			ParamsFrom(other);
			InitialLocalPosition = other.InitialLocalPosition;
			InitialLocalRotation = other.InitialLocalRotation;
			LastFinalPosition = other.LastFinalPosition;
			LastFinalRotation = other.LastFinalRotation;
			ProceduralPosition = other.ProceduralPosition;
			ProceduralPositionWeightBlended = other.ProceduralPositionWeightBlended;
			TrueTargetRotation = other.TrueTargetRotation;
			PosRefRotation = other.PosRefRotation;
			PreviousPosReferenceRotation = other.PreviousPosReferenceRotation;
			PreviousPosition = other.PreviousPosition;
			BoneLength = other.BoneLength;
			BoneDimensionsScaled = other.BoneDimensionsScaled;
			BoneLengthScaled = other.BoneLengthScaled;
			LocalOffset = other.LocalOffset;
			ColliderRadius = other.ColliderRadius;
			VelocityHelper = other.VelocityHelper;
			QVelocityHelper = other.QVelocityHelper;
			PreviousPush = other.PreviousPush;
		}

		internal void User_ReassignTransform(Transform t)
		{
			transform = t;
		}

		public void Reset()
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0035: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0098: Unknown result type (might be due to invalid IL or missing references)
			//IL_009d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_0081: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0091: Unknown result type (might be due to invalid IL or missing references)
			PreviousPush = Vector3.zero;
			VelocityHelper = Vector3.zero;
			QVelocityHelper = Quaternion.identity;
			if (Object.op_Implicit((Object)(object)transform))
			{
				ProceduralPosition = transform.position;
				PosRefRotation = transform.rotation;
				PreviousPosReferenceRotation = transform.rotation;
			}
			else if (Object.op_Implicit((Object)(object)ParentBone.transform))
			{
				ProceduralPosition = ParentBone.transform.position + ParentToFrontOffset();
			}
			PreviousPosition = ProceduralPosition;
			ProceduralPositionWeightBlended = ProceduralPosition;
		}
	}

	public enum ECollisionSpace
	{
		World_Slow,
		Selective_Fast
	}

	public enum ECollisionMode
	{
		m_3DCollision,
		m_2DCollision
	}

	[Serializable]
	public class IKBoneSettings
	{
		[Range(0f, 181f)]
		public float AngleLimit = 45f;

		[Range(0f, 181f)]
		public float TwistAngleLimit = 5f;

		public bool UseInChain = true;
	}

	public enum FEWavingType
	{
		Simple,
		Advanced
	}

	public enum EFDeltaType
	{
		DeltaTime,
		SmoothDeltaTime,
		UnscaledDeltaTime,
		FixedDeltaTime,
		SafeDelta
	}

	public enum EAnimationStyle
	{
		Quick,
		Accelerating,
		Linear
	}

	public enum ETailCategory
	{
		Setup,
		Tweak,
		Features,
		Shaping
	}

	public enum ETailFeaturesCategory
	{
		Main,
		Collisions,
		IK,
		Experimental
	}

	public enum EFixedMode
	{
		None,
		Basic,
		Late
	}

	private static ListHashSet<TailAnimator2> components = new ListHashSet<TailAnimator2>();

	[Tooltip("Using some simple calculations to make tail bend on colliders")]
	public bool UseCollision;

	[Tooltip("How collision should be detected, world gives you collision on all world colliders but with more use of cpu (using unity's rigidbodies), 'Selective' gives you possibility to detect collision on selected colliders without using Rigidbodies, it also gives smoother motion (deactivated colliders will still detect collision, unless its game object is disabled)")]
	public ECollisionSpace CollisionSpace;

	public ECollisionMode CollisionMode;

	[Tooltip("If you want to stop checking collision if segment collides with one collider\n\nSegment collision with two or more colliders in the same time with this option enabled can result in stuttery motion")]
	public bool CheapCollision;

	[Tooltip("Using trigger collider to include encountered colliders into collide with list")]
	public bool DynamicWorldCollidersInclusion;

	[Tooltip("Radius of trigger collider for dynamic inclusion of colliders")]
	public float InclusionRadius;

	public bool IgnoreMeshColliders;

	public List<Collider> IncludedColliders;

	public List<Collider2D> IncludedColliders2D;

	protected List<FImp_ColliderData_Base> IncludedCollidersData;

	protected List<FImp_ColliderData_Base> CollidersDataToCheck;

	[Tooltip("Capsules can give much more precise collision detection")]
	public int CollidersType;

	public bool CollideWithOtherTails;

	[Tooltip("Collision with colliders even if they're disabled (but game object must be enabled)\nHelpful to setup character limbs collisions without need to create new Layer")]
	public bool CollideWithDisabledColliders;

	[Range(0f, 1f)]
	public float CollisionSlippery;

	[Tooltip("If tail colliding objects should fit to colliders (0) or be reflect from them (Reflecting Only with 'Slithery' parameter greater than ~0.2)")]
	[Range(0f, 1f)]
	public float ReflectCollision;

	public AnimationCurve CollidersScaleCurve;

	public float CollidersScaleMul;

	[Range(0f, 1f)]
	public float CollisionsAutoCurve;

	public List<Collider> IgnoredColliders;

	public List<Collider2D> IgnoredColliders2D;

	public bool CollidersSameLayer;

	[Tooltip("If you add rigidbodies to each tail segment's collider, collision will work on everything but it will be less optimal, you don't have to add here rigidbodies but then you must have not kinematic rigidbodies on objects segments can collide")]
	public bool CollidersAddRigidbody;

	public float RigidbodyMass;

	[FPD_Layers]
	public int CollidersLayer;

	public bool UseSlitheryCurve;

	[FPD_FixedCurveWindow(0f, 0f, 1f, 1.2f, 0.1f, 0.8f, 1f, 0.9f)]
	public AnimationCurve SlitheryCurve;

	private float lastSlithery;

	private Keyframe[] lastSlitheryCurvKeys;

	public bool UseCurlingCurve;

	[FPD_FixedCurveWindow(0f, 0f, 1f, 1f, 0.65f, 0.4f, 1f, 0.9f)]
	public AnimationCurve CurlingCurve;

	private float lastCurling;

	private Keyframe[] lastCurlingCurvKeys;

	public bool UseSpringCurve;

	[FPD_FixedCurveWindow(0f, 0f, 1f, 1f, 0.9f, 0.7f, 0.2f, 0.9f)]
	public AnimationCurve SpringCurve;

	private float lastSpringiness;

	private Keyframe[] lastSpringCurvKeys;

	public bool UseSlipperyCurve;

	[FPD_FixedCurveWindow(0f, 0f, 1f, 1f, 0.2f, 0.9f, 0.6f, 0.9f)]
	public AnimationCurve SlipperyCurve;

	private float lastSlippery;

	private Keyframe[] lastSlipperyCurvKeys;

	public bool UsePosSpeedCurve;

	[FPD_FixedCurveWindow(0f, 0f, 1f, 1f, 0.2f, 1f, 0.3f, 0.9f)]
	public AnimationCurve PosCurve;

	private float lastPosSpeeds;

	private Keyframe[] lastPosCurvKeys;

	public bool UseRotSpeedCurve;

	[FPD_FixedCurveWindow(0f, 0f, 1f, 1f, 0.7f, 0.7f, 0.7f, 0.9f)]
	public AnimationCurve RotCurve;

	private float lastRotSpeeds;

	private Keyframe[] lastRotCurvKeys;

	[Tooltip("Spreading Tail Animator motion weight over bones")]
	public bool UsePartialBlend;

	[FPD_FixedCurveWindow(0f, 0f, 1f, 1f, 0.2f, 0.5f, 0.85f, 1f)]
	public AnimationCurve BlendCurve;

	private float lastTailAnimatorAmount;

	private Keyframe[] lastBlendCurvKeys;

	private TailSegment _ex_bone;

	public bool UseIK;

	private bool ikInitialized;

	[SerializeField]
	private FIK_CCDProcessor IK;

	[Tooltip("Target object to follow by IK")]
	public Transform IKTarget;

	public bool IKAutoWeights;

	[Range(0f, 1f)]
	public float IKBaseReactionWeight;

	[FPD_FixedCurveWindow(0f, 0f, 1f, 1f, 0.2f, 0.5f, 0.85f, 1f)]
	public AnimationCurve IKReactionWeightCurve;

	public bool IKAutoAngleLimits;

	[FPD_Suffix(0f, 181f, FPD_SuffixAttribute.SuffixMode.FromMinToMaxRounded, "°", true, 0)]
	public float IKAutoAngleLimit;

	[Tooltip("If ik process should work referencing to previously computed CCDIK pose (can be more precise but need more adjusting in weights and angle limits)")]
	public bool IKContinousSolve;

	[Tooltip("Inverting ik iteration order to generate different pose results - more straight towards target")]
	public bool IkInvertOrder;

	[Tooltip("How much IK motion sohuld be used in tail animator motion -> 0: turned off")]
	[FPD_Suffix(0f, 1f, FPD_SuffixAttribute.SuffixMode.From0to100, "%", true, 0)]
	public float IKBlend;

	[FPD_Suffix(0f, 1f, FPD_SuffixAttribute.SuffixMode.From0to100, "%", true, 0)]
	[Tooltip("If syncing with animator then applying motion of keyframe animation for IK")]
	public float IKAnimatorBlend;

	[Range(1f, 32f)]
	[Tooltip("How much iterations should do CCDIK algorithm in one frame")]
	public int IKReactionQuality;

	[Tooltip("Smoothing reactions in CCD IK algorithm")]
	[Range(0f, 1f)]
	public float IKSmoothing;

	[Range(0f, 1.5f)]
	public float IKStretchToTarget;

	[FPD_FixedCurveWindow(0f, 0f, 1f, 1f, 0.9f, 0.4f, 0.5f, 1f)]
	public AnimationCurve IKStretchCurve;

	public List<IKBoneSettings> IKLimitSettings;

	public bool IKSelectiveChain;

	private Vector3? _IKCustomPos;

	private List<TailSegment> _pp_reference;

	private TailSegment _pp_ref_rootParent;

	private TailSegment _pp_ref_lastChild;

	private bool _pp_initialized;

	[Tooltip("Rotation offset for tail (just first (root) bone is rotated)")]
	public Quaternion RotationOffset;

	[Tooltip("Rotate each segment a bit to create curving effect")]
	public Quaternion Curving;

	[Tooltip("Spread curving rotation offset weight over tail segments")]
	public bool UseCurvingCurve;

	[FPD_FixedCurveWindow(0f, -1f, 1f, 1f, 0.75f, 0.75f, 0.75f, 0.85f)]
	public AnimationCurve CurvCurve;

	private Quaternion lastCurving;

	private Keyframe[] lastCurvingKeys;

	[Tooltip("Make tail longer or shorter")]
	public float LengthMultiplier;

	[Tooltip("Spread length multiplier weight over tail segments")]
	public bool UseLengthMulCurve;

	[FPD_FixedCurveWindow(0f, 0f, 1f, 3f, 0f, 1f, 1f, 1f)]
	public AnimationCurve LengthMulCurve;

	private float lastLengthMul;

	private Keyframe[] lastLengthKeys;

	[Tooltip("Spread gravity weight over tail segments")]
	public bool UseGravityCurve;

	[Tooltip("Spread gravity weight over tail segments")]
	[FPD_FixedCurveWindow(0f, 0f, 1f, 1f, 0.85f, 0.35f, 0.25f, 0.85f)]
	public AnimationCurve GravityCurve;

	[Tooltip("Simulate gravity weight for tail logics")]
	public Vector3 Gravity;

	private Vector3 lastGravity;

	private Keyframe[] lastGravityKeys;

	[Tooltip("Using auto waving option to give floating effect")]
	public bool UseWaving;

	[Tooltip("Adding some variation to waving animation")]
	public bool CosinusAdd;

	[Tooltip("If you want few tails to wave in the same way you can set this sinus period cycle value")]
	public float FixedCycle;

	[Tooltip("How frequent swings should be")]
	public float WavingSpeed;

	[Tooltip("How big swings should be")]
	public float WavingRange;

	[Tooltip("What rotation axis should be used in auto waving")]
	public Vector3 WavingAxis;

	[CompilerGenerated]
	private Quaternion _003CWavingRotationOffset_003Ek__BackingField;

	[Tooltip("Type of waving animation algorithm, it can be simple trigonometric wave or animation based on noises (advanced)")]
	public FEWavingType WavingType;

	[Tooltip("Offsetting perlin noise to generate different variation of tail rotations")]
	public float AlternateWave;

	private float _waving_waveTime;

	private float _waving_cosTime;

	private Vector3 _waving_sustain;

	public bool UseWind;

	[FPD_Suffix(0f, 2.5f, FPD_SuffixAttribute.SuffixMode.PercentageUnclamped, "%", true, 0)]
	public float WindEffectPower;

	[FPD_Suffix(0f, 2.5f, FPD_SuffixAttribute.SuffixMode.PercentageUnclamped, "%", true, 0)]
	public float WindTurbulencePower;

	[FPD_Suffix(0f, 1.5f, FPD_SuffixAttribute.SuffixMode.PercentageUnclamped, "%", true, 0)]
	public float WindWorldNoisePower;

	public Vector3 WindEffect;

	public List<TailSegment> TailSegments;

	[SerializeField]
	private TailSegment GhostParent;

	[SerializeField]
	private TailSegment GhostChild;

	private Vector3 _limiting_limitPosition;

	private Vector3 _limiting_influenceOffset;

	private float _limiting_stretchingHelperTooLong;

	private float _limiting_stretchingHelperTooShort;

	private Quaternion _limiting_angle_ToTargetRot;

	private Quaternion _limiting_angle_targetInLocal;

	private Quaternion _limiting_angle_newLocal;

	private Vector3 _tc_segmentGravityOffset;

	private Vector3 _tc_segmentGravityToParentDir;

	private Vector3 _tc_preGravOff;

	[Tooltip("If you want to use max distance fade option to smoothly disable tail animator when object is going far away from camera")]
	public bool UseMaxDistance;

	[Tooltip("(By default camera transform) Measuring distance from this object to define if object is too far and not need to update tail animator")]
	public Transform DistanceFrom;

	[HideInInspector]
	public Transform _distanceFrom_Auto;

	[Tooltip("Max distance to main camera / target object to smoothly turn off tail animator.")]
	public float MaximumDistance;

	[Tooltip("If object in range should be detected only when is nearer than 'MaxDistance' to avoid stuttery enabled - disable switching")]
	[Range(0f, 1f)]
	public float MaxOutDistanceFactor;

	[Tooltip("If distance should be measured not using Up (y) axis")]
	public bool DistanceWithoutY;

	[Tooltip("Offsetting point from which we want to measure distance to target")]
	public Vector3 DistanceMeasurePoint;

	[Tooltip("Disable fade duration in seconds")]
	[Range(0.25f, 2f)]
	public float FadeDuration;

	private bool maxDistanceExceed;

	private Transform finalDistanceFrom;

	private bool wasCameraSearch;

	private float distanceWeight;

	private int _tc_startI;

	private int _tc_startII;

	private TailSegment _tc_rootBone;

	private Quaternion _tc_lookRot;

	private Quaternion _tc_targetParentRot;

	private Quaternion _tc_startBoneRotOffset;

	private float _tc_tangle;

	private float _sg_springVelo;

	private float _sg_curly;

	private Vector3 _sg_push;

	private Vector3 _sg_targetPos;

	private Vector3 _sg_targetChildWorldPosInParentFront;

	private Vector3 _sg_dirToTargetParentFront;

	private Quaternion _sg_orientation;

	private float _sg_slitFactor;

	private bool wasDisabled;

	private float justDelta;

	private float secPeriodDelta;

	private float deltaForLerps;

	private float rateDelta;

	protected float collectedDelta;

	protected int framesToSimulate;

	protected int previousframesToSimulate;

	private bool updateTailAnimator;

	private int startAfterTPoseCounter;

	private bool fixedUpdated;

	private bool lateFixedIsRunning;

	private bool fixedAllow;

	[Range(0f, 1f)]
	[Tooltip("Making tail segment deflection influence back segments")]
	public float Deflection;

	[FPD_Suffix(1f, 89f, FPD_SuffixAttribute.SuffixMode.FromMinToMaxRounded, "°", true, 0)]
	public float DeflectionStartAngle;

	[Range(0f, 1f)]
	public float DeflectionSmooth;

	[FPD_FixedCurveWindow(0f, 0f, 1f, 1f, 0.65f, 0.4f, 1f, 0.9f)]
	public AnimationCurve DeflectionFalloff;

	[Tooltip("Deflection can be triggered every time tail is waving but you not always would want this feature be enabled (different behaviour of tail motion)")]
	public bool DeflectOnlyCollisions;

	private List<TailSegment> _defl_source;

	private float _defl_treshold;

	private bool _forceDisable;

	private float _forceDisableElapsed;

	public ETailCategory _Editor_Category;

	public ETailFeaturesCategory _Editor_FeaturesCategory;

	public bool DrawGizmos;

	[Tooltip("First bone of tail motion chain")]
	public Transform StartBone;

	[Tooltip("Finish bone of tail motion chain")]
	public Transform EndBone;

	[Tooltip("Adjusting end point for end tail bone motion")]
	public Vector3 EndBoneJointOffset;

	public List<Transform> _TransformsGhostChain;

	public int _GhostChainInitCount;

	protected bool initialized;

	[Tooltip("Target FPS update rate for Tail Animator.\n\nIf you want Tail Animator to behave the same in low/high fps, set this value for example to 60.\nIt also can help optimizing if your game have more than 60 fps.")]
	public int UpdateRate;

	[Tooltip("If your character Unity's Animator have update mode set to 'Animate Physics' you should enable it here too")]
	public EFixedMode AnimatePhysics;

	[Tooltip("When using target fps rate you can interpolate coordinates for smoother effect when object with tail is moving a lot")]
	public bool InterpolateRate;

	[Tooltip("Simulating tail motion at initiation to prevent jiggle start")]
	public bool Prewarm;

	internal float OverrideWeight;

	protected float conditionalWeight;

	protected bool collisionInitialized;

	protected bool forceRefreshCollidersData;

	private Vector3 previousWorldPosition;

	protected Transform rootTransform;

	protected bool preAutoCorrect;

	[Tooltip("Blending Slithery - smooth & soft tentacle like movement (value = 1)\nwith more stiff & springy motion (value = 0)\n\n0: Stiff somewhat like tree branch\n1: Soft like squid tentacle / Animal tail")]
	[Range(0f, 1.2f)]
	public float Slithery;

	[Tooltip("How curly motion should be applied to tail segments")]
	[Range(0f, 1f)]
	public float Curling;

	[Range(0f, 1f)]
	[Tooltip("Elastic spring effect making motion more 'meaty'")]
	public float Springiness;

	[Range(0f, 1f)]
	[Tooltip("If you want to limit stretching/gumminess of position motion when object moves fast. Recommended adjust to go with it under 0.3 value.\nValue = 1: Unlimited stretching")]
	public float MaxStretching;

	[FPD_Suffix(1f, 181f, FPD_SuffixAttribute.SuffixMode.FromMinToMaxRounded, "°", true, 0)]
	[Tooltip("Limiting max rotation angle for each tail segment")]
	public float AngleLimit;

	[Tooltip("If you need specific axis to be limited.\nLeave unchanged to limit all axes.")]
	public Vector3 AngleLimitAxis;

	[Tooltip("If you want limit axes symmetrically leave this parameter unchanged, if you want limit one direction of axis more than reversed, tweak this parameter")]
	public Vector2 LimitAxisRange;

	[Tooltip("If limiting shouldn't be too rapidly performed")]
	[Range(0f, 1f)]
	public float LimitSmoothing;

	[Tooltip("If your object moves very fast making tail influenced by speed too much then you can controll it with this parameter")]
	[FPD_Suffix(0f, 1.5f, FPD_SuffixAttribute.SuffixMode.PercentageUnclamped, "%", true, 0)]
	public float MotionInfluence;

	[Tooltip("Additional Y influence controll useful when your character is jumping (works only when MotionInfluence value is other than 100%)")]
	[Range(0f, 1f)]
	public float MotionInfluenceInY;

	[Tooltip("If first bone of chain should also be affected with whole chain")]
	public bool IncludeParent;

	[Tooltip("By basic algorithm of Tail Animator different sized tails with different number of bones would animate with different bending thanks to this toggle every setup bends in very similar amount.\n\nShort tails will bend more and longer oner with bigger amount of bones less with this option enabled.")]
	[Range(0f, 1f)]
	public float UnifyBendiness;

	[Range(0f, 1f)]
	[Tooltip("Reaction Speed is defining how fast tail segments will return to target position, it gives animation more underwater/floaty feeling if it's lower")]
	public float ReactionSpeed;

	[Range(0f, 1f)]
	[Tooltip("Sustain is similar to reaction speed in reverse, but providing sustain motion effect when increased")]
	public float Sustain;

	[Tooltip("Rotation speed is defining how fast tail segments will return to target rotation, it gives animation more lazy feeling if it's lower")]
	[Range(0f, 1f)]
	public float RotationRelevancy;

	[Tooltip("Smoothing motion values change over time style to be applied for 'Reaction Speed' and 'Rotation Relevancy' parameters")]
	public EAnimationStyle SmoothingStyle;

	[Tooltip("Slowmo or speedup tail animation reaction")]
	public float TimeScale;

	[Tooltip("Delta time type to be used by algorithm")]
	public EFDeltaType DeltaType;

	[Tooltip("Useful when you use other components to affect bones hierarchy and you want this component to follow other component's changes\n\nIt can be really useful when working with 'Spine Animator'")]
	public bool UpdateAsLast;

	[Tooltip("Checking if keyframed animation has some empty keyframes which could cause unwanted twisting errors")]
	public bool DetectZeroKeyframes;

	[Tooltip("Initializing Tail Animator after first frames of game to not initialize with model's T-Pose but after playing some other animation")]
	public bool StartAfterTPose;

	[Tooltip("If you want Tail Animator to stop computing when choosed animator is not enabled")]
	public Animator OptimizeWithAnimator;

	[Tooltip("Blend Source Animation (keyframed / unanimated) and Tail Animator")]
	[FPD_Suffix(0f, 1f, FPD_SuffixAttribute.SuffixMode.From0to100, "%", true, 0)]
	public float TailAnimatorAmount;

	[Tooltip("Removing transforms hierachy structure to optimize Unity's calculations on Matrixes.\nIt can give very big boost in performance for long tails but it can't work with animated models!")]
	public bool DetachChildren;

	[Tooltip("If tail movement should not move in depth you can use this parameter")]
	public int Axis2D;

	[Tooltip("[Experimental: Works only with Slithery Blend set to >= 1] Making each segment go to target pose in front of parent segment creating new animation effect")]
	[Range(-1f, 1f)]
	public float Tangle;

	[Tooltip("Making tail animate also roll rotation like it was done in Tail Animator V1 ! Use Rotation Relevancy Parameter (set lower than 0.5) !")]
	public bool AnimateRoll;

	[Range(0f, 1f)]
	[Tooltip("Overriding keyframe animation with just Tail Animator option (keyframe animation treated as t-pose bones rotations)")]
	public float OverrideKeyframeAnimation;

	private Transform _baseTransform;

	public List<Component> DynamicAlwaysInclude { get; private set; }

	public Quaternion WavingRotationOffset
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003CWavingRotationOffset_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003CWavingRotationOffset_003Ek__BackingField = value;
		}
	}

	public float _TC_TailLength { get; private set; }

	public string EditorIconPath
	{
		get
		{
			if (PlayerPrefs.GetInt("AnimsH", 1) == 0)
			{
				return "";
			}
			return "Tail Animator/Tail Animator Icon Small";
		}
	}

	public bool IsInitialized => initialized;

	public Transform BaseTransform
	{
		get
		{
			if (Object.op_Implicit((Object)(object)_baseTransform))
			{
				return _baseTransform;
			}
			if (_TransformsGhostChain != null && _TransformsGhostChain.Count > 0)
			{
				_baseTransform = _TransformsGhostChain[0];
			}
			if ((Object)(object)_baseTransform != (Object)null)
			{
				return _baseTransform;
			}
			return ((Component)this).transform;
		}
	}

	private void OnEnable()
	{
		components.TryAdd(this);
	}

	private void OnDisable()
	{
		components.Remove(this);
	}

	public static void TickAll()
	{
		for (int num = components.Count - 1; num >= 0; num--)
		{
			TailAnimator2 tailAnimator = components[num];
			if (ObjectEx.IsUnityNull(tailAnimator))
			{
				components.RemoveAt(num);
			}
			else
			{
				tailAnimator.Tick();
			}
		}
	}

	public static void LateTickAll()
	{
		for (int num = components.Count - 1; num >= 0; num--)
		{
			TailAnimator2 tailAnimator = components[num];
			if (ObjectEx.IsUnityNull(tailAnimator))
			{
				components.RemoveAt(num);
			}
			else
			{
				tailAnimator.LateTick();
			}
		}
	}

	public static void FixedTickAll()
	{
		for (int num = components.Count - 1; num >= 0; num--)
		{
			TailAnimator2 tailAnimator = components[num];
			if (ObjectEx.IsUnityNull(tailAnimator))
			{
				components.RemoveAt(num);
			}
			else
			{
				tailAnimator.FixedTick();
			}
		}
	}

	private void RefreshSegmentsColliders()
	{
		if (CollisionSpace == ECollisionSpace.Selective_Fast && TailSegments != null && TailSegments.Count > 1)
		{
			for (int i = 0; i < TailSegments.Count; i++)
			{
				TailSegments[i].ColliderRadius = GetColliderSphereRadiusFor(i);
			}
		}
	}

	private void BeginCollisionsUpdate()
	{
		if (CollisionSpace != ECollisionSpace.Selective_Fast)
		{
			return;
		}
		RefreshIncludedCollidersDataList();
		CollidersDataToCheck.Clear();
		for (int i = 0; i < IncludedCollidersData.Count; i++)
		{
			if ((Object)(object)IncludedCollidersData[i].Transform == (Object)null)
			{
				forceRefreshCollidersData = true;
				break;
			}
			if (!((Component)IncludedCollidersData[i].Transform).gameObject.activeInHierarchy)
			{
				continue;
			}
			if (CollideWithDisabledColliders)
			{
				IncludedCollidersData[i].RefreshColliderData();
				CollidersDataToCheck.Add(IncludedCollidersData[i]);
			}
			else if (CollisionMode == ECollisionMode.m_3DCollision)
			{
				if ((Object)(object)IncludedCollidersData[i].Collider == (Object)null)
				{
					forceRefreshCollidersData = true;
					break;
				}
				if (IncludedCollidersData[i].Collider.enabled)
				{
					IncludedCollidersData[i].RefreshColliderData();
					CollidersDataToCheck.Add(IncludedCollidersData[i]);
				}
			}
			else
			{
				if ((Object)(object)IncludedCollidersData[i].Collider2D == (Object)null)
				{
					forceRefreshCollidersData = true;
					break;
				}
				if (((Behaviour)IncludedCollidersData[i].Collider2D).enabled)
				{
					IncludedCollidersData[i].RefreshColliderData();
					CollidersDataToCheck.Add(IncludedCollidersData[i]);
				}
			}
		}
	}

	private void SetupSphereColliders()
	{
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0388: Unknown result type (might be due to invalid IL or missing references)
		//IL_040e: Unknown result type (might be due to invalid IL or missing references)
		if (CollisionSpace == ECollisionSpace.World_Slow)
		{
			for (int i = 1; i < _TransformsGhostChain.Count; i++)
			{
				if (CollidersSameLayer)
				{
					((Component)_TransformsGhostChain[i]).gameObject.layer = ((Component)this).gameObject.layer;
				}
				else
				{
					((Component)_TransformsGhostChain[i]).gameObject.layer = CollidersLayer;
				}
			}
			if (CollidersType != 0)
			{
				for (int j = 1; j < _TransformsGhostChain.Count - 1; j++)
				{
					CapsuleCollider val = ((Component)_TransformsGhostChain[j]).gameObject.AddComponent<CapsuleCollider>();
					TailCollisionHelper tailCollisionHelper = ((Component)_TransformsGhostChain[j]).gameObject.AddComponent<TailCollisionHelper>().Init(CollidersAddRigidbody, RigidbodyMass);
					tailCollisionHelper.TailCollider = (Collider)(object)val;
					tailCollisionHelper.Index = j;
					tailCollisionHelper.ParentTail = this;
					val.radius = GetColliderSphereRadiusFor(_TransformsGhostChain, j);
					val.direction = 2;
					Vector3 val2 = _TransformsGhostChain[j].position - _TransformsGhostChain[j + 1].position;
					val.height = ((Vector3)(ref val2)).magnitude * 2f - val.radius;
					val.center = _TransformsGhostChain[j].InverseTransformPoint(Vector3.Lerp(_TransformsGhostChain[j].position, _TransformsGhostChain[j + 1].position, 0.5f));
					TailSegments[j].ColliderRadius = val.radius;
					TailSegments[j].CollisionHelper = tailCollisionHelper;
				}
			}
			else
			{
				for (int k = 1; k < _TransformsGhostChain.Count; k++)
				{
					SphereCollider val3 = ((Component)_TransformsGhostChain[k]).gameObject.AddComponent<SphereCollider>();
					TailCollisionHelper tailCollisionHelper2 = ((Component)_TransformsGhostChain[k]).gameObject.AddComponent<TailCollisionHelper>().Init(CollidersAddRigidbody, RigidbodyMass);
					tailCollisionHelper2.TailCollider = (Collider)(object)val3;
					tailCollisionHelper2.Index = k;
					tailCollisionHelper2.ParentTail = this;
					val3.radius = GetColliderSphereRadiusFor(_TransformsGhostChain, k);
					TailSegments[k].ColliderRadius = val3.radius;
					TailSegments[k].CollisionHelper = tailCollisionHelper2;
				}
			}
		}
		else
		{
			for (int l = 0; l < _TransformsGhostChain.Count; l++)
			{
				TailSegments[l].ColliderRadius = GetColliderSphereRadiusFor(l);
			}
			IncludedCollidersData = new List<FImp_ColliderData_Base>();
			CollidersDataToCheck = new List<FImp_ColliderData_Base>();
			if (DynamicWorldCollidersInclusion)
			{
				if (CollisionMode == ECollisionMode.m_3DCollision)
				{
					for (int m = 0; m < IncludedColliders.Count; m++)
					{
						DynamicAlwaysInclude.Add((Component)(object)IncludedColliders[m]);
					}
				}
				else
				{
					for (int n = 0; n < IncludedColliders2D.Count; n++)
					{
						DynamicAlwaysInclude.Add((Component)(object)IncludedColliders2D[n]);
					}
				}
				Transform transform = TailSegments[TailSegments.Count / 2].transform;
				float num = Vector3.Distance(_TransformsGhostChain[0].position, _TransformsGhostChain[_TransformsGhostChain.Count - 1].position);
				TailCollisionHelper tailCollisionHelper3 = ((Component)transform).gameObject.AddComponent<TailCollisionHelper>();
				tailCollisionHelper3.ParentTail = this;
				SphereCollider val4 = null;
				CircleCollider2D val5 = null;
				if (CollisionMode == ECollisionMode.m_3DCollision)
				{
					val4 = ((Component)transform).gameObject.AddComponent<SphereCollider>();
					((Collider)val4).isTrigger = true;
					tailCollisionHelper3.TailCollider = (Collider)(object)val4;
				}
				else
				{
					val5 = ((Component)transform).gameObject.AddComponent<CircleCollider2D>();
					((Collider2D)val5).isTrigger = true;
					tailCollisionHelper3.TailCollider2D = (Collider2D)(object)val5;
				}
				tailCollisionHelper3.Init(addRigidbody: true, 1f, kinematic: true);
				float num2 = Mathf.Abs(((Component)transform).transform.lossyScale.z);
				if (num2 == 0f)
				{
					num2 = 1f;
				}
				if ((Object)(object)val4 != (Object)null)
				{
					val4.radius = num / num2;
				}
				else
				{
					val5.radius = num / num2;
				}
				if (CollidersSameLayer)
				{
					((Component)transform).gameObject.layer = ((Component)this).gameObject.layer;
				}
				else
				{
					((Component)transform).gameObject.layer = CollidersLayer;
				}
			}
			RefreshIncludedCollidersDataList();
		}
		collisionInitialized = true;
	}

	internal void CollisionDetection(int index, Collision collision)
	{
		TailSegments[index].collisionContacts = collision;
	}

	internal void ExitCollision(int index)
	{
		TailSegments[index].collisionContacts = null;
	}

	protected bool UseCollisionContact(int index, ref Vector3 pos)
	{
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		if (TailSegments[index].collisionContacts == null)
		{
			return false;
		}
		if (TailSegments[index].collisionContacts.contacts.Length == 0)
		{
			return false;
		}
		Collision collisionContacts = TailSegments[index].collisionContacts;
		float num = FImp_ColliderData_Sphere.CalculateTrueRadiusOfSphereCollider(TailSegments[index].transform, TailSegments[index].ColliderRadius) * 0.95f;
		if (Object.op_Implicit((Object)(object)collisionContacts.collider))
		{
			Collider collider = collisionContacts.collider;
			SphereCollider val = (SphereCollider)(object)((collider is SphereCollider) ? collider : null);
			if (Object.op_Implicit((Object)(object)val))
			{
				FImp_ColliderData_Sphere.PushOutFromSphereCollider(val, num, ref pos, Vector3.zero);
			}
			else
			{
				Collider collider2 = collisionContacts.collider;
				CapsuleCollider val2 = (CapsuleCollider)(object)((collider2 is CapsuleCollider) ? collider2 : null);
				if (Object.op_Implicit((Object)(object)val2))
				{
					FImp_ColliderData_Capsule.PushOutFromCapsuleCollider(val2, num, ref pos, Vector3.zero);
				}
				else
				{
					Collider collider3 = collisionContacts.collider;
					BoxCollider val3 = (BoxCollider)(object)((collider3 is BoxCollider) ? collider3 : null);
					if (Object.op_Implicit((Object)(object)val3))
					{
						if (Object.op_Implicit((Object)(object)TailSegments[index].CollisionHelper.RigBody))
						{
							if (Object.op_Implicit((Object)(object)((Collider)val3).attachedRigidbody))
							{
								if (TailSegments[index].CollisionHelper.RigBody.mass > 1f)
								{
									FImp_ColliderData_Box.PushOutFromBoxCollider(val3, collisionContacts, num, ref pos);
									Vector3 val4 = pos;
									FImp_ColliderData_Box.PushOutFromBoxCollider(val3, num, ref pos);
									pos = Vector3.Lerp(pos, val4, TailSegments[index].CollisionHelper.RigBody.mass / 5f);
								}
								else
								{
									FImp_ColliderData_Box.PushOutFromBoxCollider(val3, num, ref pos);
								}
							}
							else
							{
								FImp_ColliderData_Box.PushOutFromBoxCollider(val3, num, ref pos);
							}
						}
						else
						{
							FImp_ColliderData_Box.PushOutFromBoxCollider(val3, num, ref pos);
						}
					}
					else
					{
						Collider collider4 = collisionContacts.collider;
						MeshCollider val5 = (MeshCollider)(object)((collider4 is MeshCollider) ? collider4 : null);
						if (Object.op_Implicit((Object)(object)val5))
						{
							FImp_ColliderData_Mesh.PushOutFromMeshCollider(val5, collisionContacts, num, ref pos);
						}
						else
						{
							Collider collider5 = collisionContacts.collider;
							FImp_ColliderData_Terrain.PushOutFromTerrain((TerrainCollider)(object)((collider5 is TerrainCollider) ? collider5 : null), num, ref pos);
						}
					}
				}
			}
		}
		return true;
	}

	public void RefreshIncludedCollidersDataList()
	{
		bool flag = false;
		if (CollisionMode == ECollisionMode.m_3DCollision)
		{
			if (IncludedColliders.Count != IncludedCollidersData.Count || forceRefreshCollidersData)
			{
				IncludedCollidersData.Clear();
				for (int num = IncludedColliders.Count - 1; num >= 0; num--)
				{
					if ((Object)(object)IncludedColliders[num] == (Object)null)
					{
						IncludedColliders.RemoveAt(num);
					}
					else
					{
						FImp_ColliderData_Base colliderDataFor = FImp_ColliderData_Base.GetColliderDataFor(IncludedColliders[num]);
						IncludedCollidersData.Add(colliderDataFor);
					}
				}
				flag = true;
			}
		}
		else if (IncludedColliders2D.Count != IncludedCollidersData.Count || forceRefreshCollidersData)
		{
			IncludedCollidersData.Clear();
			for (int num2 = IncludedColliders2D.Count - 1; num2 >= 0; num2--)
			{
				if ((Object)(object)IncludedColliders2D[num2] == (Object)null)
				{
					IncludedColliders2D.RemoveAt(num2);
				}
				else
				{
					FImp_ColliderData_Base colliderDataFor2 = FImp_ColliderData_Base.GetColliderDataFor(IncludedColliders2D[num2]);
					IncludedCollidersData.Add(colliderDataFor2);
				}
			}
			flag = true;
		}
		if (flag)
		{
			forceRefreshCollidersData = false;
		}
	}

	public bool PushIfSegmentInsideCollider(TailSegment bone, ref Vector3 targetPoint)
	{
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		if (!CheapCollision)
		{
			for (int i = 0; i < CollidersDataToCheck.Count; i++)
			{
				bool flag2 = CollidersDataToCheck[i].PushIfInside(ref targetPoint, bone.GetRadiusScaled(), Vector3.zero);
				if (!flag && flag2)
				{
					flag = true;
					bone.LatestSelectiveCollision = CollidersDataToCheck[i];
				}
			}
		}
		else
		{
			for (int j = 0; j < CollidersDataToCheck.Count; j++)
			{
				if (CollidersDataToCheck[j].PushIfInside(ref targetPoint, bone.GetRadiusScaled(), Vector3.zero))
				{
					bone.LatestSelectiveCollision = CollidersDataToCheck[j];
					return true;
				}
			}
		}
		return flag;
	}

	protected float GetColliderSphereRadiusFor(int i)
	{
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		_ = TailSegments[i];
		float num = 1f;
		if (i >= _TransformsGhostChain.Count)
		{
			return num;
		}
		if (_TransformsGhostChain.Count > 1)
		{
			num = Vector3.Distance(_TransformsGhostChain[1].position, _TransformsGhostChain[0].position);
		}
		float num2 = num;
		if (i != 0)
		{
			num2 = Mathf.Lerp(num, Vector3.Distance(_TransformsGhostChain[i - 1].position, _TransformsGhostChain[i].position) * 0.5f, CollisionsAutoCurve);
		}
		float num3 = _TransformsGhostChain.Count - 1;
		if (num3 <= 0f)
		{
			num3 = 1f;
		}
		float num4 = 1f / num3;
		return 0.5f * num2 * CollidersScaleMul * CollidersScaleCurve.Evaluate(num4 * (float)i);
	}

	protected float GetColliderSphereRadiusFor(List<Transform> transforms, int i)
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		float num = 1f;
		if (transforms.Count > 1)
		{
			num = Vector3.Distance(_TransformsGhostChain[1].position, _TransformsGhostChain[0].position);
		}
		float num2 = num;
		if (i != 0)
		{
			num2 = Vector3.Distance(_TransformsGhostChain[i - 1].position, _TransformsGhostChain[i].position);
		}
		float num3 = Mathf.Lerp(num, num2 * 0.5f, CollisionsAutoCurve);
		float num4 = 1f / (float)(transforms.Count - 1);
		return 0.5f * num3 * CollidersScaleMul * CollidersScaleCurve.Evaluate(num4 * (float)i);
	}

	public void AddCollider(Collider collider)
	{
		if (!IncludedColliders.Contains(collider))
		{
			IncludedColliders.Add(collider);
		}
	}

	public void AddCollider(Collider2D collider)
	{
		if (!IncludedColliders2D.Contains(collider))
		{
			IncludedColliders2D.Add(collider);
		}
	}

	public void CheckForColliderDuplicatesAndNulls()
	{
		for (int i = 0; i < IncludedColliders.Count; i++)
		{
			Collider col = IncludedColliders[i];
			if (IncludedColliders.Count((Collider o) => (Object)(object)o == (Object)(object)col) > 1)
			{
				IncludedColliders.RemoveAll((Collider o) => (Object)(object)o == (Object)(object)col);
				IncludedColliders.Add(col);
			}
		}
		for (int num = IncludedColliders.Count - 1; num >= 0; num--)
		{
			if ((Object)(object)IncludedColliders[num] == (Object)null)
			{
				IncludedColliders.RemoveAt(num);
			}
		}
	}

	public void CheckForColliderDuplicatesAndNulls2D()
	{
		for (int i = 0; i < IncludedColliders2D.Count; i++)
		{
			Collider2D col = IncludedColliders2D[i];
			if (IncludedColliders2D.Count((Collider2D o) => (Object)(object)o == (Object)(object)col) > 1)
			{
				IncludedColliders2D.RemoveAll((Collider2D o) => (Object)(object)o == (Object)(object)col);
				IncludedColliders2D.Add(col);
			}
		}
	}

	private void TailCalculations_ComputeSegmentCollisions(TailSegment bone, ref Vector3 position)
	{
		if (bone.CollisionContactFlag)
		{
			bone.CollisionContactFlag = false;
		}
		else if (bone.CollisionContactRelevancy > 0f)
		{
			bone.CollisionContactRelevancy -= justDelta;
		}
		if (CollisionSpace == ECollisionSpace.Selective_Fast)
		{
			if (PushIfSegmentInsideCollider(bone, ref position))
			{
				bone.CollisionContactFlag = true;
				bone.CollisionContactRelevancy = justDelta * 7f;
				bone.ChildBone.CollisionContactRelevancy = Mathf.Max(bone.ChildBone.CollisionContactRelevancy, justDelta * 3.5f);
				if (bone.ChildBone.ChildBone != null)
				{
					bone.ChildBone.ChildBone.CollisionContactRelevancy = Mathf.Max(bone.ChildBone.CollisionContactRelevancy, justDelta * 3f);
				}
			}
		}
		else if (UseCollisionContact(bone.Index, ref position))
		{
			bone.CollisionContactFlag = true;
			bone.CollisionContactRelevancy = justDelta * 7f;
			bone.ChildBone.CollisionContactRelevancy = Mathf.Max(bone.ChildBone.CollisionContactRelevancy, justDelta * 3.5f);
			if (bone.ChildBone.ChildBone != null)
			{
				bone.ChildBone.ChildBone.CollisionContactRelevancy = Mathf.Max(bone.ChildBone.CollisionContactRelevancy, justDelta * 3f);
			}
		}
	}

	private void ExpertParamsUpdate()
	{
		Expert_UpdatePosSpeed();
		Expert_UpdateRotSpeed();
		Expert_UpdateSpringiness();
		Expert_UpdateSlithery();
		Expert_UpdateCurling();
		Expert_UpdateSlippery();
		Expert_UpdateBlending();
	}

	private void ExpertCurvesEndUpdate()
	{
		lastPosSpeeds = ReactionSpeed;
		if (!UsePosSpeedCurve && lastPosCurvKeys != null)
		{
			lastPosCurvKeys = null;
			lastPosSpeeds += 0.001f;
		}
		lastRotSpeeds = RotationRelevancy;
		if (!UseRotSpeedCurve && lastRotCurvKeys != null)
		{
			lastRotCurvKeys = null;
			lastRotSpeeds += 0.001f;
		}
		lastSpringiness = Springiness;
		if (!UseSpringCurve && lastSpringCurvKeys != null)
		{
			lastSpringCurvKeys = null;
			lastSpringiness += 0.001f;
		}
		lastSlithery = Slithery;
		if (!UseSlitheryCurve && lastSlitheryCurvKeys != null)
		{
			lastSlitheryCurvKeys = null;
			lastSlithery += 0.001f;
		}
		lastCurling = Curling;
		if (!UseCurlingCurve && lastCurlingCurvKeys != null)
		{
			lastCurlingCurvKeys = null;
			lastCurling += 0.001f;
		}
		lastSlippery = CollisionSlippery;
		if (!UseSlipperyCurve && lastSlipperyCurvKeys != null)
		{
			lastSlipperyCurvKeys = null;
			lastSlippery += 0.001f;
		}
		lastTailAnimatorAmount = TailAnimatorAmount;
		if (!UsePartialBlend && lastBlendCurvKeys != null)
		{
			lastBlendCurvKeys = null;
			lastTailAnimatorAmount += 0.001f;
		}
	}

	private void Expert_UpdatePosSpeed()
	{
		if (UsePosSpeedCurve)
		{
			for (_ex_bone = TailSegments[0]; _ex_bone != null; _ex_bone = _ex_bone.ChildBone)
			{
				_ex_bone.PositionSpeed = PosCurve.Evaluate(_ex_bone.IndexOverlLength);
			}
		}
		else if (lastPosSpeeds != ReactionSpeed)
		{
			for (_ex_bone = TailSegments[0]; _ex_bone != null; _ex_bone = _ex_bone.ChildBone)
			{
				_ex_bone.PositionSpeed = ReactionSpeed;
			}
		}
	}

	private void Expert_UpdateRotSpeed()
	{
		if (UseRotSpeedCurve)
		{
			for (_ex_bone = TailSegments[0]; _ex_bone != null; _ex_bone = _ex_bone.ChildBone)
			{
				_ex_bone.RotationSpeed = RotCurve.Evaluate(_ex_bone.IndexOverlLength);
			}
		}
		else if (lastRotSpeeds != RotationRelevancy)
		{
			for (_ex_bone = TailSegments[0]; _ex_bone != null; _ex_bone = _ex_bone.ChildBone)
			{
				_ex_bone.RotationSpeed = RotationRelevancy;
			}
		}
	}

	private void Expert_UpdateSpringiness()
	{
		if (UseSpringCurve)
		{
			for (_ex_bone = TailSegments[0]; _ex_bone != null; _ex_bone = _ex_bone.ChildBone)
			{
				_ex_bone.Springiness = SpringCurve.Evaluate(_ex_bone.IndexOverlLength);
			}
		}
		else if (lastSpringiness != Springiness)
		{
			for (_ex_bone = TailSegments[0]; _ex_bone != null; _ex_bone = _ex_bone.ChildBone)
			{
				_ex_bone.Springiness = Springiness;
			}
		}
	}

	private void Expert_UpdateSlithery()
	{
		if (UseSlitheryCurve)
		{
			for (_ex_bone = TailSegments[0]; _ex_bone != null; _ex_bone = _ex_bone.ChildBone)
			{
				_ex_bone.Slithery = SlitheryCurve.Evaluate(_ex_bone.IndexOverlLength);
			}
		}
		else if (lastSlithery != Slithery)
		{
			for (_ex_bone = TailSegments[0]; _ex_bone != null; _ex_bone = _ex_bone.ChildBone)
			{
				_ex_bone.Slithery = Slithery;
			}
		}
	}

	private void Expert_UpdateCurling()
	{
		if (UseCurlingCurve)
		{
			for (_ex_bone = TailSegments[0]; _ex_bone != null; _ex_bone = _ex_bone.ChildBone)
			{
				_ex_bone.Curling = CurlingCurve.Evaluate(_ex_bone.IndexOverlLength);
			}
		}
		else if (lastCurling != Curling)
		{
			for (_ex_bone = TailSegments[0]; _ex_bone != null; _ex_bone = _ex_bone.ChildBone)
			{
				_ex_bone.Curling = Curling;
			}
		}
	}

	private void Expert_UpdateSlippery()
	{
		if (UseSlipperyCurve)
		{
			for (_ex_bone = TailSegments[0]; _ex_bone != null; _ex_bone = _ex_bone.ChildBone)
			{
				_ex_bone.Slippery = SlipperyCurve.Evaluate(_ex_bone.IndexOverlLength);
			}
		}
		else if (lastSlippery != CollisionSlippery)
		{
			for (_ex_bone = TailSegments[0]; _ex_bone != null; _ex_bone = _ex_bone.ChildBone)
			{
				_ex_bone.Slippery = CollisionSlippery;
			}
		}
	}

	private void Expert_UpdateBlending()
	{
		if (UsePartialBlend)
		{
			for (_ex_bone = TailSegments[0]; _ex_bone != null; _ex_bone = _ex_bone.ChildBone)
			{
				_ex_bone.BlendValue = BlendCurve.Evaluate(_ex_bone.IndexOverlLength);
			}
		}
		else if (lastTailAnimatorAmount != TailAnimatorAmount)
		{
			for (_ex_bone = TailSegments[0]; _ex_bone != null; _ex_bone = _ex_bone.ChildBone)
			{
				_ex_bone.BlendValue = TailAnimatorAmount;
			}
		}
	}

	private void InitIK()
	{
		if (!IKSelectiveChain)
		{
			IK = new FIK_CCDProcessor(_TransformsGhostChain.ToArray());
		}
		else
		{
			List<Transform> list = new List<Transform>();
			if (IKLimitSettings.Count != _TransformsGhostChain.Count)
			{
				list = _TransformsGhostChain;
			}
			else
			{
				for (int i = 0; i < _TransformsGhostChain.Count; i++)
				{
					if (IKLimitSettings[i].UseInChain)
					{
						list.Add(_TransformsGhostChain[i]);
					}
				}
			}
			IK = new FIK_CCDProcessor(list.ToArray());
		}
		if (IKAutoWeights)
		{
			IK.AutoWeightBones(IKBaseReactionWeight);
		}
		else
		{
			IK.AutoWeightBones(IKReactionWeightCurve);
		}
		if (IKAutoAngleLimits)
		{
			IK.AutoLimitAngle(IKAutoAngleLimit, 4f + IKAutoAngleLimit / 15f);
		}
		if (!IKSelectiveChain)
		{
			IK.Init(_TransformsGhostChain[0]);
		}
		else
		{
			IK.Init(IK.Bones[0].transform);
		}
		ikInitialized = true;
		IK_ApplyLimitBoneSettings();
	}

	public void IKSetCustomPosition(Vector3? tgt)
	{
		_IKCustomPos = tgt;
	}

	private void UpdateIK()
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		if (!ikInitialized)
		{
			InitIK();
		}
		if (IKBlend <= Mathf.Epsilon)
		{
			return;
		}
		if (_IKCustomPos.HasValue)
		{
			IK.IKTargetPosition = _IKCustomPos.Value;
		}
		else if ((Object)(object)IKTarget == (Object)null)
		{
			IK.IKTargetPosition = TailSegments[TailSegments.Count - 1].ProceduralPosition;
		}
		else
		{
			IK.IKTargetPosition = IKTarget.position;
		}
		IK.Invert = IkInvertOrder;
		IK.IKWeight = IKBlend;
		IK.SyncWithAnimator = IKAnimatorBlend;
		IK.ReactionQuality = IKReactionQuality;
		IK.Smoothing = IKSmoothing;
		IK.StretchToTarget = IKStretchToTarget;
		IK.StretchCurve = IKStretchCurve;
		IK.ContinousSolving = IKContinousSolve;
		if (IK.StretchToTarget > 0f)
		{
			IK.ContinousSolving = false;
		}
		if (Axis2D == 3)
		{
			IK.Use2D = true;
		}
		else
		{
			IK.Use2D = false;
		}
		IK.Update();
		if (DetachChildren)
		{
			TailSegment tailSegment = TailSegments[0];
			tailSegment = TailSegments[1];
			if (!IncludeParent)
			{
				tailSegment.RefreshKeyLocalPositionAndRotation(tailSegment.InitialLocalPosition, tailSegment.InitialLocalRotation);
				tailSegment = TailSegments[2];
			}
			while (tailSegment != GhostChild)
			{
				tailSegment.RefreshKeyLocalPositionAndRotation(tailSegment.InitialLocalPosition, tailSegment.InitialLocalRotation);
				tailSegment = tailSegment.ChildBone;
			}
		}
		else
		{
			for (TailSegment tailSegment2 = TailSegments[0]; tailSegment2 != GhostChild; tailSegment2 = tailSegment2.ChildBone)
			{
				tailSegment2.RefreshKeyLocalPositionAndRotation();
			}
		}
	}

	public void IK_ApplyLimitBoneSettings()
	{
		if (!IKAutoAngleLimits)
		{
			if (IKLimitSettings.Count != _TransformsGhostChain.Count)
			{
				IK_RefreshLimitSettingsContainer();
			}
			if (IK.IKBones.Length != IKLimitSettings.Count)
			{
				Debug.Log((object)"[TAIL ANIMATOR IK] Wrong IK bone count!");
				return;
			}
			if (!IKAutoAngleLimits)
			{
				for (int i = 0; i < IKLimitSettings.Count; i++)
				{
					IK.IKBones[i].AngleLimit = IKLimitSettings[i].AngleLimit;
					IK.IKBones[i].TwistAngleLimit = IKLimitSettings[i].TwistAngleLimit;
				}
			}
		}
		if (ikInitialized)
		{
			if (IKAutoWeights)
			{
				IK.AutoWeightBones(IKBaseReactionWeight);
			}
			else
			{
				IK.AutoWeightBones(IKReactionWeightCurve);
			}
		}
		if (IKAutoAngleLimits)
		{
			IK.AutoLimitAngle(IKAutoAngleLimit, 10f + IKAutoAngleLimit / 10f);
		}
	}

	public void IK_RefreshLimitSettingsContainer()
	{
		IKLimitSettings = new List<IKBoneSettings>();
		for (int i = 0; i < _TransformsGhostChain.Count; i++)
		{
			IKLimitSettings.Add(new IKBoneSettings());
		}
	}

	private bool PostProcessingNeeded()
	{
		if (Deflection > Mathf.Epsilon)
		{
			return true;
		}
		return false;
	}

	private void PostProcessing_Begin()
	{
		TailSegments_UpdateCoordsForRootBone(_pp_reference[_tc_startI]);
		if (Deflection > Mathf.Epsilon)
		{
			Deflection_BeginUpdate();
		}
	}

	private void PostProcessing_ReferenceUpdate()
	{
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		TailSegment tailSegment;
		for (tailSegment = _pp_reference[_tc_startI]; tailSegment != _pp_ref_lastChild; tailSegment = tailSegment.ChildBone)
		{
			tailSegment.ParamsFrom(TailSegments[tailSegment.Index]);
			TailSegment_PrepareVelocity(tailSegment);
		}
		TailSegment_PrepareMotionParameters(_pp_ref_lastChild);
		TailSegment_PrepareVelocity(_pp_ref_lastChild);
		tailSegment = _pp_reference[_tc_startII];
		if (!DetachChildren)
		{
			while (tailSegment != _pp_ref_lastChild)
			{
				TailSegment_PrepareRotation(tailSegment);
				TailSegment_BaseSwingProcessing(tailSegment);
				TailCalculations_SegmentPreProcessingStack(tailSegment);
				TailSegment_PreRotationPositionBlend(tailSegment);
				tailSegment = tailSegment.ChildBone;
			}
		}
		else
		{
			while (tailSegment != _pp_ref_lastChild)
			{
				TailSegment_PrepareRotationDetached(tailSegment);
				TailSegment_BaseSwingProcessing(tailSegment);
				TailCalculations_SegmentPreProcessingStack(tailSegment);
				TailSegment_PreRotationPositionBlend(tailSegment);
				tailSegment = tailSegment.ChildBone;
			}
		}
		TailCalculations_UpdateArtificialChildBone(_pp_ref_lastChild);
		for (tailSegment = _pp_reference[_tc_startII]; tailSegment != _pp_ref_lastChild; tailSegment = tailSegment.ChildBone)
		{
			TailCalculations_SegmentRotation(tailSegment, tailSegment.LastKeyframeLocalPosition);
		}
		TailCalculations_SegmentRotation(tailSegment, tailSegment.LastKeyframeLocalPosition);
		tailSegment.ParentBone.RefreshFinalRot(tailSegment.ParentBone.TrueTargetRotation);
	}

	private void ShapingParamsUpdate()
	{
		Shaping_UpdateCurving();
		Shaping_UpdateGravity();
		Shaping_UpdateLengthMultiplier();
	}

	private void Shaping_UpdateCurving()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		if (!FEngineering.QIsZero(Curving))
		{
			if (UseCurvingCurve)
			{
				for (_ex_bone = TailSegments[0]; _ex_bone != null; _ex_bone = _ex_bone.ChildBone)
				{
					_ex_bone.Curving = Quaternion.LerpUnclamped(Quaternion.identity, Curving, CurvCurve.Evaluate(_ex_bone.IndexOverlLength));
				}
			}
			else if (!FEngineering.QIsSame(Curving, lastCurving))
			{
				for (int i = 0; i < TailSegments.Count; i++)
				{
					TailSegments[i].Curving = Curving;
				}
			}
		}
		else if (!FEngineering.QIsSame(Curving, lastCurving))
		{
			for (int j = 0; j < TailSegments.Count; j++)
			{
				TailSegments[j].Curving = Quaternion.identity;
			}
		}
	}

	private void Shaping_UpdateGravity()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		if (!FEngineering.VIsZero(Gravity))
		{
			if (UseGravityCurve)
			{
				for (_ex_bone = TailSegments[0]; _ex_bone != null; _ex_bone = _ex_bone.ChildBone)
				{
					_ex_bone.Gravity = Gravity * 40f * GravityCurve.Evaluate(_ex_bone.IndexOverlLength);
				}
			}
			else if (!FEngineering.VIsSame(Gravity, lastGravity))
			{
				for (int i = 0; i < TailSegments.Count; i++)
				{
					TailSegments[i].Gravity = Gravity / 40f;
					TailSegment tailSegment = TailSegments[i];
					tailSegment.Gravity *= 1f + (float)TailSegments[i].Index / 2f * (1f - TailSegments[i].Slithery);
				}
			}
		}
		else if (!FEngineering.VIsSame(Gravity, lastGravity))
		{
			for (int j = 0; j < TailSegments.Count; j++)
			{
				TailSegments[j].Gravity = Vector3.zero;
				TailSegments[j].GravityLookOffset = Vector3.zero;
			}
		}
	}

	private void Shaping_UpdateLengthMultiplier()
	{
		if (UseLengthMulCurve)
		{
			for (_ex_bone = TailSegments[0]; _ex_bone != null; _ex_bone = _ex_bone.ChildBone)
			{
				_ex_bone.LengthMultiplier = LengthMulCurve.Evaluate(_ex_bone.IndexOverlLength);
			}
		}
		else if (lastLengthMul != LengthMultiplier)
		{
			for (int i = 0; i < TailSegments.Count; i++)
			{
				TailSegments[i].LengthMultiplier = LengthMultiplier;
			}
		}
	}

	private void ShapingEndUpdate()
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		lastCurving = Curving;
		if (!UseCurvingCurve && lastCurvingKeys != null)
		{
			lastCurvingKeys = null;
			lastCurving.x += 0.001f;
		}
		lastGravity = Gravity;
		if (!UseGravityCurve && lastGravityKeys != null)
		{
			lastGravityKeys = null;
			lastGravity.x += 0.001f;
		}
		lastLengthMul = LengthMultiplier;
		if (!UseLengthMulCurve && lastLengthKeys != null)
		{
			lastLengthKeys = null;
			lastLengthMul += 0.0001f;
		}
	}

	private void Waving_Initialize()
	{
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		if (FixedCycle == 0f)
		{
			_waving_waveTime = Random.Range(-MathF.PI, MathF.PI) * 100f;
			_waving_cosTime = Random.Range(-MathF.PI, MathF.PI) * 50f;
		}
		else
		{
			_waving_waveTime = FixedCycle;
			_waving_cosTime = FixedCycle;
		}
		_waving_sustain = Vector3.zero;
	}

	private void Waving_AutoSwingUpdate()
	{
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_016c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		_waving_waveTime += justDelta * (2f * WavingSpeed);
		if (WavingType == FEWavingType.Simple)
		{
			float num = Mathf.Sin(_waving_waveTime) * (30f * WavingRange);
			if (CosinusAdd)
			{
				_waving_cosTime += justDelta * (2.535f * WavingSpeed);
				num += Mathf.Cos(_waving_cosTime) * (27f * WavingRange);
			}
			WavingRotationOffset = Quaternion.Euler(num * WavingAxis * TailSegments[0].BlendValue);
		}
		else
		{
			float num2 = _waving_waveTime * 0.23f;
			float num3 = AlternateWave * -5f;
			float num4 = AlternateWave * 100f;
			float num5 = AlternateWave * 20f;
			float num6 = Mathf.PerlinNoise(num2, num3) * 2f - 1f;
			float num7 = Mathf.PerlinNoise(num4 + num2, num2 + num4) * 2f - 1f;
			float num8 = Mathf.PerlinNoise(num5, num2) * 2f - 1f;
			WavingRotationOffset = Quaternion.Euler(Vector3.Scale(WavingAxis * WavingRange * 35f * TailSegments[0].BlendValue, new Vector3(num6, num7, num8)));
		}
	}

	private void Waving_SustainUpdate()
	{
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0182: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		TailSegment tailSegment = TailSegments[0];
		float num = _TC_TailLength / (float)TailSegments.Count;
		num = Mathf.Pow(num, 1.65f);
		num = _sg_curly / num / 6f;
		if (num < 0.1f)
		{
			num = 0.1f;
		}
		else if (num > 1f)
		{
			num = 1f;
		}
		int num2 = (int)Mathf.LerpUnclamped((float)TailSegments.Count * 0.4f, (float)TailSegments.Count * 0.6f, Sustain);
		float end = FEasing.EaseOutExpo(1f, 0.09f, Sustain);
		float num3 = 1.5f;
		num3 *= 1f - TailSegments[0].Curling / 8f;
		num3 *= 1.5f - num / 1.65f;
		num3 *= Mathf.Lerp(0.7f, 1.2f, tailSegment.Slithery);
		num3 *= FEasing.EaseOutExpo(1f, end, tailSegment.Springiness);
		Vector3 val = TailSegments[num2].PreviousPush;
		if (num2 + 1 < TailSegments.Count)
		{
			val += TailSegments[num2 + 1].PreviousPush;
		}
		if (num2 - 1 > TailSegments.Count)
		{
			val += TailSegments[num2 - 1].PreviousPush;
		}
		_waving_sustain = val * Sustain * num3 * 2f;
	}

	private void WindEffectUpdate()
	{
		if (Object.op_Implicit((Object)(object)TailAnimatorWind.Instance))
		{
			TailAnimatorWind.Instance.AffectTailWithWind(this);
		}
	}

	protected virtual void Init()
	{
		//IL_0571: Unknown result type (might be due to invalid IL or missing references)
		//IL_0576: Unknown result type (might be due to invalid IL or missing references)
		//IL_057c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03de: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0404: Unknown result type (might be due to invalid IL or missing references)
		//IL_040f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0414: Unknown result type (might be due to invalid IL or missing references)
		//IL_042b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0430: Unknown result type (might be due to invalid IL or missing references)
		//IL_0435: Unknown result type (might be due to invalid IL or missing references)
		//IL_044b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0461: Unknown result type (might be due to invalid IL or missing references)
		//IL_0477: Unknown result type (might be due to invalid IL or missing references)
		//IL_047c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0488: Unknown result type (might be due to invalid IL or missing references)
		//IL_048d: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0343: Unknown result type (might be due to invalid IL or missing references)
		//IL_0348: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_024b: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0385: Unknown result type (might be due to invalid IL or missing references)
		//IL_038a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0394: Unknown result type (might be due to invalid IL or missing references)
		//IL_0399: Unknown result type (might be due to invalid IL or missing references)
		//IL_0366: Unknown result type (might be due to invalid IL or missing references)
		//IL_0371: Unknown result type (might be due to invalid IL or missing references)
		//IL_0376: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Unknown result type (might be due to invalid IL or missing references)
		if (initialized)
		{
			return;
		}
		if (_TransformsGhostChain == null || _TransformsGhostChain.Count == 0)
		{
			_TransformsGhostChain = new List<Transform>();
			GetGhostChain();
		}
		TailSegments = new List<TailSegment>();
		for (int i = 0; i < _TransformsGhostChain.Count; i++)
		{
			if ((Object)(object)_TransformsGhostChain[i] == (Object)null)
			{
				Debug.Log((object)("[Tail Animator] Null bones in " + ((Object)this).name + " !"));
				continue;
			}
			TailSegment tailSegment = new TailSegment(_TransformsGhostChain[i]);
			tailSegment.SetIndex(i, _TransformsGhostChain.Count);
			TailSegments.Add(tailSegment);
		}
		if (TailSegments.Count == 0)
		{
			Debug.Log((object)("[Tail Animator] Could not create tail bones chain in " + ((Object)this).name + " !"));
			return;
		}
		_TC_TailLength = 0f;
		_baseTransform = _TransformsGhostChain[0];
		for (int j = 0; j < TailSegments.Count; j++)
		{
			TailSegment tailSegment2 = TailSegments[j];
			TailSegment tailSegment3;
			if (j == 0)
			{
				if (Object.op_Implicit((Object)(object)tailSegment2.transform.parent))
				{
					tailSegment3 = new TailSegment(tailSegment2.transform.parent);
					tailSegment3.SetParentRef(new TailSegment(tailSegment3.transform.parent));
				}
				else
				{
					tailSegment3 = new TailSegment(tailSegment2.transform);
					Vector3 val;
					if (_TransformsGhostChain.Count > 1)
					{
						val = _TransformsGhostChain[0].position - _TransformsGhostChain[1].position;
						if (((Vector3)(ref val)).magnitude == 0f)
						{
							val = ((Component)this).transform.position - _TransformsGhostChain[1].position;
						}
					}
					else
					{
						val = tailSegment2.transform.position - _TransformsGhostChain[0].position;
					}
					if (((Vector3)(ref val)).magnitude == 0f)
					{
						val = ((Component)this).transform.position - _TransformsGhostChain[0].position;
					}
					if (((Vector3)(ref val)).magnitude == 0f)
					{
						val = ((Component)this).transform.forward;
					}
					tailSegment3.LocalOffset = tailSegment3.transform.InverseTransformPoint(tailSegment3.transform.position + val);
					tailSegment3.SetParentRef(new TailSegment(tailSegment2.transform));
				}
				GhostParent = tailSegment3;
				GhostParent.Validate();
				tailSegment2.SetParentRef(GhostParent);
			}
			else
			{
				tailSegment3 = TailSegments[j - 1];
				tailSegment2.ReInitializeLocalPosRot(tailSegment3.transform.InverseTransformPoint(tailSegment2.transform.position), tailSegment2.transform.localRotation);
			}
			if (j == TailSegments.Count - 1)
			{
				Transform transform = null;
				if (tailSegment2.transform.childCount > 0)
				{
					transform = tailSegment2.transform.GetChild(0);
				}
				GhostChild = new TailSegment(transform);
				Vector3 val2 = ((!FEngineering.VIsZero(EndBoneJointOffset)) ? tailSegment2.transform.TransformVector(EndBoneJointOffset) : (Object.op_Implicit((Object)(object)tailSegment2.transform.parent) ? (tailSegment2.transform.position - tailSegment2.transform.parent.position) : ((tailSegment2.transform.childCount <= 0) ? (tailSegment2.transform.TransformDirection(Vector3.forward) * 0.05f) : (tailSegment2.transform.GetChild(0).position - tailSegment2.transform.position))));
				GhostChild.ProceduralPosition = tailSegment2.transform.position + val2;
				GhostChild.ProceduralPositionWeightBlended = GhostChild.ProceduralPosition;
				GhostChild.PreviousPosition = GhostChild.ProceduralPosition;
				GhostChild.PosRefRotation = Quaternion.identity;
				GhostChild.PreviousPosReferenceRotation = Quaternion.identity;
				GhostChild.ReInitializeLocalPosRot(tailSegment2.transform.InverseTransformPoint(GhostChild.ProceduralPosition), Quaternion.identity);
				GhostChild.RefreshFinalPos(GhostChild.ProceduralPosition);
				GhostChild.RefreshFinalRot(GhostChild.PosRefRotation);
				GhostChild.TrueTargetRotation = GhostChild.PosRefRotation;
				tailSegment2.TrueTargetRotation = tailSegment2.transform.rotation;
				tailSegment2.SetChildRef(GhostChild);
				GhostChild.SetParentRef(tailSegment2);
			}
			else
			{
				tailSegment2.SetChildRef(TailSegments[j + 1]);
			}
			tailSegment2.SetParentRef(tailSegment3);
			_TC_TailLength += Vector3.Distance(tailSegment2.ProceduralPosition, tailSegment3.ProceduralPosition);
			if ((Object)(object)tailSegment2.transform != (Object)(object)_baseTransform)
			{
				tailSegment2.AssignDetachedRootCoords(BaseTransform);
			}
		}
		GhostParent.SetIndex(-1, TailSegments.Count);
		GhostChild.SetIndex(TailSegments.Count, TailSegments.Count);
		GhostParent.SetChildRef(TailSegments[0]);
		previousWorldPosition = BaseTransform.position;
		WavingRotationOffset = Quaternion.identity;
		if (CollidersDataToCheck == null)
		{
			CollidersDataToCheck = new List<FImp_ColliderData_Base>();
		}
		DynamicAlwaysInclude = new List<Component>();
		if (UseCollision)
		{
			SetupSphereColliders();
		}
		if (_defl_source == null)
		{
			_defl_source = new List<TailSegment>();
		}
		Waving_Initialize();
		if (DetachChildren)
		{
			DetachChildrenTransforms();
		}
		initialized = true;
		if (TailSegments.Count == 1 && (Object)(object)TailSegments[0].transform.parent == (Object)null)
		{
			Debug.Log((object)"[Tail Animator] Can't initialize one-bone length chain on bone which don't have any parent!");
			Debug.LogError((object)"[Tail Animator] Can't initialize one-bone length chain on bone which don't have any parent!");
			TailAnimatorAmount = 0f;
			initialized = false;
			return;
		}
		if (UseWind)
		{
			TailAnimatorWind.Refresh();
		}
		if (PostProcessingNeeded() && !_pp_initialized)
		{
			InitializePostProcessing();
		}
		if (!Prewarm)
		{
			return;
		}
		ShapingParamsUpdate();
		ExpertParamsUpdate();
		Tick();
		LateTick();
		justDelta = rateDelta;
		secPeriodDelta = 1f;
		deltaForLerps = secPeriodDelta;
		rateDelta = 1f / 60f;
		CheckIfTailAnimatorShouldBeUpdated();
		if (updateTailAnimator)
		{
			int num = 60 + TailSegments.Count / 2;
			for (int k = 0; k < num; k++)
			{
				PreCalibrateBones();
				LateTick();
			}
		}
	}

	public void DetachChildrenTransforms()
	{
		int num = ((!IncludeParent) ? 1 : 0);
		for (int num2 = TailSegments.Count - 1; num2 >= num; num2--)
		{
			if (Object.op_Implicit((Object)(object)TailSegments[num2].transform))
			{
				TailSegments[num2].transform.DetachChildren();
			}
		}
	}

	private void InitializePostProcessing()
	{
		_pp_reference = new List<TailSegment>();
		_pp_ref_rootParent = new TailSegment(GhostParent);
		for (int i = 0; i < TailSegments.Count; i++)
		{
			TailSegment item = new TailSegment(TailSegments[i]);
			_pp_reference.Add(item);
		}
		_pp_ref_lastChild = new TailSegment(GhostChild);
		_pp_ref_rootParent.SetChildRef(_pp_reference[0]);
		_pp_ref_rootParent.SetParentRef(new TailSegment(GhostParent.ParentBone.transform));
		for (int j = 0; j < _pp_reference.Count; j++)
		{
			TailSegment tailSegment = _pp_reference[j];
			tailSegment.SetIndex(j, TailSegments.Count);
			if (j == 0)
			{
				tailSegment.SetParentRef(_pp_ref_rootParent);
				tailSegment.SetChildRef(_pp_reference[j + 1]);
			}
			else if (j == _pp_reference.Count - 1)
			{
				tailSegment.SetParentRef(_pp_reference[j - 1]);
				tailSegment.SetChildRef(_pp_ref_lastChild);
			}
			else
			{
				tailSegment.SetParentRef(_pp_reference[j - 1]);
				tailSegment.SetChildRef(_pp_reference[j + 1]);
			}
		}
		_pp_ref_lastChild.SetParentRef(_pp_reference[_pp_reference.Count - 1]);
		_pp_initialized = true;
	}

	protected void StretchingLimiting(TailSegment bone)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0144: Unknown result type (might be due to invalid IL or missing references)
		//IL_0149: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0170: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0108: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = bone.ParentBone.ProceduralPosition - bone.ProceduralPosition;
		float magnitude = ((Vector3)(ref val)).magnitude;
		if (!(magnitude > 0f))
		{
			return;
		}
		float num = bone.BoneLengthScaled + bone.BoneLengthScaled * 2.5f * MaxStretching;
		if (magnitude > num)
		{
			if (MaxStretching == 0f)
			{
				_limiting_limitPosition = bone.ProceduralPosition + val * ((magnitude - bone.BoneLengthScaled) / magnitude);
				bone.ProceduralPosition = _limiting_limitPosition;
				return;
			}
			_limiting_limitPosition = bone.ParentBone.ProceduralPosition - ((Vector3)(ref val)).normalized * num;
			float num2 = Mathf.InverseLerp(magnitude, 0f, num) + _limiting_stretchingHelperTooLong;
			if (num2 > 0.999f)
			{
				num2 = 0.99f;
			}
			if (ReactionSpeed < 0.5f)
			{
				num2 *= deltaForLerps * (10f + ReactionSpeed * 30f);
			}
			bone.ProceduralPosition = Vector3.Lerp(bone.ProceduralPosition, _limiting_limitPosition, num2);
			return;
		}
		num = bone.BoneLengthScaled + bone.BoneLengthScaled * 1.1f * MaxStretching;
		if (magnitude < num)
		{
			_limiting_limitPosition = bone.ProceduralPosition + val * ((magnitude - bone.BoneLengthScaled) / magnitude);
			if (MaxStretching == 0f)
			{
				bone.ProceduralPosition = _limiting_limitPosition;
			}
			else
			{
				bone.ProceduralPosition = Vector3.LerpUnclamped(bone.ProceduralPosition, _limiting_limitPosition, Mathf.InverseLerp(magnitude, 0f, num) + _limiting_stretchingHelperTooShort);
			}
		}
	}

	protected Vector3 AngleLimiting(TailSegment child, Vector3 targetPos)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_027c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0276: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		//IL_0227: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0231: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_024e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		_limiting_limitPosition = targetPos;
		_limiting_angle_ToTargetRot = Quaternion.FromToRotation(child.ParentBone.transform.TransformDirection(child.LastKeyframeLocalPosition), targetPos - child.ParentBone.ProceduralPosition) * child.ParentBone.transform.rotation;
		_limiting_angle_targetInLocal = FEngineering.QToLocal(child.ParentBone.transform.rotation, _limiting_angle_ToTargetRot);
		float num2 = 0f;
		if (((Vector3)(ref AngleLimitAxis)).sqrMagnitude == 0f)
		{
			num2 = Quaternion.Angle(_limiting_angle_targetInLocal, child.LastKeyframeLocalRotation);
		}
		else
		{
			((Vector3)(ref AngleLimitAxis)).Normalize();
			Vector3 val;
			if (LimitAxisRange.x == LimitAxisRange.y)
			{
				val = Vector3.Scale(((Quaternion)(ref child.InitialLocalRotation)).eulerAngles, AngleLimitAxis);
				float magnitude = ((Vector3)(ref val)).magnitude;
				val = Vector3.Scale(((Quaternion)(ref _limiting_angle_targetInLocal)).eulerAngles, AngleLimitAxis);
				num2 = Mathf.DeltaAngle(magnitude, ((Vector3)(ref val)).magnitude);
				if (num2 < 0f)
				{
					num2 = 0f - num2;
				}
			}
			else
			{
				val = Vector3.Scale(((Quaternion)(ref child.InitialLocalRotation)).eulerAngles, AngleLimitAxis);
				float magnitude2 = ((Vector3)(ref val)).magnitude;
				val = Vector3.Scale(((Quaternion)(ref _limiting_angle_targetInLocal)).eulerAngles, AngleLimitAxis);
				num2 = Mathf.DeltaAngle(magnitude2, ((Vector3)(ref val)).magnitude);
				if (num2 > LimitAxisRange.x && num2 < LimitAxisRange.y)
				{
					num2 = 0f;
				}
				if (num2 < 0f)
				{
					num2 = 0f - num2;
				}
			}
		}
		if (num2 > AngleLimit)
		{
			float num3 = Mathf.Abs(Mathf.DeltaAngle(num2, AngleLimit));
			num = Mathf.InverseLerp(0f, AngleLimit, num3);
			if (LimitSmoothing > Mathf.Epsilon)
			{
				float num4 = Mathf.Lerp(55f, 15f, LimitSmoothing);
				_limiting_angle_newLocal = Quaternion.SlerpUnclamped(_limiting_angle_targetInLocal, child.LastKeyframeLocalRotation, deltaForLerps * num4 * num);
			}
			else
			{
				_limiting_angle_newLocal = Quaternion.SlerpUnclamped(_limiting_angle_targetInLocal, child.LastKeyframeLocalRotation, num);
			}
			_limiting_angle_ToTargetRot = FEngineering.QToWorld(child.ParentBone.transform.rotation, _limiting_angle_newLocal);
			_limiting_limitPosition = child.ParentBone.ProceduralPosition + _limiting_angle_ToTargetRot * Vector3.Scale(child.transform.lossyScale, child.LastKeyframeLocalPosition);
		}
		if (num > Mathf.Epsilon)
		{
			return _limiting_limitPosition;
		}
		return targetPos;
	}

	private void MotionInfluenceLimiting()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		if (MotionInfluence != 1f)
		{
			_limiting_influenceOffset = (BaseTransform.position - previousWorldPosition) * (1f - MotionInfluence);
			if (MotionInfluenceInY < 1f)
			{
				_limiting_influenceOffset.y = (BaseTransform.position.y - previousWorldPosition.y) * (1f - MotionInfluenceInY);
			}
			for (int i = 0; i < TailSegments.Count; i++)
			{
				TailSegment tailSegment = TailSegments[i];
				tailSegment.ProceduralPosition += _limiting_influenceOffset;
				TailSegment tailSegment2 = TailSegments[i];
				tailSegment2.PreviousPosition += _limiting_influenceOffset;
			}
			TailSegment ghostChild = GhostChild;
			ghostChild.ProceduralPosition += _limiting_influenceOffset;
			TailSegment ghostChild2 = GhostChild;
			ghostChild2.PreviousPosition += _limiting_influenceOffset;
		}
	}

	private void CalculateGravityPositionOffsetForSegment(TailSegment bone)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		_tc_segmentGravityOffset = (bone.Gravity + WindEffect) * bone.BoneLengthScaled;
		_tc_segmentGravityToParentDir = bone.ProceduralPosition - bone.ParentBone.ProceduralPosition;
		Vector3 val = _tc_segmentGravityToParentDir + _tc_segmentGravityOffset;
		_tc_preGravOff = ((Vector3)(ref val)).normalized * ((Vector3)(ref _tc_segmentGravityToParentDir)).magnitude;
		bone.ProceduralPosition = bone.ParentBone.ProceduralPosition + _tc_preGravOff;
	}

	private void Axis2DLimit(TailSegment child)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		child.ProceduralPosition -= FEngineering.VAxis2DLimit(child.ParentBone.transform, child.ParentBone.ProceduralPosition, child.ProceduralPosition, Axis2D);
	}

	public float GetDistanceMeasure(Vector3 targetPosition)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		if (DistanceWithoutY)
		{
			Vector3 val = BaseTransform.position + BaseTransform.TransformVector(DistanceMeasurePoint);
			return Vector2.Distance(new Vector2(val.x, val.z), new Vector2(targetPosition.x, targetPosition.z));
		}
		return Vector3.Distance(BaseTransform.position + BaseTransform.TransformVector(DistanceMeasurePoint), targetPosition);
	}

	private void MaxDistanceCalculations()
	{
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)DistanceFrom != (Object)null)
		{
			finalDistanceFrom = DistanceFrom;
		}
		else if ((Object)(object)finalDistanceFrom == (Object)null)
		{
			if ((Object)(object)_distanceFrom_Auto == (Object)null)
			{
				Camera main = Camera.main;
				if (Object.op_Implicit((Object)(object)main))
				{
					_distanceFrom_Auto = ((Component)main).transform;
				}
				else if (!wasCameraSearch)
				{
					main = Object.FindObjectOfType<Camera>();
					if (Object.op_Implicit((Object)(object)main))
					{
						_distanceFrom_Auto = ((Component)main).transform;
					}
					wasCameraSearch = true;
				}
			}
			finalDistanceFrom = _distanceFrom_Auto;
		}
		if (MaximumDistance > 0f && (Object)(object)finalDistanceFrom != (Object)null)
		{
			if (!maxDistanceExceed)
			{
				if (GetDistanceMeasure(finalDistanceFrom.position) > MaximumDistance + MaximumDistance * MaxOutDistanceFactor)
				{
					maxDistanceExceed = true;
				}
				distanceWeight += Time.unscaledDeltaTime * (1f / FadeDuration);
				if (distanceWeight > 1f)
				{
					distanceWeight = 1f;
				}
			}
			else
			{
				if (GetDistanceMeasure(finalDistanceFrom.position) <= MaximumDistance)
				{
					maxDistanceExceed = false;
				}
				distanceWeight -= Time.unscaledDeltaTime * (1f / FadeDuration);
				if (distanceWeight < 0f)
				{
					distanceWeight = 0f;
				}
			}
		}
		else
		{
			maxDistanceExceed = false;
			distanceWeight = 1f;
		}
	}

	private Vector3 TailCalculations_SmoothPosition(Vector3 from, Vector3 to, TailSegment bone)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (SmoothingStyle == EAnimationStyle.Accelerating)
		{
			return TailCalculations_SmoothPositionSmoothDamp(from, to, ref bone.VelocityHelper, bone.PositionSpeed);
		}
		if (SmoothingStyle == EAnimationStyle.Quick)
		{
			return TailCalculations_SmoothPositionLerp(from, to, bone.PositionSpeed);
		}
		return TailCalculations_SmoothPositionLinear(from, to, bone.PositionSpeed);
	}

	private Vector3 TailCalculations_SmoothPositionLerp(Vector3 from, Vector3 to, float speed)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.Lerp(from, to, secPeriodDelta * speed);
	}

	private Vector3 TailCalculations_SmoothPositionSmoothDamp(Vector3 from, Vector3 to, ref Vector3 velo, float speed)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.SmoothDamp(from, to, ref velo, Mathf.LerpUnclamped(0.08f, 0.0001f, Mathf.Sqrt(Mathf.Sqrt(speed))), float.PositiveInfinity, rateDelta);
	}

	private Vector3 TailCalculations_SmoothPositionLinear(Vector3 from, Vector3 to, float speed)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return Vector3.MoveTowards(from, to, deltaForLerps * speed * 45f);
	}

	private Quaternion TailCalculations_SmoothRotation(Quaternion from, Quaternion to, TailSegment bone)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		if (SmoothingStyle == EAnimationStyle.Accelerating)
		{
			return TailCalculations_SmoothRotationSmoothDamp(from, to, ref bone.QVelocityHelper, bone.RotationSpeed);
		}
		if (SmoothingStyle == EAnimationStyle.Quick)
		{
			return TailCalculations_SmoothRotationLerp(from, to, bone.RotationSpeed);
		}
		return TailCalculations_SmoothRotationLinear(from, to, bone.RotationSpeed);
	}

	private Quaternion TailCalculations_SmoothRotationLerp(Quaternion from, Quaternion to, float speed)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		return Quaternion.Lerp(from, to, secPeriodDelta * speed);
	}

	private Quaternion TailCalculations_SmoothRotationSmoothDamp(Quaternion from, Quaternion to, ref Quaternion velo, float speed)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		return FEngineering.SmoothDampRotation(from, to, ref velo, Mathf.LerpUnclamped(0.25f, 0.0001f, Mathf.Sqrt(Mathf.Sqrt(speed))), rateDelta);
	}

	private Quaternion TailCalculations_SmoothRotationLinear(Quaternion from, Quaternion to, float speed)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return Quaternion.RotateTowards(from, to, speed * deltaForLerps * 1600f);
	}

	private void TailCalculations_Begin()
	{
		if (IncludeParent)
		{
			_tc_startI = 0;
			_tc_rootBone = TailSegments[0];
		}
		else
		{
			_tc_startI = 1;
			if (TailSegments.Count > 1)
			{
				_tc_rootBone = TailSegments[1];
			}
			else
			{
				_tc_rootBone = TailSegments[0];
				_tc_startI = -1;
			}
		}
		_tc_startII = _tc_startI + 1;
		if (_tc_startII > TailSegments.Count - 1)
		{
			_tc_startII = -1;
		}
		if (Deflection > Mathf.Epsilon && !_pp_initialized)
		{
			InitializePostProcessing();
		}
		if (Tangle < 0f)
		{
			_tc_tangle = Mathf.LerpUnclamped(1f, 1.5f, Tangle + 1f);
		}
		else
		{
			_tc_tangle = Mathf.LerpUnclamped(1f, -4f, Tangle);
		}
	}

	private void TailSegments_UpdateRootFeatures()
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		if (UseWaving)
		{
			Waving_AutoSwingUpdate();
			_tc_startBoneRotOffset = WavingRotationOffset * RotationOffset;
		}
		else
		{
			_tc_startBoneRotOffset = RotationOffset;
		}
		if (Sustain > Mathf.Epsilon)
		{
			Waving_SustainUpdate();
		}
		if (PostProcessingNeeded())
		{
			PostProcessing_Begin();
		}
	}

	private void TailCalculations_SegmentPreProcessingStack(TailSegment child)
	{
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		if (!UseCollision)
		{
			if (AngleLimit < 181f)
			{
				child.ProceduralPosition = AngleLimiting(child, child.ProceduralPosition);
			}
			if (child.PositionSpeed < 1f)
			{
				child.ProceduralPosition = TailCalculations_SmoothPosition(child.PreviousPosition, child.ProceduralPosition, child);
			}
		}
		else
		{
			if (child.PositionSpeed < 1f)
			{
				child.ProceduralPosition = TailCalculations_SmoothPosition(child.PreviousPosition, child.ProceduralPosition, child);
			}
			TailCalculations_ComputeSegmentCollisions(child, ref child.ProceduralPosition);
			if (AngleLimit < 181f)
			{
				child.ProceduralPosition = AngleLimiting(child, child.ProceduralPosition);
			}
		}
		if (MaxStretching < 1f)
		{
			StretchingLimiting(child);
		}
		if (!FEngineering.VIsZero(child.Gravity) || UseWind)
		{
			CalculateGravityPositionOffsetForSegment(child);
		}
		if (Axis2D > 0)
		{
			Axis2DLimit(child);
		}
	}

	private void TailCalculations_SegmentPostProcessing(TailSegment bone)
	{
		if (Deflection > Mathf.Epsilon)
		{
			Deflection_SegmentOffsetSimple(bone, ref bone.ProceduralPosition);
		}
	}

	private void TailCalculations_SegmentRotation(TailSegment child, Vector3 localOffset)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		_tc_lookRot = Quaternion.FromToRotation(child.ParentBone.transform.TransformDirection(localOffset), child.ProceduralPositionWeightBlended - child.ParentBone.ProceduralPositionWeightBlended);
		_tc_targetParentRot = _tc_lookRot * child.ParentBone.transform.rotation;
		if (AnimateRoll)
		{
			_tc_targetParentRot = Quaternion.Lerp(child.ParentBone.TrueTargetRotation, _tc_targetParentRot, deltaForLerps * Mathf.LerpUnclamped(10f, 60f, child.RotationSpeed));
		}
		child.ParentBone.TrueTargetRotation = _tc_targetParentRot;
		child.ParentBone.PreviousPosReferenceRotation = child.ParentBone.PosRefRotation;
		if (!AnimateRoll && child.RotationSpeed < 1f)
		{
			_tc_targetParentRot = TailCalculations_SmoothRotation(child.ParentBone.PosRefRotation, _tc_targetParentRot, child);
		}
		child.ParentBone.PosRefRotation = _tc_targetParentRot;
	}

	private void TailCalculations_SegmentRotationDetached(TailSegment child, Vector3 localOffset)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		_tc_lookRot = Quaternion.FromToRotation(child.ParentBone.transform.TransformDirection(localOffset), child.ProceduralPositionWeightBlended - child.ParentBone.ProceduralPositionWeightBlended);
		_tc_targetParentRot = _tc_lookRot * child.transform.rotation;
		if (AnimateRoll)
		{
			_tc_targetParentRot = Quaternion.Lerp(child.ParentBone.TrueTargetRotation, _tc_targetParentRot, deltaForLerps * Mathf.LerpUnclamped(10f, 60f, child.RotationSpeed));
		}
		child.ParentBone.TrueTargetRotation = _tc_targetParentRot;
		child.ParentBone.PreviousPosReferenceRotation = child.ParentBone.PosRefRotation;
		if (!AnimateRoll && child.RotationSpeed < 1f)
		{
			_tc_targetParentRot = TailCalculations_SmoothRotation(child.ParentBone.PosRefRotation, _tc_targetParentRot, child);
		}
		child.ParentBone.PosRefRotation = _tc_targetParentRot;
	}

	private void TailCalculations_ApplySegmentMotion(TailSegment child)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		child.ParentBone.transform.rotation = child.ParentBone.TrueTargetRotation;
		child.transform.position = child.ProceduralPositionWeightBlended;
		child.RefreshFinalPos(child.ProceduralPositionWeightBlended);
		child.ParentBone.RefreshFinalRot(child.ParentBone.TrueTargetRotation);
	}

	private void TailSegment_PrepareBoneLength(TailSegment child)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		child.BoneDimensionsScaled = Vector3.Scale(child.ParentBone.transform.lossyScale * child.LengthMultiplier, child.LastKeyframeLocalPosition);
		child.BoneLengthScaled = ((Vector3)(ref child.BoneDimensionsScaled)).magnitude;
	}

	private void TailSegment_PrepareMotionParameters(TailSegment child)
	{
		_sg_curly = Mathf.LerpUnclamped(0.5f, 0.125f, child.Curling);
		_sg_springVelo = Mathf.LerpUnclamped(0.65f, 0.9f, child.Springiness);
		_sg_curly = Mathf.Lerp(_sg_curly, Mathf.LerpUnclamped(0.95f, 0.135f, child.Curling), child.Slithery);
		_sg_springVelo = Mathf.Lerp(_sg_springVelo, Mathf.LerpUnclamped(0.1f, 0.85f, child.Springiness), child.Slithery);
	}

	private void TailSegment_PrepareVelocity(TailSegment child)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		_sg_push = child.ProceduralPosition - child.PreviousPosition;
		child.PreviousPosition = child.ProceduralPosition;
		float num = _sg_springVelo;
		if (child.CollisionContactFlag)
		{
			num *= child.Slippery;
		}
		child.ProceduralPosition += _sg_push * num;
		child.PreviousPush = _sg_push;
	}

	private void TailSegment_PrepareRotation(TailSegment child)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		_sg_targetChildWorldPosInParentFront = child.ParentBone.ProceduralPosition + TailSegment_GetSwingRotation(child, _sg_slitFactor) * child.BoneDimensionsScaled;
	}

	private void TailSegment_PrepareRotationDetached(TailSegment child)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		_sg_targetChildWorldPosInParentFront = child.ParentBone.ProceduralPosition + TailSegment_GetSwingRotationDetached(child, _sg_slitFactor) * child.BoneDimensionsScaled;
	}

	private void TailSegment_BaseSwingProcessing(TailSegment child)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		_sg_slitFactor = child.Slithery;
		if (child.CollisionContactRelevancy > 0f)
		{
			_sg_slitFactor = ReflectCollision;
		}
		_sg_dirToTargetParentFront = _sg_targetChildWorldPosInParentFront - child.ProceduralPosition;
		if (UnifyBendiness > 0f)
		{
			child.ProceduralPosition += _sg_dirToTargetParentFront * secPeriodDelta * _sg_curly * TailSegment_GetUnifiedBendinessMultiplier(child);
		}
		else
		{
			child.ProceduralPosition += _sg_dirToTargetParentFront * _sg_curly * secPeriodDelta;
		}
		if (Tangle != 0f && child.Slithery >= 1f)
		{
			child.ProceduralPosition = Vector3.LerpUnclamped(child.ProceduralPosition, _sg_targetChildWorldPosInParentFront, _tc_tangle);
		}
	}

	private void TailSegment_PreRotationPositionBlend(TailSegment child)
	{
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if (child.BlendValue * conditionalWeight < 1f)
		{
			child.ProceduralPositionWeightBlended = Vector3.LerpUnclamped(child.transform.position, child.ProceduralPosition, child.BlendValue * conditionalWeight);
		}
		else
		{
			child.ProceduralPositionWeightBlended = child.ProceduralPosition;
		}
	}

	private Quaternion TailSegment_RotationSlithery(TailSegment child)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (!FEngineering.QIsZero(child.Curving))
		{
			return GetSlitheryReferenceRotation(child) * child.Curving * child.ParentBone.LastKeyframeLocalRotation;
		}
		return GetSlitheryReferenceRotation(child) * child.ParentBone.LastKeyframeLocalRotation;
	}

	private Quaternion TailSegment_RotationSlitheryDetached(TailSegment child)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		if (!FEngineering.QIsZero(child.Curving))
		{
			return GetSlitheryReferenceRotation(child) * child.Curving * child.ParentBone.InitialLocalRotation;
		}
		return GetSlitheryReferenceRotation(child) * child.ParentBone.InitialLocalRotation;
	}

	private Quaternion GetSlitheryReferenceRotation(TailSegment child)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (child.Slithery <= 1f)
		{
			return child.ParentBone.ParentBone.PosRefRotation;
		}
		return Quaternion.LerpUnclamped(child.ParentBone.ParentBone.PosRefRotation, child.ParentBone.ParentBone.PreviousPosReferenceRotation, (child.Slithery - 1f) * 5f);
	}

	private Quaternion TailSegment_RotationStiff(TailSegment child)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if (!FEngineering.QIsZero(child.Curving))
		{
			return child.ParentBone.transform.rotation * MultiplyQ(child.Curving, (float)child.Index * 2f);
		}
		return child.ParentBone.transform.rotation;
	}

	private Quaternion TailSegment_GetSwingRotation(TailSegment child, float curlFactor)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (curlFactor >= 1f)
		{
			return TailSegment_RotationSlithery(child);
		}
		if (curlFactor > Mathf.Epsilon)
		{
			return Quaternion.LerpUnclamped(TailSegment_RotationStiff(child), TailSegment_RotationSlithery(child), curlFactor);
		}
		return TailSegment_RotationStiff(child);
	}

	private Quaternion TailSegment_GetSwingRotationDetached(TailSegment child, float curlFactor)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		if (curlFactor >= 1f)
		{
			return TailSegment_RotationSlitheryDetached(child);
		}
		if (curlFactor > Mathf.Epsilon)
		{
			return Quaternion.LerpUnclamped(TailSegment_RotationStiff(child), TailSegment_RotationSlitheryDetached(child), curlFactor);
		}
		return TailSegment_RotationStiff(child);
	}

	private float TailSegment_GetUnifiedBendinessMultiplier(TailSegment child)
	{
		float num = child.BoneLength / _TC_TailLength;
		num = Mathf.Pow(num, 0.5f);
		if (num == 0f)
		{
			num = 1f;
		}
		float num2 = _sg_curly / num / 2f;
		num2 = Mathf.LerpUnclamped(_sg_curly, num2, UnifyBendiness);
		if (num2 < 0.15f)
		{
			num2 = 0.15f;
		}
		else if (num2 > 1.4f)
		{
			num2 = 1.4f;
		}
		return num2;
	}

	public void TailSegments_UpdateCoordsForRootBone(TailSegment parent)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		TailSegment tailSegment = TailSegments[0];
		tailSegment.transform.localRotation = tailSegment.LastKeyframeLocalRotation * _tc_startBoneRotOffset;
		parent.PreviousPosReferenceRotation = parent.PosRefRotation;
		parent.PosRefRotation = parent.transform.rotation;
		parent.PreviousPosition = parent.ProceduralPosition;
		parent.ProceduralPosition = parent.transform.position;
		if (DetachChildren)
		{
			tailSegment.TrueTargetRotation = tailSegment.transform.rotation;
		}
		parent.RefreshFinalPos(parent.transform.position);
		parent.ProceduralPositionWeightBlended = parent.ProceduralPosition;
		if ((Object)(object)parent.ParentBone.transform != (Object)null)
		{
			parent.ParentBone.PreviousPosReferenceRotation = parent.ParentBone.PosRefRotation;
			parent.ParentBone.PreviousPosition = parent.ParentBone.ProceduralPosition;
			parent.ParentBone.ProceduralPosition = parent.ParentBone.transform.position;
			parent.ParentBone.PosRefRotation = parent.ParentBone.transform.rotation;
			parent.ParentBone.ProceduralPositionWeightBlended = parent.ParentBone.ProceduralPosition;
		}
		TailSegment childBone = TailSegments[_tc_startI].ChildBone;
		childBone.PreviousPosition += _waving_sustain;
		tailSegment.RefreshKeyLocalPosition();
	}

	public void TailCalculations_UpdateArtificialChildBone(TailSegment child)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		if (DetachChildren)
		{
			TailSegment_PrepareRotationDetached(child);
		}
		else
		{
			TailSegment_PrepareRotation(child);
		}
		TailSegment_BaseSwingProcessing(child);
		if (child.PositionSpeed < 1f)
		{
			child.ProceduralPosition = TailCalculations_SmoothPosition(child.PreviousPosition, child.ProceduralPosition, child);
		}
		if (MaxStretching < 1f)
		{
			StretchingLimiting(child);
		}
		if (!FEngineering.VIsZero(child.Gravity) || UseWind)
		{
			CalculateGravityPositionOffsetForSegment(child);
		}
		if (Axis2D > 0)
		{
			Axis2DLimit(child);
		}
		child.CollisionContactRelevancy = -1f;
		if (child.BlendValue * conditionalWeight < 1f)
		{
			child.ProceduralPositionWeightBlended = Vector3.LerpUnclamped(child.ParentBone.transform.TransformPoint(child.LastKeyframeLocalPosition), child.ProceduralPosition, child.BlendValue * conditionalWeight);
		}
		else
		{
			child.ProceduralPositionWeightBlended = child.ProceduralPosition;
		}
	}

	public void Editor_TailCalculations_RefreshArtificialParentBone()
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		GhostParent.ProceduralPosition = GhostParent.transform.position + FEngineering.TransformVector(GhostParent.transform.rotation, GhostParent.transform.lossyScale, GhostParent.LocalOffset);
	}

	private void SimulateTailMotionFrame(bool pp)
	{
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		TailSegments_UpdateRootFeatures();
		TailSegments_UpdateCoordsForRootBone(_tc_rootBone);
		if (pp)
		{
			PostProcessing_ReferenceUpdate();
		}
		if (_tc_startI > -1)
		{
			TailSegment tailSegment = TailSegments[_tc_startI];
			if (!DetachChildren)
			{
				while (tailSegment != GhostChild)
				{
					tailSegment.BoneDimensionsScaled = Vector3.Scale(tailSegment.ParentBone.transform.lossyScale * tailSegment.LengthMultiplier, tailSegment.LastKeyframeLocalPosition);
					tailSegment.BoneLengthScaled = ((Vector3)(ref tailSegment.BoneDimensionsScaled)).magnitude;
					TailSegment_PrepareBoneLength(tailSegment);
					TailSegment_PrepareMotionParameters(tailSegment);
					TailSegment_PrepareVelocity(tailSegment);
					tailSegment = tailSegment.ChildBone;
				}
			}
			else
			{
				while (tailSegment != GhostChild)
				{
					tailSegment.BoneDimensionsScaled = Vector3.Scale(tailSegment.ParentBone.transform.lossyScale * tailSegment.LengthMultiplier, tailSegment.InitialLocalPosition);
					tailSegment.BoneLengthScaled = ((Vector3)(ref tailSegment.BoneDimensionsScaled)).magnitude;
					TailSegment_PrepareMotionParameters(tailSegment);
					TailSegment_PrepareVelocity(tailSegment);
					tailSegment = tailSegment.ChildBone;
				}
			}
		}
		TailSegment_PrepareBoneLength(GhostChild);
		TailSegment_PrepareMotionParameters(GhostChild);
		TailSegment_PrepareVelocity(GhostChild);
		if (_tc_startII > -1)
		{
			TailSegment tailSegment2 = TailSegments[_tc_startII];
			if (!DetachChildren)
			{
				while (tailSegment2 != GhostChild)
				{
					TailSegment_PrepareRotation(tailSegment2);
					TailSegment_BaseSwingProcessing(tailSegment2);
					TailCalculations_SegmentPreProcessingStack(tailSegment2);
					if (pp)
					{
						TailCalculations_SegmentPostProcessing(tailSegment2);
					}
					TailSegment_PreRotationPositionBlend(tailSegment2);
					tailSegment2 = tailSegment2.ChildBone;
				}
			}
			else
			{
				while (tailSegment2 != GhostChild)
				{
					TailSegment_PrepareRotationDetached(tailSegment2);
					TailSegment_BaseSwingProcessing(tailSegment2);
					TailCalculations_SegmentPreProcessingStack(tailSegment2);
					if (pp)
					{
						TailCalculations_SegmentPostProcessing(tailSegment2);
					}
					TailSegment_PreRotationPositionBlend(tailSegment2);
					tailSegment2 = tailSegment2.ChildBone;
				}
			}
		}
		TailCalculations_UpdateArtificialChildBone(GhostChild);
	}

	private void UpdateTailAlgorithm()
	{
		//IL_025f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		//IL_017f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		TailCalculations_Begin();
		if (framesToSimulate != 0)
		{
			if (UseCollision)
			{
				BeginCollisionsUpdate();
			}
			bool pp = PostProcessingNeeded();
			MotionInfluenceLimiting();
			for (int i = 0; i < framesToSimulate; i++)
			{
				SimulateTailMotionFrame(pp);
			}
			TailSegments[_tc_startI].transform.position = TailSegments[_tc_startI].ProceduralPositionWeightBlended;
			TailSegments[_tc_startI].RefreshFinalPos(TailSegments[_tc_startI].ProceduralPositionWeightBlended);
			if (!DetachChildren)
			{
				if (_tc_startII > -1)
				{
					for (TailSegment tailSegment = TailSegments[_tc_startII]; tailSegment != GhostChild; tailSegment = tailSegment.ChildBone)
					{
						TailCalculations_SegmentRotation(tailSegment, tailSegment.LastKeyframeLocalPosition);
						TailCalculations_ApplySegmentMotion(tailSegment);
					}
				}
			}
			else if (_tc_startII > -1)
			{
				for (TailSegment tailSegment2 = TailSegments[_tc_startII]; tailSegment2 != GhostChild; tailSegment2 = tailSegment2.ChildBone)
				{
					TailCalculations_SegmentRotation(tailSegment2, tailSegment2.InitialLocalPosition);
					TailCalculations_ApplySegmentMotion(tailSegment2);
				}
			}
			TailCalculations_SegmentRotation(GhostChild, GhostChild.LastKeyframeLocalPosition);
			GhostChild.ParentBone.transform.rotation = GhostChild.ParentBone.TrueTargetRotation;
			GhostChild.ParentBone.RefreshFinalRot(GhostChild.ParentBone.TrueTargetRotation);
			if (Object.op_Implicit((Object)(object)GhostChild.transform))
			{
				GhostChild.RefreshFinalPos(GhostChild.transform.position);
				GhostChild.RefreshFinalRot(GhostChild.transform.rotation);
			}
		}
		else if (InterpolateRate)
		{
			secPeriodDelta = rateDelta / 24f;
			deltaForLerps = secPeriodDelta;
			SimulateTailMotionFrame(PostProcessingNeeded());
			if (_tc_startII > -1)
			{
				for (TailSegment tailSegment3 = TailSegments[_tc_startII]; tailSegment3 != GhostChild; tailSegment3 = tailSegment3.ChildBone)
				{
					TailCalculations_SegmentRotation(tailSegment3, tailSegment3.LastKeyframeLocalPosition);
					TailCalculations_ApplySegmentMotion(tailSegment3);
				}
			}
			TailCalculations_SegmentRotation(GhostChild, GhostChild.LastKeyframeLocalPosition);
			GhostChild.ParentBone.transform.rotation = GhostChild.ParentBone.TrueTargetRotation;
			GhostChild.ParentBone.RefreshFinalRot(GhostChild.ParentBone.TrueTargetRotation);
		}
		else if (_tc_startI > -1)
		{
			TailSegment tailSegment4 = TailSegments[_tc_startI];
			while (tailSegment4 != null && Object.op_Implicit((Object)(object)tailSegment4.transform))
			{
				tailSegment4.transform.position = tailSegment4.LastFinalPosition;
				tailSegment4.transform.rotation = tailSegment4.LastFinalRotation;
				tailSegment4 = tailSegment4.ChildBone;
			}
		}
	}

	private void CheckIfTailAnimatorShouldBeUpdated()
	{
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
		if (!initialized)
		{
			if (StartAfterTPose)
			{
				startAfterTPoseCounter++;
				if (startAfterTPoseCounter > 6)
				{
					Init();
				}
			}
			updateTailAnimator = false;
			return;
		}
		if (UseMaxDistance)
		{
			MaxDistanceCalculations();
			conditionalWeight = OverrideWeight * distanceWeight;
		}
		else
		{
			conditionalWeight = OverrideWeight;
		}
		if (_forceDisable)
		{
			if (FadeDuration > 0f)
			{
				_forceDisableElapsed += Time.unscaledDeltaTime * (1f / FadeDuration);
				if (_forceDisableElapsed > 1f)
				{
					_forceDisableElapsed = 1f;
				}
			}
			else
			{
				_forceDisableElapsed = 1f;
			}
			conditionalWeight *= 1f - _forceDisableElapsed;
		}
		else if (_forceDisableElapsed > 0f && FadeDuration > 0f)
		{
			_forceDisableElapsed -= Time.unscaledDeltaTime * (1f / FadeDuration);
			if (_forceDisableElapsed < 0f)
			{
				_forceDisableElapsed = 0f;
			}
			conditionalWeight *= 1f - _forceDisableElapsed;
		}
		if (DisabledByInvisibility())
		{
			return;
		}
		if (UseCollision && !collisionInitialized)
		{
			SetupSphereColliders();
		}
		if (TailSegments.Count == 0)
		{
			Debug.LogError((object)("[TAIL ANIMATOR] No tail bones defined in " + ((Object)this).name + " !"));
			initialized = false;
			updateTailAnimator = false;
			return;
		}
		if (TailAnimatorAmount * conditionalWeight <= Mathf.Epsilon)
		{
			wasDisabled = true;
			updateTailAnimator = false;
			return;
		}
		if (wasDisabled)
		{
			User_ReposeTail();
			previousWorldPosition = ((Component)this).transform.position;
			wasDisabled = false;
		}
		if (IncludeParent && TailSegments.Count > 0 && !Object.op_Implicit((Object)(object)TailSegments[0].transform.parent))
		{
			IncludeParent = false;
		}
		if (TailSegments.Count < 1)
		{
			updateTailAnimator = false;
		}
		else
		{
			updateTailAnimator = true;
		}
	}

	public bool DisabledByInvisibility()
	{
		if ((Object)(object)OptimizeWithAnimator != (Object)null && !((Behaviour)OptimizeWithAnimator).enabled)
		{
			updateTailAnimator = false;
			return true;
		}
		return false;
	}

	private void DeltaTimeCalculations()
	{
		if (UpdateRate > 0)
		{
			switch (DeltaType)
			{
			case EFDeltaType.DeltaTime:
			case EFDeltaType.SafeDelta:
				justDelta = Time.deltaTime / Mathf.Clamp(Time.timeScale, 0.01f, 1f);
				break;
			case EFDeltaType.SmoothDeltaTime:
				justDelta = Time.smoothDeltaTime;
				break;
			case EFDeltaType.UnscaledDeltaTime:
				justDelta = Time.unscaledDeltaTime;
				break;
			case EFDeltaType.FixedDeltaTime:
				justDelta = Time.fixedDeltaTime;
				break;
			}
			justDelta *= TimeScale;
			secPeriodDelta = 1f;
			deltaForLerps = secPeriodDelta;
			rateDelta = 1f / (float)UpdateRate;
			StableUpdateRateCalculations();
			return;
		}
		switch (DeltaType)
		{
		case EFDeltaType.SafeDelta:
			justDelta = Mathf.Lerp(justDelta, GetClampedSmoothDelta(), 0.075f);
			break;
		case EFDeltaType.DeltaTime:
			justDelta = Time.deltaTime;
			break;
		case EFDeltaType.SmoothDeltaTime:
			justDelta = Time.smoothDeltaTime;
			break;
		case EFDeltaType.UnscaledDeltaTime:
			justDelta = Time.unscaledDeltaTime;
			break;
		case EFDeltaType.FixedDeltaTime:
			justDelta = Time.fixedDeltaTime;
			break;
		}
		rateDelta = justDelta;
		deltaForLerps = Mathf.Pow(secPeriodDelta, 0.1f) * 0.02f;
		justDelta *= TimeScale;
		secPeriodDelta = Mathf.Min(1f, justDelta * 60f);
		framesToSimulate = 1;
		previousframesToSimulate = 1;
	}

	private void StableUpdateRateCalculations()
	{
		previousframesToSimulate = framesToSimulate;
		collectedDelta += justDelta;
		framesToSimulate = 0;
		while (collectedDelta >= rateDelta)
		{
			collectedDelta -= rateDelta;
			framesToSimulate++;
			if (framesToSimulate >= 3)
			{
				collectedDelta = 0f;
				break;
			}
		}
	}

	private void PreCalibrateBones()
	{
		for (TailSegment tailSegment = TailSegments[0]; tailSegment != GhostChild; tailSegment = tailSegment.ChildBone)
		{
			tailSegment.PreCalibrate();
		}
	}

	private void CalibrateBones()
	{
		if (UseIK && IKBlend > 0f)
		{
			UpdateIK();
		}
		_limiting_stretchingHelperTooLong = Mathf.Lerp(0.4f, 0f, MaxStretching);
		_limiting_stretchingHelperTooShort = _limiting_stretchingHelperTooLong * 1.5f;
	}

	public void CheckForNullsInGhostChain()
	{
		if (_TransformsGhostChain == null)
		{
			_TransformsGhostChain = new List<Transform>();
		}
		for (int num = _TransformsGhostChain.Count - 1; num >= 0; num--)
		{
			if ((Object)(object)_TransformsGhostChain[num] == (Object)null)
			{
				_TransformsGhostChain.RemoveAt(num);
			}
		}
	}

	private float GetClampedSmoothDelta()
	{
		return Mathf.Clamp(Time.smoothDeltaTime, 0f, 0.25f);
	}

	private Quaternion MultiplyQ(Quaternion rotation, float times)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		return Quaternion.AngleAxis(rotation.x * 57.29578f * times, Vector3.right) * Quaternion.AngleAxis(rotation.z * 57.29578f * times, Vector3.forward) * Quaternion.AngleAxis(rotation.y * 57.29578f * times, Vector3.up);
	}

	public float GetValueFromCurve(int i, AnimationCurve c)
	{
		if (!initialized)
		{
			return c.Evaluate((float)i / (float)_TransformsGhostChain.Count);
		}
		return c.Evaluate(TailSegments[i].IndexOverlLength);
	}

	public AnimationCurve ClampCurve(AnimationCurve a, float timeStart, float timeEnd, float lowest, float highest)
	{
		Keyframe[] keys = a.keys;
		for (int i = 0; i < keys.Length; i++)
		{
			if (((Keyframe)(ref keys[i])).time < timeStart)
			{
				((Keyframe)(ref keys[i])).time = timeStart;
			}
			else if (((Keyframe)(ref keys[i])).time > timeEnd)
			{
				((Keyframe)(ref keys[i])).time = timeEnd;
			}
			if (((Keyframe)(ref keys[i])).value < lowest)
			{
				((Keyframe)(ref keys[i])).value = lowest;
			}
			else if (((Keyframe)(ref keys[i])).value > highest)
			{
				((Keyframe)(ref keys[i])).value = highest;
			}
		}
		a.keys = keys;
		return a;
	}

	public void RefreshTransformsList()
	{
		if (_TransformsGhostChain == null)
		{
			_TransformsGhostChain = new List<Transform>();
			return;
		}
		for (int num = _TransformsGhostChain.Count - 1; num >= 0; num--)
		{
			if ((Object)(object)_TransformsGhostChain[0] == (Object)null)
			{
				_TransformsGhostChain.RemoveAt(num);
			}
		}
	}

	public TailSegment GetGhostChild()
	{
		return GhostChild;
	}

	private IEnumerator LateFixed()
	{
		lateFixedIsRunning = true;
		while (true)
		{
			yield return CoroutineEx.waitForFixedUpdate;
			PreCalibrateBones();
			fixedAllow = true;
		}
	}

	private void Deflection_BeginUpdate()
	{
		_defl_treshold = DeflectionStartAngle / 90f;
		float smoothTime = DeflectionSmooth / 9f;
		for (int i = _tc_startII; i < TailSegments.Count; i++)
		{
			TailSegment tailSegment = _pp_reference[i];
			if (!tailSegment.CheckDeflectionState(_defl_treshold, smoothTime, rateDelta))
			{
				bool flag = true;
				if (DeflectOnlyCollisions && tailSegment.CollisionContactRelevancy <= 0f)
				{
					flag = false;
				}
				if (flag)
				{
					Deflection_AddDeflectionSource(tailSegment);
				}
				else
				{
					Deflection_RemoveDeflectionSource(tailSegment);
				}
			}
			else
			{
				Deflection_RemoveDeflectionSource(tailSegment);
			}
		}
	}

	private void Deflection_RemoveDeflectionSource(TailSegment child)
	{
		if (!child.DeflectionRestoreState().HasValue && _defl_source.Contains(child))
		{
			_defl_source.Remove(child);
		}
	}

	private void Deflection_AddDeflectionSource(TailSegment child)
	{
		if (child.DeflectionRelevant() && !_defl_source.Contains(child))
		{
			_defl_source.Add(child);
		}
	}

	private void Deflection_SegmentOffsetSimple(TailSegment child, ref Vector3 position)
	{
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		if (child.Index == _tc_startI)
		{
			return;
		}
		float num = 0f;
		for (int i = 0; i < _defl_source.Count; i++)
		{
			if (child.Index <= _defl_source[i].Index && child.Index != _defl_source[i].Index && !(_defl_source[i].DeflectionFactor < num))
			{
				num = _defl_source[i].DeflectionFactor;
				float num2 = 0f;
				if (i > 0)
				{
					num2 = _defl_source[i].Index;
				}
				float num3 = Mathf.InverseLerp(num2, (float)_defl_source[i].Index, (float)child.Index);
				Vector3 val = _defl_source[i].DeflectionWorldPosition - child.ParentBone.ProceduralPosition;
				Vector3 proceduralPosition = child.ParentBone.ProceduralPosition;
				proceduralPosition += ((Vector3)(ref val)).normalized * child.BoneLengthScaled;
				child.ProceduralPosition = Vector3.LerpUnclamped(child.ProceduralPosition, proceduralPosition, Deflection * DeflectionFalloff.Evaluate(num3) * _defl_source[i].DeflectionSmooth);
			}
		}
	}

	public void User_SetTailTransforms(List<Transform> list)
	{
		StartBone = list[0];
		EndBone = list[list.Count - 1];
		_TransformsGhostChain = list;
		StartAfterTPose = false;
		initialized = false;
		Init();
	}

	public TailSegment User_AddTailTransform(Transform transform)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		TailSegment tailSegment = new TailSegment(transform);
		TailSegment tailSegment2 = TailSegments[TailSegments.Count - 1];
		tailSegment.ParamsFromAll(tailSegment2);
		tailSegment.RefreshFinalPos(tailSegment.transform.position);
		tailSegment.RefreshFinalRot(tailSegment.transform.rotation);
		tailSegment.ProceduralPosition = tailSegment.transform.position;
		tailSegment.PosRefRotation = tailSegment.transform.rotation;
		_TransformsGhostChain.Add(transform);
		TailSegments.Add(tailSegment);
		tailSegment2.SetChildRef(tailSegment);
		tailSegment.SetParentRef(tailSegment2);
		tailSegment.SetChildRef(GhostChild);
		GhostChild.SetParentRef(tailSegment);
		for (int i = 0; i < TailSegments.Count; i++)
		{
			TailSegments[i].SetIndex(i, TailSegments.Count);
		}
		return tailSegment;
	}

	public void User_CutEndSegmentsTo(int fromLastTo)
	{
		if (fromLastTo < TailSegments.Count)
		{
			GhostChild = TailSegments[fromLastTo];
			GhostChild.SetChildRef(null);
			for (int num = TailSegments.Count - 1; num >= fromLastTo; num--)
			{
				TailSegments.RemoveAt(num);
				_TransformsGhostChain.RemoveAt(num);
			}
		}
		else
		{
			Debug.Log((object)("[Tail Animator Cutting] Wrong index, you want cut from end to " + fromLastTo + " segment but there are only " + TailSegments.Count + " segments!"));
		}
	}

	public void User_ReposeTail()
	{
		GhostParent.Reset();
		for (int i = 0; i < TailSegments.Count; i++)
		{
			TailSegments[i].Reset();
		}
		GhostChild.Reset();
	}

	public void User_ForceDisabled(bool disable)
	{
		_forceDisable = disable;
	}

	public void OnDrop(PointerEventData data)
	{
	}

	private void OnValidate()
	{
		if (UpdateRate < 0)
		{
			UpdateRate = 0;
		}
		if (Application.isPlaying)
		{
			RefreshSegmentsColliders();
			if (UseIK)
			{
				IK_ApplyLimitBoneSettings();
			}
		}
		if (UsePartialBlend)
		{
			ClampCurve(BlendCurve, 0f, 1f, 0f, 1f);
		}
	}

	public void GetGhostChain()
	{
		if (_TransformsGhostChain == null)
		{
			_TransformsGhostChain = new List<Transform>();
		}
		if ((Object)(object)EndBone == (Object)null)
		{
			_TransformsGhostChain.Clear();
			Transform val = StartBone;
			if ((Object)(object)val == (Object)null)
			{
				val = ((Component)this).transform;
			}
			_TransformsGhostChain.Add(val);
			while (val.childCount > 0)
			{
				val = val.GetChild(0);
				if (!_TransformsGhostChain.Contains(val))
				{
					_TransformsGhostChain.Add(val);
				}
			}
			_GhostChainInitCount = _TransformsGhostChain.Count;
			return;
		}
		List<Transform> list = new List<Transform>();
		Transform val2 = StartBone;
		if ((Object)(object)val2 == (Object)null)
		{
			val2 = ((Component)this).transform;
		}
		Transform val3 = EndBone;
		list.Add(val3);
		while ((Object)(object)val3 != (Object)null && (Object)(object)val3 != (Object)(object)StartBone)
		{
			val3 = val3.parent;
			if (!list.Contains(val3))
			{
				list.Add(val3);
			}
		}
		if ((Object)(object)val3 == (Object)null)
		{
			Debug.Log((object)("[Tail Animator Editor] " + ((Object)EndBone).name + " is not child of " + ((Object)val2).name + "!"));
			Debug.LogError((object)("[Tail Animator Editor] " + ((Object)EndBone).name + " is not child of " + ((Object)val2).name + "!"));
		}
		else
		{
			if (!list.Contains(val3))
			{
				list.Add(val3);
			}
			_TransformsGhostChain.Clear();
			_TransformsGhostChain = list;
			_TransformsGhostChain.Reverse();
			_GhostChainInitCount = _TransformsGhostChain.Count;
		}
	}

	private void Start()
	{
		if (UpdateAsLast)
		{
			((Behaviour)this).enabled = false;
			((Behaviour)this).enabled = true;
		}
		if (StartAfterTPose)
		{
			startAfterTPoseCounter = 6;
		}
		else
		{
			Init();
		}
	}

	private void Reset()
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Expected O, but got Unknown
		Keyframe val = default(Keyframe);
		((Keyframe)(ref val))._002Ector(0f, 0f, 0.1f, 0.1f, 0f, 0.5f);
		Keyframe val2 = default(Keyframe);
		((Keyframe)(ref val2))._002Ector(1f, 1f, 5f, 0f, 0.1f, 0f);
		DeflectionFalloff = new AnimationCurve((Keyframe[])(object)new Keyframe[2] { val, val2 });
	}

	private void Tick()
	{
		using (TimeWarning.New("TailAnimator2:Tick"))
		{
			CheckIfTailAnimatorShouldBeUpdated();
			DeltaTimeCalculations();
			if (UseWind)
			{
				WindEffectUpdate();
			}
			if (AnimatePhysics != EFixedMode.None || !updateTailAnimator)
			{
				return;
			}
			if (DetachChildren)
			{
				if (_tc_rootBone != null && Object.op_Implicit((Object)(object)_tc_rootBone.transform))
				{
					_tc_rootBone.PreCalibrate();
				}
			}
			else if (OverrideKeyframeAnimation < 1f)
			{
				PreCalibrateBones();
			}
		}
	}

	private void FixedTick()
	{
		using (TimeWarning.New("TailAnimator2:FixedTick"))
		{
			if (AnimatePhysics != EFixedMode.Basic || !updateTailAnimator)
			{
				return;
			}
			if (DetachChildren)
			{
				if (_tc_rootBone != null && Object.op_Implicit((Object)(object)_tc_rootBone.transform))
				{
					_tc_rootBone.PreCalibrate();
				}
			}
			else
			{
				fixedUpdated = true;
				PreCalibrateBones();
			}
		}
	}

	private void LateTick()
	{
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0200: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("TailAnimator2:LateTick"))
		{
			if (!updateTailAnimator)
			{
				return;
			}
			if (AnimatePhysics == EFixedMode.Late)
			{
				if (!lateFixedIsRunning)
				{
					((MonoBehaviour)this).StartCoroutine(LateFixed());
				}
				if (!fixedAllow)
				{
					return;
				}
				fixedAllow = false;
			}
			else
			{
				if (lateFixedIsRunning)
				{
					((MonoBehaviour)this).StopCoroutine(LateFixed());
					lateFixedIsRunning = false;
				}
				if (AnimatePhysics == EFixedMode.Basic)
				{
					if (!fixedUpdated)
					{
						return;
					}
					fixedUpdated = false;
				}
			}
			if (DetachChildren)
			{
				TailSegment tailSegment = TailSegments[0];
				TailSegments[0].RefreshKeyLocalPositionAndRotation(tailSegment.InitialLocalPosition, tailSegment.InitialLocalRotation);
				TailSegments[0].PreCalibrate();
				tailSegment = TailSegments[1];
				if (!IncludeParent)
				{
					tailSegment.RefreshKeyLocalPositionAndRotation(tailSegment.InitialLocalPosition, tailSegment.InitialLocalRotation);
					tailSegment.PreCalibrate();
					tailSegment = TailSegments[2];
				}
				while (tailSegment != GhostChild)
				{
					tailSegment.RefreshKeyLocalPositionAndRotation(tailSegment.InitialLocalPosition, tailSegment.InitialLocalRotation);
					tailSegment.transform.position = _baseTransform.TransformPoint(tailSegment.InitialLocalPositionInRoot);
					tailSegment.transform.rotation = FEngineering.QToWorld(_baseTransform.rotation, tailSegment.InitialLocalRotationInRoot);
					tailSegment = tailSegment.ChildBone;
				}
			}
			else if (OverrideKeyframeAnimation > 0f)
			{
				if (OverrideKeyframeAnimation >= 1f)
				{
					PreCalibrateBones();
					for (TailSegment tailSegment2 = TailSegments[0]; tailSegment2 != GhostChild; tailSegment2 = tailSegment2.ChildBone)
					{
						tailSegment2.RefreshKeyLocalPositionAndRotation();
					}
				}
				else
				{
					for (TailSegment tailSegment3 = TailSegments[0]; tailSegment3 != GhostChild; tailSegment3 = tailSegment3.ChildBone)
					{
						tailSegment3.transform.localPosition = Vector3.LerpUnclamped(tailSegment3.transform.localPosition, tailSegment3.InitialLocalPosition, OverrideKeyframeAnimation);
						tailSegment3.transform.localRotation = Quaternion.LerpUnclamped(tailSegment3.transform.localRotation, tailSegment3.InitialLocalRotation, OverrideKeyframeAnimation);
						tailSegment3.RefreshKeyLocalPositionAndRotation();
					}
				}
			}
			else
			{
				for (TailSegment tailSegment4 = TailSegments[0]; tailSegment4 != GhostChild; tailSegment4 = tailSegment4.ChildBone)
				{
					tailSegment4.RefreshKeyLocalPositionAndRotation();
				}
			}
			ExpertParamsUpdate();
			ShapingParamsUpdate();
			CalibrateBones();
			UpdateTailAlgorithm();
			EndUpdate();
		}
	}

	private void EndUpdate()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		ShapingEndUpdate();
		ExpertCurvesEndUpdate();
		previousWorldPosition = BaseTransform.position;
	}

	public TailAnimator2()
	{
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0224: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_022f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02be: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Unknown result type (might be due to invalid IL or missing references)
		//IL_0317: Unknown result type (might be due to invalid IL or missing references)
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0343: Unknown result type (might be due to invalid IL or missing references)
		//IL_0349: Unknown result type (might be due to invalid IL or missing references)
		//IL_034e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0354: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_035f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0364: Unknown result type (might be due to invalid IL or missing references)
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		//IL_036f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_037a: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03be: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0480: Unknown result type (might be due to invalid IL or missing references)
		//IL_0485: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04df: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e4: Unknown result type (might be due to invalid IL or missing references)
		CollisionSpace = ECollisionSpace.Selective_Fast;
		InclusionRadius = 1f;
		IgnoreMeshColliders = true;
		CollideWithDisabledColliders = true;
		CollisionSlippery = 1f;
		CollidersScaleCurve = AnimationCurve.Linear(0f, 1f, 1f, 1f);
		CollidersScaleMul = 6.5f;
		CollisionsAutoCurve = 0.5f;
		CollidersSameLayer = true;
		CollidersAddRigidbody = true;
		RigidbodyMass = 1f;
		SlitheryCurve = AnimationCurve.EaseInOut(0f, 0.75f, 1f, 1f);
		lastSlithery = -1f;
		CurlingCurve = AnimationCurve.EaseInOut(0f, 0.7f, 1f, 0.3f);
		lastCurling = -1f;
		SpringCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 0f);
		lastSpringiness = -1f;
		SlipperyCurve = AnimationCurve.EaseInOut(0f, 0.7f, 1f, 1f);
		lastSlippery = -1f;
		PosCurve = AnimationCurve.EaseInOut(0f, 0.7f, 1f, 1f);
		lastPosSpeeds = -1f;
		RotCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0.9f);
		lastRotSpeeds = -1f;
		BlendCurve = AnimationCurve.EaseInOut(0f, 0.95f, 1f, 0.45f);
		lastTailAnimatorAmount = -1f;
		IKAutoWeights = true;
		IKBaseReactionWeight = 0.65f;
		IKReactionWeightCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0.25f);
		IKAutoAngleLimits = true;
		IKAutoAngleLimit = 40f;
		IKBlend = 1f;
		IKAnimatorBlend = 0.5f;
		IKReactionQuality = 2;
		IKStretchCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
		RotationOffset = Quaternion.identity;
		Curving = Quaternion.identity;
		CurvCurve = AnimationCurve.EaseInOut(0f, 0.75f, 1f, 1f);
		lastCurving = Quaternion.identity;
		LengthMultiplier = 1f;
		LengthMulCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 1f);
		lastLengthMul = 1f;
		GravityCurve = AnimationCurve.EaseInOut(0f, 0.65f, 1f, 1f);
		Gravity = Vector3.zero;
		lastGravity = Vector3.zero;
		UseWaving = true;
		WavingSpeed = 3f;
		WavingRange = 0.8f;
		WavingAxis = new Vector3(1f, 1f, 1f);
		WavingType = FEWavingType.Advanced;
		AlternateWave = 1f;
		_waving_sustain = Vector3.zero;
		WindEffectPower = 1f;
		WindTurbulencePower = 1f;
		WindWorldNoisePower = 0.5f;
		WindEffect = Vector3.zero;
		_limiting_limitPosition = Vector3.zero;
		_limiting_influenceOffset = Vector3.zero;
		_tc_segmentGravityOffset = Vector3.zero;
		_tc_segmentGravityToParentDir = Vector3.zero;
		_tc_preGravOff = Vector3.zero;
		MaximumDistance = 35f;
		FadeDuration = 0.75f;
		distanceWeight = 1f;
		_tc_startII = 1;
		_tc_lookRot = Quaternion.identity;
		_tc_targetParentRot = Quaternion.identity;
		_tc_startBoneRotOffset = Quaternion.identity;
		_tc_tangle = 1f;
		_sg_springVelo = 0.5f;
		_sg_curly = 0.5f;
		_sg_slitFactor = 0.5f;
		wasDisabled = true;
		justDelta = 0.016f;
		secPeriodDelta = 0.5f;
		deltaForLerps = 0.016f;
		rateDelta = 0.016f;
		framesToSimulate = 1;
		previousframesToSimulate = 1;
		fixedAllow = true;
		DeflectionStartAngle = 10f;
		DeflectionFalloff = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
		DeflectOnlyCollisions = true;
		_defl_treshold = 0.01f;
		DrawGizmos = true;
		EndBoneJointOffset = Vector3.zero;
		_GhostChainInitCount = -1;
		OverrideWeight = 1f;
		conditionalWeight = 1f;
		Slithery = 1f;
		Curling = 0.5f;
		MaxStretching = 0.375f;
		AngleLimit = 181f;
		AngleLimitAxis = Vector3.zero;
		LimitAxisRange = Vector2.zero;
		LimitSmoothing = 0.5f;
		MotionInfluence = 1f;
		MotionInfluenceInY = 1f;
		IncludeParent = true;
		ReactionSpeed = 0.9f;
		RotationRelevancy = 1f;
		SmoothingStyle = EAnimationStyle.Accelerating;
		TimeScale = 1f;
		DeltaType = EFDeltaType.SafeDelta;
		UpdateAsLast = true;
		DetectZeroKeyframes = true;
		StartAfterTPose = true;
		TailAnimatorAmount = 1f;
		((MonoBehaviour)this)._002Ector();
	}
}
