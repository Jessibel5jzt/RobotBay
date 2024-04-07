using ICSharpCode.SharpZipLib.Checksum;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using LitJson;

namespace WestBay
{
	public class Functional
	{
		public static string[] GetModuleNames(string targetPath)
		{
			List<string> result = new List<string>();

			var formatPath = targetPath.Replace("\\", "/");
			var pathNames = Directory.GetDirectories(formatPath);
			for (var i = 0; i < pathNames.Length; ++i)
			{
				string path = pathNames[i].Replace("\\", "/");
				string folderName = path.Substring(path.LastIndexOf('/') + 1);

				if (folderName.StartsWith(App.ModNamePrefix))
				{
					result.Add(folderName);
				}
			}

			return result.ToArray();
		}

		/// <summary>
		/// 清理所有模块
		/// </summary>
		public static void ClearAllMod()
		{
			//删除Mod
			var modNames = GetModuleNames(Application.dataPath);
			for (var i = 0; i < modNames.Length; ++i)
			{
				var modName = modNames[i];
				var modPath = $"{Application.dataPath}/{modName}";
				RemoveMod(modPath);
			}

			AssetDatabase.Refresh();
		}

		/// <summary>
		/// 移除模块链接
		/// </summary>
		/// <param name="path"></param>
		/// <param name="isLink"></param>
		public static void RemoveMod(string path, bool isLink = false)
		{
			if (isLink)
			{
				RemoveLink(path);
			}
			else
			{
				FileUtil.DeleteFileOrDirectory(path);
			}

			FileUtil.DeleteFileOrDirectory($"{path}.meta");
		}

		/// <summary>
		/// 链接模块到项目
		/// </summary>
		/// <param name="fromPath"></param>
		/// <param name="toPath"></param>
		/// <param name="isLink"></param>
		public static void CopyMod(string fromPath, string toPath, bool isLink = false)
		{
			if (isLink)
			{
				MakeLink(fromPath, toPath);
			}
			else
			{
				FileUtil.CopyFileOrDirectory(fromPath, toPath);
			}
		}

		/// <summary>
		/// 链接目录下的模块
		/// </summary>
		/// <param name="fromPath"></param>
		/// <param name="exclusiveMods"></param>
		public static void CopyMod(string fromPath, string[] exclusiveMods, string[] requiredMods)
		{
			var modNames = GetModuleNames(fromPath);
			for (int i = 0; i < modNames.Length; ++i)
			{
				var modDirName = modNames[i];
				var modName = modDirName.Replace(App.ModNamePrefix, "");
				if (exclusiveMods != null && exclusiveMods.Any(value => value == modName)) continue;
				if (requiredMods != null && !requiredMods.Any(value => value.Contains(modName))) continue;
				var fromModPath = $"{fromPath}/{modDirName}";
				var toModPath = $"{Application.dataPath}/{modDirName}";
				CopyMod(fromModPath, toModPath);
			}
		}

		/// <summary>
		/// Link Path
		/// </summary>
		/// <param name="toPath"></param>
		/// <param name="fromPath"></param>
		public static void MakeLink(string fromPath, string toPath)
		{
			toPath = toPath.Replace("/", "\\");
			fromPath = fromPath.Replace("/", "\\");
			var command = $"mklink /J {toPath} {fromPath}";

			Execute(command, 10);
		}

		/// <summary>
		/// 移除软链接目录
		/// </summary>
		/// <param name="path"></param>
		public static void RemoveLink(string path)
		{
			path = path.Replace("/", "\\");
			var command = $"rmdir /S/Q {path}";
			Execute(command, 10);
		}

		/// <summary>
		/// 链接模块
		/// </summary>
		/// <param name="command"></param>
		/// <param name="seconds"></param>
		/// <returns></returns>
		public static string Execute(string command, int seconds = 10)
		{
			string output = ""; //输出字符串
			if (command != null && !command.Equals(""))
			{
				Process process = new Process();//创建进程对象
				ProcessStartInfo startInfo = new ProcessStartInfo
				{
					FileName = "cmd.exe",//设定需要执行的命令
					Arguments = "/C " + command,//“/C”表示执行完命令后马上退出
					UseShellExecute = false,//不使用系统外壳程序启动
					RedirectStandardInput = true,//不重定向输入
					RedirectStandardOutput = true, //重定向输出
					CreateNoWindow = true//不创建窗口
				};
				process.StartInfo = startInfo;
				try
				{
					if (process.Start())//开始进程
					{
						if (seconds == 0)
						{
							process.WaitForExit();//这里无限等待进程结束
						}
						else
						{
							process.WaitForExit(seconds); //等待进程结束，等待时间为指定的毫秒
						}
						output = process.StandardOutput.ReadToEnd();//读取进程的输出
					}
				}
				catch { }
				finally
				{
					if (process != null)
					{
						process.Close();
					}
				}
			}

			return output;
		}

