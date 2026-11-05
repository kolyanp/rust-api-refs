using System;
using System.Collections.Generic;
using System.Text;
using Rust.Ai.Gen2;
using UnityEngine;

namespace ConVar;

[Factory("livestock")]
public class Livestock : ConsoleSystem
{
	[ReplicatedVar(Help = "Whether the gene discs are shown on the livestock status panel, along with the inbred warning that explains them")]
	public static bool panelGenes = false;

	[ReplicatedVar(Help = "Whether both copies of every livestock gene are shown rather than only the one the animal expresses. Reveals what an animal carries without breeding it")]
	public static bool panelAlleles = false;

	[ReplicatedVar(Help = "Whether a farm animal's status panel says how long it has lived while owned")]
	public static bool panelAge = true;

	[ReplicatedVar(Help = "Whether a farm animal's status panel shows how much it trusts you, as one segment per band of the trust ladder")]
	public static bool panelTrust = true;

	[ReplicatedVar(Help = "Whether a farm animal's status panel shows its potential, how content it was raised, which its sale price is multiplied by")]
	public static bool panelPotential = true;

	[ReplicatedVar(Help = "Whether the male and female symbols are shown next to a farm animal's name on its status panel")]
	public static bool panelGender = true;

	[ReplicatedVar(Help = "How many seconds near a livestock animal before a bull stops squaring up at you")]
	public static float trustToTolerate = 30f;

	[ReplicatedVar(Help = "How many seconds near a livestock animal before it accepts being led, milked or sheared by you")]
	public static float trustToLead = 300f;

	[ReplicatedVar(Help = "The most seconds of trust a player can build with a livestock animal, which is the full bond: it defends you. Only its owner reaches it, see livestock.nonOwnerTrustShare")]
	public static float maxTrust = 900f;

	[ServerVar(Help = "The share of livestock.maxTrust a player can reach with an animal they do not own, kept short of 1 so only the owner has the full bond and is defended. At 1 anybody can")]
	public static float nonOwnerTrustShare = 0.95f;

	[ReplicatedVar(Help = "Whether the milk or wool row of a farm animal's status panel also says what it produces and how often, after its genes")]
	public static bool panelYield = false;

	[ReplicatedVar(Help = "Whether a farm animal's status panel shows how often it dungs, after its genes")]
	public static bool panelDung = false;

	[ReplicatedVar(Help = "Condition at or below which a livestock animal stops being willing to breed")]
	public static float breedPauseCondition = 0.6f;

	[ReplicatedVar(Help = "Condition at or above which a livestock animal that stopped breeding is willing again")]
	public static float breedResumeCondition = 0.7f;

	[ReplicatedVar(Help = "Whether livestock can be mounted into vehicle flatbeds")]
	public static bool allowMounting = true;

	[ReplicatedVar(Help = "What every gene's slider position is multiplied by on an inbred livestock animal, for its coat and for what it produces alike. At 0.5 a Good gene looks and performs as Ok and an Ok gene falls halfway to Bad. An animal is inbred when both its lineage markers match")]
	public static float inbredScale = 0.5f;

	[ReplicatedVar(Help = "The least a livestock animal's grass and sun needs count for in its overall mood, so a floored or roofed barn herd is kept below full rather than at nothing. The needs themselves still read down to 0")]
	public static float shelterNeedFloor = 0.5f;

	[ReplicatedVar(Help = "How much wool a player must be carrying before a livestock vendor will haggle over it at all")]
	public static int vendorWoolMinimum = 25;

	[ServerVar(Help = "Multiplier applied to how long a livestock infant takes to grow up; 0.5 = grow up twice as fast, 0 = disable aging entirely")]
	public static float ageScale = 1f;

	[ServerVar(Help = "Whether livestock look for a mate on their own; disabling it leaves livestock.makepregnant working")]
	public static bool breedingEnabled = true;

	[ServerVar(Help = "How many livestock animals of one species a server holds before none of that species breeds. Cattle and sheep are counted apart against the same number, 0 never caps")]
	public static int maxPerSpecies = 300;

	[ServerVar(Help = "How many livestock animals of one species can share open ground, linked through any within livestock.freeRangeLinkRadius of each other, litters on the way counted at their largest, before none of them breeds, 0 never caps")]
	public static int maxFreeRangeHerd = 5;

	[ServerVar(Help = "How far (in metres) apart two livestock animals out of a pen can stand and still be linked into one free range herd for livestock.maxFreeRangeHerd")]
	public static float freeRangeLinkRadius = 40f;

	[ServerVar(Help = "Floor in square metres each head needs in a pen to breed and to have full personal space. A 2x2 of foundations floods about 27, so it houses three")]
	public static float breedingFloorPerHead = 8f;

	[ServerVar(Help = "Floor in square metres per head at or below which a pen leaves livestock no personal space at all, which kills them. Half a foundation by default")]
	public static float lethalFloorPerHead = 4.5f;

	[ServerVar(Help = "The share of a floor's sampled polys the sun must reach at some hour of a simulated day for the sun need to read full")]
	public static float sunFullShare = 0.5f;

	[ServerVar(Help = "How many of a floor's navmesh polys are sampled for sunlight when it is measured")]
	public static int sunSamplePolys = 64;

	[ServerVar(Help = "How many sun positions between sunrise and sunset each sampled poly is tested against")]
	public static int sunSampleHours = 12;

	[ServerVar(Help = "How many minutes a floor keeps its sun verdict before it is measured again, for shade that arrives without a navmesh change")]
	public static float sunResampleMinutes = 30f;

	[ServerVar(Help = "The share of its waking day a livestock animal is assumed to spend on its feet when a trough's remaining time is estimated")]
	public static float estimateStandingShare = 0.38f;

	[ServerVar(Help = "The shortest time (in seconds) a livestock animal of any species grazes in one helping, at a trough or on grass")]
	public static float grazeHelpingMin = 30f;

	[ServerVar(Help = "The longest time (in seconds) a livestock animal of any species grazes in one helping, at a trough or on grass")]
	public static float grazeHelpingMax = 50f;

	[ServerVar(Help = "How long (in seconds) a hungry livestock animal fails to get a place at a stocked trough before it eats from it where it stands")]
	public static float remoteEatAfter = 60f;

	[ServerVar(Help = "How far (in metres) from a trough a livestock animal may stand to eat from it without a place of its own")]
	public static float remoteEatDistance = 6f;

	[ServerVar(Help = "How often (in seconds) a livestock animal lying down for want of food or water looks for some again")]
	public static float starvedSearchInterval = 30f;

	[ServerVar(Help = "How far (in metres) a male livestock animal looks for a mate")]
	public static float breedSearchRadius = 30f;

	[ServerVar(Help = "How long (in seconds) a male livestock animal waits before breeding again, counted from conception. The female's is derived from how long her calf takes to grow up")]
	public static float breedCooldown = 600f;

	[ServerVar(Help = "Scales how long a mother waits after calving before she can carry again. 1 is one calf grown up per birth")]
	public static float calvingCooldownScale = 1f;

	[ServerVar(Help = "How long (in seconds) a livestock animal stays wary of a player that hurt it or one of its herd, moving off when they come close again; 0 disables grudges")]
	public static float grudgeDuration = 45f;

	[ServerVar(Help = "How close (in metres) a remembered aggressor has to get before a wary livestock animal moves off")]
	public static float grudgeRange = 8f;

	[ServerVar(Help = "How long (in seconds) a mother stands her ground for her own calf after something has threatened it, rather than running like any other cow; 0 disables maternal defence")]
	public static float motherlyDuration = 300f;

	[ServerVar(Help = "How well met a livestock animal's worst need must be (0-1) before it will canter about for the fun of it; above 1 disables it")]
	public static float friskyThreshold = 0.85f;

	[ServerVar(Help = "How close (in metres) a player has to be for a livestock animal to count the time; must sit inside the animal's sense radius; 0 disables familiarity")]
	public static float familiarityRadius = 20f;

	[ServerVar(Help = "Multiplies how fast livestock familiarity builds up; 20 makes a five minute bond take fifteen seconds. For testing, leave at 1 for play")]
	public static float familiarityRate = 1f;

