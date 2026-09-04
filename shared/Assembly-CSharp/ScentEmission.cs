using UnityEngine;

public class ScentEmission : EntityComponent<BaseCombatEntity>, IClientComponent
{
	[SerializeField]
	private bool startSampling;

	[SerializeField]
	private bool generateFakeTrail;

	[SerializeField]
	private float sampleIntervalSeconds;

	[Space]
	[SerializeField]
	private int minInitialPoints;

	[SerializeField]
	private Vector2 initialDistanceRange;

	[SerializeField]
	private float startAngleDeviation;

	[SerializeField]
	private float wiggleIntensity;

	[Space]
	[SerializeField]
	private LineRenderer[] lineRenderers;

	[SerializeField]
	private float heightOffset;

	[SerializeField]
	private bool snapToTerrain;

	public ScentEmission()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		startSampling = true;
		generateFakeTrail = true;
		sampleIntervalSeconds = 1f;
		minInitialPoints = 5;
		initialDistanceRange = new Vector2(50f, 100f);
		startAngleDeviation = 45f;
		wiggleIntensity = 1f;
		heightOffset = 0.5f;
		snapToTerrain = true;
		base._002Ector();
	}
}
