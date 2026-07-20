using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class ConsoleUI : SingletonComponent<ConsoleUI>
{
	public RustText text;

	public InputField outputField;

	public InputField inputField;

	public GameObject AutocompleteDropDown;

	public GameObject ItemTemplate;

	public Color errorColor;

	public Color warningColor;

	public Color inputColor;

	[Header("Multi-line")]
	public bool enableMultiline;

	public RectTransform inputRect;

	public RectTransform outputRect;

	public float textHeight;

	public int maxLines = 8;

	public float defaultHeight;
}
