using System;
using UnityEngine;

public class ArcadeEntity : BaseMonoBehaviour
{
	public uint id;

	public uint spriteID;

	public uint soundID;

	public bool visible;

	public Vector3 heading;

	public bool isEnabled;

	public bool dirty;

	public float alpha;

	public BoxCollider boxCollider;

	public bool host;

	public bool localAuthorativeOverride;

	public ArcadeEntity arcadeEntityParent;

	public uint prefabID;

	[Header("Health")]
	public bool takesDamage;

	public float health;

	public float maxHealth;

	[NonSerialized]
	public bool mapLoadedEntiy;

	public ArcadeEntity()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		heading = new Vector3(0f, 1f, 0f);
		alpha = 1f;
		health = 1f;
		maxHealth = 1f;
		base._002Ector();
	}
}
