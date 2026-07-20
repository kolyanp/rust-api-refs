using System.Collections.Generic;
using Newtonsoft.Json;

namespace Oxide.Game.Rust.Cui;

public class CuiElement
{
	[JsonProperty("name")]
	public string Name { get; set; }

	[JsonProperty("parent")]
	public string Parent { get; set; }

	[JsonProperty("destroyUi")]
	public string DestroyUi { get; set; }

	[JsonProperty("components")]
	public List<ICuiComponent> Components { get; } = new List<ICuiComponent>();

	[JsonProperty("fadeOut")]
	public float FadeOut { get; set; }

	[JsonProperty("update")]
	public bool Update { get; set; }

	[JsonProperty("activeSelf")]
	public bool? ActiveSelf { get; set; }
}
