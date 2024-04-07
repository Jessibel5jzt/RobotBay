using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.Threading.Tasks;
using UnityEditor.PackageManager;

namespace WestBay
{
	public class JenkinsManager : EditorWindow
	{
		#region 类型定义

		private enum Platform
		{
			Windows,
			Android
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
		private static string _commonSrcPath;
		private static string _gamesSrcPath;
		private static string _productSrcPath;
		private static string _configFilePath;
		private static IniFile _configFile;
		private static string _targetZipPath;
		private static string[] _productTypes;
		private static int _selectProductIndex;

		private const string ProjectName = "Platform";//产品名
		private static Platform _platform;//平台
		private static string _productType;//机型
		private static string _productName;//产品名
		private static string _version;//显示版本号
		private static string _buildVersion;//构建版本号
		private static string _productMainVersion;//软件版本
		private static bool _isOneKeyGen;
		private static ServerType _serverType;//是否为Release环境
		private static bool _isUSB;//是否输出至U盘更新工具
		private static bool _zipForUpload = false;//是否为上传Doop压缩
		private static string _binPath;//bin路径
		private static string _publishPath;//发布路径
		private static JsonData _versionJson;

		private static string _productGit;//当前产品地址库名

		private static string _rupsGit;//RUPS地址库名
		private static string _platformGit;//Platform地址库名
		private static string _commonGit;//Common地址库名
		private static string _gamesGit;//游戏地址库名

		private static bool _isBuildTest = false;//是否是 测试构建
		private static bool _isManualBuild;//是否是 手动打包

		#endregion private Fields

		[MenuItem("Tools/本地构建", false, 27)]
		public static void ShowWindow()
		{
			_isManualBuild = true;
			GetWindowWithRect(typeof(JenkinsManager), new Rect(0, 0, 400, 500), false, "本地构建工具");
			InitInEditor();
			InitPath();
			ReadConfig();
			PullProduct(_productGit);
		}

		#region 初始化

		public static void InitInEditor()
		{
			_rupsPath = PathUtil.GetRupsPath();
			_platform = Platform.Windows;
			_productType = "M2";
			_isUSB = false;
			_isOneKeyGen = false;

			_productTypes = Functional.GetProductNames(_rupsPath);
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
		}

		private static string[] _commonModArray;
		private static string[] _productModArray;
		private static string[] _gameModArray;

		private static void InitPath()
		{
			_rupsPath = PathUtil.GetRupsPath();
			_commonSrcPath = _rupsPath + "/SM/Common";
			_gamesSrcPath = _rupsPath + "/SM/Games";
			//product
			_productSrcPath = $"{_rupsPath}/Product/{_productType}";
			_configFilePath = $"{_rupsPath}/Product/{_productType}/Doc/发布文档/{_productType}.ini";
			if (_productType == "M2P")
			{
				EditorOptions.ProductType = "M2";
				_productSrcPath = $"{_rupsPath}/Product/M2";
				_configFilePath = $"{_rupsPath}/Product/M2/Doc/发布文档/{_productType}.ini";
			}

			if (_isBuildTest) _configFilePath = $"{_rupsPath}/Doc/RUPS发布/00-发布设置/BuildTest.ini";
		}

		public static void ReadConfig()
		{
			EditorOptions.ProductType = _productType;

			_configFile = IniMgr.LoadIniFile(_configFilePath);
			_productName = _configFile.GetValue(_sectionVersion, "ProductName");
			_version = _configFile.GetValue(_sectionVersion, "Version");
			_buildVersion = _configFile.GetValue(_sectionVersion, "BuildVersion");
			_productMainVersion = _configFile.GetValue(_sectionVersion, "BuildVersion");
			_publishPath = $"{_configFile.GetValue(_sectionPath, "ZipPath")}/{_productType}";
			_binPath = $"{_rupsPath}/Bin";
			if (_isManualBuild)
			{
				_publishPath = $"{_publishPath}/Manual";
			}
			else
			{
				_publishPath = $"{_publishPath}/Jenkins";
			}

			var verJsonPath = $"{_rupsPath}/doopversion.json";
			if (File.Exists(verJsonPath))
			{
				_versionJson = JsonMapper.ToObject(File.ReadAllText(verJsonPath));
			}

			string serverType = _configFile.GetValue(_sectionSetting, "ServerType");
			if (serverType.Equals("debug"))
			{
				_serverType = ServerType.Debug;
			}
			else
			{
				_serverType = ServerType.Release;
			}

			_commonModArray = _configFile.GetValue(_sectionModule, "Common").Split(',');
			_productModArray = _configFile.GetValue(_sectionModule, $"{_productType}").Split(',');
			_gameModArray = _configFile.GetValue(_sectionModule, "Games").Split(',');
			_productGit = _configFile.GetValue(_sectionGit, $"{_productType}Git");

			_rupsGit = "master";
			if (_configFile.IsExistKey(_sectionGit, "RupsGit")) _rupsGit = _configFile.GetValue(_sectionGit, "RupsGit");

			_platformGit = _commonGit = _gamesGit = _rupsGit;
			if (_configFile.IsExistKey(_sectionGit, "PlatformGit")) _platformGit = _configFile.GetValue(_sectionGit, "PlatformGit");
			if (_configFile.IsExistKey(_sectionGit, "CommonGit")) _commonGit = _configFile.GetValue(_sectionGit, "CommonGit");
			if (_configFile.IsExistKey(_sectionGit, "GamesGit")) _gamesGit = _configFile.GetValue(_sectionGit, "GamesGit");
		}

