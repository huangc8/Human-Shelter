using UnityEngine;
using UnityEditor;
using UnityEditor.Graphs;
using System;
using System.Collections.Generic;
using PixelCrushers.DialogueSystem;

namespace PixelCrushers.DialogueSystem.DialogueEditor {

	/// <summary>
	/// This part of the Dialogue Editor window handles the main code for 
	/// the conversation node editor.
	/// </summary>
	public partial class DialogueEditorWindow {

		private bool nodeEditorDeleteCurrentConversation = false;

		[SerializeField]
		private bool showActorNames;

		private bool isMakingLink = false;
		private DialogueEntry linkSourceEntry = null;
		private DialogueEntry linkTargetEntry = null;

		private Link selectedLink = null;
		private Link newSelectedLink = null;

		private ConversationState currentConversationState = null;
		private DialogueEntry currentRuntimeEntry = null;


		public class MultinodeSelection {
			public List<DialogueEntry> nodes = new List<DialogueEntry>();
		}

		private MultinodeSelection multinodeSelection = new MultinodeSelection();

		private bool dragged = false;

		private bool isLassoing = false;
		private Rect lassoRect = new Rect(0,0,0,0);

		private void DrawConversationSectionNodeStyle() {
			if (Application.isPlaying && DialogueManager.HasInstance) {
				currentConversationState = DialogueManager.CurrentConversationState;
				currentRuntimeEntry = (currentConversationState != null && currentConversationState.subtitle != null)
					? currentConversationState.subtitle.dialogueEntry
						: null;
			} else {
				currentRuntimeEntry = null;
			}
			CheckDialogueTreeGUIStyles();
			if (nodeEditorDeleteCurrentConversation) DeleteCurrentConversationInNodeEditor();
			if (inspectorSelection == null) inspectorSelection = currentConversation;
			DrawCanvas();
			DrawNodeEditorTopControls();
			HandleEmptyCanvasEvents();
			HandleKeyEvents();
		}

		private void DrawCanvasContents() {
			if (currentConversation == null) return;
			newSelectedLink = null;
			DrawAllConnectors();
			DrawAllNodes();
			DrawLasso();
			CheckNewSelectedLink();
			newSelectedLink = null;
		}

		private void UpdateRuntimeConversationsTab() {
			if (DialogueManager.HasInstance) {
				var newConversationState = DialogueManager.CurrentConversationState;
				if (newConversationState != currentConversationState) {
					currentConversationState = newConversationState;
					currentRuntimeEntry = (currentConversationState != null && currentConversationState.subtitle != null)
						? currentConversationState.subtitle.dialogueEntry
							: null;
					Repaint();
				}
			}
		}

		private void CheckNewSelectedLink() {
			if ((newSelectedLink != null) && (newSelectedLink != selectedLink)) {
				selectedLink = newSelectedLink;
				inspectorSelection = selectedLink;
				isLassoing = false;
				multinodeSelection.nodes.Clear();
				UnityEngine.Event.current.Use();
			} else if (currentEntry != null) {
				selectedLink = null;
			}
		}

		private void DrawAllConnectors() {
			foreach (var entry in currentConversation.dialogueEntries) {
				DrawEntryConnectors(entry);
			}
			if (isMakingLink) DrawNewLinkConnector();
		}

		private void DrawEntryConnectors(DialogueEntry entry) {
			if (entry == null) return;
			foreach (var link in entry.outgoingLinks) {
				DialogueEntry destination = currentConversation.dialogueEntries.Find(e => e.id == link.destinationDialogueID);
				if (destination != null) {
					Vector3 start = new Vector3(entry.canvasRect.center.x, entry.canvasRect.center.y, 0);
					Vector3 end = new Vector3(destination.canvasRect.center.x, destination.canvasRect.center.y, 0);
					Color connectorColor = (link == selectedLink) ? new Color(0.5f, 0.75f, 1f, 1f) : Color.white;
					if (entry == currentRuntimeEntry) {
						connectorColor = IsValidRuntimeLink(link) ? Color.green : Color.red;
					}
					DrawLink(start, end, connectorColor);
					HandleConnectorEvents(link, start, end);
				}
			}
		}

		private bool IsValidRuntimeLink(Link link) {
			return IsValidRuntimeResponse(link, currentConversationState.pcResponses) ||
				IsValidRuntimeResponse(link, currentConversationState.npcResponses);
		}

