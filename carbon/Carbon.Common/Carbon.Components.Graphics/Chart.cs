using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using Carbon.Base;
using Carbon.Extensions;
using UnityEngine;

namespace Carbon.Components.Graphics;

public struct Chart
{
	public struct ChartSettings
	{
		public bool VerticalLabels;

		public bool HorizontalLabels;

		public Pen GridColor;
	}

	public struct ChartRect
	{
		public float Width;

		public float Height;

		public float X;

		public float Y;
	}

	public class Layer
	{
		public string Name;

		public ulong[] Data;

		public bool Disabled;

		public LayerSettings LayerSettings;

		public void ToggleDisabled()
		{
			Disabled = !Disabled;
		}
	}

	public class LayerSettings
	{
		public Color Color;

		public int Shadows;
	}

	public class ProcessingThread : BaseThreadedJob
	{
		public Chart Chart;

		public Exception Exception;

		public override void ThreadFunction()
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001c: Expected O, but got Unknown
			try
			{
				Bitmap val = new Bitmap(Chart.width, Chart.height);
				try
				{
					Graphics val2 = Graphics.FromImage((Image)(object)val);
					try
					{
						val2.Clear(Chart.background);
						val2.SmoothingMode = (SmoothingMode)4;
						val2.CompositingQuality = (CompositingQuality)2;
						val2.PageUnit = (GraphicsUnit)1;
						val2.TextRenderingHint = (TextRenderingHint)4;
						val2.InterpolationMode = (InterpolationMode)6;
						Chart.DrawChart(val2, Chart.Layers, Chart.verticalLabels, Chart.horizontalLabels);
						using MemoryStream memoryStream = new MemoryStream();
						((Image)val).Save((Stream)memoryStream, ImageFormat.Png);
						Chart.image = memoryStream.ToArray();
					}
					finally
					{
						((IDisposable)val2)?.Dispose();
					}
				}
				finally
				{
					((IDisposable)val)?.Dispose();
				}
			}
			catch (Exception exception)
			{
				Logger.Error("Chart processing failed! Report to developers", Exception = exception);
			}
			base.ThreadFunction();
		}

