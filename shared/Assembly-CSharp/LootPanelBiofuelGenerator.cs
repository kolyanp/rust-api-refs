using System;
using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class LootPanelBiofuelGenerator : LootPanel
{
	[Serializable]
	public struct BarColors
	{
		public Color text;

		public Color background;

		public Color fill;

		public Color border;

		public Color label;
	}

	[Header("Stirring Bar Colors")]
	public BarColors redColors;

	public BarColors greenColors;

	[Header("UI References")]
	public InfoBar stirBar;

	public RustText stirLabel;

	public SquareBorder stirBorder;

	public Image stirBackground;
}
