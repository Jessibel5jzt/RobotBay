using System;
using System.Text;
using UnityEngine;

namespace WestBay
{
	public class DebugNamer
	{ public string Name = ""; }

	public class DebugT<T> where T : DebugNamer, new()
	{
		#region Mod Name

		private static T Nm;

		private static string Mod
		{
			get
			{
				if (Nm == null)
				{
					Nm = new T();
					Nm.Name = $"{Nm.Name}";
					return Nm.Name;
				}
				//Debug.Log("Type:" + typeof(T) + "MOD NAME:" + Nm.Name);
				return Nm.Name;
			}
		}

		public static string ModuleName;

		#endregion Mod Name

		public static void SetModuleName(string name)
		{
			ModuleName = name;
		}

		public static string GetModuleName()
		{
			return string.IsNullOrWhiteSpace(ModuleName) ? "FFTAI" : ModuleName;
		}

		public static void Log(bool b, string logTrue, string logFalse)
		{
			UnityEngine.Debug.Log(b ? $"[{GetModuleName()}] {logTrue}" : $"[{GetModuleName()}] {logFalse}");
			string message = $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] [Log] [{GetModuleName()}] \n {logTrue} \n";
			FileLog.Log2File(MsgOptimize(message));
		}

		public static void LogWarning(string log)
		{
			UnityEngine.Debug.LogWarning($"[{GetModuleName()}] {log}");
			string message = $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] [Warning] [{GetModuleName()}] \n {log} \n";
			FileLog.Log2File(MsgOptimize(message));
		}

		public static void LogError(string log)
		{
			UnityEngine.Debug.LogError($"[{GetModuleName()}] {log}");
			string message = $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] [Error] [{GetModuleName()}] \n {log} \n";
			FileLog.Log2File(MsgOptimize(message));
		}

		public static void LogException(Exception e)
		{
			UnityEngine.Debug.LogException(e);
			string message = $"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] [Exception] [{GetModuleName()}] \n {e.ToString()} \n";
			FileLog.Log2File(MsgOptimize(message));
		}

		public static void Log(string log)
		{ Log(true, log); }

		public static void Log(bool b, string logTrue)
		{ if (b) Log(true, logTrue, null); }

		public static void _New(string persistentPath)
		{
			ThreadLog.Create();
			FileLog.Create(Mod, persistentPath);
			SetModuleName("FFTAI");
		}

		public static void _New(string persistentPath, float fileExpire = 24 * 60 * 60)
		{
			ThreadLog.Create();
			FileLog.Create(Mod, persistentPath, fileExpire);
		}

		public static void LogEDITOR(string log)
		{
			if (!UnityEngine.Application.isEditor) return;

			UnityEngine.Debug.Log($"{Mod}<color=grey>[EDITOR]</color> {log}");
		}

		public static void OnApplicationQuit()
		{
			if (UnityEngine.Application.isEditor) return;
			if (UnityEngine.Application.platform != UnityEngine.RuntimePlatform.WindowsPlayer) return;

			Log($"{Mod}<color=grey>ÍË³öÁË</color>");

			FileLog.DeleteDatedDir();
		}

		private static string MsgOptimize(string msg)
		{
			msg = msg.Replace("</color>", "");

			while (msg.IndexOf("<color=") != -1)
			{
				int startIndex = msg.IndexOf("<color=");
				if (startIndex != -1)
				{
					int endIndex = msg.IndexOf('>');
					if (endIndex != -1)
					{
						msg = msg.Remove(startIndex, endIndex - startIndex + 1);
					}
				}
			}
			return msg;
		}

		public class Th
		{
			public static void Log(string log)
			{
				ThreadLog.Ins.AddLog($"{Mod} {log}");
			}

			public static void LogWarning(string log)
			{
				ThreadLog.Ins.AddLogW($"{Mod} {log}");
			}

			public static void LogError(string log)
			{
				ThreadLog.Ins.AddLogE($"{Mod} {log}");
			}
		}
	}//class
}//namespace