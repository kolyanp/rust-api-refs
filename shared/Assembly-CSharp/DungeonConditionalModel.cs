using UnityEngine;

public class DungeonConditionalModel : MonoBehaviour
{
	public MapLayer Layer;

	private void Start()
	{
		foreach (Transform child in TransformEx.GetChildren(((Component)this).transform))
		{
			((Component)child).gameObject.SetActive(World.Config != null && !World.Config.BelowGroundRails);
		}
	}
}
