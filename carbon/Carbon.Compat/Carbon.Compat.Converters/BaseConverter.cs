using System.Collections.Immutable;
using System.IO;
using AsmResolver.DotNet;
using AsmResolver.DotNet.Builder;
using AsmResolver.PE.Builder;
using AsmResolver.PE.DotNet.Builder;
using Carbon.Compat.Patches;

namespace Carbon.Compat.Converters;

public abstract class BaseConverter
{
	public struct Context
	{
		public string Author;

		public bool NoEntrypoint;

		public byte[] Buffer;
	}

	internal static ManagedPEImageBuilder _imageBuilder;

	internal static ManagedPEFileBuilder _fileBuilder;

	public abstract ImmutableList<IAssemblyPatch> Patches { get; }

	public abstract string Name { get; }

	public virtual byte[] Convert(ModuleDefinition asm, Context ctx = default(Context))
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Expected O, but got Unknown
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		ReferenceImporter importer = new ReferenceImporter(asm);
		foreach (IAssemblyPatch patch in Patches)
		{
			patch.Apply(asm, importer, ref ctx);
		}
		PEImageBuildResult val = _imageBuilder.CreateImage(asm);
		if (val.HasFailed)
		{
			throw new MetadataBuilderException("it failed :(");
		}
		using MemoryStream memoryStream = new MemoryStream();
		((PEFileBuilderBase<ManagedPEBuilderContext>)(object)_fileBuilder).CreateFile(val.ConstructedImage).Write((Stream)memoryStream);
		return memoryStream.ToArray();
	}

	static BaseConverter()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Expected O, but got Unknown
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Expected O, but got Unknown
		_imageBuilder = new ManagedPEImageBuilder();
		_fileBuilder = new ManagedPEFileBuilder();
	}
}
