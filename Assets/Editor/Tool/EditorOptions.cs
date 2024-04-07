using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace WestBay
{
	[InitializeOnLoad]
	public static class EditorOptions
	{
		static EditorOptions()
		{
			InitSettings();
			InitPrefs();

			//GitHelper.Prune(Application.dataPath);
		}

		private static void InitPrefs()
		{
			_isChangePersistentPath = GetSetting(ChangePersistentPathKey, true);
			App.IsChangePersistentPathInsideEditor = _isChangePersistentPath;

			_productType = GetSetting(ProductTypeKey, "");
			App.ProductTypeInsideEditor = _productType;

			_isAutoLinkMod = GetSetting(AutoLinkMod, true);
		}

		#region 产品切换打包路径

		/// <summary>
		/// 切换PersistentPath
		/// </summary>
		private static readonly string ChangePersistentPathKey = "ChangePersistentPath";

		private static bool _isChangePersistentPath = false;

		public static bool IsChangePersistentPath
		{
			get { return _isChangePersistentPath; }
			set
			{
				_isChangePersistentPath = value;
				SetSetting(ChangePersistentPathKey, _isChangePersistentPath);
				App.IsChangePersistentPathInsideEditor = _isChangePersistentPath;
			}
		}

		#endregion 产品切换打包路径

		#region 产品类型

		/// <summary>
		/// 产品类型
		/// </summary>
		private static readonly string ProductTypeKey = "ProductType";

		private static string _productType;

		public static string ProductType
		{
			get { return _productType; }
			set
			{
				_productType = value;
				SetSetting(ProductTypeKey, _productType);
				App.ProductTypeInsideEditor = _productType;
			}
		}

		#endregion 产品类型

		#region 产品切换自动链接模块

		/// <summary>
		/// 产品切换自动链接模块
		/// </summary>
		private static readonly string AutoLinkMod = "AutoLinkMod";

		private static bool _isAutoLinkMod = true;

		public static bool IsAutoLinkMod
		{
			get { return _isAutoLinkMod; }
			set
			{
				_isAutoLinkMod = value;
				SetSetting(AutoLinkMod, _isAutoLinkMod);
			}
		}

		#endregion 产品切换自动链接模块

		public static Dictionary<string, bool> SdkDic { get; set; }

		#region 存档

		private static IniFile _settings = null;
		private static string _settingFilePath;

		private static void InitSettings()
		{
			_settingFilePath = $"{PathUtil.GetRupsPath()}/PersistentData/Settings.ini";
			_settings = IniMgr.LoadIniFile(_settingFilePath);
			if (_settings == null)
			{
				_settings = new IniFile();
			}
		}

		public static string GetSetting(string key, string defaultValue)
		{
			var result = defaultValue;
			if (_settings.IsExistKey(key))
			{
				result = _settings.GetValue(key);
			}

			return result;
		}

		public static bool GetSetting(string key, bool defaultValue)
		{
			var value = GetSetting(key, defaultValue.ToString());
			return bool.Parse(value);
		}

		public static void SetSetting(string key, string value)
		{
			_settings.SetValue(key, value, _settingFilePath);
		}

		public static void SetSetting(string key, bool value)
		{
			SetSetting(key, value.ToString());
		}

		#endregion 存档
	}
}