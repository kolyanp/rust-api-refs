using System;
using System.Text;
using Facepunch;
using UnityEngine;

public class GrowableGenes
{
	public GrowableGene[] Genes;

	private static GrowableGenetics.GeneWeighting[] baseWeights = new GrowableGenetics.GeneWeighting[6];

	private static GrowableGenetics.GeneWeighting[] slotWeights = new GrowableGenetics.GeneWeighting[6];

	public GrowableGenes()
	{
		Clear();
	}

	private void Clear()
	{
		Genes = new GrowableGene[6];
		for (int i = 0; i < 6; i++)
		{
			Genes[i] = new GrowableGene();
		}
	}

	public void GenerateFavourableGenes(GrowableEntity growable, float geneChance = -1f)
	{
		if (!((Object)(object)growable == (Object)null) && !((Object)(object)growable.Properties.Genes == (Object)null))
		{
			CalculateBaseWeights(growable.Properties.Genes);
			for (int i = 0; i < 6; i++)
			{
				CalculateSlotWeights(growable.Properties.Genes, i);
				Genes[i].Set(PickFavourableGeneType(geneChance), firstSet: true);
			}
		}
	}

	public void GenerateRandom(GrowableEntity growable)
	{
		if (!((Object)(object)growable == (Object)null) && !((Object)(object)growable.Properties.Genes == (Object)null))
		{
			CalculateBaseWeights(growable.Properties.Genes);
			for (int i = 0; i < 6; i++)
			{
				CalculateSlotWeights(growable.Properties.Genes, i);
				Genes[i].Set(PickWeightedGeneType(), firstSet: true);
			}
		}
	}

	private void CalculateBaseWeights(GrowableGeneProperties properties)
	{
		int num = 0;
		GrowableGeneProperties.GeneWeight[] weights = properties.Weights;
		for (int i = 0; i < weights.Length; i++)
		{
			GrowableGeneProperties.GeneWeight geneWeight = weights[i];
			baseWeights[num].GeneType = (slotWeights[num].GeneType = (GrowableGenetics.GeneType)num);
			baseWeights[num].Weighting = geneWeight.BaseWeight;
			num++;
		}
	}

	private void CalculateSlotWeights(GrowableGeneProperties properties, int slot)
	{
		int num = 0;
		GrowableGeneProperties.GeneWeight[] weights = properties.Weights;
		for (int i = 0; i < weights.Length; i++)
		{
			GrowableGeneProperties.GeneWeight geneWeight = weights[i];
			slotWeights[num].Weighting = baseWeights[num].Weighting + geneWeight.SlotWeights[slot];
			num++;
		}
	}

	private GrowableGenetics.GeneType PickWeightedGeneType()
	{
		float num = 0f;
		GrowableGenetics.GeneWeighting[] array = slotWeights;
		for (int i = 0; i < array.Length; i++)
		{
			GrowableGenetics.GeneWeighting geneWeighting = array[i];
			num += geneWeighting.Weighting;
		}
		GrowableGenetics.GeneType result = GrowableGenetics.GeneType.Empty;
		float num2 = Random.Range(0f, num);
		float num3 = 0f;
		array = slotWeights;
		for (int i = 0; i < array.Length; i++)
		{
			GrowableGenetics.GeneWeighting geneWeighting2 = array[i];
			num3 += geneWeighting2.Weighting;
			if (num2 < num3)
			{
				result = geneWeighting2.GeneType;
				break;
			}
		}
		return result;
	}

