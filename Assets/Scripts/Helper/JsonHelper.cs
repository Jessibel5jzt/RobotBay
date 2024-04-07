using System;
using System.Collections;
using System.ComponentModel;
using LitJson;

namespace WestBay
{
	public static class JsonHelper
	{
		public static string ToJson(object obj)
		{
			return JsonMapper.ToJson(obj);
		}

		public static T FromJson<T>(string str)
		{
			T t = JsonMapper.ToObject<T>(str);
			if (!(t is ISupportInitialize iSupportInitialize))
			{
				return t;
			}
			iSupportInitialize.EndInit();
			return t;
		}

		public static object FromJson(Type type, string str)
		{
			object t = JsonMapper.ToObject(str, type);
			if (!(t is ISupportInitialize iSupportInitialize))
			{
				return t;
			}
			iSupportInitialize.EndInit();
			return t;
		}

		public static T Clone<T>(T t)
		{
			return FromJson<T>(ToJson(t));
		}

		/// <summary>
		/// 从JsonData获取指定Key的值
		/// </summary>
		/// <param name="jsonData"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static string ReadFromJson(JsonData jsonData, string key, string defaultValue = "")
		{
			string result = defaultValue;

			if (Contains(jsonData, key) && jsonData[key] != null)
			{
				result = jsonData[key].ToString();
			}

			return result;
		}

		/// <summary>
		/// 判断JsonData是否有指定的Key值
		/// </summary>
		/// <param name="jsonData"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static bool Contains(JsonData jsonData, string key)
		{
			if (jsonData == null || string.IsNullOrEmpty(key)) return false;

			return ((IDictionary)jsonData).Contains(key);
		}
	}
}