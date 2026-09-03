using System;
using System.Runtime.CompilerServices;
using UnityEngine.Rendering;

public class CommandBufferDesc
{
	public delegate void FillCommandBuffer(CommandBuffer cb);

	[CompilerGenerated]
	private CameraEvent _003CCameraEvent_003Ek__BackingField;

	public string Name;

	public CameraEvent CameraEvent
	{
		[CompilerGenerated]
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _003CCameraEvent_003Ek__BackingField;
		}
		[CompilerGenerated]
		private set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			_003CCameraEvent_003Ek__BackingField = value;
		}
	}

	public int OrderId { get; private set; }

	public Action<CommandBuffer> FillDelegate { get; private set; }

	public CommandBufferDesc(CameraEvent cameraEvent, int orderId, FillCommandBuffer fill, string name = "")
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		base._002Ector();
		CameraEvent = cameraEvent;
		OrderId = orderId;
		FillDelegate = fill.Invoke;
		Name = name;
	}
}
