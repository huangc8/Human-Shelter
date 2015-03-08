using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;

namespace PixelCrushers.DialogueSystem.Articy {
	
	/// <summary>
	/// This class does the actual work of converting ArticyData (version-independent 
	/// articy:draft data) into a dialogue database.
	/// </summary>
	public class ArticyConverter {
		
		/// <summary>
		/// This static utility method creates a converter and uses it run the conversion.
		/// </summary>
		/// <param name='articyData'>
		/// Articy data.
		/// </param>
		/// <param name='prefs'>
		/// Prefs.
		/// </param>
		/// <param name='database'>
		/// Dialogue database.
		/// </param>
		public static void ConvertArticyDataToDatabase(ArticyData articyData, ConverterPrefs prefs, DialogueDatabase database) {
			ArticyConverter converter = new ArticyConverter();
			converter.Convert(articyData, prefs, database);
		}
		
		private const string ArticyIdFieldTitle = "Articy Id";
		private const string ArticyTechnicalNameFieldTitle = "Technical Name";
		private const string DestinationArticyIdFieldTitle = "destinationArticyId";
		private const int StartEntryID = 0;
		
		private ArticyData articyData;
		private ConverterPrefs prefs;
		private DialogueDatabase database;
		private Template template;
		private int conversationID;
		private int actorID;
		private int itemID;
		private int locationID;
		private int entryID;
		private List<string> fullVariableNames = new List<string>();
		
		/// <summary>
		/// Convert the ArticyData, using the preferences in Prefs, into a dialogue database.
		/// </summary>
		/// <param name='articyData'>
		/// Articy data.
		/// </param>
		/// <param name='prefs'>
		/// Prefs.
		/// </param>
		/// <param name='database'>
		/// Dialogue database.
		/// </param>
		public void Convert(ArticyData articyData, ConverterPrefs prefs, DialogueDatabase database) {
			if (articyData != null) {
				Setup(articyData, prefs, database);
				ConvertProjectAttributes();
				ConvertEntities();
				ConvertLocations();
				ConvertFlowFragments();
				ConvertVariables();
				ConvertDialogues();
			}
		}
		
		/// <summary>
		/// Sets up the conversion process.
		/// </summary>
		/// <param name='articyData'>
		/// Articy data.
		/// </param>
		/// <param name='prefs'>
		/// Prefs.
		/// </param>
		/// <param name='database'>
		/// Dialogue database.
		/// </param>
		private void Setup(ArticyData articyData, ConverterPrefs prefs, DialogueDatabase database) {
			this.articyData = articyData;
			this.prefs = prefs;
			this.database = database;
			database.actors = new List<Actor>();
			database.items = new List<Item>();
			database.locations = new List<Location>();
			database.variables = new List<Variable>();
			database.conversations = new List<Conversation>();
			conversationID = 0;
			actorID = 0;
			itemID = 0;
			locationID = 0;
			fullVariableNames.Clear();
			template = Template.FromEditorPrefs();
		}
		
		private void ConvertProjectAttributes() {
			database.version = articyData.ProjectVersion;
			database.author = articyData.ProjectAuthor;
		}
		
		/// <summary>
		/// Converts articy entities into Dialogue System actors and items.
		/// </summary>
		private void ConvertEntities() {
			foreach (ArticyData.Entity articyEntity in articyData.entities.Values) {
				ConversionSetting conversionSetting = prefs.ConversionSettings.GetConversionSetting(articyEntity.id);
				if (conversionSetting.Include) {
					switch (conversionSetting.Category) {
					case EntityCategory.NPC:
					case EntityCategory.Player:
						actorID++;
						bool isPlayer = (conversionSetting.Category == EntityCategory.Player);
						Actor actor = template.CreateActor(actorID, articyEntity.displayName.DefaultText, isPlayer);
						Field.SetValue(actor.fields, ArticyIdFieldTitle, articyEntity.id, FieldType.Text);
						Field.SetValue(actor.fields, ArticyTechnicalNameFieldTitle, articyEntity.technicalName, FieldType.Text);
						Field.SetValue(actor.fields, "Description", articyEntity.text.DefaultText, FieldType.Text);
						if (!string.IsNullOrEmpty(articyEntity.previewImage)) Field.SetValue(actor.fields, "Pictures", string.Format("[{0}]", articyEntity.previewImage), FieldType.Text);
						SetFeatureFields(actor.fields, articyEntity.features);
						database.actors.Add(actor);
						break;
					case EntityCategory.Item:
						itemID++;
						Item item = template.CreateItem(itemID, articyEntity.displayName.DefaultText);
						Field.SetValue(item.fields, ArticyIdFieldTitle, articyEntity.id, FieldType.Text);
						Field.SetValue(item.fields, ArticyTechnicalNameFieldTitle, articyEntity.technicalName, FieldType.Text);
						Field.SetValue(item.fields, "Description", articyEntity.text.DefaultText, FieldType.Text);
						Field.SetValue(item.fields, "Is Item", "True", FieldType.Boolean);
						SetFeatureFields(item.fields, articyEntity.features);
						database.items.Add(item);
						break;
					default:
						Debug.LogError(string.Format("{0}: Internal error converting entity type '{1}'", DialogueDebug.Prefix, conversionSetting.Category));
						break;
					}
				}
			}
			if (!string.IsNullOrEmpty(prefs.PortraitFolder)) database.actors.ForEach(a => FindPortraitTexture(a));
		}
		
