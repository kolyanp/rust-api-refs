using System;
using System.Collections.Generic;
using System.Text;
using Oxide.Pooling;

namespace Oxide.Core.Libraries.Covalence;

public class Formatter
{
	private class Token : Poolable<Token>
	{
		public TokenType Type;

		public object Val;

		public string Pattern;

		private static readonly Stack<Token> pool = new Stack<Token>();

		public static Token TakeFromPool(TokenType type, object val, string pattern)
		{
			Token token = Poolable<Token>.TakeFromPool();
			token.Type = type;
			token.Val = val;
			token.Pattern = pattern;
			return token;
		}

		protected override void Reset()
		{
			Type = TokenType.String;
			Val = null;
			Pattern = null;
		}
	}

	private enum TokenType
	{
		String,
		Bold,
		Italic,
		Color,
		Size,
		CloseBold,
		CloseItalic,
		CloseColor,
		CloseSize
	}

	private class Lexer : Poolable<Lexer>
	{
		private enum StateType
		{
			Str,
			Tag,
			CloseTag,
			EndTag,
			ParamTag
		}

		private List<Token> tokens = new List<Token>();

		private int patternStart;

		private int tokenStart;

		private int position;

		private string text;

		private StateType state;

		private TokenType currentTokenType;

		private Func<string, object> currentParser;

		private char Current()
		{
			return text[position];
		}

		private void Next()
		{
			position++;
		}

		private void StartNewToken()
		{
			tokenStart = position;
		}

		private void StartNewPattern()
		{
			patternStart = position;
			StartNewToken();
		}

		private void ResetTokenStart()
		{
			tokenStart = patternStart;
		}

		private string Token()
		{
			return text.Substring(tokenStart, position - tokenStart);
		}

		private void Add(TokenType type, object val = null)
		{
			Token item = Formatter.Token.TakeFromPool(type, val, text.Substring(patternStart, position - patternStart));
			tokens.Add(item);
		}

		private void WritePatternString()
		{
			if (patternStart < position)
			{
				int num = tokenStart;
				tokenStart = patternStart;
				Add(TokenType.String, Token());
				tokenStart = num;
			}
		}

		private static bool IsValidColorCode(string val)
		{
			if (val.Length != 6 && val.Length != 8)
			{
				return false;
			}
			foreach (char c in val)
			{
				if ((c < '0' || c > '9') && (c < 'a' || c > 'f') && (c < 'A' || c > 'F'))
				{
					return false;
				}
			}
			return true;
		}

		private static object ParseColor(string val)
		{
			if (!colorNames.TryGetValue(val.ToLower(), out var value) && !IsValidColorCode(val))
			{
				return null;
			}
			value = value ?? val;
			if (value.Length == 6)
			{
				value += "ff";
			}
			return value;
		}

		private static object ParseSize(string val)
		{
			if (int.TryParse(val, out var result))
			{
				return result;
			}
			return null;
		}

		private void EndTag()
		{
			if (Current() == ']')
			{
				Next();
				Add(currentTokenType);
				StartNewPattern();
				state = StateType.Str;
			}
			else
			{
				ResetTokenStart();
				state = StateType.Str;
			}
		}

		private void ParamTag()
		{
			if (Current() != ']')
			{
				Next();
				return;
			}
			object obj = currentParser(Token());
			if (obj == null)
			{
				ResetTokenStart();
				state = StateType.Str;
				return;
			}
			Next();
			Add(currentTokenType, obj);
			StartNewPattern();
			state = StateType.Str;
		}

		private void CloseTag()
		{
			switch (Current())
			{
			case 'b':
				currentTokenType = TokenType.CloseBold;
				Next();
				state = StateType.EndTag;
				break;
			case 'i':
				currentTokenType = TokenType.CloseItalic;
				Next();
				state = StateType.EndTag;
				break;
			case '#':
				currentTokenType = TokenType.CloseColor;
				Next();
				state = StateType.EndTag;
				break;
			case '+':
				currentTokenType = TokenType.CloseSize;
				Next();
				state = StateType.EndTag;
				break;
			default:
				ResetTokenStart();
				state = StateType.Str;
				break;
			}
		}

