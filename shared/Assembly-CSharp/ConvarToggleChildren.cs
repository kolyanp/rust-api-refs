using Facepunch;
using UnityEngine;

public class ConvarToggleChildren : MonoBehaviour
{
	public string ConvarName;

	public string ConvarEnabled = "True";

	private bool state;

	private ConsoleSystem.Command Command;

	protected void Awake()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		Command = ConsoleSystem.Index.Client.Find(StringView.op_Implicit(ConvarName));
		if (Command == null)
		{
			Command = ConsoleSystem.Index.Server.Find(StringView.op_Implicit(ConvarName));
		}
		if (Command != null)
		{
			SetState(Command.String == ConvarEnabled);
		}
	}

	protected void Update()
	{
		if (Command != null)
		{
			bool flag = Command.String == ConvarEnabled;
			if (state != flag)
			{
				SetState(flag);
			}
		}
	}

	private void SetState(bool newState)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		foreach (Transform item in ((Component)this).transform)
		{
			((Component)item).gameObject.SetActive(newState);
		}
		state = newState;
	}
}
