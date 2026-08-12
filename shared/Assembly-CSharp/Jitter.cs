using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class Jitter
{
	private readonly Vector2[] haltonSequence;

	[CompilerGenerated]
	private Vector2 _003COffset_003Ek__BackingField;

	[CompilerGenerated]
	private Vector2 _003CTexelOffset_003Ek__BackingField;

	public int SampleIndex { get; private set; }

	public int SampleCount { get; private set; }

	public Vector2 Offset
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003COffset_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003COffset_003Ek__BackingField = value;
		}
	}

	public Vector2 TexelOffset
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003CTexelOffset_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003CTexelOffset_003Ek__BackingField = value;
		}
	}

	public Jitter()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		haltonSequence = (Vector2[])(object)new Vector2[16]
		{
			new Vector2(0.5f, 0.333333f),
			new Vector2(0.25f, 0.666667f),
			new Vector2(0.75f, 0.111111f),
			new Vector2(0.125f, 0.444444f),
			new Vector2(0.625f, 0.777778f),
			new Vector2(0.375f, 0.222222f),
			new Vector2(0.875f, 0.555556f),
			new Vector2(0.0625f, 0.888889f),
			new Vector2(0.5625f, 0.037037f),
			new Vector2(0.3125f, 0.37037f),
			new Vector2(0.8125f, 0.703704f),
			new Vector2(0.1875f, 0.148148f),
			new Vector2(0.6875f, 0.481481f),
			new Vector2(0.4375f, 0.814815f),
			new Vector2(0.9375f, 0.259259f),
			new Vector2(1f / 32f, 0.592593f)
		};
		SampleCount = 8;
		Offset = Vector2.zero;
		TexelOffset = Vector2.zero;
		base._002Ector();
		SampleCount = haltonSequence.Length;
	}

	private Matrix4x4 GetJitteredProjectionMatrix(Camera camera)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		Offset = haltonSequence[++SampleIndex % 8] - new Vector2(0.5f, 0.5f);
		TexelOffset = new Vector2(Offset.x / (float)camera.pixelWidth, Offset.y / (float)camera.pixelHeight);
		return RuntimeUtilities.GetJitteredPerspectiveProjectionMatrix(camera, Offset);
	}

	public void ConfigureCameraJitter(PostProcessRenderContext context)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		Camera camera = context.camera;
		camera.nonJitteredProjectionMatrix = camera.projectionMatrix;
		camera.projectionMatrix = GetJitteredProjectionMatrix(camera);
		camera.useJitteredProjectionMatrixForTransparentRendering = true;
	}
}
