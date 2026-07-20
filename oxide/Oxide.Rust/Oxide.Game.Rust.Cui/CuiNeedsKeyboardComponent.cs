namespace Oxide.Game.Rust.Cui;

public class CuiNeedsKeyboardComponent : ICuiComponent, ICuiEnableable
{
	public string Type => "NeedsKeyboard";

	public bool? Enabled { get; set; }
}
