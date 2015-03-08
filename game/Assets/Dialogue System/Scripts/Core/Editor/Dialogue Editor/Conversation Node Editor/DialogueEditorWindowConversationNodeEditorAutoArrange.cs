using UnityEngine;
using UnityEditor;
using UnityEditor.Graphs;
using System;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.DialogueEditor {

	/// <summary>
	/// This part of the Dialogue Editor window handles the auto-arrange
	/// feature for the conversation node editor.
	/// </summary>
	public partial class DialogueEditorWindow {

		private const float AutoWidthBetweenNodes = 20f;
		private const float AutoHeightBetweenNodes = 20f;

		private const float AutoStartX = 20f;
		private const float AutoStartY = 20f;

		private void CheckNodeArrangement() {
			if (startEntry == null) return;
			if ((startEntry.canvasRect.x == 0) && (startEntry.canvasRect.y == 0)) AutoArrangeNodes();
		}

		private void ConfirmAndAutoArrangeNodes() {
			if (EditorUtility.DisplayDialog("Auto-Arrange Nodes", "Are you sure you want to auto-arrange the nodes in this conversation?", "OK", "Cancel")) {
				AutoArrangeNodes();
			}
		}
		
		private void AutoArrangeNodes() {
			InitializeDialogueTree();
			List<List<DialogueEntry>> tree = new List<List<DialogueEntry>>();
			ArrangeGatherChildren(dialogueTree, 0, tree);
			ArrangeTree(tree);
			ArrangeOrphans();
		}

		private void ArrangeGatherChildren(DialogueNode node, int level, List<List<DialogueEntry>> tree) {
			if (node == null) return;
			while (tree.Count <= level) {
				tree.Add(new List<DialogueEntry>());
			}
			if (!tree[level].Contains(node.entry)) tree[level].Add(node.entry);
			if (node.hasFoldout) {
				foreach (var child in node.children) {
					ArrangeGatherChildren(child, level + 1, tree);
				}
			}
		}

		private float GetTreeWidth(List<List<DialogueEntry>> tree) {
			float maxWidth = 0;
			foreach (List<DialogueEntry> level in tree) {
				float levelWidth = level.Count * (DialogueEntry.CanvasRectWidth + AutoWidthBetweenNodes);
				maxWidth = Mathf.Max(maxWidth, levelWidth);
			}
			return maxWidth;
		}

		private void ArrangeTree(List<List<DialogueEntry>> tree) {
			float treeWidth = GetTreeWidth(tree);
			float x = AutoStartX;
			if (orphans.Count > 0) x += DialogueEntry.CanvasRectWidth + AutoWidthBetweenNodes;
			float y = AutoStartY;
			for (int level = 0; level < tree.Count; level++) {
				ArrangeLevel(tree[level], x, y, treeWidth);
				y += DialogueEntry.CanvasRectHeight + AutoHeightBetweenNodes;
			}
		}

		private void ArrangeLevel(List<DialogueEntry> nodes, float x, float y, float treeWidth) {
			if (nodes == null || nodes.Count == 0) return;
			float nodeCanvasWidth = treeWidth / nodes.Count;
			float nodeCanvasOffset = (nodeCanvasWidth - DialogueEntry.CanvasRectWidth) / 2;
			for (int i = 0; i < nodes.Count; i++) {
				float nodeX = x + (i * nodeCanvasWidth) + nodeCanvasOffset;
				nodes[i].canvasRect = new Rect(nodeX, y, DialogueEntry.CanvasRectWidth, DialogueEntry.CanvasRectHeight);
			}
		}

		private void ArrangeOrphans() {
			float y = AutoStartY;
			foreach (var orphan in orphans) {
				orphan.entry.canvasRect.x = AutoStartX;
				orphan.entry.canvasRect.y = y;
				y += orphan.entry.canvasRect.height + AutoHeightBetweenNodes;
			}
		}

	}

}