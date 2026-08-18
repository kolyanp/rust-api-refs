using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using ProtoBuf;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
public class CliffPlacementSandbox : MonoBehaviour
{
	public enum TerrainPatch
	{
		Flat,
		SlopeX,
		Ridge,
		ConvexDome,
		ConcaveBowl
	}

	public enum TerrainSource
	{
		ProceduralReal,
		CannedPatch,
		MapFileRegion
	}

	private Material _gizmoMat;

	private readonly List<TerrainAnchor> _gizmoAnchors;

	private readonly List<TerrainModifier> _gizmoModifiers;

	private int _gizmoTargetsFrame;

	private readonly HashSet<Transform> _selectedGizmoRoots;

	private static readonly Color AnchorColor;

	private static readonly Color HeightSetColor;

	private static readonly Color HeightRaiseColor;

	private static readonly Color HeightAddColor;

	private static readonly Color OtherModColor;

	private WorldSerialization _mapSerialization;

	private short[] _mapHeights;

	private int _mapRes;

	private Vector3 _mapWorldPos;

	private Vector3 _mapWorldSize;

	private short[] _preCliffHeights;

	private int _preCliffRes;

	private bool _preCliffLoaded;

	private float[] _bakedRegion;

	private string _preCliffStatus;

	[Tooltip("ProceduralReal runs the game's real base heightmap generator (GenerateHeight). CannedPatch uses a simple analytic patch.")]
	[Header("Terrain source")]
	public TerrainSource Source;

	[Tooltip("World seed fed to the real generator. 0 = auto.")]
	[Header("Procedural (real Rust base heightmap)")]
	public uint Seed;

	[Tooltip("Square map size in metres. Real maps are thousands; smaller = quicker but less varied.")]
	public float ProceduralMapSize;

	[Tooltip("Vertical height range in metres (terrain Size.y).")]
	public float ProceduralHeightRange;

	[Tooltip("Auto-slope finder: acceptable terrain steepness (degrees) for dropping the cliff.")]
	public int SlopeFinderMinAngle;

	public int SlopeFinderMaxAngle;

	[Header("Terrain")]
	[Tooltip("Unity heightmap resolution. Snapped to the nearest 2^n+1 by Unity.")]
	public int HeightmapResolution;

	[Tooltip("World-space size of the sandbox terrain (x/z = extent, y = height range).")]
	public Vector3 TerrainSize;

	[Tooltip("World-space origin (bottom-south-west corner) of the sandbox terrain.")]
	public Vector3 TerrainOrigin;

	[Tooltip("Which canned height patch to seed the terrain with.")]
	public TerrainPatch CurrentPatch;

	[Header("Map file region (real .map crop)")]
	[Tooltip("Path to a real, shipped .map file. Use the inspector's drag/drop or picker to set it.")]
	public string MapFilePath;

	[Tooltip("World-space X/Z centre of the region to crop out of the map (Y is ignored).")]
	public Vector3 RegionCenter;

	[Tooltip("Side length in metres of the square region cropped from the map.")]
	public float RegionSize;

	[Tooltip("World-Y that normalized height 0 maps to. Rust map heights are sea-level centred, so the terrain sits at y = -500 with a 1000m range (matches the shipped map loader). Nudge this if the terrain sits above/below the spawned cliffs.")]
	public float MapWorldYOffset;

	[Tooltip("Auto-pick the region resolution to match the source map's per-cell detail over the cropped window. Turn off to use MapRegionResolution directly.")]
	public bool AutoMapRegionResolution;

	[Tooltip("Heightmap resolution of the cropped sandbox terrain (snapped to 2^n+1). Used when AutoMapRegionResolution is off; otherwise shows the last auto-computed value.")]
	public int MapRegionResolution;

	[Tooltip("Spawn the real cliff prefabs that the map placed inside the region (kept linked to their prefab assets, so editing the prefab and recalculating shows the effect).")]
	public bool SpawnRealCliffs;

	[Tooltip("Only spawn decor prefabs whose asset path looks like a cliff/rock, instead of all decor in the region.")]
	public bool CliffPrefabsOnly;

	[Tooltip("Use the cached pre-cliff terrain (T0) as the recalc baseline instead of the baked map terrain. T0 is captured once via 'Tools > Cliff Sandbox > Arm Pre-Cliff Terrain Capture' during a real generation of this map's seed/size. When off (or no cache), recalc falls back to the baked map terrain, which can produce spurious gaps.")]
	public bool UsePreCliffBaseline;

	[Tooltip("Procedural-generation scene the one-click 'Bake pre-cliff T0' button drives to capture T0. Must be a full generator scene (engine bootstrap + generating World Setup), e.g. the shipped 'Procedural Map' scene. Only used by the editor bake button.")]
	public string GenerationScenePath;

	[Tooltip("The cliff prefab instance to place. Assign via the inspector dropdown or drag one in.")]
	[Header("Placement")]
	public Transform cliffRoot;

	[Tooltip("Anchor solve mode. PlaceCliffs uses MaximizeHeight for the first cliff of a chain.")]
	public TerrainAnchorMode AnchorMode;

	[Tooltip("Snap the cliff root to the anchored Y after placing, so it visually follows the solve.")]
	public bool SnapCliffToAnchoredHeight;

	[Tooltip("When recalculating in Map File Region mode, re-apply each cliff's terrain modifiers at its real recorded map position instead of re-solving its anchors and moving it. This makes the recalc a faithful preview of the generated terrain around the cliff (anchors are still reported accepted/rejected for info). Turn off to also re-solve and move cliffs.")]
	public bool RecalcKeepMapPositions;

	[Tooltip("When recalculating in Map File Region mode, re-spawn the real cliffs from their source prefab assets first so edits made to the prefab in the Project window are reflected. Spawned cliffs are plain clones (not linked to the asset), so without this a recalc would re-carve using the stale, pre-edit cliff instances. Turn off to recalc the exact instances currently in the scene (e.g. after moving them by hand).")]
	public bool ResyncCliffsFromPrefabsOnRecalc;

	[Tooltip("Hot-reload: before Carve selected / Re-anchor selected run, re-spawn the selected cliff(s) from their source prefab assets so edits made in Prefab Mode (or the Project window) apply immediately - no need to exit and re-enter play mode. Spawned cliffs are plain clones not linked to the asset, so without this Carve/Re-anchor keep using the stale, pre-edit clone. Turn off to act on the exact instances in the scene (e.g. after hand-moving them).")]
	public bool HotReloadPrefabsBeforeAction;

	private int _nextSandboxCliffId;

	[Tooltip("Replay each cliff's TerrainPlacement heightmap stamps during recalc, in addition to its TerrainModifiers. Rocks/formations flatten and blend the terrain under themselves with these stamps; the generator applies them before later pieces solve their anchors. Without replaying them the re-solve samples rougher, un-flattened terrain and rejects anchors the real map accepted. Turn off to replay only the height modifiers (the older behaviour).")]
	public bool ReplayTerrainPlacementsOnRecalc;

	[Tooltip("When re-anchoring, restrict the re-solve + reposition to cliffs whose name contains this text (case-insensitive); every other cliff stays locked to its faithful recorded map position. Leave empty to re-anchor every cliff. Re-anchoring ALL cliffs moves earlier pieces, which shifts the terrain later pieces were anchored to - so to test one edited prefab, set this to its name (e.g. 'cliff_hills_large_b') and only it will move.")]
	public string ReAnchorOnlyName;

	[Header("Placement gizmos (play mode)")]
	[Tooltip("Draw TerrainAnchor / TerrainModifier gizmos in the Game view while playing (the built-in gizmos only show in the Scene view and are disabled in play mode).")]
	public bool ShowPlacementGizmos;

	[Tooltip("Include TerrainAnchor gizmos (vertical solve range + radius).")]
	public bool GizmoAnchors;

	[Tooltip("Include TerrainHeightSet modifier gizmos (radius ring).")]
	public bool GizmoModifierHeightSet;

	[Tooltip("Include TerrainHeightRaise modifier gizmos (radius ring).")]
	public bool GizmoModifierHeightRaise;

	[Tooltip("Include TerrainHeightAdd modifier gizmos (radius ring).")]
	public bool GizmoModifierHeightAdd;

	[Tooltip("Include any other (non-height) TerrainModifier gizmos (radius ring).")]
	public bool GizmoModifierOther;

	[Tooltip("Only draw placement gizmos within this many metres of the camera (0 = no limit). Keeps dense real-map regions readable by hiding far-away gizmos.")]
	public float GizmoDrawDistance;

	[Tooltip("Click-to-select mode: instead of drawing every gizmo in range, left-click a cliff to toggle its gizmos on, click again to turn them off. Several cliffs can be selected at once.")]
	public bool GizmoSelectionMode;

	[Tooltip("Spawn a bright, roughly player-sized capsule on the terrain so you can judge scale against the cliffs. Use 'Marker where I'm looking' or the J key to drop it under the aim.")]
	[Header("Scale reference")]
	public bool ShowPlayerScaleReference;

	[Tooltip("Height of the scale-reference capsule in metres (Rust player is about 1.8m).")]
	public float PlayerReferenceHeight;

	[Header("Camera (play-mode freecam)")]
	[Tooltip("Hold right-mouse in Game view to fly: WASD move, Q/E down/up, Shift sprint, scroll = speed.")]
	public bool EnableFreecam;

