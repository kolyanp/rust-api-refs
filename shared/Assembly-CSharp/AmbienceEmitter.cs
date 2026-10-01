using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class AmbienceEmitter : MonoBehaviour, IClientComponent, IComparable<AmbienceEmitter>
{
	public AmbienceDefinitionList baseAmbience;

	public AmbienceDefinitionList stings;

	public bool isStatic = true;

	public bool followCamera;

	public bool isBaseEmitter;

	public bool active;

	public float cameraDistanceSq = float.PositiveInfinity;

	public BoundingSphere boundingSphere;

	public float crossfadeTime = 2f;

	public Dictionary<AmbienceDefinition, float> nextStingTime = new Dictionary<AmbienceDefinition, float>();

	public float deactivateTime = float.PositiveInfinity;

	public bool playUnderwater = true;

	public bool playAbovewater = true;

	public Enum currentTopology
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	public Enum currentBiome
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	public int CompareTo(AmbienceEmitter other)
	{
		return cameraDistanceSq.CompareTo(other.cameraDistanceSq);
	}
}
