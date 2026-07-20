using UnityEngine;

[CreateAssetMenu(menuName = "Rust/DefaultViewmodelClearValuesConfig")]
public class DefaultViewmodelClearValuesConfig : BaseScriptableObject
{
	private static DefaultViewmodelClearValuesConfig _instance;

	public LeanTweenType defaultCameraResetEaseType = LeanTweenType.linear;

	public static DefaultViewmodelClearValuesConfig instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FileSystem.Load<DefaultViewmodelClearValuesConfig>("assets/scripts/viewmodel/viewmodelcamera/config/defaultviewmodelclearvaluesconfig.asset", true);
			}
			if (_instance == null)
			{
				Debug.LogError((object)"Failed to load DefaultViewmodelClearValuesConfig");
			}
			return _instance;
		}
	}
}
