using Rust.UI;

public class UIClanRankCreator : BaseMonoBehaviour
{
	public static readonly Phrase CreateRankFailure;

	public static readonly Phrase CreateRankDuplicate;

	public static readonly Phrase CreateRankInvalidLength;

	public static readonly Phrase CreateRankNameInvalidCharacters;

	public static readonly Phrase CreateRankNameInvalid;

	public UIClans UiClans;

	public RustInput RankName;

	public RustButton Submit;

	static UIClanRankCreator()
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
		CreateRankFailure = new Phrase("clan.create_rank.fail", "Failed to create the new rank.");
		CreateRankDuplicate = new Phrase("clan.create_rank.duplicate", "There is already a rank in your clan with that name.");
		CreateRankInvalidLength = new Phrase("clan.create_rank.name_invalid_length", "Clan rank names must be between {0} and {1} characters long.");
		CreateRankNameInvalidCharacters = new Phrase("clan.create_rank.name_invalid_characters", "Clan rank names can only contain letters, numbers, spaces, apostrophes, and hyphens.");
		CreateRankNameInvalid = new Phrase("clan.create_rank.name_invalid", "The clan rank name you typed in is not valid.");
	}
}
