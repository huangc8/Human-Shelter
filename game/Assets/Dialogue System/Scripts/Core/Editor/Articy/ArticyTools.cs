using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace PixelCrushers.DialogueSystem.Articy {
	
	/// <summary>
	/// This static utility class contains tools for working with Articy data.
	/// </summary>
	public static class ArticyTools {

		private static string[] htmlTags = new string[] { "<html>", "<head>", "<style>", "#s0", "{text-align:left;}", "#s1", 
			"{font-size:11pt;}", "</style>", "</head>", "<body>", "<p id=\"s0\">", "<span id=\"s1\">",
			"</span>", "</p>", "</body>", "</html>" };
		
		/// <summary>
		/// Removes HTML tags from a string.
		/// </summary>
		/// <returns>
		/// The string without HTML.
		/// </returns>
		/// <param name='s'>
		/// The HTML-filled string.
		/// </param>
		public static string RemoveHtml(string s) {
			// This is a rather inefficient first pass, but it gets the job done.
			// On the roadmap: Replace with http://www.codeproject.com/Articles/298519/Fast-Token-Replacement-in-Csharp
			if (!string.IsNullOrEmpty(s)) {
				foreach (string htmlTag in htmlTags) {
					s = s.Replace(htmlTag, string.Empty);
				}
				s = s.Replace("&#39;", "'");
				s = s.Replace("&quot;", "\"");
				s = s.Replace("&amp;", "&");
				s = s.Trim();
			}
			return s;
		}
		
		/// <summary>
		/// Checks the first few lines of an XML file for a schema identifier.
		/// </summary>
		/// <returns>
		/// <c>true</c> if it contains the schema identifier.
		/// </returns>
		/// <param name='xmlFilename'>
		/// Name of the XML file to check.
		/// </param>
		/// <param name='schemaId'>
		/// Schema identifier to check for.
		/// </param>
		public static bool ContainsSchemaId(string xmlFilename, string schemaId) {
			StreamReader xmlStream = new StreamReader(xmlFilename);
			if (xmlStream != null) {
				for (int i = 0; i < 5; i++) {
					string s = xmlStream.ReadLine();
					if (s.Contains(schemaId)) return true;
				}
				xmlStream.Close();
			}
			return false;
		}
		
	}
		 
}