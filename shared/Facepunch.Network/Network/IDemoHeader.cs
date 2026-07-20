using System.IO;

namespace Network;

public interface IDemoHeader
{
	long Length { get; set; }

	void Write(BinaryWriter writer);
}
