using UnityEngine;
using System;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// This is the abstract base class for quest log windows. You can implement a quest log
	/// window in any GUI system by creating a subclass.
	/// 
	/// When open, the window displays active and completed quests. It gets the titles, 
	/// descriptions, and states of the quests from the QuestLog class.
	/// 
	/// The window allows the player to abandon quests (if the quest's Abandonable field is
	/// true) and toggle tracking (if the quest's Trackable field is true).
	/// </summary>
	/// <remarks>
	/// If pauseWhileOpen is set to <c>true</c>, the quest log window pauses the game by setting 
	/// <c>Time.timeScale</c> to <c>0</c>. When closed, it restores the previous time scale.
	/// </remarks>
	public abstract class QuestLogWindow : MonoBehaviour {

		public LocalizedTextTable localizedText = null;

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
		/// The state to assign abandoned quests.
		/// </summary>
		public QuestState abandonQuestState = QuestState.Unassigned;
		
		/// <summary>
		/// If <c>true</c>, the window sets <c>Time.timeScale = 0</c> to pause the game while 
		/// displaying the quest log window.
		/// </summary>
		public bool pauseWhileOpen = true;

		/// <summary>
		/// If <c>true<c/c>, the cursor is unlocked while the quest log window is open.
		/// </summary>
		public bool unlockCursorWhileOpen = true;
		
		/// <summary>
		/// Indicates whether the quest log window is currently open.
		/// </summary>
		/// <value>
		/// <c>true</c> if open; otherwise, <c>false</c>.
		/// </value>
		public bool IsOpen { get; private set; }

		[Serializable]
		public class QuestInfo {
			public string Title { get; set; }
			public FormattedText Heading { get; set; }
			public FormattedText Description { get; set; }
			public FormattedText[] Entries { get; set; }
			public QuestState[] EntryStates { get; set; }
			public bool Trackable { get; set; }
			public bool Track { get; set; }
			public bool Abandonable { get; set; }
			public QuestInfo(string title, FormattedText heading, FormattedText description,
			                 FormattedText[] entries, QuestState[] entryStates, bool trackable, 
			                 bool track, bool abandonable) {
				this.Title = title;
				this.Heading = heading;
				this.Description = description;
				this.Entries = entries;
				this.EntryStates = entryStates;
				this.Trackable = trackable;
				this.Track = track;
				this.Abandonable = abandonable;
			}
		}

		/// <summary>
		/// The current list of quests. This will change based on whether the player is
		/// viewing active or completed quests.
		/// </summary>
		/// <value>The quests.</value>
		public QuestInfo[] Quests { get; protected set; }

		/// <summary>
		/// The title of the currently-selected quest.
		/// </summary>
		/// <value>The selected quest.</value>
		public string SelectedQuest { get; protected set; }

		/// <summary>
		/// The message to show if Quests[] is empty.
		/// </summary>
		/// <value>The no quests message.</value>
		public string NoQuestsMessage { get; protected set; }

		/// <summary>
		/// The previous time scale prior to opening the window.
		/// </summary>
		private float previousTimeScale = 1;

		/// <summary>
		/// The current quest state mask.
		/// </summary>
		protected QuestState currentQuestStateMask = QuestState.Active;

		/// <summary>
		/// Indicates whether the window is showing active quests or completed quests.
		/// </summary>
		/// <value><c>true</c> if showing active quests; otherwise, <c>false</c>.</value>
		public bool IsShowingActiveQuests {
			get { return currentQuestStateMask == QuestState.Active; }
		}

		/// <summary>
		/// Opens the window. Your implementation should override this to handle any
		/// window-opening activity, then call openedWindowHandler at the end.
		/// </summary>
		/// <param name="openedWindowHandler">Opened window handler.</param>
		public virtual void OpenWindow(Action openedWindowHandler) {
			openedWindowHandler();
		}

		/// <summary>
		/// Closes the window. Your implementation should override this to handle any
		/// window-closing activity, then call closedWindowHandler at the end.
		/// </summary>
		/// <param name="openedWindowHandler">Closed window handler.</param>
		public virtual void CloseWindow(Action closedWindowHandler) {
			closedWindowHandler();
		}

		/// <summary>
		/// Called when the quest list has been updated -- for example, when switching between
		/// active and completed quests. Your implementation may override this to do processing.
		/// </summary>
		public virtual void OnQuestListUpdated() {}

		/// <summary>
		/// Asks the player to confirm abandonment of a quest. Your implementation should override
		/// this to show a modal dialogue box or something similar. If confirmed, it should call
		/// confirmedAbandonQuestHandler.
		/// </summary>
		/// <param name="title">Title.</param>
		/// <param name="confirmedAbandonQuestHandler">Confirmed abandon quest handler.</param>
		public virtual void ConfirmAbandonQuest(string title, Action confirmedAbandonQuestHandler) {}

		public virtual void Awake() {
			IsOpen = false;
			Quests = new QuestInfo[0];
			SelectedQuest = string.Empty;
			NoQuestsMessage = string.Empty;
		}

		/// <summary>
		/// Opens the quest window.
		/// </summary>
		public void Open() {
			PauseGameplay();
			OpenWindow(OnOpenedWindow);
		}

		private void OnOpenedWindow() {
			IsOpen = true;
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
			SelectedQuest = string.Empty;
			CloseWindow(OnClosedWindow);
		}

		private void OnClosedWindow() {
			IsOpen = false;
			ResumeGameplay();
		}

		private bool wasCursorActive = false;

		private void PauseGameplay() {
			if (pauseWhileOpen) {
				previousTimeScale = Time.timeScale;
				Time.timeScale = 0;
			}
			if (unlockCursorWhileOpen) {
				wasCursorActive = Tools.IsCursorActive();
				Tools.SetCursorActive(true);
			}
		}
		
		private void ResumeGameplay() {
			Time.timeScale = (previousTimeScale > 0) ? previousTimeScale : 1;
			if (unlockCursorWhileOpen && !wasCursorActive) Tools.SetCursorActive(false);
		}

		private void ShowQuests(QuestState questStateMask) {
			currentQuestStateMask = questStateMask;
			NoQuestsMessage = GetNoQuestsMessage(questStateMask);
			List<QuestInfo> questList = new List<QuestInfo>();
			string[] titles = QuestLog.GetAllQuests(questStateMask);
			foreach (var title in titles) {
				FormattedText description = FormattedText.Parse(QuestLog.GetQuestDescription(title), DialogueManager.MasterDatabase.emphasisSettings);
				FormattedText localizedTitle = FormattedText.Parse(QuestLog.GetQuestTitle(title), DialogueManager.MasterDatabase.emphasisSettings);
				FormattedText heading = (questHeadingSource == QuestHeadingSource.Description) ? description : localizedTitle;
				bool abandonable = QuestLog.IsQuestAbandonable(title) && IsShowingActiveQuests;
				bool trackable = QuestLog.IsQuestTrackingAvailable(title) && IsShowingActiveQuests;
				bool track = QuestLog.IsQuestTrackingEnabled(title);
				int entryCount = QuestLog.GetQuestEntryCount(title);
				FormattedText[] entries = new FormattedText[entryCount];
				QuestState[] entryStates = new QuestState[entryCount];
				for (int i = 0; i < entryCount; i++) {
					entries[i] = FormattedText.Parse(QuestLog.GetQuestEntry(title, i+1), DialogueManager.MasterDatabase.emphasisSettings);
					entryStates[i] = QuestLog.GetQuestEntryState(title, i+1);
				}
				questList.Add(new QuestInfo(title, heading, description, entries, entryStates, trackable, track, abandonable));
			}
			Quests = questList.ToArray();
			OnQuestListUpdated();
		}

		private string GetNoQuestsMessage(QuestState questStateMask) {
			return (questStateMask == QuestState.Active) ? GetLocalizedText("No Active Quests") : GetLocalizedText("No Completed Quests");
		}

		/// <summary>
		/// Gets the localized text for a field name.
		/// </summary>
		/// <returns>The localized text.</returns>
		/// <param name="fieldName">Field name.</param>
		public string GetLocalizedText(string fieldName) {
			if ((localizedText != null) && localizedText.ContainsField(fieldName)) {
				return localizedText[fieldName];
			} else {
				return fieldName;
			}
		}

		/// <summary>
		/// Determines whether the specified questInfo is for the currently-selected quest.
		/// </summary>
		/// <returns><c>true</c> if this is the selected quest; otherwise, <c>false</c>.</returns>
		/// <param name="questInfo">Quest info.</param>
		public bool IsSelectedQuest(QuestInfo questInfo) {
			return string.Equals(questInfo.Title, SelectedQuest);
		}

		/// <summary>
		/// Your GUI close button should call this.
		/// </summary>
		/// <param name="data">Ignored.</param>
		public void ClickClose(object data) {
			Close();
		}

		/// <summary>
		/// Your GUI "show active quests" button should call this.
		/// </summary>
		/// <param name="data">Ignored.</param>
		public void ClickShowActiveQuests(object data) {
			ShowQuests(QuestState.Active);
		}

		/// <summary>
		/// Your GUI "show completed quests" button should call this.
		/// </summary>
		/// <param name="data">Ignored.</param>
		public void ClickShowCompletedQuests(object data) {
			ShowQuests(QuestState.Success | QuestState.Failure);
		}

		/// <summary>
		/// Your GUI should call this when the player clicks on a quest to expand
		/// or close it.
		/// </summary>
		/// <param name="data">The quest title.</param>
		public void ClickQuest(object data) {
			if (!IsString(data)) return;
			string clickedQuest = (string) data;
			SelectedQuest = string.Equals(SelectedQuest, clickedQuest) ? string.Empty : clickedQuest;
			OnQuestListUpdated();
		}

		/// <summary>
		/// Your GUI should call this when the player clicks to abandon a quest.
		/// </summary>
		/// <param name="data">Ignored.</param>
		public void ClickAbandonQuest(object data) {
			if (string.IsNullOrEmpty(SelectedQuest)) return;
			ConfirmAbandonQuest(SelectedQuest, OnConfirmAbandonQuest);
		}

		/// <summary>
		/// Your GUI should call this when the player confirms abandonment of a quest.
		/// </summary>
		private void OnConfirmAbandonQuest() {
			QuestLog.SetQuestState(SelectedQuest, abandonQuestState);
			ShowQuests(currentQuestStateMask);
			DialogueManager.Instance.BroadcastMessage("OnQuestTrackingDisabled", SelectedQuest, SendMessageOptions.DontRequireReceiver);
			string sequence = QuestLog.GetQuestAbandonSequence(SelectedQuest);
			if (!string.IsNullOrEmpty(sequence)) DialogueManager.PlaySequence(sequence);
		}

		/// <summary>
		/// Your GUI should call this when the player clicks to toggle quest tracking.
		/// </summary>
		/// <param name="data">Ignored.</param>
		public void ClickTrackQuest(object data) {
			if (string.IsNullOrEmpty(SelectedQuest)) return;
			bool track = !QuestLog.IsQuestTrackingEnabled(SelectedQuest);
			QuestLog.SetQuestTracking(SelectedQuest, track);
			ShowQuests(currentQuestStateMask);
			DialogueManager.Instance.BroadcastMessage(track ? "OnQuestTrackingEnabled" : "OnQuestTrackingDisabled", SelectedQuest, SendMessageOptions.DontRequireReceiver);
		}

		private bool IsString(object data) {
			return (data != null) && (data.GetType() == typeof(string));
		}

		// Parameter-less versions of methods for GUI systems that require them for button hookups:
		public void ClickShowActiveQuestsButton() {
			ClickShowActiveQuests(null);
		}
		
		public void ClickShowCompletedQuestsButton() {
			ClickShowCompletedQuests(null);
		}
		
		public void ClickCloseButton() {
			ClickClose(null);
		}
		
		public void ClickAbandonQuestButton() {
			ClickAbandonQuest(null);
		}
		
		public void ClickTrackQuestButton() {
			ClickTrackQuest(null);
		}

	}

}
