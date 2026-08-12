using UnityEngine;

public class MeshPaintable : BaseMeshPaintable
{
	public string replacementTextureName;

	public int textureWidth;

	public int textureHeight;

	public Color clearColor;

	public Texture2D targetTexture;

	public bool hasChanges;

	public MeshPaintable()
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		replacementTextureName = "_MainTex";
		textureWidth = 256;
		textureHeight = 256;
		clearColor = Color.clear;
		base._002Ector();
	}
}
