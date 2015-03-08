using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem {

	/// <summary>
	/// This custom drawer for Condition uses LuaConditionWizard.
	/// </summary>
	[CustomPropertyDrawer(typeof(Condition))]
	public class ConditionPropertyDrawer : PropertyDrawer {

		private SerializedProperty luaConditionsProperty = null;
		private SerializedProperty questConditionsProperty = null;
		private SerializedProperty acceptedTagsProperty = null;
		private SerializedProperty acceptedGameObjectsProperty = null;

		private LuaConditionWizard luaConditionWizard = new LuaConditionWizard(EditorTools.selectedDatabase);
		private string currentLuaWizardContent = string.Empty;
		private float luaConditionWizardHeight = 0;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
			var height = EditorGUIUtility.singleLineHeight;
			if (!property.isExpanded) return height;
			FindProperties(property);
			luaConditionWizardHeight = luaConditionWizard.GetHeight();
			height += luaConditionWizardHeight;
			height += GetArrayHeight(luaConditionsProperty);
			height += GetArrayHeight(questConditionsProperty);
			height += GetArrayHeight(acceptedTagsProperty);
			height += GetArrayHeight(acceptedGameObjectsProperty);
			return height;
		}

		public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
			EditorGUI.BeginProperty (position, label, property);

			var rect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
			property.isExpanded = EditorGUI.Foldout(rect, property.isExpanded, "Condition");

			if (property.isExpanded) {
				position = EditorGUI.PrefixLabel (position, GUIUtility.GetControlID (FocusType.Passive), GUIContent.none);

				var oldIndentLevel = EditorGUI.indentLevel;
				EditorGUI.indentLevel = 1;

				FindProperties(property);

				// Show Lua wizard:
				var x = position.x;
				var y = position.y + EditorGUIUtility.singleLineHeight;
				var width = position.width;
				rect = new Rect(x, y, width, luaConditionWizardHeight);
				luaConditionWizard.database = EditorTools.selectedDatabase;
				if (luaConditionWizard.database != null) {
					if (!luaConditionWizard.IsOpen) {
						luaConditionWizard.OpenWizard(string.Empty);
					}
					currentLuaWizardContent = luaConditionWizard.Draw(rect, new GUIContent("Lua Condition Wizard", "Use to add Lua conditions below"), currentLuaWizardContent);
					if (!luaConditionWizard.IsOpen && !string.IsNullOrEmpty(currentLuaWizardContent)) {
						luaConditionsProperty.arraySize++;
						var luaElement = luaConditionsProperty.GetArrayElementAtIndex(luaConditionsProperty.arraySize - 1);
						luaElement.stringValue = currentLuaWizardContent;
						currentLuaWizardContent = string.Empty;
						luaConditionWizard.OpenWizard(string.Empty);
					}
				}
				y += rect.height;

				// Show regular fields:
				rect = new Rect(x, y, width, GetArrayHeight(luaConditionsProperty));
				EditorGUI.PropertyField(rect, luaConditionsProperty, true);
				y += rect.height;

				rect = new Rect(x, y, width, GetArrayHeight(questConditionsProperty));
				EditorGUI.PropertyField(rect, questConditionsProperty, true);
				y += rect.height;
				
				rect = new Rect(x, y, width, GetArrayHeight(acceptedTagsProperty));
				EditorGUI.PropertyField(rect, acceptedTagsProperty, true);
				y += rect.height;
				
				rect = new Rect(x, y, width, GetArrayHeight(acceptedGameObjectsProperty));
				EditorGUI.PropertyField(rect, acceptedGameObjectsProperty, true);
				y += rect.height;

				EditorGUI.indentLevel = oldIndentLevel;
			}

			EditorGUI.EndProperty ();
		}

		private void FindProperties(SerializedProperty property) {
			luaConditionsProperty = property.FindPropertyRelative("luaConditions");
			questConditionsProperty = property.FindPropertyRelative("questConditions");
			acceptedTagsProperty = property.FindPropertyRelative("acceptedTags");
			acceptedGameObjectsProperty = property.FindPropertyRelative("acceptedGameObjects");
		}

		private float GetArrayHeight(SerializedProperty property) {
			return property.isExpanded
				? ((2 + property.arraySize) * (EditorGUIUtility.singleLineHeight + 2f))
					: EditorGUIUtility.singleLineHeight;		
		}

	}
}
