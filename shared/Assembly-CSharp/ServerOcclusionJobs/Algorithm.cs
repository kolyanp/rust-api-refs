using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace ServerOcclusionJobs;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct Algorithm
{
	public static bool Trace(int3 from, int3 to, in GridDefinition gridDef, int blockedGridThreshold, int neighbourThreshold, bool useNeighbourThresholds)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		int neighboursChecked = 0;
		int num2 = from.x;
		int num3 = from.y;
		int num4 = from.z;
		int x = to.x;
		int y = to.y;
		int z = to.z;
		int num5 = x - from.x;
		int num6 = y - from.y;
		int num7 = z - from.z;
		int num8 = Mathf.Abs(num5);
		int num9 = Mathf.Abs(num6);
		int num10 = Mathf.Abs(num7);
		int num11 = num8 << 1;
		int num12 = num9 << 1;
		int num13 = num10 << 1;
		int num14 = ((num5 >= 0) ? 1 : (-1));
		int num15 = ((num6 >= 0) ? 1 : (-1));
		int num16 = ((num7 >= 0) ? 1 : (-1));
		int3 nStep = -math.int3(num14, num15, num16);
		if (num8 >= num9 && num8 >= num10)
		{
			int num17 = num12 - num8;
			int num18 = num13 - num8;
			for (int i = 0; i < num8; i++)
			{
				if (!AddToGridArea(new int3(num2, num3, num4), in gridDef, nStep, ref neighboursChecked, useNeighbourThresholds, neighbourThreshold) && ++num > blockedGridThreshold)
				{
					return true;
				}
				if (num17 > 0)
				{
					num3 += num15;
					num17 -= num11;
				}
				if (num18 > 0)
				{
					num4 += num16;
					num18 -= num11;
				}
				num17 += num12;
				num18 += num13;
				num2 += num14;
			}
		}
		else if (num9 >= num8 && num9 >= num10)
		{
			int num17 = num11 - num9;
			int num18 = num13 - num9;
			for (int j = 0; j < num9; j++)
			{
				if (!AddToGridArea(new int3(num2, num3, num4), in gridDef, nStep, ref neighboursChecked, useNeighbourThresholds, neighbourThreshold) && ++num > blockedGridThreshold)
				{
					return true;
				}
				if (num17 > 0)
				{
					num2 += num14;
					num17 -= num12;
				}
				if (num18 > 0)
				{
					num4 += num16;
					num18 -= num12;
				}
				num17 += num11;
				num18 += num13;
				num3 += num15;
			}
		}
		else
		{
			int num17 = num12 - num10;
			int num18 = num11 - num10;
			for (int k = 0; k < num10; k++)
			{
				if (!AddToGridArea(new int3(num2, num3, num4), in gridDef, nStep, ref neighboursChecked, useNeighbourThresholds, neighbourThreshold) && ++num > blockedGridThreshold)
				{
					return true;
				}
				if (num17 > 0)
				{
					num3 += num15;
					num17 -= num13;
				}
				if (num18 > 0)
				{
					num2 += num14;
					num18 -= num13;
				}
				num17 += num12;
				num18 += num11;
				num4 += num16;
			}
		}
		return false;
	}

	public static bool Gather(int3 from, int3 to, in GridDefinition gridDef, int blockedGridThreshold, int neighbourThreshold, bool useNeighbourThresholds, NativeList<(int3, Color)> cells)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0233: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a2: Unknown result type (might be due to invalid IL or missing references)
		int num = 0;
		int neighboursChecked = 0;
		int num2 = from.x;
		int num3 = from.y;
		int num4 = from.z;
		int x = to.x;
		int y = to.y;
		int z = to.z;
		int num5 = x - from.x;
		int num6 = y - from.y;
		int num7 = z - from.z;
		int num8 = Mathf.Abs(num5);
		int num9 = Mathf.Abs(num6);
		int num10 = Mathf.Abs(num7);
		int num11 = num8 << 1;
		int num12 = num9 << 1;
		int num13 = num10 << 1;
		int num14 = ((num5 >= 0) ? 1 : (-1));
		int num15 = ((num6 >= 0) ? 1 : (-1));
		int num16 = ((num7 >= 0) ? 1 : (-1));
		int3 nStep = -math.int3(num14, num15, num16);
		if (num8 >= num9 && num8 >= num10)
		{
			int num17 = num12 - num8;
			int num18 = num13 - num8;
			int3 val = default(int3);
			for (int i = 0; i < num8; i++)
			{
				((int3)(ref val))._002Ector(num2, num3, num4);
				if (!AddToGridArea(val, in gridDef, nStep, ref neighboursChecked, useNeighbourThresholds, neighbourThreshold, cells))
				{
					(int3, Color) tuple = (val, Color.red);
					cells.Add(ref tuple);
					if (++num > blockedGridThreshold)
					{
						return true;
					}
				}
				if (num17 > 0)
				{
					num3 += num15;
					num17 -= num11;
				}
				if (num18 > 0)
				{
					num4 += num16;
					num18 -= num11;
				}
				num17 += num12;
				num18 += num13;
				num2 += num14;
			}
		}
		else if (num9 >= num8 && num9 >= num10)
		{
			int num17 = num11 - num9;
			int num18 = num13 - num9;
			int3 val2 = default(int3);
			for (int j = 0; j < num9; j++)
			{
				((int3)(ref val2))._002Ector(num2, num3, num4);
				if (!AddToGridArea(val2, in gridDef, nStep, ref neighboursChecked, useNeighbourThresholds, neighbourThreshold, cells))
				{
					(int3, Color) tuple = (val2, Color.red);
					cells.Add(ref tuple);
					if (++num > blockedGridThreshold)
					{
						return true;
					}
				}
				if (num17 > 0)
				{
					num2 += num14;
					num17 -= num12;
				}
				if (num18 > 0)
				{
					num4 += num16;
					num18 -= num12;
				}
				num17 += num11;
				num18 += num13;
				num3 += num15;
			}
		}
		else
		{
			int num17 = num12 - num10;
			int num18 = num11 - num10;
			int3 val3 = default(int3);
			for (int k = 0; k < num10; k++)
			{
				((int3)(ref val3))._002Ector(num2, num3, num4);
				if (!AddToGridArea(val3, in gridDef, nStep, ref neighboursChecked, useNeighbourThresholds, neighbourThreshold, cells))
				{
					(int3, Color) tuple = (val3, Color.red);
					cells.Add(ref tuple);
					if (++num > blockedGridThreshold)
					{
						return true;
					}
				}
				if (num17 > 0)
				{
					num3 += num15;
					num17 -= num13;
				}
				if (num18 > 0)
				{
					num2 += num14;
					num18 -= num13;
				}
				num17 += num12;
				num18 += num11;
				num4 += num16;
			}
		}
		return false;
	}

	private static bool NeighbourBlockedOneAxis(in GridDefinition grid, int3 cell, int xDir, int yDir, int zDir)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_02db: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_030b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0310: Unknown result type (might be due to invalid IL or missing references)
		//IL_0318: Unknown result type (might be due to invalid IL or missing references)
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_017c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0197: Unknown result type (might be due to invalid IL or missing references)
		//IL_019d: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0336: Unknown result type (might be due to invalid IL or missing references)
		//IL_033e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0344: Unknown result type (might be due to invalid IL or missing references)
		//IL_034d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0352: Unknown result type (might be due to invalid IL or missing references)
		//IL_035a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0360: Unknown result type (might be due to invalid IL or missing references)
		//IL_0366: Unknown result type (might be due to invalid IL or missing references)
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0373: Unknown result type (might be due to invalid IL or missing references)
		//IL_0379: Unknown result type (might be due to invalid IL or missing references)
		//IL_0382: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0209: Unknown result type (might be due to invalid IL or missing references)
		//IL_0211: Unknown result type (might be due to invalid IL or missing references)
		//IL_0217: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0103: Unknown result type (might be due to invalid IL or missing references)
		//IL_0391: Unknown result type (might be due to invalid IL or missing references)
		//IL_0397: Unknown result type (might be due to invalid IL or missing references)
		//IL_039f: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		//IL_022c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_023c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0112: Unknown result type (might be due to invalid IL or missing references)
		//IL_011a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0133: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015c: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0403: Unknown result type (might be due to invalid IL or missing references)
		//IL_0408: Unknown result type (might be due to invalid IL or missing references)
		//IL_040e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0416: Unknown result type (might be due to invalid IL or missing references)
		//IL_041c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0421: Unknown result type (might be due to invalid IL or missing references)
		//IL_0427: Unknown result type (might be due to invalid IL or missing references)
		//IL_042f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0438: Unknown result type (might be due to invalid IL or missing references)
		//IL_027f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0295: Unknown result type (might be due to invalid IL or missing references)
		//IL_029a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		if (xDir != 0)
		{
			if (!AreBlocked(in grid, new int3(cell.x - xDir, cell.y, cell.z + 1), new int3(cell.x, cell.y, cell.z + 1), new int3(cell.x + xDir, cell.y, cell.z + 1)))
			{
				return false;
			}
			if (!AreBlocked(in grid, new int3(cell.x - xDir, cell.y, cell.z - 1), new int3(cell.x, cell.y, cell.z - 1), new int3(cell.x + xDir, cell.y, cell.z - 1)))
			{
				return false;
			}
			if (!AreBlocked(in grid, new int3(cell.x - xDir, cell.y + 1, cell.z), new int3(cell.x, cell.y + 1, cell.z), new int3(cell.x + xDir, cell.y + 1, cell.z)))
			{
				return false;
			}
			if (!AreBlocked(in grid, new int3(cell.x - xDir, cell.y - 1, cell.z), new int3(cell.x, cell.y - 1, cell.z), new int3(cell.x + xDir, cell.y - 1, cell.z)))
			{
				return false;
			}
		}
		else if (yDir != 0)
		{
			if (!AreBlocked(in grid, new int3(cell.x - 1, cell.y - yDir, cell.z), new int3(cell.x - 1, cell.y, cell.z), new int3(cell.x - 1, cell.y + yDir, cell.z)))
			{
				return false;
			}
			if (!AreBlocked(in grid, new int3(cell.x + 1, cell.y - yDir, cell.z), new int3(cell.x + 1, cell.y, cell.z), new int3(cell.x + 1, cell.y + yDir, cell.z)))
			{
				return false;
			}
			if (!AreBlocked(in grid, new int3(cell.x, cell.y - yDir, cell.z - 1), new int3(cell.x, cell.y, cell.z - 1), new int3(cell.x, cell.y + yDir, cell.z - 1)))
			{
				return false;
			}
			if (!AreBlocked(in grid, new int3(cell.x, cell.y - yDir, cell.z + 1), new int3(cell.x, cell.y, cell.z + 1), new int3(cell.x, cell.y + yDir, cell.z + 1)))
			{
				return false;
			}
		}
		else
		{
			if (!AreBlocked(in grid, new int3(cell.x - 1, cell.y, cell.z - zDir), new int3(cell.x - 1, cell.y, cell.z), new int3(cell.x - 1, cell.y, cell.z + zDir)))
			{
				return false;
			}
			if (!AreBlocked(in grid, new int3(cell.x + 1, cell.y, cell.z - zDir), new int3(cell.x + 1, cell.y, cell.z), new int3(cell.x + 1, cell.y, cell.z + zDir)))
			{
				return false;
			}
			if (!AreBlocked(in grid, new int3(cell.x, cell.y - 1, cell.z - zDir), new int3(cell.x, cell.y - 1, cell.z), new int3(cell.x, cell.y - 1, cell.z + zDir)))
			{
				return false;
			}
			if (!AreBlocked(in grid, new int3(cell.x, cell.y + 1, cell.z - zDir), new int3(cell.x, cell.y + 1, cell.z), new int3(cell.x, cell.y + 1, cell.z + zDir)))
			{
				return false;
			}
		}
		return true;
	}

	private static bool NeighbourBlockedTwoAxis(in GridDefinition grid, int3 cell, int xDir, int yDir, int zDir)
	{
		//IL_02f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0304: Unknown result type (might be due to invalid IL or missing references)
		//IL_030d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Unknown result type (might be due to invalid IL or missing references)
		//IL_0318: Unknown result type (might be due to invalid IL or missing references)
		//IL_0320: Unknown result type (might be due to invalid IL or missing references)
		//IL_0329: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_033c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0342: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_0351: Unknown result type (might be due to invalid IL or missing references)
		//IL_0359: Unknown result type (might be due to invalid IL or missing references)
		//IL_0361: Unknown result type (might be due to invalid IL or missing references)
		//IL_036a: Unknown result type (might be due to invalid IL or missing references)
		//IL_036f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0377: Unknown result type (might be due to invalid IL or missing references)
		//IL_037d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0383: Unknown result type (might be due to invalid IL or missing references)
		//IL_0388: Unknown result type (might be due to invalid IL or missing references)
		//IL_0390: Unknown result type (might be due to invalid IL or missing references)
		//IL_0398: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_018d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0195: Unknown result type (might be due to invalid IL or missing references)
		//IL_019b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03be: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0201: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_020d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_022a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Unknown result type (might be due to invalid IL or missing references)
		//IL_010a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0409: Unknown result type (might be due to invalid IL or missing references)
		//IL_0411: Unknown result type (might be due to invalid IL or missing references)
		//IL_0419: Unknown result type (might be due to invalid IL or missing references)
		//IL_0422: Unknown result type (might be due to invalid IL or missing references)
		//IL_0427: Unknown result type (might be due to invalid IL or missing references)
		//IL_042f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0435: Unknown result type (might be due to invalid IL or missing references)
		//IL_043b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0440: Unknown result type (might be due to invalid IL or missing references)
		//IL_0448: Unknown result type (might be due to invalid IL or missing references)
		//IL_0450: Unknown result type (might be due to invalid IL or missing references)
		//IL_0459: Unknown result type (might be due to invalid IL or missing references)
		//IL_0241: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_024f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_025a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0270: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_027d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0283: Unknown result type (might be due to invalid IL or missing references)
		//IL_0289: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_013d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Unknown result type (might be due to invalid IL or missing references)
		//IL_0156: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0298: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02de: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		if (xDir != 0 && zDir != 0)
		{
			if (!AreBlocked(in grid, new int3(cell.x - xDir, cell.y, cell.z), new int3(cell.x - xDir, cell.y, cell.z + zDir), new int3(cell.x, cell.y, cell.z + zDir)))
			{
				return false;
			}
			if (!AreBlocked(in grid, new int3(cell.x - xDir, cell.y + 1, cell.z - zDir), new int3(cell.x, cell.y + 1, cell.z), new int3(cell.x + xDir, cell.y + 1, cell.z + zDir)))
			{
				return false;
			}
			if (!AreBlocked(in grid, new int3(cell.x, cell.y, cell.z - zDir), new int3(cell.x + xDir, cell.y, cell.z - zDir), new int3(cell.x + xDir, cell.y, cell.z)))
			{
				return false;
			}
			if (!AreBlocked(in grid, new int3(cell.x - xDir, cell.y - 1, cell.z - zDir), new int3(cell.x, cell.y - 1, cell.z), new int3(cell.x + xDir, cell.y - 1, cell.z + zDir)))
			{
				return false;
			}
		}
		else if (xDir != 0 && yDir != 0)
		{
			if (!AreBlocked(in grid, new int3(cell.x - xDir, cell.y, cell.z), new int3(cell.x - xDir, cell.y + yDir, cell.z), new int3(cell.x, cell.y + yDir, cell.z)))
			{
				return false;
			}
			if (!AreBlocked(in grid, new int3(cell.x - xDir, cell.y - yDir, cell.z + 1), new int3(cell.x, cell.y, cell.z + 1), new int3(cell.x + xDir, cell.y + yDir, cell.z + 1)))
			{
				return false;
			}
			if (!AreBlocked(in grid, new int3(cell.x, cell.y - yDir, cell.z), new int3(cell.x + xDir, cell.y - yDir, cell.z), new int3(cell.x + xDir, cell.y, cell.z)))
			{
				return false;
			}
			if (!AreBlocked(in grid, new int3(cell.x - xDir, cell.y - yDir, cell.z - 1), new int3(cell.x, cell.y, cell.z - 1), new int3(cell.x + xDir, cell.y + yDir, cell.z - 1)))
			{
				return false;
			}
		}
		else
		{
			if (!AreBlocked(in grid, new int3(cell.x, cell.y, cell.z - zDir), new int3(cell.x, cell.y + yDir, cell.z - zDir), new int3(cell.x, cell.y + yDir, cell.z)))
			{
				return false;
			}
			if (!AreBlocked(in grid, new int3(cell.x + 1, cell.y - yDir, cell.z - zDir), new int3(cell.x + 1, cell.y, cell.z), new int3(cell.x + 1, cell.y + yDir, cell.z - zDir)))
			{
				return false;
			}
			if (!AreBlocked(in grid, new int3(cell.x, cell.y - yDir, cell.z), new int3(cell.x, cell.y - yDir, cell.z + zDir), new int3(cell.x, cell.y, cell.z + zDir)))
			{
				return false;
			}
			if (!AreBlocked(in grid, new int3(cell.x - 1, cell.y - yDir, cell.z - zDir), new int3(cell.x - 1, cell.y, cell.z), new int3(cell.x - 1, cell.y + yDir, cell.z - zDir)))
			{
				return false;
			}
		}
		return true;
	}

	private static bool NeighbourBlockedThreeAxis(in GridDefinition grid, int3 cell, int xDir, int yDir, int zDir)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0101: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0131: Unknown result type (might be due to invalid IL or missing references)
		//IL_0139: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		//IL_0147: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		if (!AreBlocked(in grid, new int3(cell.x - xDir, cell.y, cell.z), new int3(cell.x - xDir, cell.y, cell.z + zDir), new int3(cell.x, cell.y, cell.z + zDir)))
		{
			return false;
		}
		if (!AreBlocked(in grid, new int3(cell.x, cell.y, cell.z - zDir), new int3(cell.x + xDir, cell.y, cell.z - zDir), new int3(cell.x + xDir, cell.y, cell.z)))
		{
			return false;
		}
		if (!AreBlocked(in grid, new int3(cell.x - xDir, cell.y, cell.z - zDir), new int3(cell.x - xDir, cell.y + yDir, cell.z - zDir), new int3(cell.x, cell.y + yDir, cell.z)))
		{
			return false;
		}
		if (!AreBlocked(in grid, new int3(cell.x, cell.y - yDir, cell.z), new int3(cell.x + xDir, cell.y - yDir, cell.z + zDir), new int3(cell.x + xDir, cell.y, cell.z + zDir)))
		{
			return false;
		}
		return true;
	}

	private static bool AreBlocked(in GridDefinition grid, int3 p1, int3 p2, int3 p3)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		if (grid.IsValidSubGrid(p1) && grid.IsBlocked(p1))
		{
			return true;
		}
		if (grid.IsValidSubGrid(p2) && grid.IsBlocked(p2))
		{
			return true;
		}
		if (grid.IsValidSubGrid(p3) && grid.IsBlocked(p3))
		{
			return true;
		}
		return false;
	}

	private static bool AddNeighbours(int3 cell, in GridDefinition grid, int3 nStep)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		return (((nStep.x != 0) ? 1 : 0) + ((nStep.y != 0) ? 1 : 0) + ((nStep.z != 0) ? 1 : 0)) switch
		{
			1 => !NeighbourBlockedOneAxis(in grid, cell, -nStep.x, -nStep.y, -nStep.z), 
			2 => !NeighbourBlockedTwoAxis(in grid, cell, -nStep.x, -nStep.y, -nStep.z), 
			3 => !NeighbourBlockedThreeAxis(in grid, cell, -nStep.x, -nStep.y, -nStep.z), 
			_ => true, 
		};
	}

	private static bool AddToGridArea(int3 cell, in GridDefinition grid, int3 nStep, ref int neighboursChecked, bool useNeighbourThresholds, int neighbourThreshold)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		if (!grid.IsBlocked(cell))
		{
			return true;
		}
		if (!useNeighbourThresholds || ++neighboursChecked <= neighbourThreshold)
		{
			return AddNeighbours(cell, in grid, nStep);
		}
		return false;
	}

	private static bool AddToGridArea(int3 cell, in GridDefinition grid, int3 nStep, ref int neighboursChecked, bool useNeighbourThresholds, int neighbourThreshold, NativeList<(int3, Color)> cells)
	{
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if (!grid.IsBlocked(cell))
		{
			(int3, Color) tuple = (cell, Color.green);
			cells.Add(ref tuple);
			return true;
		}
		if ((!useNeighbourThresholds || ++neighboursChecked <= neighbourThreshold) && AddNeighbours(cell, in grid, nStep))
		{
			(int3, Color) tuple = (cell, Color.yellow);
			cells.Add(ref tuple);
			return true;
		}
		return false;
	}
}
