using System;
using System.Collections.Generic;

namespace Carbon.Contracts;

public interface IBaseProcessor
{
	public interface IProcess : IDisposable
	{
		bool IsRemoved { get; }

		bool IsDirty { get; }

		string File { get; set; }

		void Clear();

		void Execute(IBaseProcessor processor);

		void MarkDirty();

		void MarkDeleted();
	}

	public interface IParser
	{
		void Process(string file, string input, out string output);
	}

	Dictionary<string, IProcess> InstanceBuffer { get; set; }

	List<string> IgnoreList { get; set; }

	string Name { get; }

	string Folder { get; }

	string Extension { get; }

	bool IncludeSubdirectories { get; set; }

	void Start();

	T Get<T>(string fileName) where T : IProcess;

	void Prepare(string path);

	void Prepare(string name, string path);

	void Ignore(string path);

	bool Exists(string path);

	void Clear(IEnumerable<string> except = null);

	void ClearIgnore(string path);

	bool IsBlacklisted(string path);

	void RefreshRate();
}
