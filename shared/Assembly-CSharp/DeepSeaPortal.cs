using Facepunch;
using Facepunch.Extend;
using ProtoBuf;
using UnityEngine;

public class DeepSeaPortal : BaseEntity
{
	public enum PortalModeEnum
	{
		None,
		Entrance,
		Exit
	}

	public MeshRenderer DebugRenderer;

	public GameObjectRef BuoyPrefab;

	public PortalModeEnum PortalMode;

	public CardinalDirection PortalDirection;

	public override void InitShared()
	{
		base.InitShared();
		if (base.isServer)
		{
			DeepSeaManager.ServerPortals.Add(this);
		}
	}

	public override void DestroyShared()
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		base.DestroyShared();
		if (base.isServer)
		{
			DeepSeaManager.ServerPortals.Remove(this);
			if (PortalMode == PortalModeEnum.Entrance)
			{
				Debug.LogError((object)string.Format("DeepSea: Portal destroyed! Mode={0}, Direction={1}, Position={2}\n{3}", new object[4]
				{
					PortalMode,
					PortalDirection,
					((Component)this).transform.position,
					StackTraceUtility.ExtractStackTrace()
				}));
			}
		}
		if (PortalMode == PortalModeEnum.Entrance)
		{
			if (HasFlag(Flags.Open))
			{
				DeepSeaManager.PortalEntranceBounds = default(OBB);
				DeepSeaManager.PortalEntranceTransform = null;
			}
		}
		else if (PortalMode == PortalModeEnum.Exit)
		{
			DeepSeaManager.PortalExitBounds = default(OBB);
			DeepSeaManager.PortalExitTransform = null;
		}
	}

	public void InitBounds()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (PortalMode == PortalModeEnum.Entrance)
		{
			if (HasFlag(Flags.Open))
			{
				DeepSeaManager.PortalEntranceBounds = WorldSpaceBounds();
				DeepSeaManager.PortalEntranceTransform = ((Component)this).transform;
			}
		}
		else if (PortalMode == PortalModeEnum.Exit)
		{
			DeepSeaManager.PortalExitBounds = WorldSpaceBounds();
			DeepSeaManager.PortalExitTransform = ((Component)this).transform;
		}
	}

	public override void OnFlagsChanged(Flags old, Flags next)
	{
		base.OnFlagsChanged(old, next);
		if (PortalMode == PortalModeEnum.Entrance)
		{
			bool num = (old & Flags.Open) == Flags.Open;
			bool flag = (next & Flags.Open) == Flags.Open;
			if (num != flag)
			{
				InitBounds();
			}
		}
	}

	public override void ServerInit()
	{
		base.ServerInit();
		InitBounds();
	}

	public override void Save(SaveInfo info)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		info.msg.deepSeaPortal = Pool.Get<DeepSeaPortal>();
		Vector3 triggerSize = ((!BaseNetworkable.UseParallelSaves) ? ((Component)this).transform.localScale : Facepunch.Extend.TransformEx.Unsafe.GetLocalScaleMT(base.TransformHandle));
		info.msg.deepSeaPortal.triggerSize = triggerSize;
		info.msg.deepSeaPortal.portalMode = (int)PortalMode;
	}

	public override void Load(LoadInfo info)
	{
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		base.Load(info);
		if (info.msg.deepSeaPortal != null)
		{
			((Component)this).transform.localScale = info.msg.deepSeaPortal.triggerSize;
			PortalMode = (PortalModeEnum)info.msg.deepSeaPortal.portalMode;
			InitBounds();
		}
	}
}
