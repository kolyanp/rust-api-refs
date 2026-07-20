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
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_08bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		//IL_107f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1084: Unknown result type (might be due to invalid IL or missing references)
		//IL_1086: Unknown result type (might be due to invalid IL or missing references)
		//IL_108b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0319: Unknown result type (might be due to invalid IL or missing references)
		//IL_0320: Unknown result type (might be due to invalid IL or missing references)
		//IL_108d: Unknown result type (might be due to invalid IL or missing references)
		//IL_108f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Unknown result type (might be due to invalid IL or missing references)
		//IL_0340: Unknown result type (might be due to invalid IL or missing references)
		//IL_0347: Unknown result type (might be due to invalid IL or missing references)
		//IL_1116: Unknown result type (might be due to invalid IL or missing references)
		//IL_1118: Unknown result type (might be due to invalid IL or missing references)
		//IL_10b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_10bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_10c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_10d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_10d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_10db: Unknown result type (might be due to invalid IL or missing references)
		//IL_10e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_10e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_1141: Unknown result type (might be due to invalid IL or missing references)
		//IL_1146: Unknown result type (might be due to invalid IL or missing references)
		//IL_1148: Unknown result type (might be due to invalid IL or missing references)
		//IL_114d: Unknown result type (might be due to invalid IL or missing references)
		//IL_118e: Unknown result type (might be due to invalid IL or missing references)
		//IL_119f: Unknown result type (might be due to invalid IL or missing references)
		//IL_11b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_11c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_11cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_11d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_11d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_11da: Unknown result type (might be due to invalid IL or missing references)
		//IL_11e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_11f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_120f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1220: Unknown result type (might be due to invalid IL or missing references)
		//IL_1225: Unknown result type (might be due to invalid IL or missing references)
		//IL_122a: Unknown result type (might be due to invalid IL or missing references)
		//IL_122f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1234: Unknown result type (might be due to invalid IL or missing references)
		//IL_1236: Unknown result type (might be due to invalid IL or missing references)
		//IL_123b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1254: Unknown result type (might be due to invalid IL or missing references)
		//IL_1259: Unknown result type (might be due to invalid IL or missing references)
		//IL_126f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1274: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_1287: Unknown result type (might be due to invalid IL or missing references)
		//IL_128c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03da: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f1e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f25: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f2c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_12ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_12bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_12c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_12cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_12cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_12d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_12e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_12ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_12f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_1300: Unknown result type (might be due to invalid IL or missing references)
		//IL_1305: Unknown result type (might be due to invalid IL or missing references)
		//IL_130a: Unknown result type (might be due to invalid IL or missing references)
		//IL_130f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1314: Unknown result type (might be due to invalid IL or missing references)
		//IL_1318: Unknown result type (might be due to invalid IL or missing references)
		//IL_1324: Unknown result type (might be due to invalid IL or missing references)
		//IL_1329: Unknown result type (might be due to invalid IL or missing references)
		//IL_132e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1820: Unknown result type (might be due to invalid IL or missing references)
		//IL_1822: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f7b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f82: Unknown result type (might be due to invalid IL or missing references)
		//IL_1f89: Unknown result type (might be due to invalid IL or missing references)
		//IL_229f: Unknown result type (might be due to invalid IL or missing references)
		//IL_22a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_22a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_22af: Unknown result type (might be due to invalid IL or missing references)
		//IL_22b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_22b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_22b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_22bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0558: Unknown result type (might be due to invalid IL or missing references)
		//IL_055f: Unknown result type (might be due to invalid IL or missing references)
		//IL_056f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0576: Unknown result type (might be due to invalid IL or missing references)
		//IL_1837: Unknown result type (might be due to invalid IL or missing references)
		//IL_1839: Unknown result type (might be due to invalid IL or missing references)
		//IL_1840: Unknown result type (might be due to invalid IL or missing references)
		//IL_1842: Unknown result type (might be due to invalid IL or missing references)
		//IL_1849: Unknown result type (might be due to invalid IL or missing references)
		//IL_184e: Unknown result type (might be due to invalid IL or missing references)
		//IL_187d: Unknown result type (might be due to invalid IL or missing references)
		//IL_187f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1881: Unknown result type (might be due to invalid IL or missing references)
		//IL_1886: Unknown result type (might be due to invalid IL or missing references)
		//IL_2026: Unknown result type (might be due to invalid IL or missing references)
		//IL_2028: Unknown result type (might be due to invalid IL or missing references)
		//IL_202d: Unknown result type (might be due to invalid IL or missing references)
		//IL_202f: Unknown result type (might be due to invalid IL or missing references)
		//IL_2034: Unknown result type (might be due to invalid IL or missing references)
		//IL_05de: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_13b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_1890: Unknown result type (might be due to invalid IL or missing references)
		//IL_0609: Unknown result type (might be due to invalid IL or missing references)
		//IL_0612: Unknown result type (might be due to invalid IL or missing references)
		//IL_138d: Unknown result type (might be due to invalid IL or missing references)
		//IL_13c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_18b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_18b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_18c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_18c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_18c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_18cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_18de: Unknown result type (might be due to invalid IL or missing references)
		//IL_18e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_18ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_18f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_18fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_1902: Unknown result type (might be due to invalid IL or missing references)
		//IL_1907: Unknown result type (might be due to invalid IL or missing references)
		//IL_190c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1910: Unknown result type (might be due to invalid IL or missing references)
		//IL_191c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1921: Unknown result type (might be due to invalid IL or missing references)
		//IL_1926: Unknown result type (might be due to invalid IL or missing references)
		//IL_189e: Unknown result type (might be due to invalid IL or missing references)
		//IL_049e: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_04af: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0634: Unknown result type (might be due to invalid IL or missing references)
		//IL_063b: Unknown result type (might be due to invalid IL or missing references)
		//IL_139e: Unknown result type (might be due to invalid IL or missing references)
		//IL_04eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_050a: Unknown result type (might be due to invalid IL or missing references)
		//IL_050f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0514: Unknown result type (might be due to invalid IL or missing references)
		//IL_051d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0522: Unknown result type (might be due to invalid IL or missing references)
		//IL_0524: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_06bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_06c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_06d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_06da: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_06e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_06eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_071d: Unknown result type (might be due to invalid IL or missing references)
		//IL_071f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0730: Unknown result type (might be due to invalid IL or missing references)
		//IL_0732: Unknown result type (might be due to invalid IL or missing references)
		//IL_0737: Unknown result type (might be due to invalid IL or missing references)
		//IL_0748: Unknown result type (might be due to invalid IL or missing references)
		//IL_074d: Unknown result type (might be due to invalid IL or missing references)
		//IL_079f: Unknown result type (might be due to invalid IL or missing references)
		//IL_07a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_07cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_065f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0666: Unknown result type (might be due to invalid IL or missing references)
		//IL_13ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_13f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_13fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_13ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_1404: Unknown result type (might be due to invalid IL or missing references)
		//IL_1406: Unknown result type (might be due to invalid IL or missing references)
		//IL_140f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1414: Unknown result type (might be due to invalid IL or missing references)
		//IL_1419: Unknown result type (might be due to invalid IL or missing references)
		//IL_142b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1437: Unknown result type (might be due to invalid IL or missing references)
		//IL_143c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1441: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e18: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e1a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1489: Unknown result type (might be due to invalid IL or missing references)
		//IL_148b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1494: Unknown result type (might be due to invalid IL or missing references)
		//IL_1499: Unknown result type (might be due to invalid IL or missing references)
		//IL_149e: Unknown result type (might be due to invalid IL or missing references)
		//IL_14a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_14a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_14ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_14ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_14bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_14c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_14c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_14cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_14cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_14cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_14d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_14d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_14d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_14da: Unknown result type (might be due to invalid IL or missing references)
		//IL_14df: Unknown result type (might be due to invalid IL or missing references)
		//IL_14f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_14f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_14f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_14fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_14fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_1500: Unknown result type (might be due to invalid IL or missing references)
		//IL_1502: Unknown result type (might be due to invalid IL or missing references)
		//IL_1507: Unknown result type (might be due to invalid IL or missing references)
		//IL_151b: Unknown result type (might be due to invalid IL or missing references)
		//IL_151d: Unknown result type (might be due to invalid IL or missing references)
		//IL_144d: Unknown result type (might be due to invalid IL or missing references)
		//IL_144f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e2f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e31: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e38: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e3a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e41: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e46: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e75: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e77: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e79: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e7e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1463: Unknown result type (might be due to invalid IL or missing references)
		//IL_1465: Unknown result type (might be due to invalid IL or missing references)
		//IL_1467: Unknown result type (might be due to invalid IL or missing references)
		//IL_146c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1471: Unknown result type (might be due to invalid IL or missing references)
		//IL_1473: Unknown result type (might be due to invalid IL or missing references)
		//IL_1475: Unknown result type (might be due to invalid IL or missing references)
		//IL_1477: Unknown result type (might be due to invalid IL or missing references)
		//IL_147c: Unknown result type (might be due to invalid IL or missing references)
		//IL_147e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1480: Unknown result type (might be due to invalid IL or missing references)
		//IL_1482: Unknown result type (might be due to invalid IL or missing references)
		//IL_1487: Unknown result type (might be due to invalid IL or missing references)
		//IL_19a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_20bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_20c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_20c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_1564: Unknown result type (might be due to invalid IL or missing references)
		//IL_1570: Unknown result type (might be due to invalid IL or missing references)
		//IL_1985: Unknown result type (might be due to invalid IL or missing references)
		//IL_19ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_20da: Unknown result type (might be due to invalid IL or missing references)
		//IL_20dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_20e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_1587: Unknown result type (might be due to invalid IL or missing references)
		//IL_1996: Unknown result type (might be due to invalid IL or missing references)
		//IL_20f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_20f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_20fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_15b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_15bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_159a: Unknown result type (might be due to invalid IL or missing references)
		//IL_15a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_19e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_19ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_19f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_19f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_19fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_19fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a07: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a0c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a11: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a23: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a2f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a34: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a39: Unknown result type (might be due to invalid IL or missing references)
		//IL_23e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_23e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_23fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_2403: Unknown result type (might be due to invalid IL or missing references)
		//IL_2408: Unknown result type (might be due to invalid IL or missing references)
		//IL_2411: Unknown result type (might be due to invalid IL or missing references)
		//IL_2413: Unknown result type (might be due to invalid IL or missing references)
		//IL_2415: Unknown result type (might be due to invalid IL or missing references)
		//IL_241a: Unknown result type (might be due to invalid IL or missing references)
		//IL_241f: Unknown result type (might be due to invalid IL or missing references)
		//IL_2118: Unknown result type (might be due to invalid IL or missing references)
		//IL_211a: Unknown result type (might be due to invalid IL or missing references)
		//IL_15d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a81: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a83: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a8c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a91: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a96: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a9b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a9d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1aa6: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ab2: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ab7: Unknown result type (might be due to invalid IL or missing references)
		//IL_1abc: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ac1: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ac3: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ac5: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ac7: Unknown result type (might be due to invalid IL or missing references)
		//IL_1acc: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ace: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ad0: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ad2: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ad7: Unknown result type (might be due to invalid IL or missing references)
		//IL_1aeb: Unknown result type (might be due to invalid IL or missing references)
		//IL_1aed: Unknown result type (might be due to invalid IL or missing references)
		//IL_1aef: Unknown result type (might be due to invalid IL or missing references)
		//IL_1af4: Unknown result type (might be due to invalid IL or missing references)
		//IL_1af6: Unknown result type (might be due to invalid IL or missing references)
		//IL_1af8: Unknown result type (might be due to invalid IL or missing references)
		//IL_1afa: Unknown result type (might be due to invalid IL or missing references)
		//IL_1aff: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b13: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b15: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a45: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a47: Unknown result type (might be due to invalid IL or missing references)
		//IL_24fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_2500: Unknown result type (might be due to invalid IL or missing references)
		//IL_2516: Unknown result type (might be due to invalid IL or missing references)
		//IL_251b: Unknown result type (might be due to invalid IL or missing references)
		//IL_2520: Unknown result type (might be due to invalid IL or missing references)
		//IL_2529: Unknown result type (might be due to invalid IL or missing references)
		//IL_252b: Unknown result type (might be due to invalid IL or missing references)
		//IL_252d: Unknown result type (might be due to invalid IL or missing references)
		//IL_2532: Unknown result type (might be due to invalid IL or missing references)
		//IL_2537: Unknown result type (might be due to invalid IL or missing references)
		//IL_213a: Unknown result type (might be due to invalid IL or missing references)
		//IL_2125: Unknown result type (might be due to invalid IL or missing references)
		//IL_212e: Unknown result type (might be due to invalid IL or missing references)
		//IL_2133: Unknown result type (might be due to invalid IL or missing references)
		//IL_2138: Unknown result type (might be due to invalid IL or missing references)
		//IL_1602: Unknown result type (might be due to invalid IL or missing references)
		//IL_160e: Unknown result type (might be due to invalid IL or missing references)
		//IL_15e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_15f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a5b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a5d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a5f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a64: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a69: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a6b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a6d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a6f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a74: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a76: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a78: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a7a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1a7f: Unknown result type (might be due to invalid IL or missing references)
		//IL_2149: Unknown result type (might be due to invalid IL or missing references)
		//IL_1625: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b5c: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b68: Unknown result type (might be due to invalid IL or missing references)
		//IL_2165: Unknown result type (might be due to invalid IL or missing references)
		//IL_2167: Unknown result type (might be due to invalid IL or missing references)
		//IL_216e: Unknown result type (might be due to invalid IL or missing references)
		//IL_2170: Unknown result type (might be due to invalid IL or missing references)
		//IL_1638: Unknown result type (might be due to invalid IL or missing references)
		//IL_163f: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b7f: Unknown result type (might be due to invalid IL or missing references)
		//IL_16f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_1658: Unknown result type (might be due to invalid IL or missing references)
		//IL_1bab: Unknown result type (might be due to invalid IL or missing references)
		//IL_1bb7: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b92: Unknown result type (might be due to invalid IL or missing references)
		//IL_1b99: Unknown result type (might be due to invalid IL or missing references)
		//IL_16fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_1669: Unknown result type (might be due to invalid IL or missing references)
		//IL_1bce: Unknown result type (might be due to invalid IL or missing references)
		//IL_1bfa: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c06: Unknown result type (might be due to invalid IL or missing references)
		//IL_1be1: Unknown result type (might be due to invalid IL or missing references)
		//IL_1be8: Unknown result type (might be due to invalid IL or missing references)
		//IL_1742: Unknown result type (might be due to invalid IL or missing references)
		//IL_1713: Unknown result type (might be due to invalid IL or missing references)
		//IL_1680: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c1d: Unknown result type (might be due to invalid IL or missing references)
		//IL_17ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_1801: Unknown result type (might be due to invalid IL or missing references)
		//IL_1807: Unknown result type (might be due to invalid IL or missing references)
		//IL_1809: Unknown result type (might be due to invalid IL or missing references)
		//IL_180b: Unknown result type (might be due to invalid IL or missing references)
		//IL_180d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1795: Unknown result type (might be due to invalid IL or missing references)
		//IL_1765: Unknown result type (might be due to invalid IL or missing references)
		//IL_16b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_1693: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c30: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c37: Unknown result type (might be due to invalid IL or missing references)
		//IL_17b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_16c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_1ce8: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c50: Unknown result type (might be due to invalid IL or missing references)
		//IL_17eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_17ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_1cf6: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c61: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d3a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d0b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c78: Unknown result type (might be due to invalid IL or missing references)
		//IL_1df7: Unknown result type (might be due to invalid IL or missing references)
		//IL_1df9: Unknown result type (might be due to invalid IL or missing references)
		//IL_1dff: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e01: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e03: Unknown result type (might be due to invalid IL or missing references)
		//IL_1e05: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d8d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1d5d: Unknown result type (might be due to invalid IL or missing references)
		//IL_1cae: Unknown result type (might be due to invalid IL or missing references)
		//IL_1c8b: Unknown result type (might be due to invalid IL or missing references)
		//IL_1dad: Unknown result type (might be due to invalid IL or missing references)
		//IL_1cc1: Unknown result type (might be due to invalid IL or missing references)
		//IL_1de3: Unknown result type (might be due to invalid IL or missing references)
		//IL_1de5: Unknown result type (might be due to invalid IL or missing references)
		if (World.Cached)
		{
			return;
		}
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
			List<DungeonGridInfo> list = (Object.op_Implicit((Object)(object)TerrainMeta.Path) ? TerrainMeta.Path.DungeonGridEntrances : null);
			WorldSpaceGrid<Prefab<DungeonGridCell>> val = new WorldSpaceGrid<Prefab<DungeonGridCell>>(TerrainMeta.Size.x, (float)CellSize, (RoundingMode)1);
			int[,] array6 = new int[val.CellCount, val.CellCount];
			DungeonGridConnectionHash[,] hashmap = new DungeonGridConnectionHash[val.CellCount, val.CellCount];
			PathFinder pathFinder = new PathFinder(array6, diagonals: false);
			int cellCount = val.CellCount;
			int num = 0;
			int num2 = val.CellCount - 1;
			for (int num3 = 0; num3 < cellCount; num3++)
			{
				for (int num4 = 0; num4 < cellCount; num4++)
				{
					array6[num4, num3] = 1;
				}
			}
			List<PathSegment> list2 = new List<PathSegment>();
			List<PathLink> list3 = new List<PathLink>();
			List<PathNode> list4 = new List<PathNode>();
			List<PathNode> unconnectedNodeList = new List<PathNode>();
			List<PathNode> secondaryNodeList = new List<PathNode>();
			List<PathFinder.Point> list5 = new List<PathFinder.Point>();
			List<PathFinder.Point> list6 = new List<PathFinder.Point>();
			List<PathFinder.Point> list7 = new List<PathFinder.Point>();
			foreach (DungeonGridInfo item2 in list)
			{
				DungeonGridInfo entrance = item2;
				TerrainPathConnect[] componentsInChildren = ((Component)entrance).GetComponentsInChildren<TerrainPathConnect>(true);
				foreach (TerrainPathConnect terrainPathConnect in componentsInChildren)
				{
					if (terrainPathConnect.Type != ConnectionType)
					{
						continue;
					}
					Vector2i val2 = val.WorldToGridCoords(((Component)terrainPathConnect).transform.position);
					if (array6[val2.x, val2.y] == int.MaxValue)
					{
						continue;
					}
					PathFinder.Node stationNode = pathFinder.FindClosestWalkable(new PathFinder.Point(val2.x, val2.y), 1);
					if (stationNode == null)
					{
						continue;
					}
					Prefab<DungeonGridCell> prefab = ((val2.x > num) ? val[val2.x - 1, val2.y] : null);
					Prefab<DungeonGridCell> prefab2 = ((val2.x < num2) ? val[val2.x + 1, val2.y] : null);
					Prefab<DungeonGridCell> prefab3 = ((val2.y > num) ? val[val2.x, val2.y - 1] : null);
					Prefab<DungeonGridCell> obj = ((val2.y < num2) ? val[val2.x, val2.y + 1] : null);
					DungeonGridConnectionType dungeonGridConnectionType = prefab?.Component.East ?? DungeonGridConnectionType.None;
					DungeonGridConnectionType dungeonGridConnectionType2 = prefab2?.Component.West ?? DungeonGridConnectionType.None;
					DungeonGridConnectionType dungeonGridConnectionType3 = prefab3?.Component.North ?? DungeonGridConnectionType.None;
					DungeonGridConnectionType dungeonGridConnectionType4 = obj?.Component.South ?? DungeonGridConnectionType.None;
					bool flag = prefab != null || val2.x <= num;
					bool flag2 = prefab2 != null || val2.x >= num2;
					bool flag3 = prefab3 != null || val2.y <= num;
					bool flag4 = obj != null || val2.y >= num2;
					Prefab<DungeonGridCell> prefab4 = null;
					float num6 = float.MaxValue;
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
						OBB bounds = componentInChildren.GetBounds(val.GridToWorldCoords(val2), Quaternion.identity);
						OBB bounds2 = componentInChildren2.GetBounds(((Component)entrance).transform.position, Quaternion.identity);
						if (!((OBB)(ref bounds)).Intersects2D(bounds2))
						{
							DungeonGridLink componentInChildren3 = prefab5.Object.GetComponentInChildren<DungeonGridLink>();
							Vector3 val3 = val.GridToWorldCoords(new Vector2i(val2.x, val2.y)) + componentInChildren3.UpSocket.localPosition;
							float num8 = Vector3Ex.Magnitude2D(((Component)terrainPathConnect).transform.position - val3);
							if (!(num6 < num8))
							{
								prefab4 = prefab5;
								num6 = num8;
							}
						}
					}
					bool isStartPoint;
					if (prefab4 != null)
					{
						val[val2.x, val2.y] = prefab4;
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
						Vector3 eulerAngles = ((Quaternion)(ref rotation)).eulerAngles;
						DungeonGridLink componentInChildren5 = prefab4.Object.GetComponentInChildren<DungeonGridLink>();
						Vector3 position2 = val.GridToWorldCoords(new Vector2i(val2.x, val2.y));
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
						list3.Add(pathLink);
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
					Vector2i val4 = val.WorldToGridCoords(((Component)node.monument).transform.position);
					pathFinder.PushPoint = new PathFinder.Point(val4.x, val4.y);
					pathFinder.PushRadius = (pathFinder.PushDistance = 2);
					pathFinder.PushMultiplier = 16;
				}
				list7.Clear();
				list7.AddRange(unconnectedNodeList.Select((PathNode x) => x.node.point));
				list6.Clear();
				list6.AddRange(list4.Select((PathNode x) => x.node.point));
				list6.AddRange(secondaryNodeList.Select((PathNode x) => x.node.point));
				list6.AddRange(list5);
				PathFinder.Node node2 = pathFinder.FindPathUndirected(list6, list7, 100000);
				if (node2 == null)
				{
					PathNode node3 = unconnectedNodeList[0];
					secondaryNodeList.AddRange(unconnectedNodeList.Where((PathNode x) => (Object)(object)x.monument == (Object)(object)node3.monument));
					unconnectedNodeList.RemoveAll((PathNode x) => (Object)(object)x.monument == (Object)(object)node3.monument);
					secondaryNodeList.Remove(node3);
					list4.Add(node3);
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
				list2.Add(segment);
				PathNode node5 = unconnectedNodeList.Find((PathNode x) => x.node.point == segment.start.point || x.node.point == segment.end.point);
				secondaryNodeList.AddRange(unconnectedNodeList.Where((PathNode x) => (Object)(object)x.monument == (Object)(object)node5.monument));
				unconnectedNodeList.RemoveAll((PathNode x) => (Object)(object)x.monument == (Object)(object)node5.monument);
				secondaryNodeList.Remove(node5);
				list4.Add(node5);
				PathNode pathNode = secondaryNodeList.Find((PathNode x) => x.node.point == segment.start.point || x.node.point == segment.end.point);
				if (pathNode != null)
				{
					secondaryNodeList.Remove(pathNode);
					list4.Add(pathNode);
				}
				for (PathFinder.Node node6 = node2; node6 != null; node6 = node6.next)
				{
					if (node6 != node2 && node6.next != null)
					{
						list5.Add(node6.point);
					}
				}
			}
			foreach (PathSegment item3 in list2)
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
			for (int num9 = 0; num9 < val.CellCount; num9++)
			{
				for (int num10 = 0; num10 < val.CellCount; num10++)
				{
					if (array6[num9, num10] == int.MaxValue)
					{
						continue;
					}
					DungeonGridConnectionHash dungeonGridConnectionHash3 = hashmap[num9, num10];
					if (dungeonGridConnectionHash3.Value == 0)
					{
						continue;
					}
					ArrayEx.Shuffle(array, ref seed);
					Prefab<DungeonGridCell>[] array7 = array;
					foreach (Prefab<DungeonGridCell> prefab6 in array7)
					{
						Prefab<DungeonGridCell> prefab7 = ((num9 > num) ? val[num9 - 1, num10] : null);
						if (((prefab7 != null) ? ((prefab6.Component.West == prefab7.Component.East) ? 1 : 0) : (dungeonGridConnectionHash3.West ? ((int)prefab6.Component.West) : ((prefab6.Component.West == DungeonGridConnectionType.None) ? 1 : 0))) == 0)
						{
							continue;
						}
						Prefab<DungeonGridCell> prefab8 = ((num9 < num2) ? val[num9 + 1, num10] : null);
						if (((prefab8 != null) ? ((prefab6.Component.East == prefab8.Component.West) ? 1 : 0) : (dungeonGridConnectionHash3.East ? ((int)prefab6.Component.East) : ((prefab6.Component.East == DungeonGridConnectionType.None) ? 1 : 0))) == 0)
						{
							continue;
						}
						Prefab<DungeonGridCell> prefab9 = ((num10 > num) ? val[num9, num10 - 1] : null);
						if (((prefab9 != null) ? ((prefab6.Component.South == prefab9.Component.North) ? 1 : 0) : (dungeonGridConnectionHash3.South ? ((int)prefab6.Component.South) : ((prefab6.Component.South == DungeonGridConnectionType.None) ? 1 : 0))) == 0)
						{
							continue;
						}
						Prefab<DungeonGridCell> prefab10 = ((num10 < num2) ? val[num9, num10 + 1] : null);
						if (((prefab10 != null) ? (prefab6.Component.North == prefab10.Component.South) : (dungeonGridConnectionHash3.North ? ((byte)prefab6.Component.North != 0) : (prefab6.Component.North == DungeonGridConnectionType.None))) && (prefab6.Component.West == DungeonGridConnectionType.None || prefab7 == null || !prefab6.Component.ShouldAvoid(prefab7.ID)) && (prefab6.Component.East == DungeonGridConnectionType.None || prefab8 == null || !prefab6.Component.ShouldAvoid(prefab8.ID)) && (prefab6.Component.South == DungeonGridConnectionType.None || prefab9 == null || !prefab6.Component.ShouldAvoid(prefab9.ID)) && (prefab6.Component.North == DungeonGridConnectionType.None || prefab10 == null || !prefab6.Component.ShouldAvoid(prefab10.ID)))
						{
							val[num9, num10] = prefab6;
							bool num11 = prefab7 == null || prefab6.Component.WestVariant == prefab7.Component.EastVariant;
							bool flag5 = prefab9 == null || prefab6.Component.SouthVariant == prefab9.Component.NorthVariant;
							if (num11 && flag5)
							{
								break;
							}
						}
					}
				}
			}
			Vector3 zero2 = Vector3.zero;
			Vector3 zero3 = Vector3.zero;
			Vector2i val5 = default(Vector2i);
			do
			{
				zero3 = zero2;
				for (int num12 = 0; num12 < val.CellCount; num12++)
				{
					for (int num13 = 0; num13 < val.CellCount; num13++)
					{
						Prefab<DungeonGridCell> prefab11 = val[num12, num13];
						if (prefab11 != null)
						{
							((Vector2i)(ref val5))._002Ector(num12, num13);
							Vector3 val6 = val.GridToWorldCoords(val5);
							while (!IsValidPrefabPlacement(prefab11, zero2 + val6, Quaternion.identity, Vector3.one, EnvironmentType.Underground | EnvironmentType.Building))
							{
								zero2.y -= 3f;
							}
						}
					}
				}
			}
			while (zero2 != zero3);
			foreach (PathLink item4 in list3)
			{
				PathLinkSegment origin = item4.upwards.origin;
				origin.position += zero2;
			}
			foreach (PathLink item5 in list3)
			{
				Vector3 val7 = item5.upwards.origin.position + item5.upwards.origin.rotation * Vector3.Scale(item5.upwards.origin.upSocket.localPosition, item5.upwards.origin.scale);
				Vector3 val8 = item5.downwards.origin.position + item5.downwards.origin.rotation * Vector3.Scale(item5.downwards.origin.downSocket.localPosition, item5.downwards.origin.scale) - val7;
				Vector3[] array8 = (Vector3[])(object)new Vector3[2]
				{
					new Vector3(0f, 1f, 0f),
					new Vector3(1f, 1f, 1f)
				};
				foreach (Vector3 val9 in array8)
				{
					int num14 = 0;
					int num15 = 0;
					while (((Vector3)(ref val8)).magnitude > 1f && (num14 < 8 || num15 < 8))
					{
						bool flag6 = num14 > 2 && num15 > 2;
						bool flag7 = num14 > 4 && num15 > 4;
						Prefab<DungeonGridLink> prefab12 = null;
						Vector3 val10 = Vector3.zero;
						int num16 = int.MinValue;
						Vector3 position3 = Vector3.zero;
						Quaternion rotation2 = Quaternion.identity;
						PathLinkSegment prevSegment = item5.downwards.prevSegment;
						Vector3 val11 = prevSegment.position + prevSegment.rotation * Vector3.Scale(prevSegment.scale, prevSegment.downSocket.localPosition);
						Quaternion val12 = prevSegment.rotation * prevSegment.downSocket.localRotation;
						Prefab<DungeonGridLink>[] array9 = array5;
						foreach (Prefab<DungeonGridLink> prefab13 in array9)
						{
							float num17 = SeedRandom.Value(ref seed);
							DungeonGridLink component = prefab13.Component;
							if (prevSegment.downType != component.UpType)
							{
								continue;
							}
							switch (component.DownType)
							{
							case DungeonGridLinkType.Elevator:
								if (flag6 || val9.x != 0f || val9.z != 0f)
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
							int num18 = ((!flag6) ? component.Priority : 0);
							if (num16 > num18)
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
							float magnitude = ((Vector3)(ref val19)).magnitude;
							float magnitude2 = ((Vector3)(ref val20)).magnitude;
							Vector3 val21 = Vector3.Scale(val19, val9);
							Vector3 val22 = Vector3.Scale(val20, val9);
							float magnitude3 = ((Vector3)(ref val21)).magnitude;
							float magnitude4 = ((Vector3)(ref val22)).magnitude;
							if (val10 != Vector3.zero)
							{
								if (magnitude3 < magnitude4 || (magnitude3 == magnitude4 && magnitude < magnitude2) || (magnitude3 == magnitude4 && magnitude == magnitude2 && num17 < 0.5f))
								{
									continue;
								}
							}
							else if (magnitude3 <= magnitude4)
							{
								continue;
							}
							if (Mathf.Abs(val22.x) - Mathf.Abs(val21.x) > 0.01f || (Mathf.Abs(val22.x) > 0.01f && val19.x * val20.x < 0f) || Mathf.Abs(val22.y) - Mathf.Abs(val21.y) > 0.01f || (Mathf.Abs(val22.y) > 0.01f && val19.y * val20.y < 0f) || Mathf.Abs(val22.z) - Mathf.Abs(val21.z) > 0.01f || (Mathf.Abs(val22.z) > 0.01f && val19.z * val20.z < 0f) || (flag6 && val9.x == 0f && val9.z == 0f && component.DownType == DungeonGridLinkType.Default && ((Mathf.Abs(val20.x) > 0.01f && Mathf.Abs(val20.x) < LinkRadius * 2f - 0.1f) || (Mathf.Abs(val20.z) > 0.01f && Mathf.Abs(val20.z) < LinkRadius * 2f - 0.1f))))
							{
								continue;
							}
							num16 = num18;
							if (val9.x == 0f && val9.z == 0f)
							{
								if (!flag6 && Mathf.Abs(val20.y) < LinkTransition - 0.1f)
								{
									continue;
								}
							}
							else if ((!flag6 && magnitude4 > 0.01f && (Mathf.Abs(val20.x) < LinkRadius * 2f - 0.1f || Mathf.Abs(val20.z) < LinkRadius * 2f - 0.1f)) || (!flag7 && magnitude4 > 0.01f && (Mathf.Abs(val20.x) < LinkRadius * 1f - 0.1f || Mathf.Abs(val20.z) < LinkRadius * 1f - 0.1f)))
							{
								continue;
							}
							if (!flag6 || !(magnitude4 < 0.01f) || !(magnitude2 < 0.01f) || !(Quaternion.Angle(val15, val14) > 10f))
							{
								prefab12 = prefab13;
								val10 = val18;
								num16 = num18;
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
							num15++;
						}
						if (val9.x > 0f || val9.z > 0f)
						{
							Prefab<DungeonGridLink> prefab14 = null;
							Vector3 val23 = Vector3.zero;
							int num19 = int.MinValue;
							Vector3 position4 = Vector3.zero;
							Quaternion rotation3 = Quaternion.identity;
							PathLinkSegment prevSegment3 = item5.upwards.prevSegment;
							Vector3 val24 = prevSegment3.position + prevSegment3.rotation * Vector3.Scale(prevSegment3.scale, prevSegment3.upSocket.localPosition);
							Quaternion val25 = prevSegment3.rotation * prevSegment3.upSocket.localRotation;
							array9 = array5;
							foreach (Prefab<DungeonGridLink> prefab15 in array9)
							{
								float num20 = SeedRandom.Value(ref seed);
								DungeonGridLink component2 = prefab15.Component;
								if (prevSegment3.upType != component2.DownType)
								{
									continue;
								}
								switch (component2.DownType)
								{
								case DungeonGridLinkType.Elevator:
									if (flag6 || val9.x != 0f || val9.z != 0f)
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
								int num21 = ((!flag6) ? component2.Priority : 0);
								if (num19 > num21)
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
								float magnitude5 = ((Vector3)(ref val32)).magnitude;
								float magnitude6 = ((Vector3)(ref val33)).magnitude;
								Vector3 val34 = Vector3.Scale(val32, val9);
								Vector3 val35 = Vector3.Scale(val33, val9);
								float magnitude7 = ((Vector3)(ref val34)).magnitude;
								float magnitude8 = ((Vector3)(ref val35)).magnitude;
								if (val23 != Vector3.zero)
								{
									if (magnitude7 < magnitude8 || (magnitude7 == magnitude8 && magnitude5 < magnitude6) || (magnitude7 == magnitude8 && magnitude5 == magnitude6 && num20 < 0.5f))
									{
										continue;
									}
								}
								else if (magnitude7 <= magnitude8)
								{
									continue;
								}
								if (Mathf.Abs(val35.x) - Mathf.Abs(val34.x) > 0.01f || (Mathf.Abs(val35.x) > 0.01f && val32.x * val33.x < 0f) || Mathf.Abs(val35.y) - Mathf.Abs(val34.y) > 0.01f || (Mathf.Abs(val35.y) > 0.01f && val32.y * val33.y < 0f) || Mathf.Abs(val35.z) - Mathf.Abs(val34.z) > 0.01f || (Mathf.Abs(val35.z) > 0.01f && val32.z * val33.z < 0f) || (flag6 && val9.x == 0f && val9.z == 0f && component2.UpType == DungeonGridLinkType.Default && ((Mathf.Abs(val33.x) > 0.01f && Mathf.Abs(val33.x) < LinkRadius * 2f - 0.1f) || (Mathf.Abs(val33.z) > 0.01f && Mathf.Abs(val33.z) < LinkRadius * 2f - 0.1f))))
								{
									continue;
								}
								num19 = num21;
								if (val9.x == 0f && val9.z == 0f)
								{
									if (!flag6 && Mathf.Abs(val33.y) < LinkTransition - 0.1f)
									{
										continue;
									}
								}
								else if ((!flag6 && magnitude8 > 0.01f && (Mathf.Abs(val33.x) < LinkRadius * 2f - 0.1f || Mathf.Abs(val33.z) < LinkRadius * 2f - 0.1f)) || (!flag7 && magnitude8 > 0.01f && (Mathf.Abs(val33.x) < LinkRadius * 1f - 0.1f || Mathf.Abs(val33.z) < LinkRadius * 1f - 0.1f)))
								{
									continue;
								}
								if (!flag6 || !(magnitude8 < 0.01f) || !(magnitude6 < 0.01f) || !(Quaternion.Angle(val28, val27) > 10f))
								{
									prefab14 = prefab15;
									val23 = val31;
									num19 = num21;
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
								num14++;
							}
						}
						else
						{
							num14++;
						}
					}
				}
			}
			foreach (PathLink item6 in list3)
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
				List<PrefabReplacement> list8 = new List<PrefabReplacement>();
				Vector2i val36 = default(Vector2i);
				for (int num22 = 0; num22 < val.CellCount; num22++)
				{
					for (int num23 = 0; num23 < val.CellCount; num23++)
					{
						Prefab<DungeonGridCell> prefab16 = val[num22, num23];
						if (prefab16 == null || !prefab16.Component.Replaceable)
						{
							continue;
						}
						((Vector2i)(ref val36))._002Ector(num22, num23);
						Vector3 val37 = val.GridToWorldCoords(val36) + zero2;
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
									list8.Add(new PrefabReplacement
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
				ListEx.Shuffle<PrefabReplacement>(list8, ref seed);
				list8.Sort((PrefabReplacement a, PrefabReplacement b) => a.distance.CompareTo(b.distance));
				int num24 = 2;
				while (num24 > 0 && list8.Count > 0)
				{
					num24--;
					PrefabReplacement replacement = list8[0];
					val[replacement.gridPosition.x, replacement.gridPosition.y] = replacement.prefab;
					list8.RemoveAll(delegate(PrefabReplacement a)
					{
						//IL_0001: Unknown result type (might be due to invalid IL or missing references)
						//IL_000c: Unknown result type (might be due to invalid IL or missing references)
						//IL_0011: Unknown result type (might be due to invalid IL or missing references)
						//IL_0016: Unknown result type (might be due to invalid IL or missing references)
						Vector3 val45 = a.worldPosition - replacement.worldPosition;
						return ((Vector3)(ref val45)).magnitude < 1500f;
					});
				}
			}
			Vector2i val39 = default(Vector2i);
			for (int num25 = 0; num25 < val.CellCount; num25++)
			{
				for (int num26 = 0; num26 < val.CellCount; num26++)
				{
					Prefab<DungeonGridCell> prefab18 = val[num25, num26];
					if (prefab18 != null)
					{
						((Vector2i)(ref val39))._002Ector(num25, num26);
						Vector3 val40 = val.GridToWorldCoords(val39);
						World.AddPrefab("Dungeon", prefab18, zero2 + val40, Quaternion.identity, Vector3.one);
					}
				}
			}
			Vector2i val41 = default(Vector2i);
			Vector2i val43 = default(Vector2i);
			for (int num27 = 0; num27 < val.CellCount - 1; num27++)
			{
				for (int num28 = 0; num28 < val.CellCount - 1; num28++)
				{
					Prefab<DungeonGridCell> prefab19 = val[num27, num28];
					Prefab<DungeonGridCell> prefab20 = val[num27 + 1, num28];
					Prefab<DungeonGridCell> prefab21 = val[num27, num28 + 1];
					Prefab<DungeonGridCell>[] array7;
					if (prefab19 != null && prefab20 != null && prefab19.Component.EastVariant != prefab20.Component.WestVariant)
					{
						ArrayEx.Shuffle(array4, ref seed);
						array7 = array4;
						foreach (Prefab<DungeonGridCell> prefab22 in array7)
						{
							if (prefab22.Component.West == prefab19.Component.East && prefab22.Component.East == prefab20.Component.West && prefab22.Component.WestVariant == prefab19.Component.EastVariant && prefab22.Component.EastVariant == prefab20.Component.WestVariant)
							{
								((Vector2i)(ref val41))._002Ector(num27, num28);
								Vector3 val42 = val.GridToWorldCoords(val41) + new Vector3(val.CellSizeHalf, 0f, 0f);
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
							((Vector2i)(ref val43))._002Ector(num27, num28);
							Vector3 val44 = val.GridToWorldCoords(val43) + new Vector3(0f, 0f, val.CellSizeHalf);
							World.AddPrefab("Dungeon", prefab23, zero2 + val44, Quaternion.identity, Vector3.one);
							break;
						}
					}
				}
			}
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
