using System;

namespace API.Abstracts;

public abstract class Singleton<T> where T : class
{
	private static readonly Lazy<T> Instance = new Lazy<T>(() => CreateInstance());

	public static T GetInstance()
	{
		return Instance.Value;
	}

	private static T CreateInstance()
	{
		return Activator.CreateInstance(typeof(T), nonPublic: true) as T;
	}
}