		private void Tag()
		{
			switch (Current())
			{
			case 'b':
				currentTokenType = TokenType.Bold;
				Next();
				state = StateType.EndTag;
				break;
			case 'i':
				currentTokenType = TokenType.Italic;
				Next();
				state = StateType.EndTag;
				break;
			case '#':
				currentTokenType = TokenType.Color;
				currentParser = ParseColor;
				Next();
				StartNewToken();
				state = StateType.ParamTag;
				break;
			case '+':
				currentTokenType = TokenType.Size;
				currentParser = ParseSize;
				Next();
				StartNewToken();
				state = StateType.ParamTag;
				break;
			case '/':
				Next();
				state = StateType.CloseTag;
				break;
			default:
				ResetTokenStart();
				state = StateType.Str;
				break;
			}
		}

		private void Str()
		{
			if (Current() == '[')
			{
				WritePatternString();
				StartNewPattern();
				Next();
				state = StateType.Tag;
			}
			else
			{
				Next();
			}
		}

		public static List<Token> Lex(string text)
		{
			return new Lexer().TokenizeText(text);
		}

		public List<Token> TokenizeText(string text)
		{
			this.text = text;
			while (position < text.Length)
			{
				switch (state)
				{
				case StateType.Str:
					Str();
					break;
				case StateType.Tag:
					Tag();
					break;
				case StateType.CloseTag:
					CloseTag();
					break;
				case StateType.EndTag:
					EndTag();
					break;
				case StateType.ParamTag:
					ParamTag();
					break;
				}
			}
			WritePatternString();
			return tokens;
		}

		protected override void Reset()
		{
			text = null;
			patternStart = 0;
			tokenStart = 0;
			position = 0;
			Poolable<Formatter.Token>.ReturnToPool(tokens);
			currentTokenType = TokenType.String;
			currentParser = null;
			state = StateType.Str;
		}
	}

	private class Entry : Poolable<Entry>
	{
		public string Pattern;

		public Element Element;

		public static Entry TakeFromPool(string pattern, Element element)
		{
			Entry entry = Poolable<Entry>.TakeFromPool();
			entry.Pattern = pattern;
			entry.Element = element;
			return entry;
		}

		protected override void Reset()
		{
			Pattern = null;
			Element = null;
		}
	}

	private class ElementTreeBuilder : Poolable<ElementTreeBuilder>
	{
		private readonly Stack<Entry> entries = new Stack<Entry>();

		public bool shouldPoolElements;

		public Element ProcessTokens(List<Token> tokens)
		{
			int num = 0;
			entries.Clear();
			entries.Push(Entry.TakeFromPool(null, Element.Tag(ElementType.String, shouldPoolElements)));
			while (num < tokens.Count)
			{
				Token token = tokens[num++];
				Element element = entries.Peek().Element;
				if (token.Type == closeTags[element.Type])
				{
					entries.Pop();
					entries.Peek().Element.Body.Add(element);
					continue;
				}
				switch (token.Type)
				{
				case TokenType.String:
					element.Body.Add(Element.String(token.Val, shouldPoolElements));
					break;
				case TokenType.Bold:
					entries.Push(Entry.TakeFromPool(token.Pattern, Element.Tag(ElementType.Bold, shouldPoolElements)));
					break;
				case TokenType.Italic:
					entries.Push(Entry.TakeFromPool(token.Pattern, Element.Tag(ElementType.Italic, shouldPoolElements)));
					break;
				case TokenType.Color:
					entries.Push(Entry.TakeFromPool(token.Pattern, Element.ParamTag(ElementType.Color, token.Val, shouldPoolElements)));
					break;
				case TokenType.Size:
					entries.Push(Entry.TakeFromPool(token.Pattern, Element.ParamTag(ElementType.Size, token.Val, shouldPoolElements)));
					break;
				default:
					element.Body.Add(Element.String(token.Pattern, shouldPoolElements));
					break;
				}
			}
			while (entries.Count > 1)
			{
				Entry entry = entries.Pop();
				List<Element> body = entries.Peek().Element.Body;
				body.Add(Element.String(entry.Pattern));
				body.AddRange(entry.Element.Body);
				Poolable<Entry>.ReturnToPool(entry);
			}
			Entry entry2 = entries.Pop();
			Element element2 = entry2.Element;
			Poolable<Entry>.ReturnToPool(entry2);
			return element2;
		}