	[Tooltip("Base freecam move speed in metres/second (adjust live with the scroll wheel while flying).")]
	public float FreecamMoveSpeed;

	[Tooltip("Speed multiplier while holding Shift.")]
	public float FreecamSprintMultiplier;

	[Tooltip("Mouse-look sensitivity (degrees per pixel of mouse delta).")]
	public float FreecamLookSensitivity;

	[Tooltip("On Initialize, move the main camera to a vantage overlooking the terrain centre.")]
	public bool MoveCameraOnInitialize;

	[Tooltip("After Place / Auto-place / Recalculate, fly the camera over to frame the cliff.")]
	public bool FrameCameraOnPlacedCliff;

	[Header("Hotkeys / UI")]
	public bool DrawOnScreenControls;

	private GameObject _terrainGO;

	private TerrainData _terrainData;

	private TerrainMeta _meta;

	private TerrainHeightMap _heightmap;

	private int _res;

	private float[] _baseline;

	private bool _initialized;

	private float[] _preCarve;

	private string _preCarveSelectionKey;

	private string _lastPlaceInfo;

	private string _lastAnchorBreakdown;

	private string _breakdownSubject;

	private bool _editingTextField;

	private bool _breakdownCapturedRejected;

	private readonly List<GameObject> _spawnedCliffs;

	private bool _freecamActive;

	private float _freecamYaw;

	private float _freecamPitch;

	private GameObject _playerScaleRef;

	private int SelectedGizmoCount
	{
		get
		{
			_selectedGizmoRoots.RemoveWhere((Transform r) => (Object)(object)r == (Object)null);
			return _selectedGizmoRoots.Count;
		}
	}

	public string PreCliffStatus => _preCliffStatus;

	public bool PreCliffLoaded => _preCliffLoaded;

