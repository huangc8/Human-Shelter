using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PixelCrushers.DialogueSystem;

namespace PixelCrushers.DialogueSystem {

	/// <summary>
	/// This part of the Dialogue Editor window contains the voiceover script export code.
	/// </summary>
	public static class VoiceoverScriptExporter {

		/// <summary>
		/// The main export method. Exports a voiceover script to a CSV file.
		/// </summary>
		/// <param name="database">Source database.</param>
		/// <param name="filename">Target CSV filename.</param>
		/// <param name="exportActors">If set to <c>true</c> export actors.</param>
		public static void Export(DialogueDatabase database, string filename, bool exportActors, EntrytagFormat entrytagFormat) {
			using (StreamWriter file = new StreamWriter(filename, false, Encoding.UTF8)) {
				ExportDatabaseProperties(database, file);
				if (exportActors) ExportActors(database, file);
				ExportConversations(database, entrytagFormat, file);
			}
		}

		private static void ExportDatabaseProperties(DialogueDatabase database, StreamWriter file) {
			file.WriteLine("Database," + CleanField(database.name));
			file.WriteLine("Author," + CleanField(database.author));
			file.WriteLine("Version," + CleanField(database.version));
			file.WriteLine("Description," + CleanField(database.description));
		}

		private static void ExportActors(DialogueDatabase database, StreamWriter file) {
			file.WriteLine(string.Empty);
			file.WriteLine("---Actors---");
			file.WriteLine("Name,Description");
			foreach (var actor in database.actors) {
				file.WriteLine(CleanField(actor.Name) + "," + CleanField(actor.LookupValue("Description")));
			}
		}
		
		private static void ExportConversations(DialogueDatabase database, EntrytagFormat entrytagFormat, StreamWriter file) {
			file.WriteLine(string.Empty);
			file.WriteLine("---Conversations---");

			// Find all languages: (TO BE IMPLEMENTED LATER)
			//List<string> otherDialogueText = new List<string>();
			//foreach (var conversation in database.conversations) {
			//	foreach (var entry in conversation.dialogueEntries) {
			//		foreach (var field in entry.fields) {
			//		}
			//	}
			//}

			// Cache actor names:
			Dictionary<int, string> actorNames = new Dictionary<int, string>();

			// Export all conversations:
			foreach (var conversation in database.conversations) {
				file.WriteLine(string.Empty);
				file.WriteLine(string.Format("Conversation {0},{1}", conversation.id, CleanField(conversation.Title)));
				file.WriteLine(string.Format("Description,{0}", CleanField(conversation.Description)));
				StringBuilder sb = new StringBuilder("entrytag,Actor,Dialogue Text");
				//foreach (var fieldTitle in otherDialogueText) {
				//	sb.AppendFormat(",{0}", CleanField(fieldTitle));
				//}
				file.WriteLine(sb.ToString());
				foreach (var entry in conversation.dialogueEntries) {
					if (entry.id > 0) {
						if (!actorNames.ContainsKey(entry.ActorID)) {
							Actor actor = database.GetActor(entry.ActorID);
							actorNames.Add(entry.ActorID, (actor != null) ? CleanField(actor.Name) : "ActorNotFound");
						}
						string actorName = actorNames[entry.ActorID];
						string entrytag = database.GetEntrytag(conversation, entry, entrytagFormat);
						string lineText = CleanField(entry.SubtitleText);
						file.WriteLine(string.Format("{0},{1},{2}", entrytag, actorName, lineText));
					}
				}
			}
		}
		
		private static string CleanField(string s) {
			return CSVExporter.CleanField(s);
		}

	}

}