		/// <summary>
		/// 压缩文件夹为ZIP
		/// </summary>
		/// <param name="sourceFilePath"></param>
		/// <param name="destinationZipFilePath"></param>
		public static void CreateZip(string sourceFilePath, string destinationZipFilePath)
		{
			if (sourceFilePath[sourceFilePath.Length - 1] != System.IO.Path.DirectorySeparatorChar)
				sourceFilePath += System.IO.Path.DirectorySeparatorChar;

			ZipOutputStream zipStream = new ZipOutputStream(File.Create(destinationZipFilePath));
			zipStream.SetLevel(0);
			CreateZipFile(sourceFilePath, zipStream, sourceFilePath.Length);
			zipStream.Finish();
			zipStream.Close();
		}

		/// <summary>
		/// 递归压缩文件
		/// </summary>
		/// <param name="sourceFilePath"></param>
		/// <param name="zipStream"></param>
		/// <param name="subIndex"></param>
		private static void CreateZipFile(string sourceFilePath, ZipOutputStream zipStream, int subIndex)
		{
			if (string.IsNullOrWhiteSpace(sourceFilePath) || zipStream == null) return;

			Crc32 crc = new Crc32();
			string[] filesArray = Directory.GetFileSystemEntries(sourceFilePath);
			foreach (string file in filesArray)
			{
				if (Directory.Exists(file))
				{
					CreateZipFile(file, zipStream, subIndex);
				}
				else
				{
					FileStream fileStream = File.OpenRead(file);
					byte[] buffer = new byte[fileStream.Length];
					fileStream.Read(buffer, 0, buffer.Length);

					string tempFile = file.Substring(subIndex);
					ZipEntry entry = new ZipEntry(tempFile)
					{
						DateTime = DateTime.Now,
						Size = fileStream.Length
					};
					fileStream.Close();

					crc.Reset();
					crc.Update(buffer);
					entry.Crc = crc.Value;
					zipStream.PutNextEntry(entry);
					zipStream.Write(buffer, 0, buffer.Length);
				}
			}
		}

		public static string[] GetProductNames(string rupsRoot, bool isAddDefault = false)
		{
			List<string> result = new List<string>();
			if (isAddDefault)
			{
				result.Add("None");
			}

			var dirs = Directory.GetDirectories($"{rupsRoot}/Product");
			foreach (var item in dirs)
			{
				var di = new DirectoryInfo(item);
				if (di.Name.Contains("-")) continue;

				result.Add(di.Name);
			}
			return result.ToArray();
		}

		public static string[] GetSdkNames()
		{
			List<string> result = new List<string>();

			var dirs = Directory.GetDirectories($"{PathUtil.GetRupsPath()}/SM/Common/Packages");
			foreach (var item in dirs)
			{
				var di = new DirectoryInfo(item);
				result.Add(di.Name);
			}
			return result.ToArray();
		}

		public static bool UpdateManifest(string productType)
		{
			bool flag = false;
			var rupsPath = PathUtil.GetRupsPath();
			var platformPath = rupsPath + "/Platform";
			GitHelper.RevertFile(platformPath, "Packages/manifest.json");

			var configFilePath = $"{rupsPath}/Product/{productType}/Doc/发布文档/{productType}.ini";
			var configFile = IniMgr.LoadIniFile(configFilePath);
			if (configFile != null)
			{
				var dependencies = configFile.GetValue("Dependencies", "Manifest");
				if (!string.IsNullOrEmpty(dependencies))
				{
					string manifestFilePath = Application.dataPath + "/../Packages/manifest.json";
					string packageInfo = File.ReadAllText(manifestFilePath);
					var packageObject = JsonMapper.ToObject(packageInfo);
					var originManifest = packageObject["dependencies"];
					var currentManifest = JsonMapper.ToObject(dependencies)["dependencies"];

					foreach (var key in currentManifest.Keys)
					{
						if (string.IsNullOrEmpty(originManifest[key].ToJson()))
						{
							originManifest[key] = currentManifest[key];
							flag = true;
						}
					}
					packageObject["dependencies"] = originManifest;
					string manifestText = packageObject.ToJson();
					File.WriteAllText(manifestFilePath, manifestText);
				}
			}

			return flag;
		}
	}
}