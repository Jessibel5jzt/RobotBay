using LitJson;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace WestBay
{
	public class PublishTool : EditorWindow
	{
		#region 类型定义

		private enum Platform
		{
			Windows,
			//Android
		}

		private enum ServerType
		{
			Debug,
			Release
		}

		#endregion 类型定义

		#region private Fields

		private static string _sectionVersion = "Version";
		private static string _sectionSetting = "Setting";
		private static string _sectionModule = "Module";
		private static string _sectionGit = "Git";
		private static string _sectionPath = "Path";
		private static string _sectionUnity = "Unity";

		private static string _rupsPath;
		private static string _configFilePath;
		private static IniFile _configFile;
		private static string[] _productTypes;
		private static int _selectProductIndex;

		private static Platform _platform;//平台
		private static ServerType _serverType;//是否为Release环境
		private static string _productType;//机型
		private static string _displayVersion;//显示版本号
		private static string _buildVersion;//构建版本号
		private static string _productMainVersion;//软件版本

		private static string _productGit;//当前产品分支
		private static string _rupsGit;//RUPS分支
		private static string _platformGit;//Platform分支
		private static string _commonGit;//Common分支
		private static string _gamesGit;//游戏分支

		private static string _sdk;//包依赖sdk
		private static string _tools;//包携带工具
		private static bool _isTask;//个人任务构建

		#endregion private Fields

		[MenuItem("Tools/远程构建", false, 28)]
		public static void ShowWindow()
		{
			_isTask = true;
			GetWindowWithRect(typeof(PublishTool), new Rect(0, 0, 400, 500), false, "远程构建工具");
			Init();
			InitConfig(_productType);
		}

		#region 初始化

		public static void Init()
		{
			_rupsPath = PathUtil.GetRupsPath();
			_platform = Platform.Windows;

			var pds = Functional.GetProductNames(_rupsPath);
			if (pds.ToList().Contains("M2"))
			{
				_productTypes = new string[pds.Length + 1];
				Array.Copy(pds, _productTypes, pds.Length);
				_productTypes[pds.Length] = "M2P";
			}
			else
			{
				_productTypes = pds;
			}

			_selectProductIndex = 0;
			if (!string.IsNullOrWhiteSpace(App.ProductTypeInsideEditor))
			{
				for (int i = 0; i < _productTypes.Length; ++i)
				{
					if (_productTypes[i].Equals(App.ProductTypeInsideEditor))
					{
						_selectProductIndex = i;
						break;
					}
				}
			}
			_productType = _productTypes[_selectProductIndex];
		}

		private static string _commonMods;
		private static string _productMods;
		private static string _gameMods;

		public static void InitConfig(string productType)
		{
			_rupsPath = PathUtil.GetRupsPath();

			_configFilePath = $"{_rupsPath}/Product/{productType}/Doc/发布文档/{productType}.ini";
			if (productType == "M2P")
			{
				_configFilePath = $"{_rupsPath}/Product/M2/Doc/发布文档/{productType}.ini";
			}

			_configFile = IniMgr.LoadIniFile(_configFilePath);
			_displayVersion = _configFile.GetValue(_sectionVersion, "Version");
			_buildVersion = _configFile.GetValue(_sectionVersion, "BuildVersion");
			_productMainVersion = _configFile.GetValue(_sectionVersion, "ProductMainVersion");

			string serverType = _configFile.GetValue(_sectionSetting, "ServerType");
			if (serverType.Equals("debug"))
			{
				_serverType = ServerType.Debug;
			}
			else
			{
				_serverType = ServerType.Release;
			}

			_commonMods = _configFile.GetValue(_sectionModule, "Common");
			_productMods = _configFile.GetValue(_sectionModule, $"{productType}");
			_gameMods = _configFile.GetValue(_sectionModule, "Games");
			_productGit = _configFile.GetValue(_sectionGit, $"{productType}Git");

			_rupsGit = "";
			if (_configFile.IsExistKey(_sectionGit, "RupsGit")) _rupsGit = _configFile.GetValue(_sectionGit, "RupsGit");

			_platformGit = _commonGit = _gamesGit = _rupsGit;
			if (_configFile.IsExistKey(_sectionGit, "PlatformGit")) _platformGit = _configFile.GetValue(_sectionGit, "PlatformGit");
			if (_configFile.IsExistKey(_sectionGit, "CommonGit")) _commonGit = _configFile.GetValue(_sectionGit, "CommonGit");
			if (_configFile.IsExistKey(_sectionGit, "GamesGit")) _gamesGit = _configFile.GetValue(_sectionGit, "GamesGit");

			_tools = _configFile.GetValue(_sectionPath, "Tools");
			_sdk = _configFile.GetValue(_sectionUnity, "SDK");
		}

		#endregion 初始化

		private void OnGUI()
		{
			GUILayout.BeginHorizontal();
			EditorGUILayout.Space();
			_platform = (Platform)EditorGUILayout.EnumPopup("平台:", _platform, GUILayout.Width(300));
			EditorGUILayout.Space();
			GUILayout.EndHorizontal();

			EditorGUILayout.Space();

			GUILayout.BeginHorizontal();
			EditorGUILayout.Space();
			EditorGUI.BeginChangeCheck();
			var selectIndex = EditorGUILayout.Popup("产品：", _selectProductIndex, _productTypes, GUILayout.Width(300));
			if (selectIndex != _selectProductIndex)
			{
				_selectProductIndex = selectIndex;
				_productType = _productTypes[_selectProductIndex];
			}
			bool isChange = EditorGUI.EndChangeCheck();
			if (isChange)
			{
				InitConfig(_productType);
			}
			EditorGUILayout.Space();
			GUILayout.EndHorizontal();

			EditorGUILayout.Space();

			if (!_isTask)
			{
				GUILayout.BeginHorizontal();
				EditorGUILayout.Space();
				_serverType = (ServerType)EditorGUILayout.EnumPopup("设置ServerType:", _serverType, GUILayout.Width(300));
				EditorGUILayout.Space();
				GUILayout.EndHorizontal();

				EditorGUILayout.Space();

				GUILayout.BeginHorizontal();
				EditorGUILayout.Space();
				_displayVersion = EditorGUILayout.TextField("显示版本号:", _displayVersion, GUILayout.Width(300));
				EditorGUILayout.Space();
				GUILayout.EndHorizontal();

				EditorGUILayout.Space();

				GUILayout.BeginHorizontal();
				EditorGUILayout.Space();
				_buildVersion = EditorGUILayout.TextField("构建版本号:", _buildVersion, GUILayout.Width(300));
				EditorGUILayout.Space();
				GUILayout.EndHorizontal();

				EditorGUILayout.Space();

				GUILayout.BeginHorizontal();
				EditorGUILayout.Space();
				_productMainVersion = EditorGUILayout.TextField("版本号:", _productMainVersion, GUILayout.Width(300));
				EditorGUILayout.Space();
				GUILayout.EndHorizontal();

				EditorGUILayout.Space();

				GUILayout.BeginHorizontal();
				EditorGUILayout.Space();
				_rupsGit = EditorGUILayout.TextField("RUPSGit:", _rupsGit, GUILayout.Width(300));
				EditorGUILayout.Space();
				GUILayout.EndHorizontal();

				EditorGUILayout.Space();

				if (string.IsNullOrWhiteSpace(_rupsGit))
				{
					GUILayout.BeginHorizontal();
					EditorGUILayout.Space();
					_platformGit = EditorGUILayout.TextField("PlatformGit:", _platformGit, GUILayout.Width(300));
					EditorGUILayout.Space();
					GUILayout.EndHorizontal();

					EditorGUILayout.Space();

					GUILayout.BeginHorizontal();
					EditorGUILayout.Space();
					_commonGit = EditorGUILayout.TextField("CommonGit:", _commonGit, GUILayout.Width(300));
					EditorGUILayout.Space();
					GUILayout.EndHorizontal();

					EditorGUILayout.Space();

					GUILayout.BeginHorizontal();
					EditorGUILayout.Space();
					_gamesGit = EditorGUILayout.TextField("GamesGit:", _gamesGit, GUILayout.Width(300));
					EditorGUILayout.Space();
					GUILayout.EndHorizontal();

					EditorGUILayout.Space();
				}
				GUILayout.BeginHorizontal();
				EditorGUILayout.Space();
				_productGit = EditorGUILayout.TextField("ProductGit:", _productGit, GUILayout.Width(300));
				EditorGUILayout.Space();
				GUILayout.EndHorizontal();

				EditorGUILayout.Space();

				GUILayout.BeginHorizontal();
				EditorGUILayout.Space();
				_commonMods = EditorGUILayout.TextField("CommonMods:", _commonMods, GUILayout.Width(300));
				EditorGUILayout.Space();
				GUILayout.EndHorizontal();

				EditorGUILayout.Space();

				GUILayout.BeginHorizontal();
				EditorGUILayout.Space();
				_productMods = EditorGUILayout.TextField("ProductMods:", _productMods, GUILayout.Width(300));
				EditorGUILayout.Space();
				GUILayout.EndHorizontal();

				EditorGUILayout.Space();

				GUILayout.BeginHorizontal();
				EditorGUILayout.Space();
				_gameMods = EditorGUILayout.TextField("GameMods:", _gameMods, GUILayout.Width(300));
				EditorGUILayout.Space();
				GUILayout.EndHorizontal();

				EditorGUILayout.Space();

				GUILayout.BeginHorizontal();
				EditorGUILayout.Space();
				_sdk = EditorGUILayout.TextField("SDK:", _sdk, GUILayout.Width(300));
				EditorGUILayout.Space();
				GUILayout.EndHorizontal();

				EditorGUILayout.Space();

				GUILayout.BeginHorizontal();
				EditorGUILayout.Space();
				_tools = EditorGUILayout.TextField("Tools:", _tools, GUILayout.Width(300));
				EditorGUILayout.Space();
				GUILayout.EndHorizontal();

				EditorGUILayout.Space();

				GUILayout.Space(50);

				EditorGUILayout.Space();
			}
			else
			{
				GUILayout.BeginHorizontal();
				EditorGUILayout.Space();
				_rupsGit = EditorGUILayout.TextField("分支:", _rupsGit, GUILayout.Width(300));
				EditorGUILayout.Space();
				GUILayout.EndHorizontal();

				EditorGUILayout.Space();
			}

			GUILayout.BeginHorizontal();
			GUILayout.Space(100);
			if (GUILayout.Button($"Jenkins出包", GUILayout.Width(200), GUILayout.Height(80)))
			{
				Close();
				var json = EditRequestJson();
				_ = RequestPublish(json);
				if (!_isTask)
				{
					RewritePublishConfig();
				}
			}
			GUILayout.EndHorizontal();
		}

		private void RewritePublishConfig()
		{
			_configFile.SetValueNotWrite("Version", _displayVersion, _sectionVersion);
			_configFile.SetValueNotWrite("BuildVersion", _buildVersion, _sectionVersion);
			_configFile.SetValueNotWrite("ProductMainVersion", _productMainVersion, _sectionVersion);

			_configFile.SetValueNotWrite("ServerType", _serverType.ToString().ToLower(), _sectionSetting);

			_configFile.SetValueNotWrite("Common", _commonMods, _sectionModule);
			_configFile.SetValueNotWrite("productType", _productMods, _sectionModule);
			_configFile.SetValueNotWrite("Games", _gameMods, _sectionModule);

			_configFile.SetValueNotWrite($"{_productType}Git", _productGit, _sectionGit);
			if (_configFile.IsExistKey(_sectionGit, "RupsGit")) _configFile.SetValueNotWrite("RupsGit", _rupsGit, _sectionGit);
			if (_configFile.IsExistKey(_sectionGit, "PlatformGit")) _configFile.SetValueNotWrite("PlatformGit", _platformGit, _sectionGit);
			if (_configFile.IsExistKey(_sectionGit, "CommonGit")) _configFile.SetValueNotWrite("CommonGit", _commonGit, _sectionGit);
			if (_configFile.IsExistKey(_sectionGit, "GamesGit")) _configFile.SetValueNotWrite("GamesGit", _gamesGit, _sectionGit);

			if (_configFile.IsExistKey(_sectionUnity, "SDK")) _configFile.SetValueNotWrite("SDK", _sdk, _sectionUnity);

			_configFile.SetValueNotWrite("Tools", _tools, _sectionPath);

			_configFile.WriteToFile(_configFilePath);
		}

		private string EditRequestJson()
		{
			if (_isTask)
			{
				var reqData = new JsonData
				{
					["PublishType"] = "2",
					["Product"] = _productType,
					["RUPSGit"] = _rupsGit,
					["CommonMods"] = _commonMods,
					["ProductMods"] = _productMods,
					["GameMods"] = _gameMods,
				};
				return reqData.ToJson();
			}
			else
			{
				var reqData = new JsonData
				{
					["PublishType"] = "1",
					["Product"] = _productType,
					["DisplayVersion"] = _displayVersion,
					["BuildVersion"] = _buildVersion,
					["ProductMainVersion"] = _productMainVersion,
					["ServerType"] = _serverType.ToString().ToLower(),
					["SDK"] = _sdk,
					["Tools"] = _tools,
					["CommonMods"] = _commonMods,
					["ProductMods"] = _productMods,
					["GameMods"] = _gameMods,
					["RUPSGit"] = _rupsGit,
					["PlatformGit"] = _platformGit,
					["CommonGit"] = _commonGit,
					["GamesGit"] = _gamesGit,
					["ProductGit"] = _productGit
				};
				return reqData.ToJson();
			}
		}

		private static async Task RequestPublish(string json)
		{
			var url = "http://192.168.8.254:8080/generic-webhook-trigger/invoke?token=Publish";

			Debug.Log("地址：" + url);
			try
			{
				await WebReqHelper.PostRequestByJson(url, json,
					delegate (bool isSuccess, string res)
					{
						if (isSuccess)
						{
							Debug.Log("Jenkins请求已触发：" + res);
						}
						else
						{
							//网络错误或者其他httperror
							Debug.Log("WebReqHelper: network error/http error");
						}
					});
			}
			catch (Exception ex)
			{
				Debug.Log("WebReqHelper error =" + ex.ToString());
			}
		}
	}
}