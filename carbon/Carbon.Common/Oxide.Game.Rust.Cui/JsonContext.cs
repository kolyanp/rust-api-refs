using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Oxide.Game.Rust.Cui;

public class JsonContext
{
	public readonly StringBuilder sb;

	public readonly StringWriter sw;

	public readonly JsonTextWriter jw;

	public readonly JsonTextWriter jwFormatted;

	public JsonContext()
	{
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Expected O, but got Unknown
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected O, but got Unknown
		sb = new StringBuilder(65536);
		sw = new StringWriter(sb, CultureInfo.InvariantCulture);
		jw = new JsonTextWriter((TextWriter)sw)
		{
			Formatting = (Formatting)0,
			ArrayPool = JsonArrayPool<char>.Shared,
			CloseOutput = false
		};
		jwFormatted = new JsonTextWriter((TextWriter)sw)
		{
			Formatting = (Formatting)1,
			ArrayPool = JsonArrayPool<char>.Shared,
			CloseOutput = false
		};
	}
}
