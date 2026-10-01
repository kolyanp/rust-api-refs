using System;
using UnityEngine;

[Serializable]
public class DynamicResourceRefBase
{
	[SerializeField]
	private string _path;

	[SerializeField]
	private string _bundleName;

	public string Path => _path;

	public string BundleName => _bundleName;
}