		/// <summary>
		/// Converts locations.
		/// </summary>
		private void ConvertLocations() {
			foreach (ArticyData.Location articyLocation in articyData.locations.Values) {
				if (prefs.ConversionSettings.GetConversionSetting(articyLocation.id).Include) {
					locationID++;
					Location location = template.CreateLocation(locationID, articyLocation.displayName.DefaultText);
					Field.SetValue(location.fields, ArticyIdFieldTitle, articyLocation.id, FieldType.Text);
					Field.SetValue(location.fields, ArticyTechnicalNameFieldTitle, articyLocation.technicalName, FieldType.Text);
					Field.SetValue(location.fields, "Description", articyLocation.text.DefaultText, FieldType.Text);						
					SetFeatureFields(location.fields, articyLocation.features);
					database.locations.Add(location);
				}
			}
		}
		
		/// <summary>
		/// Converts flow fragments into items. (The quest system uses the Item[] table.)
		/// </summary>
		private void ConvertFlowFragments() {
			foreach (ArticyData.FlowFragment articyFlowFragment in articyData.flowFragments.Values) {
				if (prefs.ConversionSettings.GetConversionSetting(articyFlowFragment.id).Include) {
					itemID++;
					Item item = template.CreateItem(itemID, articyFlowFragment.displayName.DefaultText);
					Field.SetValue(item.fields, ArticyIdFieldTitle, articyFlowFragment.id, FieldType.Text);
					Field.SetValue(item.fields, ArticyTechnicalNameFieldTitle, articyFlowFragment.technicalName, FieldType.Text);
					Field.SetValue(item.fields, "Description", articyFlowFragment.text.DefaultText, FieldType.Text);
					Field.SetValue(item.fields, "Success Description", string.Empty, FieldType.Text);
					Field.SetValue(item.fields, "Failure Description", string.Empty, FieldType.Text);
					Field.SetValue(item.fields, "Is Item", "False", FieldType.Boolean);
					SetFeatureFields(item.fields, articyFlowFragment.features);
					database.items.Add(item);
				}
			}
		}
		
		private void SetFeatureFields(List<Field> fields, ArticyData.Features features) {
			foreach (ArticyData.Feature feature in features.features) {
				foreach (ArticyData.Property property in feature.properties) {
					foreach (Field field in property.fields) {
						if (!string.IsNullOrEmpty(field.title)) {
							fields.Add(new Field(field.title, field.value, field.type));
						}
					}
				}
			}
			//foreach (ArticyData.Property property in features.properties) {
			//	if (!string.IsNullOrEmpty(property.field.title)) {
			//		fields.Add(new Field(property.field.title, property.field.value, property.field.type));
			//	}
			//}
		}
		
