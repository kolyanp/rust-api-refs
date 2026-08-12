using System;
using System.Text;
using UnityEngine;

[Serializable]
public class VolumeCloudsNoiseLayerConfig
{
	[Range(0f, 6f)]
	public float Ceiling;

	[Range(-5f, 1f)]
	public float Floor;

	public Vector2 Frequency;

	public Vector3 Offset;

	public int Octaves;

	public float Rotation;

	[Range(0.001f, 10f)]
	public float Exponent;

	public void CopyFrom(VolumeCloudsNoiseLayerConfig copy)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		Floor = copy.Floor;
		Ceiling = copy.Ceiling;
		Frequency = copy.Frequency;
		Offset = copy.Offset;
		Octaves = copy.Octaves;
		Rotation = copy.Rotation;
		Exponent = copy.Exponent;
	}

	public void Lerp(VolumeCloudsNoiseLayerConfig a, VolumeCloudsNoiseLayerConfig b, float t)
	{
		Floor = Mathf.SmoothStep(a.Floor, b.Floor, t);
		Ceiling = Mathf.SmoothStep(a.Ceiling, b.Ceiling, t);
		Frequency.x = Mathf.SmoothStep(a.Frequency.x, b.Frequency.x, t);
		Frequency.y = Mathf.SmoothStep(a.Frequency.y, b.Frequency.y, t);
		Offset.x = Mathf.SmoothStep(a.Offset.x, b.Offset.x, t);
		Offset.y = Mathf.SmoothStep(a.Offset.y, b.Offset.y, t);
		Offset.z = Mathf.SmoothStep(a.Offset.z, b.Offset.z, t);
		Rotation = Mathf.SmoothStep(a.Rotation, b.Rotation, t);
		Exponent = Mathf.SmoothStep(a.Exponent, b.Exponent, t);
	}

	public void Output(StringBuilder sb, Vector2 ofs)
	{
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		sb.AppendLine($"Ceiling: {Ceiling}");
		sb.AppendLine($"Floor: {Floor}");
		sb.AppendLine($"Freq: {Frequency}");
		sb.AppendLine($"Offset: {ofs}");
		sb.AppendLine($"Octaves: {Octaves}");
		sb.AppendLine($"Exp: {Exponent}");
	}

	public VolumeCloudsNoiseLayerConfig()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		Ceiling = 1f;
		Frequency = Vector2.one;
		Offset = Vector3.zero;
		Octaves = 1;
		Exponent = 1f;
		base._002Ector();
	}
}
