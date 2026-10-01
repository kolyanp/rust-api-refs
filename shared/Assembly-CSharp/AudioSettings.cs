using ConVar;
using UnityEngine;
using UnityEngine.Audio;

public class AudioSettings : MonoBehaviour
{
	public static float duckingFactor = 1f;

	public AudioMixer mixer;

	private void Update()
	{
		if (!((Object)(object)mixer == (Object)null))
		{
			mixer.SetFloat("MasterVol", LinearToDecibel(Audio.master * duckingFactor));
			float num = default;
			mixer.GetFloat("MusicVol", ref num);
			if (!LevelManager.isLoaded || !MainCamera.isValid)
			{
				mixer.SetFloat("MusicVol", Mathf.Lerp(num, LinearToDecibel(Audio.musicvolumemenu), Time.deltaTime));
			}
			else
			{
				mixer.SetFloat("MusicVol", Mathf.Lerp(num, LinearToDecibel(Audio.musicvolume), Time.deltaTime));
			}
			float num2 = 1f - (((Object)(object)SingletonComponent<MixerSnapshotManager>.Instance == (Object)null) ? 0f : SingletonComponent<MixerSnapshotManager>.Instance.deafness);
			mixer.SetFloat("WorldVol", LinearToDecibel(Audio.game * num2));
			mixer.SetFloat("UIVol", LinearToDecibel(Audio.ui * num2));
			mixer.SetFloat("WorldVolFlashbang", LinearToDecibel(Audio.game));
			mixer.SetFloat("VoiceVol", LinearToDecibel(Audio.voices * num2));
			mixer.SetFloat("InstrumentVol", LinearToDecibel(Audio.instruments * num2));
			float num3 = LinearToDecibel(Audio.voiceProps * num2) - 28.7f;
			mixer.SetFloat("VoicePropsVol", num3 * num2);
			mixer.SetFloat("SeasonalEventsVol", LinearToDecibel(Audio.eventAudio * num2));
			float num4 = (((Object)(object)SingletonComponent<MixerSnapshotManager>.Instance == (Object)null) ? 0f : SingletonComponent<MixerSnapshotManager>.Instance.headgearMuffle);
			float num5 = Mathf.Exp(Mathf.Lerp(Mathf.Log(22000f), Mathf.Log(1800f), num4));
			float num6 = default;
			mixer.GetFloat("WorldHeadgearCutoff", ref num6);
			float num7 = Mathf.Lerp(num6, num5, Time.deltaTime * 4f);
			mixer.SetFloat("WorldHeadgearCutoff", num7);
			mixer.SetFloat("VoiceHeadgearCutoff", num7);
		}
	}

	private float LinearToDecibel(float linear)
	{
		if (linear > 0f)
		{
			return 20f * Mathf.Log10(linear);
		}
		return -144f;
	}
}
