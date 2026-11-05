using System.Collections.Generic;

public class ImageStorageEntity : BaseEntity, IServerFileReceiver
{
	private struct ImageRequest
	{
		public IImageReceiver Receiver;

		public float Time;
	}

	private List<ImageRequest> _requests;

	private uint _requestedCrc;

	protected virtual uint CrcToLoad => 0u;
}
