using System;
using System.Collections.Generic;
using Facepunch;
using Rust;
using UnityEngine;

public class ConvarComponent : MonoBehaviour
{
	[Serializable]
	public class ConvarEvent
	{
		public string convar;

		public string on;

		public MonoBehaviour component;

		internal ConsoleSystem.Command cmd;

		public void OnEnable()
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			cmd = ConsoleSystem.Index.Client.Find(StringView.op_Implicit(convar));
			if (cmd == null)
			{
				cmd = ConsoleSystem.Index.Server.Find(StringView.op_Implicit(convar));
			}
			if (cmd != null)
			{
				cmd.OnValueChanged += cmd_OnValueChanged;
				cmd_OnValueChanged(cmd);
			}
		}

		private void cmd_OnValueChanged(ConsoleSystem.Command obj)
		{
			if (!((Object)(object)component == (Object)null))
			{
				bool flag = obj.String == on;
				if (((Behaviour)component).enabled != flag)
				{
					((Behaviour)component).enabled = flag;
				}
			}
		}

		public void OnDisable()
		{
			if (!Application.isQuitting && cmd != null)
			{
				cmd.OnValueChanged -= cmd_OnValueChanged;
			}
		}
	}

	public bool runOnServer = true;

	public bool runOnClient = true;

	public List<ConvarEvent> List = new List<ConvarEvent>();

	protected void OnEnable()
	{
		if (!ShouldRun())
		{
			return;
		}
		foreach (ConvarEvent item in List)
		{
			item.OnEnable();
		}
	}

	protected void OnDisable()
	{
		if (Application.isQuitting || !ShouldRun())
		{
			return;
		}
		foreach (ConvarEvent item in List)
		{
			item.OnDisable();
		}
	}

	private bool ShouldRun()
	{
		if (!runOnServer)
		{
			return false;
		}
		return true;
	}
}