	private GrowableGenetics.GeneType PickFavourableGeneType(float favourableGeneChance = -1f)
	{
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		if (favourableGeneChance < 0f)
		{
			favourableGeneChance = PlanterBoxStatic.FavourableGeneChance;
		}
		BufferList<GrowableGenetics.GeneWeighting> val = Pool.Get<BufferList<GrowableGenetics.GeneWeighting>>();
		BufferList<GrowableGenetics.GeneWeighting> val2 = Pool.Get<BufferList<GrowableGenetics.GeneWeighting>>();
		float num = 0f;
		float num2 = 0f;
		GrowableGenetics.GeneWeighting[] array = slotWeights;
		for (int i = 0; i < array.Length; i++)
		{
			GrowableGenetics.GeneWeighting geneWeighting = array[i];
			if (GrowableGene.IsPositive(geneWeighting.GeneType))
			{
				val.Add(geneWeighting);
				num += geneWeighting.Weighting;
			}
			else
			{
				val2.Add(geneWeighting);
				num2 += geneWeighting.Weighting;
			}
		}
		float num3 = Mathx.RemapValClamped(Mathf.Clamp(favourableGeneChance, 0f, 1f), 0f, 1f, 1f, 0f);
		float num4 = num + num2 * num3;
		float num5 = Random.Range(0f, num4);
		float num6 = 0f;
		Enumerator<GrowableGenetics.GeneWeighting> enumerator = val.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				GrowableGenetics.GeneWeighting current = enumerator.Current;
				num6 += current.Weighting;
				if (num5 < num6)
				{
					Pool.FreeUnmanaged<GrowableGenetics.GeneWeighting>(ref val);
					Pool.FreeUnmanaged<GrowableGenetics.GeneWeighting>(ref val2);
					return current.GeneType;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		enumerator = val2.GetEnumerator();
		try
		{
			while (enumerator.MoveNext())
			{
				GrowableGenetics.GeneWeighting current2 = enumerator.Current;
				num6 += current2.Weighting * num3;
				if (num5 < num6)
				{
					Pool.FreeUnmanaged<GrowableGenetics.GeneWeighting>(ref val);
					Pool.FreeUnmanaged<GrowableGenetics.GeneWeighting>(ref val2);
					return current2.GeneType;
				}
			}
		}
		finally
		{
			((IDisposable)enumerator/*cast due to constrained. prefix*/).Dispose();
		}
		Pool.FreeUnmanaged<GrowableGenetics.GeneWeighting>(ref val);
		Pool.FreeUnmanaged<GrowableGenetics.GeneWeighting>(ref val2);
		return GrowableGenetics.GeneType.Empty;
	}

	public int GetGeneTypeCount(GrowableGenetics.GeneType geneType)
	{
		int num = 0;
		GrowableGene[] genes = Genes;
		for (int i = 0; i < genes.Length; i++)
		{
			if (genes[i].Type == geneType)
			{
				num++;
			}
		}
		return num;
	}

	public int GetPositiveGeneCount()
	{
		int num = 0;
		GrowableGene[] genes = Genes;
		for (int i = 0; i < genes.Length; i++)
		{
			if (genes[i].IsPositive())
			{
				num++;
			}
		}
		return num;
	}

	public int GetNegativeGeneCount()
	{
		int num = 0;
		GrowableGene[] genes = Genes;
		for (int i = 0; i < genes.Length; i++)
		{
			if (!genes[i].IsPositive())
			{
				num++;
			}
		}
		return num;
	}

	public void Save(BaseNetworkable.SaveInfo info)
	{
		info.msg.growableEntity.genes = GrowableGeneEncoding.EncodeGenesToInt(this);
		info.msg.growableEntity.previousGenes = GrowableGeneEncoding.EncodePreviousGenesToInt(this);
	}

	public void Load(BaseNetworkable.LoadInfo info)
	{
		if (info.msg.growableEntity != null)
		{
			GrowableGeneEncoding.DecodeIntToGenes(info.msg.growableEntity.genes, this);
			GrowableGeneEncoding.DecodeIntToPreviousGenes(info.msg.growableEntity.previousGenes, this);
		}
	}

	public void DebugPrint()
	{
		Debug.Log((object)GetDisplayString(previousGenes: false));
	}

	private string GetDisplayString(bool previousGenes)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < 6; i++)
		{
			stringBuilder.Append(GrowableGene.GetDisplayCharacter(previousGenes ? Genes[i].PreviousType : Genes[i].Type));
		}
		return stringBuilder.ToString();
	}
}
