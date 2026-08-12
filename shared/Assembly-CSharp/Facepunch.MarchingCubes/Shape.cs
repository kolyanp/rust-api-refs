using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace Facepunch.MarchingCubes;

[BurstCompile]
public readonly struct Shape
{
	public readonly ShapeType Type;

	public readonly bool IsAdditive;

	public readonly float Smoothing;

	public readonly float3 Position;

	public readonly float3 Extents;

	public readonly quaternion Rotation;

	private readonly quaternion InvRotation;

	public Shape(ShapeType type, float3 position, float3 extents, quaternion rotation, bool isAdditive, float smoothing)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		Type = type;
		Position = position;
		Extents = extents;
		Rotation = rotation;
		InvRotation = math.inverse(rotation);
		IsAdditive = isAdditive;
		Smoothing = smoothing;
	}

	public Bounds GetBounds()
	{
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0180: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0203: Unknown result type (might be due to invalid IL or missing references)
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_021e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0239: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		float num = 4f * Smoothing + 1f;
		switch (Type)
		{
		case ShapeType.Sphere:
			return new Bounds(float3.op_Implicit(Position), Vector3.one * ((Extents.x + num) * 2f));
		case ShapeType.AABB:
			return new Bounds(float3.op_Implicit(Position), float3.op_Implicit(Extents + num) * 2f);
		case ShapeType.OBB:
		case ShapeType.SharpOBB:
			return Facepunch.MarchingCubes.SDFBounds.OrientedExtentsBounds(Position, Extents + num, Rotation);
		case ShapeType.Cylinder:
			return Facepunch.MarchingCubes.SDFBounds.OrientedExtentsBounds(Position, new float3(Extents.x, Extents.y, Extents.x) + num, Rotation);
		case ShapeType.Capsule:
			return Facepunch.MarchingCubes.SDFBounds.OrientedExtentsBounds(Position, new float3(Extents.x, Extents.y + Extents.x, Extents.x) + num, Rotation);
		case ShapeType.Cone:
			return Facepunch.MarchingCubes.SDFBounds.OrientedExtentsBounds(Position, new float3(Extents.x, Extents.y, Extents.x) + num, Rotation);
		case ShapeType.HexPrism:
		{
			float num2 = Extents.x * 1.1547005f;
			return Facepunch.MarchingCubes.SDFBounds.OrientedExtentsBounds(Position, new float3(num2, num2, Extents.y) + num, Rotation);
		}
		case ShapeType.Bulge:
			return new Bounds(float3.op_Implicit(Position), Vector3.one * (Extents.x + 1f) * 2f);
		case ShapeType.Smooth:
			return new Bounds(float3.op_Implicit(Position), Vector3.one * (Extents.x + 2f) * 2f);
		default:
			return new Bounds(float3.op_Implicit(Position), Vector3.zero);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal float SphereDistance(float3 p)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return math.length(p - Position) - Extents.x;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal float AABBDistance(float3 p)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		float3 val = math.abs(Position - p) - Extents;
		return math.length(math.max(val, float3.op_Implicit(0f))) + math.min(math.max(val.x, math.max(val.y, val.z)), 0f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal float OBBDistance(float3 p)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		float num = math.cmin(Extents) * 0.25f;
		float3 val = math.abs(math.rotate(InvRotation, Position - p)) - Extents + num;
		return math.length(math.max(val, float3.op_Implicit(0f))) + math.min(math.max(val.x, math.max(val.y, val.z)), 0f) - num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal float SharpOBBDistance(float3 p)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		float3 val = math.abs(math.rotate(InvRotation, p - Position)) - Extents;
		return math.length(math.max(val, float3.op_Implicit(0f))) + math.min(math.max(val.x, math.max(val.y, val.z)), 0f);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal float CylinderDistance(float3 p)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		float3 val = math.rotate(InvRotation, p - Position);
		float2 val2 = math.abs(new float2(math.length(((float3)(ref val)).xz), val.y)) - new float2(Extents.x, Extents.y);
		return math.min(math.max(val2.x, val2.y), 0f) + math.length(math.max(val2, float2.op_Implicit(0f)));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal float CapsuleDistance(float3 p)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		float3 val = math.rotate(InvRotation, p - Position);
		val.y -= math.clamp(val.y, 0f - Extents.y, Extents.y);
		return math.length(val) - Extents.x;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal float ConeDistance(float3 p)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Unknown result type (might be due to invalid IL or missing references)
		float x = Extents.x;
		float y = Extents.y;
		float3 val = math.rotate(InvRotation, p - Position);
		float2 val2 = default(float2);
		((float2)(ref val2))._002Ector(math.length(((float3)(ref val)).xz), val.y);
		float2 val3 = default(float2);
		((float2)(ref val3))._002Ector(0f, y);
		float2 val4 = default(float2);
		((float2)(ref val4))._002Ector(0f - x, 2f * y);
		float2 val5 = default(float2);
		((float2)(ref val5))._002Ector(val2.x - math.min(val2.x, math.select(0f, x, val2.y < 0f)), math.abs(val2.y) - y);
		float2 val6 = val2 - val3 + val4 * math.clamp(math.dot(val3 - val2, val4) / math.dot(val4, val4), 0f, 1f);
		return ((val6.x < 0f && val5.y < 0f) ? (-1f) : 1f) * math.sqrt(math.min(math.dot(val5, val5), math.dot(val6, val6)));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal float HexPrismDistance(float3 p)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		float x = Extents.x;
		float y = Extents.y;
		float3 val = default(float3);
		((float3)(ref val))._002Ector(-0.8660254f, 0.5f, 0.57735f);
		float3 val2 = math.abs(math.rotate(InvRotation, p - Position));
		((float3)(ref val2)).xy = ((float3)(ref val2)).xy - 2f * math.min(math.dot(((float3)(ref val)).xy, ((float3)(ref val2)).xy), 0f) * ((float3)(ref val)).xy;
		float2 val3 = default(float2);
		((float2)(ref val3))._002Ector(math.length(((float3)(ref val2)).xy - new float2(math.clamp(val2.x, (0f - val.z) * x, val.z * x), x)) * math.sign(val2.y - x), val2.z - y);
		return math.min(math.max(val3.x, val3.y), 0f) + math.length(math.max(val3, float2.op_Implicit(0f)));
	}
}
