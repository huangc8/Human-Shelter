using UnityEngine;
using System;

namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// Lua expression changed delegate.
	/// </summary>
	public delegate void LuaChangedDelegate(LuaWatchItem luaWatchItem, Lua.Result newValue);
	
	/// <summary>
	/// Watch item for Lua observers. This allows the observer to be notified when a Lua value changes.
	/// </summary>
	public class LuaWatchItem {
		
		/// <summary>
		/// The lua expression to watch.
		/// </summary>
		public string LuaExpression { get; set; }
		
		/// <summary>
		/// The current value of the expression.
		/// </summary>
		private string currentValue;
		
		/// <summary>
		/// Delegate to call when the expression changes.
		/// </summary>
		private event LuaChangedDelegate LuaChanged;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="PixelCrushers.DialogueSystem.WatchItem"/> class.
		/// </summary>
		/// <param name='luaExpression'>
		/// Lua expression to watch.
		/// </param>
		/// <param name='luaChangedHandler'>
		/// Delegate to call when the expression changes.
		/// </param>
		public LuaWatchItem(string luaExpression, LuaChangedDelegate luaChangedHandler) {
			this.LuaExpression = luaExpression.StartsWith("return ") ? luaExpression : ("return " + luaExpression);
			this.currentValue = Lua.Run(this.LuaExpression).AsString;
			this.LuaChanged = luaChangedHandler;
		}

		/// <summary>
		/// Checks if the watch item matches a specified luaExpression and luaChangedHandler.
		/// </summary>
		/// <param name='luaExpression'>
		/// The lua expression.
		/// </param>
		/// <param name='luaChangedHandler'>
		/// The notification delegate.
		/// </param>
		public bool Matches(string luaExpression, LuaChangedDelegate luaChangedHandler) {
			return (luaChangedHandler == LuaChanged) && string.Equals(luaExpression, this.LuaExpression);
		}

		/// <summary>
		/// Checks the watch item and calls the delegate if the Lua expression changed.
		/// </summary>
		public void Check() {
			Lua.Result result = Lua.Run(LuaExpression);
			string newValue = result.AsString;
			if (!string.Equals(currentValue, newValue)) {
				currentValue = newValue;
				if (LuaChanged != null) LuaChanged(this, result);
			}
		}
		
	}
		
}