		/// <summary>
		/// Converts articy variable sets and variables into Dialogue System variables.
		/// </summary>
		private void ConvertVariables() {
			int variableID = 0;
			foreach (ArticyData.VariableSet articyVariableSet in articyData.variableSets.Values) {
				foreach (ArticyData.Variable articyVariable in articyVariableSet.variables) {
					string fullName = ArticyData.FullVariableName(articyVariableSet, articyVariable);
					fullVariableNames.Add(fullName);
					if (prefs.ConversionSettings.GetConversionSetting(fullName).Include) {
						variableID++;
						Variable variable = template.CreateVariable(variableID, fullName, articyVariable.defaultValue);
						variable.Type = (articyVariable.dataType == ArticyData.VariableDataType.Boolean)
							? FieldType.Boolean
								: (articyVariable.dataType == ArticyData.VariableDataType.Integer)
								? FieldType.Number
								: FieldType.Text;
						database.variables.Add(variable);
					}
				}
			}
		}
		
		/// <summary>
		/// Converts dialogues using the articy project's hierarchy.
		/// </summary>
		private void ConvertDialogues() {
			ProcessNode(articyData.hierarchy.node);
		}
		
		/// <summary>
		/// Processes a node in the hierarchy. If it's a dialogue, convert it (and its child nodes)
		/// into a Dialogue System conversation. Otherwise keep processing the children.
		/// </summary>
		/// <param name='node'>
		/// Node to process.
		/// </param>
		private void ProcessNode(ArticyData.Node node) {
			foreach (ArticyData.Node childNode in node.nodes) {
				if (childNode.type == ArticyData.NodeType.Dialogue) {
					ConvertDialogue(childNode);
				} else {
					ProcessNode(childNode);
				}
			}
		}
		
		/// <summary>
		/// Converts a dialogue hierarchy node into a conversation.
		/// </summary>
		/// <param name='dialogueNode'>
		/// Dialogue node.
		/// </param>
		private void ConvertDialogue(ArticyData.Node dialogueNode) {
			if (prefs.ConversionSettings.GetConversionSetting(dialogueNode.id).Include) {
				if (articyData.dialogues.ContainsKey(dialogueNode.id)) {
					Conversation conversation = CreateNewConversation(articyData.dialogues[dialogueNode.id]);
					if (conversation != null) ConvertDialogueNodes(conversation, dialogueNode);
				} else {
					Debug.LogWarning(string.Format("{0}: Can't find dialogue ID {1}", DialogueDebug.Prefix, dialogueNode.id));
				}
			}
		}
		
		/// <summary>
		/// Creates a new Dialogue System conversation from an articy dialogue. This also adds the
		/// conversation's mandatory first dialogue entry, "START".
		/// </summary>
		/// <returns>
		/// The new conversation.
		/// </returns>
		/// <param name='articyDialogue'>
		/// Articy dialogue.
		/// </param>
		private Conversation CreateNewConversation(ArticyData.Dialogue articyDialogue) {
			if (articyDialogue != null) {
				conversationID++;
				Conversation conversation = template.CreateConversation(conversationID, articyDialogue.displayName.DefaultText);
				Field.SetValue(conversation.fields, ArticyIdFieldTitle, articyDialogue.id, FieldType.Text);
				Field.SetValue(conversation.fields, "Description", articyDialogue.text.DefaultText, FieldType.Text);
				conversation.ActorID = FindActorIdFromArticyDialogue(articyDialogue, 0);
				conversation.ConversantID = FindActorIdFromArticyDialogue(articyDialogue, 1);
				DialogueEntry startEntry = template.CreateDialogueEntry(StartEntryID, conversationID, "START");
				Field.SetValue(startEntry.fields, ArticyIdFieldTitle, articyDialogue.id, FieldType.Text);
				ConvertPins(startEntry, articyDialogue.pins);
				startEntry.outgoingLinks = new List<Link>();
				Field.SetValue(startEntry.fields, "Sequence", "None()", FieldType.Text);
				conversation.dialogueEntries.Add(startEntry);
				database.conversations.Add(conversation);
				return conversation;
			} else {
				return null;
			}
		}
		
		/// <summary>
		/// Converts dialogue fragment and connection (hub, jump) nodes, and adds them to a 
		/// Dialogue System conversation. First converts all dialogue fragments so the conversation
		/// has a master list of dialogue entries. Then converts all connections to hook up those
		/// entries.
		/// </summary>
		/// <param name='conversation'>
		/// Conversation.
		/// </param>
		/// <param name='dialogueNode'>
		/// Dialogue node.
		/// </param>
		private void ConvertDialogueNodes(Conversation conversation, ArticyData.Node dialogueNode) {
			ConvertDialogueFragments(conversation, dialogueNode);
			ConvertConnections(conversation, dialogueNode);
		}
		
