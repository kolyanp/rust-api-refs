using Rust.UI;
using UnityEngine;
using UnityEngine.UI;

public class VitalInfo : MonoBehaviour, IClientComponent, IVitalNotice
{
	public enum Vital
	{
		BuildingBlocked,
		CanBuild,
		Crafting,
		CraftLevel1,
		CraftLevel2,
		CraftLevel3,
		DecayProtected,
		Decaying,
		SafeZone,
		Buffed,
		Pet,
		ModifyClan,
		DartEffects,
		NegativeEffects,
		RaidBlocked,
		RaidEnabled,
		RentDue,
		CombatZone
	}

	public HudElement Element;

	public Image InfoImage;

	public Vital VitalType;

	public RustText text;

	public static readonly Phrase BoostActive;

	public static readonly Phrase BoostsActive;

	public static readonly Phrase DartEffectActive;

	public static readonly Phrase DartEffectsActive;

	public static readonly Phrase NegativeEffectActive;

	public static readonly Phrase NegativeEffectsActive;

	public static readonly Phrase RaidOpensAt;

	static VitalInfo()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		BoostActive = new Phrase("tea.boostactive", "{0} Boost active");
		BoostsActive = new Phrase("tea.boostactive.plural", "{0} Boosts active");
		DartEffectActive = new Phrase("dart.effectactive", "{0} dart effect");
		DartEffectsActive = new Phrase("dart.effectactive.plural", "{0} dart effects");
		NegativeEffectActive = new Phrase("negative.active", "{0} negative effect");
		NegativeEffectsActive = new Phrase("negative.active.plural", "{0} negative effects");
		RaidOpensAt = new Phrase("raid.opensat", "Raiding opens at {0}");
	}
}
