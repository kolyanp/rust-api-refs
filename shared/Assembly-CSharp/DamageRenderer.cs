using System;
using System.Collections.Generic;
using UnityEngine;

public class DamageRenderer : MonoBehaviour, IClientComponent
{
	[Serializable]
	private struct DamageShowingRenderer(Renderer renderer, int[] indices)
	{
		public Renderer renderer = renderer;

		public int[] indices = indices;
	}

	[SerializeField]
	private List<Material> damageShowingMats;

	[SerializeField]
	private float maxDamageOpacity = 0.9f;

	[SerializeField]
	[HideInInspector]
	private List<DamageShowingRenderer> damageShowingRenderers;

	[SerializeField]
	[HideInInspector]
	private List<GlassPane> damageShowingGlassRenderers;
}
