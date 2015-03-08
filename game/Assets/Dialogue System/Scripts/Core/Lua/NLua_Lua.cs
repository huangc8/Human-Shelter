#if USE_NLUA
// This version of the Dialogue System's Lua wrapper class works with NLua.
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using LuaInterface = NLua;

namespace PixelCrushers.DialogueSystem {
	
	/// <summary>
	/// A static class that provides a global Lua virtual machine.
	/// </summary>
	public sealed class Lua {
		
		/// <summary>
		/// Stores a Lua interpreter result (LuaValue) and provides easy conversion to basic types.
		/// </summary>
		public struct Result {
			public object[] retVals;
			public LuaTableWrapper luaTableWrapper;
			public Result(object[] retVals) { this.retVals = retVals; luaTableWrapper = null; }
			public bool HasReturnValue { get { return (retVals != null) && (retVals.Length > 0) && (retVals[0] != null); } }
			public string AsString { get { return HasReturnValue ? retVals[0].ToString() : string.Empty; } }
			public bool AsBool { get { return (HasReturnValue && (retVals[0] is bool)) ? (bool) retVals[0] : string.Compare(AsString, "True", StringComparison.OrdinalIgnoreCase) == 0; } }
			public float AsFloat { get { return HasReturnValue ? Tools.StringToFloat(retVals[0].ToString()) : 0; } }
			public int AsInt { get { return HasReturnValue ? Tools.StringToInt(retVals[0].ToString()) : 0; } }
			public bool IsTable { get { return HasReturnValue & (retVals[0] is LuaInterface.LuaTable); } }
			public LuaInterface.LuaTable AsLuaTable { get { return IsTable ? retVals[0] as LuaInterface.LuaTable : null; } }
			public LuaTableWrapper AsTable { 
				get { 
					if (luaTableWrapper == null) luaTableWrapper = new LuaTableWrapper(AsLuaTable); 
					return luaTableWrapper;
				} 
			}
		}
		
		public static readonly Result NoResult = new Result(null);
		
		/// <summary>
		/// Lua.RunRaw sets this Boolean flag whenever it's invoked.
		/// </summary>
		/// <value>
		/// <c>true</c> when Lua.RunRaw is invoked</c>.
		/// </value>
		public static bool WasInvoked { get; set; }
		
		/// <summary>
		/// Provides direct access to the LuaInterface instance.
		/// </summary>
		/// <value>The LuaInterface instance.</value>
		public static LuaInterface.Lua VM { get { return luaVM; } }
		
		/// <summary>
		/// The reference to the global Lua virtual machine.
		/// </summary>
		private static readonly LuaInterface.Lua luaVM = new LuaInterface.Lua();
		
		/// <summary>
		/// Runs the specified luaCode.
		/// </summary>
		/// <returns>
		/// A Result struct from which you can retrieve basic data types from the first return value.
		/// </returns>
		/// <param name='luaCode'>
		/// The Lua code to run. Generally, if you want a return value, this string should start with "return".
		/// </param>
		/// <param name='debug'>
		/// If <c>true</c>, logs the Lua command to the console.
		/// </param>
		/// <param name='allowExceptions'>
		/// If <c>true</c>, exceptions are passed up to the caller. Otherwise they're caught and ignored.
		/// </param>
		/// <example>
		/// float myHeight = Lua.Run("return height").asFloat;
		/// </example>
		public static Result Run(string luaCode, bool debug, bool allowExceptions) {
			return new Result(RunRaw(luaCode, debug, allowExceptions));
		}
		
		/// <summary>
		/// Runs the specified luaCode. Exceptions are ignored.
		/// </summary>
		/// <param name="luaCode">The Lua code to run. Generally, if you want a return value, this string should start with "return".</param>
		/// <param name="debug">If set to <c>true</c>, logs the Lua command to the console.</param>
		public static Result Run(string luaCode, bool debug) {
			return Run(luaCode, debug, false);
		}
		
		/// <summary>
		/// Run the specified luaCode. The code is not logged to the console, and exceptions are ignored.
		/// </summary>
		/// <param name="luaCode">The Lua code to run. Generally, if you want a return value, this string should start with "return".</param>
		public static Result Run(string luaCode) {
			return Run(luaCode, false, false);
		}
		
