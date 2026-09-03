using UnityEngine;

public class AIInformationGrid : MonoBehaviour
{
	public int CellSize = 10;

	public Bounds BoundingBox;

	public AIInformationCell[] Cells;

	private Vector3 origin;

	private int xCellCount;

	private int zCellCount;

	private const int maxPointResults = 2048;

	private AIMovePoint[] movePointResults = new AIMovePoint[2048];

	private AICoverPoint[] coverPointResults = new AICoverPoint[2048];

	private const int maxCellResults = 512;

	private AIInformationCell[] resultCells = new AIInformationCell[512];

	[ContextMenu("Init")]
	public void Init()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0126: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0129: Unknown result type (might be due to invalid IL or missing references)
		//IL_0136: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_0186: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		AIInformationZone component = ((Component)this).GetComponent<AIInformationZone>();
		if ((Object)(object)component == (Object)null)
		{
			Debug.LogWarning((object)"Unable to Init AIInformationGrid, no AIInformationZone found!");
			return;
		}
		BoundingBox = component.bounds;
		((Bounds)(ref BoundingBox)).center = ((Component)this).transform.position + ((Bounds)(ref component.bounds)).center + new Vector3(0f, ((Bounds)(ref BoundingBox)).extents.y, 0f);
		AIPoint[] componentsInChildren = ((Component)this).GetComponentsInChildren<AIPoint>(true);
		foreach (AIPoint aIPoint in componentsInChildren)
		{
			((Bounds)(ref BoundingBox)).Encapsulate(((Component)aIPoint).transform.position);
		}
		float num = ((Bounds)(ref BoundingBox)).extents.x * 2f;
		float num2 = ((Bounds)(ref BoundingBox)).extents.z * 2f;
		xCellCount = (int)Mathf.Ceil(num / (float)CellSize);
		zCellCount = (int)Mathf.Ceil(num2 / (float)CellSize);
		Cells = new AIInformationCell[xCellCount * zCellCount];
		Vector3 val = (origin = ((Bounds)(ref BoundingBox)).min);
		val.x = ((Bounds)(ref BoundingBox)).min.x + (float)CellSize / 2f;
		val.z = ((Bounds)(ref BoundingBox)).min.z + (float)CellSize / 2f;
		Bounds bounds = default(Bounds);
		for (int j = 0; j < zCellCount; j++)
		{
			for (int k = 0; k < xCellCount; k++)
			{
				Vector3 val2 = val;
				((Bounds)(ref bounds))._002Ector(val2, new Vector3((float)CellSize, ((Bounds)(ref BoundingBox)).extents.y * 2f, (float)CellSize));
				Cells[GetIndex(k, j)] = new AIInformationCell(bounds, ((Component)this).gameObject, k, j);
				val.x += CellSize;
			}
			val.x = ((Bounds)(ref BoundingBox)).min.x + (float)CellSize / 2f;
			val.z += CellSize;
		}
	}

	private int GetIndex(int x, int z)
	{
		return z * xCellCount + x;
	}

	public AIInformationCell CellAt(int x, int z)
	{
		return Cells[GetIndex(x, z)];
	}

	public AIMovePoint[] GetMovePointsInRange(Vector3 position, float maxRange, out int pointCount)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		pointCount = 0;
		AIInformationCell[] cellsInRange = GetCellsInRange(position, maxRange, out var cellCount);
		if (cellCount > 0)
		{
			for (int i = 0; i < cellCount; i++)
			{
				if (cellsInRange[i] == null)
				{
					continue;
				}
				foreach (AIMovePoint item in cellsInRange[i].MovePoints.Items)
				{
					movePointResults[pointCount] = item;
					pointCount++;
				}
			}
		}
		return movePointResults;
	}

	public AICoverPoint[] GetCoverPointsInRange(Vector3 position, float maxRange, out int pointCount)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		pointCount = 0;
		AIInformationCell[] cellsInRange = GetCellsInRange(position, maxRange, out var cellCount);
		if (cellCount > 0)
		{
			for (int i = 0; i < cellCount; i++)
			{
				if (cellsInRange[i] == null)
				{
					continue;
				}
				foreach (AICoverPoint item in cellsInRange[i].CoverPoints.Items)
				{
					coverPointResults[pointCount] = item;
					pointCount++;
				}
			}
		}
		return coverPointResults;
	}

	public AIInformationCell[] GetCellsInRange(Vector3 position, float maxRange, out int cellCount)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		cellCount = 0;
		int num = (int)(maxRange / (float)CellSize);
		AIInformationCell cell = GetCell(position);
		if (cell == null)
		{
			cell = GetCell(ClampToGrid(position));
		}
		if (cell == null)
		{
			return resultCells;
		}
		int num2 = Mathf.Max(cell.X - num, 0);
		int num3 = Mathf.Min(cell.X + num, xCellCount - 1);
		int num4 = Mathf.Max(cell.Z - num, 0);
		int num5 = Mathf.Min(cell.Z + num, zCellCount - 1);
		for (int i = num4; i <= num5; i++)
		{
			for (int j = num2; j <= num3; j++)
			{
				resultCells[cellCount] = CellAt(j, i);
				cellCount++;
				if (cellCount >= 512)
				{
					return resultCells;
				}
			}
		}
		return resultCells;
	}

	private Vector3 ClampToGrid(Vector3 position)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		position.x = Mathf.Clamp(position.x, origin.x, origin.x + (float)(xCellCount * CellSize) - 0.01f);
		position.z = Mathf.Clamp(position.z, origin.z, origin.z + (float)(zCellCount * CellSize) - 0.01f);
		return position;
	}

	public AIInformationCell GetCell(Vector3 position)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		if (Cells == null)
		{
			return null;
		}
		Vector3 val = position - origin;
		if (val.x < 0f || val.z < 0f)
		{
			return null;
		}
		int num = (int)(val.x / (float)CellSize);
		int num2 = (int)(val.z / (float)CellSize);
		if (num < 0 || num >= xCellCount)
		{
			return null;
		}
		if (num2 < 0 || num2 >= zCellCount)
		{
			return null;
		}
		return CellAt(num, num2);
	}

	public void OnDrawGizmosSelected()
	{
		DebugDraw();
	}

	public void DebugDraw()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (Cells != null)
		{
			AIInformationCell[] cells = Cells;
			for (int i = 0; i < cells.Length; i++)
			{
				cells[i]?.DebugDraw(Color.white, points: false);
			}
		}
	}
}