	private void FrameCameraOnCliff(Transform root)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		Camera main = Camera.main;
		if (!((Object)(object)main == (Object)null) && !((Object)(object)root == (Object)null))
		{
			Bounds val = CalcHierarchyBounds(root);
			Vector3 val2 = ((Bounds)(ref val)).extents;
			float num = Mathf.Max(((Vector3)(ref val2)).magnitude, 5f) * 3f;
			val2 = Quaternion.Euler(30f, -45f, 0f) * Vector3.forward;
			Vector3 normalized = ((Vector3)(ref val2)).normalized;
			((Component)main).transform.position = ((Bounds)(ref val)).center - normalized * num;
			((Component)main).transform.rotation = Quaternion.LookRotation(normalized, Vector3.up);
			if (main.farClipPlane < num * 4f)
			{
				main.farClipPlane = num * 4f;
			}
		}
	}

	private static Bounds CalcHierarchyBounds(Transform root)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		Renderer[] componentsInChildren = ((Component)root).GetComponentsInChildren<Renderer>();
		if (componentsInChildren.Length == 0)
		{
			return new Bounds(root.position, Vector3.one * 5f);
		}
		Bounds bounds = componentsInChildren[0].bounds;
		for (int i = 1; i < componentsInChildren.Length; i++)
		{
			((Bounds)(ref bounds)).Encapsulate(componentsInChildren[i].bounds);
		}
		return bounds;
	}

	private void EnsureGizmoMaterial()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		if (!((Object)(object)_gizmoMat != (Object)null))
		{
			Shader val = Shader.Find("Hidden/Internal-Colored");
			_gizmoMat = new Material(val)
			{
				hideFlags = (HideFlags)61
			};
			_gizmoMat.SetInt("_SrcBlend", 5);
			_gizmoMat.SetInt("_DstBlend", 10);
			_gizmoMat.SetInt("_Cull", 0);
			_gizmoMat.SetInt("_ZWrite", 0);
			_gizmoMat.SetInt("_ZTest", 8);
		}
	}

	private void RefreshGizmoTargets()
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016e: Unknown result type (might be due to invalid IL or missing references)
		if (_gizmoTargetsFrame == Time.frameCount)
		{
			return;
		}
		_gizmoTargetsFrame = Time.frameCount;
		_gizmoAnchors.Clear();
		_gizmoModifiers.Clear();
		if (GizmoSelectionMode)
		{
			RefreshGizmoTargetsFromSelection();
			return;
		}
		Camera main = Camera.main;
		bool flag = (Object)(object)main != (Object)null && GizmoDrawDistance > 0f;
		Vector3 val = (((Object)(object)main != (Object)null) ? ((Component)main).transform.position : Vector3.zero);
		float num = GizmoDrawDistance * GizmoDrawDistance;
		Vector3 val2;
		if (GizmoAnchors)
		{
			TerrainAnchor[] array = Object.FindObjectsByType<TerrainAnchor>((FindObjectsInactive)1, (FindObjectsSortMode)0);
			foreach (TerrainAnchor terrainAnchor in array)
			{
				if ((Object)(object)_terrainGO != (Object)null && ((Component)terrainAnchor).transform.IsChildOf(_terrainGO.transform))
				{
					continue;
				}
				if (flag)
				{
					val2 = ((Component)terrainAnchor).transform.position - val;
					if (((Vector3)(ref val2)).sqrMagnitude > num)
					{
						continue;
					}
				}
				_gizmoAnchors.Add(terrainAnchor);
			}
		}
		if (!AnyModifierGizmoEnabled())
		{
			return;
		}
		TerrainModifier[] array2 = Object.FindObjectsByType<TerrainModifier>((FindObjectsInactive)1, (FindObjectsSortMode)0);
		foreach (TerrainModifier terrainModifier in array2)
		{
			if (((Object)(object)_terrainGO != (Object)null && ((Component)terrainModifier).transform.IsChildOf(_terrainGO.transform)) || !IsModifierGizmoEnabled(terrainModifier))
			{
				continue;
			}
			if (flag)
			{
				val2 = ((Component)terrainModifier).transform.position - val;
				if (((Vector3)(ref val2)).sqrMagnitude > num)
				{
					continue;
				}
			}
			_gizmoModifiers.Add(terrainModifier);
		}
	}

	private void RefreshGizmoTargetsFromSelection()
	{
		_selectedGizmoRoots.RemoveWhere((Transform r) => (Object)(object)r == (Object)null);
		foreach (Transform selectedGizmoRoot in _selectedGizmoRoots)
		{
			if (GizmoAnchors)
			{
				TerrainAnchor[] componentsInChildren = ((Component)selectedGizmoRoot).GetComponentsInChildren<TerrainAnchor>(true);
				foreach (TerrainAnchor item in componentsInChildren)
				{
					_gizmoAnchors.Add(item);
				}
			}
			if (!AnyModifierGizmoEnabled())
			{
				continue;
			}
			TerrainModifier[] componentsInChildren2 = ((Component)selectedGizmoRoot).GetComponentsInChildren<TerrainModifier>(true);
			foreach (TerrainModifier terrainModifier in componentsInChildren2)
			{
				if (IsModifierGizmoEnabled(terrainModifier))
				{
					_gizmoModifiers.Add(terrainModifier);
				}
			}
		}
	}

	private void ToggleGizmoSelection(Transform root)
	{
		if (!((Object)(object)root == (Object)null) && !_selectedGizmoRoots.Remove(root))
		{
			_selectedGizmoRoots.Add(root);
		}
	}

	private void ClearGizmoSelection()
	{
		_selectedGizmoRoots.Clear();
	}

	private bool AnyModifierGizmoEnabled()
	{
		if (!GizmoModifierHeightSet && !GizmoModifierHeightRaise && !GizmoModifierHeightAdd)
		{
			return GizmoModifierOther;
		}
		return true;
	}

	private bool IsModifierGizmoEnabled(TerrainModifier modifier)
	{
		if (modifier is TerrainHeightSet)
		{
			return GizmoModifierHeightSet;
		}
		if (modifier is TerrainHeightRaise)
		{
			return GizmoModifierHeightRaise;
		}
		if (modifier is TerrainHeightAdd)
		{
			return GizmoModifierHeightAdd;
		}
		return GizmoModifierOther;
	}

	private void OnRenderObject()
	{
		if (ShowPlacementGizmos && Application.isPlaying && (GizmoAnchors || AnyModifierGizmoEnabled()))
		{
			RefreshGizmoTargets();
			EnsureGizmoMaterial();
			_gizmoMat.SetPass(0);
			GL.PushMatrix();
			GL.Begin(1);
			for (int i = 0; i < _gizmoAnchors.Count; i++)
			{
				DrawAnchorGizmo(_gizmoAnchors[i]);
			}
			for (int j = 0; j < _gizmoModifiers.Count; j++)
			{
				DrawModifierGizmo(_gizmoModifiers[j]);
			}
			GL.End();
			GL.PopMatrix();
		}
	}

	private void DrawAnchorGizmo(TerrainAnchor anchor)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		Vector3 position = ((Component)anchor).transform.position;
		Vector3 lossyScale = ((Component)anchor).transform.lossyScale;
		float num = 1f + anchor.SlopeScale * Mathf.InverseLerp(0f, 90f, Vector3.Angle(Vector3.up, ((Component)anchor).transform.up));
		float num2 = anchor.Extents * lossyScale.y * num;
		float num3 = anchor.Offset * lossyScale.y * num;
		Vector3 val = position + Vector3.up * (num3 - num2);
		Vector3 val2 = position + Vector3.up * (num3 + num2);
		DrawLine(val, val2, AnchorColor);
		DrawLine(val - Vector3.right * 0.5f, val + Vector3.right * 0.5f, AnchorColor);
		DrawLine(val2 - Vector3.right * 0.5f, val2 + Vector3.right * 0.5f, AnchorColor);
		if (anchor.Radius > 0f)
		{
			DrawCircleY(position, anchor.Radius, AnchorColor);
		}
	}

	private void DrawModifierGizmo(TerrainModifier modifier)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		float num = ((Component)modifier).transform.lossyScale.y * modifier.Radius;
		if (!(num <= 0f))
		{
			DrawCircleY(((Component)modifier).transform.position, num, ModifierColor(modifier));
		}
	}

	private static Color ModifierColor(TerrainModifier modifier)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		if (modifier is TerrainHeightSet)
		{
			return HeightSetColor;
		}
		if (modifier is TerrainHeightRaise)
		{
			return HeightRaiseColor;
		}
		if (modifier is TerrainHeightAdd)
		{
			return HeightAddColor;
		}
		return OtherModColor;
	}

	private static void DrawLine(Vector3 a, Vector3 b, Color color)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		GL.Color(color);
		GL.Vertex(a);
		GL.Vertex(b);
	}

	private static void DrawCircleY(Vector3 center, float radius, Color color)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		GL.Color(color);
		Vector3 val = center + new Vector3(radius, 0f, 0f);
		for (int i = 1; i <= 32; i++)
		{
			float num = (float)i / 32f * MathF.PI * 2f;
			Vector3 val2 = center + new Vector3(Mathf.Cos(num) * radius, 0f, Mathf.Sin(num) * radius);
			GL.Vertex(val);
			GL.Vertex(val2);
			val = val2;
		}
	}

	private bool TryLoadMapForRegion(out string error)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0154: Unknown result type (might be due to invalid IL or missing references)
		error = null;
		if (string.IsNullOrEmpty(MapFilePath) || !File.Exists(MapFilePath))
		{
			error = "map file not found: '" + MapFilePath + "'. Set one in the inspector.";
			return false;
		}
		WorldSerialization val = new WorldSerialization();
		try
		{
			val.Load(MapFilePath);
		}
		catch (Exception ex)
		{
			error = "failed to read '" + MapFilePath + "': " + ex.Message;
			return false;
		}
		MapData map = val.GetMap("terrain");
		if (map == null || map.data == null || map.data.Length == 0)
		{
			error = "the .map has no 'terrain' heightmap layer.";
			return false;
		}
		int num = Mathf.RoundToInt(Mathf.Sqrt((float)map.data.Length / 2f));
		if (num * num * 2 != map.data.Length)
		{
			error = $"unexpected terrain map size ({map.data.Length} bytes) - not a square short grid.";
			return false;
		}
		short[] array = new short[num * num];
		Buffer.BlockCopy(map.data, 0, array, 0, map.data.Length);
		_mapSerialization = val;
		_mapHeights = array;
		_mapRes = num;
		float num2 = val.world.size;
		_mapWorldPos = new Vector3((0f - num2) * 0.5f, MapWorldYOffset, (0f - num2) * 0.5f);
		_mapWorldSize = new Vector3(num2, 1000f, num2);
		return true;
	}

	private int ComputeMapRegionResolution()
	{
		float num = _mapWorldSize.x / (float)Mathf.Max(1, _mapRes - 1);
		int num2 = Mathf.CeilToInt(RegionSize / Mathf.Max(0.0001f, num)) + 1;
		int num3 = 33;
		while (num3 < num2 && num3 < 4097)
		{
			num3 = (num3 - 1) * 2 + 1;
		}
		return Mathf.Clamp(num3, 65, 2049);
	}

	private void FillFromMapRegion()
	{
		float num = Mathf.Max(1f, RegionSize) * 0.5f;
		float num2 = RegionCenter.x - num;
		float num3 = RegionCenter.z - num;
		float num4 = ((_res <= 1) ? 1 : (_res - 1));
		for (int i = 0; i < _res; i++)
		{
			float worldZ = num3 + (float)i / num4 * RegionSize;
			for (int j = 0; j < _res; j++)
			{
				float worldX = num2 + (float)j / num4 * RegionSize;
				_heightmap.SetHeight(j, i, SampleMapHeight01(worldX, worldZ));
			}
		}
	}

	private float SampleMapHeight01(float worldX, float worldZ)
	{
		float num = Mathf.Clamp01((worldX - _mapWorldPos.x) / _mapWorldSize.x);
		float num2 = Mathf.Clamp01((worldZ - _mapWorldPos.z) / _mapWorldSize.z);
		float num3 = num * (float)(_mapRes - 1);
		float num4 = num2 * (float)(_mapRes - 1);
		int num5 = Mathf.Clamp((int)num3, 0, _mapRes - 1);
		int num6 = Mathf.Clamp((int)num4, 0, _mapRes - 1);
		int num7 = Mathf.Min(num5 + 1, _mapRes - 1);
		int num8 = Mathf.Min(num6 + 1, _mapRes - 1);
		float num9 = num3 - (float)num5;
		float num10 = num4 - (float)num6;
		float num11 = BitUtility.Short2Float((int)_mapHeights[num6 * _mapRes + num5]);
		float num12 = BitUtility.Short2Float((int)_mapHeights[num6 * _mapRes + num7]);
		float num13 = BitUtility.Short2Float((int)_mapHeights[num8 * _mapRes + num5]);
		float num14 = BitUtility.Short2Float((int)_mapHeights[num8 * _mapRes + num7]);
		float num15 = Mathf.Lerp(num11, num12, num9);
		float num16 = Mathf.Lerp(num13, num14, num9);
		return Mathf.Lerp(num15, num16, num10);
	}

	private void SpawnRealCliffsInRegion()
	{
	}

	private static bool LooksLikeCliff(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return false;
		}
		string text = path.ToLowerInvariant();
		if (!text.Contains("cliff") && !text.Contains("rock") && !text.Contains("formation") && !text.Contains("iceberg"))
		{
			return text.Contains("ice_sheet");
		}
		return true;
	}

	private void EnsurePlayerScaleReference(bool reposition)
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
		if (!Application.isPlaying)
		{
			return;
		}
		if (!ShowPlayerScaleReference)
		{
			DestroyPlayerScaleRef();
		}
		else if (_initialized && !((Object)(object)_heightmap == (Object)null))
		{
			if ((Object)(object)_playerScaleRef == (Object)null)
			{
				_playerScaleRef = CreatePlayerScaleReference();
				reposition = true;
			}
			_playerScaleRef.transform.localScale = new Vector3(0.5f, Mathf.Max(0.1f, PlayerReferenceHeight) * 0.5f, 0.5f);
			if (reposition)
			{
				Vector3 val = TerrainMeta.Position + new Vector3(TerrainMeta.Size.x * 0.5f, 0f, TerrainMeta.Size.z * 0.5f);
				PlacePlayerRefAtXZ(val.x, val.z);
			}
		}
	}

	public void PlacePlayerScaleRefAtLookTarget()
	{
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		if (!Application.isPlaying || !_initialized || (Object)(object)_heightmap == (Object)null)
		{
			return;
		}
		ShowPlayerScaleReference = true;
		EnsurePlayerScaleReference(reposition: false);
		if ((Object)(object)_playerScaleRef == (Object)null)
		{
			return;
		}
		Camera main = Camera.main;
		if ((Object)(object)main == (Object)null)
		{
			return;
		}
		Vector3 position = ((Component)main).transform.position;
		Vector3 forward = ((Component)main).transform.forward;
		RaycastHit val = default(RaycastHit);
		Vector3 val2;
		if (Physics.Raycast(position, forward, ref val, 100000f, -1, (QueryTriggerInteraction)1))
		{
			val2 = ((RaycastHit)(ref val)).point;
		}
		else
		{
			float num = TerrainMeta.Position.y + TerrainMeta.Size.y * 0.5f;
			if (Mathf.Abs(forward.y) > 0.0001f)
			{
				float num2 = (num - position.y) / forward.y;
				val2 = ((num2 > 0f) ? (position + forward * num2) : position);
			}
			else
			{
				val2 = position;
			}
		}
		PlacePlayerRefAtXZ(val2.x, val2.z);
	}

	public void GoToMarker()
	{
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		if (Application.isPlaying && _initialized && !((Object)(object)_heightmap == (Object)null))
		{
			ShowPlayerScaleReference = true;
			EnsurePlayerScaleReference(reposition: false);
			Camera main = Camera.main;
			if (!((Object)(object)main == (Object)null))
			{
				Vector3 val = (((Object)(object)_playerScaleRef != (Object)null) ? _playerScaleRef.transform.position : (TerrainMeta.Position + new Vector3(TerrainMeta.Size.x * 0.5f, 0f, TerrainMeta.Size.z * 0.5f)));
				float num = Mathf.Max(0.1f, PlayerReferenceHeight);
				Vector3 val2 = Quaternion.Euler(20f, -45f, 0f) * Vector3.forward;
				Vector3 normalized = ((Vector3)(ref val2)).normalized;
				float num2 = Mathf.Max(6f, num * 4f);
				((Component)main).transform.position = val - normalized * num2 + Vector3.up * (num * 0.5f);
				Transform transform = ((Component)main).transform;
				val2 = val - ((Component)main).transform.position;
				transform.rotation = Quaternion.LookRotation(((Vector3)(ref val2)).normalized, Vector3.up);
			}
		}
	}

	public void MovePlayerScaleRefToCamera()
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		if (Application.isPlaying && _initialized && !((Object)(object)_heightmap == (Object)null))
		{
			ShowPlayerScaleReference = true;
			EnsurePlayerScaleReference(reposition: false);
			if (!((Object)(object)_playerScaleRef == (Object)null))
			{
				Camera main = Camera.main;
				Vector3 val = (((Object)(object)main != (Object)null) ? ((Component)main).transform.position : _playerScaleRef.transform.position);
				PlacePlayerRefAtXZ(val.x, val.z);
			}
		}
	}

	private void PlacePlayerRefAtXZ(float worldX, float worldZ)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)_playerScaleRef == (Object)null))
		{
			float x = TerrainMeta.Position.x;
			float num = TerrainMeta.Position.x + TerrainMeta.Size.x;
			float z = TerrainMeta.Position.z;
			float num2 = TerrainMeta.Position.z + TerrainMeta.Size.z;
			worldX = Mathf.Clamp(worldX, x, num);
			worldZ = Mathf.Clamp(worldZ, z, num2);
			float normX = TerrainMeta.NormalizeX(worldX);
			float normZ = TerrainMeta.NormalizeZ(worldZ);
			float height = _heightmap.GetHeight(normX, normZ);
			_playerScaleRef.transform.position = new Vector3(worldX, height + Mathf.Max(0.1f, PlayerReferenceHeight) * 0.5f, worldZ);
		}
	}

	private GameObject CreatePlayerScaleReference()
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		GameObject obj = GameObject.CreatePrimitive((PrimitiveType)1);
		((Object)obj).name = "PlayerScaleReference";
		Collider component = obj.GetComponent<Collider>();
		if ((Object)(object)component != (Object)null)
		{
			if (Application.isPlaying)
			{
				Object.Destroy((Object)(object)component);
			}
			else
			{
				Object.DestroyImmediate((Object)(object)component);
			}
		}
		MeshRenderer component2 = obj.GetComponent<MeshRenderer>();
		if ((Object)(object)component2 != (Object)null)
		{
			((Renderer)component2).shadowCastingMode = (ShadowCastingMode)0;
			((Renderer)component2).material.color = new Color(0.1f, 0.9f, 1f, 1f);
		}
		return obj;
	}

	private void DestroyPlayerScaleRef()
	{
		if (!((Object)(object)_playerScaleRef == (Object)null))
		{
			if (Application.isPlaying)
			{
				Object.Destroy((Object)(object)_playerScaleRef);
			}
			else
			{
				Object.DestroyImmediate((Object)(object)_playerScaleRef);
			}
			_playerScaleRef = null;
		}
	}

	public bool TryGetMapSeedSize(out uint size, out uint seed)
	{
		return TryParseSeedSizeFromMapName(out size, out seed);
	}

	private bool TryParseSeedSizeFromMapName(out uint size, out uint seed)
	{
		size = 0u;
		seed = 0u;
		if (string.IsNullOrEmpty(MapFilePath))
		{
			return false;
		}
		string[] array = Path.GetFileNameWithoutExtension(MapFilePath).Split('.');
		List<uint> list = new List<uint>();
		for (int i = 0; i < array.Length; i++)
		{
			if (uint.TryParse(array[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out var result))
			{
				list.Add(result);
			}
			else
			{
				list.Clear();
			}
		}
		if (list.Count < 3)
		{
			return false;
		}
		size = list[list.Count - 3];
		seed = list[list.Count - 2];
		return size != 0;
	}

	private bool TryLoadPreCliff()
	{
		_preCliffLoaded = false;
		_preCliffHeights = null;
		if (Source != TerrainSource.MapFileRegion)
		{
			_preCliffStatus = "pre-cliff baseline: only used in Map File Region mode";
			return false;
		}
		if (!TryParseSeedSizeFromMapName(out var size, out var seed))
		{
			_preCliffStatus = "pre-cliff baseline: couldn't read seed/size from map filename (expected name.size.seed.protocol.map)";
			return false;
		}
		if (_mapWorldSize.x > 0f && Mathf.Abs((float)size - _mapWorldSize.x) > 1f)
		{
			_preCliffStatus = $"pre-cliff baseline: filename size {size} != map size {_mapWorldSize.x:0}";
			return false;
		}
		Seed = seed;
		_preCliffStatus = "pre-cliff baseline: editor only";
		return false;
	}

	private float SamplePreCliffHeight01(float worldX, float worldZ)
	{
		float num = Mathf.Clamp01((worldX - _mapWorldPos.x) / _mapWorldSize.x);
		float num2 = Mathf.Clamp01((worldZ - _mapWorldPos.z) / _mapWorldSize.z);
		float num3 = num * (float)(_preCliffRes - 1);
		float num4 = num2 * (float)(_preCliffRes - 1);
		int num5 = Mathf.Clamp((int)num3, 0, _preCliffRes - 1);
		int num6 = Mathf.Clamp((int)num4, 0, _preCliffRes - 1);
		int num7 = Mathf.Min(num5 + 1, _preCliffRes - 1);
		int num8 = Mathf.Min(num6 + 1, _preCliffRes - 1);
		float num9 = num3 - (float)num5;
		float num10 = num4 - (float)num6;
		float num11 = BitUtility.Short2Float((int)_preCliffHeights[num6 * _preCliffRes + num5]);
		float num12 = BitUtility.Short2Float((int)_preCliffHeights[num6 * _preCliffRes + num7]);
		float num13 = BitUtility.Short2Float((int)_preCliffHeights[num8 * _preCliffRes + num5]);
		float num14 = BitUtility.Short2Float((int)_preCliffHeights[num8 * _preCliffRes + num7]);
		float num15 = Mathf.Lerp(num11, num12, num9);
		float num16 = Mathf.Lerp(num13, num14, num9);
		return Mathf.Lerp(num15, num16, num10);
	}

	private void ApplyPreCliffBaseline()
	{
		if (_baseline == null)
		{
			return;
		}
		float num = Mathf.Max(1f, RegionSize) * 0.5f;
		float num2 = RegionCenter.x - num;
		float num3 = RegionCenter.z - num;
		float num4 = ((_res <= 1) ? 1 : (_res - 1));
		if (_bakedRegion == null || _bakedRegion.Length != _res * _res)
		{
			_bakedRegion = new float[_res * _res];
		}
		for (int i = 0; i < _res; i++)
		{
			float worldZ = num3 + (float)i / num4 * RegionSize;
			for (int j = 0; j < _res; j++)
			{
				float worldX = num2 + (float)j / num4 * RegionSize;
				_bakedRegion[i * _res + j] = SampleMapHeight01(worldX, worldZ);
			}
		}
		if (!_preCliffLoaded)
		{
			return;
		}
		for (int k = 0; k < _res; k++)
		{
			float worldZ2 = num3 + (float)k / num4 * RegionSize;
			for (int l = 0; l < _res; l++)
			{
				float worldX2 = num2 + (float)l / num4 * RegionSize;
				_baseline[k * _res + l] = SamplePreCliffHeight01(worldX2, worldZ2);
			}
		}
	}

	private string ComputePreCliffValidation()
	{
		if (_bakedRegion == null || (Object)(object)_heightmap == (Object)null)
		{
			return string.Empty;
		}
		float y = _mapWorldSize.y;
		double num = 0.0;
		float num2 = 0f;
		int num3 = 0;
		int num4 = _res * _res;
		for (int i = 0; i < _res; i++)
		{
			for (int j = 0; j < _res; j++)
			{
				float height = _heightmap.GetHeight01(j, i);
				float num5 = _bakedRegion[i * _res + j];
				float num6 = Mathf.Abs(height - num5);
				num += (double)num6;
				if (num6 > num2)
				{
					num2 = num6;
				}
				if (num6 <= 0.00025f)
				{
					num3++;
				}
			}
		}
		float num7 = (float)(num / (double)num4) * y;
		float num8 = num2 * y;
		float num9 = 100f * (float)num3 / (float)num4;
		return $"validation vs baked map: mean {num7:0.00}m, max {num8:0.0}m, {num9:0.0}% within 0.25m";
	}

	private void ClearPreCliffState()
	{
		_preCliffHeights = null;
		_preCliffLoaded = false;
		_bakedRegion = null;
		_preCliffStatus = "pre-cliff baseline: not loaded";
	}

	private int StableCliffKey(Transform root)
	{
		if ((Object)(object)root == (Object)null)
		{
			return 0;
		}
		SandboxCliffSource component = ((Component)root).GetComponent<SandboxCliffSource>();
		if ((Object)(object)component != (Object)null && component.SandboxId != 0)
		{
			return component.SandboxId;
		}
		return ((Object)root).GetInstanceID();
	}

	private void OnDisable()
	{
		Teardown();
	}

	private void OnDestroy()
	{
		Teardown();
	}

	public void CyclePatch()
	{
		CurrentPatch = (TerrainPatch)((int)(CurrentPatch + 1) % Enum.GetValues(typeof(TerrainPatch)).Length);
		if (_initialized)
		{
			FillPatch(CurrentPatch);
			SnapshotBaseline();
			_heightmap.ApplyToTerrain();
		}
	}

	public void InitializeSandbox()
	{
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Expected O, but got Unknown
		//IL_0153: Unknown result type (might be due to invalid IL or missing references)
		//IL_0196: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0354: Unknown result type (might be due to invalid IL or missing references)
		//IL_0363: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Unknown result type (might be due to invalid IL or missing references)
		//IL_0381: Unknown result type (might be due to invalid IL or missing references)
		//IL_0390: Unknown result type (might be due to invalid IL or missing references)
		//IL_039f: Unknown result type (might be due to invalid IL or missing references)
		Teardown();
		bool flag = Source == TerrainSource.ProceduralReal;
		bool flag2 = Source == TerrainSource.MapFileRegion;
		if (flag2 && !TryLoadMapForRegion(out var error))
		{
			Debug.LogError((object)("[CliffSandbox] Map region load failed: " + error));
			return;
		}
		Vector3 val = default(Vector3);
		Vector3 val2 = default(Vector3);
		int num2;
		if (flag2)
		{
			float num = Mathf.Max(1f, RegionSize) * 0.5f;
			((Vector3)(ref val))._002Ector(RegionSize, _mapWorldSize.y, RegionSize);
			((Vector3)(ref val2))._002Ector(RegionCenter.x - num, _mapWorldPos.y, RegionCenter.z - num);
			if (AutoMapRegionResolution)
			{
				MapRegionResolution = ComputeMapRegionResolution();
			}
			num2 = MapRegionResolution;
		}
		else
		{
			val = (Vector3)(flag ? new Vector3(ProceduralMapSize, ProceduralHeightRange, ProceduralMapSize) : TerrainSize);
			val2 = (flag ? (-0.5f * val) : TerrainOrigin);
			val2.y = 0f;
			num2 = HeightmapResolution;
		}
		if (flag)
		{
			World.InitSeed(Seed);
			if (World.Config == null)
			{
				World.Config = new WorldConfig();
			}
		}
		_terrainData = new TerrainData();
		_terrainData.heightmapResolution = Mathf.Max(33, num2);
		_terrainData.size = val;
		_res = _terrainData.heightmapResolution;
		_terrainGO = Terrain.CreateTerrainGameObject(_terrainData);
		((Object)_terrainGO).name = "CliffSandboxTerrain";
		_terrainGO.transform.position = val2;
		_meta = _terrainGO.AddComponent<TerrainMeta>();
		_terrainGO.AddComponent<TerrainHeightMap>();
		_meta.terrainData = _terrainData;
		Terrain terrain = default(Terrain);
		if (!_meta.terrainRenderer.HasTerrain && _terrainGO.TryGetComponent<Terrain>(ref terrain))
		{
			_meta.terrainRenderer.SetTerrain(terrain);
		}
		_meta.Init();
		_meta.SetupComponents();
		_heightmap = TerrainMeta.HeightMap;
		if ((Object)(object)_heightmap == (Object)null)
		{
			Debug.LogError((object)"[CliffSandbox] TerrainHeightMap did not initialize.");
			return;
		}
		_res = _heightmap.res;
		if (flag2)
		{
			FillFromMapRegion();
		}
		else if (flag)
		{
			GenerateRealBaseHeight();
		}
		else
		{
			FillPatch(CurrentPatch);
		}
		SnapshotBaseline();
		if (flag2)
		{
			ClearPreCliffState();
			if (UsePreCliffBaseline)
			{
				TryLoadPreCliff();
			}
			ApplyPreCliffBaseline();
		}
		_heightmap.ApplyToTerrain();
		_initialized = true;
		if (flag2 && SpawnRealCliffs)
		{
			SpawnRealCliffsInRegion();
		}
		EnsurePlayerScaleReference(reposition: true);
		if (MoveCameraOnInitialize)
		{
			MoveCameraToTerrainOverlook(val2, val);
		}
		Vector3 val3 = val2;
		Vector3 val4 = val2 + val;
		string arg = (flag2 ? ("map region '" + Path.GetFileName(MapFilePath) + "'") : (flag ? $"real base heightmap (seed {World.Seed})" : $"patch '{CurrentPatch}'"));
		Debug.Log((object)($"[CliffSandbox] Initialized {_res}x{_res} terrain from {arg}. " + string.Format("Bounds X[{0:0}..{1:0}] Z[{2:0}..{3:0}] Y[{4:0}..{5:0}]. ", new object[6] { val3.x, val4.x, val3.z, val4.z, val3.y, val4.y }) + (flag2 ? "Edit a spawned cliff's prefab, then 'Recalculate all cliffs' (G) to re-solve." : (flag ? "Use 'Auto-place on slope' (F) to drop the cliff on a suitable incline." : "Drop a cliff inside these bounds, assign 'cliffRoot', then Place."))));
	}

	private void MoveCameraToTerrainOverlook(Vector3 origin, Vector3 size)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		Camera main = Camera.main;
		if (!((Object)(object)main == (Object)null) && !((Object)(object)_heightmap == (Object)null))
		{
			int num = _res / 2;
			float num2 = origin.x + ((float)num + 0.5f) / (float)_res * size.x;
			float num3 = origin.z + ((float)num + 0.5f) / (float)_res * size.z;
			float height = _heightmap.GetHeight(num, num);
			Vector3 val = default(Vector3);
			((Vector3)(ref val))._002Ector(num2, height, num3);
			float num4 = Mathf.Clamp(size.x * 0.5f, 60f, 1500f);
			Vector3 val2 = Quaternion.Euler(35f, -45f, 0f) * Vector3.forward;
			((Component)main).transform.position = val - val2 * num4;
			((Component)main).transform.rotation = Quaternion.LookRotation(val2, Vector3.up);
			if (main.farClipPlane < num4 * 4f)
			{
				main.farClipPlane = num4 * 4f;
			}
		}
	}

	public void ResetTerrain()
	{
		if (EnsureReady())
		{
			RestoreBaseline();
			_heightmap.ApplyToTerrain();
		}
	}

	public void PlaceCliff()
	{
		if (!EnsureReady())
		{
			return;
		}
		if ((Object)(object)cliffRoot == (Object)null)
		{
			Debug.LogWarning((object)"[CliffSandbox] No cliffRoot assigned.");
			return;
		}
		RestoreBaseline();
		_lastPlaceInfo = PlaceCliffInstance(cliffRoot);
		_heightmap.ApplyToTerrain();
		if (FrameCameraOnPlacedCliff)
		{
			FrameCameraOnCliff(cliffRoot);
		}
		bool flag = _lastPlaceInfo.Contains("REJECTED");
		Debug.Log((object)("[CliffSandbox] Placed '" + ((Object)cliffRoot).name + "': " + _lastPlaceInfo.Replace('\n', ' ') + (flag ? " | Anchors rejected: the terrain under the cliff doesn't fit the anchor extents. Orient the cliff to the slope (Auto-place F in Procedural mode), move it onto a steeper incline, or check the prefab's TerrainAnchor Extents/Offset." : string.Empty)));
	}

	public void RecalculateAllCliffs()
	{
		if (!EnsureReady())
		{
			return;
		}
		if (ResyncCliffsFromPrefabsOnRecalc && Source == TerrainSource.MapFileRegion && SpawnRealCliffs)
		{
			SpawnRealCliffsInRegion();
		}
		HashSet<Transform> hashSet = CollectCliffRoots();
		if (hashSet.Count == 0)
		{
			Debug.LogWarning((object)"[CliffSandbox] No cliffs found (need a TerrainAnchor/TerrainModifier in the hierarchy).");
			return;
		}
		List<Transform> list = OrderCliffRootsBySpawn(hashSet);
		RestoreBaseline();
		_lastAnchorBreakdown = string.Empty;
		_breakdownCapturedRejected = false;
		int num = 0;
		List<string> list2 = new List<string>();
		foreach (Transform item in list)
		{
			if (PlaceCliffInstance(item).Contains("REJECTED"))
			{
				list2.Add(((Object)item).name);
			}
			else
			{
				num++;
			}
		}
		_heightmap.ApplyToTerrain();
		string text = ComputePreCliffValidation();
		_lastPlaceInfo = $"recalculated {list.Count} cliff(s), {num} accepted";
		if (list2.Count > 0)
		{
			if (!string.IsNullOrEmpty(ReAnchorOnlyName))
			{
				List<string> list3 = list2.FindAll((string n) => n.IndexOf(ReAnchorOnlyName, StringComparison.OrdinalIgnoreCase) >= 0);
				_lastPlaceInfo += ((list3.Count > 0) ? ("\ntarget '" + ReAnchorOnlyName + "' REJECTED (no single height fits its anchors): " + string.Join(", ", list3)) : $"\ntarget '{ReAnchorOnlyName}' anchors OK ({list2.Count} other piece(s) rejected, ignored)");
			}
			else
			{
				string text2 = string.Join(", ", list2.GetRange(0, Mathf.Min(12, list2.Count)));
				if (list2.Count > 12)
				{
					text2 += $", (+{list2.Count - 12} more)";
				}
				_lastPlaceInfo += $"\n{list2.Count} anchor REJECT(s) (no single height fits all anchors): {text2}";
			}
		}
		if (!string.IsNullOrEmpty(text))
		{
			_lastPlaceInfo = _lastPlaceInfo + "\n" + text;
		}
		_breakdownSubject = ReAnchorOnlyName;
		Debug.Log((object)("[CliffSandbox] " + _lastPlaceInfo.Replace('\n', ' ') + "."));
		if (!string.IsNullOrEmpty(_lastAnchorBreakdown))
		{
			Debug.Log((object)("[CliffSandbox] Anchor breakdown (" + ReAnchorOnlyName + "):\n" + _lastAnchorBreakdown));
		}
	}

	public void ReAnchorSelectedCliffs()
	{
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_013c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0148: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0160: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		if (!EnsureReady())
		{
			return;
		}
		List<Transform> list = new List<Transform>();
		foreach (Transform selectedGizmoRoot in _selectedGizmoRoots)
		{
			if ((Object)(object)selectedGizmoRoot != (Object)null && !list.Contains(selectedGizmoRoot))
			{
				list.Add(selectedGizmoRoot);
			}
		}
		if (list.Count == 0)
		{
			_lastPlaceInfo = "Re-anchor selected: nothing selected. Enable 'Click-to-select cliffs' and click the cliff to test, then press this again.";
			_lastAnchorBreakdown = string.Empty;
			Debug.LogWarning((object)"[CliffSandbox] Re-anchor selected: no cliff selected.");
			return;
		}
		_lastAnchorBreakdown = string.Empty;
		_breakdownSubject = "selected";
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Transform item in list)
		{
			TerrainAnchor[] componentsInChildren = ((Component)item).GetComponentsInChildren<TerrainAnchor>(true);
			if (componentsInChildren.Length == 0)
			{
				num3++;
				stringBuilder.Append("\n" + ((Object)item).name + ": no anchors (nothing to solve)");
				continue;
			}
			PrefabAttribute[] attrs = componentsInChildren;
			PrimeAttributes(attrs, item);
			Vector3 position = item.position;
			Quaternion rotation = item.rotation;
			Vector3 lossyScale = item.lossyScale;
			if (string.IsNullOrEmpty(_lastAnchorBreakdown))
			{
				_lastAnchorBreakdown = $"instance @ ({position.x:0},{position.y:0},{position.z:0}):\n" + BuildAnchorBreakdown(componentsInChildren, position, rotation, lossyScale);
			}
			Vector3 pos = position;
			if (item.ApplyTerrainAnchors(componentsInChildren, ref pos, rotation, lossyScale, AnchorMode))
			{
				float num4 = pos.y - position.y;
				item.position = new Vector3(position.x, pos.y, position.z);
				num++;
				stringBuilder.Append($"\n{((Object)item).name}: solved, dY {num4:0.00} -> Y {pos.y:0.0}");
			}
			else
			{
				num2++;
				stringBuilder.Append("\n" + ((Object)item).name + ": REJECTED (no single Y fits its anchors - see breakdown)");
			}
		}
		_lastPlaceInfo = $"Re-anchored {list.Count} selected cliff(s): {num} moved, {num2} rejected" + ((num3 > 0) ? $", {num3} anchor-less" : string.Empty) + " (terrain unchanged)." + stringBuilder.ToString();
		Debug.Log((object)("[CliffSandbox] " + _lastPlaceInfo.Replace('\n', ' ')));
		if (!string.IsNullOrEmpty(_lastAnchorBreakdown))
		{
			Debug.Log((object)("[CliffSandbox] Anchor breakdown (selected):\n" + _lastAnchorBreakdown));
		}
	}

	public void CarveSelectedCliffs()
	{
		//IL_0318: Unknown result type (might be due to invalid IL or missing references)
		//IL_031a: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0279: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Unknown result type (might be due to invalid IL or missing references)
		//IL_0286: Unknown result type (might be due to invalid IL or missing references)
		//IL_028b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		if (!EnsureReady())
		{
			return;
		}
		List<Transform> list = new List<Transform>();
		foreach (Transform selectedGizmoRoot in _selectedGizmoRoots)
		{
			if ((Object)(object)selectedGizmoRoot != (Object)null && !list.Contains(selectedGizmoRoot))
			{
				list.Add(selectedGizmoRoot);
			}
		}
		if (list.Count == 0)
		{
			_lastPlaceInfo = "Carve selected: nothing selected. Enable 'Click-to-select cliffs' and click the cliff to carve, then press this again.";
			Debug.LogWarning((object)"[CliffSandbox] Carve selected: no cliff selected.");
			return;
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		StringBuilder stringBuilder = new StringBuilder();
		list.Sort((Transform a, Transform b) => StableCliffKey(a).CompareTo(StableCliffKey(b)));
		List<int> list2 = list.ConvertAll(StableCliffKey);
		list2.Sort();
		string text = string.Join(",", list2);
		bool flag = false;
		if (_preCarve != null && _preCarve.Length == _res * _res && _preCarveSelectionKey == text)
		{
			for (int num4 = 0; num4 < _res; num4++)
			{
				for (int num5 = 0; num5 < _res; num5++)
				{
					_heightmap.SetHeight(num5, num4, _preCarve[num4 * _res + num5]);
				}
			}
			flag = true;
		}
		else
		{
			_preCarve = new float[_res * _res];
			for (int num6 = 0; num6 < _res; num6++)
			{
				for (int num7 = 0; num7 < _res; num7++)
				{
					_preCarve[num6 * _res + num7] = _heightmap.GetHeight01(num7, num6);
				}
			}
			_preCarveSelectionKey = text;
		}
		foreach (Transform item in list)
		{
			TerrainModifier[] componentsInChildren = ((Component)item).GetComponentsInChildren<TerrainModifier>(true);
			TerrainPlacement[] componentsInChildren2 = ((Component)item).GetComponentsInChildren<TerrainPlacement>(true);
			List<TerrainModifier> list3 = new List<TerrainModifier>(componentsInChildren.Length);
			PrefabAttribute[] attrs = componentsInChildren2;
			PrimeAttributes(attrs, item);
			for (int num8 = 0; num8 < componentsInChildren.Length; num8++)
			{
				PrimeAttribute(componentsInChildren[num8], item);
				if (IsHeightModifier(componentsInChildren[num8]))
				{
					list3.Add(componentsInChildren[num8]);
				}
			}
			bool flag2 = ReplayTerrainPlacementsOnRecalc && componentsInChildren2.Length != 0;
			if (list3.Count == 0 && !flag2)
			{
				num2++;
				stringBuilder.Append("\n" + ((Object)item).name + ": no height modifiers (nothing to carve)");
				continue;
			}
			Vector3 position = item.position;
			Quaternion rotation = item.rotation;
			Vector3 lossyScale = item.lossyScale;
			int num9 = 0;
			if (flag2)
			{
				try
				{
					item.ApplyTerrainPlacements(componentsInChildren2, position, rotation, lossyScale);
					num9 = componentsInChildren2.Length;
				}
				catch (Exception ex)
				{
					Debug.LogWarning((object)("[CliffSandbox] '" + ((Object)item).name + "' TerrainPlacement replay failed: " + ex.Message));
				}
			}
			int num10 = 0;
			for (int num11 = 0; num11 < list3.Count; num11++)
			{
				if (list3[num11] is TerrainHeightAdd)
				{
					num10++;
				}
			}
			num3 += num10;
			if (list3.Count > 0)
			{
				item.ApplyTerrainModifiers(list3.ToArray(), position, rotation, lossyScale);
			}
			num++;
			stringBuilder.Append($"\n{((Object)item).name}: carved {list3.Count} height mod(s)" + ((num9 > 0) ? $", {num9} placement(s)" : string.Empty) + ((num10 > 0) ? $" ({num10} HeightAdd - accumulates on re-press)" : string.Empty));
		}
		_heightmap.ApplyToTerrain();
		_lastPlaceInfo = $"Carved {num} selected cliff(s) at their current position(s)" + ((num2 > 0) ? $", {num2} without height modifiers" : string.Empty) + (flag ? " (rewound previous carve first)" : string.Empty) + " (baseline untouched)." + ((num3 > 0) ? " Note: HeightAdd re-applies from the rewound base (re-press safe; switching selections back and forth still accumulates)." : string.Empty) + stringBuilder.ToString();
		Debug.Log((object)("[CliffSandbox] " + _lastPlaceInfo.Replace('\n', ' ')));
	}

	private List<Transform> OrderCliffRootsBySpawn(HashSet<Transform> roots)
	{
		List<Transform> list = new List<Transform>(roots.Count);
		HashSet<Transform> hashSet = new HashSet<Transform>();
		for (int i = 0; i < _spawnedCliffs.Count; i++)
		{
			GameObject val = _spawnedCliffs[i];
			if (!((Object)(object)val == (Object)null))
			{
				Transform root = val.transform.root;
				if (roots.Contains(root) && hashSet.Add(root))
				{
					list.Add(root);
				}
			}
		}
		foreach (Transform root2 in roots)
		{
			if (hashSet.Add(root2))
			{
				list.Add(root2);
			}
		}
		return list;
	}

	private HashSet<Transform> CollectCliffRoots()
	{
		HashSet<Transform> hashSet = new HashSet<Transform>();
		TerrainAnchor[] array = Object.FindObjectsByType<TerrainAnchor>((FindObjectsInactive)1, (FindObjectsSortMode)0);
		foreach (TerrainAnchor terrainAnchor in array)
		{
			hashSet.Add(((Component)terrainAnchor).transform.root);
		}
		TerrainModifier[] array2 = Object.FindObjectsByType<TerrainModifier>((FindObjectsInactive)1, (FindObjectsSortMode)0);
		foreach (TerrainModifier terrainModifier in array2)
		{
			hashSet.Add(((Component)terrainModifier).transform.root);
		}
		if ((Object)(object)_terrainGO != (Object)null)
		{
			hashSet.Remove(_terrainGO.transform.root);
		}
		return hashSet;
	}

	private string PlaceCliffInstance(Transform root)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01db: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_0114: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0177: Unknown result type (might be due to invalid IL or missing references)
		//IL_017d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0190: Unknown result type (might be due to invalid IL or missing references)
		//IL_0192: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		TerrainAnchor[] componentsInChildren = ((Component)root).GetComponentsInChildren<TerrainAnchor>(true);
		TerrainModifier[] componentsInChildren2 = ((Component)root).GetComponentsInChildren<TerrainModifier>(true);
		TerrainPlacement[] componentsInChildren3 = ((Component)root).GetComponentsInChildren<TerrainPlacement>(true);
		List<TerrainModifier> list = new List<TerrainModifier>(componentsInChildren2.Length);
		PrefabAttribute[] attrs = componentsInChildren;
		PrimeAttributes(attrs, root);
		attrs = componentsInChildren3;
		PrimeAttributes(attrs, root);
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			PrimeAttribute(componentsInChildren2[i], root);
			if (IsHeightModifier(componentsInChildren2[i]))
			{
				list.Add(componentsInChildren2[i]);
			}
		}
		Vector3 position = root.position;
		Quaternion rotation = root.rotation;
		Vector3 lossyScale = root.lossyScale;
		float y = position.y;
		Vector3 pos = position;
		bool flag = true;
		if (componentsInChildren.Length != 0)
		{
			flag = root.ApplyTerrainAnchors(componentsInChildren, ref pos, rotation, lossyScale, AnchorMode);
		}
		if (!string.IsNullOrEmpty(ReAnchorOnlyName) && ((Object)root).name.IndexOf(ReAnchorOnlyName, StringComparison.OrdinalIgnoreCase) >= 0 && !_breakdownCapturedRejected)
		{
			Vector3 position2 = root.position;
			_lastAnchorBreakdown = $"instance @ ({position2.x:0},{position2.y:0},{position2.z:0}):\n" + BuildAnchorBreakdown(componentsInChildren, position, rotation, lossyScale);
			if (!flag)
			{
				_breakdownCapturedRejected = true;
			}
		}
		bool flag2 = (RecalcKeepMapPositions || (!string.IsNullOrEmpty(ReAnchorOnlyName) && ((Object)root).name.IndexOf(ReAnchorOnlyName, StringComparison.OrdinalIgnoreCase) < 0)) && Source == TerrainSource.MapFileRegion;
		Vector3 pos2 = (flag2 ? position : pos);
		int num = 0;
		if (ReplayTerrainPlacementsOnRecalc && componentsInChildren3.Length != 0)
		{
			try
			{
				root.ApplyTerrainPlacements(componentsInChildren3, pos2, rotation, lossyScale);
				num = componentsInChildren3.Length;
			}
			catch (Exception ex)
			{
				Debug.LogWarning((object)("[CliffSandbox] '" + ((Object)root).name + "' TerrainPlacement replay failed: " + ex.Message));
			}
		}
		TerrainModifier[] array = list.ToArray();
		if (array.Length != 0)
		{
			root.ApplyTerrainModifiers(array, pos2, rotation, lossyScale);
		}
		if (!flag2 && SnapCliffToAnchoredHeight && componentsInChildren.Length != 0)
		{
			root.position = new Vector3(root.position.x, pos.y, root.position.z);
		}
		int num2 = componentsInChildren2.Length - array.Length;
		return string.Format("anchors: {0} ({1})\n", componentsInChildren.Length, flag ? "accepted" : "REJECTED") + string.Format("snap dY: {0:0.00}{1}\n", pos.y - y, flag2 ? " (locked to map pos)" : string.Empty) + $"height mods: {array.Length}" + ((num2 > 0) ? $" ({num2} non-height skipped)" : "") + ((num > 0) ? $", placements: {num}" : string.Empty);
	}

	private static bool IsHeightModifier(TerrainModifier m)
	{
		if (!(m is TerrainHeightSet) && !(m is TerrainHeightRaise))
		{
			return m is TerrainHeightAdd;
		}
		return true;
	}

	private string BuildAnchorBreakdown(TerrainAnchor[] anchors, Vector3 pos, Quaternion rot, Vector3 scale)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0210: Unknown result type (might be due to invalid IL or missing references)
		if (anchors == null || anchors.Length == 0)
		{
			return "no anchors on this cliff";
		}
		float num = float.MinValue;
		float num2 = float.MaxValue;
		int num3 = -1;
		int num4 = -1;
		List<string> list = new List<string>(anchors.Length + 4);
		for (int i = 0; i < anchors.Length; i++)
		{
			TerrainAnchor terrainAnchor = anchors[i];
			Vector3 val = rot * Vector3.Scale(terrainAnchor.worldPosition, scale);
			Vector3 pos2 = pos + val;
			terrainAnchor.Apply(out var height, out var min, out var max, pos2, scale, rot);
			float num5 = min - val.y;
			float num6 = max - val.y;
			if (num5 > num)
			{
				num = num5;
				num3 = i;
			}
			if (num6 < num2)
			{
				num2 = num6;
				num4 = i;
			}
			list.Add(string.Format("  [{0}] {1}: terrainH {2:0.0}  root-Y window [{3:0.0} .. {4:0.0}]", new object[5]
			{
				i,
				((Object)terrainAnchor).name,
				height,
				num5,
				num6
			}) + $"  (E{terrainAnchor.Extents:0.#}/O{terrainAnchor.Offset:0.#}/oY {val.y:0.0})");
		}
		if (num2 > 1f && num < 1f)
		{
			num = 1f;
		}
		string text = ((!(num2 < num)) ? ($"OK: shared root-Y window [{num:0.0} .. {num2:0.0}]. MaximizeHeight seats it at {num2:0.0} " + string.Format("(bound by anchor [{0}] {1}); current Y {2:0.0} -> dY {3:0.0}. ", new object[4]
		{
			num4,
			((Object)anchors[num4]).name,
			pos.y,
			num2 - pos.y
		}) + ((Mathf.Abs(num2 - pos.y) < 0.05f) ? "It won't move because the lowest ceiling equals its current height - your new anchor isn't the binding one (its terrain isn't low enough, given its Extents/Offset)." : "It should move by that dY.")) : ($"REJECTED: no single Y fits all {anchors.Length} anchors. Highest floor {num:0.0} " + $"(anchor [{num3}] {((Object)anchors[num3]).name}) is {num - num2:0.0}m above the " + $"lowest ceiling {num2:0.0} (anchor [{num4}] {((Object)anchors[num4]).name}). " + $"Widen one of their Extents by >= {num - num2:0.0}m, or raise the floor anchor / lower the ceiling anchor."));
		return text + "\n" + string.Join("\n", list);
	}

	private unsafe void GenerateRealBaseHeight()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		WorldConfig worldConfig = World.Config ?? (World.Config = new WorldConfig());
		short* unsafePtr = (short*)NativeArrayUnsafeUtility.GetUnsafePtr<short>(_heightmap.dst);
		GenerateHeight.Native_GenerateHeight(unsafePtr, _heightmap.res, TerrainMeta.Position, TerrainMeta.Size, World.Seed, TerrainMeta.LootAxisAngle, worldConfig.PercentageTier0, worldConfig.PercentageTier1, worldConfig.PercentageTier2, TerrainMeta.BiomeAxisAngle, worldConfig.PercentageBiomeArid, worldConfig.PercentageBiomeTemperate, worldConfig.PercentageBiomeTundra, worldConfig.PercentageBiomeArctic);
	}

	public void AutoPlaceCliffOnSlope()
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0135: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		if (!EnsureReady())
		{
			return;
		}
		if ((Object)(object)cliffRoot == (Object)null)
		{
			Debug.LogWarning((object)"[CliffSandbox] No cliffRoot assigned.");
			return;
		}
		int res = _res;
		int num = Mathf.Max(1, res / 160);
		float num2 = 0.5f * (float)(SlopeFinderMinAngle + SlopeFinderMaxAngle);
		float num3 = float.NegativeInfinity;
		int num4 = -1;
		int num5 = -1;
		Vector3 val = Vector3.up;
		for (int i = 1; i < res - 1; i += num)
		{
			for (int j = 1; j < res - 1; j += num)
			{
				Vector3 normal = _heightmap.GetNormal(j, i);
				float num6 = Vector3.Angle(Vector3.up, normal);
				if (!(num6 < (float)SlopeFinderMinAngle) && !(num6 > (float)SlopeFinderMaxAngle))
				{
					float num7 = 0f - Mathf.Abs(num6 - num2);
					if (num7 > num3)
					{
						num3 = num7;
						num4 = j;
						num5 = i;
						val = normal;
					}
				}
			}
		}
		if (num4 < 0)
		{
			Debug.LogWarning((object)($"[CliffSandbox] No slope in [{SlopeFinderMinAngle}..{SlopeFinderMaxAngle}] deg found. " + "Try a different seed/size or widen the range."));
			return;
		}
		float num8 = ((float)num4 + 0.5f) / (float)res;
		float num9 = ((float)num5 + 0.5f) / (float)res;
		float num10 = TerrainMeta.Position.x + num8 * TerrainMeta.Size.x;
		float num11 = TerrainMeta.Position.z + num9 * TerrainMeta.Size.z;
		float height = _heightmap.GetHeight(num4, num5);
		cliffRoot.position = new Vector3(num10, height, num11);
		cliffRoot.rotation = QuaternionEx.LookRotationForcedUp(val, Vector3.up);
		Debug.Log((object)($"[CliffSandbox] Auto-placed '{((Object)cliffRoot).name}' on {Vector3.Angle(Vector3.up, val):0} deg " + $"slope at ({num10:0},{height:0},{num11:0})."));
		PlaceCliff();
	}

	private void PrimeAttributes(PrefabAttribute[] attrs, Transform root)
	{
		for (int i = 0; i < attrs.Length; i++)
		{
			PrimeAttribute(attrs[i], root);
		}
	}

	private void PrimeAttribute(PrefabAttribute attr, Transform root)
	{
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		attr.worldPosition = root.InverseTransformPoint(((Component)attr).transform.position);
		attr.worldRotation = Quaternion.Inverse(root.rotation) * ((Component)attr).transform.rotation;
		attr.worldForward = attr.worldRotation * Vector3.forward;
	}

	private bool EnsureReady()
	{
		if (_initialized && (Object)(object)_heightmap != (Object)null && _heightmap.res == _res)
		{
			if ((Object)(object)TerrainMeta.HeightMap == (Object)(object)_heightmap)
			{
				return true;
			}
			Debug.LogWarning((object)"[CliffSandbox] The active terrain heightmap changed out from under the sandbox (terrain was re-initialised elsewhere). Press Initialize to rebuild before placing or recalculating cliffs.");
			_initialized = false;
			return false;
		}
		Debug.LogWarning((object)"[CliffSandbox] Not initialized. Press Initialize first.");
		return false;
	}

	private void SnapshotBaseline()
	{
		_preCarve = null;
		_preCarveSelectionKey = null;
		_baseline = new float[_res * _res];
		for (int i = 0; i < _res; i++)
		{
			for (int j = 0; j < _res; j++)
			{
				_baseline[i * _res + j] = _heightmap.GetHeight01(j, i);
			}
		}
	}

	private void RestoreBaseline()
	{
		if (_baseline == null)
		{
			return;
		}
		_preCarve = null;
		_preCarveSelectionKey = null;
		for (int i = 0; i < _res; i++)
		{
			for (int j = 0; j < _res; j++)
			{
				_heightmap.SetHeight(j, i, _baseline[i * _res + j]);
			}
		}
	}

	private void FillPatch(TerrainPatch patch)
	{
		for (int i = 0; i < _res; i++)
		{
			float v = ((_res > 1) ? ((float)i / (float)(_res - 1)) : 0f);
			for (int j = 0; j < _res; j++)
			{
				float u = ((_res > 1) ? ((float)j / (float)(_res - 1)) : 0f);
				_heightmap.SetHeight(j, i, Mathf.Clamp01(SamplePatch(patch, u, v)));
			}
		}
	}

	private static float SamplePatch(TerrainPatch patch, float u, float v)
	{
		switch (patch)
		{
		case TerrainPatch.Flat:
			return 0.3f;
		case TerrainPatch.SlopeX:
			return 0.12f + 0.45f * u;
		case TerrainPatch.Ridge:
		{
			float num3 = 1f - Mathf.Abs(2f * u - 1f);
			return 0.2f + 0.35f * num3;
		}
		case TerrainPatch.ConvexDome:
		{
			float num2 = DistFromCentre(u, v);
			return 0.15f + 0.4f * Mathf.Clamp01(1f - num2);
		}
		case TerrainPatch.ConcaveBowl:
		{
			float num = DistFromCentre(u, v);
			return 0.18f + 0.37f * Mathf.Clamp01(num);
		}
		default:
			return 0.3f;
		}
	}

	private static float DistFromCentre(float u, float v)
	{
		float num = (u - 0.5f) * 2f;
		float num2 = (v - 0.5f) * 2f;
		return Mathf.Clamp01(Mathf.Sqrt(num * num + num2 * num2));
	}

	private void Teardown()
	{
		ClearSpawnedCliffs();
		DestroyPlayerScaleRef();
		ClearGizmoSelection();
		if ((Object)(object)_gizmoMat != (Object)null)
		{
			if (Application.isPlaying)
			{
				Object.Destroy((Object)(object)_gizmoMat);
			}
			else
			{
				Object.DestroyImmediate((Object)(object)_gizmoMat);
			}
			_gizmoMat = null;
		}
		if ((Object)(object)_terrainGO != (Object)null)
		{
			if (Application.isPlaying)
			{
				Object.Destroy((Object)(object)_terrainGO);
			}
			else
			{
				Object.DestroyImmediate((Object)(object)_terrainGO);
			}
			_terrainGO = null;
			_heightmap = null;
		}
		else if ((Object)(object)_heightmap != (Object)null)
		{
			_heightmap.Dispose();
			_heightmap = null;
		}
		if ((Object)(object)_terrainData != (Object)null)
		{
			if (Application.isPlaying)
			{
				Object.Destroy((Object)(object)_terrainData);
			}
			else
			{
				Object.DestroyImmediate((Object)(object)_terrainData);
			}
			_terrainData = null;
		}
		_meta = null;
		_baseline = null;
		_preCarve = null;
		_preCarveSelectionKey = null;
		ClearPreCliffState();
		_initialized = false;
	}

	private void ClearSpawnedCliffs(bool immediate = false)
	{
		for (int i = 0; i < _spawnedCliffs.Count; i++)
		{
			GameObject val = _spawnedCliffs[i];
			if (!((Object)(object)val == (Object)null))
			{
				if (Application.isPlaying && !immediate)
				{
					Object.Destroy((Object)(object)val);
				}
				else
				{
					Object.DestroyImmediate((Object)(object)val);
				}
			}
		}
		_spawnedCliffs.Clear();
		cliffRoot = null;
	}

	public CliffPlacementSandbox()
	{
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		_gizmoAnchors = new List<TerrainAnchor>();
		_gizmoModifiers = new List<TerrainModifier>();
		_gizmoTargetsFrame = -1;
		_selectedGizmoRoots = new HashSet<Transform>();
		_preCliffStatus = "pre-cliff baseline: not loaded";
		Seed = 54321u;
		ProceduralMapSize = 2000f;
		ProceduralHeightRange = 1000f;
		SlopeFinderMinAngle = 30;
		SlopeFinderMaxAngle = 65;
		HeightmapResolution = 513;
		TerrainSize = new Vector3(500f, 100f, 500f);
		TerrainOrigin = Vector3.zero;
		CurrentPatch = TerrainPatch.SlopeX;
		MapFilePath = string.Empty;
		RegionCenter = Vector3.zero;
		RegionSize = 300f;
		MapWorldYOffset = -500f;
		AutoMapRegionResolution = true;
		MapRegionResolution = 513;
		SpawnRealCliffs = true;
		CliffPrefabsOnly = true;
		UsePreCliffBaseline = true;
		GenerationScenePath = "Assets/Scenes/Release/Procedural Map.unity";
		AnchorMode = TerrainAnchorMode.MaximizeHeight;
		SnapCliffToAnchoredHeight = true;
		RecalcKeepMapPositions = true;
		ResyncCliffsFromPrefabsOnRecalc = true;
		HotReloadPrefabsBeforeAction = true;
		_nextSandboxCliffId = 1;
		ReplayTerrainPlacementsOnRecalc = true;
		ReAnchorOnlyName = string.Empty;
		ShowPlacementGizmos = true;
		GizmoAnchors = true;
		GizmoModifierHeightSet = true;
		GizmoModifierHeightRaise = true;
		GizmoModifierHeightAdd = true;
		GizmoModifierOther = true;
		GizmoDrawDistance = 60f;
		GizmoSelectionMode = true;
		ShowPlayerScaleReference = true;
		PlayerReferenceHeight = 1.8f;
		EnableFreecam = true;
		FreecamMoveSpeed = 400f;
		FreecamSprintMultiplier = 5f;
		FreecamLookSensitivity = 0.1f;
		MoveCameraOnInitialize = true;
		FrameCameraOnPlacedCliff = true;
		DrawOnScreenControls = true;
		_lastPlaceInfo = "-";
		_lastAnchorBreakdown = string.Empty;
		_breakdownSubject = string.Empty;
		_spawnedCliffs = new List<GameObject>();
		((MonoBehaviour)this)._002Ector();
	}

	static CliffPlacementSandbox()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		AnchorColor = new Color(0.2f, 0.9f, 1f, 1f);
		HeightSetColor = new Color(0.3f, 1f, 0.4f, 1f);
		HeightRaiseColor = new Color(1f, 0.85f, 0.2f, 1f);
		HeightAddColor = new Color(1f, 0.4f, 0.9f, 1f);
		OtherModColor = new Color(0.8f, 0.8f, 0.8f, 1f);
	}
}
