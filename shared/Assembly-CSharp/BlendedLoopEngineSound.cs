using UnityEngine;

public class BlendedLoopEngineSound : MonoBehaviour, IClientComponent
{
	public class EngineLoop
	{
		public BlendedEngineLoopDefinition.EngineLoopDefinition definition;

		public BlendedLoopEngineSound parent;

		public Sound sound;

		public SoundModulation.Modulator gainMod;

		public SoundModulation.Modulator pitchMod;
	}

	public BlendedEngineLoopDefinition loopDefinition;

	public bool engineOn;

	[Range(0f, 1f)]
	public float RPMControl;

	public float smoothedRPMControl;

	private EngineLoop[] engineLoops;

	public bool debugPlayImmediately;

	public float maxDistance => loopDefinition.engineLoops[0].soundDefinition.maxDistance;

	public EngineLoop[] GetEngineLoops()
	{
		return engineLoops;
	}
}
