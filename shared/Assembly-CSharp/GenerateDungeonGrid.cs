using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GenerateDungeonGrid : ProceduralComponent
{
	private class PathNode
	{
		public MonumentInfo monument;

		public PathFinder.Node node;
	}

	private class PathSegment
	{
		public PathFinder.Node start;

		public PathFinder.Node end;
	}

	private class PathLink
	{
		public PathLinkSide downwards;

		public PathLinkSide upwards;
	}

	private class PathLinkSide
	{
		public PathLinkSegment origin;

		public List<PathLinkSegment> segments;

		public PathLinkSegment prevSegment
		{
			get
			{
				if (segments.Count <= 0)
				{
					return origin;
				}
				return segments[segments.Count - 1];
			}
		}
	}

	private class PathLinkSegment
	{
		public Vector3 position;

		public Quaternion rotation;

		public Vector3 scale;

		public Prefab<DungeonGridLink> prefab;

		public DungeonGridLink link;

		public Transform downSocket => link.DownSocket;

		public Transform upSocket => link.UpSocket;

		public DungeonGridLinkType downType => link.DownType;

		public DungeonGridLinkType upType => link.UpType;
	}

	private struct PrefabReplacement
	{
		public Vector2i gridPosition;

		public Vector3 worldPosition;

		public int distance;

		public Prefab<DungeonGridCell> prefab;
	}

	public string TunnelFolder = string.Empty;

	public string StationFolder = string.Empty;

	public string UpwardsFolder = string.Empty;

	public string TransitionFolder = string.Empty;

	public string LinkFolder = string.Empty;

	public InfrastructureType ConnectionType = InfrastructureType.Tunnel;

	public int CellSize = 216;

	public float LinkHeight = 1.5f;

	public float LinkRadius = 3f;

	public float LinkTransition = 9f;

	private const int MaxDepth = 100000;

	public override bool RunOnCache => true;

	public override void Process(uint seed)
	{
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_020e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0213: Unknown result type (might be due to invalid IL or missing references)
		//IL_0368: Unknown result type (might be due to invalid IL or missing references)
		//IL_036d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0372: Unknown result type (might be due to invalid IL or missing references)
		//IL_0376: Unknown result type (might be due to invalid IL or missing references)
		//IL_037d: Unknown result type (might be due to invalid IL or missing references)
		//IL_09df: Unknown result type (might be due to invalid IL or missing references)
		//IL_09e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_09e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_09f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_09f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_039b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_022e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0404: Unknown result type (might be due to invalid IL or missing references)
		//IL_040d: Unknown result type (might be due to invalid IL or missing references)
		//IL_041b: Unknown result type (might be due to invalid IL or missing references)
		//IL_11cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_11d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_11d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_11d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_042f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0436: Unknown result type (might be due to invalid IL or missing references)
		//IL_11da: Unknown result type (might be due to invalid IL or missing references)
		//IL_11dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0446: Unknown result type (might be due to invalid IL or missing references)
		//IL_045a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0461: Unknown result type (might be due to invalid IL or missing references)
		//IL_1279: Unknown result type (might be due to invalid IL or missing references)
		//IL_127b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1206: Unknown result type (might be due to invalid IL or missing references)
		//IL_1211: Unknown result type (might be due to invalid IL or missing references)
		//IL_1213: Unknown result type (might be due to invalid IL or missing references)
		//IL_1218: Unknown result type (might be due to invalid IL or missing references)
		//IL_122f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1231: Unknown result type (might be due to invalid IL or missing references)
		//IL_1233: Unknown result type (might be due to invalid IL or missing references)
		//IL_1238: Unknown result type (might be due to invalid IL or missing references)
		//IL_123d: Unknown result type (might be due to invalid IL or missing references)
		//IL_12a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_12a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_12ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_12b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_12f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_1302: Unknown result type (might be due to invalid IL or missing references)
		//IL_1318: Unknown result type (might be due to invalid IL or missing references)
		//IL_1329: Unknown result type (might be due to invalid IL or missing references)
		//IL_132e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1333: Unknown result type (might be due to invalid IL or missing references)
		//IL_1338: Unknown result type (might be due to invalid IL or missing references)
		//IL_133d: Unknown result type (might be due to invalid IL or missing references)
		//IL_134b: Unknown result type (might be due to invalid IL or missing references)
		//IL_135c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1372: Unknown result type (might be due to invalid IL or missing references)
		//IL_1383: Unknown result type (might be due to invalid IL or missing references)
		//IL_1388: Unknown result type (might be due to invalid IL or missing references)
		//IL_138d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1392: Unknown result type (might be due to invalid IL or missing references)
		//IL_1397: Unknown result type (might be due to invalid IL or missing references)
		//IL_1399: Unknown result type (might be due to invalid IL or missing references)
		//IL_139e: Unknown result type (might be due to invalid IL or missing references)
		//IL_13b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_13bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_13d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_13d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_13ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_13ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_04dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_2081: Unknown result type (might be due to invalid IL or missing references)
		//IL_2088: Unknown result type (might be due to invalid IL or missing references)
		//IL_208f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0509: Unknown result type (might be due to invalid IL or missing references)
		//IL_141d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1422: Unknown result type (might be due to invalid IL or missing references)
		//IL_142b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1430: Unknown result type (might be due to invalid IL or missing references)
		//IL_1432: Unknown result type (might be due to invalid IL or missing references)
		//IL_1437: Unknown result type (might be due to invalid IL or missing references)
		//IL_1449: Unknown result type (might be due to invalid IL or missing references)
		//IL_1450: Unknown result type (might be due to invalid IL or missing references)
		//IL_1457: Unknown result type (might be due to invalid IL or missing references)
		//IL_1463: Unknown result type (might be due to invalid IL or missing references)
		//IL_1468: Unknown result type (might be due to invalid IL or missing references)
		//IL_146d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1472: Unknown result type (might be due to invalid IL or missing references)
		//IL_1477: Unknown result type (might be due to invalid IL or missing references)
		//IL_147b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1487: Unknown result type (might be due to invalid IL or missing references)
		//IL_148c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1491: Unknown result type (might be due to invalid IL or missing references)
		//IL_1983: Unknown result type (might be due to invalid IL or missing references)
		//IL_1985: Unknown result type (might be due to invalid IL or missing references)
		//IL_20de: Unknown result type (might be due to invalid IL or missing references)
		//IL_20e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_20ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_2413: Unknown result type (might be due to invalid IL or missing references)
		//IL_241e: Unknown result type (might be due to invalid IL or missing references)
		//IL_2420: Unknown result type (might be due to invalid IL or missing references)
		//IL_2425: Unknown result type (might be due to invalid IL or missing references)
		//IL_242e: Unknown result type (might be due to invalid IL or missing references)
		//IL_2430: Unknown result type (might be due to invalid IL or missing references)
		//IL_2432: Unknown result type (might be due to invalid IL or missing references)
		//IL_2437: Unknown result type (might be due to invalid IL or missing references)
		//IL_243c: Unknown result type (might be due to invalid IL or missing references)
		//IL_067e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0685: Unknown result type (might be due to invalid IL or missing references)
		//IL_0695: Unknown result type (might be due to invalid IL or missing references)
		//IL_069c: Unknown result type (might be due to invalid IL or missing references)
		//IL_199a: Unknown result type (might be due to invalid IL or missing references)
		//IL_199c: Unknown result type (might be due to invalid IL or missing references)
		//IL_19a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_19a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_19ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_19b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_19e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_19e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_19e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_19e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_2186: Unknown result type (might be due to invalid IL or missing references)
		//IL_2191: Unknown result type (might be due to invalid IL or missing references)
		//IL_2193: Unknown result type (might be due to invalid IL or missing references)
		//IL_2198: Unknown result type (might be due to invalid IL or missing references)
		//IL_219a: Unknown result type (might be due to invalid IL or missing references)
		//IL_219f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0704: Unknown result type (might be due to invalid IL or missing references)
		//IL_070d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1514: Unknown result type (might be due to invalid IL or missing references)
		//IL_19f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_072f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0738: Unknown result type (might be due to invalid IL or missing references)
		//IL_14f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_1525: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a15: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a1a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a23: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a28: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a2a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a2f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a41: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a48: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a4f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a5b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a60: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a65: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a6a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a6f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a73: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a7f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a84: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a89: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a01: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_05be: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_05dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_075a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0761: Unknown result type (might be due to invalid IL or missing references)
		//IL_1501: Unknown result type (might be due to invalid IL or missing references)
		//IL_060d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0614: Unknown result type (might be due to invalid IL or missing references)
		//IL_061b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0620: Unknown result type (might be due to invalid IL or missing references)
		//IL_062c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0631: Unknown result type (might be due to invalid IL or missing references)
		//IL_0636: Unknown result type (might be due to invalid IL or missing references)
		//IL_063f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0644: Unknown result type (might be due to invalid IL or missing references)
		//IL_0646: Unknown result type (might be due to invalid IL or missing references)
		//IL_07c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_07cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_07d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_07de: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_07fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0804: Unknown result type (might be due to invalid IL or missing references)
		//IL_080b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0810: Unknown result type (might be due to invalid IL or missing references)
		//IL_0815: Unknown result type (might be due to invalid IL or missing references)
		//IL_0817: Unknown result type (might be due to invalid IL or missing references)
		//IL_081c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0847: Unknown result type (might be due to invalid IL or missing references)
		//IL_0849: Unknown result type (might be due to invalid IL or missing references)
		//IL_085a: Unknown result type (might be due to invalid IL or missing references)
		//IL_085c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0861: Unknown result type (might be due to invalid IL or missing references)
		//IL_0872: Unknown result type (might be due to invalid IL or missing references)
		//IL_0877: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_08cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_08dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_08de: Unknown result type (might be due to invalid IL or missing references)
		//IL_08e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_08f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_08f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0785: Unknown result type (might be due to invalid IL or missing references)
		//IL_078c: Unknown result type (might be due to invalid IL or missing references)
		//IL_154f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1558: Unknown result type (might be due to invalid IL or missing references)
		//IL_155d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1562: Unknown result type (might be due to invalid IL or missing references)
		//IL_1567: Unknown result type (might be due to invalid IL or missing references)
		//IL_1569: Unknown result type (might be due to invalid IL or missing references)
		//IL_1572: Unknown result type (might be due to invalid IL or missing references)
		//IL_1577: Unknown result type (might be due to invalid IL or missing references)
		//IL_157c: Unknown result type (might be due to invalid IL or missing references)
		//IL_158e: Unknown result type (might be due to invalid IL or missing references)
		//IL_159a: Unknown result type (might be due to invalid IL or missing references)
		//IL_159f: Unknown result type (might be due to invalid IL or missing references)
		//IL_15a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f7b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f7d: Unknown result type (might be due to invalid IL or missing references)
		//IL_15ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_15ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_15f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_15fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_1601: Unknown result type (might be due to invalid IL or missing references)
		//IL_1606: Unknown result type (might be due to invalid IL or missing references)
		//IL_1608: Unknown result type (might be due to invalid IL or missing references)
		//IL_1611: Unknown result type (might be due to invalid IL or missing references)
		//IL_161d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1622: Unknown result type (might be due to invalid IL or missing references)
		//IL_1627: Unknown result type (might be due to invalid IL or missing references)
		//IL_162c: Unknown result type (might be due to invalid IL or missing references)
		//IL_162e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1630: Unknown result type (might be due to invalid IL or missing references)
		//IL_1632: Unknown result type (might be due to invalid IL or missing references)
		//IL_1637: Unknown result type (might be due to invalid IL or missing references)
		//IL_1639: Unknown result type (might be due to invalid IL or missing references)
		//IL_163b: Unknown result type (might be due to invalid IL or missing references)
		//IL_163d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1642: Unknown result type (might be due to invalid IL or missing references)
		//IL_1656: Unknown result type (might be due to invalid IL or missing references)
		//IL_1658: Unknown result type (might be due to invalid IL or missing references)
		//IL_165a: Unknown result type (might be due to invalid IL or missing references)
		//IL_165f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1661: Unknown result type (might be due to invalid IL or missing references)
		//IL_1663: Unknown result type (might be due to invalid IL or missing references)
		//IL_1665: Unknown result type (might be due to invalid IL or missing references)
		//IL_166a: Unknown result type (might be due to invalid IL or missing references)
		//IL_167e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1680: Unknown result type (might be due to invalid IL or missing references)
		//IL_15b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_15b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f92: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f94: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f9b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f9d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1fa4: Unknown result type (might be due to invalid IL or missing references)
		//IL_1fa9: Unknown result type (might be due to invalid IL or missing references)
		//IL_1fd8: Unknown result type (might be due to invalid IL or missing references)
		//IL_1fda: Unknown result type (might be due to invalid IL or missing references)
		//IL_1fdc: Unknown result type (might be due to invalid IL or missing references)
		//IL_1fe1: Unknown result type (might be due to invalid IL or missing references)
		//IL_15c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_15c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_15ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_15cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_15d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_15d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_15d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_15da: Unknown result type (might be due to invalid IL or missing references)
		//IL_15df: Unknown result type (might be due to invalid IL or missing references)
		//IL_15e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_15e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_15e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_15ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b0c: Unknown result type (might be due to invalid IL or missing references)
		//IL_222a: Unknown result type (might be due to invalid IL or missing references)
		//IL_222c: Unknown result type (might be due to invalid IL or missing references)
		//IL_2231: Unknown result type (might be due to invalid IL or missing references)
		//IL_16c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_16d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ae8: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b1d: Unknown result type (might be due to invalid IL or missing references)
		//IL_2245: Unknown result type (might be due to invalid IL or missing references)
		//IL_2247: Unknown result type (might be due to invalid IL or missing references)
		//IL_224c: Unknown result type (might be due to invalid IL or missing references)
		//IL_16ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_1af9: Unknown result type (might be due to invalid IL or missing references)
		//IL_225e: Unknown result type (might be due to invalid IL or missing references)
		//IL_2260: Unknown result type (might be due to invalid IL or missing references)
		//IL_2265: Unknown result type (might be due to invalid IL or missing references)
		//IL_1716: Unknown result type (might be due to invalid IL or missing references)
		//IL_1722: Unknown result type (might be due to invalid IL or missing references)
		//IL_16fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_1704: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b47: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b50: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b55: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b5a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b5f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b61: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b6a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b6f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b74: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b86: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b92: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b97: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b9c: Unknown result type (might be due to invalid IL or missing references)
		//IL_2572: Unknown result type (might be due to invalid IL or missing references)
		//IL_257d: Unknown result type (might be due to invalid IL or missing references)
		//IL_257f: Unknown result type (might be due to invalid IL or missing references)
		//IL_2599: Unknown result type (might be due to invalid IL or missing references)
		//IL_259e: Unknown result type (might be due to invalid IL or missing references)
		//IL_25a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_25ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_25ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_25b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_25b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_25ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_2283: Unknown result type (might be due to invalid IL or missing references)
		//IL_2285: Unknown result type (might be due to invalid IL or missing references)
		//IL_1739: Unknown result type (might be due to invalid IL or missing references)
		//IL_1be4: Unknown result type (might be due to invalid IL or missing references)
		//IL_1be6: Unknown result type (might be due to invalid IL or missing references)
		//IL_1bef: Unknown result type (might be due to invalid IL or missing references)
		//IL_1bf4: Unknown result type (might be due to invalid IL or missing references)
		//IL_1bf9: Unknown result type (might be due to invalid IL or missing references)
		//IL_1bfe: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c00: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c09: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c15: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c1a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c1f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c24: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c26: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c28: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c2a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c2f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c31: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c33: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c35: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c3a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c4e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c50: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c52: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c57: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c59: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c5b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c5d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c62: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c76: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c78: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ba8: Unknown result type (might be due to invalid IL or missing references)
		//IL_1baa: Unknown result type (might be due to invalid IL or missing references)
		//IL_2692: Unknown result type (might be due to invalid IL or missing references)
		//IL_269d: Unknown result type (might be due to invalid IL or missing references)
		//IL_269f: Unknown result type (might be due to invalid IL or missing references)
		//IL_26b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_26be: Unknown result type (might be due to invalid IL or missing references)
		//IL_26c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_26cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_26ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_26d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_26d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_26da: Unknown result type (might be due to invalid IL or missing references)
		//IL_22a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_2290: Unknown result type (might be due to invalid IL or missing references)
		//IL_2299: Unknown result type (might be due to invalid IL or missing references)
		//IL_229e: Unknown result type (might be due to invalid IL or missing references)
		//IL_22a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_1765: Unknown result type (might be due to invalid IL or missing references)
		//IL_1771: Unknown result type (might be due to invalid IL or missing references)
		//IL_174c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1753: Unknown result type (might be due to invalid IL or missing references)
		//IL_1bbe: Unknown result type (might be due to invalid IL or missing references)
		//IL_1bc0: Unknown result type (might be due to invalid IL or missing references)
		//IL_1bc2: Unknown result type (might be due to invalid IL or missing references)
		//IL_1bc7: Unknown result type (might be due to invalid IL or missing references)
		//IL_1bcc: Unknown result type (might be due to invalid IL or missing references)
		//IL_1bce: Unknown result type (might be due to invalid IL or missing references)
		//IL_1bd0: Unknown result type (might be due to invalid IL or missing references)
		//IL_1bd2: Unknown result type (might be due to invalid IL or missing references)
		//IL_1bd7: Unknown result type (might be due to invalid IL or missing references)
		//IL_1bd9: Unknown result type (might be due to invalid IL or missing references)
		//IL_1bdb: Unknown result type (might be due to invalid IL or missing references)
		//IL_1bdd: Unknown result type (might be due to invalid IL or missing references)
		//IL_1be2: Unknown result type (might be due to invalid IL or missing references)
		//IL_22b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_1788: Unknown result type (might be due to invalid IL or missing references)
		//IL_1cbf: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ccb: Unknown result type (might be due to invalid IL or missing references)
		//IL_22d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_22d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_22d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_22db: Unknown result type (might be due to invalid IL or missing references)
		//IL_179b: Unknown result type (might be due to invalid IL or missing references)
		//IL_17a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ce2: Unknown result type (might be due to invalid IL or missing references)
		//IL_1853: Unknown result type (might be due to invalid IL or missing references)
		//IL_17bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d0e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d1a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1cf5: Unknown result type (might be due to invalid IL or missing references)
		//IL_1cfc: Unknown result type (might be due to invalid IL or missing references)
		//IL_1861: Unknown result type (might be due to invalid IL or missing references)
		//IL_17cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d31: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d5d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d69: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d44: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d4b: Unknown result type (might be due to invalid IL or missing references)
		//IL_18a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_1876: Unknown result type (might be due to invalid IL or missing references)
		//IL_17e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d80: Unknown result type (might be due to invalid IL or missing references)
		//IL_1962: Unknown result type (might be due to invalid IL or missing references)
		//IL_1964: Unknown result type (might be due to invalid IL or missing references)
		//IL_196a: Unknown result type (might be due to invalid IL or missing references)
		//IL_196c: Unknown result type (might be due to invalid IL or missing references)
		//IL_196e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1970: Unknown result type (might be due to invalid IL or missing references)
		//IL_18f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_18c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_1819: Unknown result type (might be due to invalid IL or missing references)
		//IL_17f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d93: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d9a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1918: Unknown result type (might be due to invalid IL or missing references)
		//IL_182c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e4b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1db3: Unknown result type (might be due to invalid IL or missing references)
		//IL_194e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1950: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e59: Unknown result type (might be due to invalid IL or missing references)
		//IL_1dc4: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e9d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e6e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ddb: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f5a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f5c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f62: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f64: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f66: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f68: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ef0: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ec0: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e11: Unknown result type (might be due to invalid IL or missing references)
		//IL_1dee: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f10: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e24: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f46: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f48: Unknown result type (might be due to invalid IL or missing references)
		if (World.Cached)
		{
			return;
		}
		List<DungeonGridInfo> entrances;
		WorldSpaceGrid<Prefab<DungeonGridCell>> grid;
		if (World.Networked)
		{
			World.Spawn("Dungeon");
		}
		else
		{
			if (ConnectionType == InfrastructureType.Tunnel && !World.Config.BelowGroundRails)
			{
				return;
			}
			Prefab<DungeonGridCell>[] array = Prefab.Load<DungeonGridCell>("assets/bundled/prefabs/autospawn/" + TunnelFolder, (GameManager)null, (PrefabAttribute.Library)null, true, false);
			if (array == null || array.Length == 0)
			{
				return;
			}
			Prefab<DungeonGridCell>[] array2 = Prefab.Load<DungeonGridCell>("assets/bundled/prefabs/autospawn/" + StationFolder, (GameManager)null, (PrefabAttribute.Library)null, true, false);
			if (array2 == null || array2.Length == 0)
			{
				return;
			}
			Prefab<DungeonGridCell>[] array3 = Prefab.Load<DungeonGridCell>("assets/bundled/prefabs/autospawn/" + UpwardsFolder, (GameManager)null, (PrefabAttribute.Library)null, true, false);
			if (array3 == null)
			{
				return;
			}
			Prefab<DungeonGridCell>[] array4 = Prefab.Load<DungeonGridCell>("assets/bundled/prefabs/autospawn/" + TransitionFolder, (GameManager)null, (PrefabAttribute.Library)null, true, false);
			if (array4 == null)
			{
				return;
			}
			Prefab<DungeonGridLink>[] array5 = Prefab.Load<DungeonGridLink>("assets/bundled/prefabs/autospawn/" + LinkFolder, (GameManager)null, (PrefabAttribute.Library)null, true, false);
			if (array5 == null)
			{
				return;
			}
			array5 = array5.OrderByDescending((Prefab<DungeonGridLink> x) => x.Component.Priority).ToArray();
			entrances = (Object.op_Implicit((Object)(object)TerrainMeta.Path) ? TerrainMeta.Path.DungeonGridEntrances : null);
			grid = new WorldSpaceGrid<Prefab<DungeonGridCell>>(TerrainMeta.Size.x, (float)CellSize, (RoundingMode)1);
			int[,] array6 = new int[grid.CellCount, grid.CellCount];
			DungeonGridConnectionHash[,] hashmap = new DungeonGridConnectionHash[grid.CellCount, grid.CellCount];
			PathFinder pathFinder = new PathFinder(array6, diagonals: false);
			int cellCount = grid.CellCount;
			int num = 0;
			int num2 = grid.CellCount - 1;
			for (int num3 = 0; num3 < cellCount; num3++)
			{
				for (int num4 = 0; num4 < cellCount; num4++)
				{
					array6[num4, num3] = 1;
				}
			}
			if (Object.op_Implicit((Object)(object)TerrainMeta.Path))
			{
				foreach (DungeonGridBlocker dungeonGridBlocker in TerrainMeta.Path.DungeonGridBlockers)
				{
					OBB bounds = dungeonGridBlocker.GetBounds();
					for (int num5 = 0; num5 < cellCount; num5++)
					{
						for (int num6 = 0; num6 < cellCount; num6++)
						{
							OBB val = new OBB(grid.GridToWorldCoords(new Vector2i(num6, num5)), new Vector3(grid.CellSize, 1f, grid.CellSize), Quaternion.identity);
							if (bounds.Intersects2D(val) && !IsNearEntrance(num6, num5))
							{
								array6[num6, num5] = int.MaxValue;
							}
						}
					}
				}
			}
			List<PathSegment> list = new List<PathSegment>();
			List<PathLink> list2 = new List<PathLink>();
			List<PathNode> list3 = new List<PathNode>();
			List<PathNode> unconnectedNodeList = new List<PathNode>();
			List<PathNode> secondaryNodeList = new List<PathNode>();
			List<PathFinder.Point> list4 = new List<PathFinder.Point>();
			List<PathFinder.Point> list5 = new List<PathFinder.Point>();
			List<PathFinder.Point> list6 = new List<PathFinder.Point>();
			foreach (DungeonGridInfo item2 in entrances)
			{
				DungeonGridInfo entrance = item2;
				TerrainPathConnect[] componentsInChildren = ((Component)entrance).GetComponentsInChildren<TerrainPathConnect>(true);
				foreach (TerrainPathConnect terrainPathConnect in componentsInChildren)
				{
					if (terrainPathConnect.Type != ConnectionType)
					{
						continue;
					}
					Vector2i val2 = grid.WorldToGridCoords(((Component)terrainPathConnect).transform.position);
					if (array6[val2.x, val2.y] == int.MaxValue)
					{
						continue;
					}
					PathFinder.Node stationNode = pathFinder.FindClosestWalkable(new PathFinder.Point(val2.x, val2.y), 1);
					if (stationNode == null)
					{
						continue;
					}
					Prefab<DungeonGridCell> prefab = ((val2.x > num) ? grid[val2.x - 1, val2.y] : null);
					Prefab<DungeonGridCell> prefab2 = ((val2.x < num2) ? grid[val2.x + 1, val2.y] : null);
					Prefab<DungeonGridCell> prefab3 = ((val2.y > num) ? grid[val2.x, val2.y - 1] : null);
					Prefab<DungeonGridCell> obj = ((val2.y < num2) ? grid[val2.x, val2.y + 1] : null);
					DungeonGridConnectionType dungeonGridConnectionType = prefab?.Component.East ?? DungeonGridConnectionType.None;
					DungeonGridConnectionType dungeonGridConnectionType2 = prefab2?.Component.West ?? DungeonGridConnectionType.None;
					DungeonGridConnectionType dungeonGridConnectionType3 = prefab3?.Component.North ?? DungeonGridConnectionType.None;
					DungeonGridConnectionType dungeonGridConnectionType4 = obj?.Component.South ?? DungeonGridConnectionType.None;
					bool flag = prefab != null || val2.x <= num;
					bool flag2 = prefab2 != null || val2.x >= num2;
					bool flag3 = prefab3 != null || val2.y <= num;
					bool flag4 = obj != null || val2.y >= num2;
					Prefab<DungeonGridCell> prefab4 = null;
					float num8 = float.MaxValue;
					ArrayEx.Shuffle(array2, ref seed);
					Prefab<DungeonGridCell>[] array7 = array2;
					foreach (Prefab<DungeonGridCell> prefab5 in array7)
					{
						if ((flag && prefab5.Component.West != dungeonGridConnectionType) || (flag2 && prefab5.Component.East != dungeonGridConnectionType2) || (flag3 && prefab5.Component.South != dungeonGridConnectionType3) || (flag4 && prefab5.Component.North != dungeonGridConnectionType4))
						{
							continue;
						}
						DungeonVolume componentInChildren = prefab5.Object.GetComponentInChildren<DungeonVolume>();
						DungeonVolume componentInChildren2 = ((Component)entrance).GetComponentInChildren<DungeonVolume>();
						OBB bounds2 = componentInChildren.GetBounds(grid.GridToWorldCoords(val2), Quaternion.identity);
						OBB bounds3 = componentInChildren2.GetBounds(((Component)entrance).transform.position, Quaternion.identity);
						if (!bounds2.Intersects2D(bounds3))
						{
							DungeonGridLink componentInChildren3 = prefab5.Object.GetComponentInChildren<DungeonGridLink>();
							Vector3 val3 = grid.GridToWorldCoords(new Vector2i(val2.x, val2.y)) + componentInChildren3.UpSocket.localPosition;
							float num10 = Vector3Ex.Magnitude2D(((Component)terrainPathConnect).transform.position - val3);
							if (!(num8 < num10))
							{
								prefab4 = prefab5;
								num8 = num10;
							}
						}
					}
					bool isStartPoint;
					if (prefab4 != null)
					{
						grid[val2.x, val2.y] = prefab4;
						array6[val2.x, val2.y] = int.MaxValue;
						isStartPoint = secondaryNodeList.Count == 0;
						secondaryNodeList.RemoveAll((PathNode x) => x.node.point == stationNode.point);
						unconnectedNodeList.RemoveAll((PathNode x) => x.node.point == stationNode.point);
						if (prefab4.Component.West != DungeonGridConnectionType.None)
						{
							AddNode(val2.x - 1, val2.y);
						}
						if (prefab4.Component.East != DungeonGridConnectionType.None)
						{
							AddNode(val2.x + 1, val2.y);
						}
						if (prefab4.Component.South != DungeonGridConnectionType.None)
						{
							AddNode(val2.x, val2.y - 1);
						}
						if (prefab4.Component.North != DungeonGridConnectionType.None)
						{
							AddNode(val2.x, val2.y + 1);
						}
						PathLink pathLink = new PathLink();
						DungeonGridLink componentInChildren4 = ((Component)entrance).gameObject.GetComponentInChildren<DungeonGridLink>();
						Vector3 position = ((Component)entrance).transform.position;
						Quaternion rotation = ((Component)entrance).transform.rotation;
						Vector3 eulerAngles = rotation.eulerAngles;
						DungeonGridLink componentInChildren5 = prefab4.Object.GetComponentInChildren<DungeonGridLink>();
						Vector3 position2 = grid.GridToWorldCoords(new Vector2i(val2.x, val2.y));
						Vector3 zero = Vector3.zero;
						pathLink.downwards = new PathLinkSide();
						pathLink.downwards.origin = new PathLinkSegment();
						pathLink.downwards.origin.position = position;
						pathLink.downwards.origin.rotation = Quaternion.Euler(eulerAngles);
						pathLink.downwards.origin.scale = Vector3.one;
						pathLink.downwards.origin.link = componentInChildren4;
						pathLink.downwards.segments = new List<PathLinkSegment>();
						pathLink.upwards = new PathLinkSide();
						pathLink.upwards.origin = new PathLinkSegment();
						pathLink.upwards.origin.position = position2;
						pathLink.upwards.origin.rotation = Quaternion.Euler(zero);
						pathLink.upwards.origin.scale = Vector3.one;
						pathLink.upwards.origin.link = componentInChildren5;
						pathLink.upwards.segments = new List<PathLinkSegment>();
						list2.Add(pathLink);
					}
					void AddNode(int x, int y)
					{
						//IL_0059: Unknown result type (might be due to invalid IL or missing references)
						PathFinder.Node node8 = pathFinder.FindClosestWalkable(new PathFinder.Point(x, y), 1);
						if (node8 != null)
						{
							PathNode item = new PathNode
							{
								monument = (Object.op_Implicit((Object)(object)TerrainMeta.Path) ? TerrainMeta.Path.FindClosest(TerrainMeta.Path.Monuments, ((Component)entrance).transform.position) : ((Component)((Component)entrance).transform).GetComponentInParent<MonumentInfo>()),
								node = node8
							};
							if (isStartPoint)
							{
								secondaryNodeList.Add(item);
							}
							else
							{
								unconnectedNodeList.Add(item);
							}
							DungeonGridConnectionHash dungeonGridConnectionHash4 = hashmap[node8.point.x, node8.point.y];
							DungeonGridConnectionHash dungeonGridConnectionHash5 = hashmap[stationNode.point.x, stationNode.point.y];
							if (node8.point.x > stationNode.point.x)
							{
								dungeonGridConnectionHash4.West = true;
								dungeonGridConnectionHash5.East = true;
							}
							if (node8.point.x < stationNode.point.x)
							{
								dungeonGridConnectionHash4.East = true;
								dungeonGridConnectionHash5.West = true;
							}
							if (node8.point.y > stationNode.point.y)
							{
								dungeonGridConnectionHash4.South = true;
								dungeonGridConnectionHash5.North = true;
							}
							if (node8.point.y < stationNode.point.y)
							{
								dungeonGridConnectionHash4.North = true;
								dungeonGridConnectionHash5.South = true;
							}
							hashmap[node8.point.x, node8.point.y] = dungeonGridConnectionHash4;
							hashmap[stationNode.point.x, stationNode.point.y] = dungeonGridConnectionHash5;
						}
					}
				}
			}
			while (unconnectedNodeList.Count != 0 || secondaryNodeList.Count != 0)
			{
				if (unconnectedNodeList.Count == 0)
				{
					PathNode node = secondaryNodeList[0];
					unconnectedNodeList.AddRange(secondaryNodeList.Where((PathNode x) => (Object)(object)x.monument == (Object)(object)node.monument));
					secondaryNodeList.RemoveAll((PathNode x) => (Object)(object)x.monument == (Object)(object)node.monument);
					Vector2i val4 = grid.WorldToGridCoords(((Component)node.monument).transform.position);
					pathFinder.PushPoint = new PathFinder.Point(val4.x, val4.y);
					pathFinder.PushRadius = (pathFinder.PushDistance = 2);
					pathFinder.PushMultiplier = 16;
				}
				list6.Clear();
				list6.AddRange(unconnectedNodeList.Select((PathNode x) => x.node.point));
				list5.Clear();
				list5.AddRange(list3.Select((PathNode x) => x.node.point));
				list5.AddRange(secondaryNodeList.Select((PathNode x) => x.node.point));
				list5.AddRange(list4);
				PathFinder.Node node2 = pathFinder.FindPathUndirected(list5, list6, 100000);
				if (node2 == null)
				{
					PathNode node3 = unconnectedNodeList[0];
					secondaryNodeList.AddRange(unconnectedNodeList.Where((PathNode x) => (Object)(object)x.monument == (Object)(object)node3.monument));
					unconnectedNodeList.RemoveAll((PathNode x) => (Object)(object)x.monument == (Object)(object)node3.monument);
					secondaryNodeList.Remove(node3);
					list3.Add(node3);
					continue;
				}
				PathSegment segment = new PathSegment();
				for (PathFinder.Node node4 = node2; node4 != null; node4 = node4.next)
				{
					if (node4 == node2)
					{
						segment.start = node4;
					}
					if (node4.next == null)
					{
						segment.end = node4;
					}
				}
				list.Add(segment);
				PathNode node5 = unconnectedNodeList.Find((PathNode x) => x.node.point == segment.start.point || x.node.point == segment.end.point);
				secondaryNodeList.AddRange(unconnectedNodeList.Where((PathNode x) => (Object)(object)x.monument == (Object)(object)node5.monument));
				unconnectedNodeList.RemoveAll((PathNode x) => (Object)(object)x.monument == (Object)(object)node5.monument);
				secondaryNodeList.Remove(node5);
				list3.Add(node5);
				PathNode pathNode = secondaryNodeList.Find((PathNode x) => x.node.point == segment.start.point || x.node.point == segment.end.point);
				if (pathNode != null)
				{
					secondaryNodeList.Remove(pathNode);
					list3.Add(pathNode);
				}
				for (PathFinder.Node node6 = node2; node6 != null; node6 = node6.next)
				{
					if (node6 != node2 && node6.next != null)
					{
						list4.Add(node6.point);
					}
				}
			}
			foreach (PathSegment item3 in list)
			{
				PathFinder.Node node7 = item3.start;
				while (node7 != null && node7.next != null)
				{
					DungeonGridConnectionHash dungeonGridConnectionHash = hashmap[node7.point.x, node7.point.y];
					DungeonGridConnectionHash dungeonGridConnectionHash2 = hashmap[node7.next.point.x, node7.next.point.y];
					if (node7.point.x > node7.next.point.x)
					{
						dungeonGridConnectionHash.West = true;
						dungeonGridConnectionHash2.East = true;
					}
					if (node7.point.x < node7.next.point.x)
					{
						dungeonGridConnectionHash.East = true;
						dungeonGridConnectionHash2.West = true;
					}
					if (node7.point.y > node7.next.point.y)
					{
						dungeonGridConnectionHash.South = true;
						dungeonGridConnectionHash2.North = true;
					}
					if (node7.point.y < node7.next.point.y)
					{
						dungeonGridConnectionHash.North = true;
						dungeonGridConnectionHash2.South = true;
					}
					hashmap[node7.point.x, node7.point.y] = dungeonGridConnectionHash;
					hashmap[node7.next.point.x, node7.next.point.y] = dungeonGridConnectionHash2;
					node7 = node7.next;
				}
			}
			for (int num11 = 0; num11 < grid.CellCount; num11++)
			{
				for (int num12 = 0; num12 < grid.CellCount; num12++)
				{
					if (array6[num11, num12] == int.MaxValue)
					{
						continue;
					}
					DungeonGridConnectionHash dungeonGridConnectionHash3 = hashmap[num11, num12];
					if (dungeonGridConnectionHash3.Value == 0)
					{
						continue;
					}
					ArrayEx.Shuffle(array, ref seed);
					Prefab<DungeonGridCell>[] array7 = array;
					foreach (Prefab<DungeonGridCell> prefab6 in array7)
					{
						Prefab<DungeonGridCell> prefab7 = ((num11 > num) ? grid[num11 - 1, num12] : null);
						if (!((prefab7 != null) ? (prefab6.Component.West == prefab7.Component.East) : (dungeonGridConnectionHash3.West ? (prefab6.Component.West != DungeonGridConnectionType.None) : (prefab6.Component.West == DungeonGridConnectionType.None))))
						{
							continue;
						}
						Prefab<DungeonGridCell> prefab8 = ((num11 < num2) ? grid[num11 + 1, num12] : null);
						if (!((prefab8 != null) ? (prefab6.Component.East == prefab8.Component.West) : (dungeonGridConnectionHash3.East ? (prefab6.Component.East != DungeonGridConnectionType.None) : (prefab6.Component.East == DungeonGridConnectionType.None))))
						{
							continue;
						}
						Prefab<DungeonGridCell> prefab9 = ((num12 > num) ? grid[num11, num12 - 1] : null);
						if (!((prefab9 != null) ? (prefab6.Component.South == prefab9.Component.North) : (dungeonGridConnectionHash3.South ? (prefab6.Component.South != DungeonGridConnectionType.None) : (prefab6.Component.South == DungeonGridConnectionType.None))))
						{
							continue;
						}
						Prefab<DungeonGridCell> prefab10 = ((num12 < num2) ? grid[num11, num12 + 1] : null);
						bool flag5;
						if (prefab10 != null)
						{
							flag5 = prefab6.Component.North == prefab10.Component.South;
						}
						else
						{
							flag5 = (dungeonGridConnectionHash3.North ? (prefab6.Component.North != DungeonGridConnectionType.None) : (prefab6.Component.North == DungeonGridConnectionType.None));
						}
						if (flag5 && (prefab6.Component.West == DungeonGridConnectionType.None || prefab7 == null || !prefab6.Component.ShouldAvoid(prefab7.ID)) && (prefab6.Component.East == DungeonGridConnectionType.None || prefab8 == null || !prefab6.Component.ShouldAvoid(prefab8.ID)) && (prefab6.Component.South == DungeonGridConnectionType.None || prefab9 == null || !prefab6.Component.ShouldAvoid(prefab9.ID)) && (prefab6.Component.North == DungeonGridConnectionType.None || prefab10 == null || !prefab6.Component.ShouldAvoid(prefab10.ID)))
						{
							grid[num11, num12] = prefab6;
							bool num13 = prefab7 == null || prefab6.Component.WestVariant == prefab7.Component.EastVariant;
							bool flag6 = prefab9 == null || prefab6.Component.SouthVariant == prefab9.Component.NorthVariant;
							if (num13 & flag6)
							{
								break;
							}
						}
					}
				}
			}
			Vector3 zero2 = Vector3.zero;
			Vector3 zero3 = Vector3.zero;
			do
			{
				zero3 = zero2;
				for (int num14 = 0; num14 < grid.CellCount; num14++)
				{
					for (int num15 = 0; num15 < grid.CellCount; num15++)
					{
						Prefab<DungeonGridCell> prefab11 = grid[num14, num15];
						if (prefab11 != null)
						{
							Vector2i val5 = new Vector2i(num14, num15);
							Vector3 val6 = grid.GridToWorldCoords(val5);
							while (!IsValidPrefabPlacement(prefab11, zero2 + val6, Quaternion.identity, Vector3.one, EnvironmentType.Underground | EnvironmentType.Building))
							{
								zero2.y -= 3f;
							}
						}
					}
				}
			}
			while (zero2 != zero3);
			foreach (PathLink item4 in list2)
			{
				PathLinkSegment origin = item4.upwards.origin;
				origin.position += zero2;
			}
			foreach (PathLink item5 in list2)
			{
				Vector3 val7 = item5.upwards.origin.position + item5.upwards.origin.rotation * Vector3.Scale(item5.upwards.origin.upSocket.localPosition, item5.upwards.origin.scale);
				Vector3 val8 = item5.downwards.origin.position + item5.downwards.origin.rotation * Vector3.Scale(item5.downwards.origin.downSocket.localPosition, item5.downwards.origin.scale) - val7;
				Vector3[] array8 = new Vector3[2]
				{
					new Vector3(0f, 1f, 0f),
					new Vector3(1f, 1f, 1f)
				};
				foreach (Vector3 val9 in array8)
				{
					int num16 = 0;
					int num17 = 0;
					while (val8.magnitude > 1f && (num16 < 8 || num17 < 8))
					{
						bool flag7 = num16 > 2 && num17 > 2;
						bool flag8 = num16 > 4 && num17 > 4;
						Prefab<DungeonGridLink> prefab12 = null;
						Vector3 val10 = Vector3.zero;
						int num18 = int.MinValue;
						Vector3 position3 = Vector3.zero;
						Quaternion rotation2 = Quaternion.identity;
						PathLinkSegment prevSegment = item5.downwards.prevSegment;
						Vector3 val11 = prevSegment.position + prevSegment.rotation * Vector3.Scale(prevSegment.scale, prevSegment.downSocket.localPosition);
						Quaternion val12 = prevSegment.rotation * prevSegment.downSocket.localRotation;
						Prefab<DungeonGridLink>[] array9 = array5;
						foreach (Prefab<DungeonGridLink> prefab13 in array9)
						{
							float num19 = SeedRandom.Value(ref seed);
							DungeonGridLink component = prefab13.Component;
							if (prevSegment.downType != component.UpType)
							{
								continue;
							}
							switch (component.DownType)
							{
							case DungeonGridLinkType.Elevator:
								if (flag7 || val9.x != 0f || val9.z != 0f)
								{
									continue;
								}
								break;
							case DungeonGridLinkType.Transition:
								if (val9.x != 0f || val9.z != 0f)
								{
									continue;
								}
								break;
							}
							int num20 = ((!flag7) ? component.Priority : 0);
							if (num18 > num20)
							{
								continue;
							}
							Quaternion val13 = val12 * Quaternion.Inverse(component.UpSocket.localRotation);
							Quaternion val14 = val13 * component.DownSocket.localRotation;
							PathLinkSegment prevSegment2 = item5.upwards.prevSegment;
							Quaternion val15 = prevSegment2.rotation * prevSegment2.upSocket.localRotation;
							if (component.Rotation > 0)
							{
								if (Quaternion.Angle(val15, val14) > (float)component.Rotation)
								{
									continue;
								}
								Quaternion val16 = val15 * Quaternion.Inverse(val14);
								val13 *= val16;
								val14 *= val16;
							}
							Vector3 val17 = val11 - val13 * component.UpSocket.localPosition;
							Vector3 val18 = val13 * (component.DownSocket.localPosition - component.UpSocket.localPosition);
							Vector3 val19 = val8 + val10;
							Vector3 val20 = val8 + val18;
							float magnitude = val19.magnitude;
							float magnitude2 = val20.magnitude;
							Vector3 val21 = Vector3.Scale(val19, val9);
							Vector3 val22 = Vector3.Scale(val20, val9);
							float magnitude3 = val21.magnitude;
							float magnitude4 = val22.magnitude;
							if (val10 != Vector3.zero)
							{
								if (magnitude3 < magnitude4 || (magnitude3 == magnitude4 && magnitude < magnitude2) || (magnitude3 == magnitude4 && magnitude == magnitude2 && num19 < 0.5f))
								{
									continue;
								}
							}
							else if (magnitude3 <= magnitude4)
							{
								continue;
							}
							if (Mathf.Abs(val22.x) - Mathf.Abs(val21.x) > 0.01f || (Mathf.Abs(val22.x) > 0.01f && val19.x * val20.x < 0f) || Mathf.Abs(val22.y) - Mathf.Abs(val21.y) > 0.01f || (Mathf.Abs(val22.y) > 0.01f && val19.y * val20.y < 0f) || Mathf.Abs(val22.z) - Mathf.Abs(val21.z) > 0.01f || (Mathf.Abs(val22.z) > 0.01f && val19.z * val20.z < 0f) || (flag7 && val9.x == 0f && val9.z == 0f && component.DownType == DungeonGridLinkType.Default && ((Mathf.Abs(val20.x) > 0.01f && Mathf.Abs(val20.x) < LinkRadius * 2f - 0.1f) || (Mathf.Abs(val20.z) > 0.01f && Mathf.Abs(val20.z) < LinkRadius * 2f - 0.1f))))
							{
								continue;
							}
							num18 = num20;
							if (val9.x == 0f && val9.z == 0f)
							{
								if (!flag7 && Mathf.Abs(val20.y) < LinkTransition - 0.1f)
								{
									continue;
								}
							}
							else if ((!flag7 && magnitude4 > 0.01f && (Mathf.Abs(val20.x) < LinkRadius * 2f - 0.1f || Mathf.Abs(val20.z) < LinkRadius * 2f - 0.1f)) || (!flag8 && magnitude4 > 0.01f && (Mathf.Abs(val20.x) < LinkRadius * 1f - 0.1f || Mathf.Abs(val20.z) < LinkRadius * 1f - 0.1f)))
							{
								continue;
							}
							if (!flag7 || !(magnitude4 < 0.01f) || !(magnitude2 < 0.01f) || !(Quaternion.Angle(val15, val14) > 10f))
							{
								prefab12 = prefab13;
								val10 = val18;
								num18 = num20;
								position3 = val17;
								rotation2 = val13;
							}
						}
						if (val10 != Vector3.zero)
						{
							PathLinkSegment pathLinkSegment = new PathLinkSegment();
							pathLinkSegment.position = position3;
							pathLinkSegment.rotation = rotation2;
							pathLinkSegment.scale = Vector3.one;
							pathLinkSegment.prefab = prefab12;
							pathLinkSegment.link = prefab12.Component;
							item5.downwards.segments.Add(pathLinkSegment);
							val8 += val10;
						}
						else
						{
							num17++;
						}
						if (val9.x > 0f || val9.z > 0f)
						{
							Prefab<DungeonGridLink> prefab14 = null;
							Vector3 val23 = Vector3.zero;
							int num21 = int.MinValue;
							Vector3 position4 = Vector3.zero;
							Quaternion rotation3 = Quaternion.identity;
							PathLinkSegment prevSegment3 = item5.upwards.prevSegment;
							Vector3 val24 = prevSegment3.position + prevSegment3.rotation * Vector3.Scale(prevSegment3.scale, prevSegment3.upSocket.localPosition);
							Quaternion val25 = prevSegment3.rotation * prevSegment3.upSocket.localRotation;
							array9 = array5;
							foreach (Prefab<DungeonGridLink> prefab15 in array9)
							{
								float num22 = SeedRandom.Value(ref seed);
								DungeonGridLink component2 = prefab15.Component;
								if (prevSegment3.upType != component2.DownType)
								{
									continue;
								}
								switch (component2.DownType)
								{
								case DungeonGridLinkType.Elevator:
									if (flag7 || val9.x != 0f || val9.z != 0f)
									{
										continue;
									}
									break;
								case DungeonGridLinkType.Transition:
									if (val9.x != 0f || val9.z != 0f)
									{
										continue;
									}
									break;
								}
								int num23 = ((!flag7) ? component2.Priority : 0);
								if (num21 > num23)
								{
									continue;
								}
								Quaternion val26 = val25 * Quaternion.Inverse(component2.DownSocket.localRotation);
								Quaternion val27 = val26 * component2.UpSocket.localRotation;
								PathLinkSegment prevSegment4 = item5.downwards.prevSegment;
								Quaternion val28 = prevSegment4.rotation * prevSegment4.downSocket.localRotation;
								if (component2.Rotation > 0)
								{
									if (Quaternion.Angle(val28, val27) > (float)component2.Rotation)
									{
										continue;
									}
									Quaternion val29 = val28 * Quaternion.Inverse(val27);
									val26 *= val29;
									val27 *= val29;
								}
								Vector3 val30 = val24 - val26 * component2.DownSocket.localPosition;
								Vector3 val31 = val26 * (component2.UpSocket.localPosition - component2.DownSocket.localPosition);
								Vector3 val32 = val8 - val23;
								Vector3 val33 = val8 - val31;
								float magnitude5 = val32.magnitude;
								float magnitude6 = val33.magnitude;
								Vector3 val34 = Vector3.Scale(val32, val9);
								Vector3 val35 = Vector3.Scale(val33, val9);
								float magnitude7 = val34.magnitude;
								float magnitude8 = val35.magnitude;
								if (val23 != Vector3.zero)
								{
									if (magnitude7 < magnitude8 || (magnitude7 == magnitude8 && magnitude5 < magnitude6) || (magnitude7 == magnitude8 && magnitude5 == magnitude6 && num22 < 0.5f))
									{
										continue;
									}
								}
								else if (magnitude7 <= magnitude8)
								{
									continue;
								}
								if (Mathf.Abs(val35.x) - Mathf.Abs(val34.x) > 0.01f || (Mathf.Abs(val35.x) > 0.01f && val32.x * val33.x < 0f) || Mathf.Abs(val35.y) - Mathf.Abs(val34.y) > 0.01f || (Mathf.Abs(val35.y) > 0.01f && val32.y * val33.y < 0f) || Mathf.Abs(val35.z) - Mathf.Abs(val34.z) > 0.01f || (Mathf.Abs(val35.z) > 0.01f && val32.z * val33.z < 0f) || (flag7 && val9.x == 0f && val9.z == 0f && component2.UpType == DungeonGridLinkType.Default && ((Mathf.Abs(val33.x) > 0.01f && Mathf.Abs(val33.x) < LinkRadius * 2f - 0.1f) || (Mathf.Abs(val33.z) > 0.01f && Mathf.Abs(val33.z) < LinkRadius * 2f - 0.1f))))
								{
									continue;
								}
								num21 = num23;
								if (val9.x == 0f && val9.z == 0f)
								{
									if (!flag7 && Mathf.Abs(val33.y) < LinkTransition - 0.1f)
									{
										continue;
									}
								}
								else if ((!flag7 && magnitude8 > 0.01f && (Mathf.Abs(val33.x) < LinkRadius * 2f - 0.1f || Mathf.Abs(val33.z) < LinkRadius * 2f - 0.1f)) || (!flag8 && magnitude8 > 0.01f && (Mathf.Abs(val33.x) < LinkRadius * 1f - 0.1f || Mathf.Abs(val33.z) < LinkRadius * 1f - 0.1f)))
								{
									continue;
								}
								if (!flag7 || !(magnitude8 < 0.01f) || !(magnitude6 < 0.01f) || !(Quaternion.Angle(val28, val27) > 10f))
								{
									prefab14 = prefab15;
									val23 = val31;
									num21 = num23;
									position4 = val30;
									rotation3 = val26;
								}
							}
							if (val23 != Vector3.zero)
							{
								PathLinkSegment pathLinkSegment2 = new PathLinkSegment();
								pathLinkSegment2.position = position4;
								pathLinkSegment2.rotation = rotation3;
								pathLinkSegment2.scale = Vector3.one;
								pathLinkSegment2.prefab = prefab14;
								pathLinkSegment2.link = prefab14.Component;
								item5.upwards.segments.Add(pathLinkSegment2);
								val8 -= val23;
							}
							else
							{
								num16++;
							}
						}
						else
						{
							num16++;
						}
					}
				}
			}
			foreach (PathLink item6 in list2)
			{
				foreach (PathLinkSegment segment2 in item6.downwards.segments)
				{
					World.AddPrefab("Dungeon", segment2.prefab, segment2.position, segment2.rotation, segment2.scale);
				}
				foreach (PathLinkSegment segment3 in item6.upwards.segments)
				{
					World.AddPrefab("Dungeon", segment3.prefab, segment3.position, segment3.rotation, segment3.scale);
				}
			}
			if (TerrainMeta.Path.Rails.Count > 0)
			{
				List<PrefabReplacement> list7 = new List<PrefabReplacement>();
				for (int num24 = 0; num24 < grid.CellCount; num24++)
				{
					for (int num25 = 0; num25 < grid.CellCount; num25++)
					{
						Prefab<DungeonGridCell> prefab16 = grid[num24, num25];
						if (prefab16 == null || !prefab16.Component.Replaceable)
						{
							continue;
						}
						Vector2i val36 = new Vector2i(num24, num25);
						Vector3 val37 = grid.GridToWorldCoords(val36) + zero2;
						Prefab<DungeonGridCell>[] array7 = array3;
						foreach (Prefab<DungeonGridCell> prefab17 in array7)
						{
							if (prefab16.Component.North != prefab17.Component.North || prefab16.Component.South != prefab17.Component.South || prefab16.Component.West != prefab17.Component.West || prefab16.Component.East != prefab17.Component.East || !IsValidPrefabPlacement(prefab17, val37, Quaternion.identity, Vector3.one, EnvironmentType.Underground) || !prefab17.ApplyTerrainChecks(val37, Quaternion.identity, Vector3.one) || !prefab17.ApplyTerrainFilters(val37, Quaternion.identity, Vector3.one))
							{
								continue;
							}
							MonumentInfo componentInChildren6 = prefab17.Object.GetComponentInChildren<MonumentInfo>();
							Vector3 val38 = val37;
							if (Object.op_Implicit((Object)(object)componentInChildren6))
							{
								val38 += ((Component)componentInChildren6).transform.position;
							}
							if (!(val38.y < 1f))
							{
								float distanceToAboveGroundRail = GetDistanceToAboveGroundRail(val38);
								if (!(distanceToAboveGroundRail < 200f))
								{
									list7.Add(new PrefabReplacement
									{
										gridPosition = val36,
										worldPosition = val38,
										distance = Mathf.RoundToInt(distanceToAboveGroundRail),
										prefab = prefab17
									});
								}
							}
						}
					}
				}
				ListEx.Shuffle<PrefabReplacement>(list7, ref seed);
				list7.Sort((PrefabReplacement a, PrefabReplacement b) => a.distance.CompareTo(b.distance));
				int num26 = 2;
				while (num26 > 0 && list7.Count > 0)
				{
					num26--;
					PrefabReplacement replacement = list7[0];
					grid[replacement.gridPosition.x, replacement.gridPosition.y] = replacement.prefab;
					list7.RemoveAll((PrefabReplacement a) =>
					{
						//IL_0001: Unknown result type (might be due to invalid IL or missing references)
						//IL_000c: Unknown result type (might be due to invalid IL or missing references)
						//IL_0011: Unknown result type (might be due to invalid IL or missing references)
						//IL_0016: Unknown result type (might be due to invalid IL or missing references)
						Vector3 val45 = a.worldPosition - replacement.worldPosition;
						return val45.magnitude < 1500f;
					});
				}
			}
			for (int num27 = 0; num27 < grid.CellCount; num27++)
			{
				for (int num28 = 0; num28 < grid.CellCount; num28++)
				{
					Prefab<DungeonGridCell> prefab18 = grid[num27, num28];
					if (prefab18 != null)
					{
						Vector2i val39 = new Vector2i(num27, num28);
						Vector3 val40 = grid.GridToWorldCoords(val39);
						World.AddPrefab("Dungeon", prefab18, zero2 + val40, Quaternion.identity, Vector3.one);
					}
				}
			}
			for (int num29 = 0; num29 < grid.CellCount - 1; num29++)
			{
				for (int num30 = 0; num30 < grid.CellCount - 1; num30++)
				{
					Prefab<DungeonGridCell> prefab19 = grid[num29, num30];
					Prefab<DungeonGridCell> prefab20 = grid[num29 + 1, num30];
					Prefab<DungeonGridCell> prefab21 = grid[num29, num30 + 1];
					Prefab<DungeonGridCell>[] array7;
					if (prefab19 != null && prefab20 != null && prefab19.Component.EastVariant != prefab20.Component.WestVariant)
					{
						ArrayEx.Shuffle(array4, ref seed);
						array7 = array4;
						foreach (Prefab<DungeonGridCell> prefab22 in array7)
						{
							if (prefab22.Component.West == prefab19.Component.East && prefab22.Component.East == prefab20.Component.West && prefab22.Component.WestVariant == prefab19.Component.EastVariant && prefab22.Component.EastVariant == prefab20.Component.WestVariant)
							{
								Vector2i val41 = new Vector2i(num29, num30);
								Vector3 val42 = grid.GridToWorldCoords(val41) + new Vector3(grid.CellSizeHalf, 0f, 0f);
								World.AddPrefab("Dungeon", prefab22, zero2 + val42, Quaternion.identity, Vector3.one);
								break;
							}
						}
					}
					if (prefab19 == null || prefab21 == null || prefab19.Component.NorthVariant == prefab21.Component.SouthVariant)
					{
						continue;
					}
					ArrayEx.Shuffle(array4, ref seed);
					array7 = array4;
					foreach (Prefab<DungeonGridCell> prefab23 in array7)
					{
						if (prefab23.Component.South == prefab19.Component.North && prefab23.Component.North == prefab21.Component.South && prefab23.Component.SouthVariant == prefab19.Component.NorthVariant && prefab23.Component.NorthVariant == prefab21.Component.SouthVariant)
						{
							Vector2i val43 = new Vector2i(num29, num30);
							Vector3 val44 = grid.GridToWorldCoords(val43) + new Vector3(0f, 0f, grid.CellSizeHalf);
							World.AddPrefab("Dungeon", prefab23, zero2 + val44, Quaternion.identity, Vector3.one);
							break;
						}
					}
				}
			}
		}
		bool IsNearEntrance(int x, int z)
		{
			//IL_0022: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			foreach (DungeonGridInfo item7 in entrances)
			{
				Vector2i val45 = grid.WorldToGridCoords(((Component)item7).transform.position);
				if (Mathf.Abs(val45.x - x) + Mathf.Abs(val45.y - z) <= 1)
				{
					return true;
				}
			}
			return false;
		}
		bool IsValidPrefabPlacement(Prefab<DungeonGridCell> prefab24, Vector3 pos, Quaternion rot, Vector3 scale, EnvironmentType type)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_0066: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			//IL_004d: Unknown result type (might be due to invalid IL or missing references)
			//IL_007b: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_007d: Unknown result type (might be due to invalid IL or missing references)
			Vector3 val45 = Vector3.up * 9f;
			Vector3 val46 = Vector3.up * (LinkTransition + 1f);
			if (!prefab24.CheckEnvironmentVolumesBelowAltitude(pos + val45, rot, scale, EnvironmentType.Underground, EnvironmentType.Entrance) && !prefab24.CheckEnvironmentVolumesInsideTerrain(pos + val45, rot, scale, EnvironmentType.TrainTunnels, EnvironmentType.Entrance))
			{
				return false;
			}
			if (prefab24.CheckEnvironmentVolumes(pos + val46, rot, scale, type))
			{
				return false;
			}
			if (prefab24.CheckEnvironmentVolumes(pos, rot, scale, type))
			{
				return false;
			}
			return true;
		}
	}

	private float GetDistanceToAboveGroundRail(Vector3 pos)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		float num = float.MaxValue;
		if (Object.op_Implicit((Object)(object)TerrainMeta.Path))
		{
			foreach (PathList rail in TerrainMeta.Path.Rails)
			{
				Vector3[] points = rail.Path.Points;
				foreach (Vector3 val in points)
				{
					num = Mathf.Min(num, Vector3Ex.Distance2D(val, pos));
				}
			}
		}
		return num;
	}
}
