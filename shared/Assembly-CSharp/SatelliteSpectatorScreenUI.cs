using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class SatelliteSpectatorScreenUI : MonoBehaviour
{
	[SerializeField]
	[Header("Map")]
	private RawImage mapImage;

	[SerializeField]
	private RectTransform mapContentRect;

	[SerializeField]
	private float mapZoom = 2f;

	[SerializeField]
	[Header("Markers (optional — created procedurally if unset)")]
	private RectTransform reticleRect;

	[SerializeField]
	private RectTransform impactCircleRect;

	[SerializeField]
	private RectTransform lockedCrashRect;

	[SerializeField]
	[Header("Text")]
	private RustText statusText;

	[SerializeField]
	private RustText driftText;
}