	[ServerVar(Help = "How many seconds near a livestock animal before a curious one follows you without a lead")]
	public static float trustToFollow = 5f;

	[ServerVar(Help = "How recently (in seconds) a player the herd counts as one of its own must have been attacked for a bull to come after whoever is doing it; 0 stops him defending players")]
	public static float defendReactionTime = 5f;

	[ServerVar(Help = "The seconds a livestock animal credits to its owner's team and to the cupboard authed and door code sharers of its home; kept inside the tolerated band on purpose")]
	public static float trustFloor = 60f;

	[ServerVar(Help = "The seconds a livestock animal credits to a player under a livestock handling effect; kept inside the tolerated band on purpose")]
	public static float calmedTrustFloor = 60f;

	[ServerVar(Help = "How many seconds of familiarity a player loses with a livestock animal they hurt")]
	public static float familiarityHurtPenalty = 60f;

	[ServerVar(Help = "How many seconds of familiarity a player loses with the herd around an animal they hurt")]
	public static float familiarityDistressPenalty = 30f;

	[ServerVar(Help = "The share of livestock born curious, so they follow a player they know without needing a lead; 0 disables curious animals")]
	public static float curiousChance = 0.2f;

	[ServerVar(Help = "How far (in metres) a player can get before a curious livestock animal gives up following them")]
	public static float curiousBreakDistance = 20f;

	[ServerVar(Help = "The share of livestock born sleepy, so once grown they spend most of the day lying down away from the herd. An animal is never curious and sleepy at once; 0 disables sleepy animals")]
	public static float sleepyChance = 0.2f;

	[ServerVar(Help = "How far (in metres) a player can get from an animal they are leading before the lead breaks. 0 never breaks it")]
	public static float leadBreakDistance = 15f;

	[ServerVar(Help = "Adds the day budget shares to a livestock animal's ai.showState read-out")]
	public static bool debugDayBudget = false;

	[ServerVar(Help = "Adds the condition and its breeding band to a livestock animal's ai.showState read-out")]
	public static bool debugCondition = false;

	[ServerVar(Help = "Contentment at which a livestock animal is in full condition; below it the condition target falls in proportion, so a herd held at the shelter floor sits at half condition")]
	public static float conditionFullContentment = 1f;

	[ServerVar(Help = "How many seconds a perfectly happy young livestock animal takes to raise its potential from nothing to full")]
	public static float potentialRiseSeconds = 600f;

	[ServerVar(Help = "How many seconds a totally unhappy young livestock animal takes to drop its potential from full to nothing")]
	public static float potentialFallSeconds = 900f;

	[ServerVar(Help = "The most potential a livestock animal with no mother can reach, from 0 to 1. Covers animals bought from a vendor, caught wild or spawned in")]
	public static float potentialUnbredCap = 0f;

	[ServerVar(Help = "How many seconds a totally neglected livestock animal takes to fall from full health to 1 hp, or to dead when it has no personal space left")]
	public static float conditionFallSeconds = 3600f;

	[ServerVar(Help = "How many seconds a well kept livestock animal takes to heal from nothing back to full health")]
	public static float conditionRiseSeconds = 3600f;

	[ServerVar(Help = "Condition below which a livestock animal stops dunging at all. A starving animal never dungs whatever this says")]
	public static float dungConditionFloor = 0.15f;

	[ServerVar(Help = "Condition at or above which a livestock animal dungs at its full authored rate")]
	public static float dungConditionFull = 0.8f;

	[ServerVar(Help = "How much longer a livestock animal at the condition floor waits between dung drops than one in its prime")]
	public static float dungSlowestScale = 3f;

	[ServerVar(Help = "Condition at or below which a livestock animal sells for the least a vendor will pay")]
	public static float priceConditionFloor = 0.15f;

	[ServerVar(Help = "Condition at or above which a livestock animal sells for the vendor's full price")]
	public static float priceConditionFull = 0.8f;

	[ServerVar(Help = "What a livestock animal at the condition floor sells for, as a share of the vendor's full price")]
	public static float priceWorstScale = 0.4f;

	[ServerVar(Help = "How far a livestock animal's genes move what a vendor pays, where 1 pays its full gene advantage and 0 prices every animal as an average one")]
	public static float priceGeneWeight = 1f;

	[ServerVar(Help = "The livestock sale value that counts as full quality, which is a full budget for the vendor's sale table")]
	public static float priceFullQualityScale = 1.5f;

	[ServerVar(Help = "How many offers a livestock vendor makes for one animal before he is annoyed and stops buying")]
	public static int vendorOfferCount = 3;

	[ServerVar(Help = "How many items a livestock vendor picks for one offer before paying the rest of its value in the sale table's fallback")]
	public static int vendorOfferItems = 2;

	[ServerVar(Help = "How far a livestock vendor's offer can swing either side of what an animal is really worth")]
	public static float vendorOfferVariance = 0.05f;

	[ServerVar(Help = "The chance each livestock offer is a generous one, where the vendor pays for a better animal than the one in front of him; 0 disables it")]
	public static float vendorGenerousChance = 0.05f;

	[ServerVar(Help = "The least a generous livestock offer lifts an animal's quality by")]
	public static float vendorGenerousMin = 0.1f;

	[ServerVar(Help = "The most a generous livestock offer lifts an animal's quality by")]
	public static float vendorGenerousMax = 0.25f;

	[ServerVar(Help = "Potential below which a livestock vendor opens his first offer by saying the animal was neglected growing up; 0 disables it")]
	public static float vendorLowPotential = 0.5f;

	[ServerVar(Help = "How many seconds a livestock vendor refuses to look at goods whose every offer was turned down")]
	public static float vendorAnnoyedTime = 900f;

	[ServerVar(Help = "How much wool counts as a full lot, which is a full budget for the vendor's wool table")]
	public static int vendorWoolLotSize = 400;

	[ServerVar(Help = "Whether livestock genes do anything; off makes every animal an average one without touching the genomes they carry")]
	public static bool genesEnabled = true;

	[ServerVar(Help = "The chance each allele of a wild livestock animal is rolled Good")]
	public static float geneGoodChance = 0.2f;

	[ServerVar(Help = "The chance each allele of a wild livestock animal is rolled Bad; the rest come out Ok")]
	public static float geneBadChance = 0.3f;

	[ServerVar(Help = "The chance each inherited livestock allele is rerolled from scratch instead of coming from its parent; leaving this at 0 is what makes breeding predictable enough to reason about")]
	public static float geneMutationChance = 0f;

	[ServerVar(Help = "The most young one livestock birth can ever produce, however good the mother's Fertility is")]
	public static int maxLitterSize = 3;

	[ServerVar(Help = "How often a Bad Dung livestock animal drops dung, as a share of an average one")]
	public static float geneDungBad = 0.6f;

	[ServerVar(Help = "How often a Good Dung livestock animal drops dung, as a multiple of an average one")]
	public static float geneDungGood = 1.6f;

	[ServerVar(Help = "How much milk or wool a Bad Yield livestock animal gives, as a share of an average one")]
	public static float geneYieldBad = 0.6f;

	[ServerVar(Help = "How much milk or wool a Good Yield livestock animal gives, as a multiple of an average one")]
	public static float geneYieldGood = 1.6f;

	[ServerVar(Help = "How often a Bad Fertility livestock animal breeds, as a share of an average one; litter size never falls below one")]
	public static float geneFertilityBad = 0.65f;

	[ServerVar(Help = "How often a Good Fertility livestock animal breeds, and how many young it carries, as a multiple of an average one")]
	public static float geneFertilityGood = 1.6f;

	[ServerVar(Help = "What an average animal's food, water, room, grass and sun needs are divided by for a Bad Hardiness livestock animal, so below 1 it needs more of everything")]
	public static float geneHardinessBad = 0.667f;

	[ServerVar(Help = "What an average animal's food, water, room, grass and sun needs are divided by for a Good Hardiness livestock animal, so at 2 it gets by on half")]
	public static float geneHardinessGood = 2f;

