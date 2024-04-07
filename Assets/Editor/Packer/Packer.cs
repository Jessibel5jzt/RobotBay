using LitJson;
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace WestBay
{
	/// <summary>
	/// 模块打包工具
	/// </summary>
	public class Packer : EditorWindow
	{
		public enum PlatformType
		{
			Windows,
			Android,
			Linux,
			iOS,
		}

		public enum BuildType
		{
			Development,
			Release,
			BeforeRelease
		}

		/// <summary>
		/// AssetBundle文件后缀
		/// </summary>
		private const string _abPostFix = "ab";

		/// <summary>
		/// 忽略标记的文件夹：
		/// </summary>
		private static readonly HashSet<string> TagIgnoreFolder = new HashSet<string>(new string[]{
			"Scripts",".vs",".git"
		});

		/// <summary>
		/// 忽略标记的文件：
		/// </summary>
		private static readonly HashSet<string> TagIgnoreFiles = new HashSet<string>(new string[]{
			".meta", ".cs", ".ini", ".tpsheet", ".db", ".mp4", ".ogv",
			"ico.png", "intro.png", "robotpic",".csv",".json"
		});

		/// <summary>
		/// 忽略标记的包含的字段的文件：
		/// </summary>
		private static readonly string[] TagIgnoreFileNames = new string[]{
			"robotpic"
		};

		/// <summary>
		/// 原名拷贝的文件：
		/// </summary>
		private static readonly HashSet<string> CopyFileExtensions = new HashSet<string>(new string[]{
			".ini", ".mp4",".ogv", ".ogg",
			"ico.png", "intro.png", ".csv",".json"
		});

		/// <summary>
		/// 包含拷贝资源名称列表
		/// </summary>
		private static readonly string[] CopyFileNames = new string[]{
			"robotpic"
		};

		/// <summary>
		/// 忽略拷贝目录
		/// </summary>
		private static readonly HashSet<string> CopyIgnoreFolder = new HashSet<string>(new string[]{
			"Scripts",".vs",".git"
		});

		/// <summary>
		/// 需要记录MD5的类型
		/// </summary>
		private static readonly HashSet<string> MD5FileExtensions = new HashSet<string>(new string[] {
			$".{_abPostFix}",
			".ini",".mp4", ".mp3", ".ogv", ".ogg", ".manifest",".csv",".json"
		});

		/// <summary>
		///  包含生成MD5资源名称列表
		/// </summary>
		private static readonly string[] MD5FileNames = new string[]{
			"robotpic"
		};

		#region 文件夹打包

		private static HashSet<string> CopyFoldersIgnoreFiles = new HashSet<string>(new string[] {
			".md","LICENSE",".meta"
		});

		/// <summary>
		/// 原名拷贝的目录
		/// </summary>
		private static HashSet<string> CopyFolders = new HashSet<string>(new string[] {
		});

		/// <summary>
		/// 生成MD5的目录
		/// </summary>
		private static HashSet<string> MD5Folders = new HashSet<string>(new string[] {
		});

		#endregion 文件夹打包

#if UNITY_STANDALONE_WIN
		private static PlatformType _platType = PlatformType.Windows;
#elif UNITY_STANDALONE_OSX
		private static PlatformType _platType = PlatformType.iOS;
#elif UNITY_ANDROID
		private static PlatformType _platType = PlatformType.Android;
#elif UNITY_IOS
		private static PlatformType _platType = PlatformType.iOS;
#else
		private static PlatformType _platType = PlatformType.Windows;
#endif

		private static BuildType _buildType = BuildType.Development;
		private static BuildAssetBundleOptions _buildAssetBundleOptions = BuildAssetBundleOptions.None;
		private static BuildTarget _buildTarget = BuildTarget.StandaloneWindows64;

		private static int AutoVersion = 0;
		private static JsonData AutoVerJson;
		private static string RupsVer = "";
		private static string ProductBuildVer = "";

		// 目录结构固定如下：
		private static string RupsRoot = "";              // x:/.../Rups

		private static string ModNameLower = "";     //{modulex} ( 如: platform, login)
		private static string AssetPlatformOrModule = "";             // Assets/module|platform

		private static string PackageAB = "";           // {RupsRoot}/PackagedAB/{PLATFORM}/{modulex}
		private static string ModuleRoot = "";          // {RupsRoot}/SM/{SMTYPE}/{ModuleX}
		private static string ModuleAssets = "";          // {ModuleRoot}/Assets
		private static string ModuleResPath = "";       // {ModuleRoot}/{AssetPlatformOrModule}
		private static string ModuleResVersionFilePath = "";       // {ModuleRoot}/{AssetPlatformOrModule}/version.json
		private static string OutputSAPath = "";        // {ModuleAssets}/StreamingAssets/{ModuleX}

		private static string SMDllPath = ""; // {ModuleResPath}/Codes
		private static string SMScriptsPath = ""; // {ModuleResPath}/Scripts

		//当前模块列表
		private static string[] ModNames = null;

		private static int _selectModIndex = 0;

		//当前设备列表
		private static string[] ProductNames = null;

		private static int _selectProductIndex = 0;

		// 是否保存ab包至发布目录
		private static bool IsRelease = false;

		private static bool IsPlatform = false;

		#region Log

		private static void Log(string log)
		{
			UnityEngine.Debug.Log("<color=blue>[PACKER]</color>" + log);
		}

		private static void LogWarning(string log)
		{
			UnityEngine.Debug.LogWarning("<color=yellow>[PACKER]</color>" + log);
		}

		private static void LogError(string log)
		{
			UnityEngine.Debug.LogError("<color=red>[PACKER]</color>" + log);
		}

		#endregion Log

		#region 初始化路径

		/// <summary>
		/// 获取子子模块列表
		/// </summary>
		/// <returns></returns>
		private static string[] GetModuleNames()
		{
			List<string> result = new List<string>();

			string projPathAsserts = Application.dataPath.Replace("\\", "/");
			var pathNames = Directory.GetDirectories(projPathAsserts);
			for (var i = 0; i < pathNames.Length; ++i)
			{
				string path = pathNames[i].Replace("\\", "/");
				string folderName = path.Substring(path.LastIndexOf('/') + 1);
				if (folderName.StartsWith(App.ModNamePrefix))
				{
					result.Add(folderName.Substring(App.ModNamePrefix.Length));
				}
			}

			return result.ToArray();
		}

		/// <summary>
		/// 初始化路径
		/// </summary>
		public static void InitPath()
		{
			if (ModNames == null || ModNames.Length == 0)
			{
				ModNames = GetModuleNames();
				if (ModNames.Length == 0) return;
			}

			var modName = ModNames[_selectModIndex];
			InitPath(modName);
		}

		/// <summary>
		/// 初始化路径
		/// </summary>
		/// <param name="moduleName"></param>
		/// <param name="path"></param>
		public static void InitPath(string moduleName, string path = null)
		{
			string projPathAsserts = Application.dataPath.Replace("\\", "/");
			string projPath = projPathAsserts.Substring(0, projPathAsserts.LastIndexOf('/'));

			ModNameLower = moduleName.ToLower();
			IsPlatform = moduleName.Equals(App.SharedModule, StringComparison.Ordinal) == true;

			var RupsModuleFolder = IsPlatform ? $"{App.ModNamePrefix}{App.SharedModule}" : $"{App.ModNamePrefix}{moduleName}";
			RupsRoot = string.IsNullOrEmpty(path) ? PathUtil.GetRupsPath() : path;
			AssetPlatformOrModule = $"Assets/{RupsModuleFolder}";
			PackageAB = $"{RupsRoot}/PackagedAB/{_platType}/{ModNameLower}";

			if (ProductNames == null || ProductNames.Length == 0)
			{
				ProductNames = GetMachineNames();
			}
			ModuleRoot = projPath;
			ModuleAssets = projPathAsserts;
			ModuleResPath = $"{ModuleRoot}/{AssetPlatformOrModule}";
			ModuleResVersionFilePath = $"{ModuleResPath}/version.json";
			OutputSAPath = $"{PathUtil.GetPersistPath()}/{ModNameLower}";//$"{projPathAsserts}/StreamingAssets/{ModNameLower}";

			SMDllPath = $"{ModuleResPath}/Codes";
			SMScriptsPath = $"{ModuleResPath}/Scripts";

			if (IsRelease)
			{
				RupsVer = GetRupsVersionFromLog();
				var productName = ProductNames[_selectProductIndex];
				var curIniFile = IniMgr.LoadIniFile($"{PathUtil.GetRupsPath()}/Product/{productName}/Doc/发布文档/{productName}.ini");
				if (curIniFile != null)
				{
					ProductBuildVer = curIniFile.GetValue("BuildVersion");
				}

				var verJsonPath = $"{RupsRoot}/doopversion.json";
				if (File.Exists(verJsonPath))
				{
					AutoVerJson = JsonMapper.ToObject(File.ReadAllText(verJsonPath));
					AutoVersion = GetAutoVer(moduleName.ToLower());
				}
				else
				{
					Debug.LogEDITOR("未找到本地版本文件doopversion.json，切换回开发模式");
					SetBuildType(BuildType.Development);
				}
			}
		}

		#endregion 初始化路径

		public static void SetPlatformType(PlatformType pt)
		{
			_platType = pt;

			switch (_platType)
			{
				case PlatformType.Windows:
					_buildTarget = BuildTarget.StandaloneWindows64;
					break;

				case PlatformType.Android:
					_buildTarget = BuildTarget.Android;
					break;

				case PlatformType.iOS:
					_buildTarget = BuildTarget.iOS;
					break;

				case PlatformType.Linux:
					_buildTarget = BuildTarget.StandaloneLinux64;
					break;

				default:
					_buildTarget = BuildTarget.StandaloneWindows64;
					break;
			}
		}

		public static void SetBuildType(BuildType bt)
		{
			_buildType = bt;
			switch (_buildType)
			{
				case BuildType.Development:
					IsRelease = false;
					break;

				case BuildType.Release:
					IsRelease = true;
					break;
			}
		}

		private static List<ToggleClass> _moduleTogClass;
		private static float _offsetX;
		private static float _offsetY;
		private static Vector2 _commonScrollView;

		#region 工具界面

		[MenuItem("Tools/打包工具", false, 0)]
		public static void ShowWindow()
		{
			IsPacking = false;
			ModNames = GetModuleNames();

			_moduleTogClass = new List<ToggleClass>();
			for (int i = 0; i < ModNames.Length; i++)
			{
				_moduleTogClass.Add(new ToggleClass(false, ModNames[i]));
			}
			_offsetX = 10;
			_offsetY = 220;
			_commonScrollView = new Vector2();

			GetWindowWithRect(typeof(Packer), new Rect(0, 0, 500, 450), false, "打包工具ModulePacker");
		}

		private static string[] GetMachineNames()
		{
			List<string> result = new List<string>();

			var dirs = Directory.GetDirectories($"{RupsRoot}/Product");
			foreach (var item in dirs)
			{
				var di = new DirectoryInfo(item);
				result.Add(di.Name);
			}
			return result.ToArray();
		}

		private void OnGUI()
		{
			var ctrlH = GUILayout.Height(30);
			var dropDownW = GUILayout.Width(495);
			_platType = (PlatformType)EditorGUILayout.EnumPopup("平台：", _platType, dropDownW);
			SetPlatformType(_platType);

			if (!IsPacking)
			{
				InitPath();
			}

			if (ModNames != null && ModNames.Length != 0)
			{
				EditorGUILayout.Space(4);
				_buildType = string.IsNullOrWhiteSpace(ProductBuildVer) ? BuildType.Development : (BuildType)EditorGUILayout.EnumPopup("发布：", _buildType, dropDownW);
				SetBuildType(_buildType);

				if (IsRelease)
				{
					EditorGUILayout.Space(4);
					_selectProductIndex = EditorGUILayout.Popup("设备：", _selectProductIndex, ProductNames);
				}

				EditorGUILayout.Space(4);
				_selectModIndex = EditorGUILayout.Popup("模块：", _selectModIndex, ModNames);

				EditorGUILayout.Space();

				GUILayout.Label($"Assets： {ModuleAssets}");
				GUILayout.Label($"输出路径： Assets/StreamingAssets/{ModNameLower}");
				if (IsRelease)
				{
					var ShowPABSA = PackageAB.Substring(RupsRoot.Length - 2);
					GUILayout.Label($"输出路径： {ShowPABSA}");
					GUILayout.Label($"RupsVer： {RupsVer}");
					GUILayout.Label($"ProductBuildVer： {ProductBuildVer}");
					GUILayout.Label($"AutoVer： {AutoVersion}");
				}

				EditorGUILayout.Space();
				GUILayout.BeginHorizontal();
				if (CheckPlatformLink())
				{
					if (GUILayout.Button($"删除并重新打包 [{_platType}]", GUILayout.Width(200), ctrlH))
					{
						if (IsRelease)
						{
							PackRes(null, PackageAB);
						}
						else
						{
							PackRes();
						}
					}

					if (!IsRelease)
					{
						EditorGUILayout.Space();
						if (GUILayout.Button($"重新打包脚本 [仅用于开发]", GUILayout.Width(200), ctrlH))
						{
							PackCode();
						}
					}
				}
				else
				{
					if (GUILayout.Button($"链接子模块", GUILayout.Width(400), ctrlH))
					{
						DevToolMenu.LinkSM();
					}
				}

				EditorGUILayout.Space();
				try { GUILayout.EndHorizontal(); } catch (Exception) { throw; }
			}

			//if (CheckPlatform() && !CheckPlatformLink())
			//{
			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();

			GUIStyle style = new GUIStyle();
			style.fontSize = 14;
			style.normal.textColor = new Color(256f / 256f, 0f / 256f, 0f / 256f, 256f / 256f);

			GUILayout.Label("※请链接Mod_Platform！", style);
			//}

			EditorGUILayout.Space();
			EditorGUILayout.Space();
			EditorGUILayout.Space();

			GUILayout.Label("工具说明：");
			GUILayout.Label("一、平台需选择为打包目标平台，打包资源可以不切换unity平台");
			if (IsRelease)
			{
				GUILayout.Label("二、发布设置：当前为【发布模式】，打包时会携带完整的版本信息。");
			}
			else
			{
				GUILayout.Label("二、发布设置：当前为【开发模式】，打包时不会携带版本信息。");
			}

			EditorGUILayout.Space();

			//刷新多选界面
			RfeshMultiplePanel();
		}

		private static void RfeshMultiplePanel()
		{
			if (CheckPlatform() && !CheckPlatformLink()) return;
			if (_moduleTogClass == null || _moduleTogClass.Count == 0) return;

			float height = Mathf.Clamp(20 * ModNames.Length, 10, 160);
			for (int i = 0; i < ModNames.Length; i++)
			{
				if (i == 0)
				{
					GUILayout.BeginArea(new Rect(_offsetX, _offsetY, 150, height));
					_commonScrollView = GUILayout.BeginScrollView(_commonScrollView, GUILayout.Width(150), GUILayout.Height(height));
				}
				else
				{
					if (i % 8 == 0)
					{
						GUILayout.EndScrollView();
						GUILayout.EndArea();
						GUILayout.BeginArea(new Rect(_offsetX + (100 * (i / 8)), _offsetY, 150, height));
						_commonScrollView = GUILayout.BeginScrollView(_commonScrollView, GUILayout.Width(150), GUILayout.Height(height));
					}
				}
				_moduleTogClass[i].IsSelect = GUILayout.Toggle(_moduleTogClass[i].IsSelect, _moduleTogClass[i].Name, GUILayout.Width(100));
			}
			GUILayout.EndScrollView();
			GUILayout.EndArea();

			GUILayout.BeginArea(new Rect(_offsetX, _offsetY + height, 450, 50));
			if (GUILayout.Button($"多选打包", GUILayout.Width(450), GUILayout.Height(30)))
			{
				bool isEmpty = true;
				for (int i = 0; i < _moduleTogClass.Count; i++)
				{
					if (_moduleTogClass[i].IsSelect)
					{
						isEmpty = false;
						InitPath(_moduleTogClass[i].Name);
						PackRes();
					}
				}
				if (isEmpty)
				{
					Debug.Log("请勾选模块");
				}
			}
			GUILayout.EndArea();
		}

		#endregion 工具界面

		#region 打包

		private static AssetBundleBuild[] GetModuleAssetBundleBuild()
		{
			var names = AssetDatabase.GetAllAssetBundleNames();
			var Ends = $"{ModNameLower}.{_abPostFix}";
			List<AssetBundleBuild> abbList = new List<AssetBundleBuild>();
			foreach (var name in names)
			{
				// 只当前模块的
				if (!name.EndsWith(Ends)) continue;
				//Debug.Log(N);

				string Path = ABName2Path(name);
				if (Path == null)
				{
					Debug.LogError("NO ABName2Path");
					continue;
				}

				string ABName;
				string Var;
				ABName2Variant(name, out ABName, out Var);
				if (ABName == null)
				{
					Debug.LogError("NO ABName ABName2Variant");
					continue;
				}

				if (Var == null)
				{
					Debug.LogError("NO Var ABName2Variant");
					continue;
				}

				abbList.Add(new AssetBundleBuild
				{
					assetBundleName = ABName,
					assetBundleVariant = Var,
					assetNames = new string[] { $"{AssetPlatformOrModule}/{Path}" }
				}
				);
			}

			return abbList.ToArray();
		}

		private static bool IsPacking = false;

		/// <summary>
		/// 打包
		/// </summary>
		public static void PackRes(Action onCompleted = null, string outPutPath = "")
		{
			if (string.IsNullOrEmpty(ModNameLower))
			{
				Log($"打包失败：ModNameLower 为空");
				return;
			}

			outPutPath = string.IsNullOrWhiteSpace(outPutPath) ? OutputSAPath : outPutPath;
			ClearFolder(outPutPath);
			Log($"打包开始：{_platType} {ModNameLower}");
			IsPacking = true;
			if (!CompileCode()) return;
			Log($"生成代码 {ModNameLower}.bytes");
			//必须刷新
			AssetDatabase.Refresh();

			Log($"标记资源");
			ResetABLabels();
			//必须刷新
			AssetDatabase.Refresh();

			Log("开始打包……");
			CopyFiles(ModuleResPath, outPutPath);
			CopyFolder(ModuleResPath, outPutPath);
			var ABB = GetModuleAssetBundleBuild();
			BuildPipeline.BuildAssetBundles(outPutPath, ABB, _buildAssetBundleOptions, _buildTarget);
			if (_buildType == BuildType.Release || _buildTarget == BuildTarget.Android)
			{
				var tarPath = new DirectoryInfo(outPutPath).FullName;
				GenerateVersionInfo(tarPath);
				//创建版本信息
				GenerateVersionFile(AutoVersion, tarPath);
			}

			if (_buildType == BuildType.BeforeRelease)
			{
				GenerateVersionInfo(new DirectoryInfo(outPutPath).FullName);
			}
			Log($"打包完成");

			//必须刷新
			AssetDatabase.Refresh();

			Log("打包结束");
			IsPacking = false;
			onCompleted?.Invoke();
		}

		/// <summary>
		/// 打包代码
		/// </summary>
		/// <returns></returns>
		private static bool CompileCode()
		{
			if (!Directory.Exists(SMScriptsPath))
			{
				Log(SMScriptsPath + "目录不存在");
				return false;
			}

			Directory.CreateDirectory(SMDllPath);

			CompilerParameters Cp = new CompilerParameters();
			Cp.GenerateExecutable = false;
			Cp.GenerateInMemory = false;
			Cp.IncludeDebugInformation = false;
			Cp.OutputAssembly = $"{SMDllPath}/{ModNameLower}.bytes";
			Cp.TreatWarningsAsErrors = false;
			Cp.CompilerOptions = $"/optimize /unsafe";
			Cp.ReferencedAssemblies.AddRange(GetLibs());
			Cp.WarningLevel = 4;

			CodeDomProvider Pvdr = CodeDomProvider.CreateProvider("CSharp");
			var Ret = Pvdr.CompileAssemblyFromFile(Cp, GetCsFiles());
			int ErrorCnt = 0;

			if (Ret.Errors.Count > 0)
			{
				foreach (CompilerError E in Ret.Errors)
				{
					var FileName = "";
					if (!string.IsNullOrEmpty(E.FileName))
					{
						FileName = E.FileName.Substring(SMScriptsPath.Length + 1);
					}

					var ErrStr = $"({E.ErrorNumber}@{FileName}:{E.Line}) {E.ErrorText}";
					if (E.IsWarning)
					{
						LogWarning(ErrStr);
					}
					else
					{
						LogError(ErrStr);
						ErrorCnt++;
					}
				}
			}
			else
			{
				Log("Submodule scripts compilation succeeded.");
			}

			return ErrorCnt == 0;
		}

		/// <summary>
		/// 获取所有源代码
		/// </summary>
		/// <returns></returns>
		private static string[] GetCsFiles()
		{
			return Directory.GetFiles(SMScriptsPath, "*.cs", SearchOption.AllDirectories);
		}

		#region 仅打包代码

		/// <summary>
		/// 打包代码
		/// </summary>
		public static void PackCode()
		{
			if (string.IsNullOrEmpty(ModNameLower))
			{
				Log($"打包失败：ModNameLower 为空");
				return;
			}

			//ClearFolder();
			Log($"打包开始：{_platType}");

			if (!CompileCode()) return;
			Log($"生成代码 {ModNameLower}.bytes");
			//必须刷新
			AssetDatabase.Refresh();

			CreateFolder(OutputSAPath);
			var ABB = GetModuleCodeBundleBuild();
			BuildPipeline.BuildAssetBundles(OutputSAPath, ABB, _buildAssetBundleOptions, _buildTarget);

			if (IsRelease)
			{
				string codesPath = PackageAB + "/codes";
				DeleteFolder(codesPath);
				FileHelper.CopyDirectory(OutputSAPath + "/codes", codesPath);
				Log($"同步资源到发布：{PackageAB}");
			}
		}

		private static AssetBundleBuild[] GetModuleCodeBundleBuild()
		{
			var Names = AssetDatabase.GetAllAssetBundleNames();
			var Ends = $"{ModNameLower}.{_abPostFix}";
			List<AssetBundleBuild> AllABB = new List<AssetBundleBuild>();
			foreach (var N in Names)
			{
				// 只当前模块的
				if (!N.EndsWith(Ends)) continue;
				//Debug.Log(N);

				if (!N.Contains("bytes")) continue;

				string Path = ABName2Path(N);
				if (Path == null)
				{
					Debug.LogError("NO ABName2Path");
					continue;
				}
				string ABName;
				string Var;
				ABName2Variant(N, out ABName, out Var);
				if (ABName == null)
				{
					Debug.LogError("NO ABName ABName2Variant");
					continue;
				}
				if (Var == null)
				{
					Debug.LogError("NO Var ABName2Variant");
					continue;
				}

				AllABB.Add(new AssetBundleBuild
				{
					assetBundleName = ABName,
					assetBundleVariant = Var,
					assetNames = new string[] { $"{AssetPlatformOrModule}/{Path}" }
				}
				);
			}

			return AllABB.ToArray();
		}

		#endregion 仅打包代码

		#endregion 打包

		#region 清单文件

		/// <summary>
		/// 生成资源清单
		/// </summary>
		/// <param name="dir"></param>
		private static FileInfo GenerateVersionInfo(string dir)
		{
			PackageConfig versionProto = new PackageConfig();
			GenerateVersionProto(dir, versionProto, "");
			GenerateVersionProtoFolder(dir, versionProto, "");

			using (FileStream fileStream = new FileStream($"{dir}/{ModNameLower}.txt", FileMode.Create))
			{
				StreamWriter sw = new StreamWriter(fileStream);
				foreach (var item in versionProto.FileInfoDict)
				{
					sw.Write($"{item.Key},{item.Value.MD5},{item.Value.Size}\n");
				}
				sw.Flush();
				sw.Close();

				return new FileInfo($"{dir}/{ModNameLower}.txt");
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
			//如果是直接复制目录
			if (IsCopyFolder(dir)) return;

			var files = Directory.GetFiles(dir);
			foreach (string file in files)
			{
				FileInfo fi = new FileInfo(file);
				if (fi.Name.Equals($"{ModNameLower}.manifest")) continue;

				if (MD5FileExtensions.Contains(fi.Extension)
					|| MD5FileExtensions.Contains(fi.Name))
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

				for (int i = 0; i < MD5FileNames.Length; i++)
				{
					if (fi.Name.Contains(MD5FileNames[i]))
					{
						if (TagIgnoreFiles.Contains(fi.Extension)) continue;

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
			}

			foreach (string directory in Directory.GetDirectories(dir))
			{
				DirectoryInfo dinfo = new DirectoryInfo(directory);
				string rel = relativePath == "" ? dinfo.Name : $"{relativePath}/{dinfo.Name}";
				GenerateVersionProto($"{dir}/{dinfo.Name}", versionProto, rel);
			}
		}

		/// <summary>
		/// 创建模块版本信息
		/// </summary>
		/// <param name="rupsVer"></param>
		/// <param name="autoVer"></param>
		private static void GenerateVersionFile(int autoVer, string outPutPath)
		{
			JsonData jsonAutoVer = new JsonData
			{
				["autoVer"] = autoVer.ToString()
			};
			var buildFilePath = $"{outPutPath}/version.json";
			File.WriteAllText(ModuleResVersionFilePath, jsonAutoVer.ToJson());
			JsonData jsonBuild = new JsonData
			{
				["rupsVer"] = RupsVer,
				["productBuildVer"] = ProductBuildVer,
				["autoVer"] = autoVer.ToString()
			};

			//如果模块有Log
			var changelogPath = $"{SMScriptsPath}/changelog.json";
			if (File.Exists(changelogPath))
			{
				JsonData jsonLog = JsonMapper.ToObject(File.ReadAllText(changelogPath));
				jsonBuild["changelogVer"] = jsonLog["version"].ToString();
			}

			File.WriteAllText(buildFilePath, jsonBuild.ToJson());
		}

		/// <summary>
		/// 获取Rups版本
		/// </summary>
		/// <returns></returns>
		private static string GetRupsVersionFromLog()
		{
			string result = "";
			var changelogPath = $"{Application.dataPath}/Scripts/changelog.json";
			if (File.Exists(changelogPath))
			{
				JsonData jsonLog = JsonMapper.ToObject(File.ReadAllText(changelogPath));
				result = jsonLog["version"].ToString();
			}

			return result;
		}

		private static int GetAutoVer(string name)
		{
			if (AutoVerJson != null)
			{
				var verjson = (IDictionary)AutoVerJson;
				if (verjson.Contains(name))
				{
					var ver = AutoVerJson[name];
					if (ver != null || string.IsNullOrWhiteSpace(ver.ToString()))
					{
						return Convert.ToInt32(ver.ToString()) + 1;
					}
				}
			}
			return 1;
		}

		/// <summary>
		/// 获取模块版本
		/// </summary>
		/// <returns></returns>
		private static int GetModAutoVersion()
		{
			if (!int.TryParse(App.GetVersionFromJson("autoVer", ModuleResVersionFilePath), out int autoVersion))
			{
				autoVersion = 0;
			}
			return autoVersion;
		}

		private static void ClearConsole()
		{
			var logEntries = System.Type.GetType("UnityEditorInternal.LogEntries,UnityEditor.dll");
			var clearMethod = logEntries.GetMethod("Clear", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
			clearMethod.Invoke(null, null);
		}

		/// <summary>
		/// 清理ab包存放目录
		/// </summary>
		private static void ClearFolder(string outputPath)
		{
			DeleteFolder(outputPath);

			Log("输出目录AB已清理");
		}

		private static void DeleteFolder(string path)
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			else
			{
				try
				{
					FileHelper.CleanDirectory(path);
				}
				catch
				{
					// Method intentionally left empty.
				}
			}
		}

		private static void CreateFolder(string path)
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
		}

		#region 直接复制目录MD5

		private static void GenerateVersionProtoFolder(string dir, PackageConfig versionProto, string relativePath)
		{
			//如果是直接复制目录
			if (IsCopyFolder(dir))
			{
				var files = Directory.GetFiles(dir);
				foreach (string file in files)
				{
					FileInfo fi = new FileInfo(file);
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
				GenerateVersionProtoFolder($"{dir}/{dinfo.Name}", versionProto, rel);
			}
		}

		#endregion 直接复制目录MD5

		#endregion 清单文件

		#region 标记资源

		[MenuItem("AssetBundle/Set AssetBundle Labels（标记）")]
		public static void ResetABLabels()
		{
			ClearABLabels();
			AssetDatabase.RemoveUnusedAssetBundleNames();
			//1.找到资源文件夹
			DirectoryInfo[] modResFolders = new DirectoryInfo(ModuleResPath).GetDirectories();
			TraverseDirectory(modResFolders);
			AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
			Log("资源标记成功");
		}

		/// <summary>
		/// 文件夹是否忽略Tag标记
		/// </summary>
		/// <param name="folderName"></param>
		/// <returns></returns>
		private static bool IsTagIgnoreFolder(string folderName)
		{
			bool result = false;
			foreach (var ignoreFolder in TagIgnoreFolder)
			{
				if (folderName.EndsWith(ignoreFolder, StringComparison.CurrentCultureIgnoreCase))
				{
					result = true;
					break;
				}
			}

			return result;
		}

		/// <summary>
		/// 遍历资源文件
		/// </summary>
		/// <param name="parentDirectories"></param>
		/// <param name="oriDirectory"></param>
		private static void TraverseDirectory(DirectoryInfo[] parentDirectories)
		{
			foreach (DirectoryInfo curDirectoryInfo in parentDirectories)
			{
				if (IsTagIgnoreFolder(curDirectoryInfo.Name)) continue;

				DirectoryInfo[] subdirectory = curDirectoryInfo.GetDirectories();
				if (subdirectory.Length > 0)
				{
					TraverseDirectory(subdirectory);
				}

				if (!curDirectoryInfo.Exists) continue;

				Dictionary<string, string> namePathDict = new Dictionary<string, string>();
				OnModuleFileSystemInfo(curDirectoryInfo, namePathDict);
			}
		}

		/// <summary>
		/// 遍历文件夹中的文件系统
		/// </summary>
		private static void OnModuleFileSystemInfo(DirectoryInfo directoryInfo, Dictionary<string, string> namePathDict)
		{
			FileSystemInfo[] fileSystemInfos = directoryInfo.GetFileSystemInfos();
			foreach (var fileInfo in fileSystemInfos)
			{
				if ((fileInfo.Attributes & FileAttributes.Directory) == FileAttributes.Directory) continue;
				SetLabels(fileInfo, namePathDict);
			}
		}

		/// <summary>
		/// 修改资源文件的assetbundle Labels
		/// </summary>
		private static void SetLabels(FileSystemInfo fileInfo, Dictionary<string, string> namePathDict)
		{
			if (TagIgnoreFiles.Contains(fileInfo.Extension.ToLower())) return;
			if (TagIgnoreFiles.Contains(fileInfo.Name)) return;
			for (int i = 0; i < TagIgnoreFileNames.Length; i++)
			{
				if (fileInfo.Name.Contains(TagIgnoreFileNames[i])) return;
			}
			string bundleName = GetBundleName(fileInfo);
			string assetPath = fileInfo.FullName.Substring(fileInfo.FullName.IndexOf(@"\Assets\") + 1);
			AssetImporter assetImporter = AssetImporter.GetAtPath(assetPath);

			var ResPostFix = fileInfo.Extension.Substring(1);
			if (ResPostFix.ToLower() == "unity")
			{
				var Sp = bundleName.Split("_".ToCharArray());
				if (Sp.Length == 2)
				{
					bundleName = Sp[0];
				}
				else
				{
					Debug.LogError($"*.unity filename must end with _ModuleName");
					return;
				}
			}
			var AbName = $"{bundleName}_{ResPostFix}_{ModNameLower}";
			assetImporter.assetBundleName = AbName;
			assetImporter.assetBundleVariant = _abPostFix;

			//添加到字典
			string folderName = "";
			if (bundleName.Contains("/"))
			{
				folderName = bundleName.Split('/')[1];
			}
			else
			{
				folderName = bundleName;
			}

			string bundlePath = assetImporter.assetBundleName + "." + assetImporter.assetBundleVariant;
			if (!namePathDict.ContainsKey(folderName))
			{
				namePathDict.Add(folderName, bundlePath);
			}
		}

		/// <summary>
		/// 清除之前设置过的AssetBundleName，避免不必要的资源也打包
		/// </summary>
		private static void ClearABLabels()
		{
			var Names = AssetDatabase.GetAllAssetBundleNames();

			foreach (var item in Names)
			{
				var Ends = $"{ModNameLower}.{_abPostFix}";
				// 只清除当前模块的
				if (item.EndsWith(Ends))
				{
					AssetDatabase.RemoveAssetBundleName(item, true);
				}
			}
			Log($"当前模块({ModNameLower})标记清除成功");
		}

		/// <summary>
		/// 获取包名 从ModulesResPath 之后开始的路径
		/// </summary>
		private static string GetBundleName(FileSystemInfo fileInfo)
		{
			string unityPath = FileHelper.NormalizePath(fileInfo.FullName);
			string bundlePath = unityPath.Remove(unityPath.LastIndexOf(".")).Substring(ModuleResPath.Length + 1).ToLower();
			return bundlePath;
		}

		private static void ABName2Variant(string name, out string bn, out string var)
		{
			bn = null;
			var = null;
			if (!name.Contains(".")) return;
			int DotPos = name.LastIndexOf(".");
			var = name.Substring(DotPos + 1);
			bn = name.Substring(0, DotPos);
		}

		private static string ABName2Path(string name)
		{
			var nameArray = name.Split("_".ToCharArray());
			// folder/name, postfix, module.ab
			if (nameArray.Length < 3) return null;
			if (nameArray[1] == "unity")
			{
				string ModName = nameArray[2].Substring(0, nameArray[2].Length - 3);
				return $"{nameArray[0]}_{ModName}.{nameArray[1]}";
			}

			StringBuilder result = new StringBuilder();
			if (nameArray.Length > 3)
			{
				int extIndex = name.LastIndexOf('_');
				name = name.Substring(0, extIndex);
				extIndex = name.LastIndexOf('_');

				result.Append(name.Substring(0, extIndex));
				result.Append(".");
				result.Append(name.Substring(extIndex + 1));
			}
			else
			{
				result.Append($"{nameArray[0]}.{nameArray[1]}");
			}

			return result.ToString();
		}

		#endregion 标记资源

		#region 拷贝文件

		/// <summary>
		/// 文件是否拷贝
		/// </summary>
		/// <param name="fileName"></param>
		/// <param name="fileExtension"></param>
		/// <returns></returns>
		private static bool IsFileCopy(string fileName, string fileExtension)
		{
			for (int i = 0; i < CopyFileNames.Length; i++)
			{
				if (fileName.Contains(CopyFileNames[i]))
				{
					return true;
				}
			}

			if ((CopyFileExtensions.Contains(fileExtension)) || (CopyFileExtensions.Contains(fileName)))
			{
				return true;
			}

			return false;
		}

		private static bool IsIgnoreFolderCopy(string folderName)
		{
			return !CopyIgnoreFolder.Contains(folderName);
		}

		/// <summary>
		/// 拷贝模块文件
		/// </summary>
		/// <param name="path"></param>
		private static void CopyFiles(string sourceDir, string targetDir)
		{
			DirectoryInfo direcoryInfo = new DirectoryInfo(sourceDir);
			if (!IsIgnoreFolderCopy(direcoryInfo.Name)) return;

			FileInfo[] fileInfoArray = direcoryInfo.GetFiles();
			foreach (var fileInfo in fileInfoArray)
			{
				var sourcePath = fileInfo.FullName.Replace("\\", "/");
				if (IsFileCopy(fileInfo.Name, fileInfo.Extension))
				{
					var targetPath = sourcePath.Substring(ModuleResPath.Length, sourcePath.Length - ModuleResPath.Length);
					targetPath = targetDir + targetPath;
					var dirName = Path.GetDirectoryName(targetPath);

					Directory.CreateDirectory(dirName.ToLower());
					File.Copy(sourcePath, targetPath.ToLower(), true);
				}
			}

			var dirs = direcoryInfo.GetDirectories();
			foreach (var dir in dirs)
			{
				CopyFiles(dir.FullName, targetDir);
			}
		}

		#region 复制目录

		private static bool IsCopyFolder(string path)
		{
			if (CopyFolders.Count == 0) return false;

			string filePath = path.Replace("\\", "/");
			filePath = filePath.Substring(ModuleResPath.Length);

			string[] folders = filePath.Split('/');
			for (int i = 0; i < folders.Length; ++i)
			{
				if (CopyFolders.Contains(folders[i]))
				{
					return true;
				}
			}

			return false;
		}

		private static void CopyFolder(string sourceDir, string targetDir)
		{
			DirectoryInfo direcoryInfo = new DirectoryInfo(sourceDir);
			if (IsCopyFolder(direcoryInfo.FullName))
			{
				FileInfo[] fileInfoArray = direcoryInfo.GetFiles();
				foreach (var fileInfo in fileInfoArray)
				{
					var sourcePath = fileInfo.FullName.Replace("\\", "/");
					if (CopyFoldersIgnoreFiles.Contains(fileInfo.Name)
						|| CopyFoldersIgnoreFiles.Contains(fileInfo.Extension)) continue;

					var targetPath = sourcePath.Substring(ModuleResPath.Length, sourcePath.Length - ModuleResPath.Length);
					targetPath = targetDir + targetPath;
					var dirName = Path.GetDirectoryName(targetPath);

					Directory.CreateDirectory(dirName.ToLower());
					File.Copy(sourcePath, targetPath.ToLower(), true);
				}
			}

			var dirs = direcoryInfo.GetDirectories();
			foreach (var dir in dirs)
			{
				CopyFolder(dir.FullName, targetDir);
			}
		}

		#endregion 复制目录

		#endregion 拷贝文件

		/// <summary>
		/// 检测是否是Platform
		/// </summary>
		/// <returns></returns>
		private static bool CheckPlatform()
		{
			string projPathAsserts = Application.dataPath.Replace("\\", "/");
			return Directory.Exists(Path.Combine(projPathAsserts, _exportRootPath)) == false;
		}

		/// <summary>
		/// 是否链接Mod_Shared
		/// </summary>
		/// <returns></returns>
		private static bool CheckPlatformLink()
		{
			if (!CheckPlatform()) return true;

			string projPathAsserts = Application.dataPath.Replace("\\", "/");
			return Directory.Exists(Path.Combine(projPathAsserts, $"Mod_{App.SharedModule}")) == true;
		}

		/// <summary>
		/// 获取Unity安装目录Lib路径
		/// </summary>
		/// <returns></returns>
		private static string GetUnityLibPath()
		{
			string unityPath = EditorApplication.applicationPath.Replace("\\", "/");
			return $"{unityPath.Substring(0, unityPath.LastIndexOf("/"))}/Data/Managed/UnityEngine";
		}

		/// <summary>
		/// 导出根路径
		/// </summary>
		private static string _exportRootPath = "PlatformSDK";

		/// <summary>
		/// 获取库文件数组
		/// </summary>
		/// <returns></returns>
		private static string[] GetLibs()
		{
			string unityLibPath = GetUnityLibPath();
			string pluginPathPrefix = _exportRootPath;

			bool isPlatform = CheckPlatform();
			if (isPlatform)
			{
				var libs = new string[] {
				"System",
				"System.Core",
				"System.Xml.Linq",
				"System.Data.DataSetExtensions",
				"Microsoft.CSharp",
				"System.Data",
				"System.Net.Http",
				"System.Drawing",
				"System.Xml",
				"System.Numerics",
				"System.Numerics.Vectors",

				$"{ModuleAssets}/Plugins/PluginsLib/Mono.Data.dll",
				$"{ModuleAssets}/Plugins/PluginsLib/Mono.Data.Sqlite.dll",
				$"{ModuleAssets}/Plugins/PluginsLib/Interop.SpeechLib.dll",
				$"{ModuleAssets}/Plugins/PluginsLib/DOTween/DOTween.dll",
				$"{ModuleAssets}/Plugins/PluginsLib/DOTween/DOTween43.dll",
				$"{ModuleAssets}/Plugins/PluginsLib/DOTween/DOTween46.dll",

				$"{ModuleAssets}/Plugins/FFTAI/Log.dll",
				$"{ModuleAssets}/Plugins/FFTAI/FFTAICommunicationLib.dll",
				$"{ModuleAssets}/Plugins/FFTAI/Newtonsoft.Json.dll",

				$"{ModuleRoot}/Library/ScriptAssemblies/FFTAI.RUPS.dll",
				$"{ModuleRoot}/Library/ScriptAssemblies/UnityEngine.UI.dll",
				$"{ModuleRoot}/Library/ScriptAssemblies/Unity.Netcode.Runtime.dll",
				$"{ModuleRoot}/Library/ScriptAssemblies/Unity.WebRtc.dll",
				$"{ModuleRoot}/Library/ScriptAssemblies/UniTask.dll",
				$"{unityLibPath}/UnityEngine.dll",
				$"{unityLibPath}/UnityEngine.CoreModule.dll",
				$"{unityLibPath}/UnityEngine.UIModule.dll",
				$"{unityLibPath}/UnityEngine.PhysicsModule.dll",
				$"{unityLibPath}/UnityEngine.Physics2DModule.dll",
				$"{unityLibPath}/UnityEngine.VideoModule.dll",
				$"{unityLibPath}/UnityEngine.AnimationModule.dll",
				$"{unityLibPath}/UnityEngine.ImageConversionModule.dll",
				$"{unityLibPath}/UnityEngine.TextRenderingModule.dll",
				$"{unityLibPath}/UnityEngine.AudioModule.dll",
				$"{unityLibPath}/UnityEngine.ParticleSystemModule.dll",
				$"{unityLibPath}/UnityEngine.InputLegacyModule.dll",

				$"{unityLibPath}/UnityEngine.AssetBundleModule.dll",
				$"{unityLibPath}/UnityEngine.UnityWebRequestModule.dll",
				$"{unityLibPath}/UnityEngine.UnityWebRequestAssetBundleModule.dll",
				$"{unityLibPath}/UnityEngine.JSONSerializeModule.dll",
#if UNITY_ANDROID
				$"{unityLibPath}/UnityEngine.AndroidJNIModule.dll",
#endif
				};
				var sdkLibs = SdkTool.GetReferencedAssemblyPath();
				if (sdkLibs.Length != 0)
				{
					libs = libs.Concat(sdkLibs).ToArray();
				}

				var productLibs = ProductTool.GetReferencedAssemblyPath();
				if (productLibs.Length != 0)
				{
					libs = libs.Concat(productLibs).ToArray();
				}
				return libs;
			}
			else
			{
				var libs = new string[] {
				"System",
				"System.Core",
				"System.Xml.Linq",
				"System.Data.DataSetExtensions",
				"Microsoft.CSharp",
				"System.Data",
				"System.Net.Http",
				"System.Drawing",
				"System.Xml",

				$"{ModuleAssets}/{pluginPathPrefix}/Plugins/PluginsLib/Mono.Data.dll",
				$"{ModuleAssets}/{pluginPathPrefix}/Plugins/PluginsLib/Mono.Data.Sqlite.dll",
				$"{ModuleAssets}/{pluginPathPrefix}/Plugins/PluginsLib/Interop.SpeechLib.dll",
				$"{ModuleAssets}/{pluginPathPrefix}/Plugins/PluginsLib/DOTween/DOTween.dll",
				$"{ModuleAssets}/{pluginPathPrefix}/Plugins/PluginsLib/DOTween/DOTween43.dll",
				$"{ModuleAssets}/{pluginPathPrefix}/Plugins/PluginsLib/DOTween/DOTween46.dll",

				$"{ModuleAssets}/{pluginPathPrefix}/Plugins/FFTAI/Platform.dll",
				$"{ModuleAssets}/{pluginPathPrefix}/Plugins/FFTAI/Log.dll",
				$"{ModuleAssets}/{pluginPathPrefix}/Plugins/FFTAI/FFTAICommunicationLib.dll",
				$"{ModuleAssets}/{pluginPathPrefix}/Plugins/FFTAI/Newtonsoft.Json.dll",
				$"{ModuleRoot}/Library/ScriptAssemblies/Unity.WebRtc.dll",

				$"{ModuleRoot}/Library/ScriptAssemblies/FFTAI.RUPS.dll",
				$"{ModuleRoot}/Library/ScriptAssemblies/UnityEngine.UI.dll",
				$"{ModuleRoot}/Library/ScriptAssemblies/Unity.Netcode.Runtime.dll",
				$"{ModuleRoot}/Library/ScriptAssemblies/UniTask.dll",
				$"{unityLibPath}/UnityEngine.dll",
				$"{unityLibPath}/UnityEngine.CoreModule.dll",
				$"{unityLibPath}/UnityEngine.UIModule.dll",
				$"{unityLibPath}/UnityEngine.PhysicsModule.dll",
				$"{unityLibPath}/UnityEngine.Physics2DModule.dll",
				$"{unityLibPath}/UnityEngine.VideoModule.dll",
				$"{unityLibPath}/UnityEngine.AnimationModule.dll",
				$"{unityLibPath}/UnityEngine.ImageConversionModule.dll",
				$"{unityLibPath}/UnityEngine.TextRenderingModule.dll",
				$"{unityLibPath}/UnityEngine.AudioModule.dll",
				$"{unityLibPath}/UnityEngine.ParticleSystemModule.dll",
				$"{unityLibPath}/UnityEngine.InputLegacyModule.dll",

				$"{unityLibPath}/UnityEngine.AssetBundleModule.dll",
				$"{unityLibPath}/UnityEngine.UnityWebRequestModule.dll",
				$"{unityLibPath}/UnityEngine.UnityWebRequestAssetBundleModule.dll",
				$"{unityLibPath}/UnityEngine.JSONSerializeModule.dll",
#if UNITY_ANDROID
				$"{unityLibPath}/UnityEngine.AndroidJNIModule.dll",
#endif
				};
				var sdkLibs = SdkTool.GetReferencedAssemblyPath();
				if (sdkLibs.Length != 0)
				{
					libs = libs.Concat(sdkLibs).ToArray();
				}

				var productLibs = ProductTool.GetReferencedAssemblyPath();
				if (productLibs.Length != 0)
				{
					libs = libs.Concat(productLibs).ToArray();
				}
				return libs;
			}
		}

		#region 命令行调用

		public static void PackWindows()
		{
			Pack(PlatformType.Windows);
		}

		public static void PackLinux()
		{
			Pack(PlatformType.Linux);
		}

		public static void PackAndroid()
		{
			Pack(PlatformType.Android);
		}

		public static void PackIOS()
		{
			Pack(PlatformType.iOS);
		}

		private static void Pack(PlatformType pt)
		{
			Debug.Log("Test Packing");
			SetPlatformType(pt);
			SetBuildType(BuildType.Release);
			ModNames = GetModuleNames();
			for (int i = 0; i < ModNames.Length; i++)
			{
				if (ModNames[i] != App.SharedModule)
				{
					InitPath(ModNames[i]);
				}
			}

			PackRes();
		}

		public static void Pack(string productBuildVer, PlatformType pt, string moduleName, string basePath, string outPutPath)
		{
			SetPlatformType(pt);
			SetBuildType(BuildType.Release);
			InitPath(moduleName, basePath);
			ProductBuildVer = productBuildVer;
			PackRes(outPutPath: outPutPath);
		}

		public static void Pack(PlatformType pt, string moduleName, string basePath, string outPutPath)
		{
			SetPlatformType(pt);
			SetBuildType(BuildType.BeforeRelease);
			InitPath(moduleName, basePath);
			PackRes(outPutPath: outPutPath);
		}

		public static void PreBuildExe()
		{
			string Version = PlayerSettings.bundleVersion;

			var Vers = Version.Split(".".ToCharArray());
			string[] VerList = new string[4] { "4", "0", "0", "0" };

			for (int i = 1; i < 4; i++)
			{
				if (i < Vers.Length)
				{
					int IntV;
					int.TryParse(Vers[i], out IntV);
					if (i == 3)
					{
						IntV++;
					}
					VerList[i] = IntV.ToString();
				}
			}

			string NewVer = string.Join(".", VerList);
			Log($"Version is : {Version} -> {NewVer}");

			PlayerSettings.bundleVersion = NewVer;
		}

		#endregion 命令行调用
	}
}