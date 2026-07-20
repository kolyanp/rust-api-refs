using System;
using System.Collections.Generic;
using ConVar;
using Facepunch;
using UnityEngine;

namespace Rust.Ai.Gen2;

public class NpcBarkManager : SingletonComponent<NpcBarkManager>, IServerComponent
{
	public NPCVoicelinesDatabase voicelinesDatabase;

	private const float minTimeBetweenStarterVoicelines = 5f;

	private const float minTimeBetweenExactSameVoiceline = 300f;

	private const float minTimeBetweenStartersOfSameCategory = 60f;

	private SparseGrid<(int, double)> voicelineHistory = new SparseGrid<(int, double)>();

	public bool CanPlay(BaseEntity source, NPCVoiceline voiceline)
	{
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("NpcBarkManager.CanPlay"))
		{
			if ((Object)(object)source == (Object)null || voiceline.category == ENPCVoicelineCategory.None)
			{
				if (AI.logIssues)
				{
					Debug.LogError((object)$"NpcBarkManager: CanPlay called with null source or invalid voiceline. index: {voiceline.index}");
				}
				return false;
			}
			double timeAsDouble = Time.timeAsDouble;
			PooledList<(int, double)> val = Pool.Get<PooledList<(int, double)>>();
			try
			{
				voicelineHistory.GetNeighboors(((Component)source).transform.position, (List<(int, double)>)(object)val);
				foreach (var (num, num2) in (List<(int, double)>)(object)val)
				{
					if (!voicelinesDatabase.FindVoiceline(num, out var voiceline2))
					{
						if (AI.logIssues)
						{
							Debug.LogError((object)$"NpcBarkManager: CanPlay - voiceline {num} present in history not found in db.");
						}
						continue;
					}
					float num3 = (float)(timeAsDouble - num2);
					if (voiceline.importance != ENpcVoicelineImportance.Conversation && num3 < 5f)
					{
						return false;
					}
					if (voiceline2.category == voiceline.category && voiceline.importance == ENpcVoicelineImportance.Flavour && num3 < 60f)
					{
						return false;
					}
					if (voiceline2.index != voiceline.index || !(num3 < 300f))
					{
						continue;
					}
					return false;
				}
				return true;
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
	}

	public void OnPlay(BaseEntity source, NPCVoiceline voiceline)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		using (TimeWarning.New("NpcBarkManager.OnPlay"))
		{
			double timeAsDouble = Time.timeAsDouble;
			voicelineHistory.Add(((Component)source).transform.position, (voiceline.index, timeAsDouble));
		}
	}
}
