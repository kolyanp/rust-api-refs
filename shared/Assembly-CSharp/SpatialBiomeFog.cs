using UnityEngine;

[ExecuteAlways]
public class SpatialBiomeFog : SingletonComponent<SpatialBiomeFog>
{
	public ComputeShader FogMarchShader;

	public ComputeShader VoxelBlurShader;

	public ComputeShader StencilShader;

	public Vector3i Resolution;

	public Texture NoiseTexture;

	public float BiomeFogDensityScale;

	public bool DoVoxelBlur;

	public int BlurPasses;

	public float BlurRadius;

	public int NumDownsamples;

	public float EnvBiomeFogDensity { get; set; }

	public float UndergroundFogDensity { get; set; }

	public SpatialBiomeFog()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		Resolution = new Vector3i(128, 128, 32);
		BiomeFogDensityScale = 1f;
		DoVoxelBlur = true;
		EnvBiomeFogDensity = 1f;
		BlurPasses = 1;
		BlurRadius = 1f;
		NumDownsamples = 4;
		base._002Ector();
	}
}
