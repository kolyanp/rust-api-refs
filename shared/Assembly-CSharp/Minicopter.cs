using UnityEngine;

public class Minicopter : PlayerHelicopterWithFlares
{
	[ServerVar(Help = "Population active on the server", ShowInAdminUI = true)]
	public static float population;

	[SerializeField]
	[Header(" Minicopter Specific ")]
	private ParticleSystemContainer fxMediumDamage;

	[SerializeField]
	private ParticleSystemContainer fxHeavyDamage;

	[SerializeField]
	private SoundDefinition damagedMediumLoop;

	[SerializeField]
	private SoundDefinition damagedHeavyLoop;

	[SerializeField]
	private GameObject damageSoundTarget;
}
