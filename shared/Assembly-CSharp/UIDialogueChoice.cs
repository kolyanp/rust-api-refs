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
	public NetworkableId DisplayingAnimal;

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
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		DisplayingMission = null;
		DisplayingAnimal = default;
	}
}