		private bool IsValidRuntimeResponse(Link link, Response[] responses) {
			foreach (var response in responses) {
				if ((link.destinationConversationID == response.destinationEntry.conversationID) &&
				    (link.destinationDialogueID == response.destinationEntry.id)) {
					return true;
				}
			}
			return false;
		}

		private void DrawNewLinkConnector() {
			if (isMakingLink && (linkSourceEntry != null)) {
				Vector3 start = new Vector3(linkSourceEntry.canvasRect.center.x, linkSourceEntry.canvasRect.center.y, 0);
				if ((linkTargetEntry != null) && UnityEngine.Event.current.isMouse) {
					if (!linkTargetEntry.canvasRect.Contains(UnityEngine.Event.current.mousePosition)) {
						linkTargetEntry = null;
					}
				}
				Vector3 end = (linkTargetEntry != null)
					? new Vector3(linkTargetEntry.canvasRect.center.x, linkTargetEntry.canvasRect.center.y, 0)
						: new Vector3(UnityEngine.Event.current.mousePosition.x, UnityEngine.Event.current.mousePosition.y, 0);
				DrawLink(start, end, Color.white);
			}
		}

		private void HandleConnectorEvents(Link link, Vector3 start, Vector3 end) {
			switch (UnityEngine.Event.current.type) {
			case EventType.mouseUp:
				bool isReallyLassoing = isLassoing && (Mathf.Abs(lassoRect.width) > 10f) && (Mathf.Abs(lassoRect.height) > 10f);
				if (!isReallyLassoing && (UnityEngine.Event.current.button == LeftMouseButton) && IsPointOnLineSegment(UnityEngine.Event.current.mousePosition, start, end)) {
					newSelectedLink = link;
					currentEntry = null;
					inspectorSelection = newSelectedLink;
				}
				break;
			}
		}

		private bool IsPointOnLineSegment(Vector2 point, Vector3 start, Vector3 end) {
			const float tolerance = 10f;
			float minX = Mathf.Min(start.x, end.x);
			float minY = Mathf.Min(start.y, end.y);
			float width = Mathf.Abs(start.x - end.x);
			float height = Mathf.Abs(start.y - end.y);
			float midX = minX + (width / 2);
			if ((width <= tolerance) && (Mathf.Abs(point.x - midX) <= tolerance) && (minY <= point.y) && (point.y <= minY + height)) {
				return true; // Special case: vertical line.
			}
			Rect boundingRect = new Rect(minX, minY, width, height);
			if (boundingRect.Contains(point)) {
				float slope = (end.y - start.y) / (end.x - start.x);
				float yIntercept = -(slope * start.x) + start.y;
				float distanceFromLine = Mathf.Abs(point.y - (slope * point.x + yIntercept));
				return (distanceFromLine <= tolerance);
			}
			return false;		
		}

		public void DrawSelectedLinkContents() {
			if (selectedLink == null) return;
			selectedLink.priority = (ConditionPriority) EditorGUILayout.Popup("Priority", (int) selectedLink.priority, priorityStrings);
		}

		private void DrawAllNodes() {
			for (int i = 0; i < currentConversation.dialogueEntries.Count; i++) {
				DrawEntryNode(currentConversation.dialogueEntries[i]);
			}
			for (int i = currentConversation.dialogueEntries.Count - 1; i >= 0; i--) {
				HandleNodeEvents(currentConversation.dialogueEntries[i]);
			}

		}

		private void DrawEntryNode(DialogueEntry entry) {
			bool isSelected = multinodeSelection.nodes.Contains(entry);
			Styles.Color nodeColor = (entry.id == 0) ? Styles.Color.Orange
				: database.IsPlayerID(entry.ActorID) ? Styles.Color.Blue : Styles.Color.Gray;
			if (entry == currentRuntimeEntry) {
				nodeColor = Styles.Color.Green;
			}
			string nodeLabel = GetDialogueEntryNodeText(entry);
			if (showActorNames) {
				GUIStyle nodeStyle = new GUIStyle(Styles.GetNodeStyle("node", nodeColor, isSelected));
				nodeStyle.padding.top = 23;
				nodeStyle.padding.bottom = 0;
				GUI.Box(entry.canvasRect, nodeLabel, nodeStyle);
			} else {
				GUI.Box(entry.canvasRect, nodeLabel, Styles.GetNodeStyle("node", nodeColor, isSelected));
			}
		}

		private bool IsDragCanvasEvent() {
			return (UnityEngine.Event.current.button == MiddleMouseButton) ||
				((UnityEngine.Event.current.button == LeftMouseButton) && UnityEngine.Event.current.control && UnityEngine.Event.current.alt);
		}

