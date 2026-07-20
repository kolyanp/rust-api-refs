using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[ExecuteAlways]
[DisallowMultipleComponent]
public class UIBorder : MonoBehaviour
{
	private const string ChildName = "_UIBorder";

	[SerializeField]
	private float top;

	[SerializeField]
	private float right;

	[SerializeField]
	private float bottom;

	[SerializeField]
	private float left;

	[SerializeField]
	private Color color = Color.white;

	[SerializeField]
	private float topLeftRadius;

	[SerializeField]
	private float topRightRadius;

	[SerializeField]
	private float bottomRightRadius;

	[SerializeField]
	private float bottomLeftRadius;

	[SerializeField]
	[Range(1f, 32f)]
	private int segmentsPerCorner = 8;

	[HideInInspector]
	[SerializeField]
	private BorderGraphic graphic;

	public float Top
	{
		get
		{
			return top;
		}
		set
		{
			if (top != value)
			{
				top = value;
				Sync();
			}
		}
	}

	public float Right
	{
		get
		{
			return right;
		}
		set
		{
			if (right != value)
			{
				right = value;
				Sync();
			}
		}
	}

	public float Bottom
	{
		get
		{
			return bottom;
		}
		set
		{
			if (bottom != value)
			{
				bottom = value;
				Sync();
			}
		}
	}

	public float Left
	{
		get
		{
			return left;
		}
		set
		{
			if (left != value)
			{
				left = value;
				Sync();
			}
		}
	}

	public Color Color
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return color;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			if (color != value)
			{
				color = value;
				Sync();
			}
		}
	}

	public float TopLeftRadius
	{
		get
		{
			return topLeftRadius;
		}
		set
		{
			if (topLeftRadius != value)
			{
				topLeftRadius = value;
				Sync();
			}
		}
	}

	public float TopRightRadius
	{
		get
		{
			return topRightRadius;
		}
		set
		{
			if (topRightRadius != value)
			{
				topRightRadius = value;
				Sync();
			}
		}
	}

	public float BottomRightRadius
	{
		get
		{
			return bottomRightRadius;
		}
		set
		{
			if (bottomRightRadius != value)
			{
				bottomRightRadius = value;
				Sync();
			}
		}
	}

	public float BottomLeftRadius
	{
		get
		{
			return bottomLeftRadius;
		}
		set
		{
			if (bottomLeftRadius != value)
			{
				bottomLeftRadius = value;
				Sync();
			}
		}
	}

	public int SegmentsPerCorner
	{
		get
		{
			return segmentsPerCorner;
		}
		set
		{
			if (segmentsPerCorner != value)
			{
				segmentsPerCorner = value;
				Sync();
			}
		}
	}

	private void OnEnable()
	{
		EnsureGraphic();
		Sync();
	}

	private void OnTransformChildrenChanged()
	{
		if ((Object)(object)graphic != (Object)null)
		{
			((Component)graphic).transform.SetAsLastSibling();
		}
	}

	private void EnsureGraphic()
	{
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cd: Expected O, but got Unknown
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)graphic == (Object)null || (Object)(object)((Component)graphic).transform.parent != (Object)(object)((Component)this).transform)
		{
			Transform val = null;
			for (int i = 0; i < ((Component)this).transform.childCount; i++)
			{
				Transform child = ((Component)this).transform.GetChild(i);
				if (((Object)child).name == "_UIBorder")
				{
					val = child;
					break;
				}
			}
			if ((Object)(object)val != (Object)null)
			{
				graphic = ((Component)val).GetComponent<BorderGraphic>();
				if ((Object)(object)graphic == (Object)null)
				{
					graphic = ((Component)val).gameObject.AddComponent<BorderGraphic>();
				}
			}
			else
			{
				GameObject val2 = new GameObject("_UIBorder", new Type[2]
				{
					typeof(RectTransform),
					typeof(BorderGraphic)
				});
				RectTransform val3 = (RectTransform)val2.transform;
				((Transform)val3).SetParent(((Component)this).transform, false);
				val3.anchorMin = Vector2.zero;
				val3.anchorMax = Vector2.one;
				val3.offsetMin = Vector2.zero;
				val3.offsetMax = Vector2.zero;
				graphic = val2.GetComponent<BorderGraphic>();
			}
		}
		((Object)((Component)graphic).gameObject).hideFlags = (HideFlags)1;
		((Graphic)graphic).raycastTarget = false;
		((Component)graphic).transform.SetAsLastSibling();
	}

	private void Sync()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)graphic == (Object)null))
		{
			graphic.SetSides(top, right, bottom, left, color);
			graphic.SetCorners(topLeftRadius, topRightRadius, bottomRightRadius, bottomLeftRadius, segmentsPerCorner);
		}
	}
}
