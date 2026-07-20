using System;
using System.Collections.Generic;
using ProtoBuf;

namespace Oxide.Core.Libraries;

[ProtoContract(ImplicitFields = ImplicitFields.AllFields)]
public class UserData
{
	public string LastSeenNickname { get; set; } = "Unnamed";

	public HashSet<string> Perms { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

	public HashSet<string> Groups { get; set; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
}
