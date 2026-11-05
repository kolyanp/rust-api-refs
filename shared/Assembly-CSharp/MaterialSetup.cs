using Rust;

public class MaterialSetup : ProceduralObject, IClientComponent, IFlattenKeepsChildren
{
	public MaterialConfig config;

	public bool IncludeChildren;

	protected void OnEnable()
	{
		if (!Application.isLoading)
		{
			Setup();
		}
	}

	public override void Process()
	{
		Setup();
	}

	private void Setup()
	{
	}
}
