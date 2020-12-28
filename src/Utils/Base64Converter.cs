using System;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FortiConnect.Utils
{
	public static class Base64Converter
	{
		// https://stackoverflow.com/questions/11743160/how-do-i-encode-and-decode-a-base64-string
		public static string ConvertTextUnicodeToBase64(string plainText)
		{
			if (plainText == null) {
				return null;
			}
			byte[] encodedBytes = Encoding.Unicode.GetBytes(plainText);
			return Convert.ToBase64String(encodedBytes);
		}
		
		public static string ConvertBase64ToTextUnicode(string textToDecode)
		{
			if (textToDecode == null) {
				return null;
			}
			byte[] decodedBytes = Convert.FromBase64String(textToDecode);
			return Encoding.Unicode.GetString(decodedBytes);
		}

		public static string ConvertTextUTF8ToBase64(string plainText)
		{
			if (plainText == null) {
				return null;
			}
			byte[] encodedBytes = Encoding.UTF8.GetBytes(plainText);
			return Convert.ToBase64String(encodedBytes);
		}
		
		public static string ConvertBase64ToTextUTF8(string textToDecode)
		{
			if (textToDecode == null) {
				return null;
			}
			byte[] decodedBytes = Convert.FromBase64String(textToDecode);
			return Encoding.UTF8.GetString(decodedBytes);
		}
	}
}
