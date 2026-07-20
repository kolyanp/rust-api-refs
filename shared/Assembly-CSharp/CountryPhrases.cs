using System;
using System.Collections.Generic;
using UnityEngine;

public static class CountryPhrases
{
	[ClientVar]
	public static bool debug_browser_codes = false;

	private static readonly Phrase Country_AF = new Phrase("country_AF", "Afghanistan");

	private static readonly Phrase Country_AX = new Phrase("country_AX", "Åland Islands");

	private static readonly Phrase Country_AL = new Phrase("country_AL", "Albania");

	private static readonly Phrase Country_DZ = new Phrase("country_DZ", "Algeria");

	private static readonly Phrase Country_AS = new Phrase("country_AS", "American Samoa");

	private static readonly Phrase Country_AD = new Phrase("country_AD", "Andorra");

	private static readonly Phrase Country_AO = new Phrase("country_AO", "Angola");

	private static readonly Phrase Country_AI = new Phrase("country_AI", "Anguilla");

	private static readonly Phrase Country_AQ = new Phrase("country_AQ", "Antarctica");

	private static readonly Phrase Country_AG = new Phrase("country_AG", "Antigua and Barbuda");

	private static readonly Phrase Country_AR = new Phrase("country_AR", "Argentina");

	private static readonly Phrase Country_AM = new Phrase("country_AM", "Armenia");

	private static readonly Phrase Country_AW = new Phrase("country_AW", "Aruba");

	private static readonly Phrase Country_AU = new Phrase("country_AU", "Australia");

	private static readonly Phrase Country_AT = new Phrase("country_AT", "Austria");

	private static readonly Phrase Country_AZ = new Phrase("country_AZ", "Azerbaijan");

	private static readonly Phrase Country_BS = new Phrase("country_BS", "Bahamas");

	private static readonly Phrase Country_BH = new Phrase("country_BH", "Bahrain");

	private static readonly Phrase Country_BD = new Phrase("country_BD", "Bangladesh");

	private static readonly Phrase Country_BB = new Phrase("country_BB", "Barbados");

	private static readonly Phrase Country_BY = new Phrase("country_BY", "Belarus");

	private static readonly Phrase Country_BE = new Phrase("country_BE", "Belgium");

	private static readonly Phrase Country_BZ = new Phrase("country_BZ", "Belize");

	private static readonly Phrase Country_BJ = new Phrase("country_BJ", "Benin");

	private static readonly Phrase Country_BM = new Phrase("country_BM", "Bermuda");

	private static readonly Phrase Country_BT = new Phrase("country_BT", "Bhutan");

	private static readonly Phrase Country_BO = new Phrase("country_BO", "Bolivia");

	private static readonly Phrase Country_BA = new Phrase("country_BA", "Bosnia and Herzegovina");

	private static readonly Phrase Country_BW = new Phrase("country_BW", "Botswana");

	private static readonly Phrase Country_BV = new Phrase("country_BV", "Bouvet Island");

	private static readonly Phrase Country_BR = new Phrase("country_BR", "Brazil");

	private static readonly Phrase Country_IO = new Phrase("country_IO", "British Indian Ocean Territory");

	private static readonly Phrase Country_BN = new Phrase("country_BN", "Brunei");

	private static readonly Phrase Country_BG = new Phrase("country_BG", "Bulgaria");

	private static readonly Phrase Country_BF = new Phrase("country_BF", "Burkina Faso");

	private static readonly Phrase Country_BI = new Phrase("country_BI", "Burundi");

	private static readonly Phrase Country_CV = new Phrase("country_CV", "Cabo Verde");

	private static readonly Phrase Country_KH = new Phrase("country_KH", "Cambodia");

	private static readonly Phrase Country_CM = new Phrase("country_CM", "Cameroon");

	private static readonly Phrase Country_CA = new Phrase("country_CA", "Canada");

	private static readonly Phrase Country_CF = new Phrase("country_CF", "Central African Republic");

	private static readonly Phrase Country_TD = new Phrase("country_TD", "Chad");