		private static string GetMachineName()
		{
			var machine = _productType;
			if (machine.Equals("M2P")) machine = "M2";

			return machine.ToString();
		}

		#endregion 初始化

		#region UI界面

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
				PullProduct(_productGit);
				ReadConfig();
			}
			EditorGUILayout.Space();
			GUILayout.EndHorizontal();

			EditorGUILayout.Space();

			GUILayout.BeginHorizontal();
			EditorGUILayout.Space();
			_serverType = (ServerType)EditorGUILayout.EnumPopup("设置ServerType:", _serverType, GUILayout.Width(300));
			EditorGUILayout.Space();
			GUILayout.EndHorizontal();

			EditorGUILayout.Space();

			GUILayout.BeginHorizontal();
			EditorGUILayout.Space();
			_productName = EditorGUILayout.TextField("包名:", _productName, GUILayout.Width(300));
			EditorGUILayout.Space();
			GUILayout.EndHorizontal();

			EditorGUILayout.Space();

			GUILayout.BeginHorizontal();
			EditorGUILayout.Space();
			_version = EditorGUILayout.TextField("版本号:", _version, GUILayout.Width(300));
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
			_platformGit = EditorGUILayout.TextField("PlatformGit:", _platformGit, GUILayout.Width(245));
			if (GUILayout.Button($"签出", GUILayout.Width(50), GUILayout.Height(20)))
			{
				ExecuteGitCmd($"{_rupsPath}/{ProjectName}", _platformGit, Gitea.PlatformRepoFullName, true);
			}
			EditorGUILayout.Space();
			GUILayout.EndHorizontal();

			EditorGUILayout.Space();

			GUILayout.BeginHorizontal();
			EditorGUILayout.Space();
			_commonGit = EditorGUILayout.TextField("CommonGit:", _commonGit, GUILayout.Width(245));
			if (GUILayout.Button($"签出", GUILayout.Width(50), GUILayout.Height(20)))
			{
				ExecuteGitCmd($"{_rupsPath}/SM/Common", _commonGit, Gitea.CommonRepoFullName);
			}
			EditorGUILayout.Space();
			GUILayout.EndHorizontal();

			EditorGUILayout.Space();

			GUILayout.BeginHorizontal();
			EditorGUILayout.Space();
			_productGit = EditorGUILayout.TextField("MachineGit:", _productGit, GUILayout.Width(245));
			if (GUILayout.Button($"签出", GUILayout.Width(50), GUILayout.Height(20)))
			{
				ExecuteGitCmd($"{_productSrcPath}", _productGit, $"{Gitea.ProductRepoFullNamePrefix}/{_productType}");
			}
			EditorGUILayout.Space();
			GUILayout.EndHorizontal();

			EditorGUILayout.Space();

			GUILayout.BeginHorizontal();
			EditorGUILayout.Space();
			_isOneKeyGen = EditorGUILayout.Toggle("重新绑定代码:", _isOneKeyGen, GUILayout.Width(300));
			EditorGUILayout.Space();
			GUILayout.EndHorizontal();

			EditorGUILayout.Space();

			GUILayout.BeginHorizontal();
			EditorGUILayout.Space();
			_zipForUpload = EditorGUILayout.Toggle("分包压缩（上传doop用）:", _zipForUpload, GUILayout.Width(300));
			EditorGUILayout.Space();
			GUILayout.EndHorizontal();

