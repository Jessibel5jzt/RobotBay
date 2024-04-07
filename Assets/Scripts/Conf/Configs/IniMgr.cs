using System.IO;
using System.Text;
using UnityEngine;

namespace WestBay
{
	/// <summary>
	/// 用这个类加载IniFile
	/// </summary>
	/// @ingroup CoreApi
	public static class IniMgr
	{
		public const string ConfigFileName = "localconfig.ini";

		/// <summary>
		/// 这个是LocalConfig.ini
		/// </summary>
		public static IniFile Config { get; private set; }

		public static void Init()
		{
			if (Config != null) return;

			Config = LoadFile(ConfigFileName);

			if (Config == null && Application.isEditor)
			{
				Config = new IniFile();
			}
		}

		/// <summary>
		/// Load file in module folder.
		/// 不用后缀，只传文件名，如 resources/texts/ui.ini 就传 ui
		/// </summary>
		/// <param name="filePathName">路径名</param>
		/// <returns></returns>
		public static IniFile LoadModuleLanguageFile(string moduleName, string filePathName)
		{
			string path = LocalMgr.TransPath(moduleName, $"resources/language/en/texts/{filePathName}.ini");
			string fullPath = "";
			if (Application.isPlaying)
			{
				fullPath = PathUtil.GetPersistPath(moduleName, path);
			}
			else
			{
				fullPath = $"{Application.dataPath}/Mod_{moduleName}/{path}";
			}

			return LoadIniFile(fullPath);
		}

		/// <summary>
		/// Load file in module folder.
		/// 不用后缀，只传文件名，如 resources/default/texts/ui.ini 就传 ui
		/// </summary>
		/// <param name="filePathName">路径名</param>
		/// <returns></returns>
		public static IniFile LoadModuleFile(string moduleName, string filePathName)
		{
			string path = LocalMgr.TransPath(moduleName, $"resources/default/texts/{filePathName}.ini");
			string fullPath = PathUtil.GetPersistPath(moduleName, path);

			return LoadIniFile(fullPath);
		}

		/// <summary>
		/// 获取指定模块指定子目录IniFile
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="subPath"></param>
		/// <param name="moduleName"></param>
		/// <returns></returns>
		public static IniFile LoadFile(string fileName, string subPath = "", string moduleName = App.SharedModule)
		{
			string filePath = Path.Combine(PathUtil.GetPersistPath(moduleName, subPath), fileName);
			return LoadIniFile(filePath);
		}

		/// <summary>
		/// 获取指定目录的IniFile
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static IniFile LoadIniFile(string filePath)
		{
			IniFile result = null;

			if (File.Exists(filePath))
			{
				result = new IniFile();
				string content = File.ReadAllText(filePath, Encoding.UTF8);
				result.SetBuffer(content);
			}

			return result;
		}

		/// <summary>
		/// 加载产品的LocalConfig
		/// </summary>
		/// <param name="robotType"></param>
		public static void MergeProductIni(string robotType)
		{
			var config = LoadFile(ConfigFileName, "", $"{robotType}Train");
			if (config != null)
			{
				foreach (var key in config.Keys)
				{
					Config.SetValueNotWrite(key, config.GetValue(key));
				}
			}
		}
	}//class
}