	private static readonly Phrase Country_CL = new Phrase("country_CL", "Chile");

	private static readonly Phrase Country_CN = new Phrase("country_CN", "China");

	private static readonly Phrase Country_CX = new Phrase("country_CX", "Christmas Island");

	private static readonly Phrase Country_CC = new Phrase("country_CC", "Cocos (Keeling) Islands");

	private static readonly Phrase Country_CO = new Phrase("country_CO", "Colombia");

	private static readonly Phrase Country_KM = new Phrase("country_KM", "Comoros");

	private static readonly Phrase Country_CG = new Phrase("country_CG", "Congo");

	private static readonly Phrase Country_CD = new Phrase("country_CD", "Congo (DRC)");

	private static readonly Phrase Country_CK = new Phrase("country_CK", "Cook Islands");

	private static readonly Phrase Country_CR = new Phrase("country_CR", "Costa Rica");

	private static readonly Phrase Country_CI = new Phrase("country_CI", "Côte d’Ivoire");

	private static readonly Phrase Country_HR = new Phrase("country_HR", "Croatia");

	private static readonly Phrase Country_CU = new Phrase("country_CU", "Cuba");

	private static readonly Phrase Country_CW = new Phrase("country_CW", "Curaçao");

	private static readonly Phrase Country_CY = new Phrase("country_CY", "Cyprus");

	private static readonly Phrase Country_CZ = new Phrase("country_CZ", "Czechia");

	private static readonly Phrase Country_DK = new Phrase("country_DK", "Denmark");

	private static readonly Phrase Country_DJ = new Phrase("country_DJ", "Djibouti");

	private static readonly Phrase Country_DM = new Phrase("country_DM", "Dominica");

	private static readonly Phrase Country_DO = new Phrase("country_DO", "Dominican Republic");

	private static readonly Phrase Country_EC = new Phrase("country_EC", "Ecuador");

	private static readonly Phrase Country_EG = new Phrase("country_EG", "Egypt");

	private static readonly Phrase Country_SV = new Phrase("country_SV", "El Salvador");

	private static readonly Phrase Country_GQ = new Phrase("country_GQ", "Equatorial Guinea");

	private static readonly Phrase Country_ER = new Phrase("country_ER", "Eritrea");

	private static readonly Phrase Country_EE = new Phrase("country_EE", "Estonia");

	private static readonly Phrase Country_SZ = new Phrase("country_SZ", "Eswatini");

	private static readonly Phrase Country_ET = new Phrase("country_ET", "Ethiopia");

	private static readonly Phrase Country_FK = new Phrase("country_FK", "Falkland Islands");

	private static readonly Phrase Country_FO = new Phrase("country_FO", "Faroe Islands");

	private static readonly Phrase Country_FJ = new Phrase("country_FJ", "Fiji");

	private static readonly Phrase Country_FI = new Phrase("country_FI", "Finland");

	private static readonly Phrase Country_FR = new Phrase("country_FR", "France");

	private static readonly Phrase Country_GF = new Phrase("country_GF", "French Guiana");

	private static readonly Phrase Country_PF = new Phrase("country_PF", "French Polynesia");

	private static readonly Phrase Country_TF = new Phrase("country_TF", "French Southern Territories");

	private static readonly Phrase Country_GA = new Phrase("country_GA", "Gabon");

	private static readonly Phrase Country_GM = new Phrase("country_GM", "Gambia");

	private static readonly Phrase Country_GE = new Phrase("country_GE", "Georgia");

	private static readonly Phrase Country_DE = new Phrase("country_DE", "Germany");

	private static readonly Phrase Country_GH = new Phrase("country_GH", "Ghana");

	private static readonly Phrase Country_GI = new Phrase("country_GI", "Gibraltar");

	private static readonly Phrase Country_GR = new Phrase("country_GR", "Greece");

	private static readonly Phrase Country_GL = new Phrase("country_GL", "Greenland");

	private static readonly Phrase Country_GD = new Phrase("country_GD", "Grenada");