		/// <summary>
		/// Converts dialogue fragments, adding them a conversation.
		/// </summary>
		/// <param name='conversation'>
		/// Conversation.
		/// </param>
		/// <param name='dialogueNode'>
		/// Dialogue node.
		/// </param>
		private void ConvertDialogueFragments(Conversation conversation, ArticyData.Node dialogueNode) {
			entryID = 0;
			foreach (ArticyData.Node childNode in dialogueNode.nodes) {
				if (childNode.type == ArticyData.NodeType.DialogueFragment) {
					ConvertDialogueFragment(conversation, LookupDialogueFragment(childNode.id));
				} else if (childNode.type == ArticyData.NodeType.Hub) {
					ConvertHub(conversation, LookupHub(childNode.id));
				} else if (childNode.type == ArticyData.NodeType.Jump) {
					ConvertJump(conversation, LookupJump(childNode.id));
				} else if (childNode.type == ArticyData.NodeType.Condition) {
					ConvertCondition(conversation, LookupCondition(childNode.id));
				} else if (childNode.type == ArticyData.NodeType.Instruction) {
					ConvertInstruction(conversation, LookupInstruction(childNode.id));
				}
			}
		}
		
		private ArticyData.DialogueFragment LookupDialogueFragment(string id) {
			return articyData.dialogueFragments.ContainsKey(id) ? articyData.dialogueFragments[id] : null;
		}
		
		private ArticyData.Hub LookupHub(string id) {
			return articyData.hubs.ContainsKey(id) ? articyData.hubs[id] : null;
		}
		
		private ArticyData.Jump LookupJump(string id) {
			return articyData.jumps.ContainsKey(id) ? articyData.jumps[id] : null;
		}
		
		private ArticyData.Condition LookupCondition(string id) {
			return articyData.conditions.ContainsKey(id) ? articyData.conditions[id] : null;
		}
		
		private ArticyData.Instruction LookupInstruction(string id) {
			return articyData.instructions.ContainsKey(id) ? articyData.instructions[id] : null;
		}
		
		private ArticyData.Connection LookupConnection(string id) {
			return articyData.connections.ContainsKey(id) ? articyData.connections[id] : null;
		}
		
		private ArticyData.Connection LookupConnectionBySourcePinId(string pinID) {
			foreach (var connection in articyData.connections.Values) {
				if (string.Equals(connection.source.pinRef, pinID)) {
					return connection;
				}
			}
			return null;
		}
		
		/// <summary>
		/// Converts a dialogue fragment, including fields such as text, sequence, and pins, but doesn't
		/// connect it yet.
		/// </summary>
		/// <param name='conversation'>
		/// Conversation.
		/// </param>
		/// <param name='fragment'>
		/// Fragment.
		/// </param>
		private void ConvertDialogueFragment(Conversation conversation, ArticyData.DialogueFragment fragment) {
			if (fragment != null) {
				DialogueEntry entry = CreateNewDialogueEntry(conversation, fragment.displayName.DefaultText, fragment.id);
				ConvertLocalizableText(entry, "Dialogue Text", fragment.text);
				ConvertLocalizableText(entry, "Menu Text", fragment.menuText);
				if (prefs.StageDirectionsAreSequences) {
					ConvertLocalizableText(entry, "Sequence", fragment.stageDirections);
				}
				SetFeatureFields(entry.fields, fragment.features);
				Actor actor = FindActorByArticyId(fragment.speakerIdRef);
				entry.ActorID = (actor != null) ? actor.id : 0;
				entry.ConversantID = (entry.ActorID == conversation.ActorID) ? conversation.ConversantID : conversation.ActorID;
				ConvertPins(entry, fragment.pins);
			}
		}
		
		/// <summary>
		/// Converts a hub into a group dialogue entry in a conversation.
		/// </summary>
		/// <param name='conversation'>
		/// Conversation.
		/// </param>
		/// <param name='hub'>
		/// Hub.
		/// </param>
		private void ConvertHub(Conversation conversation, ArticyData.Hub hub) {
			if (hub != null) {
				DialogueEntry groupEntry = CreateNewDialogueEntry(conversation, hub.displayName.DefaultText, hub.id);
				SetFeatureFields(groupEntry.fields, hub.features);
				groupEntry.isGroup = true;
				ConvertPins(groupEntry, hub.pins);
			}
		}
		
