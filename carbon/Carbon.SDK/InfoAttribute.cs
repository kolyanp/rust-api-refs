using System;
using JetBrains.Annotations;
using Oxide.Core;

[AttributeUsage(AttributeTargets.Class)]
[MeansImplicitUse]
public class InfoAttribute : Attribute
{
	public string Title { get; }

	public string Author { get; }

	public VersionNumber Version { get; private set; }

	public int ResourceId { get; set; }

	public InfoAttribute(string Title, string Author, string Version)
	{
		this.Title = Title;
		this.Author = Author;
		this.Version = new VersionNumber(Version);
	}

	public InfoAttribute(string Title, string Author, double Version)
	{
		this.Title = Title;
		this.Author = Author;
		this.Version = new VersionNumber(Version.ToString());
	}
}
