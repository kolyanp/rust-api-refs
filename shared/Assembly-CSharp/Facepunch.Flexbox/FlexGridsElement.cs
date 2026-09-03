using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace Facepunch.Flexbox;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
[DisallowMultipleComponent]
public class FlexGridsElement : FlexElementBase
{
	private struct GridSlot
	{
		public IFlexNode Child;

		public int X;

		public int Y;

		public int SpanX;

		public int SpanY;
	}

	[Tooltip("Spacing to add from this elements borders to where children are laid out.")]
	public FlexPadding Padding;

	[Tooltip("Spacing to add between each child flex item.")]
	[Min(0f)]
	public float Gap;

	[Tooltip("The number of columns to use when using a fixed number of columns.")]
	[Min(1f)]
	public int ColumnCount = 1;

	[FormerlySerializedAs("ColumnWidth")]
	[Min(1f)]
	[Tooltip("The minimum width of each column when not using a fixed number of columns.")]
	public int ColumnMinWidth = 100;

	public bool FixedRowCount;

	[Min(1f)]
	public int RowCount = 1;

	[Min(1f)]
	public int RowMinHeight = 100;

	public bool BestFitOrdering;

	private bool[,] _gridMapBuffer = new bool[32, 32];

	private int _gridMapColumns = 32;

	private int _gridMapRows = 32;

	public bool[,] GridMapBuffer => _gridMapBuffer;

	public void EnsureGridMapSize(int columns, int rows)
	{
		if (_gridMapBuffer == null || _gridMapColumns < columns || _gridMapRows < rows)
		{
			_gridMapBuffer = new bool[columns, rows];
			_gridMapColumns = columns;
			_gridMapRows = rows;
		}
		else
		{
			Array.Clear(_gridMapBuffer, 0, _gridMapBuffer.Length);
		}
	}

	protected override void MeasureHorizontalImpl()
	{
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		float num = 0f;
		int num2 = Mathf.Min(ColumnCount, Mathf.Max(1, base.Children.Count));
		num = (float)(num2 * ColumnMinWidth) + (float)Mathf.Max(num2 - 1, 0) * Gap;
		foreach (IFlexNode child in base.Children)
		{
			if (child.IsDirty)
			{
				child.MeasureHorizontal();
			}
		}
		float num3 = ((base.Basis.HasValue && (int)base.Basis.Unit == 0) ? base.Basis.Value : 0f);
		float num4 = ((base.MinWidth.HasValue && (int)base.MinWidth.Unit == 0) ? base.MinWidth.Value : 0f);
		float num5 = ((base.MaxWidth.HasValue && (int)base.MaxWidth.Unit == 0) ? base.MaxWidth.Value : float.PositiveInfinity);
		float num6 = Padding.left + Padding.right;
		base.PrefWidth = Mathf.Clamp(num + num6, Mathf.Max(num4, num3), num5);
	}

