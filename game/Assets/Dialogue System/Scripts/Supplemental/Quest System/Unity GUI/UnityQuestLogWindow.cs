using UnityEngine;

namespace PixelCrushers.DialogueSystem.UnityGUI {
	
	/// <summary>
	/// <b>This class is deprecated. Use UnityGUIQuestLogWindow instead.</b>
	/// 
	/// A quest log window system implemented with Unity GUI. When open, the window displays active
	/// and completed quests. It gets the titles, descriptions, and states of the quests from the 
	/// QuestLog class. You can use this window as-is or as a template to write your own quest
	/// log window in Unity GUI, NGUI, or whatever system you choose. As-is, this component is 
	/// already quite flexible because you can assign any GUI layout to its properties, allowing
	/// you to completely change its look and feel.
	/// 
	/// Quest descriptions may contain formatted text (e.g., tags such as <c>[lua(code)]</c>),
	/// although response menu-related tags are discarded, and emphasis tags are ignored in order
	/// to give the display a consistent look.
	/// </summary>
	/// <remarks>
	/// If pauseWhileOpen is set to <c>true</c>, the quest log window pauses the game by setting 
	/// <c>Time.timeScale</c> to <c>0</c>. When closed, it restores the previous time scale.
	/// </remarks>
	public class UnityQuestLogWindow : MonoBehaviour {

		/// <summary>
		/// The GUI root.
		/// </summary>
		public GUIRoot guiRoot;
		
		/// <summary>
		/// The scroll view where quest titles and descriptions will be shown.
		/// </summary>
		public GUIScrollView scrollView;
		
		/// <summary>
		/// The button to show active quests. When clicked, it should send the message
		/// "OnShowActiveQuests" to the UnityQuestLogWindow.
		/// </summary>
		public GUIButton activeButton;
		
		/// <summary>
		/// The button to show completed quests. When clicked, it should send the message
		/// "OnShowCompletedQuests" to the UnityQuestLogWindow.
		/// </summary>
		public GUIButton completedButton;
		
		/// <summary>
		/// The name of the GUI style to use for quest titles. If blank, defaults to button.
		/// </summary>
		public string questHeadingGuiStyleName;
		
		/// <summary>
		/// The name of the GUI style to use for quest descriptions. If blank, defaults to label.
		/// </summary>
		public string questBodyGuiStyleName;

		/// <summary>
		/// The name of the GUI style to use for active quest entries. If blank, defaults to label.
		/// </summary>
		public string questEntryActiveGuiStyleName;

		/// <summary>
		/// The name of the GUI style to use when displaying successfully-completed entries. 
		/// If blank, it defaults to label.
		/// </summary>
		public string questEntrySuccessGuiStyleName;

		/// <summary>
		/// The name of the GUI style to use when displaying failed entries. 
		/// If blank, it defaults to label.
		/// </summary>
		public string questEntryFailureGuiStyleName;
		
		/// <summary>
		/// The name of the GUI style to use for the "No Active Quests" and "No Completed Quests"
		/// messages. If blank, defaults to label.
		/// </summary>
		public string noQuestsGuiStyleName;
		
		/// <summary>
		/// The message to show in the scrollView when there are no active quests.
		/// </summary>
		public string noActiveQuestsMessage = "No Active Quests";
		
		/// <summary>
		/// The message to show in the ScrollView when there are no completed quests.
		/// </summary>
		public string noCompletedQuestsMessage = "No Completed Quests";

		public enum QuestHeadingSource {
			/// <summary>
			/// Use the name of the item for the quest heading.
			/// </summary>
			Name,

			/// <summary>
			/// Use the item's Description field for the quest heading.
			/// </summary>
			Description
		};

		/// <summary>
		/// The quest title source.
		/// </summary>
		public QuestHeadingSource questHeadingSource = QuestHeadingSource.Name;
		