		/// <summary>
		/// Converts a jump into a group dialogue entry in a conversation.
		/// </summary>
		/// <param name='conversation'>
		/// Conversation.
		/// </param>
		/// <param name='jump'>
		/// Jump.
		/// </param>
		private void ConvertJump(Conversation conversation, ArticyData.Jump jump) {
			if (jump != null) {
				DialogueEntry jumpEntry = CreateNewDialogueEntry(conversation, jump.displayName.DefaultText, jump.id);
				SetFeatureFields(jumpEntry.fields, jump.features);
				jumpEntry.isGroup = true;
				ConvertPins(jumpEntry, jump.pins);
				DialogueEntry destination = FindDialogueEntryByArticyId(conversation, jump.target.idRef);
				if (destination != null) {
					Link link = new Link();
					link.originConversationID = conversation.id;
					link.originDialogueID = jumpEntry.id;
					link.destinationConversationID = conversation.id;
					link.destinationDialogueID = destination.id;
					link.isConnector = false;
					link.priority = ConditionPriority.Normal;
					jumpEntry.outgoingLinks.Add(link);
				}
			}
		}
		
		/// <summary>
		/// Converts a condition node into two dialogue entries (one for true, one for false).
		/// </summary>
		/// <param name='conversation'>
		/// Conversation.
		/// </param>
		/// <param name='condition'>
		/// Condition.
		/// </param>
		private void ConvertCondition(Conversation conversation, ArticyData.Condition condition) {
			if (condition != null) {
				string trueLuaConditions = ConvertExpression(condition.expression);
				string falseLuaConditions = string.IsNullOrEmpty(trueLuaConditions)
					? "false" : string.Format("({0}) == false", trueLuaConditions);
				foreach (var pin in condition.pins) {
					if (pin.semantic == ArticyData.SemanticType.Output) {
						bool isTruePath = (pin.index == 0);
						var connection = LookupConnectionBySourcePinId(pin.id);
						if (connection != null) {
							string title = isTruePath ? condition.expression : string.Format("!({0})", condition.expression);
							DialogueEntry entry = CreateNewDialogueEntry(conversation, title, condition.id);
							entry.ActorID = conversation.ConversantID;
							entry.ConversantID = conversation.ActorID;
							entry.DialogueText = string.Empty;
							entry.MenuText = string.Empty;
							entry.Sequence = "None()";
							entry.isGroup = true;
							Field.SetValue(entry.fields, DestinationArticyIdFieldTitle, connection.target.idRef);
							string luaConditions = isTruePath ? trueLuaConditions : falseLuaConditions;
							entry.conditionsString = AddToConditions(entry.conditionsString, luaConditions);
							entry.userScript = AddToUserScript(entry.userScript, ConvertExpression(pin.expression));
						}
					}
				}
			}
		}

		private void ConvertInstruction(Conversation conversation, ArticyData.Instruction instruction) {
			if (instruction != null) {
				DialogueEntry entry = CreateNewDialogueEntry(conversation, instruction.expression, instruction.id);
				entry.ActorID = conversation.ConversantID;
				entry.ConversantID = conversation.ActorID;
				entry.DialogueText = string.Empty;
				entry.MenuText = string.Empty;
				entry.Sequence = "None()";
				entry.isGroup = true;
				entry.conditionsString = string.Empty;
				entry.userScript = AddToUserScript(entry.userScript, ConvertExpression(instruction.expression));
			}
		}
		
		private string AddToConditions(string conditions, string moreConditions) {
			return string.IsNullOrEmpty(conditions) ? moreConditions : string.Format("({0}) and ({1})", conditions, moreConditions);
		}

		private string AddToUserScript(string script, string moreScript) {
			return string.IsNullOrEmpty(script) ? moreScript : string.Format("{0}; {1}", script, moreScript);
		}
		
