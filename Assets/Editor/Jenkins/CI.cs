using LitJson;
using System;
using UnityEditor;
using UnityEngine;

namespace WestBay
{
	public class CI : EditorWindow
	{
		private static string _repoFullName;
		private static string _branchName;
		private static bool _isCommits;

		public static void CommandLineBuild()
		{
			try
			{
				ParseParams();
				Init();
				PrepareRepo();
				LinkMod();
			}
			catch (Exception e)
			{
				Debug.Log(e.ToString());
			}
			finally
			{
				EditorApplication.Exit(0);
			}
		}

		public static void CommandLineClear()
		{
			ParseParams();
			Init();
			ClearRepo();
		}

		private static void ParseParams()
		{
			string[] args = Environment.GetCommandLineArgs();
			int paramIndex = -1;
			foreach (var arg in args)
			{
				paramIndex++;
				if (arg.Contains("-p:"))
				{
					break;
				}
			}

			_repoFullName = args[paramIndex + 1];
			_branchName = args[paramIndex + 2];
			_isCommits = args[paramIndex + 3].Equals("true");

			Debug.Log($"CommandLineBuild repoFullName=${_repoFullName} branchName=${_branchName} isCommits=${_isCommits}");
		}

		private const string ProjectName = "Platform";//产品名
		private static string _rupsPath;
		private static bool _isPlatform;
		private static bool _isGame;
		private static bool _isCommon;
		private static string _repoName;

		private static void Init()
		{
			_rupsPath = PathUtil.GetRupsPath();
			_repoName = _repoFullName.Split('/')[1];

			_isPlatform = _repoName.Equals(ProjectName);
			_isGame = _repoFullName.Contains("Games");
			_isCommon = _repoFullName.Contains("Common");

			Functional.ClearAllMod();
		}

		private static void PrepareRepo()
		{
			GitHelper.PrepareRepo($"{_rupsPath}/{ProjectName}", _branchName);
			GitHelper.PrepareRepo($"{_rupsPath}/SM/Common", _branchName);
			if (!_isPlatform)
			{
				if (_isGame)
				{
					var path = $"{_rupsPath}/SM/Games/{_repoName}";
					if (!FileHelper.IsDirectoryExist(path))
					{
						GitHelper.Clone($"{Gitea.ServerUrl}/{_repoFullName}.git", path);
					}

					GitHelper.PrepareRepo(path, _branchName);
				}
				else if (!_isCommon)
				{
					var path = $"{_rupsPath}/Product/{_repoName}";
					if (!FileHelper.IsDirectoryExist(path))
					{
						GitHelper.Clone($"{Gitea.ServerUrl}/{_repoFullName}.git", path);
					}
					GitHelper.PrepareRepo(path, _branchName);
				}
			}
		}

		private static void ClearRepo()
		{
			GitHelper.ClearRepo($"{_rupsPath}/{ProjectName}");
			GitHelper.ClearRepo($"{_rupsPath}/SM/Common");
			if (!_isPlatform)
			{
				if (_isGame)
				{
					var path = $"{_rupsPath}/SM/Games/{_repoName}";
					GitHelper.ClearRepo(path);
				}
				else if (!_isCommon)
				{
					var path = $"{_rupsPath}/Product/{_repoName}";
					GitHelper.ClearRepo(path);
				}
			}
		}

		private static void LinkMod()
		{
			if (!_isPlatform)
			{
				if (_isCommon)
				{
					Functional.CopyMod($"{_rupsPath}/SM/Common", new string[] { "Reload", "RemoteHelper", "Tobii" }, null);
				}
				else if (_isGame)
				{
					Functional.CopyMod($"{_rupsPath}/SM/Games/{_repoName}", $"{Application.dataPath}/{_repoName}");
					Functional.CopyMod($"{_rupsPath}/SM/Common/{App.ModNamePrefix}Shared", $"{Application.dataPath}/{App.ModNamePrefix}Shared");
				}
				else
				{
					Functional.CopyMod($"{_rupsPath}/Product/{_repoName}", null, null);
					Functional.CopyMod($"{_rupsPath}/SM/Common/{App.ModNamePrefix}Shared", $"{Application.dataPath}/{App.ModNamePrefix}Shared");
				}
			}

			AssetDatabase.Refresh();
		}

		#region Build All

