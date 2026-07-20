using System;
using Rust.UI;
using UnityEngine;

public class UIDialogueChoice : MonoBehaviour
{
	public RustText DialogueText;

	public GameObject MissionIcon;

	[NonSerialized]
	public BaseMission DisplayingMission;

	[NonSerialized]
	public int SpeechResponseIndex;

	public void SetMissionIconActive(bool isActive)
	{
		MissionIcon.SetActive(isActive);
	}

	public void SetDialoguePhrase(Phrase phrase)
	{
		DialogueText.SetPhrase(phrase, Array.Empty<object>());
	}

	private void OnDisable()
	{
		DisplayingMission = null;
	}
}