	protected override void LayoutHorizontalImpl(float maxWidth, float maxHeight)
	{
		//IL_00c4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b6: Unknown result type (might be due to invalid IL or missing references)
		float num = maxWidth - Padding.left - Padding.right;
		float num2 = maxHeight - Padding.top - Padding.bottom;
		float num3 = (num - Gap * (float)(ColumnCount - 1)) / (float)ColumnCount;
		float num4 = RowMinHeight;
		if (BestFitOrdering)
		{
			ReOrderChildren(num, num3);
		}
		int num5 = (FixedRowCount ? RowCount : 100);
		EnsureGridMapSize(ColumnCount, num5);
		Array.Clear(_gridMapBuffer, 0, _gridMapBuffer.Length);
		List<GridSlot> list = Pool.Get<List<GridSlot>>();
		foreach (IFlexNode child in base.Children)
		{
			FlexLength minWidth = child.MinWidth;
			int num6 = Mathf.Clamp(Mathf.CeilToInt(FlexElementBase.CalculateLengthValue(ref minWidth, num, 0f) / (num3 + Gap)), 1, ColumnCount);
			minWidth = child.MinHeight;
			int num7 = Mathf.Clamp(Mathf.CeilToInt(FlexElementBase.CalculateLengthValue(ref minWidth, num2, 0f) / (num4 + Gap)), 1, num5);
			bool flag = false;
			for (int i = 0; i < num5; i++)
			{
				for (int j = 0; j <= ColumnCount - num6; j++)
				{
					if (CanPlaceAt(j, i, num6, num7, _gridMapBuffer))
					{
						MarkUsed(j, i, num6, num7, _gridMapBuffer);
						list.Add(new GridSlot
						{
							Child = child,
							X = j,
							Y = i,
							SpanX = num6,
							SpanY = num7
						});
						flag = true;
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
		}
		foreach (GridSlot item in list)
		{
			RectTransform transform = item.Child.Transform;
			float num8 = Padding.left + (float)item.X * (num3 + Gap);
			float num9 = Padding.top + (float)item.Y * (num4 + Gap);
			float num10 = (float)item.SpanX * num3 + (float)(item.SpanX - 1) * Gap;
			float num11 = (float)item.SpanY * num4 + (float)(item.SpanY - 1) * Gap;
			item.Child.LayoutHorizontal(num10, float.PositiveInfinity);
			item.Child.LayoutVertical(float.PositiveInfinity, num11);
			transform.anchoredPosition = new Vector2(num8, 0f - num9);
			transform.sizeDelta = new Vector2(num10, num11);
		}
		Pool.FreeUnmanaged<GridSlot>(ref list);
	}

	public static bool CanPlaceAt(int xPos, int yPos, int sizeX, int sizeY, bool[,] grid)
	{
		for (int i = 0; i < sizeY; i++)
		{
			for (int j = 0; j < sizeX; j++)
			{
				if (grid[xPos + j, yPos + i])
				{
					return false;
				}
			}
		}
		return true;
	}

	public static void MarkUsed(int xPos, int yPos, int sizeX, int sizeY, bool[,] grid)
	{
		for (int i = 0; i < sizeY; i++)
		{
			for (int j = 0; j < sizeX; j++)
			{
				grid[xPos + j, yPos + i] = true;
			}
		}
	}

	private void ReOrderChildren(float innerWidth, float columnWidth)
	{
		List<GridSlot> list = base.Children.Select(delegate(IFlexNode child)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			FlexLength minWidth = child.MinWidth;
			float num4 = FlexElementBase.CalculateLengthValue(ref minWidth, innerWidth, 0f);
			minWidth = child.MaxWidth;
			float num5 = FlexElementBase.CalculateLengthValue(ref minWidth, innerWidth, float.PositiveInfinity);
			float num6 = Mathf.Clamp(columnWidth, num4, num5);
			minWidth = child.MinHeight;
			float num7 = FlexElementBase.CalculateLengthValue(ref minWidth, float.PositiveInfinity, 0f);
			float num8 = Mathf.Clamp((float)RowMinHeight, num7, float.PositiveInfinity);
			int spanX = Mathf.Clamp(Mathf.CeilToInt((num6 + Gap) / (columnWidth + Gap)), 1, ColumnCount);
			int spanY = Mathf.Clamp(Mathf.CeilToInt((num8 + Gap) / ((float)RowMinHeight + Gap)), 1, 64);
			return new GridSlot
			{
				Child = child,
				SpanX = spanX,
				SpanY = spanY
			};
		}).ToList();
		list.Sort(delegate(GridSlot a, GridSlot b)
		{
			int value = a.SpanX * a.SpanY;
			return (b.SpanX * b.SpanY).CompareTo(value);
		});
		List<IFlexNode> list2 = Pool.Get<List<IFlexNode>>();
		while (list.Count > 0)
		{
			int num = ColumnCount;
			int num2 = 0;
			while (num2 < list.Count)
			{
				if (list[num2].SpanX <= num)
				{
					list2.Add(list[num2].Child);
					num -= list[num2].SpanX;
					list.RemoveAt(num2);
				}
				else
				{
					num2++;
				}
			}
		}
		for (int num3 = 0; num3 < list2.Count; num3++)
		{
			((Transform)list2[num3].Transform).SetSiblingIndex(num3);
		}
		Pool.FreeUnmanaged<IFlexNode>(ref list2);
	}

	protected override void MeasureVerticalImpl()
	{
		//IL_01c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0221: Unknown result type (might be due to invalid IL or missing references)
		float num;
		if (FixedRowCount)
		{
			num = (float)(RowCount * RowMinHeight) + (float)Mathf.Max(RowCount - 1, 0) * Gap;
		}
		else
		{
			int[] array = new int[ColumnCount];
			int num2 = 0;
			foreach (IFlexNode child in base.Children)
			{
				if (child.IsDirty)
				{
					child.MeasureVertical();
				}
				FlexLength minWidth = child.MinWidth;
				float num3 = FlexElementBase.CalculateLengthValue(ref minWidth, 0f, 0f);
				minWidth = child.MinHeight;
				float num4 = FlexElementBase.CalculateLengthValue(ref minWidth, 0f, 0f);
				int num5 = Mathf.Clamp(Mathf.CeilToInt((num3 + Gap) / ((float)ColumnMinWidth + Gap)), 1, ColumnCount);
				int num6 = Mathf.Clamp(Mathf.CeilToInt((num4 + Gap) / ((float)RowMinHeight + Gap)), 1, 64);
				int num7 = -1;
				int num8 = int.MaxValue;
				for (int i = 0; i <= ColumnCount - num5; i++)
				{
					int num9 = 0;
					for (int j = 0; j < num5; j++)
					{
						num9 = Mathf.Max(num9, array[i + j]);
					}
					if (num9 < num8)
					{
						num8 = num9;
						num7 = i;
					}
				}
				if (num7 >= 0)
				{
					for (int k = 0; k < num5; k++)
					{
						array[num7 + k] = num8 + num6;
					}
					num2 = Mathf.Max(num2, num8 + num6);
				}
			}
			num = (float)(num2 * RowMinHeight) + (float)Mathf.Max(num2 - 1, 0) * Gap;
		}
		float num10 = ((base.Basis.HasValue && (int)base.Basis.Unit == 0) ? base.Basis.Value : 0f);
		float num11 = ((base.MinHeight.HasValue && (int)base.MinHeight.Unit == 0) ? base.MinHeight.Value : 0f);
		float num12 = ((base.MaxHeight.HasValue && (int)base.MaxHeight.Unit == 0) ? base.MaxHeight.Value : float.PositiveInfinity);
		float num13 = Padding.top + Padding.bottom;
		base.PrefHeight = Mathf.Clamp(num + num13, Mathf.Max(num11, num10), num12);
	}

	protected override void LayoutVerticalImpl(float maxWidth, float maxHeight)
	{
	}
}
