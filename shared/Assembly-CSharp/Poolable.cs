using System;
using System.Collections.Generic;
using System.Linq;
using ConVar;
using Facepunch;
using UnityEngine;

public class Poolable : MonoBehaviour, IClientComponent, IPrefabPostProcess
{
	public bool restoreHierarchy;

	[HideInInspector]
	public uint prefabID;

	[HideInInspector]
	public Behaviour[] behaviours;

	[HideInInspector]
	public Rigidbody[] rigidbodies;

	[HideInInspector]
	public Collider[] colliders;

	[HideInInspector]
	public LODGroup[] lodgroups;

	[HideInInspector]
	public Renderer[] renderers;

	[HideInInspector]
	public ParticleSystem[] particles;

	[HideInInspector]
	public bool[] behaviourStates;

	[HideInInspector]
	public bool[] rigidbodyStates;

	[HideInInspector]
	public bool[] colliderStates;

	[HideInInspector]
	public bool[] lodgroupStates;

	[HideInInspector]
	public bool[] rendererStates;

	[HideInInspector]
	public bool[] childActiveStates;

	public int ClientCount
	{
		get
		{
			if (Object.op_Implicit((Object)(object)((Component)this).GetComponent<CodeLock>()))
			{
				return 200;
			}
			if ((Object)(object)((Component)this).GetComponent<LootPanel>() != (Object)null)
			{
				return 1;
			}
			if (((Component)this).GetComponent<DecorComponent>() != null)
			{
				return 100;
			}
			if ((Object)(object)((Component)this).GetComponent<BuildingBlock>() != (Object)null)
			{
				return 100;
			}
			if ((Object)(object)((Component)this).GetComponent<Door>() != (Object)null)
			{
				if ((bool)((Component)this).GetComponent<Construction>())
				{
					return 100;
				}
				return 1;
			}
			if ((Object)(object)((Component)this).GetComponent<Projectile>() != (Object)null)
			{
				return 100;
			}
			if ((Object)(object)((Component)this).GetComponent<Gib>() != (Object)null)
			{
				return 100;
			}
			if ((Object)(object)((Component)this).GetComponent<BaseVehicleModule>() != (Object)null)
			{
				return 8;
			}
			if ((Object)(object)((Component)this).GetComponent<BaseVehicleMountPoint>() != (Object)null)
			{
				return 8;
			}
			if ((Object)(object)((Component)this).GetComponent<BaseVehicle>() != (Object)null)
			{
				return 2;
			}
			if (Object.op_Implicit((Object)(object)((Component)this).GetComponent<UIMapVendingMachineMarker>()))
			{
				return 25;
			}
			if (Object.op_Implicit((Object)(object)((Component)this).GetComponent<UIMapVendingMachineMarkerCluster>()))
			{
				return 25;
			}
			if ((Object)(object)((Component)this).GetComponent<CollectableEasterEgg>() != (Object)null)
			{
				return 50;
			}
			if (Object.op_Implicit((Object)(object)((Component)this).GetComponent<SlotMachinePayoutWidget>()))
			{
				return 24;
			}
			return 1;
		}
	}

	public int ServerCount => 0;