			EditorGUILayout.Space();
			GUILayout.BeginHorizontal();
			EditorGUILayout.Space();
			_isUSB = (!string.IsNullOrEmpty(PathUtil.GetDriverPath(""))) && EditorGUILayout.Toggle("拷贝至U盘:", _isUSB, GUILayout.Width(300));
			EditorGUILayout.Space();
			GUILayout.EndHorizontal();

			EditorGUILayout.Space();

			GUILayout.BeginHorizontal();
			EditorGUILayout.Space();
			GUILayout.Label($"输出路径： {_binPath}", GUILayout.Width(210));
			if (GUILayout.Button("选择文件夹", GUILayout.Width(85)))
			{
				OpenDirectory();
			}
			EditorGUILayout.Space();

			GUILayout.EndHorizontal();

			GUILayout.Space(50);

			EditorGUILayout.Space();

			GUILayout.BeginHorizontal();
			GUILayout.Space(100);
			if (GUILayout.Button($"打包", GUILayout.Width(200), GUILayout.Height(80)))
			{
				this.Close();
				PullRups();
			}
			GUILayout.EndHorizontal();
		}

		#endregion UI界面

		#region 压缩

		private static void ZipRupsForUpload()
		{
			Debug.Log("prepare for upload");
			var doopZipPath = $"{_rupsPath}/ZIP";

			DeleteAll($"{_binPath}/PersistentData");
			if (Directory.Exists($"{_binPath}/PersistentData"))
			{
				Directory.Delete($"{_binPath}/PersistentData");
			}

			GenerateVersionInfo(_binPath, "RUPS");

			//拷贝工具
			CopyToolsToBin(_binPath);
			if (_zipForUpload)
			{
				Zip(_binPath, doopZipPath, "RUPS");
			}
		}

		public static void FullZip()
		{
			const string packageZipName = "RUPS";
			//拷贝工具
			CopyToolsToBin(_binPath);
			//压缩成ZIP
			_targetZipPath = $"{_publishPath}/{_productType}v{_version}_{DateTime.Now:yyyyMMddHHmmss}";
			Zip(_binPath, _targetZipPath, packageZipName);
			string fileName = $"{packageZipName}_md5.txt";
			//创建Zip MD5值
			CreateZipMD5(_targetZipPath, fileName);

			CreateZipInfo(_targetZipPath);

			//定期清理测试包
			ClearZip();

			AssetDatabase.Refresh();
			string sourceModulePath, targetModuleZipPath;
			if (_zipForUpload)
			{
				foreach (var item in _commonModArray)
				{
					if (string.IsNullOrWhiteSpace(item)) continue;
					sourceModulePath = $"{_binPath}/PersistentData/{item.ToLower()}";
					targetModuleZipPath = $"{_rupsPath}/ZIP";
					Zip(sourceModulePath, targetModuleZipPath, item.ToLower());
				}

				foreach (var item in _productModArray)
				{
					if (string.IsNullOrWhiteSpace(item)) continue;
					sourceModulePath = $"{_binPath}/PersistentData/{item.ToLower()}";
					targetModuleZipPath = $"{_rupsPath}/ZIP";
					Zip(sourceModulePath, targetModuleZipPath, item.ToLower());
				}

				foreach (var item in _gameModArray)
				{
					if (string.IsNullOrWhiteSpace(item)) continue;
					sourceModulePath = $"{_binPath}/PersistentData/{item.ToLower()}";
					targetModuleZipPath = $"{_rupsPath}/ZIP";
					Zip(sourceModulePath, targetModuleZipPath, item.ToLower());
				}
			}
			AssetDatabase.Refresh();
		}

		private static void CreateZipInfo(string zipPath)
		{
			var verJsonStr = File.ReadAllText($"{_binPath}/Platform_Data/StreamingAssets/version.json");
			JsonData jsonZip = JsonMapper.ToObject(verJsonStr);
			jsonZip["ConnectRobot"] = _productType;

			var zipJson = jsonZip.ToJson();

			using (FileStream fs = new FileStream($"{zipPath}/package.json", FileMode.Create))
			{
				StreamWriter sw = new StreamWriter(fs);
				sw.Write(zipJson);
				sw.Flush();
				sw.Close();
			}
		}

		public static void Zip(string sourceZipPath, string targetPath, string zipName)
		{
			if (!Directory.Exists(targetPath))
			{
				Directory.CreateDirectory(targetPath);
			}

			string destZipPath = $"{targetPath}/{zipName}.zip";
			if (File.Exists(destZipPath))
			{
				FileUtil.DeleteFileOrDirectory(destZipPath);
			}

			//压缩
			Functional.CreateZip(sourceZipPath, destZipPath);
		}

