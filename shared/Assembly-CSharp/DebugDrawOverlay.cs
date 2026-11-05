public abstract class DebugDrawOverlay
{
	public abstract string Name { get; }

	public virtual int FirstColour => 0;

	public abstract void Build(BasePlayer viewer, DebugDrawOverlayFrame frame);
}
