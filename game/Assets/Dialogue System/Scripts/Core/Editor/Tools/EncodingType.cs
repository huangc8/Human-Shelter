using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

namespace PixelCrushers.DialogueSystem {

	public enum EncodingType {
		Default,
		ASCII,
		Unicode,
		UTF7,
		UTF8,
		UTF32
	}

	public static class EncodingTypeTools {

		public static Encoding GetEncoding(EncodingType encodingType) {
			switch (encodingType) {
			case EncodingType.ASCII: return Encoding.ASCII;
			case EncodingType.Unicode: return Encoding.Unicode;
			case EncodingType.UTF32: return Encoding.UTF32;
			case EncodingType.UTF7: return Encoding.UTF7;
			case EncodingType.UTF8: return Encoding.UTF8;
			default: return Encoding.Default;
			}
		}
		
	}
	
}