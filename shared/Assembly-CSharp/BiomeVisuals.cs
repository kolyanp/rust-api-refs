using System;
using UnityEngine;

public class BiomeVisuals : MonoBehaviour
{
	[Serializable]
	public class EnvironmentVolumeOverride
	{
		public EnvironmentType Environment;

		public Enum Biome;
	}

	public GameObject Arid;

	public GameObject Temperate;

	public GameObject Tundra;

	public GameObject Arctic;

	public bool OverrideBiome;

	public Enum ToOverride;

	[Horizontal(2, -1)]
	public EnvironmentVolumeOverride[] EnvironmentVolumeOverrides;

	private bool _supportsPooling;

	private GameObject _defaultSelection;

	protected void Awake()
	{
		_supportsPooling = PoolableEx.SupportsPoolingInParent(((Component)this).gameObject);
		if (Object.op_Implicit((Object)(object)Arid) && Arid.activeSelf)
		{
			_defaultSelection = Arid;
		}
		else if (Object.op_Implicit((Object)(object)Temperate) && Temperate.activeSelf)
		{
			_defaultSelection = Temperate;
		}
		else if (Object.op_Implicit((Object)(object)Tundra) && Tundra.activeSelf)
		{
			_defaultSelection = Tundra;
		}
		else if (Object.op_Implicit((Object)(object)Arctic) && Arctic.activeSelf)
		{
			_defaultSelection = Arctic;
		}
	}

	protected void OnEnable()
	{
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Expected I4, but got Unknown
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Expected I4, but got Unknown
		int num = (((Object)(object)TerrainMeta.BiomeMap != (Object)null) ? TerrainMeta.BiomeMap.GetBiomeMaxType(((Component)this).transform.position) : 2);
		if (OverrideBiome)
		{
			num = (int)ToOverride;
		}
		else if (EnvironmentVolumeOverrides.Length != 0)
		{
			EnvironmentType environmentType = EnvironmentManager.Get(((Component)this).transform.position);
			EnvironmentVolumeOverride[] environmentVolumeOverrides = EnvironmentVolumeOverrides;
			foreach (EnvironmentVolumeOverride environmentVolumeOverride in environmentVolumeOverrides)
			{
				if ((environmentType & environmentVolumeOverride.Environment) != 0)
				{
					num = (int)environmentVolumeOverride.Biome;
					break;
				}
			}
		}
		switch (num)
		{
		case 1:
			SetChoice(Arid);
			break;
		case 2:
			SetChoice(Temperate);
			break;
		case 4:
			SetChoice(Tundra);
			break;
		case 8:
			SetChoice(Arctic);
			break;
		default:
			SetChoice(_defaultSelection);
			break;
		}
	}

	private void SetChoice(GameObject selection)
	{
		bool flag = !_supportsPooling;
		ApplyChoice(selection, Arid, flag);
		ApplyChoice(selection, Temperate, flag);
		ApplyChoice(selection, Tundra, flag);
		ApplyChoice(selection, Arctic, flag);
		if ((Object)(object)selection != (Object)null)
		{
			selection.SetActive(true);
		}
		if (flag)
		{
			GameManager.Destroy((Component)(object)this);
		}
	}

	private void ApplyChoice(GameObject selection, GameObject target, bool shouldDestroy)
	{
		if ((Object)(object)target != (Object)null && (Object)(object)target != (Object)(object)selection)
		{
			if (shouldDestroy)
			{
				GameManager.Destroy(target);
			}
			else
			{
				target.SetActive(false);
			}
		}
	}
}
