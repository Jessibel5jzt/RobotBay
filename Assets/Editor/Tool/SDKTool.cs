using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace WestBay
{
	public class SdkTool : EditorWindow
	{
		private const string SelectProductText = "Tools/SDK选项";

		private static bool _isInit = false;
		private static string[] _sdkPackageNames = null;
		private static Dictionary<string, bool> _sdkPackages = new Dictionary<string, bool>();

		[MenuItem(SelectProductText, false, 26)]
		public static void SelectProduct()
		{
			Init();
			GetWindowWithRect(typeof(SdkTool), new Rect(0, 0, 200, 400), false, "SDK选择");
		}

		private static void Init()
		{
			_sdkPackageNames = Functional.GetSdkNames();
			_sdkPackages.Clear();
			foreach (var item in _sdkPackageNames)
			{
				_sdkPackages.Add(item, EditorOptions.GetSetting(item, false));
			}

			_isInit = true;
		}

		private void OnGUI()
		{
			if (!_isInit)
			{
				Init();
			}
			if (_sdkPackages.Count == 0) return;
			EditorGUILayout.Space(4);

			foreach (var item in _sdkPackages)
			{
				GUILayout.BeginHorizontal();
				EditorGUILayout.Space();
				var isImport = EditorOptions.GetSetting(item.Key, false);
				var isImportUI = EditorGUILayout.Toggle(item.Key.Split('.')[2], item.Value, GUILayout.Width(150));
				if (isImport != isImportUI)
				{
					EditorOptions.SetSetting(item.Key, isImportUI);

					var iniPath = $"{PathUtil.SdkEditorPath}/{item.Key}/config.ini";
					if (isImportUI)
					{
						ImportSDK(item.Key);
						LinkMod(iniPath, true);
						AssetDatabase.Refresh();
					}
					else
					{
						LinkMod(iniPath, false);
						AssetDatabase.Refresh();
						RemoveSDK(item.Key);
					}
					_isInit = false;
					AssetDatabase.Refresh();
				}
				EditorGUILayout.Space();
				GUILayout.EndHorizontal();
			}
		}

		public static void ImportSDK(string packageName)
		{
			MakeSdkLink(packageName, true);
			AssetDatabase.Refresh();
		}

		private static void RemoveSDK(string packageName)
		{
			MakeSdkLink(packageName, false);
		}

		private static void LinkMod(string iniPath, bool isLink)
		{
			if (!FileHelper.IsExist(iniPath)) return;
			var iniFile = new IniFile();
			iniFile.SetBuffer(FileHelper.ReadFile(iniPath));

			var modList = iniFile.GetValue("ModList").Split(",");
			if (modList.Length == 0) return;
			for (int i = 0; i < modList.Length; i++)
			{
				if (string.IsNullOrWhiteSpace(modList[i])) continue;
				var to = $"{Application.dataPath}/{modList[i]}";
				var from = $"{PathUtil.CommonPath}/{modList[i]}";
				if (isLink)
				{
					Functional.MakeLink(from, to);
				}
				else
				{
					Functional.RemoveLink(to);
					FileHelper.DeleteChildDirectory(Application.dataPath, modList[i]);
				}
			}
		}

		private static void MakeSdkLink(string packageName, bool isLink)
		{
			if (!FileHelper.IsExist(PathUtil.SdkEditorPath))
			{
				FileHelper.CreatPath(PathUtil.SdkEditorPath);
			}
			var to = $"{PathUtil.SdkEditorPath}/{packageName}";
			var from = $"{PathUtil.SDKPath}/{packageName}";
			if (!FileHelper.IsDirectoryExist(from))
			{
				Debug.LogError($"未找到{packageName}的开发包，如有问题请检查相关目录文件：{from}");
				return;
			}

			if (isLink)
			{
				Functional.MakeLink(from, to);
			}
			else
			{
				FileHelper.DeleteChildDirectory(PathUtil.SdkEditorPath, packageName);
			}
		}

		public static string[] GetReferencedAssemblyPath()
		{
			var libs = new List<string>();
			var keys = (from q in _sdkPackages where q.Value select q.Key).ToList();
			foreach (var item in keys)
			{
				var iniPath = $"{PathUtil.SdkEditorPath}/{item}/config.ini";
				if (FileHelper.IsExist(iniPath))
				{
					var iniFile = new IniFile();
					iniFile.SetBuffer(FileHelper.ReadFile(iniPath));
					var sdkLibsStr = iniFile.GetValue("AssemblyPathList");
					if (string.IsNullOrWhiteSpace(sdkLibsStr)) continue;
					var sdkLibs = sdkLibsStr.Split(",");
					for (int i = 0; i < sdkLibs.Length; i++)
					{
						libs.Add($"{PathUtil.SdkEditorPath}/{item}/Plugins/{sdkLibs[i]}");
					}
				}
			}

			return libs.ToArray();
		}
	}
}