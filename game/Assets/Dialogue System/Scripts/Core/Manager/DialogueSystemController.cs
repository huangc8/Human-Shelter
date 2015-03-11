using UnityEngine;
using System.Collections;

namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// This component ties together the elements of the dialogue system: dialogue database, 
	/// dialogue UI, sequencer, and conversation controller. You will typically add this to a
	/// "manager" type game object in your scene. For simplified script access, you can use
	/// the DialogueManager static class.
	/// </summary>
	[AddComponentMenu("Dialogue System/Miscellaneous/Dialogue System Controller")]
	public class DialogueSystemController : MonoBehaviour {

		// ========================================== MY FUNCTION
		public int getID(){
			return conversationController.getStatID ();
		}
		// ==========================================

		/// <summary>
		/// The initial dialogue database.
		/// </summary>
		public DialogueDatabase initialDatabase = null;
		
		/// <summary>
		/// The display settings to use for the dialogue UI and sequencer.
		/// </summary>
		public DisplaySettings displaySettings = new DisplaySettings();

		/// <summary>
		/// Set <c>true</c> to include sim status for each dialogue entry.
		/// </summary>
		public bool includeSimStatus = false;
		
		/// <summary>
		/// If <c>true</c>, preloads the master database and dialogue UI. Otherwise they're lazy-
		/// loaded only before the first time they're needed.
		/// </summary>
		public bool preloadResources = false;
		
		/// <summary>
		/// If <c>true</c>, Unity will not destroy the game object when loading a new level.
		/// </summary>
		public bool dontDestroyOnLoad = true;
		
		/// <summary>
		/// If <c>true</c>, new DialogueSystemController objects will destroy themselves if one
		/// already exists in the scene. Otherwise, if you reload a level and dontDestroyOnLoad
		/// is true, you'll end up with a second object.
		/// </summary>
		public bool allowOnlyOneInstance = true;

		/// <summary>
		/// The debug level. Information at this level or higher is logged to the console. This can
		/// be helpful when tracing through conversations.
		/// </summary>
		public DialogueDebug.DebugLevel debugLevel = DialogueDebug.DebugLevel.Warning;

		public  IDialogueUI currentDialogueUI = null;
		public  IDialogueUI originalDialogueUI = null;

		private const string DefaultDialogueUIResourceName = "Default Dialogue UI";
		
		private DatabaseManager databaseManager = null;
		private DisplaySettings originalDisplaySettings = null;
		private bool isOverrideUIPrefab = false;
		private  ConversationController conversationController = null;
		private IsDialogueEntryValidDelegate isDialogueEntryValid = null;
		private LuaWatchers luaWatchers = new LuaWatchers();
		private AssetBundleManager assetBundleManager = new AssetBundleManager();
		private bool hasStarted = false;
		private DialogueDebug.DebugLevel lastDebugLevelSet = DialogueDebug.DebugLevel.None;

		public static bool applicationIsQuitting = false;

		/// <summary>
		/// Gets the dialogue database manager.
		/// </summary>
		/// <value>
		/// The database manager.
		/// </value>
		public DatabaseManager DatabaseManager { 
			get { return databaseManager; } 
		}
		
		/// <summary>
		/// Gets the master dialogue database, which contains the initial database and any
		/// additional databases that you have added.
		/// </summary>
		/// <value>
		/// The master database.
		/// </value>
		public DialogueDatabase MasterDatabase {
			get { return databaseManager.MasterDatabase; }
		}
		
		/// <summary>
		/// Gets or sets the dialogue UI, which is an implementation of IDialogueUI. 
		/// </summary>
		/// <value>
		/// The dialogue UI.
		/// </value>
		public IDialogueUI DialogueUI {
			get { return GetDialogueUI(); }
			set { SetDialogueUI(value); }
		}

		/// <summary>
		/// The IsDialogueEntryValid delegate (if one is assigned). This is an optional delegate that you
		/// can add to check if a dialogue entry is valid before allowing a conversation to use it.
		/// </summary>
		public IsDialogueEntryValidDelegate IsDialogueEntryValid {
			get { 
				return isDialogueEntryValid; 
			}
			set { 
				isDialogueEntryValid = value;
				if (conversationController != null) conversationController.IsDialogueEntryValid = value;
			}
		}

		/// <summary>
		/// Indicates whether a conversation is currently active.
		/// </summary>
		/// <value>
		/// <c>true</c> if a conversation is active; otherwise, <c>false</c>.
		/// </value>
		public bool IsConversationActive { 
			get { return (conversationController != null) && conversationController.IsActive; }
		}

		/// <summary>
		/// Gets the current actor if a conversation is active.
		/// </summary>
		/// <value>The current actor.</value>
		public Transform CurrentActor { get; private set; }

		/// <summary>
		/// Gets the current conversant if a conversation is active.
		/// </summary>
		/// <value>The current conversant.</value>
		public Transform CurrentConversant { get; private set; }

		/// <summary>
		/// Gets or sets the current conversation state. Returns <c>null</c> if no conversation is active.
		/// This is set by ConversationController as the conversation moves from state to state.
		/// </summary>
		/// <value>The current conversation state.</value>
		public ConversationState CurrentConversationState { get; set; }

		/// <summary>
		/// Gets the title of the last conversation started. If a conversation is active,
		/// this is the title of the active conversation.
		/// </summary>
		/// <value>The title of the last conversation started.</value>
		public string LastConversationStarted { get; private set; }

		/// <summary>
		/// Indicates whether to allow the Lua environment to pass exceptions up to the
		/// caller. The default is <c>false</c>, which allows Lua to catch exceptions
		/// and just log an error to the console.
		/// </summary>
		/// <value><c>true</c> to allow Lua exceptions; otherwise, <c>false</c>.</value>
		public bool AllowLuaExceptions { get; set; }

		public void OnDestroy() {
			if (dontDestroyOnLoad && allowOnlyOneInstance) applicationIsQuitting = true;
		}
		
		/// <summary>
		/// Initializes the component by setting up the dialogue database and preparing the 
		/// dialogue UI. If the assigned UI is a prefab, an instance is added. If no UI is 
		/// assigned, the default UI is loaded.
		/// </summary>
		public void Awake() {
			if (allowOnlyOneInstance && (GameObject.FindObjectsOfType(typeof(DialogueSystemController)).Length > 1)) {
				DestroyImmediate(gameObject);
			} else {
				bool visuallyMarkOldResponses = ((displaySettings != null) && (displaySettings.inputSettings != null) && (displaySettings.inputSettings.emTagForOldResponses != EmTag.None));
				DialogueLua.includeSimStatus = includeSimStatus || visuallyMarkOldResponses;
				if (dontDestroyOnLoad) DontDestroyOnLoad(this.gameObject);
				AllowLuaExceptions = false;
				DialogueDebug.Level = debugLevel;
				lastDebugLevelSet = debugLevel;
				LastConversationStarted = string.Empty;
				CurrentActor = null;
				CurrentConversant = null;
				CurrentConversationState = null;
				InitializeDatabase();
				InitializeDisplaySettings();
			}
		}
		
		/// <summary>
		/// Start by enforcing only one instance is specified. Then start monitoring alerts.
		/// </summary>
		public void Start() {
			StartCoroutine(MonitorAlerts());
			hasStarted = true;
			if (preloadResources) PreloadResources();
		}
		
		private void InitializeDisplaySettings() {
			if (displaySettings == null) {
				displaySettings = new DisplaySettings();
				displaySettings.cameraSettings = new DisplaySettings.CameraSettings();
				displaySettings.inputSettings = new DisplaySettings.InputSettings();
				displaySettings.inputSettings.cancel = new InputTrigger(KeyCode.Escape);
				displaySettings.inputSettings.qteButtons = new string[2] { "Fire1", "Fire2" };
				displaySettings.subtitleSettings = new DisplaySettings.SubtitleSettings();
				displaySettings.localizationSettings = new DisplaySettings.LocalizationSettings();
			}
			if (displaySettings.localizationSettings.useSystemLanguage) displaySettings.localizationSettings.language = Localization.GetLanguage(Application.systemLanguage);
			Localization.Language = displaySettings.localizationSettings.language;
		}
		
		/// <summary>
		/// Sets the language to use for localized text.
		/// </summary>
		/// <param name='language'>
		/// Language to use. Specify <c>null</c> or an emtpy string to use the default language.
		/// </param>
		public void SetLanguage(string language) {
			displaySettings.localizationSettings.language = language;
			Localization.Language = language;
		}

		private void CheckDebugLevel() {
			if (debugLevel != lastDebugLevelSet) {
				DialogueDebug.Level = debugLevel;
				lastDebugLevelSet = debugLevel;
			}
		}
		
		private void InitializeDatabase() {
			databaseManager = new DatabaseManager(initialDatabase);
			databaseManager.Reset(DatabaseResetOptions.KeepAllLoaded);
			if (DialogueDebug.LogWarnings && (initialDatabase == null)) Debug.LogWarning(string.Format("{0}: No dialogue database is assigned.", new System.Object[] { DialogueDebug.Prefix }));
		}
		
		/// <summary>
		/// Adds a dialogue database to memory. To save memory or reduce load time, you may want to 
		/// break up your dialogue data into multiple smaller databases. You can add or remove 
		/// these databases as needed.
		/// </summary>
		/// <param name='database'>
		/// The database to add.
		/// </param>
		public void AddDatabase(DialogueDatabase database) {
			if (databaseManager != null) databaseManager.Add(database);
		}
		
		/// <summary>
		/// Removes a dialogue database from memory. To save memory or reduce load time, you may 
		/// want to break up your dialogue data into multiple smaller databases. You can add or 
		/// remove these databases as needed.
		/// </summary>
		/// <param name='database'>
		/// The database to remove.
		/// </param>
		public void RemoveDatabase(DialogueDatabase database) {
			if (databaseManager != null) databaseManager.Remove(database);
		}
		
		/// <summary>
		/// Resets the database to a default state.
		/// </summary>
		/// <param name='databaseResetOptions'>
		/// Accepts the following values:
		/// - RevertToDefault: Restores the default database, removing any other databases that 
		/// were added after startup.
		/// - KeepAllLoaded: Keeps all loaded databases in memory, but reverts them to their 
		/// starting values.
		/// </param>
		public void ResetDatabase(DatabaseResetOptions databaseResetOptions) {
			if (databaseManager != null) databaseManager.Reset(databaseResetOptions);
		}
		
		/// <summary>
		/// Preloads the master database. The Dialogue System delays loading of the dialogue database 
		/// until the data is needed. This avoids potentially long delays during Start(). If you want
		/// to load the database manually (for example to run Lua commands on its contents), call
		/// this method.
		/// </summary>
		public void PreloadMasterDatabase() {
			DialogueDatabase db = MasterDatabase;
			if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Loaded master database '{1}'", new System.Object[] { DialogueDebug.Prefix, db.name }));
		}

		/// <summary>
		/// Preloads the dialogue UI. The Dialogue System delays loading of the dialogue UI until the
		/// first time it's needed for a conversation or alert. Since dialogue UIs often contain
		/// textures and other art assets, loading can cause a slight pause. You may want to preload
		/// the UI at a time of your design by using this method.
		/// </summary>
		public void PreloadDialogueUI() {
			IDialogueUI ui = DialogueUI;
			if ((ui == null) && DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Unable to load the dialogue UI.", DialogueDebug.Prefix));
		}

		/// <summary>
		/// Checks whether a conversation has any valid entries linked from the start entry, since it's possible that
		/// the conditions of all entries could be false.
		/// </summary>
		/// <returns>
		/// <c>true</c>, if the conversation has a valid entry, <c>false</c> otherwise.
		/// </returns>
		/// <param name="title">
		/// The title of the conversation to look up in the master database.
		/// </param>
		/// <param name='actor'>
		/// The transform of the actor (primary participant). The sequencer uses this to direct 
		/// camera angles and perform other actions. In PC-NPC conversations, the actor is usually
		/// the PC.
		/// </param>
		/// <param name='conversant'>
		/// The transform of the conversant (the other participant). The sequencer uses this to 
		/// direct camera angles and perform other actions. In PC-NPC conversations, the conversant
		/// is usually the NPC.
		/// </param>
		public bool ConversationHasValidEntry(string title, Transform actor, Transform conversant) {
			if (string.IsNullOrEmpty(title)) return false;
			ConversationModel model = new ConversationModel(databaseManager.MasterDatabase, title, actor, conversant, 
			                                                AllowLuaExceptions, IsDialogueEntryValid);
			return model.HasValidEntry;
		}
		
		/// <summary>
		/// Checks whether a conversation has any valid entries linked from the start entry, since it's possible that
		/// the conditions of all entries could be false.
		/// </summary>
		/// <returns>
		/// <c>true</c>, if the conversation has a valid entry, <c>false</c> otherwise.
		/// </returns>
		/// <param name="title">
		/// The title of the conversation to look up in the master database.
		/// </param>
		/// <param name='actor'>
		/// The transform of the actor (primary participant). The sequencer uses this to direct 
		/// camera angles and perform other actions. In PC-NPC conversations, the actor is usually
		/// the PC.
		/// </param>
		public bool ConversationHasValidEntry(string title, Transform actor) {
			return ConversationHasValidEntry(title, actor, null);
		}
		
		/// <summary>
		/// Checks whether a conversation has any valid entries linked from the start entry, since it's possible that
		/// the conditions of all entries could be false.
		/// </summary>
		/// <returns>
		/// <c>true</c>, if the conversation has a valid entry, <c>false</c> otherwise.
		/// </returns>
		/// <param name="title">
		/// The title of the conversation to look up in the master database.
		/// </param>
		public bool ConversationHasValidEntry(string title) {
			return ConversationHasValidEntry(title, null, null);
		}

		/// <summary>
		/// Preloads the resources used by the Dialogue System to avoid delays caused by lazy loading.
		/// </summary>
		public void PreloadResources() {
			PreloadMasterDatabase();
			PreloadDialogueUI();
			var originalDebugLevel = DialogueDebug.Level;
			try {
				DialogueDebug.Level = DialogueDebug.DebugLevel.None;
				PlaySequence("None()");
				if (databaseManager.MasterDatabase == null || databaseManager.MasterDatabase.conversations == null ||
				    databaseManager.MasterDatabase.conversations.Count < 1) return;
				ConversationModel model = new ConversationModel(databaseManager.MasterDatabase, databaseManager.MasterDatabase.conversations[0].Title, null, null, AllowLuaExceptions, IsDialogueEntryValid);
				ConversationView view = this.gameObject.AddComponent<ConversationView>();
				view.Initialize(DialogueUI, GetNewSequencer(), displaySettings, OnDialogueEntrySpoken);
				view.SetPCPortrait(model.GetPCTexture(), model.GetPCName());
				conversationController = new ConversationController(model, view, displaySettings.inputSettings.alwaysForceResponseMenu, OnEndConversation);
				conversationController.Close();
			} finally {
				DialogueDebug.Level = originalDebugLevel;
			}
		}
		
		/// <summary>
		/// Starts a conversation, which also broadcasts an OnConversationStart message to the 
		/// actor and conversant. Your scripts can listen for OnConversationStart to do anything
		/// necessary at the beginning of a conversation, such as pausing other gameplay or 
		/// temporarily disabling player control. See the Feature Demo scene, which uses the
		/// SetEnabledOnDialogueEvent component to disable player control during conversations.
		/// </summary>
		/// <param name='title'>
		/// The title of the conversation to look up in the master database.
		/// </param>
		/// <param name='actor'>
		/// The transform of the actor (primary participant). The sequencer uses this to direct 
		/// camera angles and perform other actions. In PC-NPC conversations, the actor is usually
		/// the PC.
		/// </param>
		/// <param name='conversant'>
		/// The transform of the conversant (the other participant). The sequencer uses this to 
		/// direct camera angles and perform other actions. In PC-NPC conversations, the conversant
		/// is usually the NPC.
		/// </param>
		/// <example>
		/// Example:
		/// <code>StartConversation("Shopkeeper Conversation", player, shopkeeper);</code>
		/// </example>
		public void StartConversation(string title, Transform actor, Transform conversant) {
			if (!IsConversationActive) {
				if (DialogueUI != null) {
					if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Starting conversation '{1}', actor={2}, conversant={3}.", new System.Object[] { DialogueDebug.Prefix, title, actor, conversant }));
					if ((actor != null) && (actor == conversant)) {
						if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Actor and conversant are the same GameObject.", new System.Object[] { DialogueDebug.Prefix }));
					}
					CheckDebugLevel();
					CurrentActor = actor;
					CurrentConversant = conversant;
					LastConversationStarted = title;
					SetConversationUI(conversant);
					ConversationModel model = new ConversationModel(databaseManager.MasterDatabase, title, actor, conversant, AllowLuaExceptions, IsDialogueEntryValid);
					if (!model.HasValidEntry) return;
					ConversationView view = this.gameObject.AddComponent<ConversationView>();
					view.Initialize(DialogueUI, GetNewSequencer(), displaySettings, OnDialogueEntrySpoken);
					view.SetPCPortrait(model.GetPCTexture(), model.GetPCName());
					conversationController = new ConversationController(model, view, displaySettings.inputSettings.alwaysForceResponseMenu, OnEndConversation);
					gameObject.BroadcastMessage("OnConversationStart", actor, SendMessageOptions.DontRequireReceiver);
				} else {
					if (DialogueDebug.LogErrors) Debug.LogError(string.Format("{0}: No Dialogue UI is assigned. Can't start conversation '{1}'.", new System.Object[] { DialogueDebug.Prefix, title }));
				}
			} else {
					if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: Another conversation is already active. Not starting '{1}'.", new System.Object[] { DialogueDebug.Prefix, title }));
			}
		}
		
		/// <summary>
		/// Starts a conversation, which also broadcasts an OnConversationStart message to the 
		/// actor. Your scripts can listen for OnConversationStart to do anything
		/// necessary at the beginning of a conversation, such as pausing other gameplay or 
		/// temporarily disabling player control. See the Feature Demo scene, which uses the
		/// SetEnabledOnDialogueEvent component to disable player control during conversations.
		/// </summary>
		/// <param name='title'>
		/// The title of the conversation to look up in the master database.
		/// </param>
		/// <param name='actor'>
		/// The transform of the actor (primary participant). The sequencer uses this to direct 
		/// camera angles and perform other actions. In PC-NPC conversations, the actor is usually
		/// the PC.
		/// </param>
		public void StartConversation(string title, Transform actor) {
			StartConversation(title, actor, null);
		}
		
		/// <summary>
		/// Starts the conversation with no transforms specified for the actor or conversant.
		/// </summary>
		/// <param name='title'>
		/// The title of the conversation to look up in the master database.
		/// </param>
		public void StartConversation(string title) {
			StartConversation(title, null, null);
		}
		
		/// <summary>
		/// Stops the current conversation immediately, and sends an OnConversationEnd message to 
		/// the actor and conversant. Your scripts can listen for OnConversationEnd to do anything
		/// necessary at the end of a conversation, such as resuming other gameplay or re-enabling
		/// player control.
		/// </summary>
		public void StopConversation() {
			if (conversationController != null) {
				conversationController.Close();
				conversationController = null;
			}
		}

		/// <summary>
		/// Updates the responses for the current state of the current conversation.
		/// If the response menu entries' conditions have changed while the response menu is
		/// being shown, you can call this method to update the response menu.
		/// </summary>
		public void UpdateResponses() {
			if (conversationController != null) {
				conversationController.UpdateResponses();
			}
		}

		/// <summary>
		/// Sets an actor's portrait. If can be:
		/// - 'default' or <c>null</c> to use the primary portrait defined in the database,
		/// - 'pic=#' to use an alternate portrait defined in the database (numbered from 2), or
		/// - the name of a texture in a Resources folder.
		/// </summary>
		/// <param name="actorName">Actor name.</param>
		/// <param name="portraitName">Portrait name.</param>
		public void SetPortrait(string actorName, string portraitName) {
			Actor actor = MasterDatabase.GetActor(actorName);
			if (actor == null) {
				if (DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: SetPortrait({1}, {2}): actor '{1}' not found.", new System.Object[] { DialogueDebug.Prefix, actorName, portraitName }));
				return;
			}
			Texture2D texture = null;
			if (string.IsNullOrEmpty(portraitName) || string.Equals(portraitName, "default")) {
				DialogueLua.SetActorField(actorName, "Current Portrait", string.Empty);
				texture = actor.portrait;
			} else {
				DialogueLua.SetActorField(actorName, "Current Portrait", portraitName);
				if (portraitName.StartsWith("pic=")) {
					texture = actor.GetPortraitTexture(Tools.StringToInt(portraitName.Substring("pic=".Length)));
				} else {
					texture = DialogueManager.LoadAsset(portraitName) as Texture2D;
				}
			}
			if (DialogueDebug.LogWarnings && (texture == null)) Debug.LogWarning(string.Format("{0}: SetPortrait({1}, {2}): portrait texture not found.", new System.Object[] { DialogueDebug.Prefix, actorName, portraitName }));
			SetActorPortraitTexture(actorName, texture);
		}

		/// <summary>
		/// This method is used internally by SetPortrait() to set the portrait texture
		/// for an actor.
		/// </summary>
		/// <param name="actorName">Actor name.</param>
		/// <param name="portraitTexture">Portrait texture.</param>
		public void SetActorPortraitTexture(string actorName, Texture2D portraitTexture) {
			if (IsConversationActive && (conversationController != null)) {
				conversationController.SetActorPortraitTexture(actorName, portraitTexture);
			}
		}
		
		/// <summary>
		/// Looks for any dialogue UI or display settings overrides on the conversant.
		/// </summary>
		/// <param name='conversant'>
		/// Conversant.
		/// </param>
		private void SetConversationUI(Transform conversant) {
			if (conversant != null) {
				OverrideDialogueUI overrideDialogueUI = conversant.GetComponentInChildren<OverrideDialogueUI>();
				OverrideDisplaySettings overrideDisplaySettings = conversant.GetComponentInChildren<OverrideDisplaySettings>();
				if ((overrideDialogueUI != null) && (overrideDialogueUI.ui != null)) {
					isOverrideUIPrefab = Tools.IsPrefab(overrideDialogueUI.ui);
					originalDialogueUI = DialogueUI;
					displaySettings.dialogueUI = overrideDialogueUI.ui;
					currentDialogueUI = null;
				} else if (overrideDisplaySettings != null) {
					if (overrideDisplaySettings.displaySettings.dialogueUI != null) {
						isOverrideUIPrefab = Tools.IsPrefab(overrideDisplaySettings.displaySettings.dialogueUI);
						originalDialogueUI = DialogueUI;
						currentDialogueUI = null;
					}
					originalDisplaySettings = displaySettings;
					displaySettings = overrideDisplaySettings.displaySettings;
				}
				ValidateCurrentDialogueUI();
			}
		}
		
		private void RestoreOriginalUI() {
			if (originalDisplaySettings != null) displaySettings = originalDisplaySettings;
			if (originalDialogueUI != null) {
				if (isOverrideUIPrefab) {
					MonoBehaviour uiBehaviour = currentDialogueUI as MonoBehaviour;
					if (uiBehaviour != null) Destroy(uiBehaviour.gameObject);
				}
				currentDialogueUI = originalDialogueUI;
				displaySettings.dialogueUI = (originalDialogueUI as MonoBehaviour).gameObject;
			}
			isOverrideUIPrefab = false;
			originalDialogueUI = null;
			originalDisplaySettings = null;
		}
		
		private void OnDialogueEntrySpoken(Subtitle subtitle) {
			luaWatchers.NotifyObservers(LuaWatchFrequency.EveryDialogueEntry);
		}
		
		/// <summary>
		/// Handles the end conversation event.
		/// </summary>
		public void OnEndConversation() {
			if (conversationController != null) {
				Transform actor = ((conversationController.ActorInfo != null) && (conversationController.ActorInfo.transform != null)) ? conversationController.ActorInfo.transform : this.transform;
				gameObject.BroadcastMessage("OnConversationEnd", actor, SendMessageOptions.DontRequireReceiver);
				conversationController = null;
			}

			RestoreOriginalUI();
			luaWatchers.NotifyObservers(LuaWatchFrequency.EndOfConversation);
			CheckAlerts();
			CurrentActor = null;
			CurrentConversant = null;
		}
		
		/// <summary>
		/// Handles the conversation response menu timeout event.
		/// </summary>
		public void OnConversationTimeout() {
			if (IsConversationActive) {
				switch (displaySettings.inputSettings.responseTimeoutAction) {
				case ResponseTimeoutAction.ChooseFirstResponse: 
					StartCoroutine(ChooseFirstResponseAfterOneFrame());
					break;
				case ResponseTimeoutAction.EndConversation:
					StopConversation();
					break;
				}
			}
		}
		
		/// <summary>
		/// Chooses the first response after one frame. We wait one frame in case a customer-defined
		/// OnConversationTimeout handler decides to stop the conversation first.
		/// </summary>
		private IEnumerator ChooseFirstResponseAfterOneFrame() {
			yield return null;
			if (IsConversationActive) conversationController.GotoFirstResponse();
		}
		
		/// <summary>
		/// Causes a character to bark a line at another character. A bark is a line spoken outside
		/// of a full conversation. It uses a simple gameplay bark UI instead of the dialogue UI.
		/// </summary>
		/// <param name='conversationTitle'>
		/// Title of the conversation that contains the bark lines. In this conversation, all 
		/// dialogue entries linked from the first entry are considered bark lines.
		/// </param>
		/// <param name='speaker'>
		/// The character barking the line.
		/// </param>
		/// <param name='listener'>
		/// The character being barked at.
		/// </param>
		/// <param name='barkHistory'>
		/// Bark history used to track the most recent bark, so the bark controller can go through
		/// the bark lines in a specified order.
		/// </param>
		public void Bark(string conversationTitle, Transform speaker, Transform listener, BarkHistory barkHistory) {
			CheckDebugLevel();
			StartCoroutine(BarkController.Bark(conversationTitle, speaker, listener, barkHistory));
		}
		
		/// <summary>
		/// Causes a character to bark a line at another character. A bark is a line spoken outside
		/// of a full conversation. It uses a simple gameplay bark UI instead of the dialogue UI.
		/// Since this form of the Bark() method does not include a BarkHistory, a random bark is
		/// selected from the bark lines.
		/// </summary>
		/// <param name='conversationTitle'>
		/// Title of the conversation that contains the bark lines. In this conversation, all 
		/// dialogue entries linked from the first entry are considered bark lines.
		/// </param>
		/// <param name='speaker'>
		/// The character barking the line.
		/// </param>
		/// <param name='listener'>
		/// The character being barked at.
		/// </param>
		public void Bark(string conversationTitle, Transform speaker, Transform listener) {
			Bark(conversationTitle, speaker, listener, new BarkHistory(BarkOrder.Random));
		}
		
		/// <summary>
		/// Causes a character to bark a line. A bark is a line spoken outside
		/// of a full conversation. It uses a simple gameplay bark UI instead of the dialogue UI.
		/// Since this form of the Bark() method does not include a BarkHistory, a random bark is
		/// selected from the bark lines.
		/// </summary>
		/// <param name='conversationTitle'>
		/// Title of the conversation that contains the bark lines. In this conversation, all 
		/// dialogue entries linked from the first entry are considered bark lines.
		/// </param>
		/// <param name='speaker'>
		/// The character barking the line.
		/// </param>
		public void Bark(string conversationTitle, Transform speaker) {
			Bark(conversationTitle, speaker, null, new BarkHistory(BarkOrder.Random));
		}
		
		/// <summary>
		/// Causes a character to bark a line. A bark is a line spoken outside
		/// of a full conversation. It uses a simple gameplay bark UI instead of the dialogue UI.
		/// </summary>
		/// <param name='conversationTitle'>
		/// Title of the conversation that contains the bark lines. In this conversation, all 
		/// dialogue entries linked from the first entry are considered bark lines.
		/// </param>
		/// <param name='speaker'>
		/// The character barking the line.
		/// </param>
		/// <param name='barkHistory'>
		/// Bark history used to track the most recent bark, so the bark controller can go through
		/// the bark lines in a specified order.
		/// </param>
		public void Bark(string conversationTitle, Transform speaker, BarkHistory barkHistory) {
			Bark(conversationTitle, speaker, null, barkHistory);
		}

		/// <summary>
		/// Shows an alert message using the dialogue UI.
		/// </summary>
		/// <param name='message'>
		/// The message to show.
		/// </param>
		/// <param name='duration'>
		/// The duration in seconds to show the message.
		/// </param>
		public void ShowAlert(string message, float duration) {
			if (DialogueUI != null) {
				DialogueUI.ShowAlert(GetLocalizedText(message), duration);
			}
		}
		
		/// <summary>
		/// Shows an alert message using the dialogue UI for the UI's default duration.
		/// </summary>
		/// <param name='message'>
		/// The message to show.
		/// </param>
		public void ShowAlert(string message) {
			float duration = string.IsNullOrEmpty(message) ? 0 : Mathf.Max(displaySettings.subtitleSettings.minSubtitleSeconds, message.Length / displaySettings.subtitleSettings.subtitleCharsPerSecond);
			ShowAlert(message, duration);
		}
		
		/// <summary>
		/// Checks the value of Lua Variable['Alert']. If set, it shows the alert and clears the
		/// variable.
		/// </summary>
		public void CheckAlerts() {
			if (displaySettings.alertSettings.allowAlertsDuringConversations || !IsConversationActive) {
				string message = Lua.Run("return Variable['Alert']").AsString;
				if (!string.IsNullOrEmpty(message)) {
					Lua.Run("Variable['Alert'] = ''");
					ShowAlert(message);
				}
			}
		}
		
		/// <summary>
		/// Monitors the value of Lua Variable['Alert'] on a regular frequency specified in
		/// displaySettings. If the frequency is zero, it doesn't monitor.
		/// </summary>
		/// <returns>
		/// The alerts.
		/// </returns>
		private IEnumerator MonitorAlerts() {
			if ((displaySettings == null) || (displaySettings.alertSettings == null) || (Tools.ApproximatelyZero(displaySettings.alertSettings.alertCheckFrequency))) yield break;
			while (true) {
				yield return new WaitForSeconds(displaySettings.alertSettings.alertCheckFrequency);
				CheckAlerts();
			}
		}

		/// <summary>
		/// Gets localized text.
		/// </summary>
		/// <returns>If the specified field exists in the table, returns the field's 
		/// localized text for the current language. Otherwise returns the field itself.</returns>
		/// <param name="s">The field to look up.</param>
		public string GetLocalizedText(string s) {
			if (displaySettings.localizationSettings.localizedText == null) {
				return s;
			} else {
				string localizedText = displaySettings.localizationSettings.localizedText[s];
				return string.IsNullOrEmpty(localizedText) ? s : localizedText;
			}
		}
		
		/// <summary>
		/// Starts a sequence. See @ref sequencer.
		/// </summary>
		/// <returns>
		/// The sequencer that is playing the sequence.
		/// </returns>
		/// <param name='sequence'>
		/// The sequence to play.
		/// </param>
		/// <param name='speaker'>
		/// The speaker, for sequence commands that reference the speaker.
		/// </param>
		/// <param name='listener'>
		/// The listener, for sequence commands that reference the listener.
		/// </param>
		/// <param name='informParticipants'>
		/// Specifies whether to send OnSequenceStart and OnSequenceEnd messages to the speaker and
		/// listener. Default is <c>true</c>.
		/// </param>
		/// <param name='destroyWhenDone'>
		/// Specifies whether destroy the sequencer when done playing the sequence. Default is 
		/// <c>true</c>.
		/// </param>
		public Sequencer PlaySequence(string sequence, Transform speaker, Transform listener, bool informParticipants, bool destroyWhenDone) {
			CheckDebugLevel();
			Sequencer sequencer = GetNewSequencer();
			sequencer.Open();
			sequencer.PlaySequence(sequence, speaker, listener, informParticipants, destroyWhenDone);
			return sequencer;
		}
	
		/// <summary>
		/// Starts a sequence. See @ref sequencer.
		/// </summary>
		/// <returns>
		/// The sequencer that is playing the sequence.
		/// </returns>
		/// <param name='sequence'>
		/// The sequence to play.
		/// </param>
		/// <param name='speaker'>
		/// The speaker, for sequence commands that reference the speaker.
		/// </param>
		/// <param name='listener'>
		/// The listener, for sequence commands that reference the listener.
		/// </param>
		/// <param name='informParticipants'>
		/// Specifies whether to send OnSequenceStart and OnSequenceEnd messages to the speaker and
		/// listener. Default is <c>true</c>.
		/// </param>
		public Sequencer PlaySequence(string sequence, Transform speaker, Transform listener, bool informParticipants) {
			return PlaySequence(sequence, speaker, listener, informParticipants, true);
		}
	
		/// <summary>
		/// Starts a sequence, and sends OnSequenceStart/OnSequenceEnd messages to the 
		/// participants. See @ref sequencer.
		/// </summary>
		/// <returns>
		/// The sequencer that is playing the sequence.
		/// </returns>
		/// <param name='sequence'>
		/// The sequence to play.
		/// </param>
		/// <param name='speaker'>
		/// The speaker, for sequence commands that reference the speaker.
		/// </param>
		/// <param name='listener'>
		/// The listener, for sequence commands that reference the listener.
		/// </param>
		public Sequencer PlaySequence(string sequence, Transform speaker, Transform listener) {
			return PlaySequence(sequence, speaker, listener, true);
		}
		
		/// <summary>
		/// Starts a sequence. See @ref sequencer.
		/// </summary>
		/// <returns>
		/// The sequencer that is playing the sequence.
		/// </returns>
		/// <param name='sequence'>
		/// The sequence to play.
		/// </param>
		public Sequencer PlaySequence(string sequence) {
			return PlaySequence(sequence, null, null, false);
		}
		
		/// <summary>
		/// Stops a sequence.
		/// </summary>
		/// <param name='sequencer'>
		/// The sequencer playing the sequence.
		/// </param>
		public void StopSequence(Sequencer sequencer) {
			if (sequencer != null) sequencer.Close();
		}

		/// <summary>
		/// Pauses the Dialogue System. Also broadcasts OnDialogueSystemPause to 
		/// the Dialogue Manager and conversation participants. Conversations,
		/// timers, typewriter and fade effects, and the AudioWait() and Voice()
		/// sequencer commands will be paused. Other than this, AudioSource,
		/// Animation, and Animator components will not be paused; it's up to
		/// you to handle them as appropriate for your project.
		/// </summary>
		public void Pause() {
			DialogueTime.IsPaused = true;
			BroadcastDialogueSystemMessage("OnDialogueSystemPause");
		}

		/// <summary>
		/// Unpauses the Dialogue System. Also broadcasts OnDialogueSystemUnpause to 
		/// the Dialogue Manager and conversation participants.
		/// </summary>
		public void Unpause() {
			DialogueTime.IsPaused = false;
			BroadcastDialogueSystemMessage("OnDialogueSystemUnpause");
		}

		private void BroadcastDialogueSystemMessage(string message) {
			BroadcastMessage(message, SendMessageOptions.DontRequireReceiver);
			if (IsConversationActive) {
				if (CurrentActor != null) CurrentActor.BroadcastMessage(message, SendMessageOptions.DontRequireReceiver);
				if (CurrentConversant != null) CurrentConversant.BroadcastMessage(message, SendMessageOptions.DontRequireReceiver);
			}
		}
		
		/// <summary>
		/// Sets the dialogue UI. (Deprecated; just set DialogueUI now.)
		/// </summary>
		/// <param name='gameObject'>
		/// Game object containing an implementation of IDialogueUI.
		/// </param>
		public void UseDialogueUI(GameObject gameObject) {
			currentDialogueUI = null;
			displaySettings.dialogueUI = gameObject;
			ValidateCurrentDialogueUI();
		}
		
		private IDialogueUI GetDialogueUI() {
			ValidateCurrentDialogueUI();
			return currentDialogueUI;
		}
		
		private void SetDialogueUI(IDialogueUI ui) {
			MonoBehaviour uiBehaviour = ui as MonoBehaviour;
			if (uiBehaviour != null) {
				displaySettings.dialogueUI = uiBehaviour.gameObject;
				currentDialogueUI = null;
				ValidateCurrentDialogueUI();
			}
		}
		
		/// <summary>
		/// If currentDialogueUI is <c>null</c>, gets it from displaySettings.dialogueUI. If the
		/// value in displaySettings.dialogueUI is a prefab, loads the prefab. Then updates
		/// displaySettings.dialogueUI.
		/// </summary>
		private void ValidateCurrentDialogueUI() {
			if (currentDialogueUI == null) {
				GetDialogueUIFromDisplaySettings();
				if (currentDialogueUI == null) {
					currentDialogueUI = LoadDefaultDialogueUI();
				}
				MonoBehaviour uiBehaviour = currentDialogueUI as MonoBehaviour;
				if (uiBehaviour != null) displaySettings.dialogueUI = uiBehaviour.gameObject;
			}
		}
		
		private void GetDialogueUIFromDisplaySettings() {
			if (displaySettings.dialogueUI != null) {
				currentDialogueUI = Tools.IsPrefab(displaySettings.dialogueUI)
					? LoadDialogueUIPrefab(displaySettings.dialogueUI)
					: displaySettings.dialogueUI.GetComponentInChildren(typeof(IDialogueUI)) as IDialogueUI;
				if ((currentDialogueUI == null) && DialogueDebug.LogWarnings) Debug.LogWarning(string.Format("{0}: No Dialogue UI found on '{1}'.", new System.Object[] { DialogueDebug.Prefix, displaySettings.dialogueUI }));
			}
		}
		
		private IDialogueUI LoadDefaultDialogueUI() {
			if (DialogueDebug.LogInfo) Debug.Log(string.Format("{0}: Loading default Dialogue UI '{1}'.", new System.Object[] { DialogueDebug.Prefix, DefaultDialogueUIResourceName }));
			return LoadDialogueUIPrefab(DialogueManager.LoadAsset(DefaultDialogueUIResourceName) as GameObject);
		}
		
		private IDialogueUI LoadDialogueUIPrefab(GameObject prefab) {
			GameObject go = Instantiate(prefab) as GameObject;
			IDialogueUI ui = null;
			if (go != null) {
				go.transform.parent = this.transform;
				ui = go.GetComponentInChildren(typeof(IDialogueUI)) as IDialogueUI;
				if (ui == null) {
					if (DialogueDebug.LogErrors) Debug.LogError(string.Format("{0}: No Dialogue UI component found on {1}.", new System.Object[] { DialogueDebug.Prefix, prefab }));
					Destroy(go);
				}
			}
			return ui;
		}

		private Sequencer GetNewSequencer() {
			Sequencer sequencer = this.gameObject.AddComponent<Sequencer>();
			if (sequencer != null) {
				sequencer.UseCamera(displaySettings.cameraSettings.sequencerCamera, displaySettings.cameraSettings.alternateCameraObject, displaySettings.cameraSettings.cameraAngles);
				sequencer.disableInternalSequencerCommands = displaySettings.cameraSettings.disableInternalSequencerCommands;
			}
			return sequencer;
		}
		
		/// <summary>
		/// Adds a Lua expression observer.
		/// </summary>
		/// <param name='luaExpression'>
		/// Lua expression to watch.
		/// </param>
		/// <param name='frequency'>
		/// Frequency to check the expression.
		/// </param>
		/// <param name='luaChangedHandler'>
		/// Delegate to call when the expression changes. This should be in the form:
		/// <code>void MyDelegate(LuaWatchItem luaWatchItem, Lua.Result newValue) {...}</code>
		/// </param>
		public void AddLuaObserver(string luaExpression, LuaWatchFrequency frequency, LuaChangedDelegate luaChangedHandler) {
			StartCoroutine(AddLuaObserverAfterStart(luaExpression, frequency, luaChangedHandler));
		}
		
		private IEnumerator AddLuaObserverAfterStart(string luaExpression, LuaWatchFrequency frequency, LuaChangedDelegate luaChangedHandler) {
			int MaxFramesToWait = 10;
			int framesWaited = 0;
			while (!hasStarted && (framesWaited < MaxFramesToWait)) {
				framesWaited++;
				yield return null;
			}
			yield return null;
			luaWatchers.AddObserver(luaExpression, frequency, luaChangedHandler);
		}
		
		/// <summary>
		/// Removes a Lua expression observer. To be removed, the expression, frequency, and
		/// notification delegate must all match.
		/// </summary>
		/// <param name='luaExpression'>
		/// Lua expression being watched.
		/// </param>
		/// <param name='frequency'>
		/// Frequency that the expression is being watched.
		/// </param>
		/// <param name='luaChangedHandler'>
		/// Delegate that's called when the expression changes.
		/// </param>
		public void RemoveLuaObserver(string luaExpression, LuaWatchFrequency frequency, LuaChangedDelegate luaChangedHandler) {
			luaWatchers.RemoveObserver(luaExpression, frequency, luaChangedHandler);
		}
		
		/// <summary>
		/// Removes all Lua expression observers for a specified frequency.
		/// </summary>
		/// <param name='frequency'>
		/// Frequency.
		/// </param>
		public void RemoveAllObservers(LuaWatchFrequency frequency) {
			luaWatchers.RemoveAllObservers(frequency);
		}
		
		/// <summary>
		/// Removes all Lua expression observers.
		/// </summary>
		public void RemoveAllObservers() {
			luaWatchers.RemoveAllObservers();
		}
		
		void Update() {
			if (Lua.WasInvoked) {
				luaWatchers.NotifyObservers(LuaWatchFrequency.EveryUpdate);
				Lua.WasInvoked = false;
			}
		}

		/// <summary>
		/// Registers an asset bundle with the Dialogue System. This allows sequencer
		/// commands to load assets inside it.
		/// </summary>
		/// <param name="bundle">Asset bundle.</param>
		public void RegisterAssetBundle(AssetBundle bundle) {
			assetBundleManager.RegisterAssetBundle(bundle);
		}

		/// <summary>
		/// Unregisters an asset bundle from the Dialogue System. Always unregister
		/// asset bundles before freeing them.
		/// </summary>
		/// <param name="bundle">Asset bundle.</param>
		public void UnregisterAssetBundle(AssetBundle bundle) {
			assetBundleManager.UnregisterAssetBundle(bundle);
		}

		/// <summary>
		/// Loads a named asset from the registered asset bundles or from Resources.
		/// </summary>
		/// <returns>The asset, or <c>null<c/c> if not found.</returns>
		/// <param name="name">Name of the asset.</param>
		public UnityEngine.Object LoadAsset(string name) {
			return assetBundleManager.Load(name);
		}

#if EVALUATION_VERSION
		private GUIStyle evaluationWatermarkStyle = null;
		private Rect watermarkRect1;
		private Rect watermarkRect2;
		
		public void OnGUI() {
			if (evaluationWatermarkStyle == null) {
				evaluationWatermarkStyle = new GUIStyle(GUI.skin.label);
				evaluationWatermarkStyle.fontSize = 20;
				evaluationWatermarkStyle.fontStyle = FontStyle.Bold;
				evaluationWatermarkStyle.alignment = TextAnchor.MiddleCenter;
				evaluationWatermarkStyle.normal.textColor = new Color(1, 1, 1, 0.5f);
				Vector2 size = evaluationWatermarkStyle.CalcSize(new GUIContent("Evaluation Version"));
				watermarkRect1 = new Rect(Screen.width - size.x - 20, 0, size.x + 20, size.y);
				watermarkRect2 = new Rect(Screen.width - size.x - 20, size.y - 8, size.x + 20, size.y);
			}
			GUI.Label(watermarkRect1, "Dialogue System", evaluationWatermarkStyle);
			GUI.Label(watermarkRect2, "Evaluation Version", evaluationWatermarkStyle);
		}
#endif		
		
	}

}
