using UnityEngine;

namespace PixelCrushers.DialogueSystem {
		
	/// <summary>
	/// This static class contains localization methods and the current language as
	/// defined by a string (e.g., "es" for generic Spanish, "fr-CA" for French - Canadian).
	/// </summary>
	public static class Localization {
		
		/// <summary>
		/// Gets or sets the current language.
		/// </summary>
		/// <value>
		/// The language.
		/// </value>
		public static string Language { get; set; }
		
		/// <summary>
		/// Indicates whether localization is set to use default text instead of localized text.
		/// </summary>
		/// <value>
		/// <c>true</c> if default text should be used. If <c>false</c>, the language specified
		/// by the Language property should be used.
		/// </value>
		public static bool IsDefaultLanguage {
			get { return string.IsNullOrEmpty(Language); }
		}
		
		/// <summary>
		/// Converts a Unity SystemLanguage enum value to a language string.
		/// </summary>
		/// <returns>
		/// The language string representation of the specified systemLanguage.
		/// </returns>
		/// <param name='systemLanguage'>
		/// A Unity SystemLanguage enum value.
		/// </param>
		public static string GetLanguage(SystemLanguage systemLanguage) {
			switch (systemLanguage) {
			case SystemLanguage.Afrikaans: return "af";
			case SystemLanguage.Arabic: return "ar";
			case SystemLanguage.Basque: return "eu";
			case SystemLanguage.Belarusian: return "be";
			case SystemLanguage.Bulgarian: return "bg";
			case SystemLanguage.Catalan: return "ca";
			case SystemLanguage.Chinese: return "zh";
			case SystemLanguage.Czech: return "cs";
			case SystemLanguage.Danish: return "da";
			case SystemLanguage.Dutch: return "nl";
			case SystemLanguage.English: return "en";
			case SystemLanguage.Estonian: return "et";
			case SystemLanguage.Faroese: return "fo";
			case SystemLanguage.Finnish: return "fi";
			case SystemLanguage.French: return "fr";
			case SystemLanguage.German: return "de";
			case SystemLanguage.Greek: return "el";
			case SystemLanguage.Hebrew: return "he";
			case SystemLanguage.Hungarian: return "hu";
			case SystemLanguage.Icelandic: return "is";
			case SystemLanguage.Indonesian: return "id";
			case SystemLanguage.Italian: return "it";
			case SystemLanguage.Japanese: return "ja";
			case SystemLanguage.Korean: return "ko";
			case SystemLanguage.Latvian: return "lv";
			case SystemLanguage.Lithuanian: return "lt";
			case SystemLanguage.Norwegian: return "no";
			case SystemLanguage.Polish: return "pl";
			case SystemLanguage.Portuguese: return "pt";
			case SystemLanguage.Romanian: return "ro";
			case SystemLanguage.Russian: return "ru";
			case SystemLanguage.SerboCroatian: return "sr";
			case SystemLanguage.Slovak: return "sk";
			case SystemLanguage.Slovenian: return "sl";
			case SystemLanguage.Spanish: return "es";
			case SystemLanguage.Swedish: return "sv";
			case SystemLanguage.Thai: return "th";
			case SystemLanguage.Turkish: return "tr";
			case SystemLanguage.Ukrainian: return "uk";
			case SystemLanguage.Vietnamese: return "vi";
			default: return null;
			}
		}
	
	}

}