		protected override void Reset()
		{
			shouldPoolElements = false;
			while (entries.Count > 0)
			{
				Poolable<Entry>.ReturnToPool(entries.Pop());
			}
		}
	}

	private class Tag
	{
		public string Open;

		public string Close;

		public Tag(string open, string close)
		{
			Open = open;
			Close = close;
		}
	}

	public abstract class Poolable<T> where T : Poolable<T>, new()
	{
		private static readonly Stack<T> _pool = new Stack<T>();

		private static readonly object _poolLock = new object();

		protected bool isFromPool { get; private set; }

		public static T TakeFromPool()
		{
			T val;
			lock (_poolLock)
			{
				val = ((_pool.Count > 0) ? _pool.Pop() : new T());
			}
			val.isFromPool = true;
			return val;
		}

		public static void ReturnToPool(T obj)
		{
			if (obj == null || !obj.isFromPool)
			{
				return;
			}
			obj.Reset();
			lock (_poolLock)
			{
				_pool.Push(obj);
			}
		}

		public static void ReturnToPool(List<T> objs)
		{
			if (objs != null)
			{
				for (int i = 0; i < objs.Count; i++)
				{
					ReturnToPool(objs[i]);
				}
				objs.Clear();
			}
		}

		protected abstract void Reset();
	}

	private static readonly Dictionary<string, string> colorNames = new Dictionary<string, string>
	{
		["aqua"] = "00ffff",
		["black"] = "000000",
		["blue"] = "0000ff",
		["brown"] = "a52a2a",
		["cyan"] = "00ffff",
		["darkblue"] = "0000a0",
		["fuchsia"] = "ff00ff",
		["green"] = "008000",
		["grey"] = "808080",
		["lightblue"] = "add8e6",
		["lime"] = "00ff00",
		["magenta"] = "ff00ff",
		["maroon"] = "800000",
		["navy"] = "000080",
		["olive"] = "808000",
		["orange"] = "ffa500",
		["purple"] = "800080",
		["red"] = "ff0000",
		["silver"] = "c0c0c0",
		["teal"] = "008080",
		["white"] = "ffffff",
		["yellow"] = "ffff00"
	};

	private static readonly Tag emptyTag = new Tag(string.Empty, string.Empty);

	private static readonly Tag boldTag = new Tag("<b>", "</b>");

	private static readonly Tag italicTag = new Tag("<i>", "</i>");

	private static readonly Dictionary<ElementType, Func<object, Tag>> plainTextTranslations = new Dictionary<ElementType, Func<object, Tag>>();

	private static readonly Dictionary<ElementType, Func<object, Tag>> unityTranslations = new Dictionary<ElementType, Func<object, Tag>>
	{
		[ElementType.Bold] = (object _) => boldTag,
		[ElementType.Italic] = (object _) => italicTag,
		[ElementType.Color] = (object c) => new Tag($"<color=#{c}>", "</color>"),
		[ElementType.Size] = (object s) => new Tag($"<size={s}>", "</size>")
	};

	private static readonly Dictionary<ElementType, Func<object, Tag>> rustLegacyTranslations = new Dictionary<ElementType, Func<object, Tag>> { [ElementType.Color] = (object c) => new Tag("[color #" + RGBAtoRGB(c) + "]", "[color #ffffff]") };

	private static readonly Dictionary<ElementType, Func<object, Tag>> rokAnd7DTDTranslations = new Dictionary<ElementType, Func<object, Tag>> { [ElementType.Color] = (object c) => new Tag("[" + RGBAtoRGB(c) + "]", "[e7e7e7]") };

