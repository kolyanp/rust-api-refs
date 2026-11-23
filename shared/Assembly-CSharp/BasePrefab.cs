using System;
using UnityEngine;
using UnityEngine.Serialization;

public class BasePrefab : BaseMonoBehaviour, IPrefabPreProcess, ISerializationCallbackReceiver
{
	[SerializeField]
	[HideInInspector]
	[FormerlySerializedAs("prefabID")]
	private uint serializedPrefabID;

	[NonSerialized]
	private EncryptedValue<uint> prefabIDValue = 0u;

	[HideInInspector]
	public bool isClient;

	public uint prefabID
	{
		get
		{
			return prefabIDValue.Get();
		}
		set
		{
			prefabIDValue.Set(value);
		}
	}

	public bool isServer => !isClient;

	void ISerializationCallbackReceiver.OnBeforeSerialize()
	{
		serializedPrefabID = prefabID;
	}

	void ISerializationCallbackReceiver.OnAfterDeserialize()
	{
		prefabID = serializedPrefabID;
		serializedPrefabID = 0u;
	}

	public virtual void PreProcess(IPrefabProcessor preProcess, GameObject rootObj, string name, bool serverside, bool clientside, bool bundling)
	{
		prefabID = StringPool.Get(name);
		isClient = clientside;
	}
}
