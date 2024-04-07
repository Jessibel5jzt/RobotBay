using LitJson;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEditor.PackageManager;
using Cysharp.Threading.Tasks;

namespace WestBay
{
	public class BuildManager : EditorWindow
	{
		public static string BuildPath;
		public static string ZipPath;
		public static string BinPath;

		private static string _platformGitBranch;
		private static string _buildGitBranch;
		private static string _targetName;
		private static string _rupsZipPath;

		private static string _log;
		private static string _curTarget;
		private static string _logPath;
		private static string _dataPath;

		private static string _repoFullName;
		private static string _repoBranch;
		private static string _sha;
		private static string _authorEmail;
		private static string _commitInfo;

		#region CommandLineFunction

		private static async void CommandLineBuildWindowsPlatform()
		{
			var parseResult = ParsingParam(1);
			if (!parseResult) return;

			_curTarget = "Platform";
			_repoFullName = Gitea.PlatformRepoFullName;
			_repoBranch = _platformGitBranch;

			InitConfig();
			BuildPull($"{BuildPath}/Platform", _repoFullName, _repoBranch, true);
			//await GenCodeAndBuild();
			AlarmCheck();
			ClearPlatform();
			CreateRepoInfoFile(_rupsZipPath, true);
			await UniTask.Delay(1);
			GitHelper.ClearRepo($"{BuildPath}/Platform");
		}

		private static void CommandLineBuildWindowsCommon()
		{
			var parseResult = ParsingParam(2);
			if (!parseResult) return;

			_repoFullName = Gitea.CommonRepoFullName;
			_repoBranch = _buildGitBranch;

			InitConfig();
			BuildPull($"{BuildPath}/SM/Common", _repoFullName, _repoBranch);
			PreparePlatform();
			ZipPath += "RUPS/Common/";
			BuildModules("Common", "SM");
			CreateRepoInfoFile(ZipPath);

			GitHelper.ClearRepo($"{BuildPath}/SM/Common");
		}

		private static async void CommandLineBuildWindowsDevice()
		{
			var parseResult = ParsingParam(3);
			if (!parseResult) return;

			_curTarget = $"{_targetName} Platform";
			_repoFullName = $"{Gitea.ProductRepoFullNamePrefix}/{_targetName}";
			_repoBranch = _buildGitBranch;

			InitConfig();
			BuildPull($"{BuildPath}/Product/{_targetName}", _repoFullName, _repoBranch);
			PreparePlatform();
			PrepareSDK();
			//await GenCodeAndBuild();
			AlarmCheck();
			ZipPath += $"Product/{_targetName}/";
			BuildModules($"{_targetName}/Module", "Product");
			CreateRepoInfoFile(ZipPath);

			await UniTask.Delay(1);
			ProductTool.RemoveAllSDK();
			ClearPlatform();
			GitHelper.ClearRepo($"{BuildPath}/Product/{_targetName}");
		}

		private static void CommandLineBuildWindowsGame()
		{
			var parseResult = ParsingParam(3);
			if (!parseResult) return;

			_repoFullName = $"{Gitea.GameRepoFullNamePrefix}/Mod_{_targetName}";
			_repoBranch = _buildGitBranch;

			InitConfig();
			BuildPull($"{BuildPath}/SM/Games/Mod_{_targetName}", _repoFullName, _repoBranch);
			PreparePlatform();
			ZipPath += $"RUPS/Games/{_targetName}/";
			BuildModule("Games", _targetName);
			CreateRepoInfoFile(ZipPath);

			GitHelper.ClearRepo($"{BuildPath}/SM/Games/Mod_{_targetName}");
		}

		#endregion CommandLineFunction

		#region Init

