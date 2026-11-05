using UnityEngine.EventSystems;

public class PaintableImageGrid : UIBehaviour, IServerFileReceiver
{
	public GameObjectRef templateImage;

	public int cols = 4;

	public int rows = 4;

	public bool readOnly;

	public BasePlayer.FogMode FogMode = BasePlayer.FogMode.Mainland;

	public int ImageNumberOffset => BasePlayer.FogImagePartOffset(FogMode);
}
