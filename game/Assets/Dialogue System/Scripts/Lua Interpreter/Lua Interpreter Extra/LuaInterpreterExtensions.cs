using System;
using System.Collections.Generic;

// [PixelCrushers] Adds extra methods to make LuaInterpreter work with Unity.
namespace Language.Lua {

	public static class LuaInterpreterExtensions {

		/// <summary>
		/// This method replaces List.ConvertAll(), which isn't supported in WinRT.
		/// </summary>
		/// <returns>The list of evaluated expressions.</returns>
		/// <param name="exprList">A list of expressions to evaluate.</param>
		/// <param name="environment">Lua environment.</param>
		public static List<LuaValue> EvaluateAll(List<Expr> exprList, LuaTable environment) {
			List<LuaValue> values = new List<LuaValue>();
			foreach (var expr in exprList) {
				values.Add(expr.Evaluate(environment));
			}
			return values;
		}

		/// <summary>
		/// Returns a LuaValue containing the object's value.
		/// </summary>
		/// <returns>A LuaValue.</returns>
		/// <param name="o">An object of any standard type.</param>
		public static LuaValue ObjectToLuaValue(object o) {
			if (o == null) return LuaNil.Nil;
			System.Type objectType = o.GetType();
			if (objectType == typeof(bool)) return ((bool) o) ? LuaBoolean.True : LuaBoolean.False;
			if (objectType == typeof(int)) return new LuaNumber((double)((int) o));
			if (objectType == typeof(float)) return new LuaNumber((double)((float) o));
			if (objectType == typeof(double)) return new LuaNumber((double) o);
			if (objectType == typeof(string)) return new LuaString((string) o);
			if (o is LuaValue) return (o as LuaValue);
			return new LuaString(o.ToString());
		}

		public static object LuaValueToObject(LuaValue luaValue) {
			if (luaValue == null) return null;
			if (luaValue is LuaNumber) return (float) ((luaValue as LuaNumber).Number);
			return luaValue.Value;
		}

	}
	
}