		private static bool ParsingParam(int count)
		{
			var result = false;

			string[] args = Environment.GetCommandLineArgs();
			int paramIndex = 0;
			foreach (var arg in args)
			{
				if (arg.Contains("-p:"))
				{
					break;
				}
				paramIndex++;
			}

			var idx = paramIndex + 1;
			if (idx >= args.Length) return result;
			_platformGitBranch = args[idx];
			Debug.Log("Platform Branch: " + _platformGitBranch);

			if (count > 1)
			{
				idx = paramIndex + 2;
				if (idx >= args.Length) return result;

				_buildGitBranch = args[idx];
				Debug.Log("Build Branch: " + _buildGitBranch);
			}

			if (count > 2)
			{
				idx = paramIndex + 3;
				if (idx >= args.Length) return result;
				var repoName = args[idx];
				_targetName = repoName.Trim();
				Debug.Log("Build Target Name: " + _targetName);
			}

			result = true;
			return result;
		}

		public static void InitConfig()
		{
			Application.logMessageReceived += HandleLog;
			BuildPath = PathUtil.GetBuildPath();
			ZipPath = $"{BuildPath}/PackageZIP/";
			BinPath = $"{BuildPath}/Bin";
			_logPath = $"{BuildPath}/BuildLog.txt";

			_commitInfo = Gitea.RetieveRepoCommitInfoSync(_repoFullName, _repoBranch);
			if (!string.IsNullOrWhiteSpace(_commitInfo))
			{
				JsonData jsonCommit = JsonMapper.ToObject(_commitInfo);
				_sha = jsonCommit["commit"]["id"].ToString();
				_authorEmail = jsonCommit["commit"]["author"]["email"].ToString();
			}

			_dataPath = Application.dataPath.Replace("\\", "/");
			if (!File.Exists(_logPath))
			{
				File.Create(_logPath);
			}
			else
			{
				File.WriteAllText(_logPath, string.Empty);
			}
		}

		private static void PrepareSDK()
		{
			var productTxtPath = $"{BuildPath}/product.txt";
			if (!File.Exists(productTxtPath)) return;

			var buildProductInfo = File.ReadAllText(productTxtPath).Split(',');
			if (buildProductInfo.Length < 2) return;

			var productType = buildProductInfo[1];
			ZipPath += $"{productType}/";
			EditorOptions.ProductType = productType;
			if (productType == "M2P")
			{
				EditorOptions.ProductType = "M2";
			}
			ProductTool.InitAdapterWriter();
			ProductTool.ImportProductSDK(EditorOptions.ProductType);
		}

		private static void ChangelogVer()
		{
			var changelogVer = "1.0.0.0";
			var changelogPath = $"{Application.dataPath}/Scripts/changelog.json";
			if (File.Exists(changelogPath))
			{
				JsonData jsonLog = JsonMapper.ToObject(File.ReadAllText(changelogPath));
				changelogVer = jsonLog["version"].ToString();
			}
			Debug.Log(changelogVer);
			var changelogFilePath = $"{_rupsZipPath}/changelogver.txt";
			FileHelper.WriteFile(changelogFilePath, changelogVer);
		}

		#endregion Init

		#region Git

		private static void BuildPull(string repoPath, string repoFullName, string branch, bool isPlatform = false)
		{
			Debug.Log($"{branch} {repoPath} git pull");

			//clone if not exists
			if (!FileHelper.IsDirectoryExist(repoPath))
			{
				GitHelper.Clone($"{Gitea.ServerUrl}/{repoFullName}.git", repoPath);
			}

			GitHelper.PrepareRepo(repoPath, branch);
			if (isPlatform)
			{
				GitHelper.UpdateSubmodule(repoPath);
			}

			AssetDatabase.Refresh();
		}

		#endregion Git

		#region Log

		private static void HandleLog(string condition, string stackTrace, LogType type)
		{
			switch (type)
			{
				case LogType.Error:
					_log += $"{_curTarget} ERROR：";
					break;

				case LogType.Assert:
					_log += "Assert：";
					break;

				case LogType.Warning:
					return;

				case LogType.Log:
					_log += "Log：";
					break;

				case LogType.Exception:
					_log += "Exception：";
					break;

				default:
					break;
			}
			_log += $"{condition}\n";
			FileHelper.WriteFile(_logPath, _log);
		}

		#endregion Log

		#region HotfixBinding

