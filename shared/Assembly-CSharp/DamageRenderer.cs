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

	[HideInInspector]
	[SerializeField]
	private List<DamageShowingRenderer> damageShowingRenderers;

	[HideInInspector]
	[SerializeField]
	private List<GlassPane> damageShowingGlassRenderers;
}