		private bool IsRightMouseButtonEvent() {
			// Single button Mac mouse ctrl+click is the same as right click:
			return (UnityEngine.Event.current.button == RightMouseButton) ||
				((UnityEngine.Event.current.button == LeftMouseButton) && UnityEngine.Event.current.control && !UnityEngine.Event.current.alt);
		}

		private void HandleKeyEvents() {
			if (UnityEngine.Event.current.isKey && UnityEngine.Event.current.type == EventType.KeyDown) {
				if (UnityEngine.Event.current.keyCode == KeyCode.Delete) {

					// Delete key:
					if (selectedLink != null) {
						UnityEngine.Event.current.Use();
						DeleteLinkCallback(selectedLink);
					} else if (currentEntry != null) {
						UnityEngine.Event.current.Use();
						if (multinodeSelection.nodes.Count > 1) {
							DeleteMultipleEntriesCallback(currentEntry);
						} else if (currentEntry != startEntry) {
							DeleteEntryCallback(currentEntry);
						}
					}
				} else if (UnityEngine.Event.current.keyCode == KeyCode.D && (UnityEngine.Event.current.command || UnityEngine.Event.current.control)) {

					// Ctrl-D (Duplicate) key:
					if (currentEntry != null) {
						UnityEngine.Event.current.Use();
						//DuplicateCurrentEntry();
						DuplicateMultipleEntries();
					}
				}
			}
		}
		
		private void HandleNodeEvents(DialogueEntry entry) {
			switch (UnityEngine.Event.current.type) {
			case EventType.mouseDown:
				if (entry.canvasRect.Contains(UnityEngine.Event.current.mousePosition)) {
					if (IsRightMouseButtonEvent()) {
						currentEntry = entry;
						ShowNodeContextMenu(entry);
						UnityEngine.Event.current.Use();
					} else if (UnityEngine.Event.current.button == LeftMouseButton)  {
						newSelectedLink = null;
						if (isMakingLink) {
							FinishMakingLink();
						} else {
							nodeToDrag = entry;
							dragged = false;
							if (!IsShiftDown() && ((multinodeSelection.nodes.Count <= 1) || !multinodeSelection.nodes.Contains(entry))) {
								SetCurrentEntry(entry);
							}
						}
						UnityEngine.Event.current.Use();
					}
				}
				break;
			case EventType.mouseUp:
				if (UnityEngine.Event.current.button == LeftMouseButton) {
					if (!isMakingLink && entry.canvasRect.Contains(UnityEngine.Event.current.mousePosition)) {
						newSelectedLink = null;
						if (isLassoing) {
							FinishLasso();
						} else if (IsShiftDown()) {
							if (multinodeSelection.nodes.Contains(entry)) {
								RemoveEntryFromSelection(entry);
							} else {
								AddEntryToSelection(entry);
							}
						} else {
							if (!(dragged && (multinodeSelection.nodes.Count > 1))) {
								SetCurrentEntry(entry);
							}
						}
						nodeToDrag = null;
						dragged = false;
						UnityEngine.Event.current.Use();
					}
				}
				break;
			case EventType.mouseDrag:
				if ((entry == nodeToDrag)) {
					dragged = true;
					//if (IsControlDown() && IsAltDown()) {
					//	DragNodes(currentConversation.dialogueEntries);
					//} else {
						DragNodes(multinodeSelection.nodes);
						//foreach (DialogueEntry dragEntry in multinodeSelection.nodes) {
						//	dragEntry.canvasRect.x += Event.current.delta.x;
						//	dragEntry.canvasRect.x = Mathf.Max(1f, dragEntry.canvasRect.x);
						//	dragEntry.canvasRect.y += Event.current.delta.y;
						//	dragEntry.canvasRect.y = Mathf.Max(1f, dragEntry.canvasRect.y);
						//}
					//}
					UnityEngine.Event.current.Use();
				}
				break;
			}
			if (isMakingLink && UnityEngine.Event.current.isMouse) {
				if (entry.canvasRect.Contains(UnityEngine.Event.current.mousePosition)) {
					linkTargetEntry = entry;
				}
			}
		}

