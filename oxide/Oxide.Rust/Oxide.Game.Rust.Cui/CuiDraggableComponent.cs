using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Oxide.Game.Rust.Cui;

public class CuiDraggableComponent : ICuiComponent, ICuiEnableable
{
	public string Type => "Draggable";

	[JsonProperty("limitToParent")]
	public bool? LimitToParent { get; set; }

	[JsonProperty("maxDistance")]
	public float MaxDistance { get; set; }

	[JsonProperty("allowSwapping")]
	public bool? AllowSwapping { get; set; }

	[JsonProperty("dropAnywhere")]
	public bool? DropAnywhere { get; set; }

	[JsonProperty("dragAlpha")]
	public float DragAlpha { get; set; }

	[JsonProperty("parentLimitIndex")]
	public int ParentLimitIndex { get; set; }

	[JsonProperty("filter")]
	public string Filter { get; set; }

	[JsonProperty("parentPadding")]
	public string ParentPadding { get; set; }

	[JsonProperty("anchorOffset")]
	public string AnchorOffset { get; set; }

	[JsonProperty("keepOnTop")]
	public bool? KeepOnTop { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	[JsonProperty("positionRPC")]
	public CommunityEntity.DraggablePositionSendType PositionRPC { get; set; }

	[JsonProperty("moveToAnchor")]
	public bool MoveToAnchor { get; set; }

	[JsonProperty("rebuildAnchor")]
	public bool RebuildAnchor { get; set; }

	public bool? Enabled { get; set; }
}