		/// <summary>
		/// 定期清理zip文件
		/// </summary>
		private static void ClearZip()
		{
			if (!_isManualBuild)
			{
				foreach (var item in Directory.GetDirectories(_publishPath))
				{
					if (item.Contains(_productType))
					{
						int saveDays;
						if (string.IsNullOrEmpty(_configFile.GetValue(_sectionSetting, "SaveDays")))
						{
							saveDays = 30;
						}
						else
						{
							saveDays = Convert.ToInt32(_configFile.GetValue(_sectionSetting, "SaveDays"));
						}

						string[] times = item.Split('_');
						string time = times[times.Length - 1];

						DateTime saveTime = DateTime.Now.AddDays(-saveDays);
						DateTime curTime = DateTime.Now.AddDays(-saveDays);
						try
						{
							curTime = DateTime.ParseExact(time, "yyyyMMddHHmmss", System.Globalization.CultureInfo.CurrentCulture);
						}
						catch (Exception e)
						{
							Debug.Log(e.ToString());
						}

						if (curTime < saveTime)
						{
							FileUtil.DeleteFileOrDirectory($"{_publishPath}/{item}");
						}
					}
				}
			}
		}

		#endregion 压缩

		#region 构建版本

		public static void BeforeBuildWindows()
		{
			CustomSetting();
			FileUtil.DeleteFileOrDirectory(_binPath);
			AssetDatabase.Refresh();
			Directory.CreateDirectory(_binPath);

			//清理
			Functional.ClearAllMod();
			ClearStreamingAssetsPath();

			if (!Directory.Exists(_publishPath))
			{
				Directory.CreateDirectory(_publishPath);
			}

			if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64)
			{
				EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
			}

			PlayerSettings.productName = string.IsNullOrEmpty(_productName) ? ProjectName : _productName;
			PlayerSettings.bundleVersion = _version;
			var versionPath = $"{Application.streamingAssetsPath}/version.json";
			File.WriteAllText(versionPath, "");

