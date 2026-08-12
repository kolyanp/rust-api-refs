using System;
using Rust.UI;
using UnityEngine.UI;

public class UIClanRank : BaseMonoBehaviour
{
	public static readonly Phrase MoveUpFailure;

	public static readonly Phrase MoveDownFailure;

	public static readonly Phrase DeleteRankFailure;

	public static readonly Phrase DeleteRankNotEmpty;

	private static readonly Memoized<string, int> IndexToString;

	public Image Highlight;

	public RustText IndexLabel;

	public RustText Name;

	public RustButton MoveUpButton;

	public RustButton MoveDownButton;

	public RustButton DeleteButton;

	static UIClanRank()
	{
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		MoveUpFailure = (Phrase)(object)new TokenisedPhrase("clan.move_rank_up.fail", "Failed to move the rank up.");
		MoveDownFailure = (Phrase)(object)new TokenisedPhrase("clan.move_rank_down.fail", "Failed to move the rank down.");
		DeleteRankFailure = (Phrase)(object)new TokenisedPhrase("clan.delete_rank.fail", "Failed to delete the rank.");
		DeleteRankNotEmpty = new Phrase("clan.delete_rank.not_empty", "Some clan members are still be assigned this rank. You will need to assign them to a different rank before you can delete this one.");
		IndexToString = new Memoized<string, int>((Func<int, string>)((int i) => (i + 1).ToString("G")));
	}
}