		/// <summary>
		/// Evaluates a boolean condition defined in Lua code.
		/// </summary>
		/// <returns>
		/// <c>true</c> if luaCode evaluates to true; otherwise, <c>false</c>.
		/// </returns>
		/// <param name='luaCondition'>
		/// The conditional expression to evaluate. Do not include "return" in front.
		/// </param>
		/// <param name='debug'>
		/// If <c>true</c>, logs the Lua command to the console.
		/// </param>
		/// <param name='allowExceptions'>
		/// If <c>true</c>, exceptions are passed up to the caller. Otherwise they're caught and ignored.
		/// </param>
		/// <example>
		/// if (Lua.IsTrue("height > 6")) { ... }
		/// </example>
		public static bool IsTrue(string luaCondition, bool debug, bool allowExceptions) {
			return string.IsNullOrEmpty(luaCondition) ? true : Run("return " + luaCondition, debug, allowExceptions).AsBool;
		}
		
		/// <summary>
		/// Evaluates a boolean condition defined in Lua code. Exceptions are ignored.
		/// </summary>
		/// <returns><c>true</c> if luaCode evaluates to true; otherwise, <c>false</returns>
		/// <param name="luaCondition">The conditional expression to evaluate. Do not include "return" in front.</param>
		/// <param name="debug">If <c>true</c>, logs the Lua command to the console.</param>
		public static bool IsTrue(string luaCondition, bool debug) {
			return IsTrue(luaCondition, debug, false);
		}
		
		/// <summary>
		/// Evaluates a boolean condition defined in Lua code. The code is not logged to the console, and exceptions are ignored.
		/// </summary>
		/// <returns><c>true</c> if luaCode evaluates to true; otherwise, <c>false</returns>
		/// <param name="luaCondition">The conditional expression to evaluate. Do not include "return" in front.</param>
		public static bool IsTrue(string luaCondition) {
			return IsTrue(luaCondition, false, false);
		}
		
		/// <summary>
		/// Runs Lua code and returns an array of return values.
		/// </summary>
		/// <returns>
		/// An array of return values, or <c>null</c> if the code generates an error.
		/// </returns>
		/// <param name='luaCode'>
		/// The Lua code to run. If you want a return value, this string should usually start with
		/// "<c>return</c>".
		/// </param>
		/// <param name='debug'>
		/// If <c>true</c>, logs the Lua command to the console.
		/// </param>
		/// <param name='allowExceptions'>
		/// If <c>true</c>, exceptions are passed up to the caller. Otherwise they're caught and logged but ignored.
		/// </param>
		public static object[] RunRaw(string luaCode, bool debug, bool allowExceptions) {
			try {
				if (string.IsNullOrEmpty(luaCode)) {
					return null;
				} else {
					if (debug) Debug.Log(string.Format("{0}: Lua({1})", new System.Object[] { DialogueDebug.Prefix, luaCode }));
					WasInvoked = true;
					return luaVM.DoString(luaCode);
				}
			} catch (Exception e) {
				Debug.LogError(string.Format("{0}: Lua code '{1}' threw exception '{2}'", new System.Object[] { DialogueDebug.Prefix, luaCode, e.Message }));
				if (allowExceptions) throw e; else return null;
			}
		}
		
		/// <summary>
		/// Runs Lua code and returns an array of return values. Ignores exceptions.
		/// </summary>
		/// <returns>
		/// An array of return values, or <c>null</c> if the code generates an error.
		/// </returns>
		/// <param name='luaCode'>
		/// The Lua code to run. If you want a return value, this string should usually start with
		/// "<c>return</c>".
		/// </param>
		/// <param name='debug'>
		/// If <c>true</c>, logs the Lua command to the console.
		/// </param>
		public static object[] RunRaw(string luaCode, bool debug) {
			return RunRaw(luaCode, debug, false);
		}
		
		/// <summary>
		/// Runs Lua code and returns an array of return values. Does not log to console and ignores exceptions.
		/// </summary>
		/// <returns>
		/// An array of return values, or <c>null</c> if the code generates an error.
		/// </returns>
		/// <param name='luaCode'>
		/// The Lua code to run. If you want a return value, this string should usually start with
		/// "<c>return</c>".
		/// </param>
		public static object[] RunRaw(string luaCode) {
			return RunRaw(luaCode, false, false);
		}
		
		/// <summary>
		/// Registers a C# function with the Lua interpreter so it can be used in Lua.
		/// </summary>
		/// <param name='path'>
		/// The name of the function in Lua.
		/// </param>
		/// <param name='target'>
		/// Target object containing the registered method.
		/// </param>
		/// <param name='function'>
		/// The method that will be called from Lua.
		/// </param>
		public static void RegisterFunction(string path, object target, MethodInfo function) {
			luaVM.RegisterFunction(path, target, function);
		}
		
	}
	
}
#endif
