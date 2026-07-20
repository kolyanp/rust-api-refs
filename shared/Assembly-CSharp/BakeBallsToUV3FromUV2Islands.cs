using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class BakeBallsToUV3FromUV2Islands : MonoBehaviour
{
	public enum StoreMode
	{
		BottomY,
		CenterY,
		TopY
	}

	public MeshFilter meshFilter;

	[Tooltip("Which value to store in UV3.y per ball")]
	public StoreMode store;

	[Tooltip("Group key = floor(uv2.y + islandOffset). If your islands aren't exactly on integers, tweak this.")]
	public float islandOffset;

	public bool normalize01 = true;

	[ContextMenu("Bake UV3 from UV2 islands")]
	public void Bake()
	{
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0350: Unknown result type (might be due to invalid IL or missing references)
		//IL_0355: Unknown result type (might be due to invalid IL or missing references)
		if (!Object.op_Implicit((Object)(object)meshFilter))
		{
			meshFilter = ((Component)this).GetComponent<MeshFilter>();
		}
		if (!Object.op_Implicit((Object)(object)meshFilter) || !Object.op_Implicit((Object)(object)meshFilter.sharedMesh))
		{
			Debug.LogError((object)"No MeshFilter/sharedMesh.");
			return;
		}
		Mesh sharedMesh = meshFilter.sharedMesh;
		Mesh val = Object.Instantiate<Mesh>(sharedMesh);
		((Object)val).name = ((Object)sharedMesh).name + " (baked UV3)";
		Vector3[] vertices = val.vertices;
		Vector2[] uv = val.uv2;
		if (uv == null || uv.Length != vertices.Length)
		{
			Debug.LogError((object)"Mesh needs UV2 for island grouping.");
			return;
		}
		Dictionary<int, List<int>> dictionary = new Dictionary<int, List<int>>();
		for (int i = 0; i < vertices.Length; i++)
		{
			int key = Mathf.FloorToInt(uv[i].y + islandOffset + 0.0001f);
			if (!dictionary.TryGetValue(key, out var value))
			{
				value = (dictionary[key] = new List<int>());
			}
			value.Add(i);
		}
		Dictionary<int, float> dictionary2 = new Dictionary<int, float>();
		Dictionary<int, float> dictionary3 = new Dictionary<int, float>();
		foreach (KeyValuePair<int, List<int>> item in dictionary)
		{
			List<int> value2 = item.Value;
			Vector3 val2 = Vector3.zero;
			foreach (int item2 in value2)
			{
				val2 += vertices[item2];
			}
			val2 /= (float)value2.Count;
			float num = 0f;
			foreach (int item3 in value2)
			{
				float num2 = Vector3.Distance(vertices[item3], val2);
				if (num2 > num)
				{
					num = num2;
				}
			}
			dictionary2[item.Key] = val2.y;
			dictionary3[item.Key] = num;
		}
		Dictionary<int, float> dictionary4 = new Dictionary<int, float>();
		float num3 = float.PositiveInfinity;
		float num4 = float.NegativeInfinity;
		float num5 = 0f;
		foreach (int key2 in dictionary.Keys)
		{
			float num6 = dictionary2[key2];
			float num7 = dictionary3[key2];
			float num8 = (dictionary4[key2] = store switch
			{
				StoreMode.CenterY => num6, 
				StoreMode.TopY => num6 + num7, 
				_ => num6 - num7, 
			});
			if (num8 < num3)
			{
				num3 = num8;
			}
			if (num8 > num4)
			{
				num4 = num8;
			}
			if (num7 > num5)
			{
				num5 = num7;
			}
		}
		float num10 = Mathf.Max(1E-06f, num4 - num3);
		Vector2[] array = val.uv3;
		if (array == null || array.Length != vertices.Length)
		{
			array = (Vector2[])(object)new Vector2[vertices.Length];
		}
		foreach (KeyValuePair<int, List<int>> item4 in dictionary)
		{
			float num11 = dictionary4[item4.Key];
			if (normalize01)
			{
				num11 = Mathf.Clamp01((num11 - num3) / num10);
			}
			foreach (int item5 in item4.Value)
			{
				array[item5] = new Vector2(0f, num11);
			}
		}
		val.uv3 = array;
		meshFilter.sharedMesh = val;
		Debug.Log((object)$"Baked {dictionary.Count} balls into UV3.y (mode={store}, normalized={normalize01}).");
	}
}
