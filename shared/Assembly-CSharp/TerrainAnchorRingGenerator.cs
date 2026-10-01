using System.Collections.Generic;
using UnityEngine;

public class TerrainAnchorRingGenerator : MonoBehaviour, IEditorComponent
{
	public List<GameObject> Rocks = new List<GameObject>();

	public float PlaneHeight;

	[Tooltip("Raises the generated anchors without moving the slice. The solve seats the root where the anchors meet ground, so the rock sinks by the same amount. Unlike AnchorOffset - a slope-aware vertical tolerance - this bakes into the anchor's local position and rotates with the rock.")]
	public float AnchorLift;

	public float StandOff = 3f;

	public float Spacing = 4f;

	[Range(0.25f, 15f)]
	public float SampleAngleStep = 1.5f;

	[Header("TerrainAnchor Settings")]
	public float AnchorExtents = 0.4f;

	public float AnchorOffset = 0.4f;

	public float AnchorRadius = 0.25f;

	public float AnchorSlopeScale = 1f;

	[HideInInspector]
	public List<TerrainAnchor> GeneratedAnchors = new List<TerrainAnchor>();

	[HideInInspector]
	public TerrainFootprint GeneratedFootprint;
}
