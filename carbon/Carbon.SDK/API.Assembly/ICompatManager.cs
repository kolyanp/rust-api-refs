namespace API.Assembly;

public interface ICompatManager
{
	void Init();

	ConversionResult AttemptOxideConvert(ref byte[] asm);

	bool ConvertHarmonyMod(ref byte[] data, bool noEntrypoint = false);
}
