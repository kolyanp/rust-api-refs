using UnityEngine;

public enum EntityNetworkRange
{
	[InspectorName("~128m")]
	Small,
	[InspectorName("~256m (default)")]
	Medium,
	[InspectorName("~512m")]
	Large
}