		/// <summary>
		/// Creates a new dialogue entry and adds it to a conversation.
		/// </summary>
		/// <returns>
		/// The new dialogue entry.
		/// </returns>
		/// <param name='conversation'>
		/// Conversation.
		/// </param>
		/// <param name='title'>
		/// Title.
		/// </param>
		/// <param name='articyId'>
		/// Articy identifier.
		/// </param>
		private DialogueEntry CreateNewDialogueEntry(Conversation conversation, string title, string articyId) {
			entryID++;
			DialogueEntry entry = template.CreateDialogueEntry(entryID, conversation.id, title);
			Field.SetValue(entry.fields, ArticyIdFieldTitle, articyId, FieldType.Text);
			conversation.dialogueEntries.Add(entry);
			return entry;
		}
		
		/// <summary>
		/// Converts connections into a conversation's link tree.
		/// </summary>
		/// <param name='conversation'>
		/// Conversation.
		/// </param>
		/// <param name='dialogueNode'>
		/// Dialogue node.
		/// </param>
		private void ConvertConnections(Conversation conversation, ArticyData.Node dialogueNode) {
			foreach (ArticyData.Node childNode in dialogueNode.nodes) {
				if (childNode.type == ArticyData.NodeType.Connection) {
					ConvertConnection(conversation, LookupConnection(childNode.id));
				}
			}
		}
		
		/// <summary>
		/// Converts a connection into an outgoing link in a conversation's dialogue entry.
		/// Condition nodes create two dialogue entries with the same articy ID (one for true,
		/// one for false). In this case, the correct one is connected to the destination
		/// based on the the "destination articy id" field recorded in the origin entry.
		/// </summary>
		/// <param name='conversation'>
		/// Conversation.
		/// </param>
		/// <param name='connection'>
		/// Connection.
		/// </param>
		private void ConvertConnection(Conversation conversation, ArticyData.Connection connection) {
			if (connection != null) {
				foreach (DialogueEntry origin in conversation.dialogueEntries) {
					if (string.Equals(Field.LookupValue(origin.fields, ArticyIdFieldTitle), connection.source.idRef)) {
						foreach (DialogueEntry destination in conversation.dialogueEntries) {
							if (string.Equals(Field.LookupValue(destination.fields, ArticyIdFieldTitle), connection.target.idRef)) {
								bool isValidToLink = true;
								if (Field.FieldExists(origin.fields, DestinationArticyIdFieldTitle)) {
									string requiredDestinationArticyId = Field.LookupValue(origin.fields, DestinationArticyIdFieldTitle);
									string destinationArticyId = Field.LookupValue(destination.fields, ArticyIdFieldTitle);
									isValidToLink = string.Equals(requiredDestinationArticyId, destinationArticyId);
								}
								if (isValidToLink) {
									Link link = new Link();
									link.originConversationID = conversation.id;
									link.originDialogueID = origin.id;
									link.destinationConversationID = conversation.id;
									link.destinationDialogueID = destination.id;
									link.isConnector = false;
									link.priority = ConditionPriority.Normal;
									origin.outgoingLinks.Add(link);
								}
							}
						}
					}
				}
			}
		}
		
		/// <summary>
		/// Converts input pins as a dialogue entry's Conditions, and output pins as User Script.
		/// </summary>
		/// <param name='entry'>
		/// Entry.
		/// </param>
		/// <param name='pins'>
		/// Pins.
		/// </param>
		private void ConvertPins(DialogueEntry entry, List<ArticyData.Pin> pins) {
			foreach (ArticyData.Pin pin in pins) {
				switch (pin.semantic) {
				case ArticyData.SemanticType.Input:
					entry.conditionsString = AddToConditions(entry.conditionsString, ConvertExpression(pin.expression));
					break;
				case ArticyData.SemanticType.Output:
					entry.userScript = AddToUserScript(entry.userScript, ConvertExpression(pin.expression));
					break;
				default:
					Debug.LogWarning(string.Format("{0}: Unexpected semantic type {1} for pin {2}.", DialogueDebug.Prefix, pin.semantic, pin.id));
					break;
				}
			}
		}
		
