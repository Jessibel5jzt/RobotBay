using System.Collections.Concurrent;
using System.Collections;

namespace WestBay
{
	internal class ThreadLog : BasedOnMB<ThreadLog>
	{
		public static void Create()
		{
			if (!AddComponent("TreadLog")) return;
			Ins.StartCoroutine(Ins.PrintLog());
		}

		public void AddLog(string log)
		{
			Logs.Enqueue(log);
		}

		public void AddLogW(string log)
		{
			LogsW.Enqueue(log);
		}

		public void AddLogE(string log)
		{
			LogsE.Enqueue(log);
		}

		private ConcurrentQueue<string> Logs = new ConcurrentQueue<string>();
		private ConcurrentQueue<string> LogsW = new ConcurrentQueue<string>();
		private ConcurrentQueue<string> LogsE = new ConcurrentQueue<string>();

		private IEnumerator PrintLog()
		{
			while (true)
			{
				if (Exiting) break;
				DebugLog(Logs);
				DebugLogW(LogsW);
				DebugLogE(LogsE);
				yield return null;
			}
		}

		private static void DebugLog(ConcurrentQueue<string> logs)
		{
			if (logs.Count == 0) return;
			string Log;
			logs.TryDequeue(out Log);
			UnityEngine.Debug.Log(Log);
		}

		private static void DebugLogW(ConcurrentQueue<string> logs)
		{
			if (logs.Count == 0) return;
			string Log;
			logs.TryDequeue(out Log);
			UnityEngine.Debug.LogWarning(Log);
		}

		private static void DebugLogE(ConcurrentQueue<string> logs)
		{
			if (logs.Count == 0) return;
			string Log;
			logs.TryDequeue(out Log);
			UnityEngine.Debug.LogError(Log);
		}
	}//class
}//namespace