using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;

namespace PixelCrushers.DialogueSystem.Articy {

	/// <summary>
	/// articy:draft converter window.
	/// </summary>
	public class ArticyConverterWindow : EditorWindow {
		
		[MenuItem("Window/Dialogue System/Converters/articy:draft Converter", false, 1)]
		public static void Init() {
			EditorWindow.GetWindow(typeof(ArticyConverterWindow), false, "articy Convert");
		}
		
		public static ArticyConverterWindow Instance { get { return instance; } }
		
		private static ArticyConverterWindow instance = null;
		
		// Private fields for the window:
		
		private ConverterPrefs prefs = new ConverterPrefs();
		private ArticyData articyData = null;
		private string projectTitle = null;
		private string projectAuthor = null;
		private string projectVersion = null;

		private Vector2 scrollPosition = Vector2.zero;
		private bool articyAttributesFoldout = true;
		private bool articyEntitiesFoldout = false;
		private bool articyLocationsFoldout = false;
		private bool articyVariablesFoldout = false;
		private bool articyDialoguesFoldout = false;
		private bool articyFlowFoldout = false;
		
		private const float ToggleWidth = 16;
		
		public static bool IsOpen { get { return (instance != null); } }
		
		void OnEnable() {
			instance = this;
			minSize = new Vector2(320, 128);
		}
		
		void OnDisable() {
			prefs.Save();
			instance = null;
		}

		/// <summary>
		/// Draws the converter window.
		/// </summary>
		void OnGUI() {
			EditorGUIUtility.LookLikeControls();
			EditorStyles.textField.wordWrap = true;
			scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
			DrawProjectFilenameField();
			DrawStageDirectionsCheckbox();
			DrawPortraitFolderField();
			DrawArticyContent();
			DrawEncodingPopup();
			DrawSaveToField();
			DrawOverwriteCheckbox();
			DrawConversionButtons();
			EditorGUILayout.EndScrollView();
		}
		
		/// <summary>
		/// Draws the articy:draft Project filename field.
		/// </summary>
		private void DrawProjectFilenameField() {
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.TextField("articy:draft Project", prefs.ProjectFilename);
			if (GUILayout.Button("...", EditorStyles.miniButtonRight, GUILayout.Width(22))) {
				prefs.ProjectFilename = EditorUtility.OpenFilePanel("Select articy:draft Project", EditorWindowTools.GetDirectoryName(prefs.ProjectFilename), "xml");
				GUIUtility.keyboardControl = 0;
			}
			EditorGUILayout.EndHorizontal();
		}
		
		/// <summary>
		/// Draws the Portrait Folder field.
		/// </summary>
		private void DrawPortraitFolderField() {
			EditorGUILayout.BeginHorizontal();
			prefs.PortraitFolder = EditorGUILayout.TextField("Portrait Folder", prefs.PortraitFolder);
			if (GUILayout.Button("...", EditorStyles.miniButtonRight, GUILayout.Width(22))) {
				prefs.PortraitFolder = EditorUtility.OpenFolderPanel("Location of Portrait Textures", prefs.PortraitFolder, "");
				prefs.PortraitFolder = "Assets" + prefs.PortraitFolder.Replace(Application.dataPath, string.Empty);
				GUIUtility.keyboardControl = 0;
			}
			EditorGUILayout.EndHorizontal();
		}

		/// <summary>
		/// Draws the Stage Dirs Are Sequences checkbox.
		/// </summary>
		private void DrawStageDirectionsCheckbox() {
			prefs.StageDirectionsAreSequences = EditorGUILayout.Toggle(new GUIContent("StageDirs are Sequences", "Tick if articy:draft Stage Directions contain Dialogue System sequences"), prefs.StageDirectionsAreSequences);
		}
		
		/// <summary>
		/// Draws the encoding popup.
		/// </summary>
		private void DrawEncodingPopup() {
			prefs.EncodingType = (EncodingType) EditorGUILayout.EnumPopup("Encoding", prefs.EncodingType, GUILayout.Width(300));
		}
		