	[ServerVar(Help = "Adds an animal's genes to its ai.showState read-out")]
	public static bool debugGenes = false;

	[ServerVar(Help = "The chance each wild livestock spawn is one of the named special animals; births, purchases, age-ups and saves never roll for one")]
	public static float specialChance = 0.005f;

	[ServerVar(Help = "Logs each special livestock animal as it spawns")]
	public static bool debugSpecials = false;

	[ServerVar(Help = "Whether a newborn livestock animal takes its same-sex parent's name one generation on, so a line reads Melk then Melk Junior then Melk the 3rd")]
	public static bool dynasticNames = true;

	[ServerVar(Help = "The chance a freshly named livestock animal is given a developer's handle instead of an ordinary name; one named that way never founds a dynasty")]
	public static float devNameChance = 0f;

	[ServerVar(Help = "The language a livestock animal is named in when its first owner is offline or speaks a language the game has no names for")]
	public static string namelocale = "en";

	[ServerVar(Help = "Adds walk speed and gait penalty to a livestock animal's ai.showState read-out")]
	public static bool debugGait = false;

	[ServerVar(Help = "Main thread time budget per frame in milliseconds for the livestock needs queue")]
	public static float needsFrameTime = 0.1f;

	[ServerVar(Help = "How far (in metres) around you livestock.drawpens and livestock.drawherds draw")]
	public static float drawRadius = 60f;

	[ServerVar(Help = "Times how long livestock spend in their needs queue and FSM tick, for livestock.census. Off by default because the measurement itself costs")]
	public static bool profile
	{
		get
		{
			return LivestockProfiler.Enabled;
		}
		set
		{
			LivestockProfiler.Enabled = value;
			LivestockProfiler.Reset();
		}
	}

	[ServerVar(Help = "Immediately grows the nearest livestock infant into its adult stage. Optional search radius in metres (default 20).")]
	public static void ageup(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((Object)(object)basePlayer == (Object)null)
		{
			args.ReplyWith("This command has to be run by a player.");
			return;
		}
		float num = args.GetFloat(0, 20f);
		LivestockAnimal closestLivestockAnimal = GetClosestLivestockAnimal(num, basePlayer, out var closestDistance);
		if ((Object)(object)closestLivestockAnimal == (Object)null)
		{
			args.ReplyWith($"No livestock animal found within {num}m.");
			return;
		}
		string text = $"{closestLivestockAnimal.Categorize()} ({closestLivestockAnimal.Age}) at {closestDistance:0.0}m";
		if (closestLivestockAnimal.IsAdult())
		{
			args.ReplyWith(text + " is an adult, and an adult does not age out.");
		}
		else if (closestLivestockAnimal.ForceAgeUp())
		{
			args.ReplyWith("Aged up " + text + ".");
		}
		else
		{
			args.ReplyWith(text + " did not age up - check the server console, the adult form on its Species asset is missing or failed to spawn.");
		}
	}

	[ServerVar(Help = "Makes the nearest livestock animal pregnant. Obeys standard breeding rules. Optional search radius in metres (default 20).")]
	public static void makepregnant(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((Object)(object)basePlayer == (Object)null)
		{
			args.ReplyWith("This command has to be run by a player.");
			return;
		}
		float num = args.GetFloat(0, 20f);
		LivestockAnimal closestLivestockAnimal = GetClosestLivestockAnimal(num, basePlayer, out var closestDistance);
		if ((Object)(object)closestLivestockAnimal == (Object)null)
		{
			args.ReplyWith($"No livestock animal found within {num}m.");
		}
		else if (closestLivestockAnimal.MakePregnant(null))
		{
			args.ReplyWith($"{closestLivestockAnimal.Categorize()} ({closestLivestockAnimal.Age}) at {closestDistance:0.0} is now pregnant.");
		}
		else
		{
			args.ReplyWith($"{closestLivestockAnimal.Categorize()} ({closestLivestockAnimal.Age}) at {closestDistance:0.0}m cannot be made pregnant " + $"(owned {closestLivestockAnimal.HasOwner}, willing {!closestLivestockAnimal.BreedingPaused} at condition {closestLivestockAnimal.Condition:0.00}, " + $"room to breed {closestLivestockAnimal.HasRoomToBreed}, species at its cap {closestLivestockAnimal.SpeciesAtCap}, " + $"on cooldown {closestLivestockAnimal.OnBreedingCooldown}, already pregnant {closestLivestockAnimal.IsPregnant()}).");
		}
	}

	[ServerVar(Help = "Makes the nearest pregnant livestock animal give birth now, without waiting out the rest of its pregnancy. Optional search radius in metres (default 20).")]
	public static void completepregnancy(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((Object)(object)basePlayer == (Object)null)
		{
			args.ReplyWith("This command has to be run by a player.");
			return;
		}
		float num = args.GetFloat(0, 20f);
		LivestockAnimal closestLivestockAnimal = GetClosestLivestockAnimal(num, basePlayer, out var closestDistance, (LivestockAnimal a) => a.IsPregnant());
		if ((Object)(object)closestLivestockAnimal == (Object)null)
		{
			args.ReplyWith($"No pregnant livestock animal found within {num}m.");
			return;
		}
		string text = $"{closestLivestockAnimal.Categorize()} at {closestDistance:0.0}m";
		if (closestLivestockAnimal.ForceGiveBirth())
		{
			args.ReplyWith(text + " has given birth.");
		}
		else
		{
			args.ReplyWith(text + " ended its pregnancy without a calf - check the server console, the child form on its Species asset is missing or failed to spawn.");
		}
	}

	[ServerVar(Help = "Makes the nearest livestock animal drop dung now, without waiting for its timer. Optional search radius in metres (default 20).")]
	public static void dung(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((Object)(object)basePlayer == (Object)null)
		{
			args.ReplyWith("This command has to be run by a player.");
			return;
		}
		float num = args.GetFloat(0, 20f);
		LivestockAnimal closestLivestockAnimal = GetClosestLivestockAnimal(num, basePlayer, out var closestDistance);
		if ((Object)(object)closestLivestockAnimal == (Object)null)
		{
			args.ReplyWith($"No livestock animal found within {num}m.");
		}
		else if (!closestLivestockAnimal.DungEnabled)
		{
			args.ReplyWith($"{closestLivestockAnimal.Categorize()} at {closestDistance:0.0}m has no dung item set on its prefab.");
		}
		else if (!closestLivestockAnimal.TryDropDung())
		{
			args.ReplyWith($"{closestLivestockAnimal.Categorize()} at {closestDistance:0.0}m has nothing to pass " + $"(owned {closestLivestockAnimal.HasOwner}, fullness {closestLivestockAnimal.Fullness.NeedValue:0.00}, " + $"condition {closestLivestockAnimal.Condition:0.00}).");
		}
		else
		{
			args.ReplyWith($"{closestLivestockAnimal.Categorize()} at {closestDistance:0.0}m dropped dung.");
		}
	}

	[ServerVar(Help = "Adds one stored milking to the nearest cow now, up to the most it can hold, without waiting out its cooldown. Optional search radius in metres (default 20).")]
	public static void readytomilk(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((Object)(object)basePlayer == (Object)null)
		{
			args.ReplyWith("This command has to be run by a player.");
			return;
		}
		float num = args.GetFloat(0, 20f);
		Cow cow = GetClosestLivestockAnimal(num, basePlayer, out var closestDistance, (LivestockAnimal a) => a is Cow) as Cow;
		if ((Object)(object)cow == (Object)null)
		{
			args.ReplyWith($"No cow found within {num}m.");
		}
		else if (cow.ForceMilkReady())
		{
			args.ReplyWith($"Cow at {closestDistance:0.0}m is ready to milk, now {cow.MilkCharges}/{cow.MaxMilkCharges}.");
		}
		else
		{
			args.ReplyWith($"Cow at {closestDistance:0.0}m cannot be milked - bulls, calves and unowned cows never can.");
		}
	}

