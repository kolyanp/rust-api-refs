using System.Collections.Generic;

namespace Oxide.Core.Libraries.Covalence;

public class Element : Formatter.Poolable<Element>
{
	public ElementType Type;

	public object Val;

	public List<Element> Body = new List<Element>();

	public Element()
	{
	}

	public Element(ElementType type, object val)
	{
		Type = type;
		Val = val;
	}

	private static Element Get(ElementType type, object val, bool shouldPool)
	{
		if (!shouldPool)
		{
			return new Element(type, val);
		}
		Element element = Formatter.Poolable<Element>.TakeFromPool();
		element.Type = type;
		element.Val = val;
		return element;
	}

	protected override void Reset()
	{
		Type = ElementType.String;
		Val = null;
		Formatter.Poolable<Element>.ReturnToPool(Body);
	}

	public static Element String(object s, bool shouldPool = false)
	{
		return Get(ElementType.String, s, shouldPool);
	}

	public static Element Tag(ElementType type, bool shouldPool = false)
	{
		return Get(type, null, shouldPool);
	}

	public static Element ParamTag(ElementType type, object val, bool shouldPool = false)
	{
		return Get(type, val, shouldPool);
	}
}