		/// <summary>
		/// Draws the Save To field.
		/// </summary>
		private void DrawSaveToField() {
			EditorGUILayout.BeginHorizontal();
			prefs.OutputFolder = EditorGUILayout.TextField("Save To", prefs.OutputFolder);
			if (GUILayout.Button("...", EditorStyles.miniButtonRight, GUILayout.Width(22))) {
				prefs.OutputFolder = EditorUtility.SaveFolderPanel("Path to Save Database", prefs.OutputFolder, "");
				prefs.OutputFolder = "Assets" + prefs.OutputFolder.Replace(Application.dataPath, string.Empty);
				GUIUtility.keyboardControl = 0;
			}
			EditorGUILayout.EndHorizontal();
		}
		
		/// <summary>
		/// Draws the Overwrite checkbox.
		/// </summary>
		private void DrawOverwriteCheckbox() {
			prefs.Overwrite = EditorGUILayout.Toggle("Overwrite", prefs.Overwrite);
		}
		
		/// <summary>
		/// Draws the buttons Review, Clear, and Convert.
		/// </summary>
		private void DrawConversionButtons() {
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			DrawReviewButton();
			DrawClearButton();
			DrawConvertButton();
			EditorGUILayout.EndHorizontal();
		}
		
		/// <summary>
		/// Draws the Review button, and loads/reviews the project if clicked.
		/// </summary>
		private void DrawReviewButton() {
			EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(prefs.ProjectFilename));
			if (GUILayout.Button("Review", GUILayout.Width(100))) ReviewArticyProject();
			EditorGUI.EndDisabledGroup();
		}
		
		/// <summary>
		/// Draws the Clear button, and clears the project if clicked.
		/// </summary>
		private void DrawClearButton() {
			if (GUILayout.Button("Clear", GUILayout.Width(100))) ClearArticyProject();
		}
		
		/// <summary>
		/// Draws the Convert button, and converts the project into a dialogue database if clicked.
		/// </summary>
		private void DrawConvertButton() {
			EditorGUI.BeginDisabledGroup(string.IsNullOrEmpty(prefs.ProjectFilename) || string.IsNullOrEmpty(prefs.OutputFolder));
			if (GUILayout.Button("Convert", GUILayout.Width(100))) {
				ConvertArticyProject();
				if (DialogueEditor.DialogueEditorWindow.instance != null) {
					DialogueEditor.DialogueEditorWindow.instance.Repaint();
				}
			}
			EditorGUI.EndDisabledGroup();
			
		}
		
		/// <summary>
		/// Draws the articy content for review.
		/// </summary>
		private void DrawArticyContent() {
			if (articyData != null) {
				DrawHorizontalLine();
				DrawArticyAttributes();
				DrawArticyEntities();
				DrawArticyLocations();
				DrawArticyFlow();
				DrawArticyVariables();
				DrawArticyDialogues();
				DrawHorizontalLine();
			}
		}
		
		private void DrawArticyAttributes() {
			articyAttributesFoldout = EditorGUILayout.Foldout(articyAttributesFoldout, "Attributes");
			if (articyAttributesFoldout) {
				StartIndentedSection();
				EditorGUI.BeginDisabledGroup(true);
				EditorGUILayout.TextField("Title", projectTitle);
				EditorGUILayout.TextField("Author", projectAuthor);
				EditorGUILayout.TextField("Version", projectVersion);
				EditorGUI.EndDisabledGroup();
				EndIndentedSection();
			}
		}
		