		/// <summary>
		/// If <c>true</c>, the window sets <c>Time.timeScale = 0</c> to pause the game while 
		/// displaying the quest log window.
		/// </summary>
		public bool pauseWhileOpen = true;
		
		/// <summary>
		/// Indicates whether the quest log window is currently open.
		/// </summary>
		/// <value>
		/// <c>true</c> if open; otherwise, <c>false</c>.
		/// </value>
		public bool IsOpen { get; private set; }
		
		/// <summary>
		/// The previous time scale prior to opening the window.
		/// </summary>
		private float previousTimeScale = 1;
		
		/// <summary>
		/// The bitmask of quest states to show. This will change as the player toggles between
		/// Active and Completed quests.
		/// </summary>
		private QuestState questStateToShow = QuestState.Active;
		
		/// <summary>
		/// The title of the currently-selected quest. The window expands this quest to show the 
		/// full description.
		/// If <c>null</c>, no quest is open.
		/// </summary>
		private string openQuest = null;

		private string[] quests = null;
		private GUIStyle questHeadingStyle = null;
		private GUIStyle questBodyStyle = null;
		private GUIStyle questEntryActiveStyle = null;
		private GUIStyle questEntrySuccessStyle = null;
		private GUIStyle questEntryFailureStyle = null;
		private string openQuestDescription = null;
		
		private const int padding = 2;
		
		/// <summary>
		/// Awakes this instance. Attempts to find the GUI root and scroll view if they weren't 
		/// set.
		/// </summary>
		public void Awake() {
			IsOpen = false;
			if (guiRoot == null) guiRoot = GetComponentInChildren<GUIRoot>();
			if (scrollView == null) scrollView = GetComponentInChildren<GUIScrollView>();
			if (scrollView != null) {
				scrollView.MeasureContentHandler += OnMeasureContent;
				scrollView.DrawContentHandler += OnDrawContent;
			}
		}
		
		/// <summary>
		/// Start this instance by hiding the GUI root. We only need to activate it when the window
		/// is open.
		/// </summary>
		public void Start() {
			guiRoot.gameObject.SetActive(false);
		}
		
		/// <summary>
		/// Indicates that the window should close. This should be called by a close button, which 
		/// should send the message "OnClose" to the UnityQuestLogWindow.
		/// </summary>
		/// <param name='data'>
		/// Data sent from the close button. This data is ignored.
		/// </param>
		public void OnClose(object data) {
			Close();
		}
		
		/// <summary>
		/// Indicates that the window should show active quests. This should be called by an
		/// "Active Quests" button, which should send the message "OnShowActiveQuests" to the
		/// UnityQuestLogWindow.
		/// </summary>
		/// <param name='data'>
		/// Data (ignored).
		/// </param>
		public void OnShowActiveQuests(object data) {
			ShowQuests(QuestState.Active);
		}
		
		/// <summary>
		/// Indicates that the window should show completed quests. This should be called by an
		/// "Completed Quests" button, which should send the message "OnShowCompletedQuests" to the
		/// UnityQuestLogWindow.
		/// </summary>
		/// <param name='data'>
		/// Data (ignored).
		/// </param>
		public void OnShowCompletedQuests(object data) {
			ShowQuests(QuestState.Success | QuestState.Failure);
		}
		
		/// <summary>
		/// The event handler that measures the size of the content that will go into the scroll
		/// view.
		/// </summary>
		public void OnMeasureContent() {
			MeasureQuestContent();
		}
		
		/// <summary>
		/// The event handler that draws the content of the scroll view.
		/// </summary>
		public void OnDrawContent() {
			DrawQuests();
		}
		
		/// <summary>
		/// Opens the quest log window.
		/// </summary>
		/// <example>
		/// if (Input.GetKeyDown(KeyCode.L) && !myQuestLogWindow.IsOpen) {
		///     myQuestLogWindow.Open();
		/// }
		/// </example>
		public void Open() {
			IsOpen = true;
			PauseGameplay();
			if (guiRoot != null) {
				guiRoot.gameObject.SetActive(true);
				guiRoot.ManualRefresh();
			}
			ShowQuests(QuestState.Active);
		}
		
