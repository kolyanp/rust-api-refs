using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Facepunch;
using Oxide.Core;
using Rust;
using UnityEngine;

public class TriggerBase : BaseMonoBehaviour
{
	[SerializeField]
	private LayerMask interestLayers;

	[NonSerialized]
	public HashSet<GameObject> contents;

	[NonSerialized]
	public HashSet<BaseEntity> entityContents;

	public Action<BaseNetworkable> OnEntityEnterTrigger;

	public Action<BaseNetworkable> OnEntityLeaveTrigger;

	private static bool _useExcludeLayers;

	private static readonly List<TriggerBase> _allTriggerBase = new List<TriggerBase>();

	public LayerMask InterestLayers
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return interestLayers;
		}
		set
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			interestLayers = value;
			UpdateExcludeLayers();
		}
	}

	public bool HasAnyContents => !CollectionEx.IsNullOrEmpty(contents);

	public bool HasAnyEntityContents => !CollectionEx.IsNullOrEmpty(entityContents);

	[ServerVar(Help = "(Generated) When enabled, triggers use an exclude layer mask to filter out specific physics layers from trigger detection; toggling clears or sets all active triggers")]
	[ClientVar(Help = "(Generated) When enabled, triggers use an exclude layer mask to filter out specific physics layers from trigger detection; toggling clears or sets all active triggers")]
	public static bool UseExcludeLayers
	{
		get
		{
			return _useExcludeLayers;
		}
		set
		{
			if (_useExcludeLayers != value)
			{
				if (_useExcludeLayers)
				{
					ClearExcludeLayers();
				}
				if (!_useExcludeLayers)
				{
					SetExcludeLayers();
				}
			}
			_useExcludeLayers = value;
		}
	}

	protected bool IsBeingDisabled { get; private set; }

	protected virtual void Awake()
	{
		_allTriggerBase.Add(this);
		UpdateExcludeLayers();
	}

	private void UpdateExcludeLayers()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		if (!UseExcludeLayers)
		{
			return;
		}
		List<Collider> list = Pool.Get<List<Collider>>();
		((Component)this).gameObject.GetComponentsInChildren<Collider>(list);
		int num = ~LayerMask.op_Implicit(interestLayers);
		foreach (Collider item in list)
		{
			if (item.isTrigger)
			{
				item.excludeLayers = LayerMask.op_Implicit(num);
			}
		}
		Pool.FreeUnmanaged<Collider>(ref list);
	}

	[ServerVar(Help = "(Generated) Removes the exclude layer configuration from all registered TriggerBase instances, resetting them to detect all layers")]
	public static void ClearExcludeLayers()
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		Debug.Log((object)$"Clearing ExcludeLayers for {_allTriggerBase.Count} triggers");
		List<Collider> list = Pool.Get<List<Collider>>();
		foreach (TriggerBase item in _allTriggerBase)
		{
			if ((Object)(object)item == (Object)null)
			{
				continue;
			}
			((Component)item).gameObject.GetComponentsInChildren<Collider>(list);
			foreach (Collider item2 in list)
			{
				if (item2.isTrigger)
				{
					item2.excludeLayers = LayerMask.op_Implicit(0);
				}
			}
		}
		Pool.FreeUnmanaged<Collider>(ref list);
	}

	[ServerVar(Help = "(Generated) Applies the configured exclude layer mask to all registered TriggerBase instances to filter out unwanted layer detections")]
	public static void SetExcludeLayers()
	{
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		Debug.Log((object)$"Setting ExcludeLayers for {_allTriggerBase.Count} triggers");
		List<Collider> list = Pool.Get<List<Collider>>();
		foreach (TriggerBase item in _allTriggerBase)
		{
			if ((Object)(object)item == (Object)null)
			{
				continue;
			}
			((Component)item).gameObject.GetComponentsInChildren<Collider>(list);
			int num = ~LayerMask.op_Implicit(item.interestLayers);
			foreach (Collider item2 in list)
			{
				if (item2.isTrigger)
				{
					item2.excludeLayers = LayerMask.op_Implicit(num);
				}
			}
		}
		Pool.FreeUnmanaged<Collider>(ref list);
	}

	public virtual GameObject InterestedInObject(GameObject obj)
	{
		int num = 1 << obj.layer;
		if ((((LayerMask)(ref interestLayers)).value & num) != num)
		{
			return null;
		}
		return obj;
	}

	internal virtual GameObject InterestedInObjectExitOnly(GameObject obj)
	{
		return InterestedInObject(obj);
	}

	internal virtual GameObject InterestedInObjectEnterOnly(GameObject obj)
	{
		return InterestedInObject(obj);
	}

	protected virtual void OnDisable()
	{
		if (!Application.isQuitting && contents != null)
		{
			IsBeingDisabled = true;
			GameObject[] array = contents.ToArray();
			foreach (GameObject targetObj in array)
			{
				OnTriggerExitImpl(targetObj);
			}
			IsBeingDisabled = false;
			contents = null;
		}
	}

	public virtual void OnEntityEnter(BaseEntity ent)
	{
		if (!((Object)(object)ent == (Object)null))
		{
			if (entityContents == null)
			{
				entityContents = new HashSet<BaseEntity>();
			}
			if (Interface.CallHook("OnEntityEnter", this, ent) == null)
			{
				entityContents.Add(ent);
				OnEntityEnterTrigger?.Invoke(ent);
			}
		}
	}

	public virtual void OnEntityLeave(BaseEntity ent)
	{
		if (entityContents != null && Interface.CallHook("OnEntityLeave", this, ent) == null)
		{
			entityContents.Remove(ent);
			OnEntityLeaveTrigger?.Invoke(ent);
		}
	}

	public virtual void OnObjectAdded(GameObject obj, Collider col)
	{
		if (!((Object)(object)obj == (Object)null))
		{
			BaseEntity baseEntity = GameObjectEx.ToBaseEntity(obj);
			if (Object.op_Implicit((Object)(object)baseEntity))
			{
				baseEntity.EnterTrigger(this);
				OnEntityEnter(baseEntity);
			}
		}
	}

	public virtual void OnObjectRemoved(GameObject obj)
	{
		if ((Object)(object)obj == (Object)null)
		{
			return;
		}
		BaseEntity baseEntity = GameObjectEx.ToBaseEntity(obj, allowDestroyed: true);
		if (!Object.op_Implicit((Object)(object)baseEntity))
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		foreach (GameObject content in contents)
		{
			if ((Object)(object)content == (Object)null)
			{
				flag2 = true;
			}
			else if ((Object)(object)GameObjectEx.ToBaseEntity(content, allowDestroyed: true) == (Object)(object)baseEntity)
			{
				flag = true;
				break;
			}
		}
		if (flag2)
		{
			int num = contents.RemoveWhere((GameObject x) => (Object)(object)x == (Object)null);
			Debug.LogWarning((object)$"Trigger {((object)this).ToString()} contained {num} null objects, cleaned up");
		}
		if (!flag)
		{
			baseEntity.LeaveTrigger(this);
			OnEntityLeave(baseEntity);
		}
	}

	public void RemoveInvalidEntities()
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		if (CollectionEx.IsNullOrEmpty(entityContents))
		{
			return;
		}
		Collider component = ((Component)this).GetComponent<Collider>();
		if ((Object)(object)component == (Object)null)
		{
			return;
		}
		Bounds bounds = component.bounds;
		((Bounds)(ref bounds)).Expand(1f);
		List<BaseEntity> list = null;
		foreach (BaseEntity entityContent in entityContents)
		{
			if ((Object)(object)entityContent == (Object)null)
			{
				if (Debugging.checktriggers)
				{
					Debug.LogWarning((object)("Trigger " + ((object)this).ToString() + " contains destroyed entity."));
				}
				if (list == null)
				{
					list = Pool.Get<List<BaseEntity>>();
				}
				list.Add(entityContent);
			}
			else if (!((Bounds)(ref bounds)).Contains(entityContent.ClosestPoint(((Component)this).transform.position)))
			{
				if (Debugging.checktriggers)
				{
					Debug.LogWarning((object)("Trigger " + ((object)this).ToString() + " contains entity that is too far away: " + ((object)entityContent).ToString()));
				}
				if (list == null)
				{
					list = Pool.Get<List<BaseEntity>>();
				}
				list.Add(entityContent);
			}
		}
		if (list == null)
		{
			return;
		}
		foreach (BaseEntity item in list)
		{
			RemoveEntity(item);
		}
		Pool.FreeUnmanaged<BaseEntity>(ref list);
	}

	public bool CheckEntity(BaseEntity ent)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)ent == (Object)null)
		{
			return true;
		}
		Collider component = ((Component)this).GetComponent<Collider>();
		if ((Object)(object)component == (Object)null)
		{
			return true;
		}
		Bounds bounds = component.bounds;
		((Bounds)(ref bounds)).Expand(1f);
		return ((Bounds)(ref bounds)).Contains(ent.ClosestPoint(((Component)this).transform.position));
	}

	public virtual void OnObjects()
	{
	}

	public virtual void OnEmpty()
	{
		contents = null;
		entityContents = null;
	}

	public void RemoveObject(GameObject obj)
	{
		if (!((Object)(object)obj == (Object)null))
		{
			Collider component = obj.GetComponent<Collider>();
			if (!((Object)(object)component == (Object)null))
			{
				OnTriggerExit(component);
			}
		}
	}

	public void RemoveEntity(BaseEntity ent)
	{
		if ((Object)(object)this == (Object)null || contents == null || (Object)(object)ent == (Object)null)
		{
			return;
		}
		List<GameObject> list = Pool.Get<List<GameObject>>();
		foreach (GameObject content in contents)
		{
			if ((Object)(object)content != (Object)null && (Object)(object)GameObjectEx.ToBaseEntity(content, allowDestroyed: true) == (Object)(object)ent)
			{
				list.Add(content);
			}
		}
		foreach (GameObject item in list)
		{
			OnTriggerExitImpl(item);
		}
		Pool.FreeUnmanaged<GameObject>(ref list);
	}

	public void OnTriggerEnter(Collider collider)
	{
		if ((Object)(object)this == (Object)null || !((Behaviour)this).enabled)
		{
			return;
		}
		using (TimeWarning.New("TriggerBase.OnTriggerEnter"))
		{
			GameObject val = InterestedInObjectEnterOnly(((Component)collider).gameObject);
			if ((Object)(object)val == (Object)null)
			{
				return;
			}
			if (contents == null)
			{
				contents = new HashSet<GameObject>();
			}
			if (contents.Contains(val))
			{
				return;
			}
			int count = contents.Count;
			contents.Add(val);
			OnObjectAdded(val, collider);
			if (count == 0 && contents != null && contents.Count == 1)
			{
				OnObjects();
			}
		}
		if (Debugging.checktriggers)
		{
			RemoveInvalidEntities();
		}
	}

	internal virtual bool SkipOnTriggerExit(Collider collider)
	{
		return false;
	}

	public void OnTriggerExit(Collider collider)
	{
		if ((Object)(object)this == (Object)null || (Object)(object)collider == (Object)null || SkipOnTriggerExit(collider))
		{
			return;
		}
		GameObject val = InterestedInObjectExitOnly(((Component)collider).gameObject);
		if (!((Object)(object)val == (Object)null))
		{
			OnTriggerExitImpl(val);
			if (Debugging.checktriggers)
			{
				RemoveInvalidEntities();
			}
		}
	}

	public void OnTriggerExitImpl(GameObject targetObj)
	{
		if (contents != null && contents.Contains(targetObj))
		{
			contents.Remove(targetObj);
			OnObjectRemoved(targetObj);
			if (contents == null || contents.Count == 0)
			{
				OnEmpty();
			}
		}
	}
}