		private void DrawArticyEntities() {
			articyEntitiesFoldout = EditorGUILayout.Foldout(articyEntitiesFoldout, "Entities (Actors & Items)");
			if (articyEntitiesFoldout) {
				StartIndentedSection();
				foreach (ArticyData.Entity entity in articyData.entities.Values) {
					EditorGUILayout.BeginHorizontal();
					bool needToSetCategory = !prefs.ConversionSettings.ConversionSettingExists(entity.id);
					ConversionSetting conversionSetting = prefs.ConversionSettings.GetConversionSetting(entity.id);
					if (needToSetCategory) conversionSetting.Category = entity.technicalName.StartsWith("Chr") ? EntityCategory.NPC : EntityCategory.Item;
					conversionSetting.Include = EditorGUILayout.Toggle(conversionSetting.Include, GUILayout.Width(ToggleWidth));
					conversionSetting.Category = (EntityCategory) EditorGUILayout.EnumPopup(conversionSetting.Category, GUILayout.Width(64));
					EditorGUI.BeginDisabledGroup(true);
					EditorGUILayout.TextField(entity.displayName.DefaultText);
					EditorGUILayout.TextField(entity.technicalName);
					EditorGUI.EndDisabledGroup();
					EditorGUILayout.EndHorizontal();
				}
				EndIndentedSection();
			}
		}
		
		private void DrawArticyLocations() {
			articyLocationsFoldout = EditorGUILayout.Foldout(articyLocationsFoldout, "Locations");
			if (articyLocationsFoldout) {
				StartIndentedSection();
				foreach (ArticyData.Location location in articyData.locations.Values) {
					EditorGUILayout.BeginHorizontal();
					ConversionSetting conversionSetting = prefs.ConversionSettings.GetConversionSetting(location.id);
					conversionSetting.Include = EditorGUILayout.Toggle(conversionSetting.Include, GUILayout.Width(ToggleWidth));
					EditorGUI.BeginDisabledGroup(true);
					EditorGUILayout.LabelField(location.displayName.DefaultText);
					EditorGUILayout.TextField(location.technicalName);
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.TextArea(location.text.DefaultText);
					EditorGUI.EndDisabledGroup();
					EditorGUILayout.EndHorizontal();
				}
				EndIndentedSection();
			}
		}
		
		private void DrawArticyVariables() {
			articyVariablesFoldout = EditorGUILayout.Foldout(articyVariablesFoldout, "Variables");
			if (articyVariablesFoldout) {
				StartIndentedSection();
				foreach (ArticyData.VariableSet variableSet in articyData.variableSets.Values) {
					foreach (ArticyData.Variable variable in variableSet.variables) {
						EditorGUILayout.BeginHorizontal();
						string id = ArticyData.FullVariableName(variableSet, variable);
						ConversionSetting conversionSetting = prefs.ConversionSettings.GetConversionSetting(id);
						conversionSetting.Include = EditorGUILayout.Toggle(conversionSetting.Include, GUILayout.Width(ToggleWidth));
						EditorGUI.BeginDisabledGroup(true);
						EditorGUILayout.TextField(id);
						EditorGUILayout.TextField(variable.technicalName);
						EditorGUI.EndDisabledGroup();
						EditorGUILayout.EndHorizontal();
					}
				}
				EndIndentedSection();
			}
		}
		
		private void DrawArticyDialogues() {
			articyDialoguesFoldout = EditorGUILayout.Foldout(articyDialoguesFoldout, "Dialogues");
			if (articyDialoguesFoldout) {
				StartIndentedSection();
				foreach (ArticyData.Dialogue dialogue in articyData.dialogues.Values) {
					EditorGUILayout.BeginHorizontal();
					ConversionSetting conversionSetting = prefs.ConversionSettings.GetConversionSetting(dialogue.id);
					conversionSetting.Include = EditorGUILayout.Toggle(conversionSetting.Include, GUILayout.Width(ToggleWidth));
					EditorGUI.BeginDisabledGroup(true);
					EditorGUILayout.LabelField(dialogue.displayName.DefaultText);
					EditorGUILayout.TextField(dialogue.technicalName);
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.TextArea(dialogue.text.DefaultText);
					EditorGUI.EndDisabledGroup();
					EditorGUILayout.EndHorizontal();
				}
				EndIndentedSection();
			}
		}
		
