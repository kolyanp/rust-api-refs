using ConVar;
using Facepunch;
using ProtoBuf;
using Rust;
using UnityEngine;

public class HittableByTrains : EntityComponent<BaseCombatEntity>, ITrainCollidable, TrainTrackSpline.ITrainTrackUser, IOnParentDestroying, IOnParentSpawning
{
	[SerializeField]
	private float trainDamagePerMPS = 10f;

	[SerializeField]
	private float barricadeDamageMultiplier = 0.002f;

	[SerializeField]
	private float minVelToDestroy = 6f;

	[SerializeField]
	private float velReduction = 10f;

	[SerializeField]
	private GameObjectRef barricadeDamageEffect;

	private TrainCar hitTrain;

	private TriggerTrainCollisions hitTrainTrigger;

	private TrainTrackSpline track;

	public Vector3 Position
	{
		get
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			return ((Component)this).transform.position;
		}
	}

	public float FrontWheelSplineDist { get; private set; }

	public TrainCar.TrainCarType CarType => TrainCar.TrainCarType.Other;

	public bool CustomCollision(TrainCar train, TriggerTrainCollisions trainTrigger)
	{
		bool result = false;
		if (baseEntity.isServer)
		{
			float num = Mathf.Abs(train.GetTrackSpeed());
			SetHitTrain(train, trainTrigger);
			if (num < minVelToDestroy && !vehicle.cinematictrains)
			{
				InvokeRandomized(PushForceTick, 0f, 0.25f, 0.025f);
			}
			else
			{
				result = true;
				Invoke(DestroyThisBarrier, 0f);
			}
		}
		return result;
	}

	public bool EqualNetID(BaseNetworkable other)
	{
		return baseEntity.EqualNetID(other);
	}

	public Vector3 GetWorldVelocity()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return baseEntity.GetWorldVelocity();
	}

	public void OnParentSpawning()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		if (TrainTrackSpline.TryFindTrackNear(((Component)this).transform.position, 5f, out var splineResult, out var distResult))
		{
			track = splineResult;
			FrontWheelSplineDist = distResult;
			track.RegisterTrackUser(this);
		}
	}

	public void OnParentDestroying()
	{
		if ((Object)(object)track != (Object)null)
		{
			track.DeregisterTrackUser(this);
		}
	}

	private void SetHitTrain(TrainCar train, TriggerTrainCollisions trainTrigger)
	{
		hitTrain = train;
		hitTrainTrigger = trainTrigger;
	}

	private void ClearHitTrain()
	{
		SetHitTrain(null, null);
	}

	private void DestroyThisBarrier()
	{
		if (baseEntity.IsDead() || baseEntity.IsDestroyed)
		{
			return;
		}
		if ((Object)(object)hitTrain != (Object)null)
		{
			hitTrain.completeTrain.ReduceSpeedBy(velReduction);
			if (vehicle.cinematictrains)
			{
				hitTrain.Hurt(9999f, DamageType.Collision, baseEntity, useProtection: false);
			}
			else
			{
				float amount = Mathf.Abs(hitTrain.GetTrackSpeed()) * trainDamagePerMPS;
				hitTrain.Hurt(amount, DamageType.Collision, baseEntity, useProtection: false);
			}
		}
		ClearHitTrain();
		baseEntity.Kill(BaseNetworkable.DestroyMode.Gib);
	}

	private void PushForceTick()
	{
		//IL_0076: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)hitTrain == (Object)null || (Object)(object)hitTrainTrigger == (Object)null || hitTrain.IsDead() || hitTrain.IsDestroyed || baseEntity.IsDead() || !hitTrain.IsOn())
		{
			ClearHitTrain();
			CancelInvoke(PushForceTick);
			return;
		}
		bool flag = true;
		Bounds bounds = hitTrainTrigger.triggerCollider.bounds;
		if (!bounds.Intersects(baseEntity.bounds))
		{
			Vector3 val = ((hitTrainTrigger.location != TriggerTrainCollisions.Location.Front) ? hitTrainTrigger.owner.GetRearOfTrainPos() : hitTrainTrigger.owner.GetFrontOfTrainPos());
			flag = Vector3.SqrMagnitude(((Component)this).transform.position + baseEntity.bounds.ClosestPoint(val - ((Component)this).transform.position) - val) < 2f;
		}
		if (flag)
		{
			float num = hitTrainTrigger.owner.completeTrain.TotalForces;
			if (hitTrainTrigger.location == TriggerTrainCollisions.Location.Rear)
			{
				num *= -1f;
			}
			num = Mathf.Max(0f, num);
			baseEntity.Hurt(num * barricadeDamageMultiplier, DamageType.Generic, hitTrain);
			if (baseEntity.IsDead())
			{
				hitTrain.completeTrain.FreeStaticCollision();
			}
		}
		else
		{
			ClearHitTrain();
			CancelInvoke(PushForceTick);
		}
	}

	public override void InitShared()
	{
		base.InitShared();
		UpdateBaseEntity();
	}

	public override void SaveComponent(BaseNetworkable.SaveInfo info)
	{
		bool flag = baseEntity.lastAttacker is TrainCar;
		info.msg.simpleInt = Pool.Get<SimpleInt>();
		info.msg.simpleInt.value = (flag ? 1 : 0);
	}
}
