﻿using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PixelCrushers.DialogueSystem;

namespace PixelCrushers.DialogueSystem {

	/// <summary>
	/// This part of the Dialogue Editor window contains the CSV export code.
	/// </summary>
	public static class CSVExporter {

		/// <summary>
		/// The main export method. Exports a database to a CSV file.
		/// </summary>
		/// <param name="database">Source database.</param>
		/// <param name="filename">Target CSV filename.</param>
		/// <param name="exportActors">If set to <c>true</c> export actors.</param>
		/// <param name="exportItems">If set to <c>true</c> export items.</param>
		/// <param name="exportLocations">If set to <c>true</c> export locations.</param>
		/// <param name="exportVariables">If set to <c>true</c> export variables.</param>
		/// <param name="exportConversations">If set to <c>true</c> export conversations.</param>
		public static void Export(DialogueDatabase database, string filename, bool exportActors, bool exportItems, 
		                          bool exportLocations, bool exportVariables, bool exportConversations, EntrytagFormat entrytagFormat) {
			using (StreamWriter file = new StreamWriter(filename, false, Encoding.UTF8)) {
				ExportDatabaseProperties(database, file);
				if (exportActors) ExportAssets<Actor>("Actors", database.actors, CustomActorHeader, CustomActorValues, file);
				if (exportItems) ExportAssets<Item>("Items", database.items, null, null, file);
				if (exportLocations) ExportAssets<Location>("Locations", database.locations, null, null, file);
				if (exportVariables) ExportAssets<Variable>("Variables", database.variables, null, null, file);
				if (exportConversations) {
					ExportAssets<Conversation>("Conversations", database.conversations, null, null, file);
					ExportDialogueEntries(database, entrytagFormat, file);
					ExportLinks(database, file);
				}
			}
		}

