using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class ImageAlphaRaycastFilter : UIBehaviour, ICanvasRaycastFilter
{
	[NonSerialized]
	private RawImage m_rawImage;

	public float rChannelHitTestMinimumThreshold = 1f;

	protected RawImage rawImage => m_rawImage ?? (m_rawImage = ((Component)this).GetComponent<RawImage>());

	public virtual bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
	{
		//IL_0101: Expected O, but got Unknown
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00db: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		if (rChannelHitTestMinimumThreshold <= 0f)
		{
			return true;
		}
		if (rChannelHitTestMinimumThreshold > 1f)
		{
			return false;
		}
		Texture mainTexture = ((Graphic)rawImage).mainTexture;
		Texture2D val = (Texture2D)(object)((mainTexture is Texture2D) ? mainTexture : null);
		Vector2 val2 = default(Vector2);
		if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(((Graphic)rawImage).rectTransform, screenPoint, eventCamera, ref val2))
		{
			return false;
		}
		Rect pixelAdjustedRect = ((Graphic)rawImage).GetPixelAdjustedRect();
		val2.x += ((Graphic)rawImage).rectTransform.pivot.x * ((Rect)(ref pixelAdjustedRect)).width;
		val2.y += ((Graphic)rawImage).rectTransform.pivot.y * ((Rect)(ref pixelAdjustedRect)).height;
		((Vector2)(ref val2))._002Ector(val2.x / ((Rect)(ref pixelAdjustedRect)).width, val2.y / ((Rect)(ref pixelAdjustedRect)).height);
		if ((Object)(object)val != (Object)null && !((Texture)val).isReadable)
		{
			return false;
		}
		try
		{
			return val.GetPixelBilinear(val2.x, val2.y).r <= rChannelHitTestMinimumThreshold;
		}
		catch (UnityException ex)
		{
			UnityException ex2 = ex;
			Debug.LogError((object)("Using alphaHitTestMinimumThreshold greater than 0 on Graphic whose sprite texture cannot be read. " + ((Exception)(object)ex2).Message + " Also make sure to disable sprite packing for this sprite."), (Object)(object)this);
			return true;
		}
	}
}