	private static readonly Phrase Country_GP = new Phrase("country_GP", "Guadeloupe");

	private static readonly Phrase Country_GU = new Phrase("country_GU", "Guam");

	private static readonly Phrase Country_GT = new Phrase("country_GT", "Guatemala");

	private static readonly Phrase Country_GG = new Phrase("country_GG", "Guernsey");

	private static readonly Phrase Country_GN = new Phrase("country_GN", "Guinea");

	private static readonly Phrase Country_GW = new Phrase("country_GW", "Guinea-Bissau");

	private static readonly Phrase Country_GY = new Phrase("country_GY", "Guyana");

	private static readonly Phrase Country_HT = new Phrase("country_HT", "Haiti");

	private static readonly Phrase Country_HM = new Phrase("country_HM", "Heard Island and McDonald Islands");

	private static readonly Phrase Country_VA = new Phrase("country_VA", "Holy See");

	private static readonly Phrase Country_HN = new Phrase("country_HN", "Honduras");

	private static readonly Phrase Country_HK = new Phrase("country_HK", "Hong Kong");

	private static readonly Phrase Country_HU = new Phrase("country_HU", "Hungary");

	private static readonly Phrase Country_IS = new Phrase("country_IS", "Iceland");

	private static readonly Phrase Country_IN = new Phrase("country_IN", "India");

	private static readonly Phrase Country_ID = new Phrase("country_ID", "Indonesia");

	private static readonly Phrase Country_IR = new Phrase("country_IR", "Iran");

	private static readonly Phrase Country_IQ = new Phrase("country_IQ", "Iraq");

	private static readonly Phrase Country_IE = new Phrase("country_IE", "Ireland");

	private static readonly Phrase Country_IM = new Phrase("country_IM", "Isle of Man");

	private static readonly Phrase Country_IL = new Phrase("country_IL", "Israel");

	private static readonly Phrase Country_IT = new Phrase("country_IT", "Italy");

	private static readonly Phrase Country_JM = new Phrase("country_JM", "Jamaica");

	private static readonly Phrase Country_JP = new Phrase("country_JP", "Japan");

	private static readonly Phrase Country_JE = new Phrase("country_JE", "Jersey");

	private static readonly Phrase Country_JO = new Phrase("country_JO", "Jordan");

	private static readonly Phrase Country_KY = new Phrase("country_KY", "Cayman Islands");

	private static readonly Phrase Country_KZ = new Phrase("country_KZ", "Kazakhstan");

	private static readonly Phrase Country_KE = new Phrase("country_KE", "Kenya");

	private static readonly Phrase Country_KI = new Phrase("country_KI", "Kiribati");

	private static readonly Phrase Country_KP = new Phrase("country_KP", "North Korea");

	private static readonly Phrase Country_KR = new Phrase("country_KR", "South Korea");

	private static readonly Phrase Country_KW = new Phrase("country_KW", "Kuwait");

	private static readonly Phrase Country_KG = new Phrase("country_KG", "Kyrgyzstan");

	private static readonly Phrase Country_LA = new Phrase("country_LA", "Laos");

	private static readonly Phrase Country_LV = new Phrase("country_LV", "Latvia");

	private static readonly Phrase Country_LB = new Phrase("country_LB", "Lebanon");

	private static readonly Phrase Country_LS = new Phrase("country_LS", "Lesotho");

	private static readonly Phrase Country_LR = new Phrase("country_LR", "Liberia");

	private static readonly Phrase Country_LY = new Phrase("country_LY", "Libya");

	private static readonly Phrase Country_LI = new Phrase("country_LI", "Liechtenstein");

	private static readonly Phrase Country_LT = new Phrase("country_LT", "Lithuania");

	private static readonly Phrase Country_LU = new Phrase("country_LU", "Luxembourg");

	private static readonly Phrase Country_MO = new Phrase("country_MO", "Macao");

	private static readonly Phrase Country_MG = new Phrase("country_MG", "Madagascar");

	private static readonly Phrase Country_MW = new Phrase("country_MW", "Malawi");

