using System;
using UnityEngine;

public class PanelLight : SimpleLight
{
	[Serializable]
	public struct ColorSetting
	{
		public Phrase name;

		public Phrase desc;

		public Color color;

		public Material mat;
	}

	public ColorSetting[] colorSettings;

	public MeshRenderer lightOnMesh;
}