		private static void PreparePlatform()
		{
			BuildPull($"{BuildPath}/Platform", Gitea.PlatformRepoFullName, _platformGitBranch, true);
		}

		public static async Task GenCodeCompleted()
		{
			await BeforeBuildWindows();
		}

		#endregion HotfixBinding

		#region Platform

		public static async Task BeforeBuildWindows()
		{
			PlayerSettings.SplashScreen.show = false;
			FileUtil.DeleteFileOrDirectory(BinPath);
			AssetDatabase.Refresh();
			Directory.CreateDirectory(BinPath);

			//清理
			Functional.ClearAllMod();
			JenkinsManager.ClearStreamingAssetsPath();

			byte[] utf8 = Encoding.UTF8.GetBytes(ZipPath);
			ZipPath = Encoding.UTF8.GetString(utf8);
			_rupsZipPath = ZipPath + $"RUPS/Platform/{_platformGitBranch}";
			if (!Directory.Exists(ZipPath))
			{
				Directory.CreateDirectory(ZipPath);
			}

			if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.StandaloneWindows64)
			{
				EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
			}

			PlayerSettings.productName = "Platform";

			//打包
			await BuildPlatform();
		}

		private static async Task BuildPlatform()
		{
			var productTxtPath = $"{BuildPath}/product.txt";
			if (!File.Exists(productTxtPath)) return;

			var buildProductInfo = File.ReadAllText(productTxtPath).Split(',');
			if (buildProductInfo.Length < 2) return;

			var productType = buildProductInfo[1];
			if (productType == "M2P") productType = "M2";
			var isUpdated = Functional.UpdateManifest(productType);
			if (isUpdated)
			{
				Client.Resolve();
				await Task.Delay(5000);
			}

			Debug.Log($"{BinPath}/Platform.exe");
			var buildset = new BuildPlayerOptions
			{
				scenes = new string[] { "Assets/Scenes/StartUp.unity" },
				locationPathName = $"{BinPath}/Platform.exe",
				target = BuildTarget.StandaloneWindows64,
				options = BuildOptions.None,
			};
			var buildReport = BuildPipeline.BuildPlayer(buildset);
			var summary = buildReport.summary;
			if (summary.result == BuildResult.Succeeded)
			{
				Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
			}
			else if (summary.result == BuildResult.Failed)
			{
				Debug.Log("Build failed");
				return;
			}

			ZipPlatform();
			ChangelogVer();
		}

		private static void ZipPlatform()
		{
			if (Directory.Exists($"{BinPath}/PersistentData"))
			{
				Directory.Delete($"{BinPath}/PersistentData");
			}

			JenkinsManager.GenerateVersionInfo(BinPath, "RUPS");
			JenkinsManager.Zip(BinPath, _rupsZipPath, "RUPS");
		}

		private static void ClearPlatform()
		{
			BuildPull($"{BuildPath}/Platform", Gitea.PlatformRepoFullName, _platformGitBranch, true);
		}

		#endregion Platform

		#region Module

		private static void LinkModule(string moduleDir, string moduleName, string moduleType)
		{
			string fromPath = $"{BuildPath}/{moduleType}/{moduleDir}/Mod_{moduleName}";
			string toPath = $"{_dataPath}/Mod_{moduleName}";

			if (!Directory.Exists(toPath))
			{
				FileUtil.CopyFileOrDirectory(fromPath, toPath);
				AssetDatabase.Refresh();
			}
		}

		private static void RemoveLink(string moduleName)
		{
			string toPath = $"{_dataPath}/Mod_{moduleName}";

			FileUtil.DeleteFileOrDirectory(toPath);
			AssetDatabase.Refresh();
		}

		private static void BuildModule(string moduleDir, string moduleName)
		{
			LinkModule("Common", "Shared", "SM");

			ZipPath += _buildGitBranch;
			_curTarget = $"Game {moduleName}";
			Pack(moduleDir, moduleName.ToLower(), "SM");
			AlarmCheck();

			RemoveLink("Shared");
		}

