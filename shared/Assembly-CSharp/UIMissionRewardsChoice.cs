using System;
using Facepunch;
using Rust.UI;
using UnityEngine;

public class UIMissionRewardsChoice : MonoBehaviour
{
	public VirtualItemIcon[] RewardIcons;

	public UIMissionNonItemReward[] NonItemRewards;

	public GameObject CompleteMissionRoot;

	public RustText CompleteMissionText;

	public void SetChildrenActiveState(bool isActive)
	{
		VirtualItemIcon[] rewardIcons = RewardIcons;
		for (int i = 0; i < rewardIcons.Length; i++)
		{
			ComponentExtensions.SetActive<VirtualItemIcon>(rewardIcons[i], isActive);
		}
		UIMissionNonItemReward[] nonItemRewards = NonItemRewards;
		for (int i = 0; i < nonItemRewards.Length; i++)
		{
			ComponentExtensions.SetActive<UIMissionNonItemReward>(nonItemRewards[i], isActive);
		}
		CompleteMissionRoot.SetActive(isActive);
	}

	public void SetCompleteMissionText(bool isMultipleChoice)
	{
		CompleteMissionText.SetPhrase(isMultipleChoice ? NPCConversationPhrases.SelectReward : NPCConversationPhrases.MissionCompleted, Array.Empty<object>());
	}
}