	[ServerVar(Help = "Makes the nearest sheep ready to shear now, without waiting out its cooldown. Optional search radius in metres (default 20).")]
	public static void readytoshear(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((Object)(object)basePlayer == (Object)null)
		{
			args.ReplyWith("This command has to be run by a player.");
			return;
		}
		float num = args.GetFloat(0, 20f);
		Sheep sheep = GetClosestLivestockAnimal(num, basePlayer, out var closestDistance, (LivestockAnimal a) => a is Sheep) as Sheep;
		if ((Object)(object)sheep == (Object)null)
		{
			args.ReplyWith($"No sheep found within {num}m.");
		}
		else if (sheep.ForceShearReady())
		{
			args.ReplyWith($"Sheep at {closestDistance:0.0}m is ready to shear.");
		}
		else
		{
			args.ReplyWith($"Sheep at {closestDistance:0.0}m cannot be sheared - lambs never can.");
		}
	}

	[ServerVar(Help = "Fills the nearest livestock animal's hydration, as if it had just drunk its fill. Optional search radius in metres (default 20).")]
	public static void fillHydration(Arg args)
	{
		SetNeed(args, 1f, "is no longer thirsty", LivestockAnimal.Need.Hydration);
	}

	[ServerVar(Help = "Empties the nearest livestock animal's hydration, so it goes looking for water. Optional search radius in metres (default 20).")]
	public static void emptyHydration(Arg args)
	{
		SetNeed(args, 0f, "is now parched", LivestockAnimal.Need.Hydration);
	}

	[ServerVar(Help = "Empties the nearest livestock animal's fullness, so it goes looking for food. Optional search radius in metres (default 20).")]
	public static void emptyFullness(Arg args)
	{
		SetNeed(args, 0f, "is now hungry", LivestockAnimal.Need.Fullness);
	}

	[ServerVar(Help = "Fills the nearest livestock animal's fullness, as if it had just eaten its fill. Optional search radius in metres (default 20).")]
	public static void fillFullness(Arg args)
	{
		SetNeed(args, 1f, "is no longer hungry", LivestockAnimal.Need.Fullness);
	}