	public void PostProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		if (!bundling && !((Object)(object)((Component)this).gameObject != (Object)(object)rootObj))
		{
			Initialize(StringPool.Get(name));
		}
	}

	public void Initialize(uint id)
	{
		prefabID = id;
		behaviours = ((Component)this).gameObject.GetComponentsInChildren(typeof(Behaviour), true).OfType<Behaviour>().ToArray();
		rigidbodies = ((Component)this).gameObject.GetComponentsInChildren<Rigidbody>(true);
		colliders = ((Component)this).gameObject.GetComponentsInChildren<Collider>(true);
		lodgroups = ((Component)this).gameObject.GetComponentsInChildren<LODGroup>(true);
		renderers = ((Component)this).gameObject.GetComponentsInChildren<Renderer>(true);
		particles = ((Component)this).gameObject.GetComponentsInChildren<ParticleSystem>(true);
		if (behaviours.Length == 0)
		{
			behaviours = Array.Empty<Behaviour>();
		}
		if (rigidbodies.Length == 0)
		{
			rigidbodies = Array.Empty<Rigidbody>();
		}
		if (colliders.Length == 0)
		{
			colliders = Array.Empty<Collider>();
		}
		if (lodgroups.Length == 0)
		{
			lodgroups = Array.Empty<LODGroup>();
		}
		if (renderers.Length == 0)
		{
			renderers = Array.Empty<Renderer>();
		}
		if (particles.Length == 0)
		{
			particles = Array.Empty<ParticleSystem>();
		}
		behaviourStates = ArrayEx.New<bool>(behaviours.Length);
		rigidbodyStates = ArrayEx.New<bool>(rigidbodies.Length);
		colliderStates = ArrayEx.New<bool>(colliders.Length);
		lodgroupStates = ArrayEx.New<bool>(lodgroups.Length);
		rendererStates = ArrayEx.New<bool>(renderers.Length);
		CaptureComponentStates();
		childActiveStates = new bool[CountTransforms(((Component)this).transform)];
		CaptureActiveStates(((Component)this).transform, childActiveStates, 0);
	}

	private void CaptureComponentStates()
	{
		for (int i = 0; i < behaviours.Length; i++)
		{
			behaviourStates[i] = behaviours[i].enabled;
		}
		for (int j = 0; j < renderers.Length; j++)
		{
			rendererStates[j] = renderers[j].enabled;
		}
		for (int k = 0; k < lodgroups.Length; k++)
		{
			lodgroupStates[k] = lodgroups[k].enabled;
		}
		for (int l = 0; l < colliders.Length; l++)
		{
			colliderStates[l] = colliders[l].enabled;
		}
		for (int m = 0; m < rigidbodies.Length; m++)
		{
			rigidbodyStates[m] = rigidbodies[m].isKinematic;
		}
	}

	private static int CountTransforms(Transform root)
	{
		int num = 1;
		for (int i = 0; i < root.childCount; i++)
		{
			num += CountTransforms(root.GetChild(i));
		}
		return num;
	}

	private static int CaptureActiveStates(Transform root, bool[] states, int index)
	{
		states[index++] = ((Component)root).gameObject.activeSelf;
		for (int i = 0; i < root.childCount; i++)
		{
			index = CaptureActiveStates(root.GetChild(i), states, index);
		}
		return index;
	}

	private static int RestoreActiveStates(Transform root, bool[] states, int index)
	{
		if (index < 0 || index >= states.Length)
		{
			return -1;
		}
		if (index > 0 && ((Component)root).gameObject.activeSelf != states[index])
		{
			((Component)root).gameObject.SetActive(states[index]);
		}
		index++;
		for (int i = 0; i < root.childCount; i++)
		{
			index = RestoreActiveStates(root.GetChild(i), states, index);
			if (index < 0)
			{
				return -1;
			}
		}
		return index;
	}

	private void RestoreChildActiveStates()
	{
		if (childActiveStates != null && RestoreActiveStates(((Component)this).transform, childActiveStates, 0) != childActiveStates.Length)
		{
			Debug.LogError((object)("Pooled prefab changed its game object hierarchy at runtime, which pooling can't restore: " + ((Object)this).name + " (prefab has " + childActiveStates.Length + " transforms, instance has " + CountTransforms(((Component)this).transform) + ")"), (Object)(object)this);
		}
	}

	private void RestoreHierarchy(GameObject prefab)
	{
		if (!((Object)(object)prefab == (Object)null))
		{
			RestoreHierarchy(((Component)this).transform, prefab.transform);
		}
	}

	private static void RestoreHierarchy(Transform instance, Transform prefab)
	{
		int childCount = prefab.childCount;
		for (int num = instance.childCount - 1; num >= childCount; num--)
		{
			Transform child = instance.GetChild(num);
			OnParentDestroyingEx.SendOnParentDestroying(((Component)child).gameObject);
			if ((Object)(object)child.parent == (Object)(object)instance)
			{
				child.SetParent((Transform)null, true);
			}
		}
		if (instance.childCount >= childCount)
		{
			for (int i = 0; i < childCount; i++)
			{
				Transform child2 = instance.GetChild(i);
				Transform child3 = prefab.GetChild(i);
				RestorePose(child2, child3);
				RestoreHierarchy(child2, child3);
			}
		}
	}

	private static void RestorePose(Transform instance, Transform prefab)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		if (!(instance is RectTransform))
		{
			Vector3 val = default(Vector3);
			Quaternion val2 = default(Quaternion);
			prefab.GetLocalPositionAndRotation(ref val, ref val2);
			if (instance.localPosition != val || instance.localRotation != val2)
			{
				instance.SetLocalPositionAndRotation(val, val2);
			}
			if (instance.localScale != prefab.localScale)
			{
				instance.localScale = prefab.localScale;
			}
		}
	}

	public void EnterPool(GameObject prefab)
	{
		if ((Object)(object)((Component)this).transform.parent != (Object)null)
		{
			((Component)this).transform.SetParent((Transform)null, false);
		}
		if (restoreHierarchy)
		{
			RestoreHierarchy(prefab);
		}
		if (Pool.mode <= 1)
		{
			if (((Component)this).gameObject.activeSelf)
			{
				((Component)this).gameObject.SetActive(false);
			}
		}
		else
		{
			SetBehaviourEnabled(state: false);
			SetComponentEnabled(state: false);
			if (!((Component)this).gameObject.activeSelf)
			{
				((Component)this).gameObject.SetActive(true);
			}
		}
		CancelAllInvokes();
	}

	private void CancelAllInvokes()
	{
		if (behaviours == null || behaviours.Length == 0)
		{
			return;
		}
		PooledHashSet<Behaviour> val = Pool.Get<PooledHashSet<Behaviour>>();
		try
		{
			for (int i = 0; i < behaviours.Length; i++)
			{
				Behaviour val2 = behaviours[i];
				if (Object.op_Implicit((Object)(object)val2))
				{
					((HashSet<Behaviour>)(object)val).Add(val2);
				}
			}
			if (((HashSet<Behaviour>)(object)val).Count != 0)
			{
				if (Object.op_Implicit((Object)(object)SingletonComponent<InvokeHandler>.Instance))
				{
					SingletonComponent<InvokeHandler>.Instance.CancelInvokes((HashSet<Behaviour>)(object)val);
				}
				if (Object.op_Implicit((Object)(object)SingletonComponent<InvokeHandlerFixedTime>.Instance))
				{
					SingletonComponent<InvokeHandlerFixedTime>.Instance.CancelInvokes((HashSet<Behaviour>)(object)val);
				}
				if (Object.op_Implicit((Object)(object)SingletonComponent<InvokeHandlerUnscaledTime>.Instance))
				{
					SingletonComponent<InvokeHandlerUnscaledTime>.Instance.CancelInvokes((HashSet<Behaviour>)(object)val);
				}
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	public void LeavePool()
	{
		if (restoreHierarchy)
		{
			RestoreChildActiveStates();
		}
		if (Pool.mode > 1)
		{
			SetComponentEnabled(state: true);
		}
	}

	public void SetBehaviourEnabled(bool state)
	{
		try
		{
			if (!state)
			{
				for (int i = 0; i < behaviours.Length; i++)
				{
					behaviours[i].enabled = false;
				}
				for (int j = 0; j < particles.Length; j++)
				{
					ParticleSystem obj = particles[j];
					obj.Stop();
					obj.Clear();
				}
				return;
			}
			for (int k = 0; k < particles.Length; k++)
			{
				ParticleSystem val = particles[k];
				if (val.playOnAwake)
				{
					val.Play();
				}
			}
			for (int l = 0; l < behaviours.Length; l++)
			{
				behaviours[l].enabled = behaviourStates[l];
			}
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("Pooling error: " + ((Object)this).name + " (" + ex.Message + ")"));
		}
	}

	private static bool CanToggleCollider(Collider collider)
	{
		WheelCollider val = (WheelCollider)(object)((collider is WheelCollider) ? collider : null);
		if (val != null)
		{
			return (Object)(object)((Collider)val).attachedRigidbody != (Object)null;
		}
		return true;
	}

	public void SetComponentEnabled(bool state)
	{
		try
		{
			if (!state)
			{
				for (int i = 0; i < renderers.Length; i++)
				{
					renderers[i].enabled = false;
				}
				for (int j = 0; j < lodgroups.Length; j++)
				{
					lodgroups[j].enabled = false;
				}
				for (int k = 0; k < colliders.Length; k++)
				{
					Collider val = colliders[k];
					if (CanToggleCollider(val))
					{
						val.enabled = false;
					}
				}
				for (int l = 0; l < rigidbodies.Length; l++)
				{
					Rigidbody obj = rigidbodies[l];
					obj.isKinematic = true;
					obj.detectCollisions = false;
				}
				return;
			}
			for (int m = 0; m < renderers.Length; m++)
			{
				renderers[m].enabled = rendererStates[m];
			}
			for (int n = 0; n < lodgroups.Length; n++)
			{
				lodgroups[n].enabled = lodgroupStates[n];
			}
			for (int num = 0; num < colliders.Length; num++)
			{
				Collider val2 = colliders[num];
				if (CanToggleCollider(val2))
				{
					val2.enabled = colliderStates[num];
				}
			}
			for (int num2 = 0; num2 < rigidbodies.Length; num2++)
			{
				Rigidbody obj2 = rigidbodies[num2];
				obj2.isKinematic = rigidbodyStates[num2];
				obj2.detectCollisions = true;
			}
		}
		catch (Exception ex)
		{
			Debug.LogError((object)("Pooling error: " + ((Object)this).name + " (" + ex.Message + ")"));
		}
	}
}
