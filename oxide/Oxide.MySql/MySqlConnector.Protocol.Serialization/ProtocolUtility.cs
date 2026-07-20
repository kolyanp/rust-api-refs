using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MySqlConnector.Protocol.Serialization;

[_003C31066bf3_002D0b14_002D4079_002D9cb8_002Dcf7631dbb2a9_003ENullableContext(1)]
[_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
internal static class ProtocolUtility
{
	public const int MaxPacketSize = 16777215;

	public static int GetBytesPerCharacter(CharacterSet characterSet)
	{
		if (characterSet <= CharacterSet.Utf32ThaiUnicode520Weight2)
		{
			if (characterSet > CharacterSet.Utf8Mb4ThaiUnicode520Weight2)
			{
				if (characterSet - 640 <= CharacterSet.Latin2CzechCaseSensitive)
				{
					goto IL_083a;
				}
				if (characterSet - 672 <= CharacterSet.Latin2CzechCaseSensitive || characterSet - 736 <= CharacterSet.Latin2CzechCaseSensitive)
				{
					goto IL_083e;
				}
				goto IL_0840;
			}
			switch (characterSet)
			{
			default:
				if (characterSet - 576 <= CharacterSet.Latin2CzechCaseSensitive)
				{
					goto IL_083c;
				}
				if (characterSet - 608 <= CharacterSet.Latin2CzechCaseSensitive)
				{
					goto IL_083e;
				}
				goto IL_0840;
			case CharacterSet.Latin2CzechCaseSensitive:
			case CharacterSet.Dec8SwedishCaseInsensitive:
			case CharacterSet.Cp850GeneralCaseInsensitive:
			case CharacterSet.Latin1German1CaseInsensitive:
			case CharacterSet.Hp8EnglishCaseInsensitive:
			case CharacterSet.Koi8rGeneralCaseInsensitive:
			case CharacterSet.Latin1SwedishCaseInsensitive:
			case CharacterSet.Latin2GeneralCaseInsensitive:
			case CharacterSet.Swe7SwedishCaseInsensitive:
			case CharacterSet.AsciiGeneralCaseInsensitive:
			case CharacterSet.Cp1251BulgarianCaseInsensitive:
			case CharacterSet.Latin1DanishCaseInsensitive:
			case CharacterSet.HebrewGeneralCaseInsensitive:
			case CharacterSet.Tis620ThaiCaseInsensitive:
			case CharacterSet.Latin7EstonianCaseSensitive:
			case CharacterSet.Latin2HungarianCaseInsensitive:
			case CharacterSet.Koi8uGeneralCaseInsensitive:
			case CharacterSet.Cp1251UkrainianCaseInsensitive:
			case CharacterSet.GreekGeneralCaseInsensitive:
			case CharacterSet.Cp1250GeneralCaseInsensitive:
			case CharacterSet.Latin2CroatianCaseInsensitive:
			case CharacterSet.Cp1257LithuanianCaseInsensitive:
			case CharacterSet.Latin5TurkishCaseInsensitive:
			case CharacterSet.Latin1German2CaseInsensitive:
			case CharacterSet.Armscii8GeneralCaseInsensitive:
			case CharacterSet.Cp1250CzechCaseSensitive:
			case CharacterSet.Cp866GeneralCaseInsensitive:
			case CharacterSet.Keybcs2GeneralCaseInsensitive:
			case CharacterSet.MacceGeneralCaseInsensitive:
			case CharacterSet.MacromanGeneralCaseInsensitive:
			case CharacterSet.Cp852GeneralCaseInsensitive:
			case CharacterSet.Latin7GeneralCaseInsensitive:
			case CharacterSet.Latin7GeneralCaseSensitive:
			case CharacterSet.MacceBinary:
			case CharacterSet.Cp1250CroatianCaseInsensitive:
			case CharacterSet.Latin1Binary:
			case CharacterSet.Latin1GeneralCaseInsensitive:
			case CharacterSet.Latin1GeneralCaseSensitive:
			case CharacterSet.Cp1251Binary:
			case CharacterSet.Cp1251GeneralCaseInsensitive:
			case CharacterSet.Cp1251GeneralCaseSensitive:
			case CharacterSet.MacromanBinary:
			case CharacterSet.Cp1256GeneralCaseInsensitive:
			case CharacterSet.Cp1257Binary:
			case CharacterSet.Cp1257GeneralCaseInsensitive:
			case CharacterSet.Binary:
			case CharacterSet.Armscii8Binary:
			case CharacterSet.AsciiBinary:
			case CharacterSet.Cp1250Binary:
			case CharacterSet.Cp1256Binary:
			case CharacterSet.Cp866Binary:
			case CharacterSet.Dec8Binary:
			case CharacterSet.GreekBinary:
			case CharacterSet.HebrewBinary:
			case CharacterSet.Hp8Binary:
			case CharacterSet.Keybcs2Binary:
			case CharacterSet.Koi8rBinary:
			case CharacterSet.Koi8uBinary:
			case CharacterSet.Latin2Binary:
			case CharacterSet.Latin5Binary:
			case CharacterSet.Latin7Binary:
			case CharacterSet.Cp850Binary:
			case CharacterSet.Cp852Binary:
			case CharacterSet.Swe7Binary:
			case CharacterSet.Tis620Binary:
			case CharacterSet.Geostd8GeneralCaseInsensitive:
			case CharacterSet.Geostd8Binary:
			case CharacterSet.Latin1SpanishCaseInsensitive:
			case CharacterSet.Cp1250PolishCaseInsensitive:
				break;
			case CharacterSet.Big5ChineseCaseInsensitive:
			case CharacterSet.SjisJapaneseCaseInsensitive:
			case CharacterSet.EuckrKoreanCaseInsensitive:
			case CharacterSet.Gb2312ChineseCaseInsensitive:
			case CharacterSet.GbkChineseCaseInsensitive:
			case CharacterSet.Ucs2GeneralCaseInsensitive:
			case CharacterSet.Big5Binary:
			case CharacterSet.EuckrBinary:
			case CharacterSet.Gb2312Binary:
			case CharacterSet.GbkBinary:
			case CharacterSet.SjisBinary:
			case CharacterSet.Ucs2Binary:
			case CharacterSet.Cp932JapaneseCaseInsensitive:
			case CharacterSet.Cp932Binary:
			case CharacterSet.Ucs2UnicodeCaseInsensitive:
			case CharacterSet.Ucs2IcelandicCaseInsensitive:
			case CharacterSet.Ucs2LatvianCaseInsensitive:
			case CharacterSet.Ucs2RomanianCaseInsensitive:
			case CharacterSet.Ucs2SlovenianCaseInsensitive:
			case CharacterSet.Ucs2PolishCaseInsensitive:
			case CharacterSet.Ucs2EstonianCaseInsensitive:
			case CharacterSet.Ucs2SpanishCaseInsensitive:
			case CharacterSet.Ucs2SwedishCaseInsensitive:
			case CharacterSet.Ucs2TurkishCaseInsensitive:
			case CharacterSet.Ucs2CzechCaseInsensitive:
			case CharacterSet.Ucs2DanishCaseInsensitive:
			case CharacterSet.Ucs2LithuanianCaseInsensitive:
			case CharacterSet.Ucs2SlovakCaseInsensitive:
			case CharacterSet.Ucs2Spanish2CaseInsensitive:
			case CharacterSet.Ucs2RomanCaseInsensitive:
			case CharacterSet.Ucs2PersianCaseInsensitive:
			case CharacterSet.Ucs2EsperantoCaseInsensitive:
			case CharacterSet.Ucs2HungarianCaseInsensitive:
			case CharacterSet.Ucs2SinhalaCaseInsensitive:
			case CharacterSet.Ucs2German2CaseInsensitive:
			case CharacterSet.Ucs2CroatianCaseInsensitive:
			case CharacterSet.Ucs2Unicode520CaseInsensitive:
			case CharacterSet.Ucs2VietnameseCaseInsensitive:
			case CharacterSet.Ucs2GeneralMySql500CaseInsensitive:
				goto IL_083a;
			case CharacterSet.UjisJapaneseCaseInsensitive:
			case CharacterSet.Utf8Mb3GeneralCaseInsensitive:
			case CharacterSet.Utf8Mb3ToLowerCaseInsensitive:
			case CharacterSet.Utf8Mb3Binary:
			case CharacterSet.UjisBinary:
			case CharacterSet.EucjpmsJapaneseCaseInsensitive:
			case CharacterSet.EucjpmsBinary:
			case CharacterSet.Utf8Mb3UnicodeCaseInsensitive:
			case CharacterSet.Utf8Mb3IcelandicCaseInsensitive:
			case CharacterSet.Utf8Mb3LatvianCaseInsensitive:
			case CharacterSet.Utf8Mb3RomanianCaseInsensitive:
			case CharacterSet.Utf8Mb3SlovenianCaseInsensitive:
			case CharacterSet.Utf8Mb3PolishCaseInsensitive:
			case CharacterSet.Utf8Mb3EstonianCaseInsensitive:
			case CharacterSet.Utf8Mb3SpanishCaseInsensitive:
			case CharacterSet.Utf8Mb3SwedishCaseInsensitive:
			case CharacterSet.Utf8Mb3TurkishCaseInsensitive:
			case CharacterSet.Utf8Mb3CzechCaseInsensitive:
			case CharacterSet.Utf8Mb3DanishCaseInsensitive:
			case CharacterSet.Utf8Mb3LithuanianCaseInsensitive:
			case CharacterSet.Utf8Mb3SlovakCaseInsensitive:
			case CharacterSet.Utf8Mb3Spanish2CaseInsensitive:
			case CharacterSet.Utf8Mb3RomanCaseInsensitive:
			case CharacterSet.Utf8Mb3PersianCaseInsensitive:
			case CharacterSet.Utf8Mb3EsperantoCaseInsensitive:
			case CharacterSet.Utf8Mb3HungarianCaseInsensitive:
			case CharacterSet.Utf8Mb3SinhalaCaseInsensitive:
			case CharacterSet.Utf8Mb3German2CaseInsensitive:
			case CharacterSet.Utf8Mb3CroatianCaseInsensitive:
			case CharacterSet.Utf8Mb3Unicode520CaseInsensitive:
			case CharacterSet.Utf8Mb3VietnameseCaseInsensitive:
			case CharacterSet.Utf8Mb3GeneralMySql500CaseInsensitive:
				goto IL_083c;
			case CharacterSet.Utf8Mb4GeneralCaseInsensitive:
			case CharacterSet.Utf8Mb4Binary:
			case CharacterSet.Utf16GeneralCaseInsensitive:
			case CharacterSet.Utf16Binary:
			case CharacterSet.Utf16leGeneralCaseInsensitive:
			case CharacterSet.Utf32GeneralCaseInsensitive:
			case CharacterSet.Utf32Binary:
			case CharacterSet.Utf16leBinary:
			case CharacterSet.Utf16UnicodeCaseInsensitive:
			case CharacterSet.Utf16IcelandicCaseInsensitive:
			case CharacterSet.Utf16LatvianCaseInsensitive:
			case CharacterSet.Utf16RomanianCaseInsensitive:
			case CharacterSet.Utf16SlovenianCaseInsensitive:
			case CharacterSet.Utf16PolishCaseInsensitive:
			case CharacterSet.Utf16EstonianCaseInsensitive:
			case CharacterSet.Utf16SpanishCaseInsensitive:
			case CharacterSet.Utf16SwedishCaseInsensitive:
			case CharacterSet.Utf16TurkishCaseInsensitive:
			case CharacterSet.Utf16CzechCaseInsensitive:
			case CharacterSet.Utf16DanishCaseInsensitive:
			case CharacterSet.Utf16LithuanianCaseInsensitive:
			case CharacterSet.Utf16SlovakCaseInsensitive:
			case CharacterSet.Utf16Spanish2CaseInsensitive:
			case CharacterSet.Utf16RomanCaseInsensitive:
			case CharacterSet.Utf16PersianCaseInsensitive:
			case CharacterSet.Utf16EsperantoCaseInsensitive:
			case CharacterSet.Utf16HungarianCaseInsensitive:
			case CharacterSet.Utf16SinhalaCaseInsensitive:
			case CharacterSet.Utf16German2CaseInsensitive:
			case CharacterSet.Utf16CroatianCaseInsensitive:
			case CharacterSet.Utf16Unicode520CaseInsensitive:
			case CharacterSet.Utf16VietnameseCaseInsensitive:
			case CharacterSet.Utf32UnicodeCaseInsensitive:
			case CharacterSet.Utf32IcelandicCaseInsensitive:
			case CharacterSet.Utf32LatvianCaseInsensitive:
			case CharacterSet.Utf32RomanianCaseInsensitive:
			case CharacterSet.Utf32SlovenianCaseInsensitive:
			case CharacterSet.Utf32PolishCaseInsensitive:
			case CharacterSet.Utf32EstonianCaseInsensitive:
			case CharacterSet.Utf32SpanishCaseInsensitive:
			case CharacterSet.Utf32SwedishCaseInsensitive:
			case CharacterSet.Utf32TurkishCaseInsensitive:
			case CharacterSet.Utf32CzechCaseInsensitive:
			case CharacterSet.Utf32DanishCaseInsensitive:
			case CharacterSet.Utf32LithuanianCaseInsensitive:
			case CharacterSet.Utf32SlovakCaseInsensitive:
			case CharacterSet.Utf32Spanish2CaseInsensitive:
			case CharacterSet.Utf32RomanCaseInsensitive:
			case CharacterSet.Utf32PersianCaseInsensitive:
			case CharacterSet.Utf32EsperantoCaseInsensitive:
			case CharacterSet.Utf32HungarianCaseInsensitive:
			case CharacterSet.Utf32SinhalaCaseInsensitive:
			case CharacterSet.Utf32German2CaseInsensitive:
			case CharacterSet.Utf32CroatianCaseInsensitive:
			case CharacterSet.Utf32Unicode520CaseInsensitive:
			case CharacterSet.Utf32VietnameseCaseInsensitive:
			case CharacterSet.Utf8Mb4UnicodeCaseInsensitive:
			case CharacterSet.Utf8Mb4IcelandicCaseInsensitive:
			case CharacterSet.Utf8Mb4LatvianCaseInsensitive:
			case CharacterSet.Utf8Mb4RomanianCaseInsensitive:
			case CharacterSet.Utf8Mb4SlovenianCaseInsensitive:
			case CharacterSet.Utf8Mb4PolishCaseInsensitive:
			case CharacterSet.Utf8Mb4EstonianCaseInsensitive:
			case CharacterSet.Utf8Mb4SpanishCaseInsensitive:
			case CharacterSet.Utf8Mb4SwedishCaseInsensitive:
			case CharacterSet.Utf8Mb4TurkishCaseInsensitive:
			case CharacterSet.Utf8Mb4CzechCaseInsensitive:
			case CharacterSet.Utf8Mb4DanishCaseInsensitive:
			case CharacterSet.Utf8Mb4LithuanianCaseInsensitive:
			case CharacterSet.Utf8Mb4SlovakCaseInsensitive:
			case CharacterSet.Utf8Mb4Spanish2CaseInsensitive:
			case CharacterSet.Utf8Mb4RomanCaseInsensitive:
			case CharacterSet.Utf8Mb4PersianCaseInsensitive:
			case CharacterSet.Utf8Mb4EsperantoCaseInsensitive:
			case CharacterSet.Utf8Mb4HungarianCaseInsensitive:
			case CharacterSet.Utf8Mb4SinhalaCaseInsensitive:
			case CharacterSet.Utf8Mb4German2CaseInsensitive:
			case CharacterSet.Utf8Mb4CroatianCaseInsensitive:
			case CharacterSet.Utf8Mb4Unicode520CaseInsensitive:
			case CharacterSet.Utf8Mb4VietnameseCaseInsensitive:
			case CharacterSet.Gb18030ChineseCaseInsensitive:
			case CharacterSet.Gb18030Binary:
			case CharacterSet.Gb18030Unicode520CaseInsensitive:
			case CharacterSet.Utf8Mb4Uca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4GermanPhonebookUca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4IcelandicUca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4LatvianUca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4RomanianUca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4SlovenianUca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4PolishUca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4EstonianUca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4SpanishUca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4SwedishUca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4TurkishUca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4CaseSensitiveUca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4DanishUca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4LithuanianUca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4SlovakUca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4TraditionalSpanishUca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4LatinUca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4EsperantoUca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4HungarianUca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4CroatianUca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4VietnameseUca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4Uca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4GermanPhonebookUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4IcelandicUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4LatvianUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4RomanianUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4SlovenianUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4PolishUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4EstonianUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4SpanishUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4SwedishUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4TurkishUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4CaseSensitiveUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4DanishUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4LithuanianUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4SlovakUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4TraditionalSpanishUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4LatinUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4EsperantoUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4HungarianUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4CroatianUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4VietnameseUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4JapaneseUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4JapaneseUca900AccentSensitiveCaseSensitiveKanaSensitive:
			case CharacterSet.Utf8Mb4Uca900AccentSensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4RussianUca900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4RussianUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4ChineseUca900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4Uca900Binary:
			case CharacterSet.Utf8Mb4NorwegianBokmal0900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4NorwegianBokmal0900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4NorwegianNynorsk0900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4NorwegianNynorsk0900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4SerbianLatin0900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4SerbianLatin0900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4Bosnian0900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4Bosnian0900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4Bulgarian0900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4Bulgarian0900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4Galician0900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4Galician0900AccentSensitiveCaseSensitive:
			case CharacterSet.Utf8Mb4MongolianCyrillic0900AccentInsensitiveCaseInsensitive:
			case CharacterSet.Utf8Mb4MongolianCyrillic0900AccentSensitiveCaseSensitive:
				goto IL_083e;
			case (CharacterSet)17:
			case (CharacterSet)100:
			case (CharacterSet)125:
			case (CharacterSet)126:
			case (CharacterSet)127:
			case (CharacterSet)152:
			case (CharacterSet)153:
			case (CharacterSet)154:
			case (CharacterSet)155:
			case (CharacterSet)156:
			case (CharacterSet)157:
			case (CharacterSet)158:
			case (CharacterSet)184:
			case (CharacterSet)185:
			case (CharacterSet)186:
			case (CharacterSet)187:
			case (CharacterSet)188:
			case (CharacterSet)189:
			case (CharacterSet)190:
			case (CharacterSet)191:
			case (CharacterSet)216:
			case (CharacterSet)217:
			case (CharacterSet)218:
			case (CharacterSet)219:
			case (CharacterSet)220:
			case (CharacterSet)221:
			case (CharacterSet)222:
			case (CharacterSet)251:
			case (CharacterSet)252:
			case (CharacterSet)253:
			case (CharacterSet)254:
			case (CharacterSet)272:
			case (CharacterSet)276:
			case (CharacterSet)295:
			case (CharacterSet)299:
			case (CharacterSet)301:
			case (CharacterSet)302:
				goto IL_0840;
			}
		}
		else
		{
			switch (characterSet)
			{
			case CharacterSet.Dec8SwedishNoPadCaseInsensitive:
			case CharacterSet.Cp850GeneralNoPadCaseInsensitive:
			case CharacterSet.Hp8EnglishNoPadCaseInsensitive:
			case CharacterSet.Koi8rGeneralNoPadCaseInsensitive:
			case CharacterSet.Latin1SwedishNoPadCaseInsensitive:
			case CharacterSet.Latin2GeneralNoPadCaseInsensitive:
			case CharacterSet.Swe7SwedishNoPadCaseInsensitive:
			case CharacterSet.AsciiGeneralNoPadCaseInsensitive:
			case CharacterSet.HebrewGeneralNoPadCaseInsensitive:
			case CharacterSet.Tis620ThaiNoPadCaseInsensitive:
			case CharacterSet.Koi8uGeneralNoPadCaseInsensitive:
			case CharacterSet.GreekGeneralNoPadCaseInsensitive:
			case CharacterSet.Cp1250GeneralNoPadCaseInsensitive:
			case CharacterSet.Latin5TurkishNoPadCaseInsensitive:
			case CharacterSet.Armscii8GeneralNoPadCaseInsensitive:
			case CharacterSet.Cp866GeneralNoPadCaseInsensitive:
			case CharacterSet.Keybcs2GeneralNoPadCaseInsensitive:
			case CharacterSet.MacCentralEuropeanGeneralNoPadCaseInsensitive:
			case CharacterSet.MacRomanGeneralNoPadCaseInsensitive:
			case CharacterSet.Cp852GeneralNoPadCaseInsensitive:
			case CharacterSet.Latin7GeneralNoPadCaseInsensitive:
			case CharacterSet.MacCentralEuropeanNoPadBinary:
			case CharacterSet.Latin1NoPadBinary:
			case CharacterSet.Cp1251NoPadBinary:
			case CharacterSet.Cp1251GeneralNoPadCaseInsensitive:
			case CharacterSet.MacRomanNoPadBinary:
			case CharacterSet.Cp1256GeneralNoPadCaseInsensitive:
			case CharacterSet.Cp1257NoPadBinary:
			case CharacterSet.Cp1257GeneralNoPadCaseInsensitive:
			case CharacterSet.Armscii8NoPadBinary:
			case CharacterSet.AsciiNoPadBinary:
			case CharacterSet.Cp1250NoPadBinary:
			case CharacterSet.Cp1256NoPadBinary:
			case CharacterSet.Cp866NoPadBinary:
			case CharacterSet.Dec8NoPadBinary:
			case CharacterSet.GreekNoPadBinary:
			case CharacterSet.HebrewNoPadBinary:
			case CharacterSet.Hp8NoPadBinary:
			case CharacterSet.Keybcs2NoPadBinary:
			case CharacterSet.Koi8rNoPadBinary:
			case CharacterSet.Koi8uNoPadBinary:
			case CharacterSet.Latin2NoPadBinary:
			case CharacterSet.Latin5NoPadBinary:
			case CharacterSet.Latin7NoPadBinary:
			case CharacterSet.Cp850NoPadBinary:
			case CharacterSet.Cp852NoPadBinary:
			case CharacterSet.Swe7NoPadBinary:
			case CharacterSet.Tis620NoPadBinary:
			case CharacterSet.Geostd8GeneralNoPadCaseInsensitive:
			case CharacterSet.Geostd8NoPadBinary:
				break;
			case CharacterSet.Big5ChineseNoPadCaseInsensitive:
			case CharacterSet.SjisJapaneseNoPadCaseInsensitive:
			case CharacterSet.EuckrKoreanNoPadCaseInsensitive:
			case CharacterSet.Gb2312ChineseNoPadCaseInsensitive:
			case CharacterSet.GbkChineseNoPadCaseInsensitive:
			case CharacterSet.Ucs2GeneralNoPadCaseInsensitive:
			case CharacterSet.Big5NoPadBinary:
			case CharacterSet.EuckrNoPadBinary:
			case CharacterSet.Gb2312NoPadBinary:
			case CharacterSet.GbkNoPadBinary:
			case CharacterSet.SjisNoPadBinary:
			case CharacterSet.Ucs2NoPadBinary:
			case CharacterSet.Cp932JapaneseNoPadCaseInsensitive:
			case CharacterSet.Cp932NoPadBinary:
			case CharacterSet.Ucs2UnicodeNoPadCaseInsensitive:
			case CharacterSet.Ucs2Unicode520NoPadCaseInsensitive:
				goto IL_083a;
			case CharacterSet.UjisJapaneseNoPadCaseInsensitive:
			case CharacterSet.Utf8Mb3GeneralNoPadCaseInsensitive:
			case CharacterSet.Utf8Mb3NoPadBinary:
			case CharacterSet.UjisNoPadBinary:
			case CharacterSet.EucjpmsJapaneseNoPadCaseInsensitive:
			case CharacterSet.EucjpmsNoPadBinary:
			case CharacterSet.Utf8Mb3UnicodeNoPadCaseInsensitive:
			case CharacterSet.Utf8Mb3Unicode520NoPadCaseInsensitive:
				goto IL_083c;
			case CharacterSet.Utf8Mb4GeneralNoPadCaseInsensitive:
			case CharacterSet.Utf8Mb4NoPadBinary:
			case CharacterSet.Utf16GeneralNoPadCaseInsensitive:
			case CharacterSet.Utf16NoPadBinary:
			case CharacterSet.Utf16leGeneralNoPadCaseInsensitive:
			case CharacterSet.Utf32GeneralNoPadCaseInsensitive:
			case CharacterSet.Utf32NoPadBinary:
			case CharacterSet.Utf16leNoPadBinary:
			case CharacterSet.Utf16UnicodeNoPadCaseInsensitive:
			case CharacterSet.Utf16Unicode520NoPadCaseInsensitive:
			case CharacterSet.Utf32UnicodeNoPadCaseInsensitive:
			case CharacterSet.Utf32Unicode520NoPadCaseInsensitive:
			case CharacterSet.Utf8Mb4UnicodeNoPadCaseInsensitive:
			case CharacterSet.Utf8Mb4Unicode520NoPadCaseInsensitive:
				goto IL_083e;
			default:
				goto IL_0840;
			}
		}
		return 1;
		IL_0840:
		throw new NotSupportedException($"Maximum byte length of character set {characterSet} is unknown.");
		IL_083e:
		return 4;
		IL_083c:
		return 3;
		IL_083a:
		return 2;
	}

	[return: _003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)]
	public static async ValueTask<ArraySegment<byte>> ReadPayloadAsync(BufferedByteReader bufferedByteReader, IByteHandler byteHandler, Func<int> getNextSequenceNumber, ArraySegmentHolder<byte> previousPayloads, ProtocolErrorBehavior protocolErrorBehavior, IOBehavior ioBehavior)
	{
		previousPayloads.Clear();
		Packet packet;
		do
		{
			ArraySegment<byte> segment = await bufferedByteReader.ReadBytesAsync(byteHandler, 4, ioBehavior).ConfigureAwait(continueOnCapturedContext: false);
			if (segment.Count < 4)
			{
				if (protocolErrorBehavior != ProtocolErrorBehavior.Ignore)
				{
					throw new MySqlEndOfStreamException(4, segment.Count);
				}
				return default(ArraySegment<byte>);
			}
			int payloadLength = (int)SerializationUtility.ReadUInt32(MemoryExtensions.AsSpan(segment).Slice(0, 3));
			int packetSequenceNumber = MemoryExtensions.AsSpan(segment)[3];
			int expectedSequenceNumber = getNextSequenceNumber() % 256;
			ArraySegment<byte> arraySegment = await bufferedByteReader.ReadBytesAsync(byteHandler, payloadLength, ioBehavior).ConfigureAwait(continueOnCapturedContext: false);
			if (expectedSequenceNumber != -1 && packetSequenceNumber != expectedSequenceNumber)
			{
				if (protocolErrorBehavior == ProtocolErrorBehavior.Ignore)
				{
					packet = default(Packet);
				}
				else
				{
					if (arraySegment.Count <= 0 || MemoryExtensions.AsSpan(arraySegment)[0] != byte.MaxValue)
					{
						throw MySqlProtocolException.CreateForPacketOutOfOrder(expectedSequenceNumber, packetSequenceNumber);
					}
					packet = new Packet(arraySegment);
				}
			}
			else
			{
				Packet packet2;
				if (arraySegment.Count < payloadLength)
				{
					if (protocolErrorBehavior == ProtocolErrorBehavior.Throw)
					{
						throw new MySqlEndOfStreamException(payloadLength, arraySegment.Count);
					}
					packet2 = default(Packet);
				}
				else
				{
					packet2 = new Packet(arraySegment);
				}
				packet = packet2;
			}
			if (previousPayloads.Count == 0 && packet.Contents.Count < 16777215)
			{
				return packet.Contents;
			}
			byte[] array = previousPayloads.Array;
			if (array == null)
			{
				array = new byte[16777216];
			}
			else if (previousPayloads.Offset + previousPayloads.Count + packet.Contents.Count > array.Length)
			{
				Array.Resize(ref array, array.Length * 2);
			}
			MemoryExtensions.AsSpan(packet.Contents).CopyTo(MemoryExtensions.AsSpan(array, previousPayloads.Offset + previousPayloads.Count));
			previousPayloads.ArraySegment = new ArraySegment<byte>(array, previousPayloads.Offset, previousPayloads.Count + packet.Contents.Count);
		}
		while (packet.Contents.Count >= 16777215);
		return previousPayloads.ArraySegment;
	}

	public static async ValueTask WritePayloadAsync(IByteHandler byteHandler, Func<int> getNextSequenceNumber, [_003C63cbfc6b_002D8364_002D4ba4_002D98c4_002D143f3fb7bf67_003ENullable(0)] ReadOnlyMemory<byte> payload, IOBehavior ioBehavior)
	{
		byte[] buffer = ArrayPool<byte>.Shared.Rent(Math.Min(16777215, payload.Length) + 4);
		try
		{
			int bytesSent = 0;
			do
			{
				ReadOnlyMemory<byte> contents = payload.Slice(bytesSent, Math.Min(16777215, payload.Length - bytesSent));
				int count = contents.Length + 4;
				SerializationUtility.WriteUInt32((uint)contents.Length, buffer, 0, 3);
				buffer[3] = (byte)getNextSequenceNumber();
				contents.CopyTo(MemoryExtensions.AsMemory(buffer, 4));
				await byteHandler.WriteBytesAsync(new ArraySegment<byte>(buffer, 0, count), ioBehavior).ConfigureAwait(continueOnCapturedContext: false);
				bytesSent += contents.Length;
			}
			while (bytesSent < payload.Length);
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(buffer);
		}
	}
}