		private void DragNodes(List<DialogueEntry> nodeList) {
			foreach (DialogueEntry dragEntry in nodeList) {
				dragEntry.canvasRect.x += UnityEngine.Event.current.delta.x;
				dragEntry.canvasRect.x = Mathf.Max(1f, dragEntry.canvasRect.x);
				dragEntry.canvasRect.y += UnityEngine.Event.current.delta.y;
				dragEntry.canvasRect.y = Mathf.Max(1f, dragEntry.canvasRect.y);
			}
		}

		private bool IsModifierDown(EventModifiers modifier) {
			return (UnityEngine.Event.current.modifiers & modifier) == modifier;
		}

		private bool IsShiftDown() {
			return IsModifierDown(EventModifiers.Shift);
		}
		
		private bool IsControlDown() {
			return IsModifierDown(EventModifiers.Control);
		}

		private bool IsAltDown() {
			return IsModifierDown(EventModifiers.Alt);
		}

		private void SetCurrentEntry(DialogueEntry entry) {
			newSelectedLink = null;
			if (entry != currentEntry) ResetLuaWizards();
			currentEntry = entry;
			multinodeSelection.nodes.Clear();
			multinodeSelection.nodes.Add(entry);
			UpdateEntrySelection();
		}

		private void AddEntryToSelection(DialogueEntry entry) {
			newSelectedLink = null;
			currentEntry = entry;
			multinodeSelection.nodes.Add(entry);
			UpdateEntrySelection();
		}

		private void RemoveEntryFromSelection(DialogueEntry entry) {
			newSelectedLink = null;
			multinodeSelection.nodes.Remove(entry);
			if (multinodeSelection.nodes.Count == 0) {
				currentEntry = null;
			} else {
				currentEntry = multinodeSelection.nodes[multinodeSelection.nodes.Count-1];
			}
			UpdateEntrySelection();
		}

		private void UpdateEntrySelection() {
			ResetDialogueTreeCurrentEntryParticipants();
			if (multinodeSelection.nodes.Count == 0) {
				inspectorSelection = currentConversation;
				selectedLink = null;
			} else if (multinodeSelection.nodes.Count == 1) {
				inspectorSelection = currentEntry;
			} else {
				inspectorSelection = multinodeSelection;
			}
		}

		private void HandleEmptyCanvasEvents() {
			wantsMouseMove = true;
			UnityEngine.Event e = UnityEngine.Event.current;
			switch (UnityEngine.Event.current.type) {
			case EventType.mouseDown:
				isDraggingCanvas = IsDragCanvasEvent();
				if (isMakingLink) {
					if ((UnityEngine.Event.current.button == LeftMouseButton) || (UnityEngine.Event.current.button == RightMouseButton)) {
						FinishMakingLink();
					}
				} else if (IsRightMouseButtonEvent()) {
					if (selectedLink != null) {
						ShowLinkContextMenu();
					} else {
						if (currentConversation != null) {
							ShowEmptyCanvasContextMenu();
						}
					}
				} else if (UnityEngine.Event.current.button == LeftMouseButton) {
					isLassoing = true;
					lassoRect = new Rect(UnityEngine.Event.current.mousePosition.x + canvasScrollPosition.x, UnityEngine.Event.current.mousePosition.y + canvasScrollPosition.y, 1, 1);
				}
				break;
			case EventType.mouseUp:
				isDraggingCanvas = false;
				if (isLassoing) {
					FinishLasso();
					newSelectedLink = null;
				} else if (newSelectedLink == null) {
					currentEntry = null;
					selectedLink = null;
					multinodeSelection.nodes.Clear();
					inspectorSelection = currentConversation;
				}
				break;
			case EventType.mouseDrag:
				if (isDraggingCanvas) {
					canvasScrollPosition -= UnityEngine.Event.current.delta;
					canvasScrollPosition.x = Mathf.Clamp(canvasScrollPosition.x, 0, Mathf.Infinity);
					canvasScrollPosition.y = Mathf.Clamp(canvasScrollPosition.y, 0, Mathf.Infinity);
				} else if (isLassoing) {
					lassoRect.width += UnityEngine.Event.current.delta.x;
					lassoRect.height += UnityEngine.Event.current.delta.y;
				}
				break;
			}
			if (UnityEngine.Event.current.isMouse) e.Use();
		}

		private void DrawLasso() {
			if (isLassoing) {
				Color originalColor = GUI.color;
				GUI.color = new Color(1, 1, 1, 0.5f);
				GUI.Box(lassoRect, string.Empty);
				GUI.color = originalColor;
			}
		}

