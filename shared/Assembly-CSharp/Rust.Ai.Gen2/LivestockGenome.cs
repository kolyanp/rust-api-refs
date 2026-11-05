using ConVar;
using UnityEngine;

namespace Rust.Ai.Gen2;

public static class LivestockGenome
{
	public const int GeneSlots = 5;

	private const int BitsPerAllele = 2;

	private const int BitsPerGene = 4;

	private const int AlleleMask = 3;

	private const int LineageShift = 20;

	private const int BitsPerMarker = 4;

	private const int MarkerMask = 15;

	public const int LineageMarkerCount = 16;

	public const int Unrolled = 0;

	public static readonly LivestockGene[] AllGenes = new LivestockGene[4]
	{
		LivestockGene.Dung,
		LivestockGene.Yield,
		LivestockGene.Fertility,
		LivestockGene.Hardiness
	};

	public static bool IsRolled(int genome)
	{
		return genome != 0;
	}

	public static bool IsLive(LivestockGene gene)
	{
		LivestockGene[] allGenes = AllGenes;
		for (int i = 0; i < allGenes.Length; i++)
		{
			if (allGenes[i] == gene)
			{
				return true;
			}
		}
		return false;
	}

	private static int ShiftFor(LivestockGene gene, int copy)
	{
		return (int)gene * 4 + copy * 2;
	}

	public static LivestockAllele GetAllele(int genome, LivestockGene gene, int copy)
	{
		int num = (genome >> ShiftFor(gene, copy)) & 3;
		if (num != 0)
		{
			return (LivestockAllele)(num - 1);
		}
		return LivestockAllele.Ok;
	}

	public static LivestockAllele Expressed(int genome, LivestockGene gene)
	{
		LivestockAllele allele = GetAllele(genome, gene, 0);
		LivestockAllele allele2 = GetAllele(genome, gene, 1);
		if (allele < allele2)
		{
			return allele2;
		}
		return allele;
	}

	public static bool IsPurebred(int genome, LivestockGene gene)
	{
		return GetAllele(genome, gene, 0) == GetAllele(genome, gene, 1);
	}

	public static LivestockAllele Carried(int genome, LivestockGene gene)
	{
		LivestockAllele allele = GetAllele(genome, gene, 0);
		LivestockAllele allele2 = GetAllele(genome, gene, 1);
		if (allele > allele2)
		{
			return allele2;
		}
		return allele;
	}

	public static int GetMarker(int genome, int copy)
	{
		return (genome >> 20 + copy * 4) & 0xF;
	}

	public static bool IsInbred(int genome)
	{
		if (IsRolled(genome))
		{
			return GetMarker(genome, 0) == GetMarker(genome, 1);
		}
		return false;
	}

	public static int WithAllele(int genome, LivestockGene gene, int copy, LivestockAllele allele)
	{
		int num = ShiftFor(gene, copy);
		int num2 = (int)((allele + 1) & (LivestockAllele)3);
		return (genome & ~(3 << num)) | (num2 << num);
	}

	public static int WithPair(int genome, LivestockGene gene, LivestockAllele first, LivestockAllele second)
	{
		return WithAllele(WithAllele(genome, gene, 0, first), gene, 1, second);
	}

	public static bool IsGodGenome(int genome)
	{
		if (!IsRolled(genome))
		{
			return false;
		}
		LivestockGene[] allGenes = AllGenes;
		foreach (LivestockGene gene in allGenes)
		{
			if (GetAllele(genome, gene, 0) != LivestockAllele.Good || GetAllele(genome, gene, 1) != LivestockAllele.Good)
			{
				return false;
			}
		}
		return true;
	}

	public static bool HidesBad(int genome)
	{
		if (!IsRolled(genome))
		{
			return false;
		}
		bool result = false;
		LivestockGene[] allGenes = AllGenes;
		foreach (LivestockGene gene in allGenes)
		{
			if (Expressed(genome, gene) != LivestockAllele.Good)
			{
				return false;
			}
			if (Carried(genome, gene) == LivestockAllele.Bad)
			{
				result = true;
			}
		}
		return result;
	}

	public static int WithMarkers(int genome, int first, int second)
	{
		return (genome & -267386881) | ((first & 0xF) << 20) | ((second & 0xF) << 24);
	}

	public static float ExpressedQuality(int genome)
	{
		int num = 0;
		LivestockGene[] allGenes = AllGenes;
		foreach (LivestockGene gene in allGenes)
		{
			num = (int)(num + Expressed(genome, gene));
		}
		return (float)num / (float)(AllGenes.Length * 2);
	}

	public static int Combine(NpcRandom random, int mother, int father, float mutationChance = 0f)
	{
		int genome = 0;
		LivestockGene[] allGenes = AllGenes;
		foreach (LivestockGene gene in allGenes)
		{
			genome = WithPair(genome, gene, InheritedAllele(random, mother, gene), InheritedAllele(random, father, gene));
		}
		genome = WithMarkers(genome, InheritedMarker(random, mother), InheritedMarker(random, father));
		return Mutate(random, genome, mutationChance);
	}

	private static LivestockAllele InheritedAllele(NpcRandom random, int parent, LivestockGene gene)
	{
		return GetAllele(parent, gene, CoinFlip(random));
	}

	private static int InheritedMarker(NpcRandom random, int parent)
	{
		return GetMarker(parent, CoinFlip(random));
	}

	private static int CoinFlip(NpcRandom random)
	{
		if (!(random.Value < 0.5f))
		{
			return 1;
		}
		return 0;
	}

	public static int Mutate(NpcRandom random, int genome, float chance)
	{
		if (chance <= 0f)
		{
			return genome;
		}
		LivestockGene[] allGenes = AllGenes;
		foreach (LivestockGene gene in allGenes)
		{
			for (int j = 0; j < 2; j++)
			{
				if (random.Value < chance)
				{
					genome = WithAllele(genome, gene, j, RollAllele(random));
				}
			}
		}
		return genome;
	}

	public static LivestockAllele RollAllele(NpcRandom random)
	{
		float num = Mathf.Clamp01(Livestock.geneGoodChance);
		float num2 = Mathf.Clamp01(Livestock.geneBadChance);
		float value = random.Value;
		if (value < num)
		{
			return LivestockAllele.Good;
		}
		if (value > 1f - num2)
		{
			return LivestockAllele.Bad;
		}
		return LivestockAllele.Ok;
	}

	public static int RollMarker(NpcRandom random)
	{
		return random.Range(0, 16);
	}

	public static int Roll(NpcRandom random)
	{
		int genome = 0;
		LivestockGene[] allGenes = AllGenes;
		foreach (LivestockGene gene in allGenes)
		{
			genome = WithPair(genome, gene, RollAllele(random), RollAllele(random));
		}
		return WithMarkers(genome, RollMarker(random), RollMarker(random));
	}
}
