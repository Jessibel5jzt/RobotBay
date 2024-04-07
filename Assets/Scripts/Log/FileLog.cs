using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace WestBay
{
	internal class FileLog : BasedOnMB<FileLog>
	{
		private static Queue<string> Logs = null;
		private StreamWriter _streamWriter = null;
		private float _fileExpire = 24 * 60 * 60;
		private static string _targetDir;

		public static void Create(string mod, string perPath, float fileExpire = 24 * 60 * 60)
		{
			if (UnityEngine.Application.isEditor) return;
			if (!UnityEngine.Application.isPlaying) return;
			if (string.IsNullOrWhiteSpace(perPath)) return;
			if (!AddComponent("FileLog")) return;

			if (UnityEngine.Application.platform == UnityEngine.RuntimePlatform.WindowsPlayer)
			{
				_targetDir = perPath + "/logs";
				CheckDirectoryExist(_targetDir);
				Ins.Init(mod, perPath);
			}
		}

		private void Init(string mod, string perPath)
		{
			Logs = new Queue<string>();
			var LogFile = _targetDir + "/" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
			if (!File.Exists(LogFile))
			{
				FileStream file = File.Create(LogFile);
				file.Close();
			}

			_streamWriter = File.AppendText(LogFile);
			_streamWriter.WriteLine($"-------------------   {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}   -------------------");
			UnityEngine.Debug.Log($"{mod} log to file : {LogFile}");
			StartCoroutine(WriteLog());
		}

		private bool CheckExpire(string filePath)
		{
			try
			{
				string lineText = File.ReadAllLines(filePath)[0];
				var dateTime = DateTime.Parse(lineText);
				TimeSpan timeSpan = DateTime.Now.Subtract(dateTime);
				if (timeSpan.TotalSeconds > _fileExpire)
				{
					return true;
				}
			}
			catch { }

			return false;
		}

		public static void Log2File(string log)
		{
			if (Logs != null)
			{
				//Debug.Log("已经添加" + log);
				Logs.Enqueue(log);
			}
		}

		private IEnumerator WriteLog()
		{
			while (true)
			{
				while (Logs.Count > 0)
				{
					var log = Logs.Dequeue();
					_streamWriter.WriteLine(log);
				}

				_streamWriter.Flush();

				if (Exiting)
				{
					if (_streamWriter != null)
					{
						_streamWriter.Close();
						_streamWriter.Dispose();
					}
					yield break;
				}

				yield return null;
			}
		}

		public static void DeleteDatedDir()
		{
			DirectoryInfo info = new DirectoryInfo(_targetDir);
			DirectoryInfo[] allDirs = info.GetDirectories();
			for (int i = 0; i < allDirs.Length; i++)
			{
				string dateStr = allDirs[i].Name;
				DateTime dirDateTime;
				if (DateTime.TryParseExact(dateStr, "yyyyMMddHHmm", System.Globalization.CultureInfo.CurrentCulture, System.Globalization.DateTimeStyles.None, out dirDateTime))
				{
					TimeSpan span = DateTime.Now - dirDateTime;
					if (span.Days > 7)
					{
						Directory.Delete(allDirs[i].FullName, true);
					}
				}
				else
				{
					continue;
				}
			}
		}

		public static void CheckDirectoryExist(string path)
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
		}
	}//class
}//namespace