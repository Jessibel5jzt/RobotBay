using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WestBay
{
	/// <summary>
	/// ini 配置文件读取
	/// </summary>
	/// @ingroup CoreApi
	public class IniFile
	{
		private Dictionary<string, Dictionary<string, string>> Dict;

		public IniFile()
		{ Dict = new Dictionary<string, Dictionary<string, string>>(); }

		public IniFile(IniFile r)
		{ Dict = r.Dict; }

		public List<string> Keys
		{ get { return Dict[""].Keys.ToList(); } }

		private string _contents;

		/// <summary>
		/// 设置内容
		/// </summary>
		/// <param name="contents">需要读取的ini内容</param>
		public void SetBuffer(string contents)
		{
			_contents = contents;
			Dict = new Dictionary<string, Dictionary<string, string>>();
			if (string.IsNullOrEmpty(contents)) return;
			var sectionKey = string.Empty;
			byte[] array = Encoding.UTF8.GetBytes(contents);
			using (MemoryStream stream = new MemoryStream(array))
			{
				StreamReader SR = new StreamReader(stream, Encoding.UTF8);
				do
				{
					string Line = SR.ReadLine();
					if (Line == null) break;

					Line = RemoveComment(Line.Trim());
					if (Line == null) continue;

					if (Line.StartsWith("[") && Line.EndsWith("]"))
					{
						sectionKey = Line.Substring(1, Line.Length - 2);
					}
					else
					{
						if (Line.IndexOf("=") <= 0) continue;

						string[] KeyPair = Line.Split(new char[] { '=' }, 2);
						if (KeyPair.Length <= 1) continue;

						string key = KeyPair[0].Trim();
						if (key == "") continue;

						string value = KeyPair[1].Trim();
						if (value == "") continue;
						if (sectionKey == null)
						{
							Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
							keyValuePairs.Add(key, value);
							Dict.Add("", keyValuePairs);
						}
						else
						{
							if (Dict.ContainsKey(sectionKey))
							{
								Dictionary<string, string> keyValuePairs = Dict[sectionKey];
								keyValuePairs.Add(key, value);
							}
							else
							{
								Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
								keyValuePairs.Add(key, value);
								Dict.Add(sectionKey, keyValuePairs);
							}
						}
					}
				} while (true);
				SR.Close();
			}
		}

		/// <summary>
		/// 设置键值并写入路径
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <param name="path"></param>
		public void SetValue(string key, string value, string path, string section = "")
		{
			if (string.IsNullOrEmpty(key)) return;
			if (string.IsNullOrWhiteSpace(path)) return;

			string contents = SetContents(key, value, section);
			FileHelper.WriteFile(path, contents);
			SetBuffer(contents);
		}

		/// <summary>
		/// 设置键值
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		public void SetValueNotWrite(string key, string value, string section = "")
		{
			if (string.IsNullOrEmpty(key)) return;
			var contents = SetContents(key, value, section);
			SetBuffer(contents);
		}

		/// <summary>
		/// 根据key获取value（无section）
		/// </summary>
		/// <param name="key">键</param>
		/// <returns>值</returns>
		public string GetValue(string key)
		{
			if (string.IsNullOrEmpty(key)) return "";
			return GetValue("", key);
		}

		/// <summary>
		/// 得到section下key对应的value
		/// </summary>
		/// <param name="section"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public string GetValue(string section, string key)
		{
			string result = string.Empty;
			if (Dict.ContainsKey(section))
			{
				Dictionary<string, string> dictionary = Dict[section];
				if (dictionary.ContainsKey(key))
				{
					result = dictionary[key].ToString();
				}
			}

			return result;
		}

		public string GetValueWithDefault(string key, string defaultValue)
		{
			var result = defaultValue;
			if (string.IsNullOrEmpty(key)) return result;
			if (IsExistKey(key)) result = GetValue("", key);

			return result;
		}

		public string GetValueWithDefault(string section, string key, string defaultValue)
		{
			var result = defaultValue;
			if (string.IsNullOrEmpty(key)) return result;
			if (IsExistKey(key)) result = GetValue(section, key);

			return result;
		}

		/// <summary>
		/// 是否存在Key值
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool IsExistKey(string key)
		{
			return IsExistKey("", key);
		}

		/// <summary>
		/// 是否存在Key值
		/// </summary>
		/// <param name="section"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public bool IsExistKey(string section, string key)
		{
			bool result = false;

			if (Dict.ContainsKey(section))
			{
				Dictionary<string, string> dictionary = Dict[section];
				if (dictionary.ContainsKey(key))
				{
					result = true;
				}
			}

			return result;
		}

		/// <summary>
		/// 是否存在Section值
		/// </summary>
		/// <param name="section"></param>
		/// <returns></returns>
		public bool IsExistSection(string section)
		{
			bool result = false;

			if (Dict.ContainsKey(section))
			{
				result = true;
			}

			return result;
		}

		public void WriteToFile(string path)
		{
			FileHelper.WriteFile(path, _contents);
		}

		private static string RemoveComment(string lin)
		{
			if (lin == "") return null;
			var idx = lin.IndexOf(";");
			if (idx < 0) return lin;
			if (idx == 0) return null;
			return lin.Substring(0, lin.Length - idx);
		}

		private string SetContents(string key, string value, string section = "")
		{
			string contents = string.Empty;
			if (!section.Equals("") && Dict.ContainsKey(""))
			{
				foreach (var item in Dict[""])
				{
					contents += $"{item.Key}={item.Value}\n";
				}
			}

			if (Dict.ContainsKey(section))
			{
				Dictionary<string, string> dic = Dict[section];
				if (dic.ContainsKey(key))
				{
					dic[key] = value;
				}
				else
				{
					dic.Add(key, value);
				}

				if (!section.Equals(""))
				{
					contents += $"[{section}]\n";
				}
				foreach (var item in dic)
				{
					contents += $"{item.Key}={item.Value}\n";
				}
			}
			else
			{
				if (!section.Equals(""))
				{
					contents += $"[{section}]\n";
				}
				contents += $"{key}={value}\n";
			}

			foreach (var sectionDic in Dict)
			{
				if (sectionDic.Key.Equals(section) || sectionDic.Key.Equals("")) continue;
				contents += $"[{sectionDic.Key}]\n";
				foreach (var item in sectionDic.Value)
				{
					contents += $"{item.Key}={item.Value}\n";
				}
			}
			return contents;
		}
	}
}