	private static readonly Phrase Country_MY = new Phrase("country_MY", "Malaysia");

	private static readonly Phrase Country_MV = new Phrase("country_MV", "Maldives");

	private static readonly Phrase Country_ML = new Phrase("country_ML", "Mali");

	private static readonly Phrase Country_MT = new Phrase("country_MT", "Malta");

	private static readonly Phrase Country_MH = new Phrase("country_MH", "Marshall Islands");

	private static readonly Phrase Country_MQ = new Phrase("country_MQ", "Martinique");

	private static readonly Phrase Country_MR = new Phrase("country_MR", "Mauritania");

	private static readonly Phrase Country_MU = new Phrase("country_MU", "Mauritius");

	private static readonly Phrase Country_YT = new Phrase("country_YT", "Mayotte");

	private static readonly Phrase Country_MX = new Phrase("country_MX", "Mexico");

	private static readonly Phrase Country_FM = new Phrase("country_FM", "Micronesia");

	private static readonly Phrase Country_MD = new Phrase("country_MD", "Moldova");

	private static readonly Phrase Country_MC = new Phrase("country_MC", "Monaco");

	private static readonly Phrase Country_MN = new Phrase("country_MN", "Mongolia");

	private static readonly Phrase Country_ME = new Phrase("country_ME", "Montenegro");

	private static readonly Phrase Country_MS = new Phrase("country_MS", "Montserrat");

	private static readonly Phrase Country_MA = new Phrase("country_MA", "Morocco");

	private static readonly Phrase Country_MZ = new Phrase("country_MZ", "Mozambique");

	private static readonly Phrase Country_MM = new Phrase("country_MM", "Myanmar");

	private static readonly Phrase Country_NA = new Phrase("country_NA", "Namibia");

	private static readonly Phrase Country_NR = new Phrase("country_NR", "Nauru");

	private static readonly Phrase Country_NP = new Phrase("country_NP", "Nepal");

	private static readonly Phrase Country_NL = new Phrase("country_NL", "Netherlands");

	private static readonly Phrase Country_NC = new Phrase("country_NC", "New Caledonia");

	private static readonly Phrase Country_NZ = new Phrase("country_NZ", "New Zealand");

	private static readonly Phrase Country_NI = new Phrase("country_NI", "Nicaragua");

	private static readonly Phrase Country_NE = new Phrase("country_NE", "Niger");

	private static readonly Phrase Country_NG = new Phrase("country_NG", "Nigeria");

	private static readonly Phrase Country_NU = new Phrase("country_NU", "Niue");

	private static readonly Phrase Country_NF = new Phrase("country_NF", "Norfolk Island");

	private static readonly Phrase Country_MK = new Phrase("country_MK", "North Macedonia");

	private static readonly Phrase Country_MP = new Phrase("country_MP", "Northern Mariana Islands");

	private static readonly Phrase Country_NO = new Phrase("country_NO", "Norway");

	private static readonly Phrase Country_OM = new Phrase("country_OM", "Oman");

	private static readonly Phrase Country_PK = new Phrase("country_PK", "Pakistan");

	private static readonly Phrase Country_PW = new Phrase("country_PW", "Palau");

	private static readonly Phrase Country_PS = new Phrase("country_PS", "Palestine");

	private static readonly Phrase Country_PA = new Phrase("country_PA", "Panama");

	private static readonly Phrase Country_PG = new Phrase("country_PG", "Papua New Guinea");

	private static readonly Phrase Country_PY = new Phrase("country_PY", "Paraguay");

	private static readonly Phrase Country_PE = new Phrase("country_PE", "Peru");

	private static readonly Phrase Country_PH = new Phrase("country_PH", "Philippines");

	private static readonly Phrase Country_PN = new Phrase("country_PN", "Pitcairn");

	private static readonly Phrase Country_PL = new Phrase("country_PL", "Poland");

	private static readonly Phrase Country_PT = new Phrase("country_PT", "Portugal");

	private static readonly Phrase Country_PR = new Phrase("country_PR", "Puerto Rico");

