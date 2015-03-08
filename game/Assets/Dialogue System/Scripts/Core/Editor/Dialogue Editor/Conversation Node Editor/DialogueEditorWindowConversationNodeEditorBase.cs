using UnityEngine;
using UnityEditor;
using UnityEditor.Graphs;
using System;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem.DialogueEditor {

	/// <summary>
	/// This part of the Dialogue Editor windowprovides the base for
	/// a Mecanim-style editor window.
	/// </summary>
	public partial class DialogueEditorWindow {

		private const int LeftMouseButton = 0;
		private const int RightMouseButton = 1;
		private const int MiddleMouseButton = 2;
		
		private const float MinorGridLineWidth = 12f;
		private const float MajorGridLineWidth  = 120f;

		private readonly Color MinorGridLineColor = new Color(0f, 0f, 0f, 0.2f);
		private readonly Color MajorGridLineColor = new Color(0f, 0f, 0f, 0.4f);

		private const float CanvasSize = 10000f;
		private Vector2 canvasScrollView = new Vector2(CanvasSize, CanvasSize);
		private Vector2 canvasScrollPosition = Vector2.zero;
		
		private bool isDraggingCanvas = false;

		private Texture2D canvasBackgroundColorTexture = null;
		private Material linkArrowMaterial = null;

		private void DrawCanvas() {
			// Make scrollbars invisible:
			GUIStyle verticalScrollbar = GUI.skin.verticalScrollbar;
			GUIStyle horizontalScrollbar = GUI.skin.horizontalScrollbar;
			GUI.skin.verticalScrollbar = GUIStyle.none;
			GUI.skin.horizontalScrollbar = GUIStyle.none;

			try {
				canvasScrollPosition = GUI.BeginScrollView(new Rect(0, 0, position.width, position.height), canvasScrollPosition, new Rect (0, 0, canvasScrollView.x, canvasScrollView.y), false, false);
				DrawCanvasBackground();
				DrawCanvasContents();
			} finally {
				GUI.EndScrollView();
			}

			// Restore previous scrollbar style:
			GUI.skin.verticalScrollbar = verticalScrollbar;
			GUI.skin.horizontalScrollbar = horizontalScrollbar;
		}
		
		private void DrawCanvasBackground() {
			if (UnityEngine.Event.current.type != EventType.Repaint) return;
			DrawCanvasColor();
			DrawGridLines(MinorGridLineWidth, MinorGridLineColor);
			DrawGridLines(MajorGridLineWidth, MajorGridLineColor);
		}

		private void DrawCanvasColor() {
			if (canvasBackgroundColorTexture == null) {
				canvasBackgroundColorTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
				canvasBackgroundColorTexture.SetPixel(0, 0, new Color(0.16f, 0.16f, 0.16f, 0.75f));
				canvasBackgroundColorTexture.Apply();
				canvasBackgroundColorTexture.hideFlags = HideFlags.HideAndDontSave;
			}
			GUI.DrawTexture(new Rect(0, 0, maxSize.x, maxSize.y), canvasBackgroundColorTexture, ScaleMode.StretchToFill);
		}
		
		private void DrawGridLines(float gridSize, Color gridColor) {
			Handles.color = gridColor;
			float maxX = position.width + canvasScrollPosition.x;
			float maxY = position.height + canvasScrollPosition.y;
			for (float x = 0; x < maxX; x += gridSize) {
				Handles.DrawLine(new Vector2(x, 0), new Vector2(x, maxY));
			}
			for (float y = 0; y < maxY; y += gridSize) {
				Handles.DrawLine(new Vector2(0, y), new Vector2(maxX, y));
			}
		}

		public void DrawLink(Vector3 start, Vector3 end, Color color) {
			if (UnityEngine.Event.current.type != EventType.repaint) return;
			Vector3 cross = Vector3.Cross((start - end).normalized, Vector3.forward);
			Texture2D connectionTexture = (Texture2D) UnityEditor.Graphs.Styles.connectionTexture.image;
			Handles.color = color;
			Handles.DrawAAPolyLine(connectionTexture, 4f, new Vector3[] { start, end });
			Vector3 diff = (end - start);
			Vector3 direction = diff.normalized;
			Vector3 mid = ((0.5f * diff) + start) - (0.5f * cross);
			Vector3 center = mid + direction;
			if ((center.y - canvasScrollPosition.y) > 0) {
				DrawArrow(cross, direction, center, color);
			}
		}
		
		private void DrawArrow(Vector3 cross, Vector3 direction, Vector3 center, Color color) {
			const float sideLength = 6f;
			Vector3[] vertices = new Vector3[] {
				center + (direction * sideLength),
				(center - (direction * sideLength)) + (cross * sideLength),
				(center - (direction * sideLength)) - (cross * sideLength)
			};
			UseLinkArrowMaterial();
			GL.Begin(vertices.Length + 1);
			GL.Color (color);
			for (int i = 0; i < vertices.Length; i++) {
				GL.Vertex(vertices[i]);
			}
			GL.End();
		}

		private void UseLinkArrowMaterial() {
			if (linkArrowMaterial == null) {
				linkArrowMaterial = new Material (
					"Shader \"Lines/Colored Blended\" { " +
				    "  SubShader { Pass { "  +
					"    Blend SrcAlpha OneMinusSrcAlpha " +
					"    ZWrite Off Cull Off Fog { Mode Off } " +
					"    BindChannels { Bind \"vertex\", vertex Bind \"color\", color } } } }");
				linkArrowMaterial.hideFlags = HideFlags.HideAndDontSave;
				linkArrowMaterial.shader.hideFlags = HideFlags.HideAndDontSave;
			}
			linkArrowMaterial.SetPass(0);
		}

	}

}