using System;

namespace MySqlConnector.Protocol;

[Flags]
internal enum ProtocolCapabilities : ulong
{
	None = 0uL,
	LongPassword = 1uL,
	FoundRows = 2uL,
	LongFlag = 4uL,
	ConnectWithDatabase = 8uL,
	NoSchema = 0x10uL,
	Compress = 0x20uL,
	Odbc = 0x40uL,
	LocalFiles = 0x80uL,
	IgnoreSpace = 0x100uL,
	Protocol41 = 0x200uL,
	Interactive = 0x400uL,
	Ssl = 0x800uL,
	IgnoreSigpipe = 0x1000uL,
	Transactions = 0x2000uL,
	SecureConnection = 0x8000uL,
	MultiStatements = 0x10000uL,
	MultiResults = 0x20000uL,
	PreparedStatementMultiResults = 0x40000uL,
	PluginAuth = 0x80000uL,
	ConnectionAttributes = 0x100000uL,
	PluginAuthLengthEncodedClientData = 0x200000uL,
	CanHandleExpiredPasswords = 0x400000uL,
	SessionTrack = 0x800000uL,
	DeprecateEof = 0x1000000uL,
	QueryAttributes = 0x8000000uL,
	MariaDbClientProgress = 0x100000000uL,
	MariaDbComMulti = 0x200000000uL,
	MariaDbStatementBulkOperations = 0x400000000uL,
	MariaDbExtendedTypeInfo = 0x800000000uL,
	MariaDbCacheMetadata = 0x1000000000uL
}