	private static readonly Phrase Country_QA = new Phrase("country_QA", "Qatar");

	private static readonly Phrase Country_RE = new Phrase("country_RE", "Réunion");

	private static readonly Phrase Country_RO = new Phrase("country_RO", "Romania");

	private static readonly Phrase Country_RU = new Phrase("country_RU", "Russia");

	private static readonly Phrase Country_RW = new Phrase("country_RW", "Rwanda");

	private static readonly Phrase Country_BL = new Phrase("country_BL", "Saint Barthélemy");

	private static readonly Phrase Country_SH = new Phrase("country_SH", "Saint Helena");

	private static readonly Phrase Country_KN = new Phrase("country_KN", "Saint Kitts and Nevis");

	private static readonly Phrase Country_LC = new Phrase("country_LC", "Saint Lucia");

	private static readonly Phrase Country_MF = new Phrase("country_MF", "Saint Martin");

	private static readonly Phrase Country_PM = new Phrase("country_PM", "Saint Pierre and Miquelon");

	private static readonly Phrase Country_VC = new Phrase("country_VC", "Saint Vincent and the Grenadines");

	private static readonly Phrase Country_WS = new Phrase("country_WS", "Samoa");

	private static readonly Phrase Country_SM = new Phrase("country_SM", "San Marino");

	private static readonly Phrase Country_ST = new Phrase("country_ST", "São Tomé and Príncipe");

	private static readonly Phrase Country_SA = new Phrase("country_SA", "Saudi Arabia");

	private static readonly Phrase Country_SN = new Phrase("country_SN", "Senegal");

	private static readonly Phrase Country_RS = new Phrase("country_RS", "Serbia");

	private static readonly Phrase Country_SC = new Phrase("country_SC", "Seychelles");

	private static readonly Phrase Country_SL = new Phrase("country_SL", "Sierra Leone");

	private static readonly Phrase Country_SG = new Phrase("country_SG", "Singapore");

	private static readonly Phrase Country_SX = new Phrase("country_SX", "Sint Maarten");

	private static readonly Phrase Country_SK = new Phrase("country_SK", "Slovakia");

	private static readonly Phrase Country_SI = new Phrase("country_SI", "Slovenia");

	private static readonly Phrase Country_SB = new Phrase("country_SB", "Solomon Islands");

	private static readonly Phrase Country_SO = new Phrase("country_SO", "Somalia");

	private static readonly Phrase Country_ZA = new Phrase("country_ZA", "South Africa");

	private static readonly Phrase Country_GS = new Phrase("country_GS", "South Georgia and the South Sandwich Islands");

	private static readonly Phrase Country_SS = new Phrase("country_SS", "South Sudan");

	private static readonly Phrase Country_ES = new Phrase("country_ES", "Spain");

	private static readonly Phrase Country_LK = new Phrase("country_LK", "Sri Lanka");

	private static readonly Phrase Country_SD = new Phrase("country_SD", "Sudan");

	private static readonly Phrase Country_SR = new Phrase("country_SR", "Suriname");

	private static readonly Phrase Country_SJ = new Phrase("country_SJ", "Svalbard and Jan Mayen");

	private static readonly Phrase Country_SE = new Phrase("country_SE", "Sweden");

	private static readonly Phrase Country_CH = new Phrase("country_CH", "Switzerland");

	private static readonly Phrase Country_SY = new Phrase("country_SY", "Syria");

	private static readonly Phrase Country_TW = new Phrase("country_TW", "Taiwan");

	private static readonly Phrase Country_TJ = new Phrase("country_TJ", "Tajikistan");

	private static readonly Phrase Country_TZ = new Phrase("country_TZ", "Tanzania");

	private static readonly Phrase Country_TH = new Phrase("country_TH", "Thailand");

	private static readonly Phrase Country_TL = new Phrase("country_TL", "Timor-Leste");

	private static readonly Phrase Country_TG = new Phrase("country_TG", "Togo");

	private static readonly Phrase Country_TK = new Phrase("country_TK", "Tokelau");

