using System;
using System.Runtime.CompilerServices;
using UnityEngine.Rendering;

public class CommandBufferDesc
{
	public delegate void FillCommandBuffer(CommandBuffer cb);

	public string Name;

	public CameraEvent CameraEvent
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return field;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			field = value;
		}
	}

	public int OrderId { get; private set; }

	public Action<CommandBuffer> FillDelegate { get; private set; }

	public CommandBufferDesc(CameraEvent cameraEvent, int orderId, FillCommandBuffer fill, string name = "")
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		CameraEvent = cameraEvent;
		OrderId = orderId;
		FillDelegate = fill.Invoke;
		Name = name;
	}
}
