using Rust.RenderPipeline.Runtime;
using UnityEngine;

public class RenderPipelineSwitchNode : MonoBehaviour
{
	public GameObject RustRenderPipelineObject;

	public GameObject BuiltinPipelineObject;

	private void OnAwake()
	{
		DoSwitch();
	}

	private void DoSwitch()
	{
		if (RustRenderPipeline.IsActive())
		{
			BuiltinPipelineObject.SetActive(false);
			RustRenderPipelineObject.SetActive(true);
		}
		else
		{
			RustRenderPipelineObject.SetActive(false);
			BuiltinPipelineObject.SetActive(true);
		}
	}
}
