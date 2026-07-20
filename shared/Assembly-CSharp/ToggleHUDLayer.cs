using Facepunch.Extend;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToggleHUDLayer : MonoBehaviour, IClientComponent
{
	public Toggle toggleControl;

	public TextMeshProUGUI textControl;

	public string hudComponentName;

	protected void OnEnable()
	{
		UIHUD instance = SingletonComponent<UIHUD>.Instance;
		if ((Object)(object)instance != (Object)null)
		{
			Transform val = ((Component)instance).transform.FindChildRecursive(hudComponentName);
			if ((Object)(object)val != (Object)null)
			{
				toggleControl.isOn = ((Component)val).gameObject.activeSelf;
			}
			else
			{
				Debug.LogWarning((object)(((object)this).GetType().Name + ": Couldn't find child: " + hudComponentName));
			}
		}
	}

	[UnityEvent]
	public void OnToggleChanged()
	{
		ConsoleSystem.Run(ConsoleSystem.Option.Client, "global.hudcomponent", hudComponentName, toggleControl.isOn);
	}
}
