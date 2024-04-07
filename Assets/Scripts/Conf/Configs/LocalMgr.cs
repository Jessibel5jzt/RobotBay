using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace WestBay
{
	/// <summary>
	/// 1.转换地址
	/// 2.读取文本
	/// </summary>
	/// @ingroup CoreApi
	public static class LocalMgr
	{
		/// <summary>
		/// 中文
		/// </summary>
		public const string Language_CN = "cn";

		/// <summary>
		/// 英文
		/// </summary>
		public const string Language_EN = "en";

		/// <summary>
		/// 繁体中文
		/// </summary>
		public const string Language_CHT = "cht";

		/// <summary>
		/// 德语
		/// </summary>
		public const string Language_DE = "de";

		/// <summary>
		/// 当前国家
		/// </summary>
		public static string CurrentCulture { get; set; }

		/// <summary>
		/// 把一个英语路径，转换为本地语言路径
		/// </summary>
		/// <param name="enPath">
		/// 资源路径，如 bg.png 文件
		/// resources/en/images/bg.png
		/// </param>
		/// <returns>全小写，转换后的路径</returns>
		public static string TransPath(string moduleName, string enPath)
		{
			if (CurrentCulture == null) return enPath;
			if (CurrentCulture == "en") return enPath;

			string enPathTemp = enPath.ToLower();
			if (!enPathTemp.Contains("resources")) return enPath;

			var Fldrs = enPath.Split(@"\/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			if (Fldrs.Length == 0) return enPath;

			int FldrIdx = 0;
			for (int i = 0; i < Fldrs.Length; i++)
			{
				if (Fldrs[i] == "resources")
				{
					FldrIdx = i;
					break;
				}
			}

			if (Fldrs[FldrIdx + 1] != "language") return enPath;
			if (Fldrs[FldrIdx + 2].ToLower() != CurrentCulture)
			{
				string lanPath = enPath.Replace($"/{Fldrs[FldrIdx + 2]}/", $"/{CurrentCulture}/");
				if (lanPath.Contains(ThemeMgr.ThemeSplitMarker)) return lanPath;
				string fullLanPath = "";
				if (Application.isPlaying)
				{
					fullLanPath = PathUtil.GetPersistPath(moduleName, lanPath);
				}
				else
				{
					fullLanPath = $"{Application.dataPath}/Mod_{moduleName}/{lanPath}";
				}

				if (File.Exists(fullLanPath)) return lanPath;
			}

			return enPath;
		}

		/// <summary>
		/// 读取UTF8文件
		/// </summary>
		/// <param name="filePathName">全路径</param>
		/// <returns>文本内容</returns>
		public static string ReadFile(string fileFullPath)
		{
			if (!File.Exists(fileFullPath)) return null;
			string str = File.ReadAllText(fileFullPath, Encoding.UTF8);
			return str;
		}

		public static void Init()
		{
			if (!Application.isPlaying) return;

			if (ConfigMgr.Ins != null)
			{
				//CurrentCulture = ConfigMgr.Ins.Prefs.Language;
			}
			else
			{
				Debug.LogError($"LocalMgr Init ConfigMgr.Ins is null!");
			}
		}
	}//class
}//namespace