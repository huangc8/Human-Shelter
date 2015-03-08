using UnityEngine;

namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// A static class that provides a simplified interface to the dialogue system's core
	/// functions. This class manages a singleton instance of DialogueSystemController.
	/// </summary>
	public static class DialogueManager {
		
		private static DialogueSystemController instance = null;

		/// <summary>
		/// Gets the instance of DialogueSystemController.
		/// </summary>
		/// <value>
		/// The instance.
		/// </value>
		public static DialogueSystemController Instance {
			get { return FindOrCreateInstance(); }
		}

		/// <summary>
		/// Returns <c>true</c> if the singleton has found or created an instance.
		/// </summary>
		/// <value><c>true</c> if has instance; otherwise, <c>false</c>.</value>
		public static bool HasInstance {
			get { return DialogueManager.instance != null; }
		}
		
		/// <summary>
		/// Gets the database manager.
		/// </summary>
		/// <value>
		/// The database manager.
		/// </value>
		public static DatabaseManager DatabaseManager { 
			get { return Instance.DatabaseManager; } 
		}
		
		/// <summary>
		/// Gets the master database.
		/// </summary>
		/// <value>
		/// The master database.
		/// </value>
		public static DialogueDatabase MasterDatabase {
			get { return Instance.MasterDatabase; }
		}
		
		/// <summary>
		/// Gets the dialogue UI.
		/// </summary>
		/// <value>
		/// The dialogue UI.
		/// </value>
		public static IDialogueUI DialogueUI {
			get { return Instance.DialogueUI; }
			set { Instance.DialogueUI = value; }
		}
		
		/// <summary>
		/// Gets the display settings.
		/// </summary>
		/// <value>
		/// The display settings.
		/// </value>
		public static DisplaySettings DisplaySettings {
			get { return Instance.displaySettings; }
		}
		
		/// <summary>
		/// Indicates whether a conversation is active.
		/// </summary>
		/// <value>
		/// <c>true</c> if a conversation is active; otherwise, <c>false</c>.
		/// </value>
		public static bool IsConversationActive { 
			get { return Instance.IsConversationActive; }
		}

		/// <summary>
		/// The IsDialogueEntryValid delegate (if one is assigned). This is an optional delegate that you
		/// can add to check if a dialogue entry is valid before allowing a conversation to use it.
		/// It should return <c>true</c> if the entry is valid.
		/// </summary>
		public static IsDialogueEntryValidDelegate IsDialogueEntryValid {
			get { return Instance.IsDialogueEntryValid; }
			set { Instance.IsDialogueEntryValid = value; }
		}
		
		/// <summary>
		/// Gets the actor in the current conversation (or <c>null</c> if no conversation is active).
		/// </summary>
		/// <value>
		/// The actor in the current conversation.
		/// </value>
		public static Transform CurrentActor {
			get { return Instance.CurrentActor; }
		}
		
		/// <summary>
		/// Gets the conversant in the current conversation (or <c>null</c> if no conversation is active).
		/// </summary>
		/// <value>
		/// The conversant in the current conversation.
		/// </value>
		public static Transform CurrentConversant {
			get { return Instance.CurrentConversant; }
		}

		/// <summary>
		/// Gets the current conversation state (or <c>null</c> if no conversation is active).
		/// </summary>
		/// <value>The current conversation state.</value>
		public static ConversationState CurrentConversationState {
			get { return Instance.CurrentConversationState; }
		}

		/// <summary>
		/// Gets the title of the last conversation started. If a conversation is active,
		/// this is the title of the active conversation.
		/// </summary>
		/// <value>The title of the last conversation started.</value>
		public static string LastConversationStarted {
			get { return Instance.LastConversationStarted; }
		}
		
		/// <summary>
		/// Gets or sets the debug level.
		/// </summary>
		/// <value>
		/// The debug level.
		/// </value>
		public static DialogueDebug.DebugLevel DebugLevel {
			get { return Instance.debugLevel; }
			set { Instance.debugLevel = value; DialogueDebug.Level = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether exceptions in Lua code will be caught by
		/// the calling method or allowed to rise up to Unity. Defaults to <c>false</c>.
		/// </summary>
		/// <value><c>true</c> if Lua exceptions are allowed; otherwise, <c>false</c>.</value>
		public static bool AllowLuaExceptions {
			get { return Instance.AllowLuaExceptions; }
			set { Instance.AllowLuaExceptions = value; }
		}
		
		/// <summary>
		/// Sets the language to use for localized text.
		/// </summary>
		/// <param name='language'>
		/// Language to use. Specify <c>null</c> or an emtpy string to use the default language.
		/// </param>
		public static void SetLanguage(string language) {
			Instance.SetLanguage(language);
		}		
		
		/// <summary>
		/// Adds a dialogue database to memory. To save memory or reduce load time, you may want to 
		/// break up your dialogue data into multiple smaller databases. You can add or remove 
		/// these databases as needed.
		/// </summary>
		/// <param name='database'>
		/// The database to add.
		/// </param>
		public static void AddDatabase(DialogueDatabase database) {
			Instance.AddDatabase(database);
		}
		
		/// <summary>
		/// Removes a dialogue database from memory. To save memory or reduce load time, you may 
		/// want to break up your dialogue data into multiple smaller databases. You can add or 
		/// remove these databases as needed.
		/// </summary>
		/// <param name='database'>
		/// The database to remove.
		/// </param>
		public static void RemoveDatabase(DialogueDatabase database) {
			Instance.RemoveDatabase(database);
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
		public static void ResetDatabase(DatabaseResetOptions databaseResetOptions) {
			Instance.ResetDatabase(databaseResetOptions);
		}
		
		/// <summary>
		/// Preloads the master database. The Dialogue System delays loading of the dialogue database 
		/// until the data is needed. This avoids potentially long delays during Start(). If you want
		/// to load the database manually (for example to run Lua commands on its contents), call
		/// this method.
		/// </summary>
		public static void PreloadMasterDatabase() {
			Instance.PreloadMasterDatabase();
		}

		/// <summary>
		/// Preloads the dialogue UI. The Dialogue System delays loading of the dialogue UI until the
		/// first time it's needed for a conversation or alert. Since dialogue UIs often contain
		/// textures and other art assets, loading can cause a slight pause. You may want to preload
		/// the UI at a time of your design by using this method.
		/// </summary>
		public static void PreloadDialogueUI() {
			Instance.PreloadDialogueUI();
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
		public static bool ConversationHasValidEntry(string title, Transform actor, Transform conversant) {
			return Instance.ConversationHasValidEntry(title, actor, conversant);
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
		public static bool ConversationHasValidEntry(string title, Transform actor) {
			return Instance.ConversationHasValidEntry(title, actor, null);
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
		public static bool ConversationHasValidEntry(string title) {
			return Instance.ConversationHasValidEntry(title, null, null);
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
		/// StartConversation("Shopkeeper Conversation", player, shopkeeper);
		/// </example>
		public static void StartConversation(string title, Transform actor, Transform conversant) {
			Instance.StartConversation(title, actor, conversant);
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
		public static void StartConversation(string title, Transform actor) {
			Instance.StartConversation(title, actor);
		}
		
		/// <summary>
		/// Starts the conversation with no transforms specified for the actor or conversant.
		/// </summary>
		/// <param name='title'>
		/// The title of the conversation to look up in the master database.
		/// </param>
		public static void StartConversation(string title) {
			Instance.StartConversation(title, null, null);
		}
		
		/// <summary>
		/// Stop the current conversation immediately, and sends an OnConversationEnd message to 
		/// the actor and conversant. Your scripts can listen for OnConversationEnd to do anything
		/// necessary at the end of a conversation, such as resuming other gameplay or re-enabling
		/// player control.
		/// </summary>
		public static void StopConversation() {
			Instance.StopConversation();
		}
		
		/// <summary>
		/// Updates the responses for the current state of the current conversation.
		/// If the response menu entries' conditions have changed while the response menu is
		/// being shown, you can call this method to update the response menu.
		/// </summary>
		public static void UpdateResponses() {
			Instance.UpdateResponses();
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
		public static void Bark(string conversationTitle, Transform speaker, Transform listener, BarkHistory barkHistory) {
			Instance.Bark(conversationTitle, speaker, listener, barkHistory);
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
		public static void Bark(string conversationTitle, Transform speaker, Transform listener) {
			Instance.Bark(conversationTitle, speaker, listener);
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
		public static void Bark(string conversationTitle, Transform speaker) {
			Instance.Bark(conversationTitle, speaker);
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
		public static void Bark(string conversationTitle, Transform speaker, BarkHistory barkHistory) {
			Instance.Bark(conversationTitle, speaker, barkHistory);
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
		public static void ShowAlert(string message, float duration) {
			Instance.ShowAlert(message, duration);
		}
		
		/// <summary>
		/// Shows an alert message using the dialogue UI for the UI's default duration.
		/// </summary>
		/// <param name='message'>
		/// The message to show.
		/// </param>
		public static void ShowAlert(string message) {
			Instance.ShowAlert(message);
		}
		
		/// <summary>
		/// Checks Lua Variable['Alert'] to see if we need to show an alert.
		/// </summary>
		public static void CheckAlerts() {
			Instance.CheckAlerts();
		}
		
		/// <summary>
		/// Gets localized text.
		/// </summary>
		/// <returns>If the specified field exists in the table, returns the field's 
		/// localized text for the current language. Otherwise returns the field itself.</returns>
		/// <param name="s">The field to look up.</param>
		public static string GetLocalizedText(string s) {
			return Instance.GetLocalizedText(s);
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
		public static Sequencer PlaySequence(string sequence, Transform speaker, Transform listener, bool informParticipants, bool destroyWhenDone) {
			return Instance.PlaySequence(sequence, speaker, listener, informParticipants, destroyWhenDone);
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
		public static Sequencer PlaySequence(string sequence, Transform speaker, Transform listener, bool informParticipants) {
			return Instance.PlaySequence(sequence, speaker, listener, informParticipants);
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
		public static Sequencer PlaySequence(string sequence, Transform speaker, Transform listener) {
			return Instance.PlaySequence(sequence, speaker, listener);
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
		public static Sequencer PlaySequence(string sequence) {
			return Instance.PlaySequence(sequence);
		}
	
		/// <summary>
		/// Stops a sequence.
		/// </summary>
		/// <param name='sequencer'>
		/// The sequencer playing the sequence.
		/// </param>
		public static void StopSequence(Sequencer sequencer) {
			Instance.StopSequence(sequencer);
		}

		/// <summary>
		/// Pauses the Dialogue System. Also broadcasts OnDialogueSystemPause to 
		/// the Dialogue Manager and conversation participants. Conversations,
		/// timers, typewriter and fade effects, and the AudioWait() and Voice()
		/// sequencer commands will be paused. Other than this, AudioSource,
		/// Animation, and Animator components will not be paused; it's up to
		/// you to handle them as appropriate for your project.
		/// </summary>
		public static void Pause() {
			Instance.Pause();
		}

		/// <summary>
		/// Unpauses the Dialogue System. Also broadcasts OnDialogueSystemUnpause to 
		/// the Dialogue Manager and conversation participants.
		/// </summary>
		public static void Unpause() {
			Instance.Unpause();
		}
		
		/// <summary>
		/// Sets the dialogue UI.
		/// </summary>
		/// <param name='gameObject'>
		/// Game object containing an implementation of IDialogueUI.
		/// </param>
		public static void UseDialogueUI(GameObject gameObject) {
			Instance.UseDialogueUI(gameObject);
		}
		
		/// <summary>
		/// Sets an actor's portrait. If can be:
		/// - 'default' or <c>null</c> to use the primary portrait defined in the database,
		/// - 'pic=#' to use an alternate portrait defined in the database (numbered from 2), or
		/// - the name of a texture in a Resources folder.
		/// </summary>
		/// <param name="actorName">Actor name.</param>
		/// <param name="portraitName">Portrait name.</param>
		public static void SetPortrait(string actorName, string portraitName) {
			Instance.SetPortrait(actorName, portraitName);
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
		public static void AddLuaObserver(string luaExpression, LuaWatchFrequency frequency, LuaChangedDelegate luaChangedHandler) {
			Instance.AddLuaObserver(luaExpression, frequency, luaChangedHandler);
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
		public static void RemoveLuaObserver(string luaExpression, LuaWatchFrequency frequency, LuaChangedDelegate luaChangedHandler) {
			Instance.RemoveLuaObserver(luaExpression, frequency, luaChangedHandler);
		}
		
		/// <summary>
		/// Removes all Lua expression observers for a specified frequency.
		/// </summary>
		/// <param name='frequency'>
		/// Frequency.
		/// </param>
		public static void RemoveAllObservers(LuaWatchFrequency frequency) {
			Instance.RemoveAllObservers(frequency);
		}
		
		/// <summary>
		/// Removes all Lua expression observers.
		/// </summary>
		public static void RemoveAllObservers() {
			Instance.RemoveAllObservers();
		}
		
		/// <summary>
		/// Registers an asset bundle with the Dialogue System. This allows sequencer
		/// commands to load assets inside it.
		/// </summary>
		/// <param name="bundle">Asset bundle.</param>
		public static void RegisterAssetBundle(AssetBundle bundle) {
			Instance.RegisterAssetBundle(bundle);
		}
		
		/// <summary>
		/// Unregisters an asset bundle from the Dialogue System. Always unregister
		/// asset bundles before freeing them.
		/// </summary>
		/// <param name="bundle">Asset bundle.</param>
		public static void UnregisterAssetBundle(AssetBundle bundle) {
			Instance.UnregisterAssetBundle(bundle);
		}
		
		/// <summary>
		/// Loads a named asset from the registered asset bundles or from Resources.
		/// </summary>
		/// <returns>The asset, or <c>null<c/c> if not found.</returns>
		/// <param name="name">Name of the asset.</param>
		public static UnityEngine.Object LoadAsset(string name) {
			return Instance.LoadAsset(name);
		}
		
		private static DialogueSystemController FindOrCreateInstance() {
			if (instance == null) {
				instance = GameObject.FindObjectOfType(typeof(DialogueSystemController)) as DialogueSystemController;
				if (instance == null) {
					if (DialogueSystemController.applicationIsQuitting) {
						instance = null;
					} else {
						instance = new GameObject("Dialogue Manager").AddComponent<DialogueSystemController>();
					}
				}
			}
			return instance;
		}
		
	}

}
