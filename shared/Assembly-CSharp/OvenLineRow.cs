using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OvenLineRow : MonoBehaviour
{
	public LootGrid Above;

	public LootGrid Below;

	public Transform Container;

	public Color Color = Color.white;

	public Sprite TriangleSprite;

	public int LineWidth = 2;

	public int ArrowWidth = 6;

	public int ArrowHeight = 4;

	public int Padding = 2;

	private int _topCount;

	private int _bottomCount;

	private List<GameObject> images = new List<GameObject>();

	private void Update()
	{
		LootGrid above = Above;
		int num = ((above != null) ? ((Component)above).transform.childCount : 0);
		LootGrid below = Below;
		int num2 = ((below != null) ? ((Component)below).transform.childCount : 0);
		if (num2 == _bottomCount && num == _topCount)
		{
			return;
		}
		_bottomCount = num2;
		_topCount = num;
		foreach (GameObject image in images)
		{
			Object.Destroy((Object)(object)image);
		}
		CreateRow(above: true);
		CreateRow(above: false);
	}

	private void CreateRow(bool above)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0161: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_019c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_023a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_0292: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_030c: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_012e: Unknown result type (might be due to invalid IL or missing references)
		LootGrid lootGrid = (above ? Above : Below);
		int num = (above ? _topCount : _bottomCount);
		if (num == 0)
		{
			return;
		}
		int num2 = num;
		GridLayoutGroup component = ((Component)lootGrid).GetComponent<GridLayoutGroup>();
		float x = component.cellSize.x;
		float x2 = component.spacing.x;
		float num3 = x + x2;
		float num4 = num3 * (float)(num - 1) / 2f;
		if (above)
		{
			for (int i = 0; i < num; i++)
			{
				if (i == 0 || i == num - 1)
				{
					Image val = CreateImage();
					((Graphic)val).rectTransform.anchorMin = new Vector2(0.5f, above ? 0.5f : 0f);
					((Graphic)val).rectTransform.anchorMax = new Vector2(0.5f, above ? 1f : 0.5f);
					((Graphic)val).rectTransform.offsetMin = new Vector2(0f - num4 + (float)i * num3 - (float)(LineWidth / 2), (float)(above ? (LineWidth / 2) : Padding));
					((Graphic)val).rectTransform.offsetMax = new Vector2(0f - num4 + (float)i * num3 + (float)(LineWidth / 2), (float)(above ? (-Padding) : (-LineWidth / 2)));
				}
			}
		}
		else
		{
			Image val2 = CreateImage();
			((Graphic)val2).rectTransform.anchorMin = new Vector2(0.5f, 0f);
			((Graphic)val2).rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
			((Graphic)val2).rectTransform.offsetMin = new Vector2((float)(-LineWidth / 2), (float)Padding);
			((Graphic)val2).rectTransform.offsetMax = new Vector2((float)(LineWidth / 2), (float)(-LineWidth / 2));
			Image val3 = CreateImage();
			val3.sprite = TriangleSprite;
			((Object)((Component)val3).gameObject).name = "triangle";
			val3.useSpriteMesh = true;
			((Transform)((Graphic)val3).rectTransform).localRotation = Quaternion.Euler(0f, 0f, 180f);
			((Graphic)val3).rectTransform.anchorMin = new Vector2(0.5f, 0f);
			((Graphic)val3).rectTransform.anchorMax = new Vector2(0.5f, 0f);
			((Graphic)val3).rectTransform.pivot = new Vector2(0.5f, 0f);
			((Graphic)val3).rectTransform.offsetMin = new Vector2((float)(-ArrowWidth / 2), 0f);
			((Graphic)val3).rectTransform.offsetMax = new Vector2((float)(ArrowWidth / 2), (float)ArrowHeight);
		}
		if (above && num2 >= 1)
		{
			float num5 = num3 * (float)(num2 - 1) + (float)LineWidth;
			Image val4 = CreateImage();
			((Graphic)val4).rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
			((Graphic)val4).rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
			((Graphic)val4).rectTransform.offsetMin = new Vector2(num5 / -2f, (float)(-LineWidth / 2));
			((Graphic)val4).rectTransform.offsetMax = new Vector2(num5 / 2f, (float)(LineWidth / 2));
		}
	}

	private Image CreateImage()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Expected Obj, but got Unknown
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		GameObject val = new GameObject("Line");
		Image val2 = val.AddComponent<Image>();
		images.Add(val);
		((Transform)((Graphic)val2).rectTransform).SetParent(Container ?? ((Component)this).transform);
		((Component)val2).transform.localScale = Vector3.one;
		((Graphic)val2).color = Color;
		return val2;
	}

	public OvenLineRow()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
	}
}