		private void FinishLasso() {
			if (currentConversation == null) return;
			isLassoing = false;
			lassoRect = new Rect(Mathf.Min(lassoRect.x, lassoRect.x + lassoRect.width), 
			                     Mathf.Min(lassoRect.y, lassoRect.y + lassoRect.height), 
			                     Mathf.Abs(lassoRect.width), 
			                     Mathf.Abs(lassoRect.height));
			currentEntry = null;
			if (!IsShiftDown()) multinodeSelection.nodes.Clear();
			foreach (var entry in currentConversation.dialogueEntries) {
				if (lassoRect.Overlaps(entry.canvasRect)) {
					currentEntry = entry;
					if (!multinodeSelection.nodes.Contains(entry)) multinodeSelection.nodes.Add(entry);
				}
			}
			UpdateEntrySelection();
		}

		private void ShowEmptyCanvasContextMenu() {
			GenericMenu contextMenu = new GenericMenu();
			contextMenu.AddItem(new GUIContent("Create Node"), false, AddChildCallback, null);
			contextMenu.AddItem(new GUIContent("Arrange Nodes"), false, ArrangeNodesCallback, null);
			contextMenu.AddItem(new GUIContent("Delete Conversation"), false, DeleteConversationCallback, null);
			contextMenu.ShowAsContext();
		}

		private void ShowLinkContextMenu() {
			GenericMenu contextMenu = new GenericMenu();
			contextMenu.AddItem(new GUIContent("Delete Link"), false, DeleteLinkCallback, selectedLink);
			contextMenu.AddItem(new GUIContent("Arrange Nodes"), false, ArrangeNodesCallback, null);
			contextMenu.ShowAsContext();
		}

		private void ShowNodeContextMenu(DialogueEntry entry) {
			GenericMenu contextMenu = new GenericMenu();
			contextMenu.AddItem(new GUIContent("Create Child Node"), false, AddChildCallback, entry);
			contextMenu.AddItem(new GUIContent("Make Link"), false, MakeLinkCallback, entry);
			if ((multinodeSelection.nodes.Count > 1) && (multinodeSelection.nodes.Contains(entry))) {
				contextMenu.AddItem(new GUIContent("Duplicate"), false, DuplicateMultipleEntriesCallback, entry);
				contextMenu.AddItem(new GUIContent("Delete"), false, DeleteMultipleEntriesCallback, entry);
			} else if (entry == startEntry) {
				contextMenu.AddDisabledItem(new GUIContent("Duplicate"));
				contextMenu.AddDisabledItem(new GUIContent("Delete"));
			} else {
				contextMenu.AddItem(new GUIContent("Duplicate"), false, DuplicateEntryCallback, entry);
				contextMenu.AddItem(new GUIContent("Delete"), false, DeleteEntryCallback, entry);
			}
			contextMenu.AddItem(new GUIContent("Arrange Nodes"), false, ArrangeNodesCallback, entry);
			contextMenu.ShowAsContext();
		}

		private void AddChildCallback(object o) {
			DialogueEntry parentEntry = o as DialogueEntry;
			if (parentEntry == null) parentEntry = startEntry;
			LinkToNewEntry(parentEntry);
			InitializeDialogueTree();
			currentEntry.canvasRect.x = parentEntry.canvasRect.x;
			currentEntry.canvasRect.y = parentEntry.canvasRect.y + parentEntry.canvasRect.height + AutoHeightBetweenNodes;
			SetCurrentEntry(currentEntry);
			inspectorSelection = currentEntry;
			ResetDialogueEntryText();
			Repaint();
		}
		
		private void MakeLinkCallback(object o) {
			linkSourceEntry = o as DialogueEntry;
			isMakingLink = (linkSourceEntry != null);
		}

		private bool LinkExists(DialogueEntry origin, DialogueEntry destination) {
			Link link = origin.outgoingLinks.Find(x => x.destinationDialogueID == destination.id);
			return (link != null);
		}

		private void FinishMakingLink() {
			if ((linkSourceEntry != null) && (linkTargetEntry != null) && 
			    (linkSourceEntry != linkTargetEntry) &&
			    !LinkExists(linkSourceEntry, linkTargetEntry)) {
				Link link = new Link();
				link.originConversationID = currentConversation.id;
				link.originDialogueID = linkSourceEntry.id;
				link.destinationConversationID = currentConversation.id;
				link.destinationDialogueID = linkTargetEntry.id;
				linkSourceEntry.outgoingLinks.Add(link);
				InitializeDialogueTree();
				ResetDialogueEntryText();
				Repaint();
			}
			isMakingLink = false;
			linkSourceEntry = null;
			linkTargetEntry = null;
		}
		
