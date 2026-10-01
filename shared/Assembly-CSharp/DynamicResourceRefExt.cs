public static class DynamicResourceRefExt
{
	public static bool IsValid(this DynamicResourceRefBase refObj)
	{
		if (refObj != null)
		{
			return !string.IsNullOrEmpty(refObj.Path);
		}
		return false;
	}
}