		/// <summary>
		/// Converts an articyScript expression into Lua.
		/// </summary>
		/// <returns>
		/// A Lua version of the expression.
		/// </returns>
		/// <param name='expression'>
		/// articyScript expression.
		/// </param>
		private string ConvertExpression(string expression) {
			if (string.IsNullOrEmpty(expression)) return expression;
			
			// If already Lua, return it:
			if (expression.Contains("Variable[")) return expression;
			
			// Convert comments:
			string s = expression.Trim().Replace("//", "--");
			
			// Convert Boolean conditionals:
			s = s.Replace("&&", " and ");
			s = s.Replace("||", " or ");
			
			// Convert variable names:
			foreach (string fullVariableName in fullVariableNames) {
				string luaVariableReference = string.Format("Variable[\"{0}\"]", fullVariableName);
				if (s.Contains(fullVariableName)) s = s.Replace(fullVariableName, luaVariableReference);
			}
			
			// Convert negation (!) to "==false":
			s = s.Replace("!Variable", "false == Variable");
			
			// Convert arithmetic assignment operators (e.g., +=):
			if (ContainsArithmeticAssignment(s)) {
				string[] tokens = s.Split(null);
				for (int i = 1; i < tokens.Length; i++) {
					string token = tokens[i];
					if (ContainsArithmeticAssignment(token)) {
						char operation = token[0];
						tokens[i] = string.Format("= {0} {1}", tokens[i-1], operation);
					}
				}
				s = string.Join(" ", tokens);
			}
			
			return s;
		}
		
		private bool ContainsArithmeticAssignment(string s) {
			return (s != null) && (s.Contains("+=") || s.Contains("-="));
		}
		
		private void ConvertLocalizableText(DialogueEntry entry, string baseFieldTitle, ArticyData.LocalizableText localizableText) {
			foreach (KeyValuePair<string, string> kvp in localizableText.localizedString) {
				if (string.IsNullOrEmpty(kvp.Key)) {
					Field.SetValue(entry.fields, baseFieldTitle, RemoveFormattingTags(kvp.Value), FieldType.Text);
				} else {
					string localizedTitle = string.Format("{0} {1}", baseFieldTitle, kvp.Key);
					Field.SetValue(entry.fields, localizedTitle, RemoveFormattingTags(kvp.Value), FieldType.Localization);
				}
			}
		}

		private string RemoveFormattingTags(string s) {
			if (!string.IsNullOrEmpty(s) && s.Contains("font-size")) {
				Regex regex = new Regex("{font-size:[0-9]+pt;}");
				return regex.Replace(s, string.Empty);
			} else {
				return s;
			}
		}
		
		/// <summary>
		/// Sets a conversation's start cutscene to None() if it's otherwise not set.
		/// </summary>
		/// <param name='conversation'>
		/// Conversation.
		/// </param>
		private static void SetConversationStartCutsceneToNone(Conversation conversation) {
			DialogueEntry entry = conversation.GetFirstDialogueEntry();
			if (entry == null) {
				Debug.LogWarning(string.Format("{0}: Conversation '{1}' doesn't have a START dialogue entry.", DialogueDebug.Prefix, conversation.Title));
			} else {
				if (string.IsNullOrEmpty(entry.Sequence)) entry.Sequence = "None()";
			}
		}
		
		private DialogueEntry FindDialogueEntryByArticyId(Conversation conversation, string articyId) {
			foreach (DialogueEntry entry in conversation.dialogueEntries) {
				if (string.Equals(Field.LookupValue(entry.fields, ArticyIdFieldTitle), articyId)) return entry;
			}
			return null;
		}
		
		private Actor FindActorByArticyId(string articyId) {
			foreach (Actor actor in database.actors) {
				if (string.Equals(actor.LookupValue(ArticyIdFieldTitle), articyId)) return actor;
			}
			return null;
		}
		
		private int FindActorIdFromArticyDialogue(ArticyData.Dialogue articyDialogue, int index) {
			Actor actor = null;
			if (index < articyDialogue.references.Count) {
				actor = FindActorByArticyId(articyDialogue.references[index]);
			}
			return (actor != null) ? actor.id : 0;
		}
		
		private void FindPortraitTexture(Actor actor) {
			if (actor == null) return;
			string textureName = actor.TextureName;
			if (!string.IsNullOrEmpty(textureName)) {
				string filename = Path.GetFileName(textureName).Replace('\\', '/');
				string assetPath = string.Format("{0}/{1}", prefs.PortraitFolder, filename);
				Texture2D texture = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D)) as Texture2D;
				if (texture == null) {
					Debug.LogWarning(string.Format("{0}: Can't find portrait texture {1} for {2}.", DialogueDebug.Prefix, assetPath, actor.Name));
				}
				actor.portrait = texture;
			}
		}
		
	}
	
}