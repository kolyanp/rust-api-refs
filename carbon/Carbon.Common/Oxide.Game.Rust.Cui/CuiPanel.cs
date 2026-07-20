namespace Oxide.Game.Rust.Cui;

public class CuiPanel
{
	public CuiImageComponent Image { get; set; } = new CuiImageComponent();

	public CuiRawImageComponent RawImage { get; set; }

	public CuiRectTransformComponent RectTransform { get; } = new CuiRectTransformComponent();

	public bool KeyboardEnabled { get; set; }

	public bool CursorEnabled { get; set; }

	public float FadeOut { get; set; }
}