		public override void OnFinished()
		{
			Chart.onProcessEnded?.Invoke(Chart.image, Exception);
			base.OnFinished();
		}
	}

	public string Name;

	public ChartSettings Settings;

	public ChartRect Rect;

	public Layer[] Layers;

	internal int width;

	internal int height;

	internal Color background;

	internal string[] verticalLabels;

	internal string[] horizontalLabels;

	internal Brush textColor;

	internal Graphics graphic;

	internal Action<byte[], Exception> onProcessEnded;

	internal byte[] image;

	public static Chart Create(string name, int width, int height, ChartSettings settings, ChartRect rect, Layer[] layers, string[] verticalLabels, string[] horizontalLabels, Brush textColor, Color background)
	{
		return new Chart
		{
			Name = name,
			Settings = settings,
			Rect = rect,
			Layers = layers,
			verticalLabels = verticalLabels,
			horizontalLabels = horizontalLabels,
			width = width,
			height = height,
			background = background,
			textColor = textColor
		};
	}

	public void StartProcess(Action<byte[], Exception> onProcessEnded = null)
	{
		this.onProcessEnded = onProcessEnded;
		ProcessingThread processingThread = new ProcessingThread();
		processingThread.Chart = this;
		processingThread.Start();
		((MonoBehaviour)Community.Runtime.Core.persistence).StartCoroutine(processingThread.WaitFor());
	}

	internal void DrawChart(Graphics graphic, IEnumerable<Layer> layers, string[] verticalLabels, string[] horizontalLabels)
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Expected O, but got Unknown
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Expected O, but got Unknown
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Expected O, but got Unknown
		if (Settings.GridColor == null)
		{
			Settings.GridColor = Pens.DimGray;
		}
		string[] array = (from i in Enumerable.Range(0, verticalLabels.Length)
			select verticalLabels[i]).ToArray();
		StringFormat val = new StringFormat
		{
			LineAlignment = (StringAlignment)1,
			Alignment = (StringAlignment)2
		};
		StringFormat val2 = new StringFormat
		{
			LineAlignment = (StringAlignment)1,
			Alignment = (StringAlignment)1
		};
		Font val3 = new Font("Arial", 15f);
		if (Settings.HorizontalLabels)
		{
			for (int num = 0; num < horizontalLabels.Length; num++)
			{
				float num2 = Rect.X + (float)num * (Rect.Width / (float)(horizontalLabels.Length - 1));
				float num3 = Rect.Y + Rect.Height + 5f;
				graphic.DrawString(horizontalLabels[num], val3, textColor, num2, num3 + 15f, val2);
			}
		}
		if (Settings.VerticalLabels)
		{
			for (int num4 = 0; num4 < array.Length; num4++)
			{
				float num5 = Rect.X - 15f;
				float num6 = Rect.Y + Rect.Height - (float)num4 * (Rect.Height / (float)(array.Length - 1));
				graphic.DrawString(array[num4], val3, textColor, num5, num6, val);
			}
		}
		for (int num7 = 0; num7 < array.Length; num7++)
		{
			float num8 = Rect.Y + Rect.Height - (float)num7 * (Rect.Height / (float)(array.Length - 1));
			graphic.DrawLine(Pens.DimGray, Rect.X, num8, Rect.X + Rect.Width, num8);
		}
		for (int num9 = 0; num9 < horizontalLabels.Length; num9++)
		{
			float num10 = Rect.X + (float)num9 * (Rect.Width / (float)(horizontalLabels.Length - 1));
			graphic.DrawLine(Pens.DimGray, num10, Rect.Y, num10, Rect.Y + Rect.Height);
		}
		graphic.DrawLine(Pens.DimGray, Rect.X, Rect.Y, Rect.X, Rect.Y + Rect.Height);
		graphic.DrawLine(Pens.DimGray, Rect.X, Rect.Y + Rect.Height, Rect.X + Rect.Width, Rect.Y + Rect.Height);
		foreach (Layer layer in layers)
		{
			if (!layer.Disabled)
			{
				DrawChartContentShadows(graphic, layer.Data, Rect.Width, Rect.Height, Rect.X, Rect.Y, layer.LayerSettings);
			}
		}
		foreach (Layer layer2 in layers)
		{
			if (!layer2.Disabled)
			{
				DrawChartContentLineDots(graphic, layer2.Data, Rect.Width, Rect.Height, Rect.X, Rect.Y, layer2.LayerSettings);
			}
		}
	}

	internal void DrawChartContentShadows(Graphics graphic, ulong[] data, float chartWidth, float chartHeight, float chartX, float chartY, LayerSettings layerSettings)
	{
		ulong num = data.Max();
		float num2 = chartWidth / (float)(data.Length - 1);
		for (int i = 0; i < data.Length; i++)
		{
			bool flag = i >= data.Length - 1;
			float x = chartX + num2 * (float)i;
			float y = chartY + chartHeight - (float)data[i] * (chartHeight / (float)num);
			float nextX = (flag ? x : (chartX + num2 * (float)(i + 1)));
			float nextY = (flag ? y : (chartY + chartHeight - (float)data[i + 1] * (chartHeight / (float)num)));
			for (float num3 = 1f; num3 < (float)layerSettings.Shadows; num3++)
			{
				CreateShadow(num3.Scale(0f, layerSettings.Shadows, 1f, 0.75f), (int)num3.Scale(0f, layerSettings.Shadows, 25f, 0f));
			}
			if (layerSettings.Shadows > 0)
			{
				CreateShadow(1f, (int)((float)(int)layerSettings.Color.A * 0.2f));
			}
			void CreateShadow(float multiply, int alpha)
			{
				//IL_0092: Unknown result type (might be due to invalid IL or missing references)
				//IL_009d: Expected O, but got Unknown
				PointF[] array = new PointF[4]
				{
					new PointF(x, y * multiply),
					new PointF(nextX, nextY * multiply),
					new PointF(nextX, chartY + chartHeight),
					new PointF(x, chartY + chartHeight)
				};
				Color color = Color.FromArgb(alpha, layerSettings.Color);
				graphic.FillPolygon((Brush)new SolidBrush(color), array);
			}
		}
	}

	internal void DrawChartContentLineDots(Graphics graphic, ulong[] data, float chartWidth, float chartHeight, float chartX, float chartY, LayerSettings layerSettings)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Expected O, but got Unknown
		ulong num = data.Max();
		float num2 = chartWidth / (float)(data.Length - 1);
		Pen val = new Pen(layerSettings.Color, 2f);
		SolidBrush val2 = new SolidBrush(layerSettings.Color);
		for (int i = 0; i < data.Length; i++)
		{
			bool flag = i >= data.Length - 1;
			float num3 = chartX + num2 * (float)i;
			float num4 = chartY + chartHeight - (float)data[i] * (chartHeight / (float)num);
			float num5 = (flag ? num3 : (chartX + num2 * (float)(i + 1)));
			float num6 = (flag ? num4 : (chartY + chartHeight - (float)data[i + 1] * (chartHeight / (float)num)));
			graphic.DrawLine(val, num3, num4, num5, num6);
			graphic.FillEllipse((Brush)(object)val2, num3 - 7.5f, num4 - 7.5f, 15f, 15f);
		}
	}
}