		/// <summary>
		/// Closes the quest log window. While you can call this manually in your own script, this
		/// method is normally called internally when the player clicks the close button. You can 
		/// call it manually to support alternate methods of closing the window.
		/// </summary>
		/// <example>
		/// if (Input.GetKeyDown(KeyCode.L) && myQuestLogWindow.IsOpen) {
		///     myQuestLogWindow.Close();
		/// }
		/// </example>
		public void Close() {
			IsOpen = false;
			if (guiRoot != null) guiRoot.gameObject.SetActive(false);
			ResumeGameplay();
		}
		
		private void PauseGameplay() {
			if (pauseWhileOpen) {
				previousTimeScale = Time.timeScale;
				Time.timeScale = 0;
			}
		}
		
		private void ResumeGameplay() {
			Time.timeScale = (previousTimeScale > 0) ? previousTimeScale : 1;
		}
		
		private void ShowQuests(QuestState state) {
			questStateToShow = state;
			if (activeButton != null) activeButton.clickable = (state != QuestState.Active);
			if (completedButton != null) completedButton.clickable = (state == QuestState.Active);
			quests = QuestLog.GetAllQuests(questStateToShow);
			openQuest = null;
			if (scrollView != null) scrollView.ResetScrollPosition();
		}
		
		private void MeasureQuestContent() {
			float contentHeight = padding;
			if (quests != null) {
				foreach (string quest in quests) {
					contentHeight += QuestHeadingHeight(quest);
					if (string.Equals(quest, openQuest)) {
						contentHeight += QuestDescriptionHeight(quest);
						contentHeight += QuestEntriesHeight(quest);
					}
				}
			}
			contentHeight += padding;
			if (scrollView != null) scrollView.contentHeight = contentHeight;
		}

		private GUIStyle UseGUIStyle(GUIStyle guiStyle, string guiStyleName, GUIStyle defaultStyle) {
			return (guiStyle != null) ? guiStyle : UnityGUITools.GetGUIStyle(guiStyleName, defaultStyle);
		}
		
		private string QuestHeading(string quest) {
			return (questHeadingSource == QuestHeadingSource.Name) ? quest : QuestLog.GetQuestDescription(quest);
		}
		
		/// <summary>
		/// Computes the height of a quest heading, which is the height of the text or the height 
		/// of the active button, whichever is greater. This way headings will look similar to 
		/// the active/completed buttons (and will be tall enough if the skin has large buttons).
		/// </summary>
		/// <returns>
		/// The heading height.
		/// </returns>
		/// <param name='heading'>
		/// Heading.
		/// </param>
		private float QuestHeadingHeight(string quest) {
			questHeadingStyle = UseGUIStyle(questHeadingStyle, questHeadingGuiStyleName, GUI.skin.button);
			return Mathf.Max(activeButton.rect.height, questHeadingStyle.CalcHeight(new GUIContent(QuestHeading(quest)), scrollView.contentWidth - (2 * padding)));
		}

		private float QuestDescriptionHeight(string quest) {
			questBodyStyle = UseGUIStyle(questBodyStyle, questBodyGuiStyleName, GUI.skin.label);
			if (questHeadingSource == QuestHeadingSource.Name) {
				openQuestDescription = FormattedText.Parse(QuestLog.GetQuestDescription(quest), DialogueManager.MasterDatabase.emphasisSettings).text;
				return questBodyStyle.CalcHeight(new GUIContent(openQuestDescription), scrollView.contentWidth - (2 * padding));
			} else {
				return 0;
			}
		}

