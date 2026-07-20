using System.Threading.Tasks;

namespace Carbon.Extensions;

public class AsyncEx
{
	public static async Task NextTick()
	{
		TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
		Community.Runtime.Core.NextTick(delegate
		{
			tcs.SetResult(result: true);
		});
		await tcs.Task;
		tcs = null;
	}

	public static async Task NextFrame()
	{
		await NextTick();
	}

	public static async Task WaitForSeconds(float seconds)
	{
		await Task.Delay((int)(seconds * 1000f));
	}
}
