using System;
using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class MeshPaintController : MonoBehaviour, IClientComponent
{
	public enum Tool
	{
		Brush,
		Eraser,
		ColorPicker
	}

	public enum RotateMode
	{
		None,
		Movement,
		Random
	}

	public Camera pickerCamera;

	public Tool currentTool;

	public Texture2D brushTexture;

	public Vector2 brushScale;

	public Color brushColor;

	public float brushSpacing;

	public float brushSpacingFactor;

	public RawImage brushImage;

	public float brushPreviewScaleMultiplier;

	public Texture2D stampTexture;

	public RotateMode brushRotation;

	public bool applyDefaults;

	public Texture2D defaltBrushTexture;

	public float defaultBrushSize;

	public Color defaultBrushColor;

	public float defaultBrushAlpha;

	public float maxBrushScale;

	public RustButton UndoButton;

	public RustButton RedoButton;

	public GameObject BackgroundBlocker;

	public FlexibleColorPicker ColourPicker;

	[NonSerialized]
	public bool usingLineTool;

	[NonSerialized]
	public bool isDrawingLine;

	[NonSerialized]
	public Vector3 startLinePosition;

	[NonSerialized]
	public Vector3 endLinePosition;

	[NonSerialized]
	public Vector3? lineDirection;

	public MeshPaintController()
	{
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		brushScale = new Vector2(8f, 8f);
		brushColor = Color.white;
		brushSpacing = 2f;
		brushSpacingFactor = 0.25f;
		brushPreviewScaleMultiplier = 1f;
		defaultBrushSize = 16f;
		defaultBrushColor = Color.black;
		defaultBrushAlpha = 0.5f;
		maxBrushScale = 32f;
		((MonoBehaviour)this)._002Ector();
	}
}
