using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 核心功能组
/// </summary>
/// @defgroup CoreApi Core API

namespace WestBay
{
	///<summary>
	///所有相关MonoBehaviour的操着，都可以通过这个类来调用。如：
	/// </summary>
	/// @code {.cs}
	/// MB.Ins.StartCoroutine(...);
	/// @endcode
	/// @ingroup CoreApi
	public sealed class MB : MonoBehaviour
	{
		public static MB Instance { get; private set; } = null;

		public static MB Ins
		{ get { return Instance; } }

		public static MB _New()
		{
			if (Instance != null) return Instance;
			Instance = NewT();
			Debug.Log($"MB Created");
			return Instance;
		}

		private void Start()
		{
			InitAutoForeground();
			InitBugReporter();
			InitTimer();
		}

		/// <summary>
		/// Update事件,代替协程
		/// </summary>
		public event Action OnUpdateEvent;

		public event Action OnFixedUpdateEvent;

		public event Action OnApplicationQuitEvent;

		private void Update()
		{
#if UNITY_STANDALONE_WIN
			if (Screen.currentResolution.width != 1920 || Screen.currentResolution.height != 1080)
			{
				Screen.SetResolution(1920, 1080, true);
			}
#endif
			OnUpdateEvent?.Invoke();
		}

		private void FixedUpdate()
		{
			OnFixedUpdateEvent?.Invoke();
		}

		private void OnApplicationQuit()
		{
			//if (NetworkMgr.Ins != null) NetworkMgr.Ins.StopMgr();
			//if (DataSyncMgr.Ins != null) DataSyncMgr.Ins.StopMgr();
			//if (UpdateAssetMgr.Ins != null) UpdateAssetMgr.Ins.StopMgr();
			//if (DatabaseMgr.Ins != null) DatabaseMgr.Ins.StopMgr();
			//if (MqttMgr.Ins != null) MqttMgr.Ins.StopMgr();

			//WebReqHelper.StopRequest();
			//RegeditHelper.ClearRegedit();

			OnApplicationQuitEvent?.Invoke();
		}

		private void OnApplicationFocus(bool focus)
		{
			// Method intentionally left empty.
		}

		private void OnApplicationPause(bool pause)
		{
			// Method intentionally left empty.
		}

		private void OnDestroy()
		{
		}

		private void InitAutoForeground()
		{
			//Windows窗口置顶
			if (Application.platform == RuntimePlatform.WindowsPlayer && !Application.isEditor)
			{
				//gameObject.AddComponent<AutoForeground>();
			}
		}

		private void InitBugReporter()
		{
			//Bug跟踪
			//if (App.IsSubsystemEnable(Subsystem.Report)) this.gameObject.AddComponent<BugTracker.ReportCtrl>();
		}

		#region Game Pause & Resume

		//private TimerCtrl _timerCtrl;

		private void InitTimer()
		{
			//定时器管理
			//_timerCtrl = this.gameObject.AddComponent<TimerCtrl>();
		}

		private void DestoryTimer()
		{
		}

		#endregion Game Pause & Resume

		#region CreateGameObject

		private static string DDOLName = "__fftai__mbsingleton__";

		private static MB NewT()
		{
			GameObject GO = GameObject.Find(DDOLName);
			if (GO == null)
			{
				GO = new GameObject(DDOLName);
				DontDestroyOnLoad(GO);
			}
			MB mB_Ins = GO.AddComponent<MB>();
			return mB_Ins;
		}

		#endregion CreateGameObject

		#region 子系统间依赖事件

		public void InitSubsystemEvent()
		{
			MB.Ins.OnUpdateEvent += UpdateSubsystem;
		}

		private void OnModuleEnter(Module obj)
		{
			//MqttMgr.Ins.OnModuleEnter(obj.Name);
			//UpdateAssetMgr.Ins.OnModuleEnter(obj);
		}

		private void UpdateSubsystem()
		{
			//DatabaseMgr.Ins.Update();
			//NetworkMgr.Ins.Update();
		}

		#endregion 子系统间依赖事件
	}
}