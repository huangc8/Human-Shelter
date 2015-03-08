using UnityEngine;
using System;
using System.Collections.Generic;

namespace PixelCrushers.DialogueSystem {

	/// <summary>
	/// This class maintains a list of watch items.
	/// </summary>
	public class LuaWatchList {

		private List<LuaWatchItem> watchList = new List<LuaWatchItem>();
		
		/// <summary>
		/// Adds a watch item to the list.
		/// </summary>
		/// <param name='luaExpression'>
		/// Lua expression to watch.
		/// </param>
		/// <param name='luaChangedHandler'>
		/// Delegate to call when the expression changes.
		/// </param>
		public void AddObserver(string luaExpression, LuaChangedDelegate luaChangedHandler) {
			watchList.Add(new LuaWatchItem(luaExpression, luaChangedHandler));
		}
		
		/// <summary>
		/// Removes a watch item from the list.
		/// </summary>
		/// <param name='luaExpression'>
		/// Lua expression.
		/// </param>
		/// <param name='luaChangedHandler'>
		/// Lua changed handler.
		/// </param>
		public void RemoveObserver(string luaExpression, LuaChangedDelegate luaChangedHandler) {
			watchList.RemoveAll(watchItem => watchItem.Matches(luaExpression, luaChangedHandler));
		}
		
		/// <summary>
		/// Removes all watch items from the list.
		/// </summary>
		public void RemoveAllObservers() {
			watchList.Clear();
		}
		
		/// <summary>
		/// Checks all watch items.
		/// </summary>
		public void NotifyObservers() {
			if (watchList.Count > 0) {
				try {
					int count = watchList.Count;
					for (int i = count - 1; i >= 0; i--) {
						watchList[i].Check();
					}
				} catch (System.InvalidOperationException) {
					// We don't care if collection is modified during enumeration in this case.
				}
			}
		}
		
	}
	
}
