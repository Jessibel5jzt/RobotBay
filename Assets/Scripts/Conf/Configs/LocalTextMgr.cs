using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WestBay
{
	/// <summary>
	/// 语言翻译Text读取类
	/// </summary>
	public class LocalTextMgr
	{
		private static Dictionary<string, IniFile> _translationDic = new Dictionary<string, IniFile>();

		public static void AddTranslationIni(string moduleName, IniFile file)
		{
			if (!_translationDic.ContainsKey(moduleName))
			{
				_translationDic.Add(moduleName, file);
			}
		}

		public static bool ContainsTranslation(string moduleName)
		{
			return _translationDic.ContainsKey(moduleName);
		}

		public static void ClearAllTranslation()
		{
			_translationDic.Clear();
		}

		public static IniFile GetTranslationIni(string moduleName)
		{
			if (_translationDic.TryGetValue(moduleName, out IniFile file))
			{
				return file;
			}
			return null;
		}

		/// <summary>
		/// 通过Key获取 对应国家的，翻译过的字串
		/// </summary>
		/// <param name="key">字串的key</param>
		/// <returns>对应国家的翻译过的字串</returns>
		public static string GetValue(string key)
		{
			return GetModuleValue(key, GetCurrentModuleName());
		}

		/// <summary>
		/// 根据Key，获得本地化值
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public static string GetModuleValue(string key, string moduleName)
		{
			var result = $"[{key}]";

			var uiText = GetTranslationIni(moduleName);
			if (uiText != null)
			{
				result = uiText.GetValue(key);
				if (string.IsNullOrEmpty(result))
				{
					result = $"[{key}]";
				}
			}

			return result;
		}

		private static string GetCurrentModuleName()
		{
			// 编辑时运行（不是编辑器运行，是编辑的时候，实时显示翻译的结果）
			if (!Application.isPlaying)
			{
				if (SceneManager.GetActiveScene().name.Equals("StartUp"))
				{
					return App.SharedModule;
				}

				var moduleName = SceneManager.GetActiveScene().name.Split('_')[1];
				return moduleName;
			}

			if (Lobby.Ins == null) return string.Empty;

			return Lobby.Ins.GetThisModuleName();
		}

		#region JumpModule

		private const string AllowedJump = "AllowedJump";
		private const string DefaultJump = "DefaultJump";
		private const string ModuleCofig = "config";

		/// <summary>
		/// 获取跳转模块名
		/// </summary>
		/// <param name="args">
		/// 1.module name
		/// 2.jumptype(1:default,2:allowed)
		/// </param>
		/// <returns></returns>
		public static string GetJump(params object[] args)
		{
			string MN;
			if (args == null || args.Length <= 0) return null;
			MN = args[0].ToString();
			if (string.IsNullOrWhiteSpace(MN)) return null;
			var arg2 = string.Empty;
			if (args.Length > 1 && args[1] != null)
			{
				arg2 = args[1].ToString();
			}
			var userCfg = CheckUserModConfig(MN, arg2);
			if (!string.IsNullOrEmpty(userCfg))
			{
				return userCfg;
			}

			if (!string.IsNullOrEmpty(arg2))
			{
				var isAll = Convert.ToInt32(arg2) == 0;
				return GetJumpFromIni(MN, isAll) ?? null;
			}

			return GetJumpFromIni(MN, true) ?? GetJumpFromIni(GetCurrentModuleName());
		}

		private static string CheckUserModConfig(string modName, string arg2 = "")
		{
			var userModuleConfig = JsonHelper.FromJson<ModuleConfigRoot>("");
			foreach (var moduleKV in userModuleConfig.ModuleList)
			{
				if (moduleKV.Name != modName) continue;

				if (string.IsNullOrEmpty(arg2))
				{
					foreach (var keyV in moduleKV.KeyValList)
					{
						if (keyV.key == AllowedJump)
						{
							return keyV.value;
						}
					}
				}
				else
				{
					switch (Convert.ToInt32(arg2))
					{
						case 0:
							foreach (var keyV in moduleKV.KeyValList)
							{
								if (keyV.key == AllowedJump)
								{
									return keyV.value;
								}
							}
							break;

						case 1:
							foreach (var keyV in moduleKV.KeyValList)
							{
								if (keyV.key == DefaultJump)
								{
									return keyV.value;
								}
							}
							break;
					}
				}
			}
			return string.Empty;
		}

		/// <summary>
		/// 获取配置文件跳转信息
		/// </summary>
		/// <param name="moduleName"></param>
		/// <param name="all"></param>
		/// <returns></returns>
		private static string GetJumpFromIni(string moduleName, bool all = false)
		{
			if (string.IsNullOrEmpty(moduleName)) return null;
			IniFile config = IniMgr.LoadModuleFile(moduleName, ModuleCofig);
			if (config == null) return null;
			string val;
			if (all)
			{
				val = config.GetValue(AllowedJump);
				if (string.IsNullOrEmpty(val))
				{
					Debug.LogError($"can't find [{AllowedJump}] from [{ModuleCofig}.ini] in [{moduleName}]");
					return null;
				}
			}
			else
			{
				val = config.GetValue(DefaultJump);
				if (string.IsNullOrEmpty(val))
				{
					Debug.LogError($"can't find [{DefaultJump}] from [{ModuleCofig}.ini] in [{moduleName}]");
					return null;
				}
			}
			return val;
		}

		#region ModuleConfig

		public class KeyVal
		{
			/// <summary>
			///
			/// </summary>
			public string key { get; set; }

			/// <summary>
			///
			/// </summary>
			public string value { get; set; }
		}

		public class KeyValRoot
		{
			/// <summary>
			///
			/// </summary>
			public string Name { get; set; }

			/// <summary>
			///
			/// </summary>
			public List<KeyVal> KeyValList { get; set; }
		}

		public class ModuleConfigRoot
		{
			/// <summary>
			///
			/// </summary>
			public List<KeyValRoot> ModuleList { get; set; }
		}

		#endregion ModuleConfig

		#endregion JumpModule
	}
}