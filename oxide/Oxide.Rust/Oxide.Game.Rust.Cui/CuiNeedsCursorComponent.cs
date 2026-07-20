namespace Oxide.Game.Rust.Cui;

public class CuiNeedsCursorComponent : ICuiComponent, ICuiEnableable
{
	public string Type => "NeedsCursor";

	public bool? Enabled { get; set; }
}
