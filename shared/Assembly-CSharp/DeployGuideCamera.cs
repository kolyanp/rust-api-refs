using UnityEngine;

[RequireComponent(typeof(Camera))]
[RequireComponent(typeof(CommandBufferManager))]
public class DeployGuideCamera : SingletonComponent<DeployGuideCamera>
{
	public DeployGuideRendererResources Resources;

	private DeployGuideRendererInternalResources _internResources = new DeployGuideRendererInternalResources();
}
