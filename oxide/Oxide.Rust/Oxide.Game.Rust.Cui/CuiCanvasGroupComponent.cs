using Newtonsoft.Json;

namespace Oxide.Game.Rust.Cui;

public class CuiCanvasGroupComponent : ICuiComponent
{
	public string Type => "UnityEngine.UI.CanvasGroup";

	[JsonProperty("alpha")]
	public float? Alpha { get; set; }

	[JsonProperty("blocksRaycasts")]
	public bool? BlocksRaycasts { get; set; }

	[JsonProperty("interactable")]
	public bool? Interactable { get; set; }

	[JsonProperty("fade")]
	public string Fade { get; set; }
}
