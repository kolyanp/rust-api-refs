using UnityEngine;

public class PoolCueViewModel : BaseViewModel
{
	[Tooltip("One cue per seat, all parented to pool_cue_root. Only the local player's own cue is left active.")]
	[SerializeField]
	private GameObject[] seatCues;

	public void ShowSeatCue(int seat)
	{
		ShowSeatCue(seatCues, seat);
	}

	public static void ShowSeatCue(GameObject[] cues, int seat)
	{
		for (int i = 0; i < cues.Length; i++)
		{
			if ((Object)(object)cues[i] != (Object)null)
			{
				cues[i].SetActive(i == seat);
			}
		}
	}
}
