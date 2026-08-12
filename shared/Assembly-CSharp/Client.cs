public class Client : SingletonComponent<Client>
{
	public static Phrase loading_loading;

	public static Phrase loading_connecting;

	public static Phrase loading_connectionaccepted;

	public static Phrase loading_connecting_negotiate;

	public static Phrase loading_level;

	public static Phrase loading_skinnablewarmup;

	public static Phrase loading_preloadcomplete;

	public static Phrase loading_openingscene;

	public static Phrase loading_clientready;

	public static Phrase loading_prefabwarmup;

	public static Phrase loading_queue;

	public static Phrase loading_queue_status;

	public static Phrase loading_queue_next;

	public static Phrase party_too_large_phrase;

	static Client()
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Expected O, but got Unknown
		//IL_0032: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Expected O, but got Unknown
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Expected O, but got Unknown
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0064: Expected O, but got Unknown
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Expected O, but got Unknown
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Expected O, but got Unknown
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Expected O, but got Unknown
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Expected O, but got Unknown
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c8: Expected O, but got Unknown
		//IL_00d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Expected O, but got Unknown
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Expected O, but got Unknown
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0104: Expected O, but got Unknown
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Expected O, but got Unknown
		loading_loading = new Phrase("loading.loading", "Loading");
		loading_connecting = new Phrase("loading.connecting", "Connecting");
		loading_connectionaccepted = new Phrase("loading.connectionaccepted", "Connection Accepted");
		loading_connecting_negotiate = new Phrase("loading.connecting.negotiate", "Negotiating Connection");
		loading_level = new Phrase("loading.loadinglevel", "Loading Level");
		loading_skinnablewarmup = new Phrase("loading.skinnablewarmup", "Skinnable Warmup");
		loading_preloadcomplete = new Phrase("loading.preloadcomplete", "Preload Complete");
		loading_openingscene = new Phrase("loading.openingscene", "Opening Scene");
		loading_clientready = new Phrase("loading.clientready", "Client Ready");
		loading_prefabwarmup = new Phrase("loading.prefabwarmup", "Warming Prefabs [{0}/{1}]");
		loading_queue = new Phrase("loading.queue", "Queue");
		loading_queue_status = new Phrase("loading.queue.status", "{0:N0} PLAYERS AHEAD OF YOU, {1:N0} PLAYERS BEHIND");
		loading_queue_next = new Phrase("loading.queue.next", "YOU'RE NEXT - {1:N0} PLAYERS BEHIND YOU");
		party_too_large_phrase = new Phrase("loading.party_too_large", "Party too large to join server: max team size {0}");
	}
}