	private static void SetNeed(Arg args, float value, string description, LivestockAnimal.Need need)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((Object)(object)basePlayer == (Object)null)
		{
			args.ReplyWith("This command has to be run by a player.");
			return;
		}
		float num = args.GetFloat(0, 20f);
		LivestockAnimal closestLivestockAnimal = GetClosestLivestockAnimal(num, basePlayer, out var closestDistance);
		if ((Object)(object)closestLivestockAnimal == (Object)null)
		{
			args.ReplyWith($"No livestock animal found within {num}m.");
			return;
		}
		closestLivestockAnimal.SetNeedValue(need, value);
		args.ReplyWith($"{closestLivestockAnimal.Categorize()} at {closestDistance:0.0}m {description}.");
	}

	[ServerVar(Help = "Renames the nearest livestock animal. Usage: livestock.rename <name> [radius], quoting a name that contains spaces, radius in metres (default 20)")]
	public static void rename(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((Object)(object)basePlayer == (Object)null)
		{
			args.ReplyWith("This command has to be run by a player.");
			return;
		}
		string text = args.GetString(0);
		if (string.IsNullOrWhiteSpace(text))
		{
			args.ReplyWith("Provide a name, eg: livestock.rename \"Daisy\"");
			return;
		}
		float num = args.GetFloat(1, 20f);
		LivestockAnimal closestLivestockAnimal = GetClosestLivestockAnimal(num, basePlayer, out var closestDistance);
		if ((Object)(object)closestLivestockAnimal == (Object)null)
		{
			args.ReplyWith($"No livestock animal found within {num}m.");
			return;
		}
		string text2 = (string.IsNullOrEmpty(closestLivestockAnimal.AnimalName) ? "unnamed" : closestLivestockAnimal.AnimalName);
		bool hasSpecialAuthoredName = closestLivestockAnimal.HasSpecialAuthoredName;
		closestLivestockAnimal.UpdateNameFromPlayer(text, LivestockNames.HouseLocale(), basePlayer.userID);
		string text3 = (hasSpecialAuthoredName ? (" It's no longer the special " + text2 + ", so another can spawn.") : string.Empty);
		args.ReplyWith(string.Format("Renamed {0} ({1}) at {2:0.0}m from {3} to {4}.{5}", new object[6]
		{
			closestLivestockAnimal.Categorize(),
			closestLivestockAnimal.Age,
			closestDistance,
			text2,
			text,
			text3
		}));
	}

	[ServerVar(Help = "Swaps the gender of the nearest livestock animal. Optional search radius in metres (default 20).")]
	public static void swapgender(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((Object)(object)basePlayer == (Object)null)
		{
			args.ReplyWith("This command has to be run by a player.");
			return;
		}
		float num = args.GetFloat(0, 20f);
		LivestockAnimal closestLivestockAnimal = GetClosestLivestockAnimal(num, basePlayer, out var closestDistance);
		if ((Object)(object)closestLivestockAnimal == (Object)null)
		{
			args.ReplyWith($"No livestock animal found within {num}m.");
			return;
		}
		bool flag = !closestLivestockAnimal.IsMale;
		GameObjectRef gameObjectRef = closestLivestockAnimal.FormForSex(flag);
		if (gameObjectRef == null || !gameObjectRef.isValid || gameObjectRef.resourcePath == closestLivestockAnimal.PrefabName)
		{
			closestLivestockAnimal.IsMale = flag;
			args.ReplyWith(string.Format("{0} ({1}) at {2:0.0}m is now {3}.", new object[4]
			{
				closestLivestockAnimal.Categorize(),
				closestLivestockAnimal.Age,
				closestDistance,
				flag ? "male" : "female"
			}));
			return;
		}
		LivestockAnimal livestockAnimal = closestLivestockAnimal.ReplaceWith(gameObjectRef.resourcePath);
		if ((Object)(object)livestockAnimal == (Object)null)
		{
			args.ReplyWith($"Failed to swap {closestLivestockAnimal.Categorize()} at {closestDistance:0.0}m for '{gameObjectRef.resourcePath}'.");
			return;
		}
		args.ReplyWith(string.Format("{0} ({1}) at {2:0.0}m is now {3} ({4}).", new object[5]
		{
			livestockAnimal.Categorize(),
			livestockAnimal.Age,
			closestDistance,
			livestockAnimal.IsMale ? "male" : "female",
			livestockAnimal.PrefabName
		}));
	}

	[ServerVar(Help = "Prints what the nearest livestock animal's coat is made of, which is its genes and, for a species with patches, the coat seed it was born with. Usage: livestock.showcoat [radius], radius in metres (default 20)")]
	public static void showcoat(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((Object)(object)basePlayer == (Object)null)
		{
			args.ReplyWith("This command has to be run by a player.");
			return;
		}
		float num = args.GetFloat(0, 20f);
		LivestockAnimal closestLivestockAnimal = GetClosestLivestockAnimal(num, basePlayer, out var closestDistance);
		if ((Object)(object)closestLivestockAnimal == (Object)null)
		{
			args.ReplyWith($"No livestock animal found within {num}m.");
		}
		else
		{
			args.ReplyWith($"{closestLivestockAnimal.Categorize()} '{closestLivestockAnimal.NameOrLabel()}' ({closestLivestockAnimal.Age}) at " + $"{closestDistance:0.0}m wears {closestLivestockAnimal.CoatName}. Change its colour with " + "livestock.setgene.");
		}
	}

	[ServerVar(Help = "Prints the genes of the livestock animal you are looking at, or the nearest one, both copies of each, plus its lineage markers. Optional search radius in metres as the first argument (default 20).")]
	public static void showgenes(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((Object)(object)basePlayer == (Object)null)
		{
			args.ReplyWith("This command has to be run by a player.");
			return;
		}
		float num = args.GetFloat(0, 20f);
		LivestockAnimal targetedLivestockAnimal = GetTargetedLivestockAnimal(num, basePlayer, out var distance);
		if ((Object)(object)targetedLivestockAnimal == (Object)null)
		{
			args.ReplyWith($"No livestock animal found within {num}m.");
			return;
		}
		args.ReplyWith(string.Format("{0} '{1}' at {2:0.0}m: {3}", new object[4]
		{
			targetedLivestockAnimal.Categorize(),
			targetedLivestockAnimal.NameOrLabel(),
			distance,
			targetedLivestockAnimal.GetGenesDebugInfo()
		}));
	}

	[ServerVar(Help = "Sets both copies of one gene on the livestock animal you are looking at, or the nearest one, the better copy being the one that shows. Usage: livestock.setgene <Dung|Yield|Fertility|Hardiness> <firstAllele> <secondAllele> [radius], each allele Bad, Ok or Good")]
	public static void setgene(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((Object)(object)basePlayer == (Object)null)
		{
			args.ReplyWith("This command has to be run by a player.");
			return;
		}
		if (!Enum.TryParse<LivestockGene>(args.GetString(0, string.Empty), ignoreCase: true, out var result) || !LivestockGenome.IsLive(result))
		{
			args.ReplyWith("Name a gene: Dung, Yield, Fertility or Hardiness.");
			return;
		}
		if (!Enum.TryParse<LivestockAllele>(args.GetString(1, string.Empty), ignoreCase: true, out var result2) || !Enum.TryParse<LivestockAllele>(args.GetString(2, string.Empty), ignoreCase: true, out var result3))
		{
			args.ReplyWith("Name both alleles: Bad, Ok or Good, eg: livestock.setgene Yield Good Bad");
			return;
		}
		float num = args.GetFloat(3, 20f);
		LivestockAnimal targetedLivestockAnimal = GetTargetedLivestockAnimal(num, basePlayer, out var distance);
		if ((Object)(object)targetedLivestockAnimal == (Object)null)
		{
			args.ReplyWith($"No livestock animal found within {num}m.");
			return;
		}
		targetedLivestockAnimal.SetGene(result, result2, result3);
		args.ReplyWith(string.Format("{0} at {1:0.0}m now carries {2} {3}/{4}, showing {5}.", new object[6]
		{
			targetedLivestockAnimal.Categorize(),
			distance,
			result,
			result2,
			result3,
			targetedLivestockAnimal.Expressed(result)
		}));
	}

	[ServerVar(Help = "Makes the livestock animal you are looking at, or the nearest one, inbred or not, by forcing its two lineage markers together or apart. Usage: livestock.setinbred <0|1> [radius]")]
	public static void setinbred(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((Object)(object)basePlayer == (Object)null)
		{
			args.ReplyWith("This command has to be run by a player.");
			return;
		}
		bool flag = args.GetBool(0, def: true);
		float num = args.GetFloat(1, 20f);
		LivestockAnimal targetedLivestockAnimal = GetTargetedLivestockAnimal(num, basePlayer, out var distance);
		if ((Object)(object)targetedLivestockAnimal == (Object)null)
		{
			args.ReplyWith($"No livestock animal found within {num}m.");
			return;
		}
		targetedLivestockAnimal.SetLineageMarkers(0, (!flag) ? 1 : 0);
		args.ReplyWith(string.Format("{0} at {1:0.0}m is now {2}.", targetedLivestockAnimal.Categorize(), distance, targetedLivestockAnimal.IsInbred ? "inbred" : "not inbred"));
	}

	[ServerVar(Help = "Sets how many seconds of familiarity the calling player has with the livestock animal they are looking at, or the nearest one. Usage: livestock.setfamiliarity <seconds> [radius], radius in metres (default 20)")]
	public static void setfamiliarity(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((Object)(object)basePlayer == (Object)null)
		{
			args.ReplyWith("This command has to be run by a player.");
			return;
		}
		float num = args.GetFloat(0, -1f);
		float num2 = args.GetFloat(1, 20f);
		if (num < 0f)
		{
			args.ReplyWith("Provide a number of seconds, eg: livestock.setfamiliarity 300");
			return;
		}
		LivestockAnimal targetedLivestockAnimal = GetTargetedLivestockAnimal(num2, basePlayer, out var distance);
		if ((Object)(object)targetedLivestockAnimal == (Object)null)
		{
			args.ReplyWith($"No livestock animal found within {num2}m.");
			return;
		}
		targetedLivestockAnimal.SetFamiliarity(basePlayer.userID, num);
		float num3 = targetedLivestockAnimal.StoredSecondsFor(basePlayer.userID);
		args.ReplyWith(string.Format("{0} ({1}) at {2:0.0}m now has {3:0}s of familiarity with you ({4}).", new object[5]
		{
			targetedLivestockAnimal.Categorize(),
			targetedLivestockAnimal.Age,
			distance,
			num3,
			LivestockAnimal.TrustTierName(num3)
		}));
	}

	[ServerVar(Help = "Sets how many seconds of familiarity the calling player has with every livestock animal in range. 0 resets them all. Usage: livestock.setfamiliarityall <seconds> [radius], radius in metres (default 30)")]
	public static void setfamiliarityall(Arg args)
	{
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((Object)(object)basePlayer == (Object)null)
		{
			args.ReplyWith("This command has to be run by a player.");
			return;
		}
		float num = args.GetFloat(0, -1f);
		float num2 = args.GetFloat(1, 30f);
		if (num < 0f)
		{
			args.ReplyWith("Provide a number of seconds, eg: livestock.setfamiliarityall 0");
			return;
		}
		int num3 = 0;
		LivestockAnimal[] array = BaseEntity.Util.FindAll<LivestockAnimal>();
		foreach (LivestockAnimal livestockAnimal in array)
		{
			if (!((Object)(object)livestockAnimal == (Object)null) && !livestockAnimal.IsDestroyed && !livestockAnimal.IsDead() && !(Vector3.Distance(((Component)livestockAnimal).transform.position, ((Component)basePlayer).transform.position) > num2))
			{
				livestockAnimal.SetFamiliarity(basePlayer.userID, num);
				num3++;
			}
		}
		args.ReplyWith(string.Format("{0} livestock animal(s) within {1}m now have {2:0}s of familiarity with you ({3}).", new object[4]
		{
			num3,
			num2,
			num,
			LivestockAnimal.TrustTierName(num)
		}));
	}

	[ServerVar(Help = "Makes the nearest livestock animal curious or not, so it will follow a player it knows without a lead. Setting it clears sleepy. Usage: livestock.setcurious <0|1> [radius], radius in metres (default 20)")]
	public static void setcurious(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((Object)(object)basePlayer == (Object)null)
		{
			args.ReplyWith("This command has to be run by a player.");
			return;
		}
		bool flag = args.GetInt(0, 1) != 0;
		float num = args.GetFloat(1, 20f);
		LivestockAnimal targetedLivestockAnimal = GetTargetedLivestockAnimal(num, basePlayer, out var distance);
		if ((Object)(object)targetedLivestockAnimal == (Object)null)
		{
			args.ReplyWith($"No livestock animal found within {num}m.");
			return;
		}
		targetedLivestockAnimal.SetCurious(flag);
		string text = (targetedLivestockAnimal.ActsCurious ? "and will follow you once you are familiar enough" : "but only cows and calves act on it");
		args.ReplyWith(string.Format("{0} ({1}) at {2:0.0}m is {3} curious, {4}.", new object[5]
		{
			targetedLivestockAnimal.Categorize(),
			targetedLivestockAnimal.Age,
			distance,
			flag ? "now" : "no longer",
			text
		}));
	}

	[ServerVar(Help = "Makes the nearest livestock animal sleepy or not, so once grown it spends most of the day lying down away from the herd. Setting it clears curious. Usage: livestock.setsleepy <0|1> [radius], radius in metres (default 20)")]
	public static void setsleepy(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((Object)(object)basePlayer == (Object)null)
		{
			args.ReplyWith("This command has to be run by a player.");
			return;
		}
		bool flag = args.GetInt(0, 1) != 0;
		float num = args.GetFloat(1, 20f);
		LivestockAnimal targetedLivestockAnimal = GetTargetedLivestockAnimal(num, basePlayer, out var distance);
		if ((Object)(object)targetedLivestockAnimal == (Object)null)
		{
			args.ReplyWith($"No livestock animal found within {num}m.");
			return;
		}
		targetedLivestockAnimal.SetSleepy(flag);
		string text;
		if (flag)
		{
			text = (targetedLivestockAnimal.ActsSleepy ? ", and rests away from the herd" : ", but acts on it only once grown");
		}
		else
		{
			text = string.Empty;
		}
		args.ReplyWith(string.Format("{0} ({1}) at {2:0.0}m is {3} sleepy{4}.", new object[5]
		{
			targetedLivestockAnimal.Categorize(),
			targetedLivestockAnimal.Age,
			distance,
			flag ? "now" : "no longer",
			text
		}));
	}

	[ServerVar(Help = "Sends the nearest male livestock animal to mate with the nearest valid female, skipping the mate search. Optional search radius in metres (default 20).")]
	public static void forcebreed(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((Object)(object)basePlayer == (Object)null)
		{
			args.ReplyWith("This command has to be run by a player.");
			return;
		}
		float num = args.GetFloat(0, 20f);
		LivestockAnimal male = GetClosestLivestockAnimal(num, basePlayer, out var closestDistance, (LivestockAnimal a) => a.IsAdult() && a.IsMale);
		if ((Object)(object)male == (Object)null)
		{
			args.ReplyWith($"No adult male livestock animal found within {num}m.");
			return;
		}
		LivestockAnimalFSM livestockAnimalFSM = default;
		if (!((Component)male).TryGetComponent<LivestockAnimalFSM>(ref livestockAnimalFSM))
		{
			args.ReplyWith($"{male.Categorize()} at {closestDistance:0.0}m has no LivestockAnimalFSM.");
			return;
		}
		LivestockAnimal closestLivestockAnimal = GetClosestLivestockAnimal(num, basePlayer, out var closestDistance2, (LivestockAnimal a) => (Object)(object)a != (Object)(object)male && a.IsSameSpeciesAs(male) && a.CanBePregnant());
		if ((Object)(object)closestLivestockAnimal == (Object)null)
		{
			args.ReplyWith($"No {male.Categorize()} that can be made pregnant found within {num}m.");
			return;
		}
		livestockAnimalFSM.MateTarget.Set(closestLivestockAnimal);
		livestockAnimalFSM.SetState(livestockAnimalFSM.seekMate);
		args.ReplyWith(string.Format("{0} at {1:0.0}m is now approaching the {2} at {3:0.0}m.", new object[4]
		{
			male.Categorize(),
			closestDistance,
			closestLivestockAnimal.Categorize(),
			closestDistance2
		}));
	}

	[ServerVar(Help = "Moves the home of every livestock animal in range to where the caller is standing, so roam and the night gather aim here until it next eats at a trough. Optional radius in metres (default 30).")]
	public static void sendhome(Arg args)
	{
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((Object)(object)basePlayer == (Object)null)
		{
			args.ReplyWith("This command has to be run by a player.");
			return;
		}
		float num = args.GetFloat(0, 30f);
		int num2 = 0;
		LivestockAnimal[] array = BaseEntity.Util.FindAll<LivestockAnimal>();
		foreach (LivestockAnimal livestockAnimal in array)
		{
			if (!((Object)(object)livestockAnimal == (Object)null) && !livestockAnimal.IsDestroyed && !livestockAnimal.IsDead() && !(Vector3.Distance(((Component)livestockAnimal).transform.position, ((Component)basePlayer).transform.position) > num))
			{
				livestockAnimal.SetHomePosition(((Component)basePlayer).transform.position);
				num2++;
			}
		}
		args.ReplyWith($"{num2} livestock animal(s) within {num}m now anchor here.");
	}

	[ServerVar(Help = "Draws an arrow from the livestock animal you are looking at, or the nearest one, to its home: the last trough it ate at, or where it started. Green when that is inside a base. Optional search radius in metres (default 20) and seconds to draw for (default 10).")]
	public static void showhome(Arg args)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((Object)(object)basePlayer == (Object)null)
		{
			args.ReplyWith("This command has to be run by a player.");
			return;
		}
		float num = args.GetFloat(0, 20f);
		float num2 = args.GetFloat(1, 10f);
		LivestockAnimal targetedLivestockAnimal = GetTargetedLivestockAnimal(num, basePlayer, out var _);
		if ((Object)(object)targetedLivestockAnimal == (Object)null)
		{
			args.ReplyWith($"No livestock animal found within {num}m.");
			return;
		}
		Vector3 val = targetedLivestockAnimal.CenterPoint();
		Vector3 homePosition = targetedLivestockAnimal.HomePosition;
		bool flag = targetedLivestockAnimal.TryGetHomeCupboard(out var cupboard);
		Color val2 = (flag ? Color.green : Color.yellow);
		basePlayer.SendConsoleCommand("ddraw.arrow", num2, val2, val, homePosition, 0.5f);
		basePlayer.SendConsoleCommand("ddraw.sphere", num2, val2, homePosition, 1f);
		string arg = (flag ? $"in the base of cupboard {cupboard.net.ID}" : "with no cupboard");
		args.ReplyWith($"{targetedLivestockAnimal.Categorize()} lives {arg}, {Vector3.Distance(val, homePosition):0}m away.");
	}

	[ServerVar(Help = "Toggles drawing every livestock pen within livestock.drawRadius of you, each in its own colour: its border, a marker on each animal on it and each trough and water container it keeps, and what it houses. Pass 1 or 0 to set it instead.")]
	public static void drawpens(Arg args)
	{
		DebugDrawOverlayViewer.Toggle(args, LivestockPenOverlay.Instance, $"livestock pens within {drawRadius:0}m");
	}

	[ServerVar(Help = "Toggles drawing the free range herds within livestock.drawRadius of you, each in its own colour: the whole herd the count links through livestock.freeRangeLinkRadius, past the cap, joined by its shortest set of lines, and its size against livestock.maxFreeRangeHerd with how many of it are not owned. Pass 1 or 0 to set it instead.")]
	public static void drawherds(Arg args)
	{
		DebugDrawOverlayViewer.Toggle(args, LivestockHerdOverlay.Instance, $"free range herds within {drawRadius:0}m");
	}

	[ServerVar(Help = "Sets the condition of the livestock animal the caller is looking at, or the nearest one. Condition is the slow average that price, dung and willingness read. Usage: livestock.setcondition <0-1> [radius], radius in metres (default 20)")]
	public static void setcondition(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((Object)(object)basePlayer == (Object)null)
		{
			args.ReplyWith("This command has to be run by a player.");
			return;
		}
		float num = args.GetFloat(0, -1f);
		float num2 = args.GetFloat(1, 20f);
		if (num < 0f)
		{
			args.ReplyWith("Provide a condition from 0 to 1, eg: livestock.setcondition 0.3");
			return;
		}
		LivestockAnimal targetedLivestockAnimal = GetTargetedLivestockAnimal(num2, basePlayer, out var distance);
		if ((Object)(object)targetedLivestockAnimal == (Object)null)
		{
			args.ReplyWith($"No livestock animal found within {num2}m.");
			return;
		}
		targetedLivestockAnimal.SetCondition(num);
		string text = (targetedLivestockAnimal.BreedingPaused ? ", and is not willing to breed" : "");
		args.ReplyWith(string.Format("{0} ({1}) at {2:0.0}m is now in condition {3:0.00}{4}.", new object[5]
		{
			targetedLivestockAnimal.Categorize(),
			targetedLivestockAnimal.Age,
			distance,
			targetedLivestockAnimal.Condition,
			text
		}));
	}

	[ServerVar(Help = "Sets the potential of the livestock animal the caller is looking at, or the nearest one, as if it was raised that content. Usage: livestock.setpotential <0-1> [radius], radius in metres (default 20)")]
	public static void setpotential(Arg args)
	{
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((Object)(object)basePlayer == (Object)null)
		{
			args.ReplyWith("This command has to be run by a player.");
			return;
		}
		float num = args.GetFloat(0, -1f);
		float num2 = args.GetFloat(1, 20f);
		if (num < 0f)
		{
			args.ReplyWith("Provide a potential from 0 to 1, eg: livestock.setpotential 0.8");
			return;
		}
		LivestockAnimal targetedLivestockAnimal = GetTargetedLivestockAnimal(num2, basePlayer, out var distance);
		if ((Object)(object)targetedLivestockAnimal == (Object)null)
		{
			args.ReplyWith($"No livestock animal found within {num2}m.");
			return;
		}
		targetedLivestockAnimal.SetPotential(num);
		args.ReplyWith(string.Format("{0} ({1}) at {2:0.0}m now has potential {3:0.00}.", new object[4]
		{
			targetedLivestockAnimal.Categorize(),
			targetedLivestockAnimal.Age,
			distance,
			targetedLivestockAnimal.Potential
		}));
	}

	[ServerVar(Help = "Counts the livestock population and what it has produced. Optional radius in metres around the caller (default 0, the whole map).")]
	public static void census(Arg args)
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		float num = args.GetFloat(0);
		BasePlayer basePlayer = ArgEx.Player(args);
		LivestockCensus.Snapshot snapshot = LivestockCensus.Sample(((Object)(object)basePlayer != (Object)null) ? ((Component)basePlayer).transform.position : Vector3.zero, num);
		string text = ((num > 0f) ? $"within {num:0}m" : "on the whole map");
		if (snapshot.Count == 0)
		{
			args.ReplyWith("No livestock " + text + ".");
			return;
		}
		string text2 = $"cond {snapshot.MeanCondition:0.00}/{snapshot.MinCondition:0.00}, breeding paused {snapshot.BreedingPaused}";
		string text3 = $"full {snapshot.MeanFullness:0.00}/{snapshot.MinFullness:0.00}" + $" hyd {snapshot.MeanHydration:0.00}/{snapshot.MinHydration:0.00}" + $" room {snapshot.MeanPersonalSpace:0.00}/{snapshot.MinPersonalSpace:0.00}" + $" grass {snapshot.MeanGrass:0.00}/{snapshot.MinGrass:0.00}" + $" sun {snapshot.MeanSun:0.00}/{snapshot.MinSun:0.00}" + $" mood {snapshot.MeanContentment:0.00}/{snapshot.MinContentment:0.00}";
		string arg = $"crowding {LivestockCensus.DeathsBy(LivestockCensus.DeathCause.Crowding)}" + $" killed {LivestockCensus.DeathsBy(LivestockCensus.DeathCause.Killed)}" + $" other {LivestockCensus.DeathsBy(LivestockCensus.DeathCause.Other)}";
		string text4 = (profile ? ($"needs {LivestockProfiler.MicrosecondsPerFrame(LivestockProfiler.Section.Needs):0.0}us/frame" + $" fsm {LivestockProfiler.MicrosecondsPerFrame(LivestockProfiler.Section.Fsm):0.0}us/frame" + $", of the needs floods {LivestockProfiler.CallsMeasured(LivestockProfiler.Section.Floor)} at {LivestockProfiler.MicrosecondsPerCall(LivestockProfiler.Section.Floor):0.0}us each" + $", grass and sun classifies and folds {LivestockProfiler.CallsMeasured(LivestockProfiler.Section.Classify)} at {LivestockProfiler.MicrosecondsPerCall(LivestockProfiler.Section.Classify):0.0}us each" + $", spot footprint gathers {LivestockProfiler.CallsMeasured(LivestockProfiler.Section.Gather)} at {LivestockProfiler.MicrosecondsPerCall(LivestockProfiler.Section.Gather):0.0}us each" + $" and picks {LivestockProfiler.CallsMeasured(LivestockProfiler.Section.Pick)} at {LivestockProfiler.MicrosecondsPerCall(LivestockProfiler.Section.Pick):0.0}us each" + $" over {LivestockProfiler.FramesMeasured} frames") : "off, set livestock.profile 1 to measure");
		args.ReplyWith("Livestock census " + text + ":" + string.Format("\n  head {0} (cattle {1}, sheep {2}, other {3})", new object[4] { snapshot.Count, snapshot.Cattle, snapshot.Sheep, snapshot.OtherSpecies }) + string.Format("\n  stage adult {0}, infant {1}, male {2}, female {3}, pregnant {4}, on a lead {5}", new object[6] { snapshot.Adults, snapshot.Infants, snapshot.Males, snapshot.Females, snapshot.Pregnant, snapshot.Leading }) + "\n  needs mean/min " + text3 + "\n  condition mean/min " + text2 + string.Format("\n  day graze {0:P0} rest {1:P0}, asleep now {2}, starved {3}", new object[4] { snapshot.MeanGrazeShare, snapshot.MeanRestShare, snapshot.Sleeping, snapshot.Starved }) + string.Format("\n  room target {0:0.00}, in a field {1}, in a pen {2} at {3:0.0}m2 a head", new object[4] { snapshot.MeanPackingTarget, snapshot.InAField, snapshot.InAPen, snapshot.MeanFloorPerHead }) + $"\n  spread herd radius {snapshot.HerdRadius:0.0}m" + $"\n  since reset births {LivestockCensus.Births}, deaths {arg}" + $"\n  produced dung {LivestockCensus.DungDropped}, trough items eaten {LivestockCensus.TroughItemsEaten}" + "\n  server cost " + text4);
	}

	[ServerVar(Help = "Zeroes the livestock census running totals and the profiler, so the next reading covers only what happens from now.")]
	public static void censusreset(Arg args)
	{
		LivestockCensus.Reset();
		LivestockProfiler.Reset();
		args.ReplyWith("Livestock census totals and profiler reset.");
	}

	[ServerVar(Help = "Kills livestock past livestock.maxPerSpecies, cattle and sheep counted apart: unowned animals first, then owned ones, the worst kept and worst bred first in each. An optional head count culls down to that instead.")]
	public static void cullexcess(Arg args)
	{
		int num = args.GetInt(0, maxPerSpecies);
		if (num <= 0)
		{
			args.ReplyWith("There is no cap to cull down to, livestock.maxPerSpecies is 0. Give a head count instead.");
			return;
		}
		int num2 = LivestockAnimal.CullPast(num);
		args.ReplyWith($"Culled {num2} livestock, leaving at most {num} of each species.");
	}

	[ServerVar(Help = "Spawns one of the named special livestock animals in front of you, skipping the rarity roll but obeying every other rule. Usage: livestock.spawnspecial <name> [species], species being needed only for an entry that fits more than one")]
	public static void spawnspecial(Arg args)
	{
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0119: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0141: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_014b: Unknown result type (might be due to invalid IL or missing references)
		BasePlayer basePlayer = ArgEx.Player(args);
		if ((Object)(object)basePlayer == (Object)null)
		{
			args.ReplyWith("This command has to be run by a player.");
			return;
		}
		LivestockSpecialTable instance = LivestockSpecialTable.Instance;
		if ((Object)(object)instance == (Object)null || instance.Entries == null || instance.Entries.Length == 0)
		{
			args.ReplyWith("There is no livestock specials table loaded.");
			return;
		}
		string text = args.GetString(0);
		LivestockSpecial livestockSpecial = instance.Find(text);
		if (livestockSpecial == null)
		{
			args.ReplyWith("No special livestock animal is called '" + text + "'.");
			return;
		}
		LivestockSpecies livestockSpecies = ResolveSpecialSpecies(instance, livestockSpecial, args.GetString(1), out var refusal);
		if ((Object)(object)livestockSpecies == (Object)null)
		{
			args.ReplyWith(refusal);
			return;
		}
		if (livestockSpecial.Sex == LivestockAnimal.SexForm.Either)
		{
			args.ReplyWith(livestockSpecial.Name + " names no definite sex, so there is no prefab to pick.");
			return;
		}
		GameObjectRef gameObjectRef = livestockSpecies.AdultFor(livestockSpecial.Sex == LivestockAnimal.SexForm.Male);
		if (gameObjectRef == null || !gameObjectRef.isValid)
		{
			args.ReplyWith($"{((Object)livestockSpecies).name} names no adult {livestockSpecial.Sex} prefab for {livestockSpecial.Name} to spawn as.");
			return;
		}
		Vector3 pos = ((Component)basePlayer).transform.position + basePlayer.eyes.BodyForward() * 3f;
		LivestockAnimal.QueueNext(livestockSpecial);
		BaseEntity baseEntity = GameManager.server.CreateEntity(gameObjectRef.resourcePath, pos);
		if ((Object)(object)baseEntity == (Object)null)
		{
			LivestockAnimal.QueueNext(null);
			args.ReplyWith("Could not create " + gameObjectRef.resourcePath + ".");
			return;
		}
		baseEntity.Spawn();
		if (baseEntity is LivestockAnimal livestockAnimal && livestockAnimal.AnimalName == livestockSpecial.Name)
		{
			args.ReplyWith("Spawned " + livestockAnimal.Categorize() + " '" + livestockAnimal.NameOrLabel() + "' in coat " + livestockAnimal.CoatName + ": " + livestockAnimal.GetGenesDebugInfo());
		}
		else
		{
			args.ReplyWith(livestockSpecial.Name + " was refused, so an ordinary " + ((Object)livestockSpecies).name + " spawned instead. One of that name is most likely already alive.");
		}
	}

	[ServerVar(Help = "Lists the special livestock animals this server has already produced, and counts the rest without naming them")]
	public static void specials(Arg args)
	{
		LivestockSpecialTable instance = LivestockSpecialTable.Instance;
		if ((Object)(object)instance == (Object)null || instance.Entries == null || instance.Entries.Length == 0)
		{
			args.ReplyWith("There is no livestock specials table loaded.");
			return;
		}
		BasePlayer player = ArgEx.Player(args);
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		LivestockSpecial[] entries = instance.Entries;
		foreach (LivestockSpecial livestockSpecial in entries)
		{
			if (livestockSpecial == null)
			{
				continue;
			}
			if (!LivestockAnimal.IsSpecialDiscovered(livestockSpecial.Name))
			{
				num++;
				continue;
			}
			if (stringBuilder.Length > 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.Append(livestockSpecial.Name).Append(": ").Append(DescribeSpecial(livestockSpecial))
				.Append(" - ")
				.Append(FindSpecialInWorld(livestockSpecial.Name, player));
		}
		if (num > 0)
		{
			string value = ((stringBuilder.Length > 0) ? " more" : string.Empty);
			if (stringBuilder.Length > 0)
			{
				stringBuilder.AppendLine();
			}
			stringBuilder.Append(num).Append(value).Append((num == 1) ? " has" : " have")
				.Append(" not been found on this server.");
		}
		args.ReplyWith(stringBuilder.ToString());
	}

	private static string DescribeSpecial(LivestockSpecial special)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(special.Sex);
		if (special.Species != null && special.Species.Length != 0)
		{
			stringBuilder.Append(' ');
			for (int i = 0; i < special.Species.Length; i++)
			{
				if (i > 0)
				{
					stringBuilder.Append('/');
				}
				stringBuilder.Append(((Object)(object)special.Species[i] != (Object)null) ? ((Object)special.Species[i]).name : "null");
			}
		}
		else
		{
			stringBuilder.Append(" any species");
		}
		if (special.Genes != null)
		{
			ForcedGene[] genes = special.Genes;
			foreach (ForcedGene forcedGene in genes)
			{
				if (forcedGene != null && LivestockGenome.IsLive(forcedGene.Gene))
				{
					stringBuilder.Append(", ").Append(forcedGene.Gene).Append(' ')
						.Append(forcedGene.First.ToString()[0])
						.Append(forcedGene.Second.ToString()[0]);
				}
			}
		}
		stringBuilder.Append(", weight ").Append(special.Weight.ToString("0.##"));
		return stringBuilder.ToString();
	}

	private static string FindSpecialInWorld(string name, BasePlayer player)
	{
		//IL_0037: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		LivestockAnimal livestockAnimal = LivestockAnimal.FindLivingSpecial(name);
		if ((Object)(object)livestockAnimal == (Object)null)
		{
			return "found earlier this wipe, not in the world";
		}
		if ((Object)(object)player == (Object)null || !debugSpecials)
		{
			return "ALIVE";
		}
		return $"ALIVE at {Vector3.Distance(((Component)livestockAnimal).transform.position, ((Component)player).transform.position):0}m";
	}

	private static LivestockSpecies ResolveSpecialSpecies(LivestockSpecialTable table, LivestockSpecial special, string named, out string refusal)
	{
		refusal = null;
		LivestockSpecies[] array = ((special.Species != null && special.Species.Length != 0) ? special.Species : CollectEverySpecies(table));
		if (array.Length == 0)
		{
			refusal = special.Name + " names no species and no other entry does either, so there is nothing to spawn it as.";
			return null;
		}
		if (!string.IsNullOrEmpty(named))
		{
			LivestockSpecies[] array2 = array;
			foreach (LivestockSpecies livestockSpecies in array2)
			{
				if ((Object)(object)livestockSpecies != (Object)null && string.Equals(((Object)livestockSpecies).name, named, StringComparison.OrdinalIgnoreCase))
				{
					return livestockSpecies;
				}
			}
			refusal = special.Name + " cannot be a " + named + ". It takes " + NameEach(array) + ".";
			return null;
		}
		if (array.Length == 1)
		{
			return array[0];
		}
		refusal = special.Name + " fits " + NameEach(array) + ", so name one: livestock.spawnspecial " + special.Name + " <species>";
		return null;
	}

	private static LivestockSpecies[] CollectEverySpecies(LivestockSpecialTable table)
	{
		List<LivestockSpecies> list = new List<LivestockSpecies>();
		LivestockSpecial[] entries = table.Entries;
		foreach (LivestockSpecial livestockSpecial in entries)
		{
			if (livestockSpecial == null || livestockSpecial.Species == null)
			{
				continue;
			}
			LivestockSpecies[] species = livestockSpecial.Species;
			foreach (LivestockSpecies livestockSpecies in species)
			{
				if ((Object)(object)livestockSpecies != (Object)null && !list.Contains(livestockSpecies))
				{
					list.Add(livestockSpecies);
				}
			}
		}
		return list.ToArray();
	}

	private static string NameEach(LivestockSpecies[] species)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < species.Length; i++)
		{
			if (i > 0)
			{
				stringBuilder.Append(" or ");
			}
			stringBuilder.Append(((Object)(object)species[i] != (Object)null) ? ((Object)species[i]).name : "null");
		}
		return stringBuilder.ToString();
	}

	private static LivestockAnimal GetTargetedLivestockAnimal(float radius, BasePlayer player, out float distance, Func<LivestockAnimal, bool> filter = null)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		RaycastHit val = default;
		if (Physics.Raycast(player.eyes.HeadRay(), ref val, radius, 1218914561, (QueryTriggerInteraction)2))
		{
			LivestockAnimal livestockAnimal = GameObjectEx.ToBaseEntity(((Component)val.collider).gameObject) as LivestockAnimal;
			if ((Object)(object)livestockAnimal != (Object)null && !livestockAnimal.IsDestroyed && !livestockAnimal.IsDead() && (filter == null || filter(livestockAnimal)))
			{
				distance = val.distance;
				return livestockAnimal;
			}
		}
		return GetClosestLivestockAnimal(radius, player, out distance, filter);
	}

	private static LivestockAnimal GetClosestLivestockAnimal(float radius, BasePlayer player, out float closestDistance, Func<LivestockAnimal, bool> filter = null)
	{
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		LivestockAnimal result = null;
		closestDistance = radius;
		LivestockAnimal[] array = BaseEntity.Util.FindAll<LivestockAnimal>();
		foreach (LivestockAnimal livestockAnimal in array)
		{
			if (!((Object)(object)livestockAnimal == (Object)null) && !livestockAnimal.IsDestroyed && !livestockAnimal.IsDead() && (filter == null || filter(livestockAnimal)))
			{
				float num = Vector3.Distance(((Component)livestockAnimal).transform.position, ((Component)player).transform.position);
				if (!(num > closestDistance))
				{
					result = livestockAnimal;
					closestDistance = num;
				}
			}
		}
		return result;
	}
}
