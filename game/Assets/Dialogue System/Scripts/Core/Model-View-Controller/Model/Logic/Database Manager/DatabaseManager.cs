using UnityEngine;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// Manages a master DialogueDatabase and associated Lua environment.
	/// </summary>
	public class DatabaseManager {
		
		/// <summary>
		/// The master database containing all loaded assets.
		/// </summary>
		private DialogueDatabase masterDatabase = null;
		
		/// <summary>
		/// List of all databases that have been added to the master database.
		/// </summary>
		private List<DialogueDatabase> loadedDatabases = new List<DialogueDatabase>();
		
		/// <summary>
		/// Gets or sets the default database.
		/// </summary>
		/// <value>
		/// The default database to use at startup or when resetting the database manager.
		/// </value>
		public DialogueDatabase DefaultDatabase { get; set; }
		
		/// <summary>
		/// Gets the master database.
		/// </summary>
		/// <value>
		/// The master database.
		/// </value>
		public DialogueDatabase MasterDatabase { 
			get { return GetMasterDatabase(); } 
		}
		
		/// <summary>
		/// Initializes a new DatabaseManager. Loading of the default database is delayed until the
		/// first time the database is accessed. If you want to manually load the database, you can
		/// reset it or add a database to it.
		/// </summary>
		/// <param name='defaultDatabase'>
		/// (Optional) The default database.
		/// </param>
		public DatabaseManager(DialogueDatabase defaultDatabase = null) {
			masterDatabase = ScriptableObject.CreateInstance(typeof(DialogueDatabase)) as DialogueDatabase;
			DefaultDatabase = defaultDatabase;
		}
		
		/// <summary>
		/// Gets the master database. If no databases have been loaded, this method loads the 
		/// default database first.
		/// </summary>
		/// <returns>
		/// The master database.
		/// </returns>
		private DialogueDatabase GetMasterDatabase() {
			if (loadedDatabases.Count == 0) Add(DefaultDatabase);
			return masterDatabase;
		}
		
		/// <summary>
		/// Adds a database to the master database, and updates the Lua environment.
		/// </summary>
		/// <param name='database'>
		/// The database to add.
		/// </param>
		public void Add(DialogueDatabase database) {
			if ((database != null) && !loadedDatabases.Contains(database)) {
				if (loadedDatabases.Count == 0) DialogueLua.InitializeChatMapperVariables();
				DialogueLua.AddChatMapperVariables(database, loadedDatabases);
				masterDatabase.Add(database);
				loadedDatabases.Add(database);
			}
		}
			
		/// <summary>
		/// Removes a database from the master database, and updates the Lua environment.
		/// Does not remove any assets that are also defined in other loaded databases.
		/// </summary>
		/// <param name='database'>
		/// The database to remove.
		/// </param>
		public void Remove(DialogueDatabase database) {
			if (database != null) {
				loadedDatabases.Remove(database);
				masterDatabase.Remove(database, loadedDatabases);
				DialogueLua.RemoveChatMapperVariables(database, loadedDatabases);
			}
		}
				
		/// <summary>
		/// Removes all loaded databases from the master database and clears the Lua environment.
		/// </summary>
		public void Clear() {
			DialogueLua.InitializeChatMapperVariables();
			masterDatabase.Clear();
			loadedDatabases.Clear();
		}
		
		/// <summary>
		/// Resets the master database using the specified DatabaseResetOptions.
		/// </summary>
		/// <param name='databaseResetOptions'>
		/// Database reset options.
		/// - DatabaseResetOptions.RevertToDefault: Unloads everything and reloads only the default
		/// database.
		/// - DatabaseResetOptions.KeepAllLoaded: Keeps everything loaded but resets their values.
		/// </param>
		public void Reset(DatabaseResetOptions databaseResetOptions = DatabaseResetOptions.RevertToDefault) {
			switch (databaseResetOptions) {
			case DatabaseResetOptions.RevertToDefault:
				ResetToDefaultDatabase();
				break;
			case DatabaseResetOptions.KeepAllLoaded:
				ResetToLoadedDatabases();
				break;
			}
		}
		
		private void ResetToDefaultDatabase() {
			Clear();
			Add(DefaultDatabase);
		}
		
		private void ResetToLoadedDatabases() {
			List<DialogueDatabase> previousLoadedDatabases = new List<DialogueDatabase>(loadedDatabases);
			Clear();
			Add(DefaultDatabase);
			foreach (DialogueDatabase database in previousLoadedDatabases) {
				Add(database);
			}
		}
		
	}

}
