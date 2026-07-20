using Facepunch;
using UnityEngine;
using UnityEngine.Assertions;

[CreateAssetMenu(menuName = "Rust/Convar Controlled Spawn Population")]
public class ConvarControlledSpawnPopulation : DensitySpawnPopulation
{
	[Header("Convars")]
	public string PopulationConvar;

	private ConsoleSystem.Command _command;

	protected ConsoleSystem.Command Command
	{
		get
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			if (_command == null)
			{
				_command = ConsoleSystem.Index.Server.Find(StringView.op_Implicit(PopulationConvar));
				Assert.IsNotNull<ConsoleSystem.Command>(_command, $"{this} has missing convar {PopulationConvar}");
			}
			return _command;
		}
	}

	public override float TargetDensity => Command.AsFloat;
}
