using System;
using System.Collections.Generic;
using UnityEngine;

namespace Rust.Ai.Gen2;

public class LivestockFeedingSlots : MonoBehaviour
{
	[Flags]
	public enum Serves
	{
		Food = 1,
		Water = 2
	}

	[Serializable]
	public struct Slot
	{
		[Tooltip("Where the animal's nose goes. Its forward is the way the animal faces while it eats or drinks.")]
		public Transform Nose;

		public Serves Serves;
	}

	public readonly struct Place
	{
		public readonly int Index;

		public readonly Vector3 Nose;

		public readonly Vector3 Facing;

		public readonly float Ground;

		public Place(int index, Vector3 nose, Vector3 facing, float ground)
		{
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_003b: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			Index = index;
			Nose = nose;
			Vector3 val = Vector3Ex.WithY(facing, 0f);
			Vector3 facing2;
			if (!(val.sqrMagnitude > 0.0001f))
			{
				facing2 = Vector3.forward;
			}
			else
			{
				val = Vector3Ex.WithY(facing, 0f);
				facing2 = val.normalized;
			}
			Facing = facing2;
			Ground = ground;
		}

		public Vector3 StandingPointFor(BaseEntity animal)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			return Vector3Ex.WithY(Nose - Facing * NoseReachOf(animal), Ground);
		}
	}

	[Tooltip("Slots may overlap. Two that would put their animals inside each other are never used at once.")]
	public Slot[] Slots;

	private const float GeneratedNoseInset = 0.1f;

	private const float MuzzleHeight = 0.8f;

	private const int ReachBlockers = 2162688;

	private const float StandingSnap = 0.15f;

	private const float StandingBackOff = 1f;

	public static float NoseReachOf(BaseEntity animal)
	{
		float num = FrontOf(animal);
		if (!(animal is LivestockAnimal livestockAnimal))
		{
			return num;
		}
		return num * livestockAnimal.TroughReachScale;
	}

	public static bool TryStandAt(RustNavMeshAgent agent, BaseEntity animal, in Place place, out Vector3 standing)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		Vector3 val = place.StandingPointFor(animal);
		standing = default;
		if (!agent.SamplePosition(val, out var hitWS, 1f))
		{
			return false;
		}
		Vector3 val2 = Vector3Ex.WithY(hitWS.position - val, 0f);
		float num = Vector3.Dot(val2, place.Facing);
		if (!(num > 0.15f))
		{
			Vector3 val3 = val2 - place.Facing * num;
			if (!(val3.sqrMagnitude > 0.0225f))
			{
				standing = hitWS.position;
				return true;
			}
		}
		return false;
	}

	public static bool CanReachInFrom(BaseEntity owner, Vector3 standing, Vector3 nose)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		return GamePhysics.LineOfSight(standing + Vector3.up * 0.8f, nose, 2162688, owner.GetRootParentEntity());
	}

	public static float TailOf(BaseEntity animal)
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		return Mathf.Max(0f, animal.bounds.extents.z - animal.bounds.center.z);
	}

	public static float FrontOf(BaseEntity animal)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		return animal.bounds.center.z + animal.bounds.extents.z;
	}

	public static BaseEntity OwnerOf(BaseEntity deployable)
	{
		BaseEntity baseEntity = (((Object)(object)deployable != (Object)null) ? deployable.GetParentEntity() : null);
		LivestockFeedingSlots livestockFeedingSlots = default;
		while ((Object)(object)baseEntity != (Object)null)
		{
			if (((Component)baseEntity).TryGetComponent<LivestockFeedingSlots>(ref livestockFeedingSlots))
			{
				return baseEntity;
			}
			baseEntity = baseEntity.GetParentEntity();
		}
		return deployable;
	}

	public static void Gather(BaseEntity owner, Serves need, PooledList<Place> places)
	{
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)owner == (Object)null)
		{
			return;
		}
		LivestockFeedingSlots livestockFeedingSlots = default;
		if (((Component)owner).TryGetComponent<LivestockFeedingSlots>(ref livestockFeedingSlots) && livestockFeedingSlots.Slots != null && livestockFeedingSlots.Slots.Length != 0)
		{
			for (int i = 0; i < livestockFeedingSlots.Slots.Length; i++)
			{
				Slot slot = livestockFeedingSlots.Slots[i];
				if ((Object)(object)slot.Nose != (Object)null && (slot.Serves & need) != 0)
				{
					((List<Place>)(object)places).Add(new Place(i, slot.Nose.position, slot.Nose.forward, ((Component)owner).transform.position.y));
				}
			}
		}
		else
		{
			AddGenerated(owner, 0, Vector3.forward, places);
			AddGenerated(owner, 1, Vector3.back, places);
			AddGenerated(owner, 2, Vector3.right, places);
			AddGenerated(owner, 3, Vector3.left, places);
		}
	}

	private static void AddGenerated(BaseEntity owner, int index, Vector3 outward, PooledList<Place> places)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		Bounds bounds = owner.bounds;
		float num = Mathf.Max(0f, Mathf.Abs(Vector3.Dot(bounds.extents, outward)) - 0.1f);
		Vector3 nose = ((Component)owner).transform.TransformPoint(bounds.center + outward * num);
		((List<Place>)(object)places).Add(new Place(index, nose, -((Component)owner).transform.TransformDirection(outward), ((Component)owner).transform.position.y));
	}

	public static bool IsHitchedOver(BaseEntity owner, in LivestockFootprint footprint)
	{
		if (!(owner is HitchTrough { hitchSpots: not null, hitchSpots: var hitchSpots }))
		{
			return false;
		}
		for (int i = 0; i < hitchSpots.Length; i++)
		{
			if (hitchSpots[i].GetHitchable() is BaseEntity entity && LivestockFootprint.Penetration(in footprint, LivestockFootprint.Of(entity)) > 0f)
			{
				return true;
			}
		}
		return false;
	}
}
