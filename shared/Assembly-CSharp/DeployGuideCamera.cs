using UnityEngine;

[RequireComponent(typeof(CommandBufferManager))]
[RequireComponent(typeof(Camera))]
public class DeployGuideCamera : SingletonComponent<DeployGuideCamera>
{
	public DeployGuideMaterial GoodMaterial;

	public DeployGuideMaterial BadMaterial;

	public DeployGuideMaterial NeutralMaterial;
}
