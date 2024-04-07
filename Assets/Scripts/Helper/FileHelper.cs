using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace WestBay
{
	public static class FileHelper
	{
		public static bool IsExist(string path)
		{
			return File.Exists(path);
		}

		public static void MoveFile(string srcPath, string tgtPath)
		{
			File.Move(srcPath, tgtPath);
		}

		/// <summary>
		/// 获取所有文件
		/// </summary>
		/// <param name="files"></param>
		/// <param name="dir"></param>
		public static void GetAllFiles(List<string> files, string dir)
		{
			string[] fls = Directory.GetFiles(dir);
			foreach (string fl in fls)
			{
				files.Add(fl);
			}

			string[] subDirs = Directory.GetDirectories(dir);
			foreach (string subDir in subDirs)
			{
				GetAllFiles(files, subDir);
			}
		}

		/// <summary>
		/// 删除文件夹
		/// </summary>
		/// <param name="dir"></param>
		public static void CleanDirectory(string dir)
		{
			foreach (string subdir in Directory.GetDirectories(dir))
			{
				Directory.Delete(subdir, true);
			}

			foreach (string subFile in Directory.GetFiles(dir))
			{
				File.Delete(subFile);
			}
		}

		/// <summary>
		/// 复制文件夹
		/// </summary>
		/// <param name="srcDir"></param>
		/// <param name="tgtDir"></param>
		public static void CopyDirectory(string srcDir, string tgtDir)
		{
			DirectoryInfo source = new DirectoryInfo(srcDir);
			DirectoryInfo target = new DirectoryInfo(tgtDir);

			if (target.FullName.StartsWith(source.FullName, StringComparison.CurrentCultureIgnoreCase))
			{
				Debug.LogError("父目录不能拷贝到子目录！");
				return;
			}

			if (!source.Exists)
			{
				return;
			}

			if (!target.Exists)
			{
				target.Create();
			}

			FileInfo[] files = source.GetFiles();

			for (int i = 0; i < files.Length; i++)
			{
				File.Copy(files[i].FullName, Path.Combine(target.FullName, files[i].Name), true);
			}

			DirectoryInfo[] dirs = source.GetDirectories();

			for (int j = 0; j < dirs.Length; j++)
			{
				CopyDirectory(dirs[j].FullName, Path.Combine(target.FullName, dirs[j].Name));
			}
		}

		public static void DeleteChildDirectory(string dir, string childDirName)
		{
			foreach (string subdir in Directory.GetDirectories(dir))
			{
				if (subdir.Contains(childDirName))
				{
					Directory.Delete(subdir, true);
					break;
				}
			}

			foreach (string subFile in Directory.GetFiles(dir))
			{
				if (subFile.Contains(childDirName))
				{
					File.Delete(subFile);
					break;
				}
			}
		}

		/// <summary>
		/// 复制文件
		/// </summary>
		/// <param name="srcDir"></param>
		/// <param name="tgtDir"></param>
		public static void CopyFile(string srcFile, string tgtPath, string tgtName)
		{
			FileInfo source = new FileInfo(srcFile);
			DirectoryInfo targetDir = new DirectoryInfo(tgtPath);

			if (!source.Exists)
			{
				return;
			}

			if (!targetDir.Exists)
			{
				targetDir.Create();
			}
			File.Copy(srcFile, $"{tgtPath}/{tgtName}", true);
		}

		/// <summary>
		/// 复制文件（异步）
		/// </summary>
		/// <param name="SourcePath"></param>
		/// <param name="DestinationPath"></param>
		/// <returns></returns>
		public static async UniTask<bool> CopyFile(string SourcePath, string DestinationPath)
		{
			if (File.Exists(DestinationPath))
			{
				File.Delete(DestinationPath);
			}
			else
			{
				CreatPath(DestinationPath);
			}

			bool result;
			try
			{
				using (FileStream fs = new FileStream(DestinationPath, FileMode.OpenOrCreate, FileAccess.Write))
				{
					using (FileStream fs1 = new FileStream(SourcePath, FileMode.Open, FileAccess.Read))
					{
						byte[] buff = new byte[1024 * 1024];
						while (true)
						{
							int fscount = fs1.Read(buff, 0, buff.Length);
							if (fscount == 0)
							{ break; }
							fs.Write(buff, 0, fscount);
							await UniTask.Delay(10);
						}
					}
				}
				result = true;
			}
			catch (Exception ex)
			{
				Debug.Log(ex.ToString());
				result = false;
			}

			return result;
		}

		/// <summary>
		/// 获取文件夹的所有文件路径，包括子文件夹 不包含.meta文件
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string[] GetFilesPath(string path)
		{
			DirectoryInfo folder = new DirectoryInfo(path);
			DirectoryInfo[] subFolders = folder.GetDirectories();
			List<string> filesList = new List<string>();

			foreach (DirectoryInfo subFolder in subFolders)
			{
				filesList.AddRange(GetFilesPath(subFolder.FullName));
			}

			FileInfo[] files = folder.GetFiles();
			foreach (FileInfo file in files)
			{
				if (file.Extension != ".meta")
				{
					filesList.Add(NormalizePath(file.FullName));
				}
			}
			return filesList.ToArray();
		}

		/// <summary>
		/// 规范化路径名称 修正路径中的正反斜杠
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string NormalizePath(string path)
		{
			return path.Replace(@"\", "/");
		}

		/// <summary>
		/// 写入文件
		/// </summary>
		/// <param name="path"></param>
		/// <param name="data"></param>
		public static void WriteFile(string path, byte[] data)
		{
			var fi = CreatPath(path);
			using (FileStream fs = fi.Create())
			{
				fs.Write(data, 0, data.Length);
				fs.Flush();
				fs.Close();
			}
		}

		public static void WriteFile(string path, string contents)
		{
			var fi = CreatPath(path);
			using (FileStream fs = fi.Create())
			{
				StreamWriter sw = new StreamWriter(fs);
				sw.Write(contents);
				sw.Flush();
				fs.Flush();
				sw.Close();
				fs.Close();
			}
		}

		public static FileInfo CreatPath(string path)
		{
			FileInfo fi = new FileInfo(path);
			DirectoryInfo dir = fi.Directory;
			if (!dir.Exists)
			{
				dir.Create();
			}
			return fi;
		}

		/// <summary>
		/// xxx.jpg -> xxx, jpg
		/// </summary>
		/// <param name="path">abcd.jpg</param>
		/// <param name="fileName">abcd (出错返回空串）</param>
		/// <param name="extension">jpg (出错返回空串）</param>
		public static void GetNameExt(string path, out string fileName, out string extension)
		{
			fileName = "";
			extension = "";
			if (string.IsNullOrEmpty(path)) return;

			int pIdx = path.LastIndexOf('.');
			if (pIdx <= 0) return;

			fileName = path.Substring(0, pIdx);
			extension = path.Substring(pIdx + 1);
		}

		/// <summary>
		/// 读取文件
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static string ReadFile(string filePath)
		{
			if (!File.Exists(filePath)) return null;

			string result = string.Empty;
			using (FileStream fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Read))
			{
				using (StreamReader sr = new StreamReader(fs))
				{
					result = sr.ReadToEnd();
					sr.Close();
				}
				fs.Close();
			}

			return result;
		}

		public static byte[] ReadFileBytes(string filePath)
		{
			if (!File.Exists(filePath)) return new byte[0];

			byte[] bytes = File.ReadAllBytes(filePath);
			return bytes;
		}

		public static string ReadLine(string path)
		{
			if (!File.Exists(path))
				return string.Empty;

			var strLine = string.Empty;
			using (FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Read))
			{
				using (StreamReader sr = new StreamReader(fs))
				{
					strLine = sr.ReadLine();
					sr.Close();
				}
				fs.Close();
			}

			return strLine;
		}

		/// <summary>
		/// 版本号比较：new_version>=old_version
		/// </summary>
		/// <param name="newVersion">4.0.2.2</param>
		/// <param name="oldVersion">4.0.1.101</param>
		/// <returns></returns>
		public static bool CompareVersion(string newVersion, string oldVersion)
		{
			if (string.IsNullOrEmpty(newVersion) || string.IsNullOrEmpty(oldVersion)) return false;

			bool result = false;
			try
			{
				Version verNew = new Version(VersionRegularize(newVersion));
				Version verOld = new Version(VersionRegularize(oldVersion));
				if (verNew > verOld)
				{
					result = true;
				}
			}
			catch (Exception e)
			{
				Debug.LogError($"CompareVersion:{newVersion},oldVersion:{oldVersion} e:{e}");
			}

			return result;
		}

		private static string VersionRegularize(string versionStr)
		{
			string result = versionStr;
			var versions = versionStr.Split('.').ToList();
			if (versions.Count > 4)
			{
				versions.RemoveRange(4, versions.Count - 4);
				result = string.Join(".", versions);
			}

			return result;
		}

		public static void DeleteAll(string path)
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

		public static bool IsDirectoryExist(string path)
		{
			return Directory.Exists(path);
		}
	}
}