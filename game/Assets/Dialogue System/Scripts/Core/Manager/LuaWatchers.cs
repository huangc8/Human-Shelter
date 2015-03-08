using UnityEngine;
using System;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// Lua watch frequencies.
	/// </summary>
	public enum LuaWatchFrequency {
		/// <summary>
		/// Check every frame that Lua code was run.
		/// </summary>
		EveryUpdate,
		
		/// <summary>
		/// Check every time an actor speaks a DialogueEntry in a conversation.
		/// </summary>
		EveryDialogueEntry,
		
		/// <summary>
		/// Check at the end of conversations.
		/// </summary>
		EndOfConversation
	};
	
	/// <summary>
	/// This class maintains a list of watchers by watch frequency.
	/// </summary>
	public class LuaWatchers {
		
		private LuaWatchList everyUpdateList = new LuaWatchList();
		private LuaWatchList everyDialogueEntryList = new LuaWatchList();
		private LuaWatchList endOfConversationList = new LuaWatchList();
		
		/// <summary>
		/// Adds a Lua observer.
		/// </summary>
		/// <param name='luaExpression'>
		/// Lua expression to observe.
		/// </param>
		/// <param name='frequency'>
		/// Frequency to check.
		/// </param>
		/// <param name='luaChangedHandler'>
		/// Delegate to call when the expression changes.
		/// </param>
		public void AddObserver(string luaExpression, LuaWatchFrequency frequency, LuaChangedDelegate luaChangedHandler) {
			switch (frequency) {
			case LuaWatchFrequency.EveryUpdate:
				everyUpdateList.AddObserver(luaExpression, luaChangedHandler); 
				break;
			case LuaWatchFrequency.EveryDialogueEntry:
				everyDialogueEntryList.AddObserver(luaExpression, luaChangedHandler); 
				break;
			case LuaWatchFrequency.EndOfConversation:
				endOfConversationList.AddObserver(luaExpression, luaChangedHandler); 
				break;
			default:
				Debug.LogError(string.Format("{0}: Internal error - unexpected Lua watch frequency {1}", new System.Object[] { DialogueDebug.Prefix, frequency }));
				break;
			}					
		}
		
		/// <summary>
		/// Removes a Lua observer.
		/// </summary>
		/// <param name='luaExpression'>
		/// Lua expression.
		/// </param>
		/// <param name='frequency'>
		/// Frequency.
		/// </param>
		/// <param name='luaChangedHandler'>
		/// Lua changed handler.
		/// </param>
		public void RemoveObserver(string luaExpression, LuaWatchFrequency frequency, LuaChangedDelegate luaChangedHandler) {
			switch (frequency) {
			case LuaWatchFrequency.EveryUpdate:
				everyUpdateList.RemoveObserver(luaExpression, luaChangedHandler);
				break;
			case LuaWatchFrequency.EveryDialogueEntry:
				everyDialogueEntryList.RemoveObserver(luaExpression, luaChangedHandler);
				break;
			case LuaWatchFrequency.EndOfConversation:
				endOfConversationList.RemoveObserver(luaExpression, luaChangedHandler);
				break;
			default:
				Debug.LogError(string.Format("{0}: Internal error - unexpected Lua watch frequency {1}", new System.Object[] { DialogueDebug.Prefix, frequency }));
				break;
			}					
		}
		
		/// <summary>
		/// Removes all Lua observers for a specified frequency.
		/// </summary>
		/// <param name='frequency'>
		/// Frequency.
		/// </param>
		public void RemoveAllObservers(LuaWatchFrequency frequency) {
			switch (frequency) {
			case LuaWatchFrequency.EveryUpdate:
				everyUpdateList.RemoveAllObservers();
				break;
			case LuaWatchFrequency.EveryDialogueEntry:
				everyDialogueEntryList.RemoveAllObservers();
				break;
			case LuaWatchFrequency.EndOfConversation:
				endOfConversationList.RemoveAllObservers();
				break;
			default:
				Debug.LogError(string.Format("{0}: Internal error - unexpected Lua watch frequency {1}", new System.Object[] { DialogueDebug.Prefix, frequency }));
				break;
			}					
		}
		
		/// <summary>
		/// Removes all Lua observers.
		/// </summary>
		public void RemoveAllObservers() {
			everyUpdateList.RemoveAllObservers();
			everyDialogueEntryList.RemoveAllObservers();
			endOfConversationList.RemoveAllObservers();
		}
		
		/// <summary>
		/// Checks the observers for a specified frequency.
		/// </summary>
		/// <param name='frequency'>
		/// Frequency.
		/// </param>
		public void NotifyObservers(LuaWatchFrequency frequency) {
			switch (frequency) {
			case LuaWatchFrequency.EveryUpdate:
				everyUpdateList.NotifyObservers();
				break;
			case LuaWatchFrequency.EveryDialogueEntry:
				everyDialogueEntryList.NotifyObservers();
				break;
			case LuaWatchFrequency.EndOfConversation:
				endOfConversationList.NotifyObservers();
				break;
			default:
				Debug.LogError(string.Format("{0}: Internal error - unexpected Lua watch frequency {1}", new System.Object[] { DialogueDebug.Prefix, frequency }));
				break;
			}					
		}
		
	}

}
