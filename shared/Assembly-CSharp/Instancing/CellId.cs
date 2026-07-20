namespace Instancing;

public struct CellId(int index)
{
	public int Index = index;

	public override string ToString()
	{
		return Index.ToString();
	}
}
