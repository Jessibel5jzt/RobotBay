using System.IO;
using UnityEngine;

namespace WestBay
{
	/// <summary>
	/// 路径 有2
	/// 1. 包内StreamingAssets，首次运行时，其内资源自动解压至PersistentPath，从此以后不再访问此目录。此目录为只读。
	/// 2. PersistentPath，程序运行期间，_只_从这个目录读写资源。（但是Editor 模式，此路径返回1. 以方便开发）
	/// </summary>
	public class PathUtil
	{
		/// <summary>
		/// Persistent Path 游戏中读写资源目录。
		/// 可以是子模块，也可以是Platform。Platform和各个子模块平级。
		/// 保证一定会是用 “/” 分割。你不要再自己替换了。
		/// 用参数添加 模块名 和  模块子目录，也不要在外面用 +号
		/// </summary>
		/// <param name="modName">platform, login, ...</param>
		/// <param name="subPath">"resources/..."; "codes/..."; "prefabs/..."</param>
		/// <returns>最后结尾没有“/”，你需要自己添加</returns>
		public static string GetPersistPath(string modName, string subPath = null)
		{
			string result = "";
			if (!string.IsNullOrEmpty(subPath))
			{
				result = $"/{subPath}";
			}

			if (App.IsEditor)
			{
				result = $"{Application.dataPath}/Mod_{modName.ToLower()}{result}";
			}
			else
			{
				result = $"{GetPersistPath()}/{modName.ToLower()}{result}";
			}

			return result;
		}

		/// <summary>
		/// 保证一定会是用 “/” 分割。你不要再自己替换了。
		/// </summary>
		/// <returns>
		/// 最后结尾没有“/”，你需要自己添加
		/// </returns>
		public static string GetPersistPath()
		{
			if (PersistentPath != null) return PersistentPath;

			if (Application.platform == RuntimePlatform.WindowsEditor
			|| Application.platform == RuntimePlatform.LinuxEditor
			|| Application.platform == RuntimePlatform.OSXEditor)
			{
				PersistentPath = Application.streamingAssetsPath;
				if (App.IsChangePersistentPathInsideEditor)
				{
					PersistentPath = GetPersistentPathInEditor();
				}
			}
			else if (Application.platform == RuntimePlatform.WindowsPlayer
				|| Application.platform == RuntimePlatform.LinuxPlayer)
			{
				PersistentPath = $"{System.AppDomain.CurrentDomain.BaseDirectory}/PersistentData";
			}
			else
			{
				PersistentPath = Application.persistentDataPath;
			}

			PersistentPath = PersistentPath.Replace('\\', '/');
			return PersistentPath;
		}

		public static string GetEXEPath()
		{
			return System.AppDomain.CurrentDomain.BaseDirectory.Replace("\\", "/");
		}

		private static string PersistentPath;

		/// <summary>
		/// 平台名
		/// </summary>
		/// <returns></returns>
		public static string GetPlatformName()
		{
			string platform = "Windows";
			if (Application.platform == RuntimePlatform.WindowsPlayer)
			{
				platform = "Windows";
			}
			else if (Application.platform == RuntimePlatform.Android)
			{
				platform = "Android";
			}
			else if (Application.platform == RuntimePlatform.IPhonePlayer
				|| Application.platform == RuntimePlatform.OSXPlayer)
			{
				platform = "iOS";
			}
			else if (Application.platform == RuntimePlatform.LinuxPlayer)
			{
				platform = "Linux";
			}

			return platform;
		}

		public static string GetDriverPath(string directoryName)
		{
			if (string.IsNullOrWhiteSpace(directoryName)) return string.Empty;

			DriveInfo[] allDrives = DriveInfo.GetDrives();
			for (int i = 0; i < allDrives.Length; i++)
			{
				if (allDrives[i].IsReady && allDrives[i].DriveType == DriveType.Removable)
				{
					var directoryPath = $"{allDrives[i].RootDirectory.Name}{directoryName}".Replace('\\', '/');
					if (Directory.Exists(directoryPath))
					{
						return directoryPath;
					}
				}
			}

			return string.Empty;
		}

		public static string GetRupsPath()
		{
			string result = "";
			string dataPath = Application.dataPath.Replace('\\', '/');
			int len = 0;
			string[] strArray = dataPath.Split('/');
			bool isExist = false;
			for (int i = strArray.Length - 1; i >= 0; --i)
			{
				string folderName = strArray[i];

				if (folderName.ToLower().Contains("westbay"))
				{
					isExist = true;
					break;
				}
				len += folderName.Length + 1;
			}

			if (isExist)
			{
				result = dataPath.Substring(0, dataPath.Length - len);
			}
			else
			{
				Debug.LogError("Rups path not found!");
			}
			return result;
		}

		public static string GetBuildPath()
		{
			string dataPath = Application.dataPath.Replace('\\', '/');
			var idx = dataPath.IndexOf("RUPS");
			string buildPath = dataPath.Substring(0, idx + "RUPS".Length);

			return buildPath;
		}

		public static void ResetPersistentPath()
		{
			PersistentPath = null;
		}

		public static string GetPersistentPathInEditor()
		{
			var rupsPath = GetRupsPath();
			string result = $"{rupsPath}/PersistentData";

			string productType = App.ProductTypeInsideEditor;
			if (!string.IsNullOrWhiteSpace(productType))
			{
				result = $"{rupsPath}/PersistentData/{productType}";
			}

			return result;
		}

		public static string GetProductSDKPath(string productType)
		{
			return $"{ProductPath}/{productType}/Files/SDK";
		}

		public static string GetProductModulePath(string productType)
		{
			return $"{ProductPath}/{productType}/Module";
		}

		public static string SdkEditorPath
		{ get { return $"{Application.dataPath}/Packages/SDK"; } }

		public static string ProductEditorPath
		{ get { return $"{Application.dataPath}/Packages/Product"; } }

		public static string ProductPath
		{ get { return $"{GetRupsPath()}/Product"; } }

		public static string CommonPath
		{ get { return $"{GetRupsPath()}/SM/Common"; } }

		public static string SDKPath
		{ get { return $"{GetRupsPath()}/SM/Common/Packages"; } }
	}//class
}//namespace