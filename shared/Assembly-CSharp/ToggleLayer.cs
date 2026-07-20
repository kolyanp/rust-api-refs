using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToggleLayer : MonoBehaviour, IClientComponent
{
	public Toggle toggleControl;

	public TextMeshProUGUI textControl;

	public LayerSelect layer;

	protected void OnEnable()
	{
		if (Object.op_Implicit((Object)(object)MainCamera.mainCamera))
		{
			toggleControl.isOn = (MainCamera.mainCamera.cullingMask & layer.Mask) != 0;
		}
	}

	[UnityEvent]
	public void OnToggleChanged()
	{
		if (Object.op_Implicit((Object)(object)MainCamera.mainCamera))
		{
			ConsoleSystem.Run(ConsoleSystem.Option.Client, toggleControl.isOn ? "layer.show" : "layer.hide", layer.Name);
		}
	}

	protected void OnValidate()
	{
		if (Object.op_Implicit((Object)(object)textControl))
		{
			((TMP_Text)textControl).text = layer.Name;
		}
	}
}