		private void DeleteEntryCallback(object o) {
			DialogueEntry entryToDelete = o as DialogueEntry;
			if (entryToDelete == null) return;
			if (EditorUtility.DisplayDialog("Delete selected entry?", "You cannot undo this action.", "Delete", "Cancel")) {
				foreach (var origin in currentConversation.dialogueEntries) {
					DeleteNodeLinkToDialogueID(origin, entryToDelete.id);
				}
				DialogueEntry entry = currentConversation.dialogueEntries.Find(x => x.id == entryToDelete.id);
				currentConversation.dialogueEntries.Remove(entry);
				InitializeDialogueTree();
				ResetDialogueEntryText();
				Repaint();
			}
		}

		private void DeleteMultipleEntriesCallback(object o) {
			if (EditorUtility.DisplayDialog("Delete selected entries?", "You cannot undo this action.", "Delete", "Cancel")) {
				foreach (DialogueEntry entryToDelete in multinodeSelection.nodes) {
					if (entryToDelete != startEntry) {
						foreach (var origin in currentConversation.dialogueEntries) {
							DeleteNodeLinkToDialogueID(origin, entryToDelete.id);
						}
						DialogueEntry entry = currentConversation.dialogueEntries.Find(x => x.id == entryToDelete.id);
						currentConversation.dialogueEntries.Remove(entry);
					}
				}
				InitializeDialogueTree();
				ResetDialogueEntryText();
				Repaint();
			}
		}
		
		private void DeleteLinkCallback(object o) {
			Link linkToDelete = o as Link;
			if (linkToDelete == null) return;
			if (EditorUtility.DisplayDialog("Delete selected link?", "You cannot undo this action.", "Delete", "Cancel")) {
				DialogueEntry origin = currentConversation.GetDialogueEntry(linkToDelete.originDialogueID);
				DeleteNodeLinkToDialogueID(origin, linkToDelete.destinationDialogueID);
				InitializeDialogueTree();
				ResetDialogueEntryText();
				Repaint();
			}
		}

		private void DeleteNodeLinkToDialogueID(DialogueEntry origin, int destinationDialogueID) {
			if (origin == null) return;
			Link link = origin.outgoingLinks.Find(x => x.destinationDialogueID == destinationDialogueID);
			if (link == null) return;
			origin.outgoingLinks.Remove(link); 
		}

		private void ArrangeNodesCallback(object o) {
			ConfirmAndAutoArrangeNodes();
		}
		
		private void DeleteConversationCallback(object o) {
			if (currentConversation == null) return;
			if (EditorUtility.DisplayDialog(string.Format("Delete '{0}'?", currentConversation.Title), "Are you sure you want to delete this conversation?", "Delete", "Cancel")) {
				nodeEditorDeleteCurrentConversation = true;
			}
		}

		private void DeleteCurrentConversationInNodeEditor() {
			nodeEditorDeleteCurrentConversation = false;
			if (currentConversation != null) database.conversations.Remove(database.conversations.Find(c => c.id == currentConversation.id));
			ResetConversationSection();
			ActivateNodeEditorMode();
			inspectorSelection = database;
		}

		private void DuplicateEntryCallback(object o) {
			DuplicateEntry(currentEntry);
		}

		private void DuplicateMultipleEntriesCallback(object o) {
			DuplicateMultipleEntries();
		}
		
		private void DuplicateMultipleEntries() {
			foreach (var entry in multinodeSelection.nodes) {
				DuplicateEntry(entry);
			}
		}

		private void DuplicateEntry(DialogueEntry entry) {
			if (entry == null || currentConversation == null) return;
			DialogueEntry newEntry = new DialogueEntry(entry);
			newEntry.id = GetNextDialogueEntryID();
			foreach (var link in newEntry.outgoingLinks) {
				link.originDialogueID = newEntry.id;
			}
			currentConversation.dialogueEntries.Add(newEntry);
			newEntry.canvasRect.x = entry.canvasRect.x + 10f;
			newEntry.canvasRect.y = entry.canvasRect.y + entry.canvasRect.height + AutoHeightBetweenNodes;
			ApplyDialogueEntryTemplate(newEntry.fields);
			currentEntry = newEntry;
			inspectorSelection = currentEntry;
			InitializeDialogueTree();
			ResetDialogueEntryText();
			Repaint();
		}
		
	}

}