		public static void CommandLineBuildAll()
		{
			try
			{
				InitAll();
				var buildInfoPath = ParseBuildInfoPathArg();
				var buildInfo = GetBuildInfo(buildInfoPath);
				if (buildInfo == null)
				{
					Debug.LogError($"构建信息为空,path={buildInfoPath}");
					return;
				}
				PrepareAll(buildInfo);
			}
			catch (Exception e)
			{
				Debug.Log(e.ToString());
			}
			finally
			{
				EditorApplication.Exit(0);
			}
		}

		public static void CommandLineClearAll()
		{
			InitAll();
			var buildInfoPath = ParseBuildInfoPathArg();
			var buildInfo = GetBuildInfo(buildInfoPath);
			if (buildInfo == null)
			{
				Debug.LogError($"构建信息为空,path={buildInfoPath}");
				return;
			}
			ClearAll(buildInfo);
		}

		private static string ParseBuildInfoPathArg()
		{
			string result = string.Empty;

			string[] args = Environment.GetCommandLineArgs();
			int paramIndex = -1;
			foreach (var arg in args)
			{
				paramIndex++;
				if (arg.Contains("-p:"))
				{
					break;
				}
			}

			result = args[paramIndex + 1];
			Debug.Log($"CommandLinePrepare buildInfoPath=${result}");

			return result;
		}

		private static void InitAll()
		{
			_rupsPath = PathUtil.GetRupsPath();
			Functional.ClearAllMod();
			AssetDatabase.Refresh();
			FileHelper.CleanDirectory(PathUtil.ProductEditorPath);
		}

		private static JsonData GetBuildInfo(string path)
		{
			JsonData result = new JsonData();

			var infoStr = FileHelper.ReadFile(path);
			if (string.IsNullOrWhiteSpace(infoStr)) return result;
			else result = JsonMapper.ToObject(infoStr);

			return result;
		}

		private static void PrepareAll(JsonData buildInfo)
		{
			var defaultBranch = "master";
			string branchName = buildInfo["RUPSGit"].ToString();
			if (string.IsNullOrWhiteSpace(branchName)) branchName = defaultBranch;

			GitHelper.PrepareRepo($"{_rupsPath}/{ProjectName}", branchName);
			GitHelper.PrepareRepo($"{_rupsPath}/SM/Common", branchName);

			var product = buildInfo["Product"].ToString();
			if (product.ToLower().Equals("m2p")) product = "M2";

			var productPath = $"{_rupsPath}/Product/{product}";
			if (!FileHelper.IsDirectoryExist(productPath))
			{
				GitHelper.Clone($"{Gitea.ServerUrl}/{Gitea.ProductRepoFullNamePrefix}/{product}.git", productPath);
			}
			GitHelper.PrepareRepo(productPath, branchName);
			ProductTool.MakeProductLink(product, false);

			var commonMods = buildInfo["CommonMods"].ToString().Split(',');
			Functional.CopyMod($"{_rupsPath}/SM/Common", null, commonMods);
			var productMods = buildInfo["ProductMods"].ToString().Split(',');
			Functional.CopyMod($"{productPath}/Module", null, productMods);

			var gameMods = buildInfo["GameMods"].ToString().Split(',');
			for (int i = 0; i < gameMods.Length; i++)
			{
				var game = gameMods[i];
				var gamePath = $"{_rupsPath}/SM/Games/{App.ModNamePrefix}{game}";
				if (!FileHelper.IsDirectoryExist(gamePath))
				{
					GitHelper.Clone($"{Gitea.ServerUrl}/Games/{App.ModNamePrefix}{game}.git", gamePath);
				}
				GitHelper.PrepareRepo(gamePath, branchName);
				Functional.CopyMod(gamePath, $"{Application.dataPath}/{App.ModNamePrefix}{game}");
			}
		}

		public static void ClearAll(JsonData buildInfo)
		{
			GitHelper.ClearRepo($"{_rupsPath}/{ProjectName}");
			GitHelper.ClearRepo($"{_rupsPath}/SM/Common");

			var product = buildInfo["Product"].ToString();
			if (product.ToLower().Equals("m2p")) product = "M2";

			var productPath = $"{_rupsPath}/Product/{product}";
			GitHelper.ClearRepo(productPath);

			var gameMods = buildInfo["GameMods"].ToString().Split(',');
			for (int i = 0; i < gameMods.Length; i++)
			{
				var game = gameMods[i];
				var gamePath = $"{_rupsPath}/SM/Games/{App.ModNamePrefix}{game}";
				GitHelper.ClearRepo(gamePath);
			}
		}

		#endregion Build All
	}
}