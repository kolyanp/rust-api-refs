using Rust.UI;
using UnityEngine;

public class UI_ToggleLayer : MonoBehaviour, IClientComponent
{
	public RustButton toggleControl;

	public RustText layerNameText;

	public LayerSelect layer;

	protected void OnEnable()
	{
		if (Object.op_Implicit((Object)(object)MainCamera.mainCamera))
		{
			toggleControl.Value = (MainCamera.mainCamera.cullingMask & layer.Mask) != 0;
		}
	}

	[UnityEvent]
	public void OnToggleChanged()
	{
		if (Object.op_Implicit((Object)(object)MainCamera.mainCamera))
		{
			ConsoleSystem.Run(ConsoleSystem.Option.Client, toggleControl.Value ? "layer.show" : "layer.hide", layer.Name);
		}
	}
}
