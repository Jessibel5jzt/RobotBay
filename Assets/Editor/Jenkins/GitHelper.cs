using System;
using System.IO;

namespace WestBay
{
	public static class GitHelper
	{
		public static readonly CommandRunner CommandRunner = new CommandRunner("git");

		public static string Clone(string url, string path)
		{
			if (!Directory.Exists(path)) Directory.CreateDirectory(path);

			return ExecuteCmd(path, $"clone {url} {path}");
		}

		/// <summary>
		/// 获取当前分支名
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string GetCurrentBranch(string path)
		{
			//return CommandRunner.Run("symbolic-ref --short HEAD");
			//return CommandRunner.Run("branch | grep \" * \"");
			return ExecuteCmd(path, "symbolic-ref --short -q HEAD");
		}

		/// <summary>
		/// 刷新分支和标签（清除本地仓库和服务端同步）
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string Prune(string path)
		{
			return ExecuteCmd(path, "fetch origin --prune --prune-tags");
		}

		public static string RemoveRebase(string path)
		{
			return ExecuteCmd(path, "rm -rf .git/rebase-apply");
		}

		public static string AbortAM(string path)
		{
			return ExecuteCmd(path, "am --abort");
		}

		/// <summary>
		/// 签出到工作区
		/// </summary>
		/// <param name="path"></param>
		/// <param name="branchName"></param>
		/// <returns></returns>
		public static string Checkout(string path, string branchName)
		{
			return ExecuteCmd(path, $"checkout -f {branchName}");
		}

		/// <summary>
		/// 从远端拉取到工作区
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string Pull(string path)
		{
			return ExecuteCmd(path, "pull");
		}

		public static string Push(string path, string branch)
		{
			return ExecuteCmd(path, $"push origin {branch}");
		}

		public static string Merge(string path, string fromBranch)
		{
			return ExecuteCmd(path, $"merge {fromBranch}");
		}

		public static bool IsGitRepo(string path)
		{
			var ret = ExecuteCmd(path, "rev-parse --is-inside-work-tree");
			return ret.Contains("true");
		}

		public static string DeleteBranch(string path, string branch)
		{
			return ExecuteCmd(path, $"branch -d {branch}");
		}

		public static string Clean(string path)
		{
			return ExecuteCmd(path, $"clean -f ");
		}

		public static void RevertFile(string path, string filePath)
		{
			ExecuteCmd(path, $"checkout -- {filePath}");
		}

		public static void UpdateSubmodule(string path)
		{
			ExecuteCmd(path, $"submodule update --init --remote");
		}

		/// <summary>
		/// 执行Git指令
		/// </summary>
		/// <param name="path"></param>
		/// <param name="cmd"></param>
		/// <returns></returns>
		public static string ExecuteCmd(string path, string cmd)
		{
			CmdExecuteResult = CommandRunner.Run(path, cmd);
			CmdOutput = CmdExecuteResult ? CommandRunner.LastStandardOutput : CommandRunner.LastStandardError;
			ShowLog();

			return CmdOutput;
		}

		/// <summary>
		/// 指令执行返回值
		/// </summary>
		public static string CmdOutput { get; private set; }

		public static bool CmdExecuteResult { get; private set; }

		#region Logic

		public static void PrepareRepo(string path, string branch)
		{
			Debug.Log($"PrepareRepo path ={path} branch={branch}");

			Prune(path);
			var nowBranch = GetCurrentBranch(path);
			if (!branch.Equals(nowBranch))
			{
				ExecuteCmd(path, $"checkout .");
				Checkout(path, branch);
			}
			else
			{
				//ExecuteCmd(path, $"checkout .");
			}

			Pull(path);
		}

		public static void ClearRepo(string path, string defaultBranch = "master")
		{
			var nowBranch = GetCurrentBranch(path);
			if (!defaultBranch.Equals(nowBranch))
			{
				ExecuteCmd(path, $"checkout .");
				Checkout(path, defaultBranch);
				Pull(path);
				DeleteBranch(path, nowBranch);
			}
			else
			{
				ExecuteCmd(path, $"checkout .");
			}
		}

		private static void ShowLog()
		{
			if (!CmdExecuteResult)
			{
				//Debug.Log(CmdOutput);
			}
		}

		#endregion Logic
	}
}