		private static void ExportDatabaseProperties(DialogueDatabase database, StreamWriter file) {
			file.WriteLine("Database");
			file.WriteLine("Name,Version,Author,Description,Emphasis1,Emphasis2,Emphasis3,Emphasis4");
			file.WriteLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7}", 
			                             CleanField(database.name), 
			                             CleanField(database.version), 
			                             CleanField(database.author), 
			                             CleanField(database.description),
			                             EmField(database.emphasisSettings[0]), 
			                             EmField(database.emphasisSettings[1]), 
			                             EmField(database.emphasisSettings[2]),
			                             EmField(database.emphasisSettings[3])));
			file.WriteLine("Global User Script");
			file.WriteLine(CleanField(database.globalUserScript));
		}

		private delegate void HeaderDelegate(StringBuilder titles, StringBuilder types);

		private delegate void AssetDelegate<T>(StringBuilder sb, T asset) where T : Asset;

		private static void CustomActorHeader(StringBuilder titles, StringBuilder types) {
			titles.Append(",Portrait,AltPortraits");
			types.Append(",Special,Special");
		}
		
		private static void CustomActorValues(StringBuilder sb, Actor actor) {
			sb.AppendFormat(",{0},{1}", 
			                ((actor.portrait != null) ? CleanField(actor.portrait.name) : string.Empty), 
			                AltPortraitsField(actor.alternatePortraits));
		}

		private static void ExportAssets<T>(string header, List<T> assets, HeaderDelegate headerDelegate, AssetDelegate<T> assetDelegate, StreamWriter file) where T : Asset {
			// Generate list of all fields:
			List<Field> allFields = new List<Field>();
			foreach (var asset in assets) {
				foreach (var field in asset.fields) {
					if (allFields.Find(f => string.Equals(f.title, field.title)) == null) {
						allFields.Add(field);
					}
				}
			}
			
			// Export header:
			file.WriteLine(header);
			StringBuilder titles = new StringBuilder("ID");
			StringBuilder types = new StringBuilder("Number");
			if (headerDelegate != null) headerDelegate(titles, types);
			foreach (var field in allFields) {
				titles.AppendFormat(",{0}", CleanField(field.title));
				types.AppendFormat(",{0}", field.type.ToString());
			}
			file.WriteLine(titles.ToString());
			file.WriteLine(types.ToString());
			
			// Export all assets:
			foreach (var asset in assets) {
				StringBuilder sb = new StringBuilder(asset.id.ToString());
				if (assetDelegate != null) assetDelegate(sb, asset);
				AppendFields(sb, allFields, asset.fields);
				file.WriteLine(sb.ToString());
			}
		}
		
		private static void ExportDialogueEntries(DialogueDatabase database, EntrytagFormat entrytagFormat, StreamWriter file) {
			// Generate list of all fields:
			List<Field> allFields = new List<Field>();
			foreach (var conversation in database.conversations) {
				foreach (var entry in conversation.dialogueEntries) {
					foreach (var field in entry.fields) {
						if (allFields.Find(f => string.Equals(f.title, field.title)) == null) {
							allFields.Add(field);
						}
					}
				}
			}

			List<string> orderedFields = new List<string> 
			{ "Title", "Actor", "Conversant", "Menu Text", "Dialogue Text", "Sequence" };
			
			// Export header: (note: decided to omit some unused values)
			file.WriteLine("DialogueEntries");
			StringBuilder titles = new StringBuilder("entrytag,ConvID,ID,Actor,Conversant,Title,MenuText,DialogueText,IsGroup,FalseConditionAction,ConditionPriority,Conditions,Script,Sequence");
			StringBuilder types = new StringBuilder("Text,Number,Number,Number,Number,Text,Text,Text,Boolean,Special,Special,Text,Text,Text");
			foreach (var field in allFields) {
				if (!orderedFields.Contains(field.title)) {
					titles.AppendFormat(",{0}", CleanField(field.title));
					types.AppendFormat(",{0}", field.type.ToString());
				}
			}
			file.WriteLine(titles.ToString());
			file.WriteLine(types.ToString());
			
			// Export all assets:
			foreach (var conversation in database.conversations) {
				foreach (var entry in conversation.dialogueEntries) {
					StringBuilder sb = new StringBuilder();
					sb.AppendFormat("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13}", 
					                CleanField(database.GetEntrytag(conversation, entry, entrytagFormat)),
					                entry.conversationID, entry.id, 
					                entry.ActorID, entry.ConversantID, CleanField(entry.Title),
					                CleanField(entry.DefaultMenuText), CleanField(entry.DefaultDialogueText), 
					                entry.isGroup, entry.falseConditionAction, 
					                entry.conditionPriority, CleanField(entry.conditionsString), 
					                CleanField(entry.userScript), CleanField(entry.Sequence));
					AppendFields(sb, allFields, entry.fields, orderedFields);
					file.WriteLine(sb.ToString());
				}
			}
		}

		private static void ExportLinks(DialogueDatabase database, StreamWriter file) {
			// Export header:
			file.WriteLine("OutgoingLinks");
			file.WriteLine("OriginConvID,OriginID,DestConvID,DestID,ConditionPriority");
			file.WriteLine("Number,Number,Number,Number,Special");

			// Export all links:
			foreach (var conversation in database.conversations) {
				foreach (var entry in conversation.dialogueEntries) {
					foreach (var link in entry.outgoingLinks) {
						file.WriteLine(string.Format("{0},{1},{2},{3},{4}",
						                             link.originConversationID, link.originDialogueID,
						                             link.destinationConversationID, link.destinationDialogueID,
						                             link.priority));
					}
				}
			}
		}
		
		private static void AppendFields(StringBuilder sb, List<Field> allFields, List<Field> fields, List<string> omitFields) {
			foreach (var allField in allFields) {
				if ((omitFields == null) || !omitFields.Contains(allField.title)) {
					Field field = Field.Lookup(fields, allField.title);
					if (field != null) {
						sb.AppendFormat(",{0}", CleanField(field.value));
					} else {
						sb.Append(",");
						sb.Append(OmitField);
					}
				}
			}
		}
		
		private static void AppendFields(StringBuilder sb, List<Field> allFields, List<Field> fields) {
			AppendFields(sb, allFields, fields, null);
		}
		
		private const string OmitField = "{{omit}}";

		public static string CleanField(string s) {
			if (string.IsNullOrEmpty(s)) return string.Empty;
			string s2 = s.Contains("\n") ? s.Replace("\n", "\\n") : s;
			if (s2.Contains(",")) {
				return "\"" + s2.Replace("\"", "\\\"") + "\"";
			} else {
				return s2;
			}
		}

		private static string EmField(EmphasisSetting em) {
			return string.Format("{0} {1}{2}{3}", Tools.ToWebColor(em.color),
			                     (em.bold ? 'b' : '-'),
			                     (em.italic ? 'i' : '-'),
			                     (em.underline ? 'u' : '-'));
		}

		private static string AltPortraitsField(List<Texture2D> altPortraits) {
			if (altPortraits == null || altPortraits.Count == 0) return "[]";
			StringBuilder sb = new StringBuilder("[");
			bool first = true;
			foreach (Texture2D portrait in altPortraits) {
				if (!first) sb.Append(",");
				sb.Append(portrait.name);
				first = false;
			}
			sb.Append("]");
			return sb.ToString();
		}

		/// <summary>
		/// Returns the individual comma-separated values in a line.
		/// </summary>
		/// <returns>The values.</returns>
		/// <param name="line">Line.</param>
		public static string[] GetValues(string line) {
			Regex csvSplit = new Regex("(?:^|,)(\"(?:[^\"]+|\"\")*\"|[^,]*)", RegexOptions.Compiled);
			List<string> values = new List<string>();
			foreach (Match match in csvSplit.Matches(line)) {
				values.Add(UnwrapValue(match.Value.TrimStart(',')));
			}
			return values.ToArray();
		}
		
		/// <summary>
		/// Returns a "fixed" version of a comma-separated value where escaped newlines
		/// have been converted back into real newlines, and optional surrounding quotes 
		/// have been removed.
		/// </summary>
		/// <returns>The value.</returns>
		/// <param name="value">Value.</param>
		public static string UnwrapValue(string value) {
			string s = value.Replace("\\n", "\n");
			if (s.StartsWith("\"") && s.EndsWith("\"")) s = s.Substring(1, s.Length - 2);
			return s;
		}
		


	}

}