	private static readonly Phrase Country_TO = new Phrase("country_TO", "Tonga");

	private static readonly Phrase Country_TT = new Phrase("country_TT", "Trinidad and Tobago");

	private static readonly Phrase Country_TN = new Phrase("country_TN", "Tunisia");

	private static readonly Phrase Country_TR = new Phrase("country_TR", "Turkey");

	private static readonly Phrase Country_TM = new Phrase("country_TM", "Turkmenistan");

	private static readonly Phrase Country_TC = new Phrase("country_TC", "Turks and Caicos Islands");

	private static readonly Phrase Country_TV = new Phrase("country_TV", "Tuvalu");

	private static readonly Phrase Country_UG = new Phrase("country_UG", "Uganda");

	private static readonly Phrase Country_UA = new Phrase("country_UA", "Ukraine");

	private static readonly Phrase Country_AE = new Phrase("country_AE", "UAE");

	private static readonly Phrase Country_GB = new Phrase("country_GB", "United Kingdom");

	private static readonly Phrase Country_US = new Phrase("country_US", "United States");

	private static readonly Phrase Country_UM = new Phrase("country_UM", "United States Minor Outlying Islands");

	private static readonly Phrase Country_UY = new Phrase("country_UY", "Uruguay");

	private static readonly Phrase Country_UZ = new Phrase("country_UZ", "Uzbekistan");

	private static readonly Phrase Country_VU = new Phrase("country_VU", "Vanuatu");

	private static readonly Phrase Country_VE = new Phrase("country_VE", "Venezuela");

	private static readonly Phrase Country_VN = new Phrase("country_VN", "Vietnam");

	private static readonly Phrase Country_VG = new Phrase("country_VG", "Virgin Islands (British)");

	private static readonly Phrase Country_VI = new Phrase("country_VI", "Virgin Islands (U.S.)");

	private static readonly Phrase Country_WF = new Phrase("country_WF", "Wallis and Futuna");

	private static readonly Phrase Country_EH = new Phrase("country_EH", "Western Sahara");

	private static readonly Phrase Country_YE = new Phrase("country_YE", "Yemen");

	private static readonly Phrase Country_ZM = new Phrase("country_ZM", "Zambia");

	private static readonly Phrase Country_ZW = new Phrase("country_ZW", "Zimbabwe");

	public static readonly Phrase Country_UNKNOWN = new Phrase("country_UNKNOWN", "Unknown");

