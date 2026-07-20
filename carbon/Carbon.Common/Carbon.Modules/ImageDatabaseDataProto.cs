using System.Collections.Generic;
using ProtoBuf;

namespace Carbon.Modules;

[ProtoContract]
public class ImageDatabaseDataProto
{
	[ProtoMember(1)]
	public ulong Identifier;

	[ProtoMember(2)]
	public Dictionary<string, uint> Map;

	[ProtoMember(3)]
	public Dictionary<string, string> CustomMap;
}
