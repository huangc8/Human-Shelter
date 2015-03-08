using UnityEngine;
using System;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem {

	/// <summary>
	/// An asset containing a table of localized text. You can create a localized
	/// text table asset using the Assets > Create > Dialogue System menu or by
	/// right-clicking in the Project view.
	/// </summary>
	public class LocalizedTextTable : ScriptableObject {

		[Serializable]
		public class LocalizedTextField {
			public string name = string.Empty;
			public List<string> values = new List<string>();
		}

		/// <summary>
		/// The languages in the table.
		/// </summary>
		public List<string> languages = new List<string>();

		/// <summary>
		/// The fields that have localized text.
		/// </summary>
		public List<LocalizedTextField> fields = new List<LocalizedTextField>();

		private const int LanguageNotFound = -1;

		/// <summary>
		/// Gets the localized text for a field using the current language.
		/// </summary>
		/// <param name="fieldName">Field name.</param>
		public string this[string fieldName] {
			get { return GetText(fieldName); }
		}

		/// <summary>
		/// Determines whether the table contains a field.
		/// </summary>
		/// <returns><c>true</c>, if the field is in the table, <c>false</c> otherwise.</returns>
		/// <param name="fieldName">Field name.</param>
		public bool ContainsField(string fieldName) {
			return (fields.Find(f => string.Equals(f.name, fieldName)) != null);
		}

		private string GetText(string fieldName) {
			int languageIndex = GetLanguageIndex();
			if (languageIndex == LanguageNotFound) return string.Empty;
			foreach (var field in fields) {
				if (string.Equals(field.name, fieldName)) {
					if (languageIndex < field.values.Count) {
						return field.values[languageIndex];
					} else {
						return string.Empty;
					}
				}
			}
			return string.Empty;
		}

		private int GetLanguageIndex() {
			if (Localization.IsDefaultLanguage) return 0;
			for (int i = 0; i < languages.Count; i++) {
				if (string.Equals(languages[i], Localization.Language)) {
					return i;
				}
			}
			return LanguageNotFound;
		}
		
	}

}
