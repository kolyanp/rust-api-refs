using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Instancing;

public struct MeshRenderKey(Mesh mesh, Material[] materials, ShadowCastingMode castShadows, bool recieveShadows, LightProbeUsage lightProbes) : IEquatable<MeshRenderKey>
{
	public Mesh Mesh = mesh;

	public Material[] Materials = materials;

	public ShadowCastingMode CastShadows = castShadows;

	public bool RecieveShadows = recieveShadows;

	public LightProbeUsage LightProbeUsages = lightProbes;

	public bool Equals(MeshRenderKey other)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)Mesh != (Object)(object)other.Mesh || CastShadows != other.CastShadows || RecieveShadows != other.RecieveShadows || LightProbeUsages != other.LightProbeUsages)
		{
			return false;
		}
		if (Materials == null || other.Materials == null)
		{
			return Materials == other.Materials;
		}
		for (int i = 0; i < Materials.Length; i++)
		{
			if ((Object)(object)Materials[i] != (Object)(object)other.Materials[i])
			{
				return false;
			}
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		if (obj is MeshRenderKey other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (17 * 31 + ((object)Mesh)?.GetHashCode()).GetValueOrDefault();
	}
}
