using Newtonsoft.Json;

namespace Oxide.Game.Rust.Cui;

public class CuiCanvasGroupComponent : ICuiComponent, ICuiEnableable
{
	public string Type => "UnityEngine.UI.CanvasGroup";

	[JsonProperty("alpha")]
	public float? Alpha { get; set; }

	[JsonProperty("blocksRaycasts")]
	public bool? BlocksRaycasts { get; set; }

	[JsonProperty("interactable")]
	public bool? Interactable { get; set; }

	[JsonProperty("enabled")]
	public bool? Enabled { get; set; }
}
