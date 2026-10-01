using UnityEngine;
using UnityEngine.UI;

public class IconCharm : MonoBehaviour, IClientComponent
{
	public Image Icon;

	public GameObject ClearRoot;

	public Tooltip tooltip;

	public GameObject currentlySelectedRoot;

	public static Phrase clearPhrase = new Phrase("charms.clear", "Clear");

	static IconCharm()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected Obj, but got Unknown
	}
}
