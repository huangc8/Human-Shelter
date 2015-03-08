using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace PixelCrushers.DialogueSystem {

	/// <summary>
	/// This class defines the template that the Dialogue Database Editor will use when creating
	/// new dialogue database assets such as actors and conversations. The Dialogue Database Editor
	/// stores a copy of the template in EditorPrefs. The equivalent in Chat Mapper is Project
	/// Preferences.
	/// </summary>
	[System.Serializable]
	public class Template {
		
		private const string DialogueDatabaseTemplateKey = "PixelCrushers.DialogueSystem.DatabaseTemplate";
		
		public bool treatItemsAsQuests = true;
		
		public List<Field> actorFields = new List<Field>();
		public List<Field> itemFields = new List<Field>();
		public List<Field> questFields = new List<Field>();
		public List<Field> locationFields = new List<Field>();
		public List<Field> variableFields = new List<Field>();
		public List<Field> conversationFields = new List<Field>();
		public List<Field> dialogueEntryFields = new List<Field>();
		
		public Color npcLineColor = Color.red;
		public Color pcLineColor = Color.blue;
		public Color repeatLineColor = Color.gray;
		
		public static Template FromDefault() {
			Template template = new Template();
			template.actorFields.Clear();
			template.actorFields.Add(new Field("Name", string.Empty, FieldType.Text));
			template.actorFields.Add(new Field("Pictures", "[]", FieldType.Files));
			template.actorFields.Add(new Field("Description", string.Empty, FieldType.Text));
			template.actorFields.Add(new Field("IsPlayer", "False", FieldType.Boolean));
		
			template.itemFields.Clear();
			template.itemFields.Add(new Field("Name", string.Empty, FieldType.Text));
			template.itemFields.Add(new Field("Pictures", "[]", FieldType.Files));
			template.itemFields.Add(new Field("Description", string.Empty, FieldType.Text));
			template.itemFields.Add(new Field("Is Item", "True", FieldType.Boolean));
			
			template.questFields.Clear();
			template.questFields.Add(new Field("Name", string.Empty, FieldType.Text));
			template.questFields.Add(new Field("Pictures", "[]", FieldType.Files));
			template.questFields.Add(new Field("Description", string.Empty, FieldType.Text));
			template.questFields.Add(new Field("Success Description", string.Empty, FieldType.Text));
			template.questFields.Add(new Field("Failure Description", string.Empty, FieldType.Text));
			template.questFields.Add(new Field("State", "unassigned", FieldType.Text));
			template.questFields.Add(new Field("Is Item", "False", FieldType.Boolean));

			template.locationFields.Clear();
			template.locationFields.Add(new Field("Name", string.Empty, FieldType.Text));
			template.locationFields.Add(new Field("Pictures", "[]", FieldType.Files));
			template.locationFields.Add(new Field("Description", string.Empty, FieldType.Text));

			template.variableFields.Add(new Field("Name", string.Empty, FieldType.Text));
			template.variableFields.Add(new Field("Initial Value", string.Empty, FieldType.Text));
			template.variableFields.Add(new Field("Description", string.Empty, FieldType.Text));

			template.conversationFields.Add(new Field("Title", string.Empty, FieldType.Text));
			template.conversationFields.Add(new Field("Pictures", "[]", FieldType.Files));
			template.conversationFields.Add(new Field("Description", string.Empty, FieldType.Text));
			template.conversationFields.Add(new Field("Actor", "0", FieldType.Actor));
			template.conversationFields.Add(new Field("Conversant", "0", FieldType.Actor));

			template.dialogueEntryFields.Add(new Field("Title", string.Empty, FieldType.Text));
			template.dialogueEntryFields.Add(new Field("Pictures", "[]", FieldType.Files));
			template.dialogueEntryFields.Add(new Field("Description", string.Empty, FieldType.Text));
			template.dialogueEntryFields.Add(new Field("Actor", string.Empty, FieldType.Actor));
			template.dialogueEntryFields.Add(new Field("Conversant", string.Empty, FieldType.Actor));
			template.dialogueEntryFields.Add(new Field("Menu Text", string.Empty, FieldType.Text));
			template.dialogueEntryFields.Add(new Field("Dialogue Text", string.Empty, FieldType.Text));
			template.dialogueEntryFields.Add(new Field("Parenthetical", string.Empty, FieldType.Text));
			template.dialogueEntryFields.Add(new Field("Audio Files", "[]", FieldType.Files));
			template.dialogueEntryFields.Add(new Field("Video File", string.Empty, FieldType.Text));
			template.dialogueEntryFields.Add(new Field("Sequence", string.Empty, FieldType.Text));
			
			if (EditorGUIUtility.isProSkin) {
				template.npcLineColor = new Color(0.75f, 0.25f, 0.25f);
				template.pcLineColor = new Color(0.25f, 0.5f, 1f);
			}

			return template;
		}
		
		public static Template FromXml(string xml) {
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(Template));
			return xmlSerializer.Deserialize(new StringReader(xml)) as Template;
		}
		
		public string ToXml() {
 			XmlSerializer xmlSerializer = new XmlSerializer(typeof(Template));
			StringWriter writer = new StringWriter();
      		xmlSerializer.Serialize(writer, this);
			return writer.ToString();
		}
		
		public static Template FromEditorPrefs() {
			Template template = null;
			if (EditorPrefs.HasKey(DialogueDatabaseTemplateKey)) template = Template.FromXml(EditorPrefs.GetString(DialogueDatabaseTemplateKey));
			return template ?? Template.FromDefault();
		}
		
		public void SaveToEditorPrefs() {
			EditorPrefs.SetString(DialogueDatabaseTemplateKey, ToXml());
		}
		
		public Actor CreateActor(int id, string name, bool isPlayer) {
			Actor actor = new Actor();
			actor.fields = CreateFields(actorFields);
			actor.id = id;
			actor.Name = name;
			actor.IsPlayer = isPlayer;
			return actor;
		}
		
		public Item CreateItem(int id, string name) {
			Item item = new Item();
			item.id = id;
			item.fields = CreateFields(itemFields);
			item.Name = name;
			return item;
		}
		
		public Location CreateLocation(int id, string name) {
			Location location = new Location();
			location.id = id;
			location.fields = CreateFields(locationFields);
			location.Name = name;
			return location;
		}
		
		public Variable CreateVariable(int id, string name, string value) {
			Variable variable = new Variable();
			variable.fields = CreateFields(variableFields);
			variable.id = id;
			variable.Name = name;
			variable.InitialValue = value;
			return variable;
		}
		
		public Conversation CreateConversation(int id, string title) {
			Conversation conversation = new Conversation();
			conversation.id = id;
			conversation.fields = CreateFields(conversationFields);
			conversation.Title = title;
			return conversation;
		}
		
		public DialogueEntry CreateDialogueEntry(int id, int conversationID, string title) {
			DialogueEntry entry = new DialogueEntry();
			entry.fields = CreateFields(dialogueEntryFields);
			entry.id = id;
			entry.conversationID = conversationID;
			entry.Title = title;
			return entry;
		}
		
		public List<Field> CreateFields(List<Field> templateFields) {
			List<Field> fields = new List<Field>();
			templateFields.ForEach(templateField => fields.Add(new Field(templateField.title, templateField.value, templateField.type)));
			return fields;
		}
		
	}

}
