using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace PixelCrushers.DialogueSystem {

	/// <summary>
	/// This class manages the Unique ID Window prefs. It allows the window to save
	/// prefs to EditorPrefs between sessions.
	/// </summary>
	[System.Serializable]
	public class UniqueIDWindowPrefs {

		private const string UniqueIDWindowPrefsKey = "PixelCrushers.DialogueSystem.UniqueIDTool";
		
		public List<DialogueDatabase> databases = new List<DialogueDatabase>();

		public UniqueIDWindowPrefs() {}
		
		/// <summary>
		/// Clears the prefs.
		/// </summary>
		public void Clear() {
			databases.Clear();
		}
		
		/// <summary>
		/// Deletes the prefs from EditorPrefs.
		/// </summary>
		public static void DeleteEditorPrefs() {
			EditorPrefs.DeleteKey(UniqueIDWindowPrefsKey);
		}
	
		/// <summary>
		/// Load the prefs from EditorPrefs.
		/// </summary>
		public static UniqueIDWindowPrefs Load() {
			return FromXml(EditorPrefs.GetString(UniqueIDWindowPrefsKey));
		}
		
		/// <summary>
		/// Save the prefs to EditorPrefs.
		/// </summary>
		public void Save() {
			EditorPrefs.SetString(UniqueIDWindowPrefsKey, ToXml());
		}
		
		/// <summary>
		/// Loads the prefs from XML.
		/// </summary>
		/// <returns>
		/// The prefs.
		/// </returns>
		/// <param name='xml'>
		/// XML.
		/// </param>
		public static UniqueIDWindowPrefs FromXml(string xml) {
			/*
			UniqueIDWindowPrefs prefs = null;
			if (!string.IsNullOrEmpty(xml)) {
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(UniqueIDWindowPrefs));
				prefs = xmlSerializer.Deserialize(new StringReader(xml)) as UniqueIDWindowPrefs;
			}
			return (prefs != null) ? prefs : new UniqueIDWindowPrefs();
			*/
			return new UniqueIDWindowPrefs();
		}
		
		/// <summary>
		/// Returns the prefs in XML format.
		/// </summary>
		/// <returns>
		/// The xml.
		/// </returns>
		public string ToXml() {
			/*
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(UniqueIDWindowPrefs));
			StringWriter writer = new StringWriter();
      		xmlSerializer.Serialize(writer, this);
			return writer.ToString();
			*/
			return string.Empty;
		}
		
	}
	
}