	private static readonly Dictionary<ElementType, Func<object, Tag>> terrariaTranslations = new Dictionary<ElementType, Func<object, Tag>> { [ElementType.Color] = (object c) => new Tag("[c/" + RGBAtoRGB(c) + ":", "]") };

	private static readonly IPoolProvider<StringBuilder> stringPool = Interface.Oxide.PoolFactory.GetProvider<StringBuilder>();

	private static readonly Dictionary<ElementType, TokenType?> closeTags = new Dictionary<ElementType, TokenType?>
	{
		[ElementType.String] = null,
		[ElementType.Bold] = TokenType.CloseBold,
		[ElementType.Italic] = TokenType.CloseItalic,
		[ElementType.Color] = TokenType.CloseColor,
		[ElementType.Size] = TokenType.CloseSize
	};

	public static List<Element> Parse(string text)
	{
		return ParseText(text, shouldPoolElements: false).Body;
	}

	private static Element ParseText(string text, bool shouldPoolElements = true)
	{
		Lexer lexer = Poolable<Lexer>.TakeFromPool();
		try
		{
			return ProcessTokens(lexer.TokenizeText(text), shouldPoolElements);
		}
		finally
		{
			Poolable<Lexer>.ReturnToPool(lexer);
		}
	}

	private static Element ProcessTokens(List<Token> tokens, bool shouldPoolElements)
	{
		ElementTreeBuilder elementTreeBuilder = Poolable<ElementTreeBuilder>.TakeFromPool();
		try
		{
			elementTreeBuilder.shouldPoolElements = shouldPoolElements;
			return elementTreeBuilder.ProcessTokens(tokens);
		}
		finally
		{
			Poolable<ElementTreeBuilder>.ReturnToPool(elementTreeBuilder);
		}
	}

	private static Tag Translation(Element e, Dictionary<ElementType, Func<object, Tag>> translations)
	{
		if (!translations.TryGetValue(e.Type, out var value))
		{
			return emptyTag;
		}
		return value(e.Val);
	}

	private static string ToTreeFormat(List<Element> tree, Dictionary<ElementType, Func<object, Tag>> translations)
	{
		StringBuilder stringBuilder = stringPool.Take();
		try
		{
			AppendTreeFormat(tree, translations, stringBuilder);
			return stringBuilder.ToString();
		}
		finally
		{
			stringPool.Return(stringBuilder);
		}
	}

	private static void AppendTreeFormat(List<Element> tree, Dictionary<ElementType, Func<object, Tag>> translations, StringBuilder sb)
	{
		foreach (Element item in tree)
		{
			if (item.Type == ElementType.String)
			{
				sb.Append(item.Val);
				continue;
			}
			Tag tag = Translation(item, translations);
			sb.Append(tag.Open);
			AppendTreeFormat(item.Body, translations, sb);
			sb.Append(tag.Close);
		}
	}

	private static string ToTreeFormat(string text, Dictionary<ElementType, Func<object, Tag>> translations)
	{
		Element element = ParseText(text);
		try
		{
			return ToTreeFormat(element.Body, translations);
		}
		finally
		{
			Poolable<Element>.ReturnToPool(element);
		}
	}

	private static string RGBAtoRGB(object rgba)
	{
		return rgba.ToString().Substring(0, 6);
	}

	public static string ToPlaintext(string text)
	{
		return ToTreeFormat(text, plainTextTranslations);
	}

	public static string ToUnity(string text)
	{
		return ToTreeFormat(text, unityTranslations);
	}

	public static string ToRustLegacy(string text)
	{
		return ToTreeFormat(text, rustLegacyTranslations);
	}

	public static string ToRoKAnd7DTD(string text)
	{
		return ToTreeFormat(text, rokAnd7DTDTranslations);
	}

	public static string ToTerraria(string text)
	{
		return ToTreeFormat(text, terrariaTranslations);
	}
}