		private static void BuildModules(string moduleDir, string moduleType)
		{
			LinkModule("Common", "Shared", "SM");
			ZipPath += _buildGitBranch;

			var modulesPath = $"{BuildPath}/{moduleType}/{moduleDir}";
			var dics = new DirectoryInfo(modulesPath).GetDirectories();

			string moduleName;
			foreach (var item in dics)
			{
				if (!item.Name.Contains("Mod_")) continue;
				if (item.Name.Equals("Mod_Reload") || item.Name.Equals("Mod_Tobii")) continue;

				moduleName = item.Name.Remove(0, 4);
				_curTarget = $"{_targetName} Module {moduleName}";
				Pack(moduleDir, moduleName.ToLower(), moduleType);
				AlarmCheck();
			}

			RemoveLink("Shared");
		}

		private static void Pack(string moduleDir, string moduleName, string moduleType)
		{
			PackerAndClear(moduleDir, moduleName, moduleType);
			var sourceModulePath = $"{BinPath}/PersistentData/{moduleName}";
			JenkinsManager.Zip(sourceModulePath, ZipPath, moduleName);
		}

		private static void PackerAndClear(string moduleDir, string modName, string moduleType)
		{
			LinkModule(moduleDir, modName, moduleType);
			Packer.Pack(Packer.PlatformType.Windows, modName, BuildPath, $"{BinPath}/PersistentData/{modName.ToLower()}");

			if (!modName.ToLower().Equals("shared"))
			{
				RemoveLink(modName);
			}
			AssetDatabase.Refresh();
		}

		#endregion Module

		#region Alarm

		private static void AlarmCheck()
		{
			if (_log.Contains($"{_curTarget} ERROR："))
			{
				SendAlarm($"{_curTarget} Build ERROR");
			}
		}

		private static void SendAlarm(string error)
		{
			var reqData = new JsonData
			{
				["PlatformGit"] = _platformGitBranch,
				["BuildGit"] = _buildGitBranch,
				["Target"] = _targetName,
				["errorMessage"] = error,
				["BuildLog"] = _log,
				["email"] = _authorEmail,
				["sha"] = _sha,
				["commitInfo"] = _commitInfo
			};
			var json = reqData.ToJson();
			_ = RequestPublish(json);
		}

		private static async Task RequestPublish(string json)
		{
			var url = "http://192.168.8.254:8080/generic-webhook-trigger/invoke?token=Alarm";

			Debug.Log("地址：" + url);
			try
			{
				await WebReqHelper.PostRequestByJson(url, json,
					delegate (bool isSuccess, string res)
					{
						if (isSuccess)
						{
							Debug.Log("Jenkins报警器已触发：" + res);
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

		#endregion Alarm

		/// <summary>
		/// 复制构建的仓库分支信息
		/// </summary>
		/// <param name="target"></param>
		private static void CreateRepoInfoFile(string targetPath, bool isPlatform = false)
		{
			Debug.Log($"CreateRepoInfoFile:{targetPath}");

			JsonData jsonBuild = new JsonData();
			jsonBuild["repoFullName"] = _repoFullName;
			jsonBuild["branch"] = _repoBranch;

			string platformCommit = Gitea.RetieveRepoCommitInfoSync(Gitea.PlatformRepoFullName, _platformGitBranch);
			if (!string.IsNullOrWhiteSpace(platformCommit))
			{
				jsonBuild["platform_commit"] = JsonMapper.ToObject(platformCommit);
			}

			if (!isPlatform)
			{
				string commitInfo = Gitea.RetieveRepoCommitInfoSync(_repoFullName, _repoBranch);
				if (!string.IsNullOrWhiteSpace(commitInfo))
				{
					jsonBuild["commit"] = JsonMapper.ToObject(commitInfo);
				}
			}

			var repoInfoPath = $"{targetPath}/RepoInfo.json";
			if (File.Exists(repoInfoPath)) File.Delete(repoInfoPath);
			File.WriteAllText(repoInfoPath, JsonMapper.ToJson(jsonBuild));
		}
	}
}