		private void DrawArticyFlow() {
			articyFlowFoldout = EditorGUILayout.Foldout(articyFlowFoldout, "Flow (Quests)");
			if (articyFlowFoldout) {
				StartIndentedSection();
				foreach (ArticyData.FlowFragment flowFragment in articyData.flowFragments.Values) {
					EditorGUILayout.BeginHorizontal();
					ConversionSetting conversionSetting = prefs.ConversionSettings.GetConversionSetting(flowFragment.id);
					conversionSetting.Include = EditorGUILayout.Toggle(conversionSetting.Include, GUILayout.Width(ToggleWidth));
					EditorGUI.BeginDisabledGroup(true);
					EditorGUILayout.LabelField(flowFragment.displayName.DefaultText);
					EditorGUILayout.TextField(flowFragment.technicalName);
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.BeginHorizontal();
					EditorGUILayout.TextArea(flowFragment.text.DefaultText);
					EditorGUI.EndDisabledGroup();
					EditorGUILayout.EndHorizontal();
				}
				EndIndentedSection();
			}
		}
		
		private void DrawHorizontalLine() {
			GUILayout.Box("", new GUILayoutOption[]{GUILayout.ExpandWidth(true), GUILayout.Height(1)});
		}
		
		private void StartIndentedSection() {
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField(string.Empty, GUILayout.Width(8));
			EditorGUILayout.BeginVertical();
		}
		
		private void EndIndentedSection() {
			EditorGUILayout.EndVertical();
			EditorGUILayout.EndHorizontal();
		}
		
		private void ReviewArticyProject() {
			articyData = ArticySchemaTools.LoadArticyDataFromXmlFile(prefs.ProjectFilename, prefs.Encoding);
			if (articyData != null) {
				projectTitle = articyData.ProjectTitle;
				projectVersion = articyData.ProjectVersion;
				projectAuthor = articyData.ProjectAuthor;
				Debug.Log(string.Format("{0}: Loaded {1}", DialogueDebug.Prefix, prefs.ProjectFilename));
			} else {
				Debug.LogError(string.Format("{0}: Failed to load {1}", DialogueDebug.Prefix, prefs.ProjectFilename));
			}
		}
		
		private void ClearArticyProject() {
			articyData = null;
			prefs.ConversionSettings.Clear();
		}
		
		/// <summary>
		/// Converts the articy project.
		/// </summary>
		public void ConvertArticyProject() {
			if (articyData == null) ReviewArticyProject();
			if (articyData != null) try {
				string assetName = projectTitle.Replace(':', '_');
				DialogueDatabase database = LoadOrCreateDatabase(assetName);
				if (database == null) {
					Debug.LogError(string.Format("{0}: Couldn't create asset '{1}'.", DialogueDebug.Prefix, assetName));
				} else {
					ArticyConverter.ConvertArticyDataToDatabase(articyData, prefs, database);
					EditorUtility.SetDirty(database);
					AssetDatabase.SaveAssets();
					Debug.Log(string.Format("{0}: Created database '{1}' containing {2} actors, {3} conversations, {4} items/quests, {5} variables, and {6} locations.", 
						DialogueDebug.Prefix, assetName, database.actors.Count, database.conversations.Count, database.items.Count, database.variables.Count, database.locations.Count));
				}
			} finally {
				EditorUtility.ClearProgressBar();
			}
		}
		
		/// <summary>
		/// Loads the dialogue database if it already exists and overwrite is ticked; otherwise creates a new one.
		/// </summary>
		/// <returns>
		/// The database.
		/// </returns>
		/// <param name='filename'>
		/// Asset filename.
		/// </param>
		private DialogueDatabase LoadOrCreateDatabase(string filename) {
			string assetPath = string.Format("{0}/{1}.asset", prefs.OutputFolder, filename);
			DialogueDatabase database = null;
			if (prefs.Overwrite) {
				database = AssetDatabase.LoadAssetAtPath(assetPath, typeof(DialogueDatabase)) as DialogueDatabase;
				if (database != null) database.Clear();
			}
			if (database == null) {
				assetPath = AssetDatabase.GenerateUniqueAssetPath(string.Format("{0}/{1}.asset", prefs.OutputFolder, filename));
				database = ScriptableObject.CreateInstance<DialogueDatabase>();
				AssetDatabase.CreateAsset(database, assetPath);
			}
			return database;
		}
		
	}

}