			//打包
			BuildWindows();
		}

		private static void CustomSetting()
		{
			var shadowDistance = _configFile.GetValue(_sectionUnity, "ShadowDistance");
			QualitySettings.shadowDistance = string.IsNullOrWhiteSpace(shadowDistance) ? 40 : Convert.ToInt32(shadowDistance);

			var skinWeight = _configFile.GetValue(_sectionUnity, "SkinWeights");
			QualitySettings.skinWeights = string.IsNullOrWhiteSpace(skinWeight) ? SkinWeights.TwoBones : (SkinWeights)Convert.ToInt32(skinWeight);

			PlayerSettings.SplashScreen.show = false;
		}

		private static async void BuildWindows()
		{
			await BuildRUPS();

			BuildModules();

			AfterBuildWindows();
		}

		private static async Task BuildRUPS()
		{
			bool isUpdated = Functional.UpdateManifest(_productType);
			if (isUpdated)
			{
				Client.Resolve();
				await Task.Delay(5000);
			}

			Debug.Log($"{_binPath}/{_productName}.exe");
			var buildset = new BuildPlayerOptions
			{
				scenes = new string[] { "Assets/Scenes/StartUp.unity" },
				locationPathName = $"{_binPath}/{_productName}.exe",
				target = BuildTarget.StandaloneWindows64,
				options = BuildOptions.None,
			};
			var buildreport = BuildPipeline.BuildPlayer(buildset);
			var summary = buildreport.summary;

			if (summary.result == BuildResult.Succeeded)
			{
				Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
			}

			if (summary.result == BuildResult.Failed)
			{
				Debug.Log("Build failed");
				return;
			}

			SetProductVersion(GetNewAutoVer("RUPS").ToString());
			DeleteBustFolder();
			//Config.ini
			CreateConfigINI();

			ZipRupsForUpload();
		}

		private static void BuildModules()
		{
			PackerModule();

			string commonPath, machinePath, gamePath;
			foreach (var item in _commonModArray)
			{
				if (string.IsNullOrWhiteSpace(item)) continue;
				commonPath = $"{_binPath}/PersistentData/{item.ToLower()}";
				if (DirectIsNull(commonPath))
				{
					Debug.LogEDITOR($"{item.ToLower()}未成功出包");
				}
			}

			foreach (var item in _productModArray)
			{
				if (string.IsNullOrWhiteSpace(item)) continue;
				machinePath = $"{_binPath}/PersistentData/{item.ToLower()}";
				if (DirectIsNull(machinePath))
				{
					Debug.LogEDITOR($"{item.ToLower()}未成功出包");
				}
			}

			foreach (var item in _gameModArray)
			{
				if (string.IsNullOrWhiteSpace(item)) continue;
				gamePath = $"{_binPath}/PersistentData/{item.ToLower()}";
				if (DirectIsNull(gamePath))
				{
					Debug.LogEDITOR($"{item.ToLower()}未成功出包");
				}
			}
		}

		private static void AfterBuildWindows()
		{
			//设置机器运行环境
			SetProductConf();

			AssetDatabase.Refresh();

			FullZip();

			//拷贝文件夹
			if (_isUSB)
			{
				Debug.Log("is usb, copy bin");
				try
				{
					CopyUSB();
				}
				catch (Exception e)
				{
					Debug.LogWarning(e.Message);
					throw;
				}
			}

			ProductTool.InitAdapterWriter();
			ProductTool.RemoveProductSDK(EditorOptions.ProductType);

			GitHelper.ClearRepo($"{_rupsPath}/{ProjectName}");
			AssetDatabase.Refresh();
		}

		private static int GetNewAutoVer(string name)
		{
			if (_versionJson != null)
			{
				var verjson = (IDictionary)_versionJson;
				if (verjson.Contains(name))
				{
					var ver = _versionJson[name];
					if (ver != null || string.IsNullOrWhiteSpace(ver.ToString()))
					{
						return Convert.ToInt32(ver.ToString()) + 1;
					}
				}
			}
			return 1;
		}

		/// <summary>
		/// 清理StreamingAssetsPath
		/// </summary>
		public static void ClearStreamingAssetsPath()
		{
			var folders = Directory.GetDirectories(Application.streamingAssetsPath);
			for (int i = 0; i < folders.Length; ++i)
			{
				var folder = folders[i];
				FileUtil.DeleteFileOrDirectory(folder);
			}

			var files = Directory.GetFiles(Application.streamingAssetsPath);
			for (int i = 0; i < files.Length; ++i)
			{
				var file = files[i];
				FileUtil.DeleteFileOrDirectory(file);
			}

			AssetDatabase.Refresh();
		}

		/// <summary>
		/// 设置机器运行环境
		/// </summary>
		private static void SetProductConf()
		{
			var machineName = GetMachineName();

			var path = $"{_binPath}/PersistentData/{App.SharedModule}/localconfig.ini";
			Debug.Log($"SetProductConf:{path}");
			IniFile iniFile = IniMgr.LoadIniFile(path);

			iniFile.SetValue("ConnectRobot", _productType, path);
			iniFile.SetValue("Robot_Type_Mouse", machineName, path);
			if (_serverType == ServerType.Release)
			{
				iniFile.SetValue("ServerType", "release", path);
			}
		}

		/// <summary>
		/// 设置版本信息
		/// </summary>
		private static void SetProductVersion(string autoVer)
		{
			var versionPath = $"{_binPath}/Platform_Data/StreamingAssets/version.json";
			JsonData jsonBuild = new JsonData();
			var changelogVer = "1.0.0.0";
			var changelogPath = $"{Application.dataPath}/Scripts/changelog.json";
			if (File.Exists(changelogPath))
			{
				JsonData jsonLog = JsonMapper.ToObject(File.ReadAllText(changelogPath));
				changelogVer = jsonLog["version"].ToString();
			}
			jsonBuild["rupsVer"] = changelogVer;

			if (string.IsNullOrWhiteSpace(autoVer))
			{
				autoVer = "1";
			}
			jsonBuild["productVer"] = _version;
			jsonBuild["productBuildVer"] = _buildVersion;
			jsonBuild["productMainVer"] = _productMainVersion;
			jsonBuild["autoVer"] = int.Parse(autoVer);
			var verJson = jsonBuild.ToJson();
			Debug.Log(verJson);
			File.WriteAllText(versionPath, verJson);
		}

		/// <summary>
		/// 删除Platform_BurstDebugInformation_DoNotShip目录
		/// </summary>
		private static void DeleteBustFolder()
		{
			var path = $"{_binPath}/Platform_BurstDebugInformation_DoNotShip";
			FileUtil.DeleteFileOrDirectory(path);
		}

		/// <summary>
		/// U盘打包（仅导出选中文件），拷贝文件
		/// </summary>
		private static void CopyUSB()
		{
			var usbPath = $"{PathUtil.GetDriverPath("RUPS")}/packages/{_productType}v{_version}";
			if (Directory.Exists(usbPath))
			{
				DeleteAll(usbPath);
				Directory.Delete(usbPath);
			}
			Directory.CreateDirectory(usbPath);
			CopyFolder(_binPath, usbPath);
		}

		/// <summary>
		/// 拷贝工具到
		/// </summary>
		private static void CopyToolsToBin(string tarPath)
		{
			var toolsPath = _configFile.GetValue(_sectionPath, "Tools").Split(',');
			foreach (var item in toolsPath)
			{
				var sourcePath = $"{_rupsPath}/{item.Replace('\\', '/')}";
				var strArr = sourcePath.Split('/');
				var folderName = strArr[strArr.Length - 1];
				var targetPath = $"{tarPath}/{folderName}";
				CopyFolder(sourcePath, targetPath);
			}
		}

		/// <summary>
		/// 生成资源清单
		/// </summary>
		/// <param name="dir"></param>
		public static FileInfo GenerateVersionInfo(string dir, string name)
		{
			PackageConfig versionProto = new PackageConfig();
			GenerateVersionProto(dir, versionProto, "");

			using (FileStream fileStream = new FileStream($"{dir}/{name}.txt", FileMode.Create))
			{
				StreamWriter sw = new StreamWriter(fileStream);
				foreach (var item in versionProto.FileInfoDict)
				{
					sw.Write($"{item.Key},{item.Value.MD5},{item.Value.Size}\n");
				}
				sw.Flush();
				//关闭写数据流
				sw.Close();

				return new FileInfo($"{dir}/{name}.txt");
			}
		}

		/// <summary>
		/// 写入资源包信息字典
		/// </summary>
		/// <param name="dir"></param>
		/// <param name="versionProto"></param>
		/// <param name="relativePath"></param>
		private static void GenerateVersionProto(string dir, PackageConfig versionProto, string relativePath)
		{
			var Files = Directory.GetFiles(dir);
			foreach (string file in Files)
			{
				FileInfo fi = new FileInfo(file);
				if (fi.Extension != ".meta")
				{
					string md5 = Util.FileMD5(file);
					long size = fi.Length;
					string filePath = relativePath == "" ? fi.Name : $"{relativePath}/{fi.Name}";
					versionProto.Size += size;
					versionProto.FileInfoDict.Add(filePath, new FileVersionInfo
					{
						File = filePath,
						MD5 = md5,
						Size = size,
					});
				}
			}

			foreach (string directory in Directory.GetDirectories(dir))
			{
				DirectoryInfo dinfo = new DirectoryInfo(directory);
				string rel = relativePath == "" ? dinfo.Name : $"{relativePath}/{dinfo.Name}";
				GenerateVersionProto($"{dir}/{dinfo.Name}", versionProto, rel);
			}
		}

		/// <summary>
		/// 创建Config.ini更新配置
		/// </summary>
		private static void CreateConfigINI()
		{
			using (FileStream fs = new FileStream($"{_binPath}/Config.ini", FileMode.Create))
			{
				StreamWriter sw = new StreamWriter(fs);
				if (_isUSB)
				{
					sw.Write($"OverwriteFolder=0");
				}
				else
				{
					sw.Write($"OverwriteFolder=1");
				}
				sw.Flush();
				sw.Close();
			}
		}

		/// <summary>
		/// 创建Zip包 MD5值
		/// </summary>
		/// <param name="zipPath"></param>
		/// <param name="fileName"></param>
		private static void CreateZipMD5(string zipPath, string fileName)
		{
			string md5 = Util.FileMD5($"{zipPath}/RUPS.zip");
			using (FileStream fs = new FileStream($"{zipPath}/{fileName}", FileMode.Create))
			{
				StreamWriter sw = new StreamWriter(fs);
				sw.Write($"{md5}");
				sw.Flush();
				sw.Close();
			}
		}

		#endregion 构建版本

		#region Git&Command

		/// <summary>
		/// 拉取并签出指定分支
		/// </summary>
		/// <param name="repoPath"></param>
		/// <param name="branch"></param>
		private static void ExecuteGitCmd(string repoPath, string branch, string repoFullName, bool isPlatform = false)
		{
			Debug.Log($"ExecuteGitCmd {repoPath},{branch},{repoFullName}");
			if (!FileHelper.IsDirectoryExist(repoPath))
			{
				GitHelper.Clone($"{Gitea.ServerUrl}/{repoFullName}.git", repoPath);
			}
			GitHelper.PrepareRepo(repoPath, branch);
			if (isPlatform)
			{
				GitHelper.UpdateSubmodule(repoPath);
				AssetDatabase.Refresh();
			}
		}

		/// <summary>
		/// 打包模块并清理
		/// </summary>
		/// <param name="fromPath"></param>
		/// <param name="toPath"></param>
		/// <param name="modName"></param>
		/// <param name="removeLink"></param>
		private static void PackerAndClear(string fromPath, string toPath, string modName, bool removeLink = true)
		{
			Functional.CopyMod(fromPath, toPath);
			AssetDatabase.Refresh();

			Packer.Pack(_buildVersion, Packer.PlatformType.Windows, modName, _rupsPath, $"{_binPath}/PersistentData/{modName.ToLower()}");
			AssetDatabase.Refresh();

			if (removeLink)
			{
				Functional.RemoveMod(toPath);
				AssetDatabase.Refresh();
			}

			GitHelper.ClearRepo(fromPath);
		}

		private static void PullRups()
		{
			if (!_isManualBuild)
			{
				ExecuteGitCmd($"{_rupsPath}/{ProjectName}", _platformGit, Gitea.PlatformRepoFullName, true);
			}

			//Common
			ExecuteGitCmd($"{_rupsPath}/SM/Common", _commonGit, Gitea.CommonRepoFullName);
			//Games
			for (int i = 0; i < _gameModArray.Length; i++)
			{
				if (string.IsNullOrWhiteSpace(_gameModArray[i])) continue;

				string customGameGit = _configFile.GetValue(_sectionModule, $"GamesGit_{_gameModArray[i]}");
				if (string.IsNullOrEmpty(customGameGit))
				{
					ExecuteGitCmd($"{_gamesSrcPath}/{App.ModNamePrefix}{_gameModArray[i]}", _gamesGit, $"{Gitea.GameRepoFullNamePrefix}/{App.ModNamePrefix}{_gameModArray[i]}");
				}
				else
				{
					ExecuteGitCmd($"{_gamesSrcPath}/{App.ModNamePrefix}{_gameModArray[i]}", customGameGit, $"{Gitea.GameRepoFullNamePrefix}/{App.ModNamePrefix}{_gameModArray[i]}");
				}
			}
		}

		/// <summary>
		/// 拉取产品
		/// </summary>
		private static void PullProduct(string branch)
		{
			ExecuteGitCmd($"{_productSrcPath}", branch, $"{Gitea.ProductRepoFullNamePrefix}/{_productType}");
		}

		/// <summary>
		/// 拉取最新的Common Machine Games
		/// </summary>
		private static void PackerModule()
		{
			string dataPath = Application.dataPath.Replace("\\", "/");
			string fromPath;
			string toPath;

			//Packer SharedModule
			fromPath = _commonSrcPath + $"/{App.ModNamePrefix}{App.SharedModule}";
			toPath = $"{dataPath}/{App.ModNamePrefix}{App.SharedModule}";
			PackerAndClear(fromPath, toPath, App.SharedModule, false);
			//Packer Common
			for (int i = 0; i < _commonModArray.Length; i++)
			{
				if (string.IsNullOrWhiteSpace(_commonModArray[i])) continue;
				if (!_commonModArray[i].Contains(App.SharedModule))
				{
					fromPath = $"{_commonSrcPath}/{App.ModNamePrefix}{_commonModArray[i]}";
					toPath = $"{dataPath}/{App.ModNamePrefix}{_commonModArray[i]}";
					PackerAndClear(fromPath, toPath, _commonModArray[i]);
				}
			}

			//Packer Machine
			for (int i = 0; i < _productModArray.Length; i++)
			{
				if (string.IsNullOrWhiteSpace(_productModArray[i])) continue;
				fromPath = $"{_productSrcPath}/Module/{App.ModNamePrefix}{_productModArray[i]}";
				toPath = $"{dataPath}/{App.ModNamePrefix}{_productModArray[i]}";
				PackerAndClear(fromPath, toPath, _productModArray[i]);
			}

			//Packer Games
			for (int i = 0; i < _gameModArray.Length; i++)
			{
				if (string.IsNullOrWhiteSpace(_gameModArray[i])) continue;
				fromPath = $"{_gamesSrcPath}/{App.ModNamePrefix}{_gameModArray[i]}";
				toPath = $"{dataPath}/{App.ModNamePrefix}{_gameModArray[i]}";
				PackerAndClear(fromPath, toPath, _gameModArray[i]);
			}

			//Remove {App.ModNamePrefix}Shared
			toPath = $"{dataPath}/{App.ModNamePrefix}{App.SharedModule}";
			Functional.RemoveMod(toPath);
			AssetDatabase.Refresh();

			Functional.ClearAllMod();
		}

		private static void CommandLineBuildWindows(string productType, bool isBuildTest = false)
		{
			_productType = productType;
			_isManualBuild = false;
			_isBuildTest = isBuildTest;

			InitPath();
			ReadConfig();
			PullProduct(_productGit);
			PullRups();

			ProductTool.InitAdapterWriter();
			ProductTool.ImportProductSDK(EditorOptions.ProductType);
		}

		public static void CommandLineBuild()
		{
			string[] args = System.Environment.GetCommandLineArgs();
			int paramIndex = -1;
			foreach (var arg in args)
			{
				paramIndex++;
				if (arg.Contains("-p:"))
				{
					break;
				}
			}

			var productType = args[paramIndex + 1];
			if (!string.IsNullOrWhiteSpace(productType))
			{
				CommandLineBuildWindows(productType);
			}
		}

		public static void CommandLineBuildAndroid()
		{
			string path = PathUtil.GetRupsPath() + "/Android";
			DeleteAll(path);

			if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
			{
				EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
			}
			BuildPipeline.BuildPlayer(GetBuildScenes(), GetAndroidPath(), BuildTarget.Android, BuildOptions.None);
		}

		public static void CommandLineBuildTest()
		{
			CommandLineBuildWindows("M2P", true);
		}

		public static string GetAndroidPath()
		{
			string androidPath = PathUtil.GetRupsPath();
			androidPath += $"/Android/{ProjectName}.apk";
			return androidPath;
		}

		#endregion Git&Command

		#region 工具方法

		public static string[] GetBuildScenes()
		{
			List<string> names = new List<string>();

			foreach (EditorBuildSettingsScene e in EditorBuildSettings.scenes)
			{
				if (e == null)
					continue;
				if (e.enabled)
				{
					names.Add(e.path);
				}
			}
			return names.ToArray();
		}

		/// <summary>
		/// 文件夹是否为空
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private static bool DirectIsNull(string path)
		{
			var result = true;
			string[] directionName = Directory.GetDirectories(path);
			if (directionName.Length > 0) result = false;
			return result;
		}

		public void OpenDirectory()
		{
			OpenDialogDir ofn2 = new OpenDialogDir();
			ofn2.pszDisplayName = new string(new char[2000]);     // 存放目录路径缓冲区  
			ofn2.lpszTitle = "选择保存路径";// 标题  

			IntPtr pidlPtr = DllOpenFileDialog.SHBrowseForFolder(ofn2);

			char[] charArray = new char[2000];

			DllOpenFileDialog.SHGetPathFromIDList(pidlPtr, charArray);
			string fullDirPath = new string(charArray);
			fullDirPath = fullDirPath.Substring(0, fullDirPath.IndexOf('\0'));
			_binPath = fullDirPath;
			UnityEngine.Debug.Log(fullDirPath);
		}

		private static void DeleteAll(string path)
		{
			if (!Directory.Exists(path))
			{
				return;
			}

			foreach (var file in Directory.GetFiles(path))
			{
				File.Delete(file);
			}

			foreach (var subDir in Directory.GetDirectories(path))
			{
				DeleteAll(subDir);
				Directory.Delete(subDir);
			}
		}

		/// <summary>
		/// 拷贝文件夹
		/// </summary>
		/// <param name="sourcePath"></param>
		/// <param name="targetPath"></param>
		private static void CopyFolder(string sourcePath, string targetPath)
		{
			if (!Directory.Exists(sourcePath))
			{
				Directory.CreateDirectory(sourcePath);
			}
			if (!Directory.Exists(targetPath))
			{
				Directory.CreateDirectory(targetPath);
			}

			CopyFile(sourcePath, targetPath);
			string[] directionName = Directory.GetDirectories(sourcePath);
			foreach (string dirPath in directionName)
			{
				string directionPathTemp = targetPath + "\\" + dirPath.Substring(sourcePath.Length + 1);
				CopyFolder(dirPath, directionPathTemp);
			}
		}

		/// <summary>
		/// 拷贝文件
		/// </summary>
		/// <param name="sourcePath"></param>
		/// <param name="targetPath"></param>
		private static void CopyFile(string sourcePath, string targetPath)
		{
			string[] filesList = Directory.GetFiles(sourcePath);
			foreach (string f in filesList)
			{
				string fTarPath = targetPath + "\\" + f.Substring(sourcePath.Length + 1);
				if (File.Exists(fTarPath))
				{
					File.Copy(f, fTarPath, true);
				}
				else
				{
					File.Copy(f, fTarPath);
				}
			}
		}

		#endregion 工具方法
	}
}