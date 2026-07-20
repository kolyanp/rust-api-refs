using System.Collections.Generic;
using UnityEngine;

namespace FIMSpace.FTail;

public static class FTail_Skinning
{
	public static FTail_SkinningVertexData[] CalculateVertexWeightingData(Mesh baseMesh, Transform[] bonesCoords, Vector3 spreadOffset, int weightBoneLimit = 2, float spreadValue = 0.8f, float spreadPower = 0.185f)
	{
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		Vector3[] array = (Vector3[])(object)new Vector3[bonesCoords.Length];
		Quaternion[] array2 = (Quaternion[])(object)new Quaternion[bonesCoords.Length];
		for (int i = 0; i < bonesCoords.Length; i++)
		{
			array[i] = bonesCoords[0].parent.InverseTransformPoint(bonesCoords[i].position);
			array2[i] = FEngineering.QToLocal(bonesCoords[0].parent.rotation, bonesCoords[i].rotation);
		}
		return CalculateVertexWeightingData(baseMesh, array, array2, spreadOffset, weightBoneLimit, spreadValue, spreadPower);
	}

	public static FTail_SkinningVertexData[] CalculateVertexWeightingData(Mesh baseMesh, Vector3[] bonesPos, Quaternion[] bonesRot, Vector3 spreadOffset, int weightBoneLimit = 2, float spreadValue = 0.8f, float spreadPower = 0.185f)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		if (weightBoneLimit < 1)
		{
			weightBoneLimit = 1;
		}
		if (weightBoneLimit > 2)
		{
			weightBoneLimit = 2;
		}
		int vertexCount = baseMesh.vertexCount;
		FTail_SkinningVertexData[] array = new FTail_SkinningVertexData[vertexCount];
		Vector3[] array2 = (Vector3[])(object)new Vector3[bonesPos.Length];
		for (int i = 0; i < bonesPos.Length - 1; i++)
		{
			array2[i] = bonesPos[i + 1] - bonesPos[i];
		}
		if (array2.Length > 1)
		{
			array2[^1] = array2[^2];
		}
		for (int j = 0; j < vertexCount; j++)
		{
			array[j] = new FTail_SkinningVertexData(baseMesh.vertices[j]);
			array[j].CalculateVertexParameters(bonesPos, bonesRot, array2, weightBoneLimit, spreadValue, spreadOffset, spreadPower);
		}
		return array;
	}

	public static SkinnedMeshRenderer SkinMesh(Mesh baseMesh, Transform skinParent, Transform[] bonesStructure, FTail_SkinningVertexData[] vertData)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		Vector3[] array = (Vector3[])(object)new Vector3[bonesStructure.Length];
		Quaternion[] array2 = (Quaternion[])(object)new Quaternion[bonesStructure.Length];
		for (int i = 0; i < bonesStructure.Length; i++)
		{
			array[i] = skinParent.InverseTransformPoint(bonesStructure[i].position);
			array2[i] = FEngineering.QToLocal(skinParent.rotation, bonesStructure[i].rotation);
		}
		return SkinMesh(baseMesh, array, array2, vertData);
	}

	public static SkinnedMeshRenderer SkinMesh(Mesh baseMesh, Vector3[] bonesPositions, Quaternion[] bonesRotations, FTail_SkinningVertexData[] vertData)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0175: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		if (bonesPositions == null)
		{
			return null;
		}
		if (bonesRotations == null)
		{
			return null;
		}
		if ((Object)(object)baseMesh == (Object)null)
		{
			return null;
		}
		if (vertData == null)
		{
			return null;
		}
		Mesh val = Object.Instantiate<Mesh>(baseMesh);
		((Object)val).name = ((Object)baseMesh).name + " [FSKINNED]";
		Transform transform = new GameObject(((Object)baseMesh).name + " [FSKINNED]").transform;
		SkinnedMeshRenderer val2 = ((Component)transform).gameObject.AddComponent<SkinnedMeshRenderer>();
		Transform[] array = (Transform[])(object)new Transform[bonesPositions.Length];
		Matrix4x4[] array2 = (Matrix4x4[])(object)new Matrix4x4[bonesPositions.Length];
		string text = ((((Object)baseMesh).name.Length >= 6) ? ((Object)baseMesh).name.Substring(0, 5) : ((Object)baseMesh).name);
		for (int i = 0; i < bonesPositions.Length; i++)
		{
			array[i] = new GameObject("BoneF-" + text + "[" + i + "]").transform;
			if (i == 0)
			{
				array[i].SetParent(transform, true);
			}
			else
			{
				array[i].SetParent(array[i - 1], true);
			}
			((Component)array[i]).transform.position = bonesPositions[i];
			((Component)array[i]).transform.rotation = bonesRotations[i];
			array2[i] = array[i].worldToLocalMatrix * transform.localToWorldMatrix;
		}
		BoneWeight[] array3 = (BoneWeight[])(object)new BoneWeight[val.vertexCount];
		for (int j = 0; j < array3.Length; j++)
		{
			array3[j] = default(BoneWeight);
		}
		for (int k = 0; k < vertData.Length; k++)
		{
			for (int l = 0; l < vertData[k].weights.Length; l++)
			{
				array3[k] = SetWeightIndex(array3[k], l, vertData[k].bonesIndexes[l]);
				array3[k] = SetWeightToBone(array3[k], l, vertData[k].weights[l]);
			}
		}
		val.bindposes = array2;
		val.boneWeights = array3;
		List<Vector3> list = new List<Vector3>();
		List<Vector4> list2 = new List<Vector4>();
		baseMesh.GetNormals(list);
		baseMesh.GetTangents(list2);
		val.SetNormals(list);
		val.SetTangents(list2);
		val.bounds = baseMesh.bounds;
		val2.sharedMesh = val;
		val2.rootBone = array[0];
		val2.bones = array;
		return val2;
	}

	public static BoneWeight SetWeightIndex(BoneWeight weight, int bone = 0, int index = 0)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		switch (bone)
		{
		case 1:
			((BoneWeight)(ref weight)).boneIndex1 = index;
			break;
		case 2:
			((BoneWeight)(ref weight)).boneIndex2 = index;
			break;
		case 3:
			((BoneWeight)(ref weight)).boneIndex3 = index;
			break;
		default:
			((BoneWeight)(ref weight)).boneIndex0 = index;
			break;
		}
		return weight;
	}

	public static BoneWeight SetWeightToBone(BoneWeight weight, int bone = 0, float value = 1f)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		switch (bone)
		{
		case 1:
			((BoneWeight)(ref weight)).weight1 = value;
			break;
		case 2:
			((BoneWeight)(ref weight)).weight2 = value;
			break;
		case 3:
			((BoneWeight)(ref weight)).weight3 = value;
			break;
		default:
			((BoneWeight)(ref weight)).weight0 = value;
			break;
		}
		return weight;
	}
}
