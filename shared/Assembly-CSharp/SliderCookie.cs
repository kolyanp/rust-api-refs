using System.Globalization;
using Facepunch;
using Rust;
using Rust.UI;
using UnityEngine;
using UnityEngine.Events;

public class SliderCookie : MonoBehaviour
{
	public string MaxValueConvarName;

	private ConsoleSystem.Command Command;

	public void OnEnable()
	{
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		RustSlider val = default(RustSlider);
		if (!((Component)this).TryGetComponent<RustSlider>(ref val))
		{
			return;
		}
		if (!string.IsNullOrEmpty(MaxValueConvarName))
		{
			Command = ConsoleSystem.Index.Client.Find(StringView.op_Implicit(MaxValueConvarName));
			if (Command != null)
			{
				val.SetMaxValue(Command.AsFloat);
			}
		}
		float result;
		float num = (float.TryParse(PlayerPrefs.GetString("SliderCookie_" + ((Object)this).name), NumberStyles.Float, CultureInfo.InvariantCulture, out result) ? result : val.ValueInternal);
		val.ValueInternal = num + 1f;
		val.Value = num;
		((UnityEvent<float>)(object)val.OnChanged).AddListener((UnityAction<float>)OnSliderChanged);
	}

	public void OnDisable()
	{
		RustSlider val = default(RustSlider);
		if (!Application.isQuitting && ((Component)this).TryGetComponent<RustSlider>(ref val))
		{
			((UnityEvent<float>)(object)val.OnChanged).RemoveListener((UnityAction<float>)OnSliderChanged);
		}
	}

	[UnityEvent]
	private void OnSliderChanged(float v)
	{
		PlayerPrefs.SetString("SliderCookie_" + ((Object)this).name, v.ToString(CultureInfo.InvariantCulture));
	}
}
