using System;
using System.Collections;
using UnityEngine;

namespace Carbon.Contracts;

public interface IScriptProcessor : IBaseProcessor, IDisposable
{
	public interface IScript : IProcess, IDisposable
	{
		IScriptLoader Loader { get; set; }
	}

	GameObject gameObject { get; }

	void InvokeRepeating(Action action, float delay, float repeat);

	bool AllPendingScriptsComplete();

	bool AllNonRequiresScriptsComplete();

	bool AllExtensionsComplete();

	void StartCoroutine(IEnumerator coroutine);

	void StopCoroutine(IEnumerator coroutine);

	void Remove(string name);
}
