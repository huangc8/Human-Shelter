using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace PixelCrushers.DialogueSystem.Articy {

	/// <summary>
	/// This class contains articy project conversion settings. It's used by ConverterPrefs.
	/// </summary>
	public class ConversionSettings {
		
		private Dictionary<string, ConversionSetting> dict = new Dictionary<string, ConversionSetting>();
		
		public List<ConversionSetting> list = new List<ConversionSetting>();
		
		public static ConversionSettings FromXml(string xml) {
			ConversionSettings conversionSettings = null;
			if (string.IsNullOrEmpty(xml)) {
				conversionSettings = new ConversionSettings();
			} else {
				XmlSerializer xmlSerializer = new XmlSerializer(typeof(ConversionSettings));
				conversionSettings = xmlSerializer.Deserialize(new StringReader(xml)) as ConversionSettings;
				if (conversionSettings != null) conversionSettings.AfterDeserialization();
			}
			return conversionSettings;
		}
		
		public string ToXml() {
			BeforeSerialization();
 			XmlSerializer xmlSerializer = new XmlSerializer(typeof(ConversionSettings));
			StringWriter writer = new StringWriter();
      		xmlSerializer.Serialize(writer, this);
			return writer.ToString();
		}
		
		private void BeforeSerialization() {
			list.Clear();
			foreach (var entry in dict) {
				list.Add(entry.Value);
			}
		}
		
		private void AfterDeserialization() {
			dict.Clear();
			list.ForEach(element => dict.Add(element.Id, element));
		}
		
		public void Clear() {
			dict.Clear();
			list.Clear();
		}
		
		public ConversionSetting GetConversionSetting(string Id) {
			if (string.IsNullOrEmpty(Id)) return null;
			if (!dict.ContainsKey(Id)) dict[Id] = new ConversionSetting(Id);
			return dict[Id];
		}
		
		public bool ConversionSettingExists(string Id) {
			return !string.IsNullOrEmpty(Id) && dict.ContainsKey(Id);
		}
		
	}
	
}
