using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace WestBay
{
	public class ProductTool : EditorWindow
	{
		private const string SelectProductText = "Tools/产品选项";

		private static bool _isInit = false;
		private static string[] ProductTypes = null;
		private static int _selectProductIndex = 0;

		/// <summary>
		/// 是否改变打包路径
		/// </summary>
		private static bool _isChangePersistentPath = true;

		/// <summary>
		/// 切换产品时是否自动链接模块
		/// </summary>
		private static bool _isAutoLinkMod = true;

		[MenuItem(SelectProductText, false, 25)]
		public static void SelectProduct()
		{
			Init();
			GetWindowWithRect(typeof(ProductTool), new Rect(0, 0, 500, 400), false, "产品类型选择");
		}

		private static void Init()
		{
			var rupsRoot = PathUtil.GetRupsPath();
			ProductTypes = Functional.GetProductNames(rupsRoot, true);
			_isChangePersistentPath = EditorOptions.IsChangePersistentPath;
			_selectProductIndex = 0;
			if (!string.IsNullOrWhiteSpace(App.ProductTypeInsideEditor))
			{
				for (int i = 0; i < ProductTypes.Length; ++i)
				{
					if (ProductTypes[i].Equals(App.ProductTypeInsideEditor))
					{
						_selectProductIndex = i;
						break;
					}
				}
			}
			InitAdapterWriter();
			_isInit = true;
		}

		public static void InitAdapterWriter()
		{
		}

		private void OnGUI()
		{
			if (!_isInit)
			{
				Init();
			}
			EditorGUILayout.Space(4);

			GUILayout.BeginHorizontal();
			EditorGUILayout.Space();
			var isChange = EditorGUILayout.Toggle("切换模块包路径", _isChangePersistentPath, GUILayout.Width(400));
			if (isChange != _isChangePersistentPath)
			{
				_isChangePersistentPath = isChange;
				EditorOptions.IsChangePersistentPath = _isChangePersistentPath;
				PathUtil.ResetPersistentPath();
			}
			EditorGUILayout.Space();
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			EditorGUILayout.Space();
			var isLink = EditorGUILayout.Toggle("是否链接模块", _isAutoLinkMod, GUILayout.Width(400));
			if (isLink != _isAutoLinkMod)
			{
				_isAutoLinkMod = isLink;
				EditorOptions.IsAutoLinkMod = _isAutoLinkMod;
			}
			EditorGUILayout.Space();
			GUILayout.EndHorizontal();

			if (ProductTypes != null && ProductTypes.Length != 0)
			{
				GUILayout.BeginHorizontal();
				EditorGUILayout.Space();
				var preProduct = EditorOptions.ProductType;
				var selectIndex = EditorGUILayout.Popup("产品：", _selectProductIndex, ProductTypes, GUILayout.Width(400));
				if (selectIndex != _selectProductIndex)
				{
					_selectProductIndex = selectIndex;
					EditorOptions.ProductType = ProductTypes[_selectProductIndex];
					PathUtil.ResetPersistentPath();

					if (!FileHelper.IsExist(PathUtil.ProductEditorPath))
					{
						FileHelper.CreatPath(PathUtil.ProductEditorPath);
					}

					if (_isAutoLinkMod)
					{
						RemoveMods(preProduct);
						AssetDatabase.Refresh();
						LinkMods();
					}

					RemoveProductSDK(preProduct);
					ImportProductSDK(EditorOptions.ProductType);

					_isInit = false;
					AssetDatabase.Refresh();
				}

				EditorGUILayout.Space();
				GUILayout.EndHorizontal();
			}
			GUILayout.BeginHorizontal();
			EditorGUILayout.Space();
			GUILayout.Label($"当前产品： {EditorOptions.ProductType}", GUILayout.Width(400));
			EditorGUILayout.Space();
			GUILayout.EndHorizontal();

			EditorGUILayout.Space(4);
			GUILayout.Label("说明：");
			GUILayout.Label($"1.模块包的路径为{PathUtil.GetPersistPath()}");
		}

		public static void ImportProductSDK(string productName)
		{
			MakeProductLink(productName);
			MakeAdapter(productName, true);
		}

		public static void RemoveProductSDK(string productName)
		{
			MakeAdapter(productName, false);

			FileHelper.CleanDirectory(PathUtil.ProductEditorPath);
			AssetDatabase.Refresh();
		}

		public static void RemoveAllSDK()
		{
			FileHelper.CleanDirectory(PathUtil.ProductEditorPath);
		}

		private static void MakeAdapter(string product, bool isWrite)
		{
			var packageName = $"com.fftai.{product.ToLower()}";
			if (isWrite)
			{
			}
			else
			{
			}

			AssetDatabase.Refresh();
		}

		public static void MakeProductLink(string productName, bool fresh = true)
		{
			if (productName.Equals("None")) return;
			var packageName = $"com.fftai.{productName.ToLower()}";
			var to = $"{PathUtil.ProductEditorPath}/{packageName}";
			var from = $"{PathUtil.GetProductSDKPath(productName)}/{packageName}";

			if (!FileHelper.IsDirectoryExist(from))
			{
				Debug.Log($"未找到{productName}的开发包，如有问题请检查相关目录文件：{from}");
				return;
			}

			Functional.MakeLink(from, to);

			if (fresh)
			{
				AssetDatabase.Refresh();
			}
		}

		private static void LinkMods()
		{
			var productModulePath = PathUtil.GetProductModulePath(EditorOptions.ProductType);
			if (!Directory.Exists(productModulePath)) return;

			var mods = Directory.GetDirectories(productModulePath);
			if (mods.Length == 0) return;

			for (int i = 0; i < mods.Length; i++)
			{
				if (string.IsNullOrWhiteSpace(mods[i])) continue;

				var dir = new DirectoryInfo(mods[i]);
				if (!dir.Name.Contains("Mod_")) continue;

				var to = $"{Application.dataPath}/{dir.Name}";
				var from = mods[i];

				Functional.MakeLink(from, to);
			}
		}

		private static void RemoveMods(string lastProduct)
		{
			foreach (var item in Directory.GetDirectories(Application.dataPath))
			{
				if (item.Contains($"Mod_{lastProduct}"))
				{
					Functional.RemoveLink(item);
					var dir = new DirectoryInfo(item);
					FileHelper.DeleteChildDirectory(Application.dataPath, dir.Name);
				}
			}
		}

		public static string[] GetReferencedAssemblyPath()
		{
			var libs = new List<string>();
			var packageName = $"com.fftai.{EditorOptions.ProductType.ToLower()}";
			var iniPath = $"{PathUtil.ProductEditorPath}/{packageName}/config.ini";

			if (FileHelper.IsExist(iniPath))
			{
				var iniFile = new IniFile();
				iniFile.SetBuffer(FileHelper.ReadFile(iniPath));
				var sdkLibsStr = iniFile.GetValue("AssemblyPathList");
				if (string.IsNullOrWhiteSpace(sdkLibsStr)) return libs.ToArray();
				var sdkLibs = iniFile.GetValue("AssemblyPathList").Split(",");
				for (int i = 0; i < sdkLibs.Length; i++)
				{
					libs.Add($"{PathUtil.ProductEditorPath}/{packageName}/Plugins/{sdkLibs[i]}");
				}
			}

			return libs.ToArray();
		}
	}
}