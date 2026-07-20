using Rust.UI;
using TMPro;
using UnityEngine;

public class CopyText : MonoBehaviour
{
	public RustText TargetText;

	[UnityEvent]
	public void TriggerCopy()
	{
		if ((Object)(object)TargetText != (Object)null)
		{
			GUIUtility.systemCopyBuffer = ((TMP_Text)TargetText).text;
		}
	}
}