	private static readonly Dictionary<string, Phrase> _phrases = new Dictionary<string, Phrase>(StringComparer.OrdinalIgnoreCase)
	{
		["AF"] = Country_AF,
		["AX"] = Country_AX,
		["AL"] = Country_AL,
		["DZ"] = Country_DZ,
		["AS"] = Country_AS,
		["AD"] = Country_AD,
		["AO"] = Country_AO,
		["AI"] = Country_AI,
		["AQ"] = Country_AQ,
		["AG"] = Country_AG,
		["AR"] = Country_AR,
		["AM"] = Country_AM,
		["AW"] = Country_AW,
		["AU"] = Country_AU,
		["AT"] = Country_AT,
		["AZ"] = Country_AZ,
		["BS"] = Country_BS,
		["BH"] = Country_BH,
		["BD"] = Country_BD,
		["BB"] = Country_BB,
		["BY"] = Country_BY,
		["BE"] = Country_BE,
		["BZ"] = Country_BZ,
		["BJ"] = Country_BJ,
		["BM"] = Country_BM,
		["BT"] = Country_BT,
		["BO"] = Country_BO,
		["BA"] = Country_BA,
		["BW"] = Country_BW,
		["BV"] = Country_BV,
		["BR"] = Country_BR,
		["IO"] = Country_IO,
		["BN"] = Country_BN,
		["BG"] = Country_BG,
		["BF"] = Country_BF,
		["BI"] = Country_BI,
		["CV"] = Country_CV,
		["KH"] = Country_KH,
		["CM"] = Country_CM,
		["CA"] = Country_CA,
		["KY"] = Country_KY,
		["CF"] = Country_CF,
		["TD"] = Country_TD,
		["CL"] = Country_CL,
		["CN"] = Country_CN,
		["CX"] = Country_CX,
		["CC"] = Country_CC,
		["CO"] = Country_CO,
		["KM"] = Country_KM,
		["CG"] = Country_CG,
		["CD"] = Country_CD,
		["CK"] = Country_CK,
		["CR"] = Country_CR,
		["CI"] = Country_CI,
		["HR"] = Country_HR,
		["CU"] = Country_CU,
		["CW"] = Country_CW,
		["CY"] = Country_CY,
		["CZ"] = Country_CZ,
		["DK"] = Country_DK,
		["DJ"] = Country_DJ,
		["DM"] = Country_DM,
		["DO"] = Country_DO,
		["EC"] = Country_EC,
		["EG"] = Country_EG,
		["SV"] = Country_SV,
		["GQ"] = Country_GQ,
		["ER"] = Country_ER,
		["EE"] = Country_EE,
		["SZ"] = Country_SZ,
		["ET"] = Country_ET,
		["FK"] = Country_FK,
		["FO"] = Country_FO,
		["FJ"] = Country_FJ,
		["FI"] = Country_FI,
		["FR"] = Country_FR,
		["GF"] = Country_GF,
		["PF"] = Country_PF,
		["TF"] = Country_TF,
		["GA"] = Country_GA,
		["GM"] = Country_GM,
		["GE"] = Country_GE,
		["DE"] = Country_DE,
		["GH"] = Country_GH,
		["GI"] = Country_GI,
		["GR"] = Country_GR,
		["GL"] = Country_GL,
		["GD"] = Country_GD,
		["GP"] = Country_GP,
		["GU"] = Country_GU,
		["GT"] = Country_GT,
		["GG"] = Country_GG,
		["GN"] = Country_GN,
		["GW"] = Country_GW,
		["GY"] = Country_GY,
		["HT"] = Country_HT,
		["HM"] = Country_HM,
		["VA"] = Country_VA,
		["HN"] = Country_HN,
		["HK"] = Country_HK,
		["HU"] = Country_HU,
		["IS"] = Country_IS,
		["IN"] = Country_IN,
		["ID"] = Country_ID,
		["IR"] = Country_IR,
		["IQ"] = Country_IQ,
		["IE"] = Country_IE,
		["IM"] = Country_IM,
		["IL"] = Country_IL,
		["IT"] = Country_IT,
		["JM"] = Country_JM,
		["JP"] = Country_JP,
		["JE"] = Country_JE,
		["JO"] = Country_JO,
		["KZ"] = Country_KZ,
		["KE"] = Country_KE,
		["KI"] = Country_KI,
		["KP"] = Country_KP,
		["KR"] = Country_KR,
		["KW"] = Country_KW,
		["KG"] = Country_KG,
		["LA"] = Country_LA,
		["LV"] = Country_LV,
		["LB"] = Country_LB,
		["LS"] = Country_LS,
		["LR"] = Country_LR,
		["LY"] = Country_LY,
		["LI"] = Country_LI,
		["LT"] = Country_LT,
		["LU"] = Country_LU,
		["MO"] = Country_MO,
		["MG"] = Country_MG,
		["MW"] = Country_MW,
		["MY"] = Country_MY,
		["MV"] = Country_MV,
		["ML"] = Country_ML,
		["MT"] = Country_MT,
		["MH"] = Country_MH,
		["MQ"] = Country_MQ,
		["MR"] = Country_MR,
		["MU"] = Country_MU,
		["YT"] = Country_YT,
		["MX"] = Country_MX,
		["FM"] = Country_FM,
		["MD"] = Country_MD,
		["MC"] = Country_MC,
		["MN"] = Country_MN,
		["ME"] = Country_ME,
		["MS"] = Country_MS,
		["MA"] = Country_MA,
		["MZ"] = Country_MZ,
		["MM"] = Country_MM,
		["NA"] = Country_NA,
		["NR"] = Country_NR,
		["NP"] = Country_NP,
		["NL"] = Country_NL,
		["NC"] = Country_NC,
		["NZ"] = Country_NZ,
		["NI"] = Country_NI,
		["NE"] = Country_NE,
		["NG"] = Country_NG,
		["NU"] = Country_NU,
		["NF"] = Country_NF,
		["MK"] = Country_MK,
		["MP"] = Country_MP,
		["NO"] = Country_NO,
		["OM"] = Country_OM,
		["PK"] = Country_PK,
		["PW"] = Country_PW,
		["PS"] = Country_PS,
		["PA"] = Country_PA,
		["PG"] = Country_PG,
		["PY"] = Country_PY,
		["PE"] = Country_PE,
		["PH"] = Country_PH,
		["PN"] = Country_PN,
		["PL"] = Country_PL,
		["PT"] = Country_PT,
		["PR"] = Country_PR,
		["QA"] = Country_QA,
		["RE"] = Country_RE,
		["RO"] = Country_RO,
		["RU"] = Country_RU,
		["RW"] = Country_RW,
		["BL"] = Country_BL,
		["SH"] = Country_SH,
		["KN"] = Country_KN,
		["LC"] = Country_LC,
		["MF"] = Country_MF,
		["PM"] = Country_PM,
		["VC"] = Country_VC,
		["WS"] = Country_WS,
		["SM"] = Country_SM,
		["ST"] = Country_ST,
		["SA"] = Country_SA,
		["SN"] = Country_SN,
		["RS"] = Country_RS,
		["SC"] = Country_SC,
		["SL"] = Country_SL,
		["SG"] = Country_SG,
		["SX"] = Country_SX,
		["SK"] = Country_SK,
		["SI"] = Country_SI,
		["SB"] = Country_SB,
		["SO"] = Country_SO,
		["ZA"] = Country_ZA,
		["GS"] = Country_GS,
		["SS"] = Country_SS,
		["ES"] = Country_ES,
		["LK"] = Country_LK,
		["SD"] = Country_SD,
		["SR"] = Country_SR,
		["SJ"] = Country_SJ,
		["SE"] = Country_SE,
		["CH"] = Country_CH,
		["SY"] = Country_SY,
		["TW"] = Country_TW,
		["TJ"] = Country_TJ,
		["TZ"] = Country_TZ,
		["TH"] = Country_TH,
		["TL"] = Country_TL,
		["TG"] = Country_TG,
		["TK"] = Country_TK,
		["TO"] = Country_TO,
		["TT"] = Country_TT,
		["TN"] = Country_TN,
		["TR"] = Country_TR,
		["TM"] = Country_TM,
		["TC"] = Country_TC,
		["TV"] = Country_TV,
		["UG"] = Country_UG,
		["UA"] = Country_UA,
		["AE"] = Country_AE,
		["GB"] = Country_GB,
		["US"] = Country_US,
		["UM"] = Country_UM,
		["UY"] = Country_UY,
		["UZ"] = Country_UZ,
		["VU"] = Country_VU,
		["VE"] = Country_VE,
		["VN"] = Country_VN,
		["VG"] = Country_VG,
		["VI"] = Country_VI,
		["WF"] = Country_WF,
		["EH"] = Country_EH,
		["YE"] = Country_YE,
		["ZM"] = Country_ZM,
		["ZW"] = Country_ZW
	};

	public static IReadOnlyDictionary<string, Phrase> AllCountries => _phrases;

	public static Phrase Get(string code)
	{
		if (string.IsNullOrEmpty(code))
		{
			if (debug_browser_codes)
			{
				Debug.Log((object)"Country code was null or empty");
			}
			return Country_UNKNOWN;
		}
		if (_phrases.TryGetValue(code, out var value))
		{
			return value;
		}
		if (debug_browser_codes)
		{
			Debug.Log((object)("Couldn't find country phrase for code '" + code + "'"));
		}
		return Country_UNKNOWN;
	}
}
