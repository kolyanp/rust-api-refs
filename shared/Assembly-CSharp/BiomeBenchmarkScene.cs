using UnityEngine;

public class BiomeBenchmarkScene : BenchmarkScene
{
	[Header("Biome Benchmark")]
	public uint IslandWidth;

	public uint IslandHeight;

	public uint IslandGap;

	public float FlyingSpeed;

	public GameObject WorldSetupPrefab;

	public TerrainConfig TerrainConfig;

	public Enum BiomesToTest;

	public float StreamingPause;

	[Header("Biome Benchmark - Debug")]
	public bool DebugMode;

	[Range(0f, 4f)]
	public int DebugIslandNum;

	[Range(0f, 1f)]
	public float DebugProgress;

	public BiomeBenchmarkScene()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		IslandWidth = 300u;
		IslandHeight = 1000u;
		IslandGap = 50u;
		FlyingSpeed = 16f;
		BiomesToTest = (Enum)(-1);
		StreamingPause = 4f;
		base._002Ector();
	}
}