		private float QuestEntriesHeight(string quest) {
			float height = 0;
			int entryCount = QuestLog.GetQuestEntryCount(quest);
			for (int i = 1; i <= entryCount; i++) {
				QuestState entryState = QuestLog.GetQuestEntryState(quest, i);
				GUIStyle questEntryStyle = GetQuestEntryStyle(entryState);
				if (entryState != QuestState.Unassigned) {
					string entryDescription = QuestLog.GetQuestEntry(quest, i);
					height += questEntryStyle.CalcHeight(new GUIContent(entryDescription), scrollView.contentWidth - (2 * padding));
				}
			}
			return height;
		}

		private GUIStyle GetQuestEntryStyle(QuestState entryState) {
			questEntryActiveStyle = UseGUIStyle(questEntryActiveStyle, questEntryActiveGuiStyleName, GUI.skin.label);
			questEntrySuccessStyle = UseGUIStyle(questEntrySuccessStyle, questEntrySuccessGuiStyleName, GUI.skin.label);
			questEntryFailureStyle = UseGUIStyle(questEntryFailureStyle, questEntryFailureGuiStyleName, GUI.skin.label);
			switch (entryState) {
			case QuestState.Success: return questEntrySuccessStyle;
			case QuestState.Failure: return questEntryFailureStyle;
			default: return questEntryActiveStyle;
			}
		}
		
		private void DrawQuests() {
			if ((quests != null) && (scrollView != null)) {
				float contentY = padding;
				foreach (string quest in quests) {
					float headingHeight = QuestHeadingHeight(quest);
					if (GUI.Button(new Rect(padding, contentY, scrollView.contentWidth - (2 * padding), headingHeight), QuestHeading(quest), questHeadingStyle)) {
						openQuest = string.Equals(quest, openQuest) ? null : quest;
					}
					contentY += headingHeight;
					if (string.Equals(quest, openQuest)) {
						contentY = DrawQuestDescription(quest, contentY);
						contentY = DrawQuestEntries(quest, contentY);
					}
				}
				if (quests.Length == 0) {
					string description = (questStateToShow == QuestState.Active) 
						? noActiveQuestsMessage 
						: noCompletedQuestsMessage;
					GUIStyle noQuestsStyle = UnityGUITools.GetGUIStyle(noQuestsGuiStyleName, GUI.skin.label);
					float descriptionHeight = noQuestsStyle.CalcHeight(new GUIContent(description), scrollView.contentWidth - 4);
					GUI.Label(new Rect(2, contentY, scrollView.contentWidth, descriptionHeight), description, noQuestsStyle);
					contentY += descriptionHeight;
				}
			}
		}

		private float DrawQuestDescription(string quest, float contentY) {
			if (questHeadingSource == QuestHeadingSource.Name) {
				questBodyStyle = UseGUIStyle(questBodyStyle, questBodyGuiStyleName, GUI.skin.label);
				float descriptionHeight = questBodyStyle.CalcHeight(new GUIContent(openQuestDescription), scrollView.contentWidth - (2 * padding));
				GUI.Label(new Rect(padding, contentY, scrollView.contentWidth, descriptionHeight), openQuestDescription, questBodyStyle);
				return contentY + descriptionHeight;
			} else {
				return contentY;
			}
		}
		
		private float DrawQuestEntries(string quest, float contentY) {
			float currentY = contentY;
			int entryCount = QuestLog.GetQuestEntryCount(quest);
			for (int i = 1; i <= entryCount; i++) {
				QuestState entryState = QuestLog.GetQuestEntryState(quest, i);
				if (entryState != QuestState.Unassigned) {
					string entryDescription = QuestLog.GetQuestEntry(quest, i);
					GUIStyle questEntryStyle = GetQuestEntryStyle(entryState);
					float entryHeight = questEntryStyle.CalcHeight(new GUIContent(entryDescription), scrollView.contentWidth - (2 * padding));
					GUI.Label(new Rect(padding, currentY, scrollView.contentWidth, entryHeight), entryDescription, questEntryStyle);
					currentY += entryHeight;
				}
			}
			return currentY;
		}
		
	}

}
