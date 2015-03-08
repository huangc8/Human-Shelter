using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PixelCrushers.DialogueSystem {

	/// <summary>
	/// Custom inspector editor for dialogue database assets.
	/// </summary>
	[CustomEditor (typeof(DialogueDatabase))]
	public class DialogueDatabaseEditor : Editor {

		public static DialogueDatabaseEditor instance = null;
		
		public enum RefreshSource {
			None,
			ChatMapper,
			ArticyDraft,
			AuroraToolset
		};

		/// <summary>
		/// The refresh source for the quick Refresh button.
		/// </summary>
		public RefreshSource refreshSource = RefreshSource.None;

		void OnEnable(){
			instance = this;
			EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemOnGUI;
		}
		
		void OnDisable(){
			instance = null;
			EditorApplication.projectWindowItemOnGUI -= OnProjectWindowItemOnGUI;
		}
		
		/// <summary>
		/// Checks for double-clicks on the dialogue database to open the editor window.
		/// </summary>
		/// <param name="guid">GUID.</param>
		/// <param name="selectionRect">Selection rect.</param>
		private void OnProjectWindowItemOnGUI(string guid, Rect selectionRect) {
			if (UnityEngine.Event.current.type == EventType.MouseDown && UnityEngine.Event.current.clickCount == 2 && selectionRect.Contains(UnityEngine.Event.current.mousePosition)) {
				OpenDialogueEditorWindow();
			}
		}
		
		/// <summary>
		/// Draws the inspector GUI. Provides extra features such a button to 
		/// open the Dialogue Editor window, and a quick reconvert button.
		/// </summary>
		public override void OnInspectorGUI() {
			if (DialogueEditor.DialogueEditorWindow.inspectorSelection != null) {
				DrawInspectorSelection();
			} else {
				DrawExtraFeatures();
				DrawDefaultInspector();
			}
		}

		private void DrawExtraFeatures() {
			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button("Edit...", EditorStyles.miniButton, GUILayout.Width(80))) OpenDialogueEditorWindow();
			GUILayout.FlexibleSpace();
			EditorGUI.BeginDisabledGroup(refreshSource == RefreshSource.None);
			if (GUILayout.Button("Reconvert...", EditorStyles.miniButton, GUILayout.Width(80))) ReconvertDatabase();
			EditorGUI.EndDisabledGroup();
			refreshSource = (RefreshSource) EditorGUILayout.EnumPopup(refreshSource, GUILayout.Width(100));
			EditorGUILayout.EndHorizontal();
			EditorWindowTools.DrawHorizontalLine();
		}

		private void OpenDialogueEditorWindow() {
			PixelCrushers.DialogueSystem.DialogueEditor.DialogueEditorWindow.OpenDialogueEditorWindow();
		}
		
		private void ReconvertDatabase() {
			switch (refreshSource) {
			case RefreshSource.ChatMapper: RunChatMapperConverter(); break;
			case RefreshSource.ArticyDraft: RunArticyConverter(); break;
			case RefreshSource.AuroraToolset: RunAuroraConverter(); break;
			}
			if (DialogueEditor.DialogueEditorWindow.instance != null) {
				DialogueEditor.DialogueEditorWindow.instance.Repaint();
			}
		}
		
		private void RunChatMapperConverter() {
			bool alreadyOpen = ChatMapperConverter.IsOpen;
			if (!alreadyOpen) ChatMapperConverter.Init();
			ChatMapperConverter.Instance.ConvertChatMapperProjects();
			if (!alreadyOpen) ChatMapperConverter.Instance.Close();
		}
		
		private void RunArticyConverter() {
			bool alreadyOpen = Articy.ArticyConverterWindow.IsOpen;
			if (!alreadyOpen) Articy.ArticyConverterWindow.Init();
			Articy.ArticyConverterWindow.Instance.ConvertArticyProject();
			if (!alreadyOpen) Articy.ArticyConverterWindow.Instance.Close();
		}
		
		private void RunAuroraConverter() {
			bool alreadyOpen = Aurora.AuroraConverterWindow.IsOpen;
			if (!alreadyOpen) Aurora.AuroraConverterWindow.Init();
			Aurora.AuroraConverterWindow.Instance.ConvertAuroraFiles();
			if (!alreadyOpen) Aurora.AuroraConverterWindow.Instance.Close();
		}

		private void DrawInspectorSelection() {
			if (DialogueEditor.DialogueEditorWindow.instance == null) return;
			object selection = DialogueEditor.DialogueEditorWindow.inspectorSelection;
			if (selection == null) return;
			Type selectionType = selection.GetType();
			if (selectionType == typeof(Conversation)) {
				DrawInspectorSelectionTitle("Conversation");
				if (DialogueEditor.DialogueEditorWindow.instance.DrawConversationProperties()) {
					DialogueEditor.DialogueEditorWindow.instance.UpdateConversationTitles();
				}
				DialogueEditor.DialogueEditorWindow.instance.DrawConversationFieldsFoldout();
			} else if (selectionType == typeof(DialogueEntry)) {
				DrawInspectorSelectionTitle("Dialogue Entry");
				//if (DialogueEditor.DialogueEditorWindow.instance.DrawDialogueEntryFieldContents()) {
				if (DialogueEditor.DialogueEditorWindow.instance.DrawDialogueEntryInspector()) {
					DialogueEditor.DialogueEditorWindow.instance.ResetDialogueEntryText(selection as DialogueEntry);
					DialogueEditor.DialogueEditorWindow.instance.Repaint();
				}
			} else if (selectionType == typeof(Link)) {
				DrawInspectorSelectionTitle("Link");
				DialogueEditor.DialogueEditorWindow.instance.DrawSelectedLinkContents();
			} else if (selectionType == typeof(DialogueEditor.DialogueEditorWindow.MultinodeSelection)) {
				DrawInspectorSelectionTitle("Multiple Dialogue Entries");
				EditorGUILayout.HelpBox("Dialogue entries cannot be multi-edited.", MessageType.None);
			}
		}

		private void DrawInspectorSelectionTitle(string title) {
			EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
		}
		
	}

}
