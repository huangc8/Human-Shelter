using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace PixelCrushers.DialogueSystem.Articy {
	
	/// <summary>
	/// This static utility class loads an arbitrary articy XML as a schema-independent
	/// ArticyData object, regardless of what version of articy generated the XML file.
	/// </summary>
	public static class ArticySchemaTools {

		public static ArticyData LoadArticyDataFromXmlFile(string xmlFilename, Encoding encoding) {
			if (Articy_2_2.Articy_2_2_Tools.IsSchema(xmlFilename)) {
				return Articy_2_2.Articy_2_2_Tools.LoadArticyData(xmlFilename, encoding);
			} else if (Articy_1_4.Articy_1_4_Tools.IsSchema(xmlFilename)) {
				return Articy_1_4.Articy_1_4_Tools.LoadArticyData(xmlFilename, encoding);
			} else {
				return null;
			}
		}
		
	}
		 
}