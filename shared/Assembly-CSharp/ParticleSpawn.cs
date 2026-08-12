using System.Runtime.CompilerServices;
using UnityEngine;

public class ParticleSpawn : SingletonComponent<ParticleSpawn>, IClientComponent
{
	public GameObjectRef[] Prefabs;

	public int PatchCount = 8;

	public int PatchSize = 100;

	[CompilerGenerated]
	private Vector3 _003COrigin_003Ek__BackingField;

	public Vector3 Origin
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003COrigin_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003COrigin_003Ek__BackingField = value;
		}
	}
}
