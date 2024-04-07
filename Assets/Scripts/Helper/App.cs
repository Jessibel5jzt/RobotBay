using LitJson;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace WestBay
{
	/// <summary>
	/// 一些开发用的设置
	/// </summary>
	public class App
	{
		protected App()
		{
		}

		#region 版本信息

		/// <summary>
		/// Project Manifest
		/// </summary>
		public const string VERSION_JSON_FILE = "version.json";

		public const string PRODUCT_VERSION_NAME = "productVer";
		public const string PRODUCT_BUILD_VERSION_NAME = "productBuildVer";
		public const string PRODUCT_MAIN_VERSION_NAME = "productMainVer";
		public const string AUTO_VERSION_NAME = "autoVer";
		public const string RUPS_VERSION_NAME = "rupsVer";

		/// <summary>
		/// 获取当前软件版本号（显示用）
		/// </summary>
		/// <returns></returns>
		public static string GetProductVersion()
		{
			return GetVersion(PRODUCT_VERSION_NAME, Application.version);
		}

		/// <summary>
		/// 获取产品内部版本号（产品计划版本号）
		/// </summary>
		/// <returns></returns>
		public static string GetProductBuildVersion()
		{
			return GetVersion(PRODUCT_BUILD_VERSION_NAME, Application.version);
		}

		/// <summary>
		/// 获取Rups版本
		/// </summary>
		/// <returns></returns>
		public static string GetRupsVersion()
		{
			return GetVersion(RUPS_VERSION_NAME, Application.version);
		}

		/// <summary>
		/// 获取模块changelog版本号（数据版本号）
		/// </summary>
		/// <returns></returns>
		public static string GetModuleChangelogVersion(string moduleName)
		{
			if (Application.isEditor)
			{
				var changelogPath = $"{Application.dataPath}/Mod_{moduleName}/Scripts/changelog.json";
				if (File.Exists(changelogPath))
				{
					JsonData jsonLog = JsonMapper.ToObject(File.ReadAllText(changelogPath));
					return jsonLog["version"].ToString();
				}
				return string.Empty;
			}
			return GetVersion("changelogVer", Application.version, moduleName);
		}

		/// <summary>
		/// 从version.json获取版本信息
		/// </summary>
		/// <param name="key"></param>
		/// <param name="defaultVer"></param>
		/// <returns></returns>
		public static string GetVersion(string key, string defaultVer = "", string moduleName = "")
		{
			string version = defaultVer;
			string versionFilePath = $"{Application.streamingAssetsPath}/{VERSION_JSON_FILE}";
			if (!string.IsNullOrEmpty(moduleName))
			{
				versionFilePath = PathUtil.GetPersistPath(moduleName, VERSION_JSON_FILE);
			}

			var pathVer = GetVersionFromJson(key, versionFilePath);
			if (!string.IsNullOrWhiteSpace(pathVer))
			{
				version = pathVer;
			}

			return version;
		}

		/// <summary>
		/// 获取版本信息
		/// </summary>
		/// <param name="key"></param>
		/// <param name="defaultVer"></param>
		/// <returns></returns>
		public static VersionInfo GetVersionInfo(string moduleName = "")
		{
			string versionFilePath = $"{Application.streamingAssetsPath}/{VERSION_JSON_FILE}";
			if (!string.IsNullOrEmpty(moduleName))
			{
				versionFilePath = PathUtil.GetPersistPath(moduleName, VERSION_JSON_FILE);
			}

			if (File.Exists(versionFilePath))
			{
				var json = File.ReadAllText(versionFilePath);
				return GetVersionFromJson(json);
			}
			else
			{
				return new VersionInfo();
			}
		}

		public static string GetVersionFromJson(string key, string jsonPath)
		{
			if (File.Exists(jsonPath))
			{
				var jsonVersion = JsonMapper.ToObject(File.ReadAllText(jsonPath));
				return JsonHelper.ReadFromJson(jsonVersion, key);
			}

			return string.Empty;
		}

		public static VersionInfo GetVersionFromJson(string json)
		{
			if (string.IsNullOrWhiteSpace(json)) return new VersionInfo();

			var jsondata = JsonMapper.ToObject(json);

			VersionInfo versions = new VersionInfo();
			versions.AutoVer = JsonHelper.ReadFromJson(jsondata, AUTO_VERSION_NAME);
			versions.ProductMainVer = JsonHelper.ReadFromJson(jsondata, PRODUCT_MAIN_VERSION_NAME);
			versions.ProductBuildVersion = JsonHelper.ReadFromJson(jsondata, PRODUCT_BUILD_VERSION_NAME);
			versions.RupsVersion = JsonHelper.ReadFromJson(jsondata, RUPS_VERSION_NAME);
			versions.ProductVerForShow = JsonHelper.ReadFromJson(jsondata, PRODUCT_VERSION_NAME);
			return versions;
		}

		public static string GetJsonFromVersion(VersionInfo versionInfo)
		{
			if (!IsVersionConfirm(versionInfo)) return string.Empty;
			JsonData version = new JsonData();
			version[AUTO_VERSION_NAME] = versionInfo.AutoVer;
			version[PRODUCT_BUILD_VERSION_NAME] = versionInfo.ProductBuildVersion;
			version[RUPS_VERSION_NAME] = versionInfo.RupsVersion;
			if (!string.IsNullOrWhiteSpace(versionInfo.ProductMainVer))
			{
				version[PRODUCT_MAIN_VERSION_NAME] = versionInfo.ProductMainVer;
			}
			if (!string.IsNullOrWhiteSpace(versionInfo.ProductVerForShow))
			{
				version[PRODUCT_VERSION_NAME] = versionInfo.ProductVerForShow;
			}
			return version.ToJson();
		}

		public static bool IsVersionConfirm(VersionInfo versionInfo)
		{
			if (string.IsNullOrEmpty(versionInfo.RupsVersion)) return false;
			if (string.IsNullOrEmpty(versionInfo.ProductBuildVersion)) return false;
			if (string.IsNullOrEmpty(versionInfo.AutoVer)) return false;
			return true;
		}

		/// <summary>
		/// 版本比对
		/// </summary>
		/// <param name="localVersion"></param>
		/// <param name="doopVersion"></param>
		/// <returns></returns>
		public static int CompareToGetNewerVersionInfo(List<VersionInfo> versionInfoList)
		{
			if (versionInfoList.Count == 0) return -1;
			string maxVer = "0.0.0.0";

			int maxIdx = 0;
			for (int i = 0; i < versionInfoList.Count; i++)
			{
				if (!IsVersionConfirm(versionInfoList[i])) continue;

				if (FileHelper.CompareVersion(versionInfoList[i].ProductBuildVersion, maxVer))
				{
					maxVer = versionInfoList[i].ProductBuildVersion;
					maxIdx = i;
				}
			}

			return maxIdx;
		}

		#endregion 版本信息

		#region 编辑器环境变量

		/// <summary>
		/// 是否是编辑器模式
		/// </summary>
		public static bool IsEditor { get; set; } = false;

		/// <summary>
		/// 编辑器下产品类型
		/// </summary>
		public static string ProductTypeInsideEditor { get; set; } = "";

		public static bool UseModuleScript { get; set; } = false;

		/// <summary>
		/// 编辑器下是否改变AB包目录
		/// </summary>
		public static bool IsChangePersistentPathInsideEditor { get; set; } = false;

		public static bool IsDebug { get; set; } = true;

		#endregion 编辑器环境变量

		public const string ModNamePrefix = "Mod_";
		public const string SharedModule = "Shared";
	}

	public struct VersionInfo
	{
		public string RupsVersion { get; set; }
		public string ProductBuildVersion { get; set; }
		public string AutoVer { get; set; }
		public string ProductVerForShow { get; set; }
		public string ProductMainVer { get; set; }
	}

	/// <summary>
	/// WestBay Debug
	/// </summary>
	public class Debug : DebugT<Debug.WestBay>
	{
		public class WestBay : DebugNamer
		{
			public WestBay()
			{ Name = "WestBay"; }
		}
	}
}