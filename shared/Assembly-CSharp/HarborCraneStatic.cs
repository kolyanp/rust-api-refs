using Facepunch;
using Facepunch.Extend;
using ProtoBuf;
using UnityEngine;

public class HarborCraneStatic : HarborCrane
{
	public float StartingDepth;

	public float StartingHeight;

	public float StartingAngle;

	public Transform HangingLadder;

	private TransformHandle craneGrabHandle;

	private TransformHandle armRootHandle;

	public override void PostMapEntitySpawn()
	{
		base.PostMapEntitySpawn();
		SetArmPos(StartingAngle, StartingHeight, StartingDepth);
	}

	public override void ServerInit()
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		base.ServerInit();
		craneGrabHandle = ((Component)CraneGrab).transformHandle;
		armRootHandle = ((Component)ArmRoot).transformHandle;
	}

	public override void Save(SaveInfo info)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		base.Save(info);
		info.msg.harborCrane = Pool.Get<HarborCrane>();
		Vector3 val;
		Quaternion val2;
		if (BaseNetworkable.UseParallelSaves)
		{
			val = Facepunch.Extend.TransformEx.Unsafe.GetLocalPosMT(in craneGrabHandle);
			val2 = Facepunch.Extend.TransformEx.Unsafe.GetLocalRotMT(in armRootHandle);
		}
		else
		{
			val = CraneGrab.localPosition;
			val2 = ArmRoot.localRotation;
		}
		info.msg.harborCrane.depth = val.x;
		info.msg.harborCrane.height = val.y;
		info.msg.harborCrane.yaw = ((Quaternion)(ref val2)).eulerAngles.z;
	}

	private void SetArmPos(float angle, float height, float depth)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		ArmRoot.localEulerAngles = new Vector3(0f, 0f, angle);
		CraneGrab.localPosition = new Vector3(depth, height, 0f);
		HangingLadder.rotation = Quaternion.LookRotation(((Component)this).transform.right, Vector3.up);
		UpdateArmSupports(((Component)this).transform.forward);
	}

	public override void Load(LoadInfo info)
	{
		base.Load(info);
		if (info.msg.harborCrane != null)
		{
			SetArmPos(info.msg.harborCrane.yaw, info.msg.harborCrane.height, info.msg.harborCrane.depth);
		}
	}
}
