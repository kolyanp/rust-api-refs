using System.Collections.Generic;
using UnityEngine;

namespace Rust.Ai.Gen2;

public class CoverComponent : MonoBehaviour, IServerComponent
{
	[Polymorphic]
	[SerializeReference]
	public CoverGroup coverGroup;

	private void OnEnable()
	{
		if (coverGroup != null)
		{
			coverGroup.GenerateCovers(((Component)this).gameObject);
		}
		SingletonComponent<NpcCoverManager>.Instance.Add(this);
	}

	private void OnDisable()
	{
		if ((Object)(object)SingletonComponent<NpcCoverManager>.Instance != (Object)null)
		{
			SingletonComponent<NpcCoverManager>.Instance.Remove(this);
		}
	}

	public bool GetCovers(List<Cover> covers, Vector3 from)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		return coverGroup.GetCovers(((Component)this).transform, covers, from);
	}

	[Button("Bake")]
	public void Bake()
	{
		if (coverGroup != null)
		{
			coverGroup.GenerateCovers(((Component)this).gameObject);
